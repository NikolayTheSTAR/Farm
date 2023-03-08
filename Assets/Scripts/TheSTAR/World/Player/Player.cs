using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using TheSTAR.Input;
using TheSTAR.World.Farm;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using World;

namespace TheSTAR.World.Player
{
    public class Player : GameWorldObject, ICameraFocusable, IJoystickControlled, IDropReceiver, ITransactionReactable
    {
        [SerializeField] private NavMeshAgent meshAgent;
        [SerializeField] private EntranceTrigger trigger;
        [SerializeField] private Transform visualTran;
        [SerializeField] private Transform toolArmTran;
        [SerializeField] private PlayerBackpack backpack;
        [SerializeField] private Transform legLeft;
        [SerializeField] private Transform legRight;
        [SerializeField] private Transform armLeft;
        [SerializeField] private Transform armRight;

        private float 
        _mineStrikePeriod = 1,
        _dropToFactoryPeriod = 1;
    
        private bool 
        _isMoving = false, 
        _isFarming = false, 
        _isTransaction = false;

        private TransactionsController _transactions;
        private FarmController _farm;
        
        private List<ICollisionInteractable> _currentCIs;
        private FarmSource _currentSource;
        private Factory _currentFactory;

        private Coroutine
        _farmCoroutine,
        _transactionCoroutine,
        moveCoroutine;

        private int
        _animFarmLTID = -1,
        _animStepLTID = -1;

        private Action<Factory> _dropToFactoryAction;
    
        public event Action OnMoveEvent;

        private const float DefaultMineStrikeTime = 0.5f;
        private const float StepTime = 0.5f;
        private const float StepAngle = 30;
        private const float BackpackStepAngle = 5;
        private const string CharacterConfigPath = "Configs/CharacterConfig";
    
        private CharacterConfig _characterConfig;

        private CharacterConfig CharacterConfig
        {
            get
            {
                if (_characterConfig == null) _characterConfig = Resources.Load<CharacterConfig>(CharacterConfigPath);
                return _characterConfig;
            }
        }

        public void Init(TransactionsController transactions, FarmController farm, Action<Factory> dropToFactoryAction, float dropToFactoryPeriod)
        {
            _farm = farm;
            _transactions = transactions;
            
            trigger.Init(OnEnter, OnExit);
            _dropToFactoryAction = dropToFactoryAction;
            _dropToFactoryPeriod = dropToFactoryPeriod;
        
            trigger.SetRadius(CharacterConfig.TriggerRadius);

            _currentCIs = new List<ICollisionInteractable>();
        }

        #region Logic Enter

        public void JoystickInput(Vector2 input)
        {
            Vector3 finalMoveDirection;

            //Debug.Log(input);
        
            if (input == Vector2.zero)
            {
                finalMoveDirection = transform.position;
                if (_isMoving) OnStopMove();
            }
            else
            {
                var tempMoveDirection = new Vector3(input.x, 0, input.y);
                finalMoveDirection = transform.position + new Vector3(tempMoveDirection.x, 0, tempMoveDirection.z);

                if (!_isMoving) OnStartMove();

                OnMove();

                // rotate
            
                var lookRotation = Quaternion.LookRotation(tempMoveDirection);
                var euler = lookRotation.eulerAngles;
                visualTran.rotation = Quaternion.Euler(0, euler.y, 0);
            }

            meshAgent.SetDestination(finalMoveDirection);
        }

        private void OnEnter(Collider other)
        {
            var ci = other.GetComponent<ICollisionInteractable>();
            if (ci == null) return;
        
            ci.OnEnter();
        
            if (!other.CompareTag("Item")) _currentCIs.Add(ci);
        
            if (!ci.CanInteract) return;
            if (ci.Condition == CiCondition.None) ci.Interact(this);
        }
    
        private void OnExit(Collider other)
        {
            var ci = other.GetComponent<ICollisionInteractable>();
            if (ci == null) return;
            if (_currentCIs.Contains(ci)) _currentCIs.Remove(ci);
        
            ci.StopInteract(this);
        }

        private void OnStartMove()
        {
            _isMoving = true;

            foreach (var ci in _currentCIs)
            {
                if (ci == null || !ci.CanInteract) break;
                if (ci.Condition == CiCondition.PlayerIsStopped) ci.StopInteract(this);   
            }

            moveCoroutine = StartCoroutine(MovingCor());
        }
    
        private void OnMove() => OnMoveEvent?.Invoke();
    
        private void OnStopMove()
        {
            _isMoving = false;

            foreach (var ci in _currentCIs)
            {
                if (ci == null || !ci.CanInteract) continue;
                if (ci.Condition != CiCondition.PlayerIsStopped) continue;
                ci.Interact(this);

                break;
            }
            
            BreakMoveAnim();
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        }
    
        #endregion
    
        public void RetryInteract()
        {
            foreach (var ci in _currentCIs)
            {
                if (ci == null || !ci.CanInteract) continue;
            
                // conditions
                if (ci.Condition == CiCondition.PlayerIsStopped && _isMoving) continue;
            
                // check for Factory
                if (ci is Factory f && !_transactions.CanStartTransaction(f)) continue;
            
                ci.Interact(this);
                return;
            }
        }

        #region Move

        private IEnumerator MovingCor()
        {
            while (_isMoving)
            {
                AnimateStep();
                yield return new WaitForSeconds(StepTime * 2);
            }
        }

