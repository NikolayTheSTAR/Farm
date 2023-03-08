using System;
using System.Collections.Generic;
using TheSTAR.GUI.Screens;
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
        [SerializeField] private GameWorldObject testFromObject;

        private const float FlyTime = 1;
        
        private GuiController _gui;

        private List<FlyUIObject> _flyObjectsPool = new List<FlyUIObject>();

        public void Init(GuiController gui)
        {
            _gui = gui;
        }

        [ContextMenu("TestFly")]
        private void TestFly()
        {
            FlyToCounter(testFromObject.transform, ItemType.Coin);
        }

        private void FlyToCounter(Transform from, ItemType itemType, Action completeAction = null)
        {
            StartFlyTo(from, _gui.FindScreen<GameScreen>().GetCounter(itemType).GetComponent<RectTransform>(), completeAction);
        }

        private void StartFlyTo(Transform from, RectTransform rect, Action completeAction = null)
        {
            var currentFlyObject = GetFlyObjectFromPool(Camera.main.WorldToScreenPoint(from.position));
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
                completeAction?.Invoke();
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