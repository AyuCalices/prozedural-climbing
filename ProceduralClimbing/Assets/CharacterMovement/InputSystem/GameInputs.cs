using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterMovement.InputSystem
{
	public class GameInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool climb;
		public bool drop;

		[Header("Movement Settings")]
		public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

		[UsedImplicitly]
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		[UsedImplicitly]
		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		[UsedImplicitly]
		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		[UsedImplicitly]
		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		[UsedImplicitly]
		public void OnClimb(InputValue value)
		{
			ClimbInput(value.isPressed);
		}
		
		[UsedImplicitly]
		public void OnDrop(InputValue value)
		{
			DropInput(value.isPressed);
		}

		private void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		private void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		private void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		private void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void ClimbInput(bool newClimbState)
		{
			climb = newClimbState;
		}

		private void DropInput(bool newDropState)
		{
			drop = newDropState;
		}

#if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif

	}
	
}