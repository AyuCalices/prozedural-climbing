using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.StateMachine;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.States
{ 
    [CreateAssetMenu(fileName = "Free Climb Hooking State", menuName = "Character/States/Free Climb Hooking State")]
    public class FreeHookingState_SO : HookingState_SO
    {
        [Header("State enter edge distance")]
        public float hookingDistanceXZ = 0.8f;
        public float hookingDistanceY = 0.25f;

        //hook target
        private Vector3 _closestPoint;
        private Vector3 _closestPointNormal;
        private Transform _targetClimbable;
        
        public override void RequestState(AnimatorState_SO currentStateAnimator)
        {
            if (currentStateAnimator is not (WallRunState_SO or AirState_SO) || !Input.climb) return;
            
            Vector3 baseTransformPos = manager.edgeDetectionSceneManager.GetBaseTransform().position;
            if (Physics.Raycast(baseTransformPos, transform.forward, out RaycastHit hit))
            {
                if (!HookingPointExists(hit)) return;

                _closestPoint = hit.point;
                _closestPointNormal = hit.normal;
                _targetClimbable = hit.collider.transform;
                
                transform.rotation = Quaternion.LookRotation(ClimbHelper.GetHorizontalPositionNormalized(-hit.normal));
                AnimatorStateMachine.ChangeState(this);
            }
        }
        
        private bool HookingPointExists(RaycastHit raycastHit)
        {
            Vector3 closestPointPos = raycastHit.point;
            Vector2 closestEdgeXZ = new(closestPointPos.x, closestPointPos.z);
                
            Vector3 baseTransformPos = manager.edgeDetectionSceneManager.GetBaseTransform().position;
            Vector2 baseTransformXZ = new(baseTransformPos.x, baseTransformPos.z);

            if (raycastHit.collider.CompareTag("FreeClimbable") 
                && Vector2.Distance(baseTransformXZ, closestEdgeXZ) < hookingDistanceXZ
                && Mathf.Abs(closestPointPos.y - baseTransformPos.y) < hookingDistanceY)
            {
                return true;
            }

            return false;
        }

        protected override void PlaceHand()
        {
            Vector3 horizontalNormal = ClimbHelper.GetHorizontalPositionNormalized(_closestPointNormal);
            Vector3 edgeTargetDir = Quaternion.AngleAxis(90, Vector3.up) * horizontalNormal * handPosition.lateralOffset;
            Vector3 forward = -horizontalNormal * handPosition.forwardPosition;
            
            manager.leftHandEffector.data.target.position = _closestPoint - new Vector3(0, handPosition.yOffset, 0) + edgeTargetDir + forward;
            manager.rightHandEffector.data.target.position = _closestPoint - new Vector3(0, handPosition.yOffset, 0) - edgeTargetDir + forward;

            Quaternion targetRotation = Quaternion.LookRotation(-_closestPointNormal);
            //Instantiate(lineRenderer).SetPositions(new[] {manager.rightHandEffector.data.target.position, manager.rightHandEffector.data.target.position + normal});
            Quaternion rightHandTargetRotation = Quaternion.Euler(
                targetRotation.eulerAngles.x + handPosition.rightHandRotation.x,
                targetRotation.eulerAngles.y + handPosition.rightHandRotation.y,
                targetRotation.eulerAngles.z + handPosition.rightHandRotation.z);
            manager.rightHandEffector.data.target.rotation = rightHandTargetRotation;
            Quaternion leftHandTargetRotation = Quaternion.Euler(
                targetRotation.eulerAngles.x + handPosition.leftHandRotation.x,
                targetRotation.eulerAngles.y + handPosition.leftHandRotation.y,
                targetRotation.eulerAngles.z + handPosition.leftHandRotation.z);
            manager.leftHandEffector.data.target.rotation = leftHandTargetRotation;
            
            hookingData.SetNewEdgeData(null);
        }
        
        protected override Vector3 GetHookingPoint() => _closestPoint;

        protected override Vector3 GetHookingPointNormal() => _closestPointNormal;

        protected override Transform GetParent() => _targetClimbable;
    }
}
