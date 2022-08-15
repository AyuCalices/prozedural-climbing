using UnityEngine;
using Utils.MinMaxSlider;

namespace RandomizedEnvironment
{
    public class RotateBehaviour : MonoBehaviour
    {
        [SerializeField] private bool hasRandomRotation;
        [SerializeField][Range(0, 1)] private float objectCanRotateChance;
        [SerializeField][Range(0, 1)] private float rotateXChance;
        [SerializeField][MinMaxSlider(0, 1)] private Vector2 xContributionRange;
        [SerializeField][Range(0, 1)] private float rotateYChance;
        [SerializeField][MinMaxSlider(0, 1)] private Vector2 yContributionRange;
        [SerializeField][Range(0, 1)] private float rotateZChance;
        [SerializeField][MinMaxSlider(0, 1)] private Vector2 zContributionRange;

        [SerializeField][MinMaxSlider(5, 20)] private Vector2 speedRange;
        
        private Quaternion _rotation;
    
        // Start is called before the first frame update
        void Start()
        {
            if (hasRandomRotation)
            {
                transform.rotation = Random.rotation;
            }

            if (Random.Range(0f, 1f) > objectCanRotateChance)
            {
                _rotation = Quaternion.identity;
                return;
            }
            
            float speed = Random.Range(speedRange.x, speedRange.y);
            
            float xRotation = 0;
            if (Random.Range(0f, 1f) < rotateXChance)
            {
                xRotation = Random.Range(xContributionRange.x, xContributionRange.y) / speed;
            }
            
            float yRotation = 0;
            if (Random.Range(0f, 1f) < rotateYChance)
            {
                yRotation = Random.Range(yContributionRange.x, yContributionRange.y) / speed;
            }
            
            float zRotation = 0;
            if (Random.Range(0f, 1f) < rotateZChance)
            {
                zRotation = Random.Range(zContributionRange.x, zContributionRange.y) / speed;
            }

            Vector3 rotation = new (xRotation, yRotation, zRotation);
            _rotation = Quaternion.Euler(rotation);
        }

        private void Update()
        {
            transform.rotation *= _rotation;
        }
    }
}
