using System;
using System.Collections.Generic;
using Configs;
using TheSTAR.Data;
using TheSTAR.GUI;
using TheSTAR.GUI.FlyUI;
using TheSTAR.Utility;
using TheSTAR.World;
using TheSTAR.World.Farm;
using UnityEngine;
using World;

namespace TheSTAR
{
    public class TransactionsController : MonoBehaviour
    {
        private List<ITransactionReactable> _transactionReactables;
        private DataController _data;
        private FarmController _farm;
        private FlyUIContainer _flyUI;

        private const string FactoriesConfigPath = "Configs/FactoriesConfig";
        private FactoriesConfig _factoriesConfig;
        public FactoriesConfig FactoriesConfig
        {
            get
            {
                if (_factoriesConfig == null) _factoriesConfig = Resources.Load<FactoriesConfig>(FactoriesConfigPath);
                return _factoriesConfig;
            }
        }
    
        public void Init(DataController data, FarmController farm, FlyUIContainer flyUI, List<ITransactionReactable> trs)
        {
            _transactionReactables = trs;
            _data = data;
            _farm = farm;
        
            InitReaction();
        }

        public void AddItem(ItemType itemType, int value = 1, bool autoSave = true)
        {
            var itemMaxCount = _farm.ItemsConfig.Items[(int)itemType].MaxValue;

            if (itemMaxCount != null)
            {
                var currentValue = _data.gameData.GetItemCount(itemType);
                var expectedValue = (currentValue + value);

                if (expectedValue > itemMaxCount) value -= (expectedValue - (int)itemMaxCount);
            }
            
            _data.gameData.AddItems(itemType, value, out int result);
            if (autoSave) _data.Save();
        
            Reaction(itemType, result);
        }

        public bool IsItemMaxCount(ItemType itemType)
        {
            var itemMaxCount = _farm.ItemsConfig.Items[(int)itemType].MaxValue;

            if (itemMaxCount == null) return false;
            
            var currentValue = _data.gameData.GetItemCount(itemType);
            return currentValue >= itemMaxCount;
        }

        public void ReduceItem(ItemType itemType, int count = 1, bool autoSave = false, Action completeAction = null, Action failAction = null)
        {
            if (_data.gameData.GetItemCount(itemType) >= count)
            {
                _data.gameData.AddItems(itemType, -count, out int result);
                if (autoSave) _data.Save();
        
                Reaction(itemType, result);
            
                completeAction?.Invoke();
            }
            else failAction?.Invoke();
        }

        public int GetItemsCount(ItemType itemType)
        {
            return _data.gameData.GetItemCount(itemType);
        }

        private void InitReaction()
        {
            var itemTypes = EnumUtility.GetValues<ItemType>();
        
            foreach (var tr in _transactionReactables)
            foreach (var itemType in itemTypes)
                tr.OnTransactionReact(itemType, _data.gameData.GetItemCount(itemType));
        }

        private void Reaction(ItemType itemType, int finalValue)
        {
            foreach (var tr in _transactionReactables) tr.OnTransactionReact(itemType, finalValue);
        }

        public bool CanStartTransaction(Factory factory) => GetItemsCount(factory.FactoryData.FromItemType) > 0;
    }

    public interface ITransactionReactable
    {
        void OnTransactionReact(ItemType itemType, int finalValue);
    }
}