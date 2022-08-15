using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.StateMachine;
using Unity.VisualScripting;
using UnityEngine;
namespace CharacterMovement.Character.Scripts.States
{

    [CreateAssetMenu(fileName = "Free Climbing State", menuName = "Character/States/Free Climbing State")]
    public class FreeClimbingState_SO : ClimbingState_SO
    {
        public EdgeClimbingState_SO edgeClimbingState;

        private EffectorPair _currentFirstEffectorPair;
        private EffectorPair _currentSecondEffectorPair;

        protected override bool CurrentHookIsValid()
        {
            return true;
        }

        protected override void Climb()
        {
            if (Input.move == Vector2.zero) return;
            
            edgeClimbingState.EdgeSwap(true);
            
            CheckFreeLeftRight(false);
            
            WallClimb();
            if (AnimatorStateMachine.GetCurrentState() is FreeClimbingState_SO)
            {
                CheckFreeForward(false);
                CheckFreeEdges(false);
                CheckFreeBehind(false);
            }
        }

        #region Hooking Point search Types

        public void CheckFreeLeftRight(bool changeState)
        {
            if (motionType == MotionType.Motion || previousHorizontalMovementDirectionType != HorizontalMovementDirectionType.None) return;
            
            Vector3 right = transform.right;
            Vector3 horizontalLeft = ClimbHelper.GetHorizontalPositionNormalized(-right);
            Vector3 horizontalRight = ClimbHelper.GetHorizontalPositionNormalized(right);
            
            Vector3 position = manager.shoulderRoot.position;
            Vector3 shoulderPosition = new (position.x, hookingData.HookingPoint.y, position.z);
            
            if (Input.move.x < 0 && FindHookingPoint(shoulderPosition, horizontalLeft, out RaycastHit raycastHit))
            {
                SetEffectorPair();
                FreeClimb(_currentFirstEffectorPair, _currentSecondEffectorPair, raycastHit, changeState);
            }
            else if (Input.move.x > 0 && FindHookingPoint(shoulderPosition, horizontalRight, out raycastHit))
            {
                SetEffectorPair();
                FreeClimb(_currentFirstEffectorPair, _currentSecondEffectorPair, raycastHit, changeState);
            }
        }

        public void CheckFreeForward(bool changeState)
        {
            if (motionType == MotionType.Motion) return;
            
            Vector3 climbDirection = GetClimbDirection();
            Vector3 horizontalForward = ClimbHelper.GetHorizontalPositionNormalized(transform.forward);
            
            Vector3 backOffset = horizontalForward * 0.4f;
            Vector3 raycastOrigin = hookingData.HookingPoint - backOffset + climbDirection;
            if (FindHookingPoint(raycastOrigin, horizontalForward, out  RaycastHit raycastHit))
            {
                SetEffectorPair();
                FreeClimb(_currentFirstEffectorPair, _currentSecondEffectorPair, raycastHit, changeState);
            }
        }

        private void CheckFreeEdges(bool changeState)
        {
            if (motionType == MotionType.Motion) return;
            
            Vector3 horizontalForward = ClimbHelper.GetHorizontalPositionNormalized(transform.forward);
            
            Vector3 right = transform.right;
            Vector3 horizontalLeft = ClimbHelper.GetHorizontalPositionNormalized(-right);
            Vector3 horizontalRight = ClimbHelper.GetHorizontalPositionNormalized(right);

            Vector3 raycastOrigin = hookingData.HookingPoint + horizontalForward * 0.1f;
            if (Input.move.x > 0 && FindHookingPoint(raycastOrigin + horizontalRight * edgeSwapThresholdY, horizontalLeft, out RaycastHit raycastHit))
            {
                SetEffectorPair();
                FreeClimb(_currentFirstEffectorPair, _currentSecondEffectorPair, raycastHit, changeState);
            }
            else if (Input.move.x < 0 && FindHookingPoint(raycastOrigin + horizontalLeft * edgeSwapThresholdY, horizontalRight,  out raycastHit))
            {
                SetEffectorPair();
                FreeClimb(_currentFirstEffectorPair, _currentSecondEffectorPair, raycastHit, changeState);
            }
        }

