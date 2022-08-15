using Edge_Detection.Scripts;
using JetBrains.Annotations;
using UnityEngine;
using Utils.Attributes;

namespace CharacterMovement.Character.Scripts.Climb
{
    [CreateAssetMenu]
    public class HookingData_SO : ScriptableObject
    {
        public EffectorPair LeftEffectorPair => leftEffectorPair;
        [SerializeField][ReadOnly] private EffectorPair leftEffectorPair;
        
        public EffectorPair RightEffectorPair => rightEffectorPair;
        [SerializeField][ReadOnly] private EffectorPair rightEffectorPair;

        private Transform _hookingPoint;
        private EdgeDetectionSceneManager _edgeDetectionSceneManager;

        public Vector3 HookingPoint => _hookingPoint.position;
        public Vector3 HookingPointNormal { get; private set; }
        public Transform HookingPointTransform => _hookingPoint.parent;
        private EdgeData _currentEdgeDataIdentifier;
        
        public void Initialize(ThirdPersonManager thirdPersonManager, BodyEffector_SO bodyEffector, Vector3 hookingPoint, Vector3 hookingPointNormal, Transform hookingParent)
        {
            _hookingPoint = new GameObject("HookingPoint").transform;
            UpdateHookingPoint(hookingPoint, hookingPointNormal, hookingParent);

            _edgeDetectionSceneManager = thirdPersonManager.edgeDetectionSceneManager;
            leftEffectorPair = new EffectorPair(thirdPersonManager.leftHandEffector, thirdPersonManager.leftFootEffector, bodyEffector.edgeToLeftFootDir, EffectorType.Left);
            rightEffectorPair = new EffectorPair(thirdPersonManager.rightHandEffector, thirdPersonManager.rightFootEffector, bodyEffector.edgeToRightFootDir, EffectorType.Right);
        }

        public void UpdateHookingPoint(Vector3 hookingPoint, Vector3 hookingPointNormal, Transform hookingParent)
        {
            _hookingPoint.position = hookingPoint;
            _hookingPoint.rotation = Quaternion.LookRotation(hookingPointNormal);
            HookingPointNormal = hookingPointNormal;
            _hookingPoint.parent = hookingParent;
        }

        public void SetNewEdgeData(EdgeData edgeData)
        {
            _currentEdgeDataIdentifier = edgeData;
        }

        private bool IsCurrentEdge(EdgeData a) =>
            a.climbable.GetInstanceID() == _currentEdgeDataIdentifier.climbable.GetInstanceID() &&
            a.GetEdgeIdentifier().Equals(_currentEdgeDataIdentifier.GetEdgeIdentifier());

        public bool CurrentEdgeDataExists()
        {
            if (_currentEdgeDataIdentifier == null) return false;

            return _edgeDetectionSceneManager.GetEdgeData().Exists(IsCurrentEdge);
        }
        
        public bool CurrentEdgeDataExists(EdgeData edgeData)
        {
            if (_currentEdgeDataIdentifier == null) return false;

            return IsCurrentEdge(edgeData);
        }

        public EdgeData GetEdgeData()
        {
            return _edgeDetectionSceneManager.GetEdgeData().Find(IsCurrentEdge);
        }
         
        public void UpdateHookingPoint()
        {
            foreach (var edge in _edgeDetectionSceneManager.GetEdgeData())
            {
                if (IsCurrentEdge(edge))
                {
                    _hookingPoint.position = edge.closestPoint;
                    _currentEdgeDataIdentifier = edge;
                }
            }
        }
    }
}
