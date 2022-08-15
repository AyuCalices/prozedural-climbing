using CharacterMovement.InputSystem;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.StateMachine
{
    public enum MovementType { Default = 0, Crouch = 1}
    public enum MovementSpeed { Stand = 0, Walk = 1, SlowRun = 2, FastRun = 3}
    
    public abstract class AnimatorState_SO : ScriptableObject
    {
        [SerializeField] protected internal bool stateTransitionEachFrame;
        [Header("Swap State After Time")]
        [SerializeField] protected internal bool hasExitTime;
        [SerializeField] protected float exitTime;
        [SerializeField] protected AnimatorState_SO defaultNextState;

        //properties
        protected Animator Animator => manager.Animator;
        protected CharacterController Controller => manager.Controller;
        protected GameInputs Input => manager.Input;
        protected GameObject GameObject => manager.MainCamera;
        protected AnimatorStateMachine AnimatorStateMachine => manager.animatorStateMachine;

        //fields
        protected Transform transform;
        protected ThirdPersonManager manager;
    
        protected readonly int animIDSpeed = Animator.StringToHash("Speed");
        protected readonly int animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        protected readonly int animIDGrounded = Animator.StringToHash("Grounded");
        protected readonly int animIDFreeFall = Animator.StringToHash("FreeFall");
        protected readonly int animIDJump = Animator.StringToHash("Jump");
        protected readonly int animIDClimb = Animator.StringToHash("Climb");
        protected readonly int animIDWallRun = Animator.StringToHash("WallRun");
        protected readonly int animIDClimbToTop = Animator.StringToHash("ClimbToTop");
        protected readonly int animIDHangingType = Animator.StringToHash("HangingType");

        protected float exitTimeDelta;

        public abstract void RequestState(AnimatorState_SO currentStateAnimator);

        protected void RequestDefaultState()
        {
            AnimatorStateMachine.ChangeState(defaultNextState);
        }

        public void Initialize(ThirdPersonManager thirdPersonManager)
        {
            manager = thirdPersonManager;
            transform = thirdPersonManager.transform;
        }

        protected virtual void Enter() { }
        

        public void InternalEnter()
        {
            if (hasExitTime)
            {
                exitTimeDelta = 0;
            }

            Enter();
        }

        protected virtual void Update() { }

        public void InternalUpdate()
        {
            Update();
            
            if (hasExitTime)
            {
                exitTimeDelta += Time.deltaTime;
                if (exitTimeDelta >= exitTime)
                {
                    RequestDefaultState();
                }
            }
        }
    
        public virtual void OnAnimatorMove() { }

        public virtual void Exit() { }
    }
}
