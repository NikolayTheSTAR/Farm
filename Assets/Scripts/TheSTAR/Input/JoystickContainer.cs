using System;
using TheSTAR.Utility;
using TheSTAR.Utility.Pointer;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheSTAR.Input
{
    public class JoystickContainer : MonoBehaviour
    {
        [SerializeField] private Pointer pointer;
        [SerializeField] private CanvasGroup joystickCanvasGroup;
        [SerializeField] private GameObject stickObject;
        
        private const float LimitDistance = 120;
        private const float ShowTime = 0.1f;

        private Action<Vector2> _joystickInputAction;
        private bool _isDown = false;
        private int _showHideLTID = -1;

        public void Init(Action<Vector2> joystickInoutAction)
        {
            pointer.InitPointer(
                (eventData) => OnJoystickDown(), 
                (eventData) => OnJoystickDrag(),
                (eventData) => OnJoystickUp());

            _joystickInputAction = joystickInoutAction;
        }

        private void Update()
        {
            JoystickInput();
        }

        private void OnJoystickDown()
        {
            _isDown = true;
            ShowJoystick();
            UpdateStickPosByMouse();
        }
    
        private void OnJoystickDrag()
        {
            _isDown = true;
            UpdateStickPosByMouse();
        }
    
        private void OnJoystickUp()
        {
            _isDown = false;
            HideJoystick();
        }

        private void UpdateStickPosByMouse()
        {
            stickObject.transform.position = UnityEngine.Input.mousePosition;
            stickObject.transform.localPosition = MathUtility.LimitForCircle(stickObject.transform.localPosition, LimitDistance);
        }

        private void JoystickInput()
        {
            if (_isDown) _joystickInputAction?.Invoke(stickObject.transform.localPosition / LimitDistance);
            else _joystickInputAction?.Invoke(Vector2.zero);
        }

        #region Show/Hide

        private void ShowJoystick()
        {
            if (_showHideLTID != -1) LeanTween.cancel(_showHideLTID);
            
            joystickCanvasGroup.transform.position = UnityEngine.Input.mousePosition;
            joystickCanvasGroup.gameObject.SetActive(true);
            joystickCanvasGroup.alpha = 0;
            
            _showHideLTID =
            LeanTween.alphaCanvas(joystickCanvasGroup, 1, ShowTime).setOnComplete(() =>
            {
                _showHideLTID = -1;
            }).id;
        }

        private void HideJoystick()
        {
            if (_showHideLTID != -1) LeanTween.cancel(_showHideLTID);

            _showHideLTID =
            LeanTween.alphaCanvas(joystickCanvasGroup, 0, ShowTime).setOnComplete(() =>
            {
                _showHideLTID = -1;
                joystickCanvasGroup.gameObject.SetActive(false);
            }).id;
        }

        #endregion
    }
}