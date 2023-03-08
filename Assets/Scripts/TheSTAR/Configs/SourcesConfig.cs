using System;
using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;
using World;

namespace Configs
{
    [CreateAssetMenu(menuName = "Data/Sources", fileName = "SourcesConfig")]
    public class SourcesConfig : ScriptableObject
    {
        [SerializeField] private SourceData[] sourceDatas = new SourceData[0];
        public SourceData[] SourceDatas => sourceDatas;
    }

    [Serializable]
    public class SourceData
    {
        [SerializeField] private SourceType _sourceType;
        [SerializeField] private ItemType _dropItemType;
        [SerializeField] private SourceMiningData _miningData;

        public ItemType DropItemType => _dropItemType;
        public SourceMiningData MiningData => _miningData;
    }
    
    [Serializable]
    public class SourceMiningData
    {
        [Range(0.01f, 3)]
        [LabelText("Mining Period (s)")]
        [SerializeField] private float _miningPeriod = 1;
        
        [Range(1, 10)]
        [SerializeField] private int _oneHitDropCount = 1;

        [Range(1, 100)]
        [SerializeField] private int _maxHitsCount;
        [SerializeField] private TimeData _recoveryTime;

        public float MiningPeriod => _miningPeriod;
        public int OneHitDropCount => _oneHitDropCount;
        public int MaxHitsCount => _maxHitsCount;
        public TimeData RecoveryTime => _recoveryTime;
    }

    [Serializable]
    public struct TimeData
    {
        [Range(0, 59)]
        [SerializeField] private int seconds;
        
        [Range(0, 59)]
        [SerializeField] private int minutes;
        
        [Range(0, 23)]
        [SerializeField] private int hours;
        
        public int Seconds => seconds;
        public int Minutes => minutes;
        public int Hours => hours;
    }
}