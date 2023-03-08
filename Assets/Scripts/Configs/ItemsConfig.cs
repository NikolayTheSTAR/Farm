using System;
using UnityEngine;
using World;

namespace Configs
{
    [CreateAssetMenu(menuName = "Data/Items", fileName = "ItemsConfig")]
    public class ItemsConfig : ScriptableObject
    {
        [SerializeField] private ItemData[] items = new ItemData[0];
        public ItemData[] Items => items;
    }

    [Serializable]
    public class ItemData
    {
        [SerializeField] private ItemType itemType;
        [Range(0, 10)]
        [SerializeField] private float physicalImpulse = 2;

        [SerializeField] private Sprite iconSprite;

        public float PhysicalImpulse => physicalImpulse;
        public Sprite IconSprite => iconSprite;
    }
}
