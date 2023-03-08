using System.Collections.Generic;
using System.Linq;
using Configs;
using Mining;
using UnityEngine;

namespace World
{
    public class GameWorld : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private ResourceSource[] sources = new ResourceSource[0];
        [SerializeField] private Factory[] factories = new Factory[0];
        [SerializeField] private Player playerPrefab;
    
        public Player CurrentPlayer { get; private set; }

        private MiningController _miningController;
        private DropItemsContainer _dropItemsContainer;
        private TransactionsController _transactions;

        public void Init(DropItemsContainer dropItemsContainer, MiningController miningController, TransactionsController transactions)
        {
            _miningController = miningController;
            _dropItemsContainer = dropItemsContainer;
            _transactions = transactions;
            
            if (CurrentPlayer != null) Destroy(CurrentPlayer);
            SpawnPlayer();

            SourceType sourceType;
            SourceData sourceData;
            foreach (var source in sources)
            {
                sourceType = source.SourceType;
                sourceData = _miningController.SourcesConfig.SourceDatas[(int)sourceType];
                source.Init(sourceData, dropItemsContainer.DropFromSenderToPlayer, (s) =>
                {
                    CurrentPlayer.StopMining(s);
                    _miningController.StartSourceRecovery(s);
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

        [ContextMenu("RegisterSources")]
        private void RegisterSources()
        {
            var allSources = GameObject.FindGameObjectsWithTag("Source");
            var tempSources = allSources.Select(sourceObject => sourceObject.GetComponent<ResourceSource>()).Where(s => s != null).ToArray();
            sources = tempSources;
        }
        
        #endif
    }
}