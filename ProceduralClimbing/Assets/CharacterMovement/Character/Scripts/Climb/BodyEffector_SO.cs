using Edge_Detection.Scripts;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.Climb
{
    [CreateAssetMenu]
    public class BodyEffector_SO : ScriptableObject
    {
        public LineRenderer lineRenderer;
        
        [SerializeField] private HandPositionData_SO handPosition;
        [SerializeField] [Range(0.5f, 2f)] private float armAngleScaling = 1.2f;
        [SerializeField] private float rootMovementHeight = 0.8f;

        [Header("Directions")]
        public Vector3 edgeToLeftFootDir;
        public Vector3 edgeToRightFootDir;
        [SerializeField] private Vector3 armCenterDirection;
        
        [Header("Raycast")]
        [SerializeField] private float raycastDistance = 2f;
        [SerializeField] private float raycastBackDistance = 1f;
        [SerializeField] private float raycastHeight = 0.65f;
        [SerializeField] private int lowerRaycastCount = 5;
        [SerializeField] private float lowerRaycastDistance = 0.05f;
        
        private Transform _transform;
        private ThirdPersonManager _manager;

        private Vector3 _handTargetToTransform;
        private float _horizontalEdgeDist;

        public float CurrentHangBlend { get; set; }
        
        
        public void Initialize(Transform transform, ThirdPersonManager manager, Vector3 handTargetToTransform, float horizontalEdgeDistance)
        {
            _transform = transform;
            _manager = manager;
            
            _handTargetToTransform = handTargetToTransform;
            _horizontalEdgeDist = horizontalEdgeDistance;
        }
        
        public void InitDirections(EffectorPair leftEffectorPair, EffectorPair rightEffectorPair)
        {
            var rotation = Quaternion.Euler(new Vector3(0, _transform.rotation.eulerAngles.y, 0));
            edgeToLeftFootDir = Quaternion.Inverse(rotation) * (leftEffectorPair.FootTipPos - leftEffectorPair.HandTarget.position);
            edgeToRightFootDir = Quaternion.Inverse(rotation) * (rightEffectorPair.FootTipPos - rightEffectorPair.HandTarget.position);
            
            Vector3 armCenterPos = (_manager.rightArmRoot.position + _manager.leftArmRoot.position) / 2;
            armCenterDirection = armCenterPos - _transform.position;
        }

        public void UpdateBodyPosition(Vector3 rightHandPos, Vector3 leftHandPos, Quaternion targetRotation, out Vector3 targetPosition)
        {
            Vector3 handTargetCenterPosition = (rightHandPos + leftHandPos) / 2;
            float handDistance = Vector3.Distance(ClimbHelper.GetHorizontalPosition(rightHandPos), ClimbHelper.GetHorizontalPosition(leftHandPos)) - handPosition.lateralOffset * 2;
            float verticalPos = (handDistance < 0.01f ? 0f : handDistance) * rootMovementHeight;
            float higherHand = rightHandPos.y > leftHandPos.y ? rightHandPos.y : leftHandPos.y;
            Vector3 horizontalPos = -(targetRotation * Vector3.forward) * _horizontalEdgeDist + handTargetCenterPosition;
            targetPosition = new (horizontalPos.x, _handTargetToTransform.y + higherHand + verticalPos, horizontalPos.z);
        }
        
        public void UpdateBodyRotationY(Vector3 rightHandPos, Vector3 leftHandPos, ref Quaternion targetRotation, ref Vector3 targetPosition)
        {
            //reset rotation, that might have occured through diagonal walls (changing the x,y,z values here directly will result in messed up w values)
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            
            //perform Rotation
            Vector3 handTargetCenterPosition = (rightHandPos + leftHandPos) / 2;
            Vector3 horizontalPos = -(targetRotation * Vector3.forward) * _horizontalEdgeDist + handTargetCenterPosition;
            float currentAngle = Vector3.Angle(leftHandPos - rightHandPos,
                handTargetCenterPosition - horizontalPos);
            targetPosition += ClimbHelper.RotateAroundPivot_ReturnDirection(targetPosition, handTargetCenterPosition, Vector3.up, 90 - currentAngle, ref targetRotation); //90 is a static value here
        }

        public void UpdateBodyRotationXZ(float wallAngle, ref Quaternion targetRot, ref Vector3 targetPos)
        {
            if (!(wallAngle < 0)) return;

            if (armCenterDirection == Vector3.zero)
            {
                Debug.LogError("Init pls!");
            }
            Vector3 armPos = targetPos + armCenterDirection;
            targetPos += ClimbHelper.RotateAroundPivot_ReturnDirection(targetPos, armPos, targetRot * Vector3.right, -wallAngle, ref targetRot);
        }

        public void UpdateBodyToArmPosition(Vector3 rightHandTargetPos, Vector3 leftHandTargetPos, float armAngle, ref Vector3 targetPosition)
        {
            Vector3 armPos = targetPosition + armCenterDirection;
            Vector3 axis = (leftHandTargetPos - rightHandTargetPos).normalized;
            Vector3 handTargetCenterPosition = (rightHandTargetPos + leftHandTargetPos) / 2;
            targetPosition += ClimbHelper.MoveAroundPivot_ReturnDirection(armPos, handTargetCenterPosition,
                axis, armAngle * armAngleScaling);
        }
        
        public Vector3 GetFootPosition(EffectorPair effectorPair, Vector3 handTargetPosition, Vector3 handTargetCenterPosition, Quaternion rotation, float wallAngle)
        {
            Vector3 position = effectorPair.GetFootTargetPosition(handTargetPosition, rotation);
            Vector3 right = rotation * Vector3.right;

            return ClimbHelper.MoveAroundPivot_ReturnPosition(position, handTargetCenterPosition, right, -wallAngle);
        }
        
        public bool GetBodyAngleByRaycast(Vector3 lowerRaycastPosition, Vector3 handTargetCenterPosition, Vector3 direction, out float angle)
        {
            angle = 0;
            Vector3 horizontalForward = ClimbHelper.GetHorizontalPositionNormalized(direction);
            Vector3 backOffset = horizontalForward * raycastBackDistance;
            Vector3 lowerRaycastPositionY = new(lowerRaycastPosition.x,
                handTargetCenterPosition.y - raycastHeight, lowerRaycastPosition.z);
            
            //Instantiate(lineRenderer).SetPositions(new[] {handTargetCenterPosition - backOffset, handTargetCenterPosition - backOffset + horizontalForward});
            for (int i = 0; i < lowerRaycastCount; i++)
            {
                Vector3 posY = Vector3.up * lowerRaycastDistance * i;
                if (Physics.Raycast(lowerRaycastPositionY - backOffset + posY, horizontalForward, out RaycastHit hitFoot, raycastDistance))
                {
                    if (Physics.Raycast(handTargetCenterPosition - backOffset, horizontalForward, out RaycastHit hitHand, raycastDistance))
                    {
                        //Instantiate(lineRenderer).SetPositions(new[] {lowerRaycastPositionY - backOffset + posY, hitFoot.point});
                        Vector3 footToHandCenterDir = hitHand.point - hitFoot.point;
                        float angleWithoutThreshold = Vector3.Angle(horizontalForward, footToHandCenterDir) - 90;
                        angle = Mathf.Abs(angleWithoutThreshold) < 0.1f ? 0 : angleWithoutThreshold;
                        return angle < 30f;
                    }
                }
            }

            return false;
        }
    }
}
