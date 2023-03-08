using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "Data/Character", fileName = "CharacterConfig")]
    public class CharacterConfig : ScriptableObject
    {
        [SerializeField] private float triggerRadius = 1.5f;

        public float TriggerRadius => triggerRadius;
    }
}