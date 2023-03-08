using System;
using UnityEngine;
using World;

namespace Configs
{
    [CreateAssetMenu(menuName = "Data/Factories", fileName = "FactoriesConfig")]
    public class FactoriesConfig : ScriptableObject
    {
        [SerializeField] private FactoryData[] factoryDatas = new FactoryData[0];
        [SerializeField] private float dropToFactoryPeriod = 1;
        [SerializeField] private float randomOffsetRange = 0.4f;
        [SerializeField] private float dropWaitAfterCreateTime = 0.2f;
        
        public FactoryData[] FactoryDatas => factoryDatas;
        public float DropToFactoryPeriod => dropToFactoryPeriod;
        public float RandomOffsetRange => randomOffsetRange;
        public float DropWaitAfterCreateTime => dropWaitAfterCreateTime;
        
    }

    [Serializable]
    public class FactoryData
    {
        [SerializeField] private FactoryType _factoryType;
        [SerializeField] private ItemType _fromItemType;
        [SerializeField] private int neededFromItemCount;
        [SerializeField] private ItemType _toItemType;
        [SerializeField] private int resultToItemCount;
        
        [Range(0, 10)]
        [SerializeField] private float craftTime = 1;

        public ItemType FromItemType => _fromItemType;
        public int NeededFromItemCount => neededFromItemCount;
        public ItemType ToItemType => _toItemType;
        public int ResultToItemCount => resultToItemCount;
        public float CraftTime => craftTime;
    }
}