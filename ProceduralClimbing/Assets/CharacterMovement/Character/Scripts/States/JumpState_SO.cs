using CharacterMovement.Character.Scripts.StateMachine;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.States
{
    [CreateAssetMenu(fileName = "Jump State", menuName = "Character/States/Jump State")]
    public class JumpState_SO : AnimatorState_SO
    {
        [Header("Jump")]
        [Tooltip("The height the player can jump")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float jumpTimeout = 0.2f;
        [SerializeField] private float MaxAirSpeed = 10f;
        

        public override void RequestState(AnimatorState_SO currentStateAnimator)
        {
            if (manager.IsGrounded() && currentStateAnimator is GroundedState_SO && Input.jump && manager.JumpTimeoutDelta <= 0.0f)
            {
                AnimatorStateMachine.ChangeState(this);
            }
        }

        protected override void Enter()
        {
            // reset our timeouts on start
            manager.JumpTimeoutDelta = jumpTimeout;
            
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            manager.VerticalVelocity = Mathf.Sqrt(jumpHeight * -2f * manager.gravity);

            Vector3 velocity = Controller.velocity;
            float magnitude = new Vector3(velocity.x, 0f, velocity.z).magnitude;
            manager.JumpSpeed = Mathf.Clamp(magnitude, 1f, MaxAirSpeed);

            // update animator if using character
            Animator.SetTrigger(animIDJump);
        }

        protected override void Update()
        {
            ApplyGravity();
        }
        
        public override void OnAnimatorMove()
        {
            Vector3 targetDirection = Quaternion.Euler(0.0f, manager.TargetRotation, 0.0f) * Vector3.forward;
            Controller.Move(targetDirection * (manager.JumpSpeed * Time.deltaTime) + new Vector3(0.0f, manager.VerticalVelocity, 0.0f) * Time.deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            
            // update animator if using character
            Animator.SetBool(animIDJump, false);
        }

        private void ApplyGravity()
        {
            manager.VerticalVelocity += manager.gravity * Time.deltaTime;
        }
    }
}
