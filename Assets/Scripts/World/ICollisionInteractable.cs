using System;

namespace World
{
    public interface ICollisionInteractable
    {
        bool CanInteract { get; }
        
        CiCondition Condition { get; }

        void Interact(Player p);

        void StopInteract(Player p);

        void OnEnter();
    }

    public enum CiCondition
    {
        None,
        PlayerIsStopped
    }
}