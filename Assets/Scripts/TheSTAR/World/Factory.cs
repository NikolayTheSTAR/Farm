using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Mining;
using TheSTAR.World.Player;
using UnityEngine;
using Tutorial;

namespace World
{
    [Serializable]
    public class Factory : GameWorldCiObject, IDropReceiver, IDropSender
    {
        [SerializeField] private FactoryType factoryType;
        [SerializeField] private Transform startSendPoint;

        public Transform startSendPos => startSendPoint;

        public override bool CanInteract => true; //!_isSending && (_itemsInStorageCount + _itemsOnWayCount) < _factoryData.NeededFromItemCount;
        public override CiCondition Condition => CiCondition.PlayerIsStopped;
        public FactoryType FactoryType => factoryType;

        private FactoryData _factoryData;
        private int _itemsInStorageCount = 0;
        private int _itemsOnWayCount = 0;
        private bool _isSending = false;

        public FactoryData FactoryData => _factoryData;
        
        private Action<IDropSender, ItemType, int> _dropItemAction;

        public void Init(FactoryData factoryData, Action<IDropSender, ItemType, int> dropItemAction)
        {
            _factoryData = factoryData;
            _dropItemAction = dropItemAction;
        }
        
        public override void Interact(Player p)
        {
            p.StartCraft(this);
        }

        public override void StopInteract(Player p)
        {
            p.StopCraft();
        }

        private void AddNeededResource()
        {
            _itemsInStorageCount++;
            if (_itemsInStorageCount < _factoryData.NeededFromItemCount) return;
            
            _itemsInStorageCount -= _factoryData.NeededFromItemCount;
            _isSending = true;
            
            // wait for craft
            
            LeanTween.value(0, 1, _factoryData.CraftTime).setOnComplete(() =>
            {
                _dropItemAction(this, _factoryData.ToItemType, _factoryData.ResultToItemCount);
            });
        }

        public void OnCompleteDrop()
        {
            _isSending = false;
        }

        public void OnStartReceiving()
        {
            _itemsOnWayCount++;
        }

        public void OnCompleteReceiving()
        {
            _itemsOnWayCount--;
            AddNeededResource();
        }
    }

    public enum FactoryType
    {
        WheatFactory
    }   
}