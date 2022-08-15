using CharacterMovement.Character.Scripts.StateMachine;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.States
{
    [CreateAssetMenu(fileName = "Air State", menuName = "Character/States/Air State")]
    public class AirState_SO : AnimatorState_SO
    {
        [Header("Rotation in Air")]
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        [SerializeField] private float rotationSmoothTime = 0.12f;
        [Tooltip("Should the character be able to move the flight direction")]
        [SerializeField] private bool enableRotation = true;

        [SerializeField] private EdgeHookingState_SO edgeHookingState;
        [SerializeField] private FreeHookingState_SO freeHookingState;

        private float _rotationVelocity;

        public override void RequestState(AnimatorState_SO currentStateAnimator)
        {
            if (manager.IsGrounded()) return;
            
            switch (currentStateAnimator)
            {
                case GroundedState_SO:
                    AnimatorStateMachine.ChangeState(this);
                    break;
                case ClimbingState_SO when Input.drop:
                    AnimatorStateMachine.ChangeState(this);
                    break;
            }
        }

        protected override void Enter()
        {
            Vector3 velocity = Controller.velocity;
            if (AnimatorStateMachine.GetPreviousState() is not JumpState_SO)
            {
                float magnitude = new Vector3(velocity.x, 0f, velocity.z).magnitude;
                manager.JumpSpeed = magnitude;
            }
        }

        protected override void Update()
        {
            ApplyGravity();
            if (enableRotation)
            {
                ApplyRotation();
            }
            
            edgeHookingState.RequestState(this);
            if (manager.CurrentState.GetType() != edgeHookingState.GetType())
            {
                freeHookingState.RequestState(this);
            }
        }

        public override void OnAnimatorMove()
        {
            Vector3 targetDirection = Quaternion.Euler(0.0f, manager.TargetRotation, 0.0f) * Vector3.forward;
            Controller.Move(targetDirection * (manager.JumpSpeed * Time.deltaTime) + new Vector3(0.0f, manager.VerticalVelocity, 0.0f) * Time.deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
        
            Animator.SetBool(animIDFreeFall, false);

            manager.VerticalVelocity = 0;
        }

        private void ApplyRotation()
        {
            // normalise input direction
            Vector3 inputDirection = new Vector3(Input.move.x, 0.0f, Input.move.y).normalized;

            // if there is a move input rotate player when the player is moving
            if (Input.move != Vector2.zero)
            {
                manager.TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + GameObject.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, manager.TargetRotation, ref _rotationVelocity, rotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        private void ApplyGravity()
        {
            Animator.SetBool(animIDFreeFall, true);
        
            manager.VerticalVelocity += manager.gravity * Time.deltaTime;
        }
    }
}
