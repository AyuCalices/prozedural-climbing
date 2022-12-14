using UnityEngine;

namespace Utils.Variables
{
    [CreateAssetMenu(fileName = "NewFloatVariable", menuName = "Utils/Variables/Float Variable")]
    public class FloatVariable : AbstractVariable<float>
    {
        public void Add(float value)
        {
            runtimeValue += value;
            if(onValueChanged != null) onValueChanged.Raise();
        }

        public void Add(FloatVariable value)
        {
            runtimeValue += value.runtimeValue;
            if(onValueChanged != null) onValueChanged.Raise();
        }
    }
}
