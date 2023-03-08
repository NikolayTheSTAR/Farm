using System.Collections.Generic;
using System.Linq;
using Configs;
using Farm;
using Mining;
using UnityEngine;

namespace World
{
    public class GameWorld : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private FarmSource[] sources = new FarmSource[0];
        [SerializeField] private Factory[] factories = new Factory[0];
        [SerializeField] private Player playerPrefab;
    
        public Player CurrentPlayer { get; private set; }

        private FarmController _farmController;
        private DropItemsContainer _dropItemsContainer;
        private TransactionsController _transactions;

        public void Init(DropItemsContainer dropItemsContainer, FarmController farmController, TransactionsController transactions)
        {
            _farmController = farmController;
            _dropItemsContainer = dropItemsContainer;
            _transactions = transactions;
            
            if (CurrentPlayer != null) Destroy(CurrentPlayer);
            SpawnPlayer();

            SourceType sourceType;
            SourceData sourceData;
            foreach (var source in sources)
            {
                if (source == null) continue;
                sourceType = source.SourceType;
                sourceData = _farmController.SourcesConfig.SourceDatas[(int)sourceType];
                source.Init(sourceData, dropItemsContainer.DropFromSenderToPlayer, (s) =>
                {
                    CurrentPlayer.StopFarm(s);
                    _farmController.StartSourceRecovery(s);
                }, () => CurrentPlayer.RetryInteract());
            }

            FactoryData factoryData = null;
            foreach (var factory in factories)
            {
                if (factory == null) continue;
                
                factoryData = transactions.FactoriesConfig.FactoryDatas[(int)factory.FactoryType];
                factory.Init(factoryData, dropItemsContainer.DropFromSenderToPlayer);
            }
        }
    
        private void SpawnPlayer()
        {
            CurrentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, transform);
            CurrentPlayer.Init(_transactions, _dropItemsContainer.DropToFactory, _transactions.FactoriesConfig.DropToFactoryPeriod);
        }
        
        #if UNITY_EDITOR

        [ContextMenu("RegisterSourcesAndFactories")]
        private void RegisterSourcesAndFactories()
        {
            var allSources = GameObject.FindGameObjectsWithTag("Source");
            var tempSources = allSources.Select(sourceObject => sourceObject.GetComponent<FarmSource>()).Where(s => s != null).ToArray();
            sources = tempSources;
            
            var allFactories = GameObject.FindGameObjectsWithTag("Factory");
            var tempFactories = allFactories.Select(sourceObject => sourceObject.GetComponent<Factory>()).Where(s => s != null).ToArray();
            factories = tempFactories;
        }
        
        #endif
    }
}