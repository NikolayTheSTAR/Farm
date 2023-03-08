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

        private int _animLTID = 0;
        private int _currentValue = 0;
        
        public ItemType ItemType => itemType;

        public void Init(Sprite iconSprite, ItemType itemType)
        {
            iconImage.sprite = iconSprite;
            this.itemType = itemType;
        }
        
        public void SetValue(int value)
        {
            gameObject.SetActive(value > 0);
            
            counterText.text = value.ToString();

            bool needAnimate = value > _currentValue;
            _currentValue = value;

            if (needAnimate)
            {
                if (_animLTID != -1)
                {
                    LeanTween.cancel(_animLTID);
                    _animLTID = -1;
                }
                
                _animLTID =
                LeanTween.scale(counterText.gameObject, new Vector3(1.2f, 1.2f, 1), 0.1f).setOnComplete(() =>
                {
                    _animLTID =
                    LeanTween.scale(counterText.gameObject, Vector3.one, 0.2f).id;
                }).id;
            }
        }
    }
}