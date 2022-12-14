using System;
using UnityEngine;

namespace Utils.Event
{
    [CreateAssetMenu(fileName = "new ActionEvent", menuName = "Utils/Action Event")]
    public class ActionEvent : ScriptableObject
    {
        private Action listeners;
    
        public void Raise()
        {
            this.listeners?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            this.listeners += listener;
        }

        public void UnregisterListener(Action listener)
        {
            this.listeners -= listener;
        }
    }
}