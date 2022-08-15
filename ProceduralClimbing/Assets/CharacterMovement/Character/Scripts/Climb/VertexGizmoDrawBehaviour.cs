using System.Collections.Generic;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.Climb
{
    public class VertexGizmoDrawBehaviour : MonoBehaviour
    {
        [SerializeField] private float gizmoRadius = 0.1f;
        
        private readonly HashSet<Vector3> _vertices = new();

        public void Add(Vector3 vertex)
        {
            _vertices.Add(vertex);
        }
    
        private void OnDrawGizmosSelected()
        {
            if (_vertices.Count == 0) return;

            foreach (var vertex in _vertices)
            {
                Gizmos.DrawSphere(vertex, gizmoRadius);
            }
        }
    }
}
