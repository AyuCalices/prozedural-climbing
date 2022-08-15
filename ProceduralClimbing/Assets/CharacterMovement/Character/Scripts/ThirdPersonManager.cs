using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CharacterMovement.Character.Scripts.StateMachine;
using CharacterMovement.Character.Scripts.States;
using CharacterMovement.InputSystem;
using Edge_Detection.Scripts;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace CharacterMovement.Character.Scripts
{
	[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
	public class ThirdPersonManager : MonoBehaviour
	{
		// ReSharper disable once NotAccessedField.Local
		[SerializeField] private CurrentState_SO currentState;
		
		[Header("Available States")]
		[SerializeField] private GroundedState_SO groundedState;
		[SerializeField] private List<AnimatorState_SO> states;

		[Header("Character")]
		[SerializeField] public Transform hipsRoot;
		[SerializeField] public Transform shoulderRoot;
		[SerializeField] public Transform rightArmRoot;
		[SerializeField] public Transform leftArmRoot;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float gravity = -15.0f;

		[Header("Animation Rigging")] 
		public EdgeDetectionSceneManager edgeDetectionSceneManager;
		public TwoBoneIKConstraint leftHandEffector;
		public TwoBoneIKConstraint rightHandEffector;
		public TwoBoneIKConstraint leftFootEffector;
		public TwoBoneIKConstraint rightFootEffector;
		
		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		[SerializeField] private bool grounded = true;
		[Tooltip("Useful for rough ground")]
		[SerializeField] private float groundedOffset1 = -0.15f;
		[SerializeField] private float groundedOffset2 = -0.25f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		[SerializeField] private float groundedRadius = 0.2f;
		[Tooltip("What layers the character uses as ground")]
		[SerializeField] private LayerMask groundLayers;

		[Header("Debug")]
		public Color transparentGreen = new(0.0f, 1.0f, 0.0f, 0.35f);
		public Color transparentRed = new(1.0f, 0.0f, 0.0f, 0.35f);
		

		//References getter and setter
		public Animator Animator { get; private set; }
		public CharacterController Controller { get; private set; }
		public GameInputs Input { get; private set; }
		public GameObject MainCamera { get; private set; }

		public AnimatorState_SO CurrentState => animatorStateMachine.GetCurrentState();
		
		//Values getter and setter
		public float JumpSpeed { get; set; }
		public float Speed_AnimationBlend { get; set; }
		public float TargetRotation { get; set; }
		public float VerticalVelocity { get; set; }
		public float JumpTimeoutDelta { get; set; }
		
		private Collider[] _groundedColliders;
		
		protected internal AnimatorStateMachine animatorStateMachine;


		private void Awake()
		{
			// get a reference to our main camera
			if (MainCamera == null)
			{
				MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			Animator = GetComponent<Animator>();
			Controller = GetComponent<CharacterController>();
			Input = GetComponent<GameInputs>();

			animatorStateMachine = new AnimatorStateMachine();
			animatorStateMachine.Initialize(groundedState, states,this);
			currentState.Initialize(animatorStateMachine);
		}

		private void Update()
		{
			currentState.Update();

			Animator = GetComponent<Animator>();
			GroundedCheck();
			
			animatorStateMachine.UpdateBehaviour();

			animatorStateMachine.UpdateState();
		}

		private void OnAnimatorMove()
		{
			animatorStateMachine.OnAnimatorMove();
		}

		[SuppressMessage("ReSharper", "Unity.PreferNonAllocApi")]
		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 position = transform.position;
			Vector3 spherePosition1 = new(position.x, position.y - groundedOffset1, position.z);
			Vector3 spherePosition2 = new(position.x, position.y - groundedOffset2, position.z);

			_groundedColliders = Physics.OverlapCapsule(spherePosition2, spherePosition1, groundedRadius, groundLayers,
				QueryTriggerInteraction.Ignore);

			grounded = _groundedColliders.Length != 0;
		}

		public bool IsGrounded() => grounded;
		
		public bool IsGroundedToLayer(LayerMask layerMask, out Collider floorCollider)
		{
			floorCollider = null;
			if (_groundedColliders == null) return false;
			
			//converting from layer to layerMask: 1 << layer = layerMask
			bool any = false;
			foreach (Collider groundedCollider in _groundedColliders)
			{
				//condition from: https://www.codegrepper.com/code-examples/csharp/unity+check+if+layer+is+in+layermask
				if ((layerMask.value & (1 << groundedCollider.gameObject.layer)) > 0)
				{
					floorCollider = groundedCollider;
					any = true;
					break;
				}
			}

			return grounded && any;
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = grounded ? transparentGreen : transparentRed;
		
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Vector3 position = transform.position;
			Gizmos.DrawSphere(new Vector3(position.x, position.y - groundedOffset1, position.z), groundedRadius);
			Gizmos.DrawSphere(new Vector3(position.x, position.y - groundedOffset2, position.z), groundedRadius);
		}
#endif
	}
}