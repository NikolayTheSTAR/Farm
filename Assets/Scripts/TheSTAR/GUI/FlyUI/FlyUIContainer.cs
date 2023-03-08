using System;
using System.Collections.Generic;
using TheSTAR.GUI.Screens;
using TheSTAR.World;
using TheSTAR.World.Farm;
using UnityEngine;
using UnityEngine.Serialization;
using World;

namespace TheSTAR.GUI.FlyUI
{
    public class FlyUIContainer : MonoBehaviour
    {
        [SerializeField] private FlyUIObject flyObjectPrefab;
        [SerializeField] private AnimationCurve speedCurve;
        [SerializeField] private AnimationCurve scaleCurve;

        private const float FlyTime = 1;
        
        private GuiController _gui;
        private TransactionsController _transactions;

        private List<FlyUIObject> _flyObjectsPool = new List<FlyUIObject>();

        public void Init(GuiController gui, TransactionsController transactions)
        {
            _gui = gui;
            _transactions = transactions;
        }
        
        public void FlyToCounter(IDropSender from, ItemType itemType, int value)
        {
            StartFlyTo(from, _gui.FindScreen<GameScreen>().GetCounter(itemType).GetComponent<RectTransform>(), itemType, value);
        }
        
        private void StartFlyTo(IDropSender sender, RectTransform rect, ItemType itemType, int value)
        {
            var currentFlyObject = GetFlyObjectFromPool(Camera.main.WorldToScreenPoint(sender.startSendPos.position));
            var startPos = currentFlyObject.transform.position;
            var distance = rect.position - startPos;

            LeanTween.value(0, 1, FlyTime).setOnUpdate((value) =>
            {
                currentFlyObject.transform.position = startPos + distance * speedCurve.Evaluate(value);
                currentFlyObject.transform.localScale =
                    new Vector3(scaleCurve.Evaluate(value), scaleCurve.Evaluate(value), 1);
            }) .setOnComplete(() =>
            {
                currentFlyObject.gameObject.SetActive(false);
                _transactions.AddItem(itemType, value);
                sender.OnCompleteDrop();
            });
        }

        private FlyUIObject GetFlyObjectFromPool(Vector3 startPos)
        {
            FlyUIObject result = null;
            result = _flyObjectsPool.Find(info => !info.gameObject.activeSelf);

            if (result == null)
            {
                result = Instantiate(flyObjectPrefab, startPos, Quaternion.identity, transform);
                _flyObjectsPool.Add(result);
            }
            else
            {
                result.transform.position = startPos;
                result.gameObject.SetActive(true);
            }

            return result;
        }
    }
}