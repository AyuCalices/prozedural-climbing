using System;
using System.Collections.Generic;
using System.Linq;
using CharacterMovement.Character.Scripts;
using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.States;
using ToolBox.Pools;
using UnityEngine;
using Edge = Edge_Detection.Scripts.Edge;

namespace Edge_Detection.Scripts
{

    public enum DebugLineDrawType
    {
        None, All, Radius
    }
    
    public class EdgeDetectionSceneManager : MonoBehaviour
    {
        [SerializeField] private EdgeDetection_SO edgeDetection;
        [SerializeField] private EdgeDetectionPosition center;
        [SerializeField] private float radius;
        
        [Header("Debugging")] 
        [SerializeField] private DebugLineDrawType debugLineDrawType;
        [SerializeField] private int nearestEdgesCount;
        [SerializeField] private GameObject lineRendererPrefab;
        
        [SerializeField] private bool drawAllGizmosOnSelect;
        [SerializeField] private VertexGizmoDrawBehaviour vertexGizmoDrawerPrefab;

        private Dictionary<int, Dictionary<Edge, TrianglePair>> _parsedMeshes;
        private List<EdgeData> _hookableEdgesData = new();

        public bool HasHookableEdge() => _hookableEdgesData is {Count: > 0};
        public EdgeData GetEdgeDataAt(int i) => _hookableEdgesData[i];
        public List<EdgeData> GetEdgeData() => _hookableEdgesData;
        public Transform GetBaseTransform() => center.transform;
        
        private Vector3 BaseTransPos => center.transform.position;

