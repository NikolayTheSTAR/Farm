using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace TheSTAR.World.Farm
{ 
    public class DropItemsContainer : MonoBehaviour
    {
        private IDropReceiver _playerDropReceiver;
        private FarmController _farmController;
        private TransactionsController _transactions;
        
        private Dictionary<ItemType, List<ResourceItem>> _itemPools;

        private float _dropWaitAfterCreateTime = 0.2f;
        private const float FlyToReceiverTime = 0.5f;
        private readonly Vector3 _standardDropOffset = new Vector3(0, 0.5f, 0);
        
        private float _randomOffsetRange = 0.2f;

        private Action _onFailDropToFactoryAction;

        private Vector3 CreateItemPosOffset => _standardDropOffset +
           new Vector3(
               Random.Range(-_randomOffsetRange, _randomOffsetRange), 
               Random.Range(-_randomOffsetRange, _randomOffsetRange),
               Random.Range(-_randomOffsetRange, _randomOffsetRange));

        public void Init(TransactionsController transactions, FarmController farmController, IDropReceiver playerDropReceiver, Action onFailDropToFactoryAction)
        {
            _transactions = transactions;
            _playerDropReceiver = playerDropReceiver;
            _itemPools = new Dictionary<ItemType, List<ResourceItem>>();
            _farmController = farmController;

            _randomOffsetRange = transactions.FactoriesConfig.RandomOffsetRange;
            _dropWaitAfterCreateTime = transactions.FactoriesConfig.DropWaitAfterCreateTime;
            _onFailDropToFactoryAction = onFailDropToFactoryAction;
        }
        
        public void DropFromSenderToWorld(IDropSender sender, ItemType dropItemType)
        {
            var offset = CreateItemPosOffset;
            DropItemTo(dropItemType, sender.startSendPos.position + offset, null, () =>
            {   
                _transactions.AddItem(dropItemType);
                sender.OnCompleteDrop();
            });
        }

        private void DropItemTo(ItemType itemType, Vector3 startPos, IDropReceiver receiver, Action completeAction = null)
        {
            receiver?.OnStartReceiving();

            var item = GetItemFromPool(itemType, startPos);
            item.transform.localScale = Vector3.zero;

            LeanTween.scale(item.gameObject, Vector3.one, 0.2f).setOnComplete(() =>
            {
                LeanTween.value(0, 1, _dropWaitAfterCreateTime).setOnComplete(() =>
                {
                    if (receiver != null) FlyToReceiver(receiver);
                    else item.OnDropToWorld(() =>
                    {
                        if (_transactions.IsItemMaxCount(itemType)) return;
                        FlyToReceiver(_playerDropReceiver);
                        item.OnTakeFromWorld();
                    });
                });
            });

            void FlyToReceiver(IDropReceiver r)
            {
                LeanTween.value(0, 1, FlyToReceiverTime).setOnUpdate((value) =>
                {
                    var way = r.transform.position - startPos;
                    item.transform.position = startPos + value * (way);
                    
                    // physic imitation
                    var impulseForce = _farmController.ItemsConfig.Items[(int)itemType].PhysicalImpulse;
                    var dopValueY = Math.Abs((value * value - value) * impulseForce);
                    item.transform.position += new Vector3(0, dopValueY, 0);

                }) .setOnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                    completeAction?.Invoke();
                    r.OnCompleteReceiving();
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
            var result = Get();
            result.OnActivate();

            return result;
            
            ResourceItem Get()
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
            }
            
            ResourceItem CreateItem() => Instantiate(_farmController.GetResourceItemPrefab(itemType), startPos, quaternion.identity, transform);
        }
    }

    public interface IDropSender
    {
        Transform startSendPos { get; }
        void OnCompleteDrop();
    }

    public interface IDropReceiver
    {
        Transform transform { get; }

        void OnStartReceiving();
        void OnCompleteReceiving();
    }
}