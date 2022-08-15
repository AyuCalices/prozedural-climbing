using System;
using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.StateMachine;
using Edge_Detection.Scripts;
using Tools.LeanTween.Framework;
using UnityEngine;

//https://forum.unity.com/threads/quaternions-forward-direction.26921/

namespace CharacterMovement.Character.Scripts.States
{
    public enum HorizontalMovementDirectionType { None, Left, Right }
    public enum MotionType { Idle, Motion }
    public enum OrientationType { None, Left, Right }


    [CreateAssetMenu(fileName = "Climbing State", menuName = "Character/States/Climbing State")]
    public abstract class ClimbingState_SO : AnimatorState_SO
    {
        public GameObject sphere;
        public LineRenderer lineRenderer;
        
        [Header("Positioning")] 
        public HookingData_SO hookingData;
        public HandPositionData_SO handPosition;
        public BodyEffector_SO bodyEffector;

        [Header("Traversal")] 
        public float effectorTargetLerpTime = 0.5f;
        public LeanTweenType movementType = LeanTweenType.easeInOutSine;
        public float handTraversalRadius = 0.6f;

        [Header("Edge swap")]
        public float edgeSwapThresholdY = 0.6f;
        public float edgeSwapThresholdXZ = 0.9f;

        [Header("Wall Climb")] 
        public WallClimbState_SO wallClimbState;

        //current traversal state
        protected static MotionType motionType;
        protected static HorizontalMovementDirectionType previousHorizontalMovementDirectionType;

        private float _effectorTargetLerpTimeDelta;
        private float _edgeSwapTimeDelta;
        
        private float _wallAngleStartXZ;
        private float _wallAngleTargetXZ;
        private static float _currentWallAngleXZ;

        private float _startHangBlend;
        private float _targetHangBlend;

        protected EffectorPair LeftEffectorPair => hookingData.LeftEffectorPair;
        protected EffectorPair RightEffectorPair => hookingData.RightEffectorPair;
        protected Vector3 HandTargetCenterPos => (hookingData.LeftEffectorPair.HandTarget.position + hookingData.RightEffectorPair.HandTarget.position) / 2;
        protected Vector3 LeftHandTargetPos => hookingData.LeftEffectorPair.HandTarget.position;
        protected Vector3 RightHandTargetPos => hookingData.RightEffectorPair.HandTarget.position;
        protected float CurrentWallAngleXZ => _currentWallAngleXZ;
        

        public override void RequestState(AnimatorState_SO currentStateAnimator)
        {
            if (currentStateAnimator is WallRunState_SO or AirState_SO)
            {
                AnimatorStateMachine.ChangeState(this);
            }
        }

        protected override void Enter()
        {
            Animator.SetBool(animIDClimb, true);
            
            LeftEffectorPair.SetTargetWeight(1);
            RightEffectorPair.SetTargetWeight(1);

            Controller.enabled = false;
            previousHorizontalMovementDirectionType = HorizontalMovementDirectionType.None;
            motionType = MotionType.Idle;
        }
        
        protected override void Update()
        {
            _currentWallAngleXZ = 0;
            _edgeSwapTimeDelta += Time.deltaTime;
            if (_edgeSwapTimeDelta < _effectorTargetLerpTimeDelta)
            {
                _currentWallAngleXZ = (_wallAngleTargetXZ - _wallAngleStartXZ) * (_edgeSwapTimeDelta / _effectorTargetLerpTimeDelta) + _wallAngleStartXZ;
                
                bodyEffector.CurrentHangBlend = (_targetHangBlend - _startHangBlend) * (_edgeSwapTimeDelta / effectorTargetLerpTime) + _startHangBlend;
            }
            else
            {
                var forward = transform.forward;
                bool hasFootObstacle = bodyEffector.GetBodyAngleByRaycast(HandTargetCenterPos, HandTargetCenterPos, forward, out _currentWallAngleXZ);

                if (!hasFootObstacle)
                {
                    _currentWallAngleXZ = 30;

                    bodyEffector.CurrentHangBlend -= Time.deltaTime / effectorTargetLerpTime;
                    bodyEffector.CurrentHangBlend = Mathf.Clamp(bodyEffector.CurrentHangBlend, 0, 1);
                }
                else
                {
                    bodyEffector.CurrentHangBlend += Time.deltaTime / effectorTargetLerpTime;
                    bodyEffector.CurrentHangBlend = Mathf.Clamp(bodyEffector.CurrentHangBlend, 0, 1);
                }
            }

            Animator.SetFloat(animIDHangingType, bodyEffector.CurrentHangBlend);
            hookingData.LeftEffectorPair.SetFootTargetWeight(bodyEffector.CurrentHangBlend).UpdateRotation(effectorTargetLerpTime);
            hookingData.RightEffectorPair.SetFootTargetWeight(bodyEffector.CurrentHangBlend).UpdateRotation(effectorTargetLerpTime);
            
            Quaternion finalRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            bodyEffector.UpdateBodyPosition(RightHandTargetPos, LeftHandTargetPos, finalRotation, out Vector3 finalPosition);
            bodyEffector.UpdateBodyRotationY(RightHandTargetPos, LeftHandTargetPos, ref finalRotation, ref finalPosition);
            bodyEffector.UpdateBodyToArmPosition(RightHandTargetPos, LeftHandTargetPos, _currentWallAngleXZ, ref finalPosition);
            bodyEffector.UpdateBodyRotationXZ(_currentWallAngleXZ, ref finalRotation, ref finalPosition);
            transform.rotation = finalRotation;
            transform.position = finalPosition;

            if (CurrentHookIsValid())
            {
                if (motionType == MotionType.Motion) return;
                
                Climb();
            }
            else
            {
                RequestDefaultState();
            }
        }

