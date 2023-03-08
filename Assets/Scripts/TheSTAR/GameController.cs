using System;
using Mining;
using TheSTAR.Data;
using TheSTAR.GUI;
using TheSTAR.GUI.FlyUI;
using TheSTAR.GUI.Screens;
using TheSTAR.Input;
using TheSTAR.World.Farm;
using UnityEngine;
using UnityEngine.Serialization;
using World;

namespace TheSTAR
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameWorld world;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private InputController input;
        [SerializeField] private DropItemsContainer drop;
        [SerializeField] private FarmController farm;
        [SerializeField] private DataController data;
        [SerializeField] private TransactionsController transactions;
        [SerializeField] private GuiController gui;
        [SerializeField] private FlyUIContainer flyUI;

        [Space] [SerializeField] private float startGameDelay = 0.5f;

        public event Action OnStartGameEvent;

        /// <summary>
        /// Main logic entry point
        /// </summary>
        private void Start()
        {
            Init();
            Invoke(nameof(StartGame), startGameDelay);
        }

        private void StartGame()
        {
            OnStartGameEvent?.Invoke();
        }

        private void Init()
        {
            farm.Init();
            world.Init(drop, farm, transactions, flyUI);
            cameraController.FocusTo(world.CurrentPlayer);
        
            gui.Init(out var trs);
            var gameScreen = gui.FindScreen<GameScreen>();
            gameScreen.Init(farm);

            trs.Add(world.CurrentPlayer);
        
            input.Init(gameScreen.JoystickContainer, world.CurrentPlayer);
            transactions.Init(data, farm, flyUI, trs);
            drop.Init(transactions, farm, world.CurrentPlayer, world.CurrentPlayer.StopCraft);
        
            flyUI.Init(gui, transactions);
        }
    }
}