        private void Start()
        {
            List<GameObject> climbableGameObjects = GameObject.FindGameObjectsWithTag("Climbable").ToList();
            climbableGameObjects.AddRange(GameObject.FindGameObjectsWithTag("FreeClimbable").ToList());

            _parsedMeshes = new();

            foreach (GameObject climbableGameObject in climbableGameObjects)
            {
                Mesh currentMesh = climbableGameObject.GetComponent<MeshFilter>().sharedMesh;
                if (!_parsedMeshes.ContainsKey(currentMesh.GetInstanceID()))
                {
                    _parsedMeshes.Add(currentMesh.GetInstanceID(), edgeDetection.ParseMeshForEdges(currentMesh));
                }
            }

            lineRendererPrefab.Populate(60);
            if (debugLineDrawType == DebugLineDrawType.All || drawAllGizmosOnSelect)
            {
                foreach (GameObject climbableGameObject in climbableGameObjects)
                {
                    Mesh currentMesh = climbableGameObject.GetComponent<MeshFilter>().sharedMesh;
                    if (_parsedMeshes.TryGetValue(currentMesh.GetInstanceID(), out Dictionary<Edge, TrianglePair> climbable))
                    {
                        if (debugLineDrawType == DebugLineDrawType.All)
                        {
                            DrawEdges(climbableGameObject.transform, climbable);
                        }

                        if (drawAllGizmosOnSelect)
                        {
                            DrawGizmos(climbableGameObject.transform, climbable);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            center.UpdatePosition();
            
            List<EdgeData> newEdgeData = new();
            int totalParsedEdges = 0;
            
            Collider[] colliders = Physics.OverlapSphere(BaseTransPos, radius);
            foreach (Collider collision in colliders)
            {
                //climbable check
                GameObject climbableGameObject = collision.gameObject;
                if (!climbableGameObject.CompareTag("Climbable") && !climbableGameObject.CompareTag("FreeClimbable")) continue;
                
                //mesh available check
                Mesh currentMesh = climbableGameObject.GetComponent<MeshFilter>().sharedMesh;
                if (!_parsedMeshes.TryGetValue(currentMesh.GetInstanceID(), out Dictionary<Edge, TrianglePair> climbable)) continue;
                
                //object transform prep
                Quaternion rotation = climbableGameObject.transform.rotation;
                Vector3 position = climbableGameObject.transform.position;
                Vector3 scale = climbableGameObject.transform.lossyScale;
                    
                //runtime edge validation
                foreach (var edgeTrianglePair in climbable)
                {
                    totalParsedEdges++;
                    if (!edgeDetection.IsValidEdgeOrientation(climbableGameObject.transform, edgeTrianglePair)) continue;

                    //Local mesh edge to world position convertion
                    Vector3 v0 = rotation * Vector3.Scale( edgeTrianglePair.Key.v0, scale) + position;
                    Vector3 v1 = rotation * Vector3.Scale( edgeTrianglePair.Key.v1, scale) + position;
                    Edge worldEdge = new(v0, v1);

                    if (!edgeDetection.IsValidEdgeWidth(worldEdge)) continue;

                    //https://gist.github.com/unitycoder/0620ef7a6b1118df4f05dd895e70dd62
                    var closestPointOnEdge = ClosestPointOnLineSegment(BaseTransPos, v0, v1);
                    var distanceToEdge = Vector3.Distance(BaseTransPos, closestPointOnEdge);
                    
                    //check if the edge is inside the radius
                    if (distanceToEdge < radius)
                    {
                        Edge localEdge = new(edgeTrianglePair.Key.v0, edgeTrianglePair.Key.v1);
                        Vector3 edgeNormal = edgeDetection.GetEdgeNormal(edgeTrianglePair.Value, rotation);
                        int index = _hookableEdgesData.FindIndex(x => x.GetEdgeIdentifier().Equals(localEdge) 
                                                                      && x.climbable.GetInstanceID() == climbableGameObject.GetInstanceID());
                        if (index != -1)
                        {
                            _hookableEdgesData[index].Update(worldEdge, closestPointOnEdge, edgeNormal, distanceToEdge);
                            newEdgeData.Add(_hookableEdgesData[index]);
                            _hookableEdgesData.RemoveAt(index);
                        }
                        else
                        {
                            newEdgeData.Add(new EdgeData(climbableGameObject.transform, localEdge, worldEdge, closestPointOnEdge, edgeNormal,
                                distanceToEdge, lineRendererPrefab, debugLineDrawType == DebugLineDrawType.Radius));
                        }
                    }
                }
            }

            newEdgeData.Sort((x, y) => x.distance.CompareTo(y.distance));

            int drawIterations = Mathf.Min(newEdgeData.Count, nearestEdgesCount);
            if (debugLineDrawType == DebugLineDrawType.Radius)
            {
                for (int i = 0; i < drawIterations; i++)
                {
                    newEdgeData[i].UpdateLineRenderer(lineRendererPrefab, BaseTransPos);
                }

                for (int i = drawIterations; i < newEdgeData.Count; i++)
                {
                    newEdgeData[i].ReleaseNearest();
                }
            }
            foreach (var edgeData in _hookableEdgesData)
            {
                edgeData.ReleaseAll();
            }
            
            _hookableEdgesData = newEdgeData;
        }
        
        //https://gist.github.com/unitycoder/0620ef7a6b1118df4f05dd895e70dd62
        private Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 segment = b - a;
            Vector3 direction = segment.normalized;
            float projection = Vector3.Dot(point - a, direction);
            if (projection < 0)
                return a;

            if (projection*projection > segment.sqrMagnitude)
                return b;

            return a + projection * direction;
        }

        private void DrawEdges(Transform objTransform, Dictionary<Edge, TrianglePair> edgeTrianglePairs)
        {
            var rotation = objTransform.rotation;
            var position = objTransform.position;
            var scale = objTransform.lossyScale;
        
            foreach (var edgeTrianglePair in edgeTrianglePairs)
            {
                if (edgeDetection.IsValidEdgeOrientation(objTransform, edgeTrianglePair))
                {
                    LineRenderer edgeLine = lineRendererPrefab.Reuse<LineRenderer>();
                    edgeLine.SetPositions(new[] {
                        Vector3.Scale(rotation * edgeTrianglePair.Key.v0, scale) + position,
                        Vector3.Scale(rotation * edgeTrianglePair.Key.v1, scale) + position
                    });
                }
            }
        }

        private void DrawGizmos(Transform objTransform, Dictionary<Edge, TrianglePair> edgeTrianglePairs)
        {
            VertexGizmoDrawBehaviour vertexGizmoDrawer = Instantiate(vertexGizmoDrawerPrefab, objTransform);

            var rotation = objTransform.rotation;
            var position = objTransform.position;
            var scale = objTransform.lossyScale;
            
            foreach (var edgeTrianglePair in edgeTrianglePairs)
            {
                if (edgeDetection.IsValidEdgeOrientation(objTransform, edgeTrianglePair))
                {
                    vertexGizmoDrawer.Add(Vector3.Scale(rotation * edgeTrianglePair.Key.v0, scale) + position);
                    vertexGizmoDrawer.Add(Vector3.Scale(rotation * edgeTrianglePair.Key.v1, scale) + position);
                }
            }
        }
    }
}
