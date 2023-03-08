using System;

namespace World
{
    public abstract class GameWorldCiObject : GameWorldObject, ICollisionInteractable
    {
        public event Action OnEnterEvent;
        
        public abstract bool CanInteract { get; }
        public abstract CiCondition Condition { get; }
        public abstract void Interact(Player p);
        public abstract void StopInteract(Player p);

        public void OnEnter() => OnEnterEvent?.Invoke();
    }
}