using Edge_Detection.Scripts;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace CharacterMovement.Character.Scripts.Climb
{
    public enum EffectorType {Left, Right}
    
    [System.Serializable]
    public class EffectorPair
    {
        [SerializeField] private TwoBoneIKConstraint handEffector;
        [SerializeField] private TwoBoneIKConstraint footEffector;
        
        private readonly Transform _baseHandParent;
        private readonly Transform _baseFootParent;
        private EffectorType _effectorType;
        private readonly Vector3 _footTargetDir;
        
        private Quaternion _startRot;
        private Quaternion _targetRot;
        private float _handRotateDeltaTime;
        private bool _isUpdate;

        public Transform HandTarget { get; private set; }
        public Transform HandParent => HandTarget.parent;
        public Transform FootTarget { get; private set; }
        public Transform FootParent => FootTarget.parent;
        public EffectorType EffectorType => _effectorType;

        public EffectorPair(TwoBoneIKConstraint handEffector, TwoBoneIKConstraint footEffector, Vector3 footTargetDir, EffectorType effectorType)
        {
            _footTargetDir = footTargetDir;
            
            this.handEffector = handEffector;
            HandTarget = handEffector.data.target;
            _baseHandParent = HandTarget.parent;

            this.footEffector = footEffector;
            FootTarget = footEffector.data.target;

            _baseFootParent = FootTarget.parent;

            _effectorType = effectorType;
        }

        public Vector3 FootTipPos => footEffector.data.tip.position;

        public Vector3 GetHandGrabOffset(Vector3 hookingPointNormal, float lateralOffset)
        {
            Vector3 handGrabOffset = Quaternion.AngleAxis(90, Vector3.up) * hookingPointNormal * lateralOffset;
            return _effectorType == EffectorType.Right ? handGrabOffset * -1 : handGrabOffset;
        }
        
        public Vector3 GetFootTargetPosition(Vector3 handTargetPosition, Quaternion rotation)
        {
            return handTargetPosition + (rotation * _footTargetDir);
        }

        public void SetFootTargetToTip()
        {
            FootTarget.position = footEffector.data.tip.position;
        }
        
        public EffectorPair InitUpdate(Quaternion startRot, Quaternion targetRot)
        {
            _startRot = startRot;
            _targetRot = targetRot;
            _handRotateDeltaTime = 0;
            _isUpdate = true;

            return this;
        }
        
        public void UpdateRotation(float effectorTargetLerpTime)
        {
            _handRotateDeltaTime += Time.deltaTime;
            if (_isUpdate && _handRotateDeltaTime / effectorTargetLerpTime < 1)
            {
                Quaternion result = Quaternion.Lerp(_startRot, _targetRot, _handRotateDeltaTime / effectorTargetLerpTime);
                HandTarget.rotation = result;
                FootTarget.rotation = result;
            }
            else
            {
                _isUpdate = false;
            }
        }
        
        public EffectorPair Hook(Transform freeClimbable)
        {
            HandTarget.parent = freeClimbable;
            FootTarget.parent = freeClimbable;

            return this;
        }

        public EffectorPair Unhook()
        {
            HandTarget.parent = _baseHandParent;
            FootTarget.parent = _baseFootParent;

            return this;
        }

        public EffectorPair SetTargetWeight(float weight)
        {
            SetHandTargetWeight(weight);
            SetFootTargetWeight(weight);

            return this;
        }

        public EffectorPair SetHandTargetWeight(float weight)
        {
            handEffector.data.targetPositionWeight = weight;
            handEffector.data.targetRotationWeight = weight;

            return this;
        }

        public EffectorPair SetFootTargetWeight(float weight)
        {
            footEffector.data.targetPositionWeight = weight;

            return this;
        }

        public float GetFootTargetWeight()
        {
            return footEffector.data.targetPositionWeight;
        }
    }
}
