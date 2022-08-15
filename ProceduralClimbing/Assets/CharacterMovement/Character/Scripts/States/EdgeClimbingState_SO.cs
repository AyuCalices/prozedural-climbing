using System.Collections.Generic;
using CharacterMovement.Character.Scripts.Climb;
using Edge_Detection.Scripts;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CharacterMovement.Character.Scripts.States
{
    [CreateAssetMenu(fileName = "Edge Climbing State", menuName = "Character/States/Edge Climbing State")]
    public class EdgeClimbingState_SO : ClimbingState_SO
    {
        public FreeClimbingState_SO freeClimbingState;
        
        private const float _threshold = 0.02f;

        protected override bool CurrentHookIsValid()
        {
            if (motionType == MotionType.Motion)
            {
                return true;
            }

            return hookingData.CurrentEdgeDataExists();
        }

        protected override void Climb()
        {
            if (Input.move == Vector2.zero) return;

            freeClimbingState.CheckFreeLeftRight(true);
            if (hookingData.CurrentEdgeDataExists())
            {
                EdgeTraversal(hookingData.GetEdgeData());
            }
            freeClimbingState.CheckFreeBehind(true);
            WallClimb();
            if (AnimatorStateMachine.GetCurrentState() is EdgeClimbingState_SO)
            {
                EdgeSwap(false);
                freeClimbingState.CheckFreeForward(true);
            }
        }

        #region Edge Swap

        public void EdgeSwap(bool changeState)
        {
            if (motionType is MotionType.Motion) return;
            
            //left
            if (Input.move.x < 0)
            {
                if (GetNearestEdge(OrientationType.Left, edgeSwapThresholdY, -edgeSwapThresholdY, false, out EdgeData nextEdgeData))
                {
                    FreeEdgeSwap(nextEdgeData, changeState);
                }
            }
            //right
            else if (Input.move.x > 0)
            {
                if (GetNearestEdge(OrientationType.Right, edgeSwapThresholdY, -edgeSwapThresholdY, false, out EdgeData nextEdgeData))
                {
                    FreeEdgeSwap(nextEdgeData, changeState);
                }
            }
            //top
            else if (Input.move.y > 0)
            {
                if (GetNearestEdge(OrientationType.None, edgeSwapThresholdY, 0, true, out EdgeData nextEdgeData))
                {
                    FreeEdgeSwap(nextEdgeData, changeState);
                }
            }
            //bot
            else if (Input.move.y < 0)
            {
                if (GetNearestEdge(OrientationType.None, 0, -edgeSwapThresholdY, true, out EdgeData nextEdgeData))
                {
                    FreeEdgeSwap(nextEdgeData, changeState);
                }
            }
        }
        
        private bool GetNearestEdge(OrientationType orientationType, float max, float min, bool isVerticalSwap, out EdgeData foundEdgeData)
        {
            foundEdgeData = null;
                
            List<EdgeData> edgeDataList = manager.edgeDetectionSceneManager.GetEdgeData();
            foreach (EdgeData edgeData in edgeDataList)
            {
                if (hookingData.CurrentEdgeDataExists(edgeData))
                {
                    continue;
                }

                float diffY = edgeData.closestPoint.y - hookingData.HookingPoint.y;

                float distance = Vector3.Distance(new Vector3(HandTargetCenterPos.x, 0, HandTargetCenterPos.z),
                    new Vector3(edgeData.closestPoint.x, 0, edgeData.closestPoint.z));
                
                Vector3 shoulderPos = manager.shoulderRoot.transform.position;
                Vector3 shoulderOnHandHeight = new (shoulderPos.x, HandTargetCenterPos.y, shoulderPos.z);
                if (!ClimbHelper.CheckRaycastObstacle(shoulderPos, edgeData.closestPoint) &&
                    !ClimbHelper.CheckRaycastObstacle(shoulderOnHandHeight, edgeData.closestPoint) &&
                    diffY < max && diffY > min && distance < edgeSwapThresholdXZ)
                {
                    if (orientationType == OrientationType.Left &&
                        !ClimbHelper.PointIsOnLeft(transform, edgeData.closestPoint)) continue;
                    
                    if (orientationType == OrientationType.Right &&
                        !ClimbHelper.PointIsOnRight(transform, edgeData.closestPoint)) continue;

                    if (isVerticalSwap && (edgeData.closestPoint == edgeData.edge[0] ||
                                           edgeData.closestPoint == edgeData.edge[1])) return false;

                    //Instantiate(lineRenderer).SetPositions(new[] {edgeData.closestPoint, hookingData.HookingPoint});
                    foundEdgeData = edgeData;
                    return true;
                }
            }

            return false;
        }

        private void FreeEdgeSwap(EdgeData nextEdge, bool changeState)
        {
            Vector3 normal = ClimbHelper.GetHorizontalPositionNormalized(nextEdge.edgeNormal);
            ClimbHelper.GetOrderedVerticesFromEdge(nextEdge, (nextEdge.edge[0] + nextEdge.edge[1]) / 2 - normal, -normal, out Vector3 left, out Vector3 right);

            if (ClimbHelper.PointIsOnLeft(transform, nextEdge.closestPoint))
            {
                EdgeSwap(LeftEffectorPair, left, 
                    RightEffectorPair, right, nextEdge, changeState);
            }
            else
            {
                EdgeSwap(RightEffectorPair, right,
                    LeftEffectorPair, left, nextEdge, changeState);
            }
        }
        
        private void EdgeSwap(EffectorPair firstEffectorPair, Vector3 firstHandDirectionEdge, 
            EffectorPair secondEffectorPair, Vector3 targetEdgeVertex, EdgeData nextEdge, bool changeState)
        {
            //clamp positions, so it doesnt hook directly on a edge but in between
            float edgeLength = Vector3.Distance(firstHandDirectionEdge, targetEdgeVertex);
            float targetDistance = Vector3.Distance(nextEdge.closestPoint, targetEdgeVertex);
            targetDistance = Mathf.Clamp(targetDistance, handPosition.lateralOffset, edgeLength - Mathf.Abs(handPosition.lateralOffset));
                                
            // hand Positioning
            Vector3 hookingPointNormal = ClimbHelper.GetHorizontalPositionNormalized(nextEdge.edgeNormal);
            //Instantiate(lineRenderer).SetPositions(new[] {nextEdge.closestPoint, nextEdge.closestPoint + hookingPointNormal2});
            Vector3 edgeTargetDir = (targetEdgeVertex - firstHandDirectionEdge).normalized * handPosition.lateralOffset;
            Vector3 hookingPoint = GetHookingPoint(firstHandDirectionEdge, targetEdgeVertex, targetDistance);
            Vector3 firstHandPosition = GetFinalHandPosition(hookingPoint, -edgeTargetDir, hookingPointNormal * -handPosition.forwardPosition);
            Vector3 secondHandPosition = GetFinalHandPosition(hookingPoint, edgeTargetDir, hookingPointNormal * -handPosition.forwardPosition);
            
            HookingPointSwap(firstEffectorPair, firstHandPosition, secondEffectorPair, secondHandPosition, hookingPoint,
                hookingPointNormal, nextEdge.climbable, nextEdge, changeState);
        }

        private Vector3 GetHookingPoint(Vector3 firstHandDirectionEdge, Vector3 targetEdgeVertex, float targetDistance)
        {
            return targetEdgeVertex + (firstHandDirectionEdge - targetEdgeVertex).normalized * targetDistance;
        }
        
        #endregion
        
        #region Edge Traversal

        private void EdgeTraversal(EdgeData currentEdgeData)
        {
            if (motionType is MotionType.Motion) return;

            ClimbHelper.GetOrderedVerticesFromEdge(currentEdgeData, manager.edgeDetectionSceneManager.GetBaseTransform(), out Vector3 leftEdgeVertex,
                out Vector3 rightEdgeVertex);
            
            if (motionType is MotionType.Idle)
            {
                //left
                if (Input.move.x < 0)
                {
                    if (previousHorizontalMovementDirectionType is HorizontalMovementDirectionType.None)
                    {
                        if (GetNextGrabDir( leftEdgeVertex, handPosition.lateralOffset, out Vector3 localGrabDir))
                        {
                            TraversalClimb(LeftEffectorPair, RightEffectorPair, localGrabDir,() =>
                            {
                                previousHorizontalMovementDirectionType = HorizontalMovementDirectionType.Left;
                            });
                        }
                    }
                    else
                    {
                        TraversalClimb(RightEffectorPair, LeftEffectorPair, 
                            ClimbHelper.SetVectorDistance(LeftHandTargetPos, RightHandTargetPos, handPosition.lateralOffset * 2),() =>
                        {
                            previousHorizontalMovementDirectionType = HorizontalMovementDirectionType.None;
                        });
                    }
                }
                //right
                else if (Input.move.x > 0)
                {
                    if (previousHorizontalMovementDirectionType is HorizontalMovementDirectionType.None)
                    {
                        if (GetNextGrabDir(rightEdgeVertex, handPosition.lateralOffset, out Vector3 localGrabDir))
                        {
                            TraversalClimb(RightEffectorPair, LeftEffectorPair, localGrabDir,() =>
                            {
                                previousHorizontalMovementDirectionType = HorizontalMovementDirectionType.Right;
                            });
                        }
                    }
                    else
                    {
                        TraversalClimb(LeftEffectorPair, RightEffectorPair, 
                            ClimbHelper.SetVectorDistance(RightHandTargetPos, LeftHandTargetPos, handPosition.lateralOffset * 2),() =>
                        {
                            previousHorizontalMovementDirectionType = HorizontalMovementDirectionType.None;
                        });
                    }
                }
            }
        }
        
        private bool GetNextGrabDir(Vector3 dir, float lateralOffset, out Vector3 localGrabDir)
        {
            Vector3 target = dir - new Vector3(0, handPosition.yOffset, 0) + transform.forward * handPosition.forwardPosition;
            Vector3 targetPos = (target - HandTargetCenterPos).normalized * handTraversalRadius;
            Vector3 edgePos = (HandTargetCenterPos - target).normalized * lateralOffset;
            float edgeDistance = Vector3.Distance(target + edgePos, HandTargetCenterPos);
                    
            localGrabDir = edgeDistance > handTraversalRadius
                ? targetPos
                : (target + edgePos) - HandTargetCenterPos;
            
            return edgeDistance > Mathf.Abs(lateralOffset);
        }

        #endregion
    }
}

