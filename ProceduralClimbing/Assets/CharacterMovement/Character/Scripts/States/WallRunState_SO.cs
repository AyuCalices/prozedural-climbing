using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.StateMachine;
using UnityEngine;
using Utils.Variables;

namespace CharacterMovement.Character.Scripts.States
{
    [CreateAssetMenu(fileName = "Wall Run State", menuName = "Character/States/Wall Run State")]
    public class WallRunState_SO : AnimatorState_SO
    {
        [Header("Positioning")]
        public FloatVariable wallDistance;
        
        [Header("State Swap")] 
        public EdgeHookingState_SO edgeHookingState;
        public FreeHookingState_SO freeHookingState;
        
        [Header("Wall Run")]
        public AnimationClip wallRun;
        public float wallRaycastDistance = 0.9f;
        public float stateEnterAngle = 15f;
        public float timePassedForHookEnabled = 0.4f;

        [Header("Above Character Validity Check")]
        public bool debug;
        public float characterHeight = 1.7f;
        public float characterUpDirRaycastLength = 2.8f;

        private Vector3 _startPos;
        private Vector3 _targetPos;

        private float _deltaTime;

        public override void RequestState(AnimatorState_SO currentStateAnimator)
        {
            if (manager.IsGrounded() && currentStateAnimator is GroundedState_SO && Input.climb)
            {
                RotateTowardsWall(currentStateAnimator);
            }
        }

        protected override void Enter()
        {
            Controller.enabled = false;
            _deltaTime = 0f;

            //move character a certain wall distance away from the wall
            if (IsWallHit(out RaycastHit hit))
            {
                _startPos = transform.position;
                _targetPos = new Vector3(hit.point.x, _startPos.y, hit.point.z) + ClimbHelper.GetHorizontalPositionNormalized(hit.normal) * wallDistance.Get();
            }

            Animator.SetFloat(animIDSpeed, 0);
                
            float inputMagnitude = Input.analogMovement ? Input.move.magnitude : 1f;
            Animator.SetFloat(animIDMotionSpeed, inputMagnitude);
                
            Animator.SetBool(animIDGrounded, true);
            Animator.SetBool(animIDWallRun, true);
        }

        protected override void Update()
        {
            //apply position
            Vector3 targetDeltaDir = (_targetPos - _startPos) * (Time.deltaTime / wallRun.length);
            transform.position += new Vector3(targetDeltaDir.x, Animator.deltaPosition.y, targetDeltaDir.z);
            
            //exit state if no hookable edge was found
            if (_deltaTime >= wallRun.length)
            {
                freeHookingState.RequestState(this);

                if (manager.CurrentState.GetType() != freeHookingState.GetType())
                {
                    RequestDefaultState();
                }
            }
            //try hook on edge
            else if (_deltaTime >= timePassedForHookEnabled && manager.edgeDetectionSceneManager.HasHookableEdge())
            {
                edgeHookingState.RequestState(this);
            }

            _deltaTime += Time.deltaTime;
        }

        public override void Exit()
        {
            Controller.enabled = true;

            Animator.SetBool(animIDWallRun, false);
            Animator.SetBool(animIDGrounded, false);
        }

        private bool IsWallHit(out RaycastHit hit)
        {
            Transform characterModelTransform = manager.transform;
            var characterModelPos = characterModelTransform.position;
            Vector3 rayOrigin = new (characterModelPos.x, manager.shoulderRoot.transform.position.y, characterModelPos.z);
            Vector3 dir = characterModelTransform.TransformDirection(Vector3.forward);

            if (debug)
            {
                Debug.DrawRay(rayOrigin, dir * wallRaycastDistance, Color.red);
            }

            return Physics.Raycast(rayOrigin, dir, out hit, wallRaycastDistance);
        }
        
        private void RotateTowardsWall(AnimatorState_SO currentStateAnimator)
        {
            if (!IsSpaceAboveCharacter()) return;
            
            if (IsWallHit(out RaycastHit hit))
            {
                float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(-hit.normal));
                if (currentStateAnimator is not WallRunState_SO && angle < stateEnterAngle)
                {
                    transform.rotation = Quaternion.LookRotation(-hit.normal);
                    AnimatorStateMachine.ChangeState(this);
                }
            }
        }

        private bool IsSpaceAboveCharacter()
        {
            var position = transform.position;
            if (debug)
            {
                Debug.DrawLine(position + Vector3.up * characterHeight,
                    position + Vector3.up * characterUpDirRaycastLength);
            }
            return !Physics.Raycast(position + Vector3.up * characterHeight, Vector3.up, characterUpDirRaycastLength - characterHeight);
        }
    }
}
