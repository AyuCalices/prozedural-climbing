using CharacterMovement.Character.Scripts.Climb;
using ToolBox.Pools;
using UnityEngine;

namespace Edge_Detection.Scripts
{
    [System.Serializable]
    public class EdgeData
    {
        //data
        [SerializeField] public float distance;
        [SerializeField] public Transform climbable;
        
        public Vector3[] edge;
        public Vector3 closestPoint;
        public Vector3 edgeNormal;
        private Edge _edgeIdentifier;
        
        //debugging
        private bool _debugEnabled;
        private LineRenderer _edgeLine;
        private bool _isDrawingNearest;
        private LineRenderer _lineToV0;
        private LineRenderer _lineToV1;
        private LineRenderer _lineToClosestPoint;

        public Edge GetEdgeIdentifier() => _edgeIdentifier;

        public EdgeData(Transform climbable, Edge edgeIdentifier, Edge worldEdge, Vector3 closestPoint, Vector3 edgeNormal, float distance, GameObject lineRendererPrefab, bool debugEnabled)
        {
            edge = new[] {worldEdge.v0, worldEdge.v1};
            this.closestPoint = closestPoint;
            this.edgeNormal = edgeNormal;
            this.distance = distance;
            this.climbable = climbable;
            
            _edgeIdentifier = edgeIdentifier;

            _debugEnabled = debugEnabled;
            if (debugEnabled)
            {
                _edgeLine = lineRendererPrefab.Reuse<LineRenderer>();
                _edgeLine.SetPositions(new [] {worldEdge.v0, worldEdge.v1});
            }
        }

        public void Update(Edge newEdgePos, Vector3 newClosestPoint, Vector3 normal, float newSqrDistance)
        {
            edge[0] = newEdgePos.v0;
            edge[1] = newEdgePos.v1;
            closestPoint = newClosestPoint;
            edgeNormal = normal;
            distance = newSqrDistance;
        }

        public void ReleaseAll()
        {
            if (!_debugEnabled) return;
            
            _edgeLine.gameObject.Release();
            ReleaseNearest();
        }

        public void ReleaseNearest()
        {
            if (!_isDrawingNearest || !_debugEnabled) return;

            _isDrawingNearest = false;
            _lineToV0.gameObject.Release();
            _lineToV1.gameObject.Release();
            _lineToClosestPoint.gameObject.Release();
        }

        private void ReuseNearestLines(GameObject lineRendererPrefab)
        {
            if (!_debugEnabled || _isDrawingNearest) return;
            
            _isDrawingNearest = true;
            _lineToV0 = lineRendererPrefab.Reuse<LineRenderer>();
            _lineToV1 = lineRendererPrefab.Reuse<LineRenderer>();
            _lineToClosestPoint = lineRendererPrefab.Reuse<LineRenderer>();
        }

        public void UpdateLineRenderer(GameObject lineRendererPrefab, Vector3 basePosition)
        {
            if (!_debugEnabled) return;
            
            if (!_isDrawingNearest)
            {
                ReuseNearestLines(lineRendererPrefab);
            }
            
            _lineToV0.SetPositions(new []
            {
                basePosition,
                edge[0]
            });
                
            _lineToV1.SetPositions(new []
            {
                basePosition,
                edge[1]
            });
                
            _lineToClosestPoint.SetPositions(new []
            {
                basePosition,
                closestPoint
            });
            
            _edgeLine.SetPositions(new [] {edge[0], edge[1]});
        }
    }
}
