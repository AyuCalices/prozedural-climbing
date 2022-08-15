using CharacterMovement.Character.Scripts.StateMachine;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.States
{
	[CreateAssetMenu(fileName = "Grounded State", menuName = "Character/States/Grounded State")]
	public class GroundedState_SO : AnimatorState_SO
	{
		[Header("Movement")]
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		[SerializeField] private float rotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		[SerializeField] private float speedChangeRate = 10.0f;
		
		[SerializeField][Range(0, -100f)] private float verticalVelocity = -10f;

		private float _animationBlend_walkType;
		private float _rotationVelocity;
		private RaycastHit _hit;

		public override void RequestState(AnimatorState_SO currentStateAnimator)
		{
			if (!manager.IsGrounded()) return;
			
			switch (currentStateAnimator)
			{
				case AirState_SO:
					AnimatorStateMachine.ChangeState(this);
					break;
				case ClimbingState_SO when Input.drop:
					AnimatorStateMachine.ChangeState(this);
					break;
			}
		}

		protected override void Enter()
		{
			// reset based on current input
			Input.jump = false;
			ApplySpeed(false);
			
			// update animator if using character
			if (Animator != null)
			{
				Animator.SetBool(animIDGrounded, true);
			}
		}

		protected override void Update()
		{
			manager.VerticalVelocity = verticalVelocity;
			
			ApplySpeed(true);
			ApplyRotation();
			ApplyJumpTimeout();
		}
    
		public override void OnAnimatorMove()
		{
			Vector3 velocity = Animator.deltaPosition;
			velocity.y = manager.VerticalVelocity * Time.deltaTime;
			Controller.Move(velocity);
		}

		public override void Exit()
		{
			base.Exit();
	    
			Animator.SetBool(animIDGrounded, false);

			manager.VerticalVelocity = 0;
		}

		private void ApplyJumpTimeout()
		{
			// jump timeout
			if (manager.JumpTimeoutDelta >= 0.0f)
			{
				manager.JumpTimeoutDelta -= Time.deltaTime;
			}
		}
		
		private void ApplySpeed(bool useBlend)
		{
			float inputMagnitude = Input.analogMovement ? Input.move.magnitude : 1f;

			//Set target animation blend
			float speedTargetAnimationBlend = (float) (Input.sprint ? MovementSpeed.FastRun : MovementSpeed.SlowRun);
			if (Input.move == Vector2.zero) speedTargetAnimationBlend = (float) MovementSpeed.Stand;

			//set current animation blend
			manager.Speed_AnimationBlend = useBlend ? Mathf.Lerp(manager.Speed_AnimationBlend, speedTargetAnimationBlend, Time.deltaTime * speedChangeRate) : speedTargetAnimationBlend;

			Animator.SetFloat(animIDSpeed, manager.Speed_AnimationBlend < 0.1 ? 0 : Mathf.Round(manager.Speed_AnimationBlend * 100f) / 100f);
			Animator.SetFloat(animIDMotionSpeed, inputMagnitude);
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
	}
}
