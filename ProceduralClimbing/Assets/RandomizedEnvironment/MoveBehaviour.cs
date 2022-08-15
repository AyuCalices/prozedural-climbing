using Tools.LeanTween.Framework;
using UnityEngine;
using Utils.MinMaxSlider;
using Random = UnityEngine.Random;

namespace RandomizedEnvironment
{
    public enum MoveType
    {
        PingPong,
        Random
    }
    
    public class MoveBehaviour : MonoBehaviour
    {
        [SerializeField] private MoveType moveType;
        
        [SerializeField][Range(0, 1)] private float objectMoveChance;
        [SerializeField][Range(0, 1)] private float rotateXChance;
        [SerializeField][MinMaxSlider(-10, 10)] private Vector2 xContributionRange;
        [SerializeField][Range(0, 1)] private float rotateYChance;
        [SerializeField][MinMaxSlider(-10, 10)] private Vector2 yContributionRange;
        [SerializeField][Range(0, 1)] private float rotateZChance;
        [SerializeField][MinMaxSlider(-10, 10)] private Vector2 zContributionRange;
    
        [SerializeField][MinMaxSlider(1, 90)] private Vector2 speedRange;

        // Start is called before the first frame update
        void Start()
        {
            if (Random.Range(0f, 1f) > objectMoveChance)
            {
                return;
            }

            moveType = MoveType.Random;
            
            if (moveType == MoveType.PingPong)
            {
                Vector3 dir = Move(out float speed);
                LeanTween.move(gameObject, gameObject.transform.position + dir, speed).setLoopPingPong();
            }
            else if (moveType == MoveType.Random)
            {
                SetNextPosition();
            }
            
        }

        private void SetNextPosition()
        {
            Vector3 dir = Move(out float speed);
            LeanTween.move(gameObject, gameObject.transform.position + dir, speed).setOnComplete(SetNextPosition);
        }

        private Vector3 Move(out float speed)
        {
            speed = Random.Range(speedRange.x, speedRange.y);

            float xMovement = 0;
            if (Random.Range(0f, 1f) < rotateXChance)
            {
                xMovement = Random.Range(xContributionRange.x, xContributionRange.y) / speed;
            }
                
            float yMovement = 0;
            if (Random.Range(0f, 1f) < rotateYChance)
            {
                yMovement = Random.Range(yContributionRange.x, yContributionRange.y) / speed;
            }
                
            float zMovement = 0;
            if (Random.Range(0f, 1f) < rotateZChance)
            {
                zMovement = Random.Range(zContributionRange.x, zContributionRange.y) / speed;
            }
    
            return new Vector3(xMovement, yMovement, zMovement);
        }
    }
}
