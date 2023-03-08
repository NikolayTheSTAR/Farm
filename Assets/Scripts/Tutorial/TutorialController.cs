using System;
using Sirenix.OdinInspector;
using TheSTAR.Data;
using UnityEngine;
using World;

namespace Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private bool useTutorial;
        
        [ShowIf("useTutorial")]
        [SerializeField] private TutorialData[] tutorialDatas = new TutorialData[0];
        
        [ShowIf("useTutorial")]
        [SerializeField] private TutorialArrow arrow;

        private Player _player;
        private TutorialData _currentTutorial;
        private bool _inProcess = false;
        private DataController _data;

        public void Init(GameController gameController, DataController data, Player p)
        {
            if (!useTutorial) return;
            
            _data = data;
            _player = p;
            
            for (int i = 0; i < tutorialDatas.Length; i++)
            {
                var tutorialData = tutorialDatas[i];

                switch (tutorialData.Condition)
                {
                    case TutorialShowCondition.ByStart:
                        gameController.OnStartGameEvent += () => StartTutorial(tutorialData);
                        break;
                    case TutorialShowCondition.ByFarm:
                    {
                        Action unsubscribe = null;
                        unsubscribe = () => tutorialData.FarmSource.OnCompleteFarmEvent -= StartTutorialAction;
                        tutorialData.FarmSource.OnCompleteFarmEvent += StartTutorialAction;
                    
                        void StartTutorialAction()
                        {
                            StartTutorial(tutorialData);
                            unsubscribe?.Invoke();
                        }

                        break;
                    }
                }
            }
        }

        private void UpdateInCurrentTutorial()
        {
            arrow.PointTo(_player.transform, _currentTutorial.GoalObject.transform);
        }

        private void StartTutorial(TutorialData tutorial)
        {
            if (_inProcess || _data.gameData.IsTutorialComplete(tutorial.ID)) return;
            
            _inProcess = true;
            
            _currentTutorial = tutorial;
            tutorial.GoalObject.OnEnterEvent += StopTutorial;
            
            arrow.gameObject.SetActive(true);
            arrow.StartAnim();
            UpdateInCurrentTutorial();
            
            _player.OnMoveEvent += UpdateInCurrentTutorial;
        }
        
        private void StopTutorial()
        {
            _currentTutorial.GoalObject.OnEnterEvent -= StopTutorial;
            _data.gameData.CompleteTutorial(_currentTutorial.ID);
            
            _inProcess = false;
            
            arrow.gameObject.SetActive(false);
            arrow.StopAnim();
            _currentTutorial = null;

            _player.OnMoveEvent -= UpdateInCurrentTutorial;
        }
    }

    [Serializable]
    public class TutorialData
    {
        [SerializeField] private string id;
        [SerializeField] private GameWorldCiObject goalObject;
        [SerializeField] private TutorialShowCondition condition;

        [ShowIf("@condition == TutorialShowCondition.ByFarm")] [SerializeField]
        private ResourceSource farmSource;
        
        public GameWorldCiObject GoalObject => goalObject;
        public TutorialShowCondition Condition => condition;
        public ResourceSource FarmSource => farmSource;
        public string ID => id;
    }

    public enum TutorialShowCondition
    {
        Never,
        ByStart,
        ByFarm
    }
}