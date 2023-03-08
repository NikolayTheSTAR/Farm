using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using World;

namespace TheSTAR.Data
{
    public sealed class DataController : MonoBehaviour
    {
        private const string GameDataFileName = "game_data.json";
        private string ProfileDataFileName(int profileIndex)
        {
            return $"profile_{profileIndex.ToString()}.json";
        }

        public GameData gameData = new();

        [SerializeField] private bool clearData = false;

        #region Unity Methodes

        private void Awake()
        {
            if (clearData) LoadDefault();
            else Load();
        }
        
        #endregion

        private string GameDataPath => Path.Combine(Application.persistentDataPath, GameDataFileName);
        public string ProfileDataPath(int profileIndex)
        {
            string value = Path.Combine(Application.persistentDataPath, ProfileDataFileName(profileIndex));
            return value;
        }
        
        [ContextMenu("Save")]
        public void Save()
        {
            SerializationToJson(out GameData newData);

            string jsonString = JsonUtility.ToJson(newData, true);
            File.WriteAllText(GameDataPath, jsonString);
        }

        [ContextMenu("Load")]
        private void Load()
        {
            if (File.Exists(GameDataPath))
            {
                string jsonString = File.ReadAllText(GameDataPath);
                GameData newData = JsonUtility.FromJson<GameData>(jsonString);
                DeserializationFromJson(newData);
            }
            else LoadDefault();
        }

        [ContextMenu("ClearData")]
        private void LoadDefault()
        {        
            gameData = new();
            Save();
        }

        // Запись данных
        private void SerializationToJson(out GameData newGameData)
        {
            newGameData = gameData;
        }

        // Чтение данных
        private void DeserializationFromJson(GameData fileGameData)
        {
            gameData = fileGameData;
        }

        [Serializable]
        public class GameData
        {
            public List<ItemCountData> itemCounts = new List<ItemCountData>();
            public List<string> completedTutorials = new List<string>();

            public void AddItems(ItemType itemType, int count, out int result)
            {
                var itemCountData = itemCounts.Find(info => info.ItemType == itemType);
                if (itemCountData == null)
                {
                    itemCountData = new ItemCountData(itemType, count);
                    itemCounts.Add(itemCountData);
                }
                else itemCountData.Count += count;
                
                result = itemCountData.Count;
            }

            public int GetItemCount(ItemType itemType)
            {
                var itemCountData = itemCounts.Find(info => info.ItemType == itemType);
                if (itemCountData != null) return itemCountData.Count;
                itemCountData = new ItemCountData(itemType, 0);
                itemCounts.Add(itemCountData);

                return itemCountData.Count;
            }

            public void CompleteTutorial(string id)
            {
                if (!IsTutorialComplete(id)) completedTutorials.Add(id);
            }

            public bool IsTutorialComplete(string id) => completedTutorials.Contains(id);
        }

        [Serializable]
        public class ItemCountData
        {
            public ItemType ItemType;
            public int Count;

            public ItemCountData(ItemType itemType, int count)
            {
                ItemType = itemType;
                Count = count;
            }
        }
    }
}