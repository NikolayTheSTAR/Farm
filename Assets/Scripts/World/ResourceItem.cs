using UnityEngine;
using UnityEngine.Serialization;

namespace World
{
    public class ResourceItem : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        public ItemType ItemType => itemType;
    }

    public enum ItemType
    {
        Wheat,
        Coin
    }
}
