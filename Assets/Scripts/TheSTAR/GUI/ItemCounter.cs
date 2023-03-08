using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace TheSTAR.GUI
{
    public class ItemCounter : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private bool smooth = false;

        private int 
        _animScaleLTID = -1,
        _smoothLTID = -1,
        _tempSmoothValue = 0,
        _currentValue = 0;
        
        private int? maxValue = null;
        private const float SmoothTime = 1;
        
        public ItemType ItemType => itemType;
        
        public void Init(Sprite iconSprite, ItemType itemType, int? maxValue = null)
        {
            iconImage.sprite = iconSprite;
            this.itemType = itemType;
            this.maxValue = maxValue;
        }
        
        public void SetValue(int toValue)
        {
            // smooth
            if (smooth)
            {
                if (Math.Abs(toValue - _currentValue) == 1) SetValueToText(toValue);
                else
                {
                    if (_smoothLTID != -1)
                    {
                        LeanTween.cancel(_smoothLTID);
                        _smoothLTID = -1;
                    }
                    
                    _smoothLTID =
                    LeanTween.value(_tempSmoothValue, toValue, SmoothTime).setOnUpdate((value) =>
                    {
                        _tempSmoothValue = (int)value;
                        SetValueToText(_tempSmoothValue);
                    }).id;
                }
            }
            else SetValueToText(toValue);
            
            var needScaleAnimate = toValue > _currentValue;
            _currentValue = toValue;
            
            // animate scale
            if (needScaleAnimate)
            {
                if (_animScaleLTID != -1)
                {
                    LeanTween.cancel(_animScaleLTID);
                    _animScaleLTID = -1;
                }
                
                _animScaleLTID =
                    LeanTween.scale(counterText.gameObject, new Vector3(1.2f, 1.2f, 1), 0.1f).setOnComplete(() =>
                    {
                        _animScaleLTID =
                            LeanTween.scale(counterText.gameObject, Vector3.one, 0.2f).id;
                    }).id;   
            }

            void SetValueToText(int value)
            {
                counterText.text = maxValue == null ? value.ToString() : $"{value}/{maxValue}";
            }
        }
    }
}