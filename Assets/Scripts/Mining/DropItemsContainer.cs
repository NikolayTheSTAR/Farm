using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace Mining
{ 
    public class DropItemsContainer : MonoBehaviour
    {
        private IDropReceiver _playerDropReceiver;
        private MiningController _miningController;
        private TransactionsController _transactions;
        
        private Dictionary<ItemType, List<ResourceItem>> _itemPools;

        private float _dropWaitAfterCreateTime = 0.2f;
        private const float FlyToReceiverTime = 0.5f;
        private readonly Vector3 _standardDropOffset = new Vector3(0, 3, -2);
        
        private float _randomOffsetRange = 0.4f;

        private Action _onFailDropToFactoryAction;

        private Vector3 CreateItemPosOffset => _standardDropOffset +
           new Vector3(
               Random.Range(-_randomOffsetRange, _randomOffsetRange), 
               Random.Range(-_randomOffsetRange, _randomOffsetRange),
               Random.Range(-_randomOffsetRange, _randomOffsetRange));

        public void Init(TransactionsController transactions, MiningController miningController, IDropReceiver playerDropReceiver, Action onFailDropToFactoryAction)
        {
            _transactions = transactions;
            _playerDropReceiver = playerDropReceiver;
            _itemPools = new Dictionary<ItemType, List<ResourceItem>>();
            _miningController = miningController;

            _randomOffsetRange = transactions.FactoriesConfig.RandomOffsetRange;
            _dropWaitAfterCreateTime = transactions.FactoriesConfig.DropWaitAfterCreateTime;
            _onFailDropToFactoryAction = onFailDropToFactoryAction;
        }
        
        public void DropFromSenderToPlayer(IDropSender sender, ItemType dropItemType)
        {
            var offset = CreateItemPosOffset;
            DropItemTo(dropItemType, sender.transform.position + offset, _playerDropReceiver, () =>
            {   
                _transactions.AddItem(dropItemType);
                sender.OnCompleteDrop();
            });
        }
        
        private void DropItemTo(ItemType itemType, Vector3 startPos, IDropReceiver receiver, Action completeAction = null)
        {
            receiver.OnStartReceiving();
            
            var item = GetItemFromPool(itemType, startPos);
            item.transform.localScale = Vector3.zero;
            
            LeanTween.scale(item.gameObject, Vector3.one, 0.2f).setOnComplete(() => { LeanTween.value(0, 1, _dropWaitAfterCreateTime).setOnComplete(FlyToReceiver);});

            void FlyToReceiver()
            {
                LeanTween.value(0, 1, FlyToReceiverTime).setOnUpdate((value) =>
                {
                    var way = receiver.transform.position - startPos;
                    item.transform.position = startPos + value * (way);
                    
                    // physic imitation
                    var impulseForce = _miningController.ItemsConfig.Items[(int)itemType].PhysicalImpulse;
                    var dopValueY = Math.Abs((value * value - value) * impulseForce);
                    item.transform.position += new Vector3(0, dopValueY, 0);

                }) .setOnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                    completeAction?.Invoke();
                    receiver.OnCompleteReceiving();
                });
            }
        }

        public void DropToFactory(Factory factory)
        {
            var factoryData = _transactions.FactoriesConfig.FactoryDatas[(int)factory.FactoryType];
            var fromItemType = factoryData.FromItemType;
            var toItemType = factoryData.ToItemType;
            
            _transactions.ReduceItem(fromItemType, 1, false, 
                () => DropItemTo(fromItemType, _playerDropReceiver.transform.position + CreateItemPosOffset, factory), 
                () => _onFailDropToFactoryAction());
        }
        
        private ResourceItem GetItemFromPool(ItemType itemType, Vector3 startPos, bool autoActivate = true)
        {
            if (_itemPools.ContainsKey(itemType))
            {
                var pool = _itemPools[itemType];
                var itemInPool = pool?.Find(info => !info.gameObject.activeSelf);
                if (itemInPool != null)
                {
                    if (autoActivate) itemInPool.gameObject.SetActive(true);
                    itemInPool.transform.position = startPos;
                    return itemInPool;
                }
                
                var newItem = CreateItem();
                pool.Add(newItem);
                return newItem;
            }
            else
            {
                var newItem = CreateItem();
                _itemPools.Add(itemType, new List<ResourceItem>(){newItem});
                return newItem;
            }

            ResourceItem CreateItem() => Instantiate(_miningController.GetResourceItemPrefab(itemType), startPos, quaternion.identity, transform);
        }
    }

    public interface IDropSender
    {
        Transform transform { get; }
        void OnCompleteDrop();
    }

    public interface IDropReceiver
    {
        Transform transform { get; }

        void OnStartReceiving();
        void OnCompleteReceiving();
    }
}