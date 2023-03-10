using System;
using System.Collections.Generic;
using Configs;
using TheSTAR.Configs;
using UnityEngine;
using World;

namespace TheSTAR.World.Farm
{
    public class FarmController : MonoBehaviour
    {
        private const string SourcesLoadPath = "Configs/SourcesConfig";
        private SourcesConfig _sourceConfig;

        public SourcesConfig SourcesConfig
        {
            get
            {
                if (_sourceConfig == null) _sourceConfig = Resources.Load<SourcesConfig>(SourcesLoadPath);
                return _sourceConfig;
            }
        }

        private const string ItemsConfigLoadPath = "Configs/ItemsConfig";
        private ItemsConfig _itemsConfig;
        public ItemsConfig ItemsConfig
        {
            get
            {
                if (_itemsConfig == null) _itemsConfig = Resources.Load<ItemsConfig>(ItemsConfigLoadPath);
                return _itemsConfig;
            }
        }
        
        private Dictionary<ItemType, ResourceItem> _loadedItemPrefabs;
        private List<RecoveryData> _recoveryDatas = new List<RecoveryData>();
        private bool _isWaitForRecovery = false;
        private float _tik;
        private const float NeededTikValue = 1;

        private string ItemLoadPath(ItemType itemType) => $"Items/{itemType.ToString()}";

        public void Init()
        {
            _loadedItemPrefabs = new Dictionary<ItemType, ResourceItem>();
        }
        
        public ResourceItem GetResourceItemPrefab(ItemType itemType)
        {
            if (_loadedItemPrefabs.ContainsKey(itemType)) return _loadedItemPrefabs[itemType];
            
            var loadedItem = Resources.Load<ResourceItem>(ItemLoadPath(itemType));
            _loadedItemPrefabs.Add(itemType, loadedItem);
            return loadedItem;
        }

        public void StartSourceRecovery(FarmSource source)
        {
            var recoveryDateTime = 
                DateTime.Now + 
                new TimeSpan(
                    source.SourceData.MiningData.RecoveryTime.Hours, 
                    source.SourceData.MiningData.RecoveryTime.Minutes,
                    source.SourceData.MiningData.RecoveryTime.Seconds);
            
            var recoveryData = new RecoveryData(source, recoveryDateTime);
            
            _recoveryDatas.Add(recoveryData);
            _isWaitForRecovery = true;
        }

        private void Update()
        {
            if (!_isWaitForRecovery) return;
            
            _tik += Time.deltaTime;
            if (!(_tik > NeededTikValue)) return;
            _tik = 0;
            
            CheckRecovery();
        }

        private void CheckRecovery()
        {
            bool breakCheck = false;

            while (!breakCheck)
            {
                if (_recoveryDatas.Count == 0)
                {
                    breakCheck = true;
                    continue;
                }
                
                var testRecoveryData = _recoveryDatas[0];

                if (DateTime.Now < testRecoveryData.recoveryTime)
                {
                    breakCheck = true;
                    continue;
                }
            
                testRecoveryData.source.Recovery();
                _recoveryDatas.Remove(testRecoveryData);
            }
            
            _isWaitForRecovery = _recoveryDatas.Count > 0;
        }

        [Serializable]
        private class RecoveryData
        {
            public FarmSource source { get; private set; }
            public DateTime recoveryTime { get; private set; }

            public RecoveryData(FarmSource source, DateTime time)
            {
                this.source = source;
                this.recoveryTime = time;
            }
        }
    }
}