        public void CheckFreeBehind(bool changeState)
        {
            if (motionType == MotionType.Motion) return;
            
            Vector3 climbDirection = GetClimbDirection();
            
            Vector3 forward = transform.forward;
            Vector3 horizontalBack = ClimbHelper.GetHorizontalPositionNormalized(-forward);
            
            Vector3 position = manager.shoulderRoot.position;
            Vector3 shoulderPosition = new (position.x, hookingData.HookingPoint.y, position.z);
            
            if (Input.move.y != 0 && FindHookingPoint(shoulderPosition + climbDirection, horizontalBack, out RaycastHit raycastHit))
            {
                SetEffectorPair();
                FreeClimb(_currentFirstEffectorPair, _currentSecondEffectorPair, raycastHit, changeState);
            }
        }

        #endregion
        
        #region Utilities

        private Vector3 GetClimbDirection()
        {
            Vector3 climbDir = Vector3.zero;
            //right
            if (Input.move.x > 0)
            {
                climbDir = transform.right * edgeSwapThresholdY;
            }
            //left
            else if (Input.move.x < 0)
            {
                climbDir = -transform.right * edgeSwapThresholdY;
            }
            //top
            else if (Input.move.y > 0)
            {
                climbDir = Vector3.up * edgeSwapThresholdY;
            }
            //down
            else if (Input.move.y < 0)
            {
                climbDir = Vector3.down * edgeSwapThresholdY;
            }

            return climbDir;
        }
        
        private void SetEffectorPair()
        {
            //right
            if (Input.move.x > 0)
            {
                _currentFirstEffectorPair = RightEffectorPair;
                _currentSecondEffectorPair = LeftEffectorPair;
            }
            //left
            else if (Input.move.x < 0)
            {
                _currentFirstEffectorPair = LeftEffectorPair;
                _currentSecondEffectorPair = RightEffectorPair;
            }
            //top
            else if (Input.move.y > 0)
            {
                if (_currentFirstEffectorPair.EffectorType == EffectorType.Left)
                {
                    _currentFirstEffectorPair = RightEffectorPair;
                    _currentSecondEffectorPair = LeftEffectorPair;
                }
                else
                {
                    _currentFirstEffectorPair = LeftEffectorPair;
                    _currentSecondEffectorPair = RightEffectorPair;
                }
            }
            //down
            else if (Input.move.y < 0)
            {
                if (_currentFirstEffectorPair.EffectorType == EffectorType.Left)
                {
                    _currentFirstEffectorPair = RightEffectorPair;
                    _currentSecondEffectorPair = LeftEffectorPair;
                }
                else
                {
                    _currentFirstEffectorPair = LeftEffectorPair;
                    _currentSecondEffectorPair = RightEffectorPair;
                }
            }
        }
        
        private bool FindHookingPoint(Vector3 raycastOrigin, Vector3 direction, out RaycastHit raycastHit)
        {
            raycastHit = default;

            RaycastHit[] wallHits = Physics.RaycastAll(raycastOrigin, direction, 1);
            //Instantiate(lineRenderer).SetPositions(new[] {raycastOrigin, raycastOrigin + direction});
            foreach (var wallHit in wallHits)
            {
                if (wallHit.collider.CompareTag("FreeClimbable"))
                {
                    raycastHit = wallHit;
                    return true;
                }       
            }

            return false;
        }
        
        private void FreeClimb(EffectorPair firstEffectorPair, EffectorPair secondEffectorPair, RaycastHit hitHand, bool changeState)
        {
            Vector3 hookingPoint = hitHand.point;
            Vector3 hookingPointNormal = ClimbHelper.GetHorizontalPositionNormalized(hitHand.normal);
            //Instantiate(lineRenderer).SetPositions(new[] {hookingPoint, hookingPoint + hookingPointNormal});

            Vector3 firstHandPosition = GetFinalHandPosition(hookingPoint, firstEffectorPair.GetHandGrabOffset(hookingPointNormal, handPosition.lateralOffset), -hookingPointNormal * handPosition.forwardPosition);
            Vector3 secondHandPosition = GetFinalHandPosition(hookingPoint, secondEffectorPair.GetHandGrabOffset(hookingPointNormal, handPosition.lateralOffset), -hookingPointNormal * handPosition.forwardPosition);
            
            HookingPointSwap(firstEffectorPair, firstHandPosition, secondEffectorPair, secondHandPosition, hookingPoint,
                hookingPointNormal, hitHand.transform, null, changeState);
        }
        
        #endregion
    }
}

