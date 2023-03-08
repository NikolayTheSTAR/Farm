using System;
using System.Collections.Generic;
using Mining;
using TheSTAR.Input;
using TheSTAR.Utility;
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
            var counter = counters.Find(info => info.ItemType == itemType);
            if (counter == null) return;
            
            counter.SetValue(finalValue);   
        }

        public void Init(MiningController mining)
        {
            counters = new List<ItemCounter>();
            var itemTypes = EnumUtility.GetValues<ItemType>();

            ItemCounter counter;
            for (var i = 0; i < itemTypes.Length; i++)
            {
                counter = Instantiate(counterPrefab, countersParent);
                counter.Init(mining.ItemsConfig.Items[i].IconSprite, itemTypes[i]);
                counters.Add(counter);
            }
        }
    }
}