        public override void Exit()
        {
            Animator.SetBool(animIDClimb, false);
            Animator.SetFloat(animIDHangingType, 1);
            
            LeftEffectorPair.Unhook().SetTargetWeight(0);
            RightEffectorPair.Unhook().SetTargetWeight(0);

            previousHorizontalMovementDirectionType = HorizontalMovementDirectionType.None;
            
            Controller.enabled = true;
        }

        private float SetHangingTypeBlend(float startAngleXZ, float targetAngleXZ, bool targetHasObstacle, float lerpTime)
        {
            float footAngleXZ = targetAngleXZ;
            if (targetAngleXZ > 30 || !targetHasObstacle)
            {
                targetAngleXZ = 30;
                footAngleXZ = 0;

                _targetHangBlend = 0;
                _startHangBlend = Animator.GetFloat(animIDHangingType);
                
                hookingData.LeftEffectorPair.SetFootTargetToTip();
                hookingData.RightEffectorPair.SetFootTargetToTip();
            }
            else
            {
                _targetHangBlend = 1;
                _startHangBlend = Animator.GetFloat(animIDHangingType);
                
                hookingData.LeftEffectorPair.SetFootTargetToTip();
                hookingData.RightEffectorPair.SetFootTargetToTip();
            }
            
            _wallAngleTargetXZ = targetAngleXZ;
            _wallAngleStartXZ = startAngleXZ;
            _edgeSwapTimeDelta = 0f;
            _effectorTargetLerpTimeDelta = lerpTime;

            return footAngleXZ;
        }

        private bool CheckNextPosition(EffectorPair firstEffectorPair, Vector3 firstHandTargetPosition, Vector3 secondHandTargetPosition, Vector3 hookingPointNormal, out bool targetHasObstacle, out float targetAngleXZ)
        {
            Vector3 handTargetCenterPos = (firstHandTargetPosition + secondHandTargetPosition) / 2;
            Quaternion targetRotation = Quaternion.LookRotation(ClimbHelper.GetHorizontalPositionNormalized(-hookingPointNormal));
            Vector3 rightEffectorPosition = firstEffectorPair.EffectorType == EffectorType.Right ? firstHandTargetPosition : secondHandTargetPosition;
            Vector3 leftEffectorPosition = firstEffectorPair.EffectorType == EffectorType.Left ? firstHandTargetPosition : secondHandTargetPosition;

            bodyEffector.UpdateBodyPosition(rightEffectorPosition, leftEffectorPosition, targetRotation, out Vector3 finalPosition);
            targetHasObstacle = bodyEffector.GetBodyAngleByRaycast(finalPosition, handTargetCenterPos, targetRotation * Vector3.forward, out targetAngleXZ);
            bodyEffector.UpdateBodyToArmPosition(rightEffectorPosition, leftEffectorPosition, targetAngleXZ, ref finalPosition);
            bodyEffector.UpdateBodyRotationXZ(targetAngleXZ, ref targetRotation, ref finalPosition);

            Vector3 firstSphereCheckPosition;
            Vector3 secondSphereCheckPosition;
            if (targetHasObstacle)
            {
                firstSphereCheckPosition = finalPosition + targetRotation * (Vector3.up * 0.4f + Vector3.forward * -0.15f);
                secondSphereCheckPosition = finalPosition + targetRotation * (Vector3.up * 0.7f + Vector3.forward * -0.15f);
            }
            else
            {
                firstSphereCheckPosition = finalPosition + targetRotation * (Vector3.up * 0.2f + Vector3.forward * 0.05f);
                secondSphereCheckPosition = finalPosition + targetRotation * (Vector3.up * 0.5f + Vector3.forward * 0.05f);
            }

            //Debug.Log(Physics.CheckSphere(firstSphereCheckPosition, 0.2f) + " " + Physics.CheckSphere(secondSphereCheckPosition, 0.2f));
            //Instantiate(sphere).transform.position = firstSphereCheckPosition;
            //Instantiate(sphere).transform.position = secondSphereCheckPosition;
            
            return !Physics.CheckCapsule(firstSphereCheckPosition, secondSphereCheckPosition, 0.15f);
        }

