using UnityEngine;
using Utils.Event;

namespace Utils.Variables
{
    public abstract class AbstractVariable<T> : ScriptableObject
    {
        protected T runtimeValue;
        [SerializeField] private T storedValue;
        [SerializeField] protected GameEvent onValueChanged;

        private void OnEnable()
        {
            Restore();
        }

        public void Restore() => runtimeValue = storedValue;

        public T Get() => runtimeValue;

        public void Set(T value)
        {
            if (value.Equals(runtimeValue)) return;
            
            runtimeValue = value;
            if(onValueChanged != null) onValueChanged?.Raise();
        }

        public void Copy(AbstractVariable<T> other) => runtimeValue = other.runtimeValue;
    }
}
