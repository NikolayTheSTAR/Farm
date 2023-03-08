using System;
using UnityEngine;
using World;

namespace TheSTAR.World
{
    public class ResourceItem : MonoBehaviour, ICollisionInteractable
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private Collider col;
        public ItemType ItemType => itemType;

        private bool inWorld = false;
        public bool InWorld => inWorld;
        
        private Action _interactAction;

        public void OnActivate()
        {
            inWorld = false;
            col.enabled = false;
        }

        public void OnDropToWorld(Action interactAction)
        {
            _interactAction = interactAction;
            inWorld = true;
            col.enabled = true;
        }

        public bool CanInteract => inWorld;
        public CiCondition Condition => CiCondition.None;
        public void Interact(Player.Player p)
        {
            Debug.Log("Interact");
            
            _interactAction?.Invoke();
            _interactAction = null;
            inWorld = false;
        }

        public void StopInteract(Player.Player p)
        {
        }

        public void OnEnter()
        {
        }
    }

    public enum ItemType
    {
        Wheat,
        Coin
    }
}
