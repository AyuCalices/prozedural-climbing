using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.StateMachine;
using UnityEngine;
using Utils.Variables;

namespace CharacterMovement.Character.Scripts.States
{
    public abstract class HookingState_SO : AnimatorState_SO
    {
        public LineRenderer lineRenderer;
        
        [Header("Positioning")]
        public HookingData_SO hookingData;
        public HandPositionData_SO handPosition;
        public BodyEffector_SO bodyEffector;
        public FloatVariable wallDistance;
        public float transformHeight = 1.1f;
        
        [Space]
        public bool isSetup;
        
        private bool _isFirstIteration;
        private bool _isHanging;

        private EffectorPair LeftEffectorPair => hookingData.LeftEffectorPair;
        private EffectorPair RightEffectorPair => hookingData.RightEffectorPair;
        private Vector3 HandTargetCenterPosition => (manager.leftHandEffector.data.target.position + manager.rightHandEffector.data.target.position) / 2;
        private Vector3 LeftHandTargetPos => hookingData.LeftEffectorPair.HandTarget.position;
        private Vector3 RightHandTargetPos => hookingData.RightEffectorPair.HandTarget.position;
        

        protected override void Enter()
        {
            _isFirstIteration = true;
            Controller.enabled = false;

            Animator.SetBool(animIDClimb, true);
            Animator.SetFloat(animIDHangingType, 1);
            bodyEffector.CurrentHangBlend = 1;
            
            PlaceHand();

            //set base Position
            float targetY = GetHookingPoint().y - transformHeight;
            Vector3 targetPosition = GetHookingPoint() + ClimbHelper.GetHorizontalPositionNormalized(GetHookingPointNormal()) * wallDistance.Get();
            Vector3 transformPosition = new (targetPosition.x, targetY, targetPosition.z);
            transform.position = transformPosition;
            
            //set reference values
            Vector3 handTargetToTransform = transformPosition - HandTargetCenterPosition;
            float horizontalEdgeDistance = Vector3.Distance(ClimbHelper.GetHorizontalPosition(transformPosition),
                ClimbHelper.GetHorizontalPosition(HandTargetCenterPosition));
            bodyEffector.Initialize(transform, manager, handTargetToTransform, horizontalEdgeDistance);
            
            //Instantiate(lineRenderer).SetPositions(new[] {GetClosestPoint(), GetClosestPoint() + GetClosestPointNormal()});
            hookingData.Initialize(manager, bodyEffector, GetHookingPoint(), GetHookingPointNormal(), GetParent());
            hookingData.LeftEffectorPair.Hook(GetParent()).SetHandTargetWeight(1);
            hookingData.RightEffectorPair.Hook(GetParent()).SetHandTargetWeight(1);
        }

        protected override void Update()
        {
            if (isSetup) return;
            
            float exitTimePercent = exitTimeDelta / exitTime;
            
            bool hasFootObstacle = bodyEffector.GetBodyAngleByRaycast(
                transform.position, HandTargetCenterPosition, transform.forward, out float wallAngle);

            float armAngle = wallAngle;
            if (!hasFootObstacle)
            {
                wallAngle = 30f;
                armAngle = 0f;
                Animator.SetFloat(animIDHangingType, 0);
                bodyEffector.CurrentHangBlend = 0;
                _isHanging = true;
            }
            else
            {
                _isHanging = false;

                hookingData.LeftEffectorPair.SetFootTargetWeight(exitTimePercent);
                hookingData.RightEffectorPair.SetFootTargetWeight(exitTimePercent);
            }

            if (_isFirstIteration)
            {
                var rotation = transform.rotation;
                Vector3 leftTarget = bodyEffector.GetFootPosition(LeftEffectorPair, LeftHandTargetPos, HandTargetCenterPosition, rotation, armAngle);
                Vector3 rightTarget = bodyEffector.GetFootPosition(RightEffectorPair, RightHandTargetPos, HandTargetCenterPosition, rotation, armAngle);
                hookingData.LeftEffectorPair.FootTarget.position = leftTarget;
                hookingData.RightEffectorPair.FootTarget.position = rightTarget;
                _isFirstIteration = false;
            }

            Quaternion finalRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);;
            bodyEffector.UpdateBodyPosition(RightHandTargetPos, LeftHandTargetPos, finalRotation, out Vector3 finalPosition);
            bodyEffector.UpdateBodyRotationY(RightHandTargetPos, LeftHandTargetPos, ref finalRotation, ref finalPosition);
            bodyEffector.UpdateBodyToArmPosition(RightHandTargetPos, LeftHandTargetPos, wallAngle, ref finalPosition);
            bodyEffector.UpdateBodyRotationXZ(wallAngle, ref finalRotation, ref finalPosition);
            transform.rotation = finalRotation;
            transform.position = finalPosition;
        }
        
        public override void Exit()
        {
            if (isSetup)
            {
                bodyEffector.InitDirections(hookingData.LeftEffectorPair, hookingData.RightEffectorPair);
            }
            else
            {
                if (_isHanging) return;
                
                hookingData.LeftEffectorPair.SetFootTargetWeight(1);
                hookingData.RightEffectorPair.SetFootTargetWeight(1);
            }
            
            Controller.enabled = true;
        }

        protected abstract void PlaceHand();
        protected abstract Vector3 GetHookingPoint();
        protected abstract Vector3 GetHookingPointNormal();
        protected abstract Transform GetParent();
    }
}
