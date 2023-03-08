using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;
using UnityEngine.Serialization;

namespace TheSTAR.GUI
{
    public sealed class GuiController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private GuiScreen[] screens = new GuiScreen[0];
        [SerializeField] private GuiScreen mainScreen;
        [SerializeField] private bool deactivateOtherScreensByStart = true;
        [SerializeField] private bool showMainScreenByStart = true;

        #endregion // Inspector

        private GuiScreen _currentScreen;

        public void Init(out List<ITransactionReactable> transactionReactableScreens)
        {
            transactionReactableScreens = new List<ITransactionReactable>();
            
            if (deactivateOtherScreensByStart)
            {
                foreach (var screen in screens)
                {
                    if (screen == null) continue;
                    if (screen.gameObject.activeSelf) screen.gameObject.SetActive(false);
                    if (screen is ITransactionReactable t) transactionReactableScreens.Add(t);
                }
            }

            if (showMainScreenByStart && mainScreen != null) Show(mainScreen, false);
        }

        public void Show<TScreen>(bool closeCurrentScreen = false, Action endAction = null) where TScreen : GuiScreen
        {
            GuiScreen screen = FindScreen<TScreen>();
            Show(screen, closeCurrentScreen, endAction);
        }

        public void Show<TScreen>(TScreen screen, bool closeCurrentScreen = false, Action endAction = null, bool skipShowAnim = false) where TScreen : GuiScreen
        {
            if (!screen) return;
            if (closeCurrentScreen && _currentScreen) _currentScreen.Hide();
            
            screen.Show(endAction, skipShowAnim);
            _currentScreen = screen;
        }

        public T FindScreen<T>() where T : GuiScreen
        {
            int index = ArrayUtility.FastFindElement<GuiScreen, T>(screens);

            if (index == -1)
            {
                Debug.LogError($"Not found screen {typeof(T)}");
                return null;
            }
            else return (T)(screens[index]);
        }

        [ContextMenu("SortScreens")]
        private void SortScreens()
        {
            System.Array.Sort(screens);
        }
    }
}