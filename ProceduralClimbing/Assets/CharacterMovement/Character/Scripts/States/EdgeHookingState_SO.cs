using System;
using System.Collections.Generic;
using CharacterMovement.Character.Scripts.Climb;
using CharacterMovement.Character.Scripts.StateMachine;
using Edge_Detection.Scripts;
using UnityEngine;

namespace CharacterMovement.Character.Scripts.States
{ 
    [CreateAssetMenu(fileName = "Hooking State", menuName = "Character/States/Hooking State")]
    public class EdgeHookingState_SO : HookingState_SO
    {
        [Header("State enter edge distance")]
        public float hookingDistanceXZ = 0.8f;
        public float hookingDistanceY = 0.25f;

        //hook target
        private EdgeData _targetEdgeData;

        public override void RequestState(AnimatorState_SO currentStateAnimator)
        {
            List<EdgeData> hookableEdgeData = manager.edgeDetectionSceneManager.GetEdgeData();
            if (hookableEdgeData.Count == 0) return;
            
            EdgeData[] edges = Array.Empty<EdgeData>();
            if (hookableEdgeData.Count == 1)
            {
                edges = new[] {manager.edgeDetectionSceneManager.GetEdgeDataAt(0)};
            }
            else if (hookableEdgeData.Count > 1)
            {
                edges = new[] {manager.edgeDetectionSceneManager.GetEdgeDataAt(0), manager.edgeDetectionSceneManager.GetEdgeDataAt(1)};
            }

            if (currentStateAnimator is WallRunState_SO or AirState_SO
                && Input.climb && CheckEdges(out EdgeData foundEdgeData, edges))
            {
                _targetEdgeData = foundEdgeData;
                transform.rotation = Quaternion.LookRotation(ClimbHelper.GetHorizontalPositionNormalized(-foundEdgeData.edgeNormal));
                AnimatorStateMachine.ChangeState(this);
            }
        }
        
        private bool CheckEdges(out EdgeData foundEdgeData, params EdgeData[] edgeDataArray)
        {
            foundEdgeData = null;
            
            foreach (var edgeData in edgeDataArray)
            {
                Vector3 closestPointPos = edgeData.closestPoint;
                Vector2 closestEdgeXZ = new(closestPointPos.x, closestPointPos.z);
                
                Vector3 baseTransformPos = manager.edgeDetectionSceneManager.GetBaseTransform().position;
                Vector2 baseTransformXZ = new(baseTransformPos.x, baseTransformPos.z);

                if (Vector2.Distance(baseTransformXZ, closestEdgeXZ) < hookingDistanceXZ
                    && Mathf.Abs(closestPointPos.y - baseTransformPos.y) < hookingDistanceY)
                {
                    foundEdgeData = edgeData;
                    return true;
                }
            }

            return false;
        }

        protected override void PlaceHand()
        {
            Vector3 normal = ClimbHelper.GetHorizontalPositionNormalized(_targetEdgeData.edgeNormal);
            ClimbHelper.GetOrderedVerticesFromEdge(_targetEdgeData, _targetEdgeData.closestPoint - normal, normal, out Vector3 left, out Vector3 right);

            Vector3 lateralDirLeft = (left - _targetEdgeData.closestPoint).normalized * handPosition.lateralOffset;
            Vector3 lateralDirRight = (right - _targetEdgeData.closestPoint).normalized * handPosition.lateralOffset;
            Vector3 forward = -normal * handPosition.forwardPosition;
            
            manager.rightHandEffector.data.target.position = _targetEdgeData.closestPoint - new Vector3(0, handPosition.yOffset, 0) - lateralDirRight + forward;
            manager.leftHandEffector.data.target.position = _targetEdgeData.closestPoint - new Vector3(0, handPosition.yOffset, 0) - lateralDirLeft + forward;
            
            Quaternion targetRotation = Quaternion.LookRotation(-normal);
            //Instantiate(lineRenderer).SetPositions(new[] {manager.rightHandEffector.data.target.position, manager.rightHandEffector.data.target.position + normal});
            Quaternion rightHandTargetRotation = Quaternion.Euler(
                targetRotation.eulerAngles.x + handPosition.rightHandRotation.x,
                targetRotation.eulerAngles.y + handPosition.rightHandRotation.y,
                targetRotation.eulerAngles.z + handPosition.rightHandRotation.z);
            manager.rightHandEffector.data.target.rotation = rightHandTargetRotation;
            Quaternion leftHandTargetRotation = Quaternion.Euler(
                targetRotation.eulerAngles.x + handPosition.leftHandRotation.x,
                targetRotation.eulerAngles.y + handPosition.leftHandRotation.y,
                targetRotation.eulerAngles.z + handPosition.leftHandRotation.z);
            manager.leftHandEffector.data.target.rotation = leftHandTargetRotation;
            
            hookingData.SetNewEdgeData(_targetEdgeData);
        }
        
        protected override Vector3 GetHookingPoint() => _targetEdgeData.closestPoint;

        protected override Vector3 GetHookingPointNormal() => _targetEdgeData.edgeNormal;

        protected override Transform GetParent() => _targetEdgeData.climbable;
    }
}
