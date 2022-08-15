using Edge_Detection.Scripts;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.Climb
{
    public static class ClimbHelper
    {
        private const float _threshold = 0.01f;
        
        public static bool PointIsOnLeft(Transform origin, Vector3 position)
        {
            Vector3 delta = (position - origin.position).normalized;
            Vector3 cross = Vector3.Cross(delta, origin.forward);
            return cross.y >= -_threshold;
        }
        
        public static bool PointIsOnRight(Transform origin, Vector3 position)
        {
            Vector3 delta = (position - origin.position).normalized;
            Vector3 cross = Vector3.Cross(delta, origin.forward);
            return cross.y <= _threshold;
        }

        public static bool CheckRaycastObstacle(Vector3 originPos, Vector3 targetPos)
        {
            Vector3 originPosition = originPos;
            Vector3 direction = targetPos - originPosition;
            
            return Physics.Raycast(originPosition, direction, Vector3.Distance(originPosition, targetPos) * 0.9f);
        }

        public static Vector3 SetVectorDistance(Vector3 direction, Vector3 originPos, float distance)
        {
            Vector3 directionToOrigin = originPos - direction;
            Vector3 directionToTarget = directionToOrigin.normalized * distance;

            Vector3 originToTargetDir = direction - originPos;
            return originToTargetDir + directionToTarget;
        }
        
        public static Vector3 GetHorizontalPosition(Vector3 position)
        {
            return new(position.x, 0, position.z);
        }
        
        public static Vector3 GetHorizontalPositionNormalized(Vector3 position)
        {
            return new Vector3(position.x, 0, position.z).normalized;
        }
        
        //https://answers.unity.com/questions/1751620/rotating-around-a-pivot-point-using-a-quaternion.html
        public static Vector3 RotateAroundPivot_ReturnDirection(Vector3 position, Vector3 pivotPoint, Vector3 axis, float angle, ref Quaternion rotation)
        {
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            Vector3 res = rot * (position - pivotPoint) + pivotPoint;
            rotation = rot * rotation;
            return res - position;
        }
        
        public static Vector3 MoveAroundPivot_ReturnDirection(Vector3 position, Vector3 pivotPoint, Vector3 axis, float angle)
        {
            Quaternion targetRotation = Quaternion.AngleAxis(angle, axis);
            Vector3 targetPosition = targetRotation * (position - pivotPoint) + pivotPoint;
            return targetPosition - position;
        }
        
        public static Vector3 MoveAroundPivot_ReturnPosition(Vector3 position, Vector3 pivotPoint, Vector3 axis, float angle)
        {
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            return rot * (position - pivotPoint) + pivotPoint;
        }

        //https://stackoverflow.com/questions/65794490/unity3d-check-if-a-point-is-to-the-left-or-right-of-a-vector
        public static void GetOrderedVerticesFromEdge(EdgeData edgeData, Transform baseTransform, out Vector3 left, out Vector3 right)
        {
            GetOrderedVerticesFromEdge(edgeData, baseTransform.position, baseTransform.forward, out left, out right);
        }
        
        //https://stackoverflow.com/questions/65794490/unity3d-check-if-a-point-is-to-the-left-or-right-of-a-vector
        public static void GetOrderedVerticesFromEdge(EdgeData edgeData, Vector3 position, Vector3 forward, out Vector3 left, out Vector3 right)
        {
            left = edgeData.edge[0];
            right = edgeData.edge[1];
            
            //get cross product for both edge points
            Vector3 deltaV0 = (edgeData.edge[0] - position).normalized;
            Vector3 crossV0 = Vector3.Cross(deltaV0, forward);
            Vector3 deltaV1 = (edgeData.edge[1] - position).normalized;
            Vector3 crossV1 = Vector3.Cross(deltaV1, forward);
            
            // Target is on the right side
            if (crossV0.y < 0)
            {
                left = edgeData.edge[1];
                right = edgeData.edge[0];

                if (crossV1.y < 0)
                {
                    if (Vector3.Distance(position, left) > Vector3.Distance(position, right))
                    {
                        left = edgeData.edge[0];
                        right = edgeData.edge[1];
                    }
                }
            }
            
            // Target is on the left side
            if (crossV0.y > 0 && crossV1.y > 0)
            {
                if (Vector3.Distance(position, left) < Vector3.Distance(position, right))
                {
                    left = edgeData.edge[1];
                    right = edgeData.edge[0];
                }
            }
        }
    }
}
