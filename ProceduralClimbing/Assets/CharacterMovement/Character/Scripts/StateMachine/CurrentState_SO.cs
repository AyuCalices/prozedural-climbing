using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Utils.Attributes;

namespace CharacterMovement.Character.Scripts.StateMachine
{
    [CreateAssetMenu]
    public class CurrentState_SO : ScriptableObject
    {
        private AnimatorStateMachine _stateMachineController;

        [SerializeField][ReadOnly][CanBeNull] private AnimatorState_SO _currentState;

        private bool _isInitialized;
        
        public void Initialize(AnimatorStateMachine stateMachineController)
        {
            _isInitialized = true;
            _stateMachineController = stateMachineController;
        }

        private void OnValidate()
        {
            if (_isInitialized)
            {
                _currentState = _stateMachineController.GetCurrentState();
            }
        }

        public void Update()
        {
            if (_isInitialized)
            {
                _currentState = _stateMachineController.GetCurrentState();
            }
        }

        private void OnDisable()
        {
            _isInitialized = false;
        }

        [CanBeNull]
        public AnimatorState_SO GetCurrentState()
        {
            return _stateMachineController.GetCurrentState();
        }
    }
}