        protected void HookingPointSwap(EffectorPair firstEffectorPair, Vector3 firstHandPosition, EffectorPair secondEffectorPair, Vector3 secondHandPosition, 
            Vector3 hookingPoint, Vector3 hookingPointNormal, Transform nextParent, EdgeData nextEdge, bool changeState)
        {
            Vector3 handTargetCenterPos = (firstHandPosition + secondHandPosition) / 2;
            
            if (!CheckNextPosition(firstEffectorPair, firstHandPosition, secondHandPosition, hookingPointNormal,
                out bool targetHasObstacle, out float targetAngleXZ)) return;
            
            if (changeState)
            {
                AnimatorStateMachine.ChangeState(this);
            }

            motionType = MotionType.Motion;
            float footAngleXZ = SetHangingTypeBlend(CurrentWallAngleXZ, targetAngleXZ, targetHasObstacle, effectorTargetLerpTime * 2);
            
            //convert to local Positions
            Quaternion finalRotation = Quaternion.LookRotation(-hookingPointNormal);
            Vector3 firstFootTargetPosition = bodyEffector.GetFootPosition(firstEffectorPair, firstHandPosition, handTargetCenterPos, finalRotation, footAngleXZ);
            Vector3 firstFootTargetLocalPosition = nextParent.InverseTransformPoint(firstFootTargetPosition);
            
            Vector3 secondFootTargetPosition = bodyEffector.GetFootPosition(secondEffectorPair, secondHandPosition, handTargetCenterPos, finalRotation, footAngleXZ);
            Vector3 secondFootTargetLocalPosition = nextParent.InverseTransformPoint(secondFootTargetPosition);
            
            Vector3 firstHandLocalPosition = nextParent.InverseTransformPoint(firstHandPosition);
            Vector3 secondHandLocalPosition = nextParent.InverseTransformPoint(secondHandPosition);
            
            //calculate rotation
            Quaternion targetRotation = Quaternion.LookRotation(-hookingPointNormal);
            Quaternion firstHandTargetRotation;
            Quaternion secondHandTargetRotation;
            if (firstEffectorPair.EffectorType == EffectorType.Right)
            {
                firstHandTargetRotation = Quaternion.Euler(
                    targetRotation.eulerAngles.x + handPosition.rightHandRotation.x,
                    targetRotation.eulerAngles.y + handPosition.rightHandRotation.y,
                    targetRotation.eulerAngles.z + handPosition.rightHandRotation.z);
                
                secondHandTargetRotation = Quaternion.Euler(
                    targetRotation.eulerAngles.x + handPosition.leftHandRotation.x,
                    targetRotation.eulerAngles.y + handPosition.leftHandRotation.y,
                    targetRotation.eulerAngles.z + handPosition.leftHandRotation.z);
            }
            else
            {
                firstHandTargetRotation = Quaternion.Euler(
                    targetRotation.eulerAngles.x + handPosition.leftHandRotation.x,
                    targetRotation.eulerAngles.y + handPosition.leftHandRotation.y,
                    targetRotation.eulerAngles.z + handPosition.leftHandRotation.z);
                
                secondHandTargetRotation = Quaternion.Euler(
                    targetRotation.eulerAngles.x + handPosition.rightHandRotation.x,
                    targetRotation.eulerAngles.y + handPosition.rightHandRotation.y,
                    targetRotation.eulerAngles.z + handPosition.rightHandRotation.z);
            }

            //perform edge swap
            hookingData.UpdateHookingPoint(hookingPoint, hookingPointNormal, nextParent);
            firstEffectorPair.Hook(nextParent).InitUpdate(firstEffectorPair.HandTarget.rotation, firstHandTargetRotation);
            PlaceFoot(firstEffectorPair, firstFootTargetLocalPosition);
            PlaceHand(firstEffectorPair, firstHandLocalPosition, () =>
            {
                //firstEffectorPair.HandTarget.rotation = Quaternion.LookRotation(-hookingPointNormal);
                secondEffectorPair.Hook(nextParent).InitUpdate(secondEffectorPair.HandTarget.rotation, secondHandTargetRotation);
                PlaceFoot(secondEffectorPair, secondFootTargetLocalPosition);
                PlaceHand(secondEffectorPair, secondHandLocalPosition, () =>
                {
                    hookingData.SetNewEdgeData(nextEdge);
                    //secondEffectorPair.HandTarget.rotation = Quaternion.LookRotation(-hookingPointNormal);
                    motionType = MotionType.Idle;
                });
            });
        }
        
