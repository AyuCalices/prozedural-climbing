using System.Collections.Generic;
using System.Linq;

namespace CharacterMovement.Character.Scripts.StateMachine
{
    /// <summary>
    /// A MonoBehaviour who uses a StateMachine decides whether a State
    /// is changed or not based on Events and communicates it to the StateMachine
    /// and if it does it provides a new State (IState Object).
    /// It can also request to go back to a previous state.
    /// </summary>
    public class AnimatorStateMachine
    {
        private AnimatorState_SO _currentStateAnimator;
        private AnimatorState_SO _previousStateAnimator;
        private List<AnimatorState_SO> _states;
        
        public AnimatorState_SO GetCurrentState() => _currentStateAnimator;
        
        public AnimatorState_SO GetPreviousState() => _previousStateAnimator;

        public void Initialize(AnimatorState_SO startingStateAnimator, List<AnimatorState_SO> states, ThirdPersonManager manager)
        {
            _states = states;
            foreach (var state in _states)
            {
                state.Initialize(manager);
            }
            
            _currentStateAnimator = startingStateAnimator;
            startingStateAnimator.InternalEnter();
        }

        public void ChangeState(AnimatorState_SO newStateAnimator)
        {
            _currentStateAnimator.Exit();
            _previousStateAnimator = _currentStateAnimator;
            _currentStateAnimator = newStateAnimator;
            _currentStateAnimator.InternalEnter();
        }

        public void UpdateBehaviour()
        {
            _currentStateAnimator.InternalUpdate();
        }

        public void OnAnimatorMove()
        {
            _currentStateAnimator.OnAnimatorMove();
        }

        public void UpdateState()
        {
            foreach (AnimatorState_SO state in _states.Where(state => state.stateTransitionEachFrame))
            {
                state.RequestState(_currentStateAnimator);
            }
        }

        public void SwitchToPreviousState()
        {
            _currentStateAnimator.Exit();
            _currentStateAnimator = _previousStateAnimator;
            _currentStateAnimator.InternalEnter();
        }
    }
}