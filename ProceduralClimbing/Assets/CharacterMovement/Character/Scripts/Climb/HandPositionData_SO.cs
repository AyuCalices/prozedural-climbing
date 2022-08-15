using UnityEngine;

namespace CharacterMovement.Character.Scripts.Climb
{
    [CreateAssetMenu(fileName = "Hand Positioning Data", menuName = "Climb/Hand Positioning Data")]
    public class HandPositionData_SO : ScriptableObject
    {
        public float yOffset = 0.06f;
        [Range(0.01f, 1f)] public float lateralOffset = 0.1f;
        public float forwardPosition = -0.02f;
        public Vector3 leftHandRotation;
        public Vector3 rightHandRotation;
    }
}
