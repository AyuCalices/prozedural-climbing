using System.Collections.Generic;
using UnityEngine;
using Utils.MinMaxSlider;

namespace RandomizedEnvironment
{
    public class ObjectPlaceBehaviour : MonoBehaviour
    {
        [SerializeField] private float placeIterations;
        [SerializeField] private List<PlaceObject> placeObjects;

        // Start is called before the first frame update
        private void Awake()
        {
            float[] chanceWorthList = new float[placeObjects.Count];
            
            float totalWorth = 0;
            for (int index = 0; index < placeObjects.Count; index++)
            {
                var placeObject = placeObjects[index];
                totalWorth += placeObject.spawnChance;
                chanceWorthList[index] = totalWorth;
            }

            for (int i = 0; i < placeIterations; i++)
            {
                float randomNumber = Random.Range(0, totalWorth);

                for (int index = 0; index < chanceWorthList.Length; index++)
                {
                    if (chanceWorthList[index] > randomNumber)
                    {
                        PlaceObject placeObject = placeObjects[index];
                        Vector3 position = new (Random.Range(placeObject.xPositionRange.x, placeObject.xPositionRange.y),
                            Random.Range(placeObject.yPositionRange.x, placeObject.yPositionRange.y),
                            Random.Range(placeObject.zPositionRange.x, placeObject.zPositionRange.y));
                        Instantiate(placeObject.prefab, transform).transform.position = position;
                        break;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class PlaceObject
    {
        [SerializeField] public float spawnChance;
        [SerializeField] public GameObject prefab;
        [SerializeField][MinMaxSlider(-30, 30)] public Vector2 xPositionRange;
        [SerializeField][MinMaxSlider(-30, 30)] public Vector2 yPositionRange;
        [SerializeField][MinMaxSlider(-30, 30)] public Vector2 zPositionRange;
    }
}