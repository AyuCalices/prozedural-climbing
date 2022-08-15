using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.StateMachine;
using Edge_Detection.Scripts;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.States
{
    [CreateAssetMenu(fileName = "Wall Climb State", menuName = "Character/States/Wall Climb State")]
    public class WallClimbState_SO : AnimatorState_SO
    {
        [Header("Edge Climb Target Values")] 
        [SerializeField] private HookingData_SO hookingData;
        [SerializeField] private float forwardPosition = 0.3f;
        [SerializeField] private float positionY = 0.35f;
        [SerializeField] private float radius = 0.2f;
        [SerializeField] [Range(0, 1)] private float floorDetectionDistance = 0.5f;
        
        public override void RequestState(AnimatorState_SO currentStateAnimator)
        {
            if (currentStateAnimator is not EdgeClimbingState_SO) return;
            
            Vector3 closestEdgeNormal = ClimbHelper.GetHorizontalPositionNormalized(-hookingData.HookingPointNormal);
            Vector3 spherePos = hookingData.HookingPoint + closestEdgeNormal * forwardPosition + new Vector3(0, positionY, 0);

            if (Physics.Raycast(spherePos, Vector3.down, floorDetectionDistance) &&
                !Physics.CheckSphere(spherePos, radius))
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0));
                Animator.SetBool(animIDClimbToTop, true);
                AnimatorStateMachine.ChangeState(this);
            }
        }

        protected override void Enter()
        {
            Controller.enabled = false;
        }

        public override void OnAnimatorMove()
        {
            transform.position += Animator.deltaPosition;
        }

        public override void Exit()
        {
            Animator.SetBool(animIDClimbToTop, false);
            
            Controller.enabled = true;
        }
    }
}