        private void AnimateStep()
        {
            float angle;
            
            _animStepLTID = 
            LeanTween.value(-1, 1, StepTime).setOnUpdate((value) =>
            {
                angle = value * StepAngle;

                legLeft.localRotation = Quaternion.Euler(angle, 0, 0);
                legRight.localRotation = Quaternion.Euler(-angle, 0, 0);
                armLeft.localRotation = Quaternion.Euler(-angle, 0, 0);
                armRight.localRotation = Quaternion.Euler(angle, 0, 0);
                backpack.transform.localRotation = Quaternion.Euler(0, 0, BackpackStepAngle * value);
                
            }).setOnComplete(() =>
            {
                _animStepLTID =
                LeanTween.value(1, -1, StepTime).setOnUpdate((value) =>
                {
                    angle = value * StepAngle;

                    legLeft.localRotation = Quaternion.Euler(angle, 0, 0);
                    legRight.localRotation = Quaternion.Euler(-angle, 0, 0);
                    armLeft.localRotation = Quaternion.Euler(-angle, 0, 0);
                    armRight.localRotation = Quaternion.Euler(angle, 0, 0);
                    backpack.transform.localRotation = Quaternion.Euler(0, 0, BackpackStepAngle * value);
                }).id;
            }).id;
        }

        #endregion
        
        #region Farm

        public void StartFarm(FarmSource source)
        {
            if (_isFarming) return;
        
            BreakFarmAnim();
            _currentSource = source;
            var miningData = source.SourceData.MiningData;
            _mineStrikePeriod = miningData.MiningPeriod;
        
            _isFarming = true;
        
            if (_farmCoroutine != null) StopCoroutine(_farmCoroutine);
            _farmCoroutine = StartCoroutine(FarmingCor());
        }
        
        public void StopFarm(FarmSource rs)
        {
            if (_currentSource != rs) return;
            
            _currentSource = null;
            _isFarming = false;
            BreakFarmAnim();
        
            if (_farmCoroutine != null) StopCoroutine(_farmCoroutine);
        
            RetryInteract();
        }

        private IEnumerator FarmingCor()
        {
            while (_isFarming)
            {
                DoFarmStrike();
                yield return new WaitForSeconds(_mineStrikePeriod);
            }
            yield return null;
        }

        private void DoFarmStrike()
        {
            //BreakFarmAnim();
            toolArmTran.gameObject.SetActive(true);
            armRight.gameObject.SetActive(false);

            var animTimeMultiply = _mineStrikePeriod > DefaultMineStrikeTime ? 1 : (_mineStrikePeriod / DefaultMineStrikeTime * 0.9f);

            _animFarmLTID =
                LeanTween.value(0, 1, DefaultMineStrikeTime * 0.8f * animTimeMultiply).setOnUpdate(
                    (value) =>
                    {
                        toolArmTran.localRotation = Quaternion.Euler(value * -90, 0, 0);
                        visualTran.localScale = new Vector3(1, 1 + value * 0.2f, 1);
                    }).setOnComplete(() =>
                {
                    _animFarmLTID =
                        LeanTween.value(1, 0, DefaultMineStrikeTime * 0.2f * animTimeMultiply).setOnUpdate(
                            (value) =>
                            {
                                toolArmTran.localRotation = Quaternion.Euler(value * -90, 0, 0);
                                visualTran.localScale = new Vector3(1, 1 + value * 0.2f, 1);
                            }).setOnComplete(() => _currentSource.TakeHit()).id;
                
                }).id;
        }
    
        private void BreakFarmAnim()
        {
            if (_animFarmLTID == -1) return;
            LeanTween.cancel(_animFarmLTID);
            visualTran.localScale = Vector3.one;
            toolArmTran.gameObject.SetActive(false);
            armRight.gameObject.SetActive(true);
            toolArmTran.localRotation = Quaternion.Euler(0, 0, 0);
            _animFarmLTID = -1;
        }

        private void BreakMoveAnim()
        {
            if (_animStepLTID == -1) return;
            
            LeanTween.cancel(_animStepLTID);
            
            legLeft.localRotation = Quaternion.Euler(0, 0, 0);
            legRight.localRotation = Quaternion.Euler(0, 0, 0);
            armLeft.localRotation = Quaternion.Euler(0, 0, 0);
            armRight.localRotation = Quaternion.Euler(0, 0, 0);
            backpack.transform.localRotation = Quaternion.Euler(0, 0, 0);

            _animStepLTID = -1;
            
        }
        
        #endregion

        #region Craft

        public void StartCraft(Factory factory)
        {
            _isTransaction = true;
            _currentFactory = factory;
        
            if (_transactionCoroutine != null) StopCoroutine(_transactionCoroutine);
            _transactionCoroutine = StartCoroutine(CraftCor());
        }
    
        public void StopCraft()
        {
            _isTransaction = false;
            if (_transactionCoroutine != null) StopCoroutine(_transactionCoroutine);
            _currentFactory = null;
        
            RetryInteract();
        }
    
        private IEnumerator CraftCor()
        {
            while (_isTransaction)
            {
                if (_currentFactory.CanInteract) _dropToFactoryAction(_currentFactory);
                yield return new WaitForSeconds(_dropToFactoryPeriod);
            }
            yield return null;
        }

        #endregion

        public void OnStartReceiving() {}

        public void OnCompleteReceiving() {}

        public void OnTransactionReact(ItemType itemType, int finalValue)
        {
            if (itemType != ItemType.Wheat) return;

            var maxValue = _farm.ItemsConfig.Items[(int)itemType].MaxValue;
            var fullness = (float)finalValue / (float)(maxValue ?? 1);
            
            if (fullness == 0) backpack.gameObject.SetActive(false);
            else
            {
                backpack.gameObject.SetActive(true);
                backpack.SetFullness(fullness);
            }
        }
    }
}