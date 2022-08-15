using UnityEngine;

namespace Edge_Detection.Scripts
{
    public class EdgeDetectionPosition : MonoBehaviour
    {
        [SerializeField] private Transform shoulderTransform;
    
        private void UpdatePosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        private void Awake()
        {
            UpdatePosition(shoulderTransform.position, shoulderTransform.rotation);
        }

        public void UpdatePosition()
        {
            UpdatePosition(shoulderTransform.position, shoulderTransform.rotation);
        }
    }
}
