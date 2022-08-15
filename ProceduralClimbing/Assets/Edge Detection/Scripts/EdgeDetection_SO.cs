using System;
using System.Collections.Generic;
using UnityEngine;

namespace Edge_Detection.Scripts
{
    /// <summary>
    /// - Since it is needed to compare the angle between two adjacent triangles, following is not sufficient: https://answers.unity.com/questions/566779/how-to-find-the-vertices-of-each-edge-on-mesh.html
    /// 
    /// - Comparing UV's and Tangents is not needed, since the calculations with angles only need positions
    ///
    /// - This Script calculates everything in the object space. It excludes positions, rotations and scales of the GameObject's Transform.
    ///   You need to add the transform on top of the result!
    ///
    /// - There might be hash collisions with Vector3! https://answers.unity.com/questions/1323978/vector3up-and-vector3down-return-same-hashcode.html
    ///
    /// General References:
    /// https://stackoverflow.com/questions/53104905/mesh-edge-detection-for-a-climbing-system
    /// 
    /// Equality References:
    /// https://www.jacksondunstan.com/articles/3798
    /// https://blog.tedd.no/2018/01/13/speeding-up-unitys-vector-in-lists-dictionaries/
    /// https://www.c-sharpcorner.com/article/fast-equality-comparison/
    /// https://thomaslevesque.com/2020/05/15/things-every-csharp-developer-should-know-1-hash-codes/
    /// </summary>

    [CreateAssetMenu(fileName = "Edge Detection Settings", menuName = "Climb/Edge Detection Settings")]
    public class EdgeDetection_SO : ScriptableObject
    {
        [Header("Parameters")] [SerializeField] private float minEdgeWidth = 0.2f;
        [SerializeField][Range(0, 180)] private float horizontalAlignment = 40f;
        [SerializeField][Range(0, 180)] private float minVerticalEdgeAngle = 50f;
        [SerializeField][Range(0, 180)] private float maxVerticalEdgeAngle = 130f;
        [SerializeField][Range(0, 180)] private float minTriangleNeighbourAngle = 30f;
        [SerializeField][Range(0, 180)] private float maxTriangleNeighbourAngle = 160f;

        public Dictionary<Edge, TrianglePair> ParseMeshForEdges(Mesh mesh)
        {
            Mesh _mesh = mesh;
            if (!_mesh.isReadable)
            {
                Debug.LogError($"The Mesh {_mesh.name} wasn't readable. Please enable them in the import settings!");
                return null;
            }
        
            Dictionary<Edge, TrianglePair> edgeTrianglePairs = new();
            foreach (var edgesTrianglePair in GetEdgeTrianglePairs(_mesh.vertices, _mesh.triangles, _mesh.normals))
            {
                if (IsValidTriangleNeighbourAngle(edgesTrianglePair.Value.t0, edgesTrianglePair.Value.t1))
                {
                    edgeTrianglePairs.Add(edgesTrianglePair.Key, edgesTrianglePair.Value);
                }
            }
            return edgeTrianglePairs;
        }

        public bool IsValidEdgeOrientation(Transform transform, KeyValuePair<Edge, TrianglePair> edgeTrianglePair)
        {
            return (IsValidHorizontalAlignment(edgeTrianglePair.Value.t0, transform)
                    || IsValidHorizontalAlignment(edgeTrianglePair.Value.t1, transform))
                   && IsValidVerticalEdgeAngle(edgeTrianglePair.Key, transform);
        }

        public bool IsValidEdgeWidth(Edge edge)
        {
            return Vector3.Distance(edge.v0, edge.v1) >= minEdgeWidth;
        }

        public Vector3 GetEdgeNormal(TrianglePair trianglePair, Quaternion rotation)
        {
            float firstTriangle = GetHorizontalAlignmentAngle(trianglePair.t0, rotation);
            float secondTriangle = GetHorizontalAlignmentAngle(trianglePair.t1, rotation);

            return firstTriangle > secondTriangle ? rotation * trianglePair.t0.normal : rotation * trianglePair.t1.normal;
        }

