using System;
using System.Collections.Generic;
using TheSTAR.Input;
using TheSTAR.Utility;
using TheSTAR.World;
using TheSTAR.World.Farm;
using UnityEngine;
using World;

namespace TheSTAR.GUI.Screens
{
    public class GameScreen : GuiScreen, ITransactionReactable
    {
        [SerializeField] private JoystickContainer joystickContainer;
        [SerializeField] private List<ItemCounter> counters;
        [SerializeField] private Transform countersParent;
        [SerializeField] private ItemCounter counterPrefab;

        public JoystickContainer JoystickContainer => joystickContainer;
        
        public void OnTransactionReact(ItemType itemType, int finalValue)
        {
            var counter = GetCounter(itemType);
            if (counter == null) return;
            
            counter.SetValue(finalValue);   
        }

        public void Init(FarmController farm)
        {
            counters = new List<ItemCounter>();
            var itemTypes = EnumUtility.GetValues<ItemType>();

            ItemCounter counter;
            for (var i = 0; i < itemTypes.Length; i++)
            {
                counter = Instantiate(counterPrefab, countersParent);
                counter.Init(farm.ItemsConfig.Items[i].IconSprite, itemTypes[i], farm.ItemsConfig.Items[i].MaxValue);
                counters.Add(counter);
            }
        }

        public ItemCounter GetCounter(ItemType itemType) => counters.Find(info => info.ItemType == itemType);
    }
}