using TheSTAR.Utility;
using UnityEngine;

namespace TheSTAR.World.Player
{
    public class PlayerBackpack : MonoBehaviour
    {
        private const float MinFullnessSize = 0.5f;
        private const float MaxFullnessSize = 1;

        public void SetSize(float fullness)
        {
            fullness = MathUtility.Limit(fullness, 0, 1);

            var scaleValue = MinFullnessSize + (MaxFullnessSize - MinFullnessSize) * fullness;
            transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        }
    }
}