        //Optimized this Method from: https://answers.unity.com/questions/1615363/how-to-find-connecting-mesh-triangles.html
        private Dictionary<Edge, TrianglePair> GetEdgeTrianglePairs(Vector3[] vertices, int[] triangles, Vector3[] normals)
        {
            Dictionary<Edge, TrianglePair> edgesTrianglePairs = new();
            for (int i = 0; i < triangles.Length; i+=3)
            {
                Vector3 normal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]) / 3;
                Triangle triangle = new(vertices[triangles[i]], vertices[triangles[i + 1]],
                    vertices[triangles[i + 2]], normal);

                //first edge
                FillEdgeTrianglePair(new Edge(triangle.v0, triangle.v1), triangle, edgesTrianglePairs);

                //second edge
                FillEdgeTrianglePair(new Edge(triangle.v1, triangle.v2), triangle, edgesTrianglePairs);

                //third edge
                FillEdgeTrianglePair(new Edge(triangle.v0, triangle.v2), triangle, edgesTrianglePairs);
            }

            return edgesTrianglePairs;
        }

        private void FillEdgeTrianglePair(Edge edge, Triangle triangle, Dictionary<Edge, TrianglePair> edgesTrianglePairs)
        {
            if (!edgesTrianglePairs.TryGetValue(edge, out TrianglePair pair))
            {
                pair = new TrianglePair(triangle);
                edgesTrianglePairs.Add(edge, pair);
            }
            else
            {
                pair.Add(triangle);
            }
        }

        private bool IsValidVerticalEdgeAngle(Edge edge, Transform transform)
        {
            Vector3 dir = edge.v1 - edge.v0;
            float angle = Vector3.Angle(Vector3.up, transform.rotation * dir);
            return minVerticalEdgeAngle < angle && maxVerticalEdgeAngle > angle;
        }

        private bool IsValidHorizontalAlignment(Triangle triangle, Transform transform)
        {
            float angle = GetHorizontalAlignmentAngle(triangle, transform.rotation);
            return horizontalAlignment > angle;
        }

        private float GetHorizontalAlignmentAngle(Triangle triangle, Quaternion rotation)
        {
            Vector3 dir = Vector3.Cross(triangle.v1 - triangle.v0, triangle.v2 - triangle.v0);
            return Vector3.Angle(Vector3.up, rotation * dir);
        }

        //https://answers.unity.com/questions/1600038/how-can-i-calculate-the-angle-between-two-adjacent.html
        private bool IsValidTriangleNeighbourAngle(Triangle triangle1, Triangle triangle2)
        {
            // center
            Vector3 c1 = (triangle1.v0 + triangle1.v1 + triangle1.v2) / 3;
            Vector3 c2 = (triangle2.v0 + triangle2.v1 + triangle2.v2) / 3;
        
            // normal of triangle 1
            Vector3 n1 = Vector3.Cross(triangle1.v1 - triangle1.v0, triangle1.v2 - triangle1.v0);
        
            // vector from triangle1 center to triangle2 center
            Vector3 dir = c2 - c1;
        
            // the two triangles are convex to each other
            if (Vector3.Dot(n1, dir) < 0f)
            {
                float angle = CalculateNeighbourTriangleAngle(triangle2, n1);
            
                if (angle < minTriangleNeighbourAngle || angle > maxTriangleNeighbourAngle)
                {
                    return false;
                }
            }
            // the two triangles are concave to each other
            else
            {
                return false;
            }

            return true;
        }

        //https://stackoverflow.com/questions/2142552/calculate-the-angle-between-two-triangles-in-cuda
        private float CalculateNeighbourTriangleAngle(Triangle triangle2, Vector3 normalTriangle1)
        {
            Vector3 n2 = Vector3.Cross(triangle2.v1 - triangle2.v0,
                triangle2.v2 - triangle2.v0);
                
            return Mathf.Acos(Vector3.Dot(normalTriangle1.normalized, n2.normalized)) * Mathf.Rad2Deg;
        }
    }
}
