using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Mining;
using TheSTAR.Input;
using UnityEngine;
using UnityEngine.AI;
using World;

public class Player : GameWorldObject, ICameraFocusable, IJoystickControlled, IDropReceiver
{
    [SerializeField] private NavMeshAgent meshAgent;
    [SerializeField] private EntranceTrigger trigger;
    [SerializeField] private Transform visualTran;
    [SerializeField] private Transform toolArmTran;
    [SerializeField] private GameObject toolObject;

    private float 
        _mineStrikePeriod = 1,
        _dropToFactoryPeriod = 1;
    
    private bool 
        _isMoving = false, 
        _isMining = false, 
        _isTransaction = false;

    private TransactionsController _transactions;
    private List<ICollisionInteractable> _currentCIs;
    private FarmSource _currentSource;
    private Factory _currentFactory;
    private Coroutine _mineCoroutine;
    private Coroutine _transactionCoroutine;
    private int _animLTID = -1;

    private Action<Factory> _dropToFactoryAction;
    
    public event Action OnMoveEvent;

    private const float DefaultMineStrikeTime = 0.5f;
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

    public void Init(TransactionsController transactions, Action<Factory> dropToFactoryAction, float dropToFactoryPeriod)
    {
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
        
        _currentCIs.Add(ci);
        
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
            if (ci == null || !ci.CanInteract) return;
            if (ci.Condition == CiCondition.PlayerIsStopped) ci.StopInteract(this);   
        }
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

            return;
        }
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
    
    #region Farm

    public void StartFarm(FarmSource source)
    {
        if (_isMining) return;
        
        BreakAnim();
        _currentSource = source;
        var miningData = source.SourceData.MiningData;
        _mineStrikePeriod = miningData.MiningPeriod;
        
        _isMining = true;
        
        if (_mineCoroutine != null) StopCoroutine(_mineCoroutine);
        _mineCoroutine = StartCoroutine(FarmingCor());
    }
        
    public void StopFarm(FarmSource rs)
    {
        if (_currentSource != rs) return;
            
        _currentSource = null;
        _isMining = false;
        BreakAnim();
        
        if (_mineCoroutine != null) StopCoroutine(_mineCoroutine);
        
        RetryInteract();
    }

    private IEnumerator FarmingCor()
    {
        while (_isMining)
        {
            DoFarmStrike();
            yield return new WaitForSeconds(_mineStrikePeriod);
        }
        yield return null;
    }

    private void DoFarmStrike()
    {
        BreakAnim();
        toolObject.SetActive(true);

        var animTimeMultiply = _mineStrikePeriod > DefaultMineStrikeTime ? 1 : (_mineStrikePeriod / DefaultMineStrikeTime * 0.9f);

        _animLTID =
            LeanTween.value(visualTran.gameObject, 0, 1, DefaultMineStrikeTime * 0.8f * animTimeMultiply).setOnUpdate(
            (value) =>
            {
                toolArmTran.localRotation = Quaternion.Euler(value * -90, 0, 0);
                visualTran.localScale = new Vector3(1, 1 + value * 0.2f, 1);
            }).setOnComplete(() =>
            {
                _animLTID =
                LeanTween.value(visualTran.gameObject, 1, 0, DefaultMineStrikeTime * 0.2f * animTimeMultiply).setOnUpdate(
                (value) =>
                {
                    toolArmTran.localRotation = Quaternion.Euler(value * -90, 0, 0);
                    visualTran.localScale = new Vector3(1, 1 + value * 0.2f, 1);
                }).setOnComplete(() => _currentSource.TakeHit()).id;
                
            }).id;
    }
    
    private void BreakAnim()
    {
        if (_animLTID == -1) return;
        LeanTween.cancel(_animLTID);
        visualTran.localScale = Vector3.one;
        toolObject.SetActive(false);
        toolArmTran.localRotation = Quaternion.Euler(0, 0, 0);
        _animLTID = -1;
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
}