        protected void TraversalClimb(EffectorPair firstEffectorPair, EffectorPair secondEffectorPair, Vector3 dir, Action setOnComplete = null)
        {
            Transform firstHandTarget = firstEffectorPair.HandTarget;
            Transform secondHandTarget = secondEffectorPair.HandTarget;

            Vector3 firstHandTargetLocalPosition = firstHandTarget.localPosition + firstEffectorPair.HandParent.InverseTransformVector(dir);
            Vector3 firstHandTargetPosition = firstEffectorPair.HandParent.TransformPoint(firstHandTargetLocalPosition);
            Vector3 secondHandTargetPosition = secondHandTarget.position;

            if (!CheckNextPosition(firstEffectorPair, firstHandTargetPosition, secondHandTargetPosition, hookingData.HookingPointNormal,
                out bool targetHasObstacle, out float targetAngleXZ)) return;

            motionType = MotionType.Motion;
            
            Vector3 handTargetCenterPos = (firstHandTargetPosition + secondHandTargetPosition) / 2;
            LeanTween.moveLocal(firstHandTarget.gameObject, firstHandTargetLocalPosition, effectorTargetLerpTime).setEase(movementType).setOnComplete(() =>
            {
                motionType = MotionType.Idle;
                hookingData.UpdateHookingPoint();
                setOnComplete?.Invoke();
            });
            
            //Body AngleXZ
            Quaternion targetBodyRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            float footAngleXZ = SetHangingTypeBlend(CurrentWallAngleXZ, targetAngleXZ, targetHasObstacle, effectorTargetLerpTime);

            Vector3 firstTarget = bodyEffector.GetFootPosition(firstEffectorPair, firstHandTargetPosition, handTargetCenterPos, targetBodyRotation, footAngleXZ);
            LeanTween.moveLocal(firstEffectorPair.FootTarget.gameObject, firstEffectorPair.FootParent.InverseTransformPoint(firstTarget), effectorTargetLerpTime).setEase(movementType);

            Vector3 secondTarget = bodyEffector.GetFootPosition(secondEffectorPair, secondHandTarget.position, handTargetCenterPos, targetBodyRotation, footAngleXZ);
            LeanTween.moveLocal(secondEffectorPair.FootTarget.gameObject, secondEffectorPair.FootParent.InverseTransformPoint(secondTarget), effectorTargetLerpTime).setEase(movementType);
        }

        protected Vector3 GetFinalHandPosition(Vector3 targetPos, Vector3 handGrabOffset, Vector3 forward)
        {
            return targetPos - new Vector3(0, handPosition.yOffset, 0) + handGrabOffset + forward;
        }

        private void PlaceHand(EffectorPair effectorPair, Vector3 targetLocalPosition, Action setOnComplete)
        {
            LeanTween.moveLocal(effectorPair.HandTarget.gameObject, targetLocalPosition,
                effectorTargetLerpTime).setEase(movementType).setOnComplete(setOnComplete.Invoke);
        }

        private void PlaceFoot(EffectorPair effectorPair, Vector3 targetLocalPosition)
        {
            LeanTween.moveLocal(effectorPair.FootTarget.gameObject, targetLocalPosition, effectorTargetLerpTime).setEase(movementType);
        }
        

        protected abstract bool CurrentHookIsValid();
        
        protected abstract void Climb();
        
        protected void WallClimb()
        {
            if (motionType is MotionType.Motion) return;

            if (Input.move.y > 0)
            {
                wallClimbState.RequestState(this);
            }
        }
    }
}
