using Edge_Detection.Scripts;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.Climb
{
    public class ClimbingDebugBehaviour : MonoBehaviour
    {
        [SerializeField] private EdgeDetectionSceneManager edgeDetectionSceneManager;
        [SerializeField] private HookingData_SO hookingData;
        [SerializeField] private float forwardPosition = 0.3f;
        [SerializeField] private float positionY = 0.35f;
        [SerializeField] private float radius = 0.2f;
        [SerializeField] private float floorDetectionDistance = -0.5f;

        private void OnDrawGizmosSelected()
        {
            if (edgeDetectionSceneManager == null || !edgeDetectionSceneManager.HasHookableEdge()) return;
            
            Vector3 closestEdgeNormal = ClimbHelper.GetHorizontalPositionNormalized(-hookingData.HookingPointNormal);
            Vector3 spherePos = hookingData.HookingPoint + closestEdgeNormal * forwardPosition + new Vector3(0, positionY, 0);
            
            Gizmos.DrawSphere(spherePos, radius);
            Gizmos.DrawLine(spherePos, spherePos + new Vector3(0, floorDetectionDistance, 0));
        }
    }
}
