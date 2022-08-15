using UnityEngine;

namespace CharacterMovement.Character.Scripts.Climb
{
    public class Vector3Ref
    {
        private Vector3 vec3;

        public Vector3Ref(Vector3 vector)
        {
            vec3 = vector;
        }

        public Vector3 Get() => vec3;

        public void Set(Vector3 vector) => vec3 = vector;
    }
}
