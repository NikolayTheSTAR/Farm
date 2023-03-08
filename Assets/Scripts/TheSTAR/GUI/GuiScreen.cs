using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using TheSTAR.Utility;

namespace TheSTAR.GUI
{
    public class GuiScreen : MonoBehaviour, IComparable<GuiScreen>, IComparableType<GuiScreen>
    {
        private bool isShow = false;

        public bool IsShow => isShow;

        public async void Show(Action endAction = null, bool skipShowAnim = false)
        {
            isShow = true;
            gameObject.SetActive(true);

            OnShow();

            if (!skipShowAnim)
            {
                AnimateShow(out int hideTime);
                await Task.Delay(hideTime);
            }
            
            endAction?.Invoke();
        }

        protected virtual void OnShow()
        {
        }

        [ContextMenu("Hide")]
        private void TestHide()
        {
            Hide();
        }

        public async void Hide(Action endAction = null)
        {
            AnimateHide(out int hideTime);

            await Task.Delay(hideTime);

            gameObject.SetActive(false);
            OnHide();
            isShow = false;

            endAction?.Invoke();
        }

        protected virtual void OnHide()
        {
        }


        protected virtual void AnimateShow(out int showTime)
        {
            showTime = 0;
        }

        protected virtual void AnimateHide(out int hideTime)
        {
            hideTime = 0;
        }

        public virtual void Reset()
        {
        }

        public override string ToString()
        {
            return GetType().ToString();
        }

        public int CompareTo(GuiScreen other)
        {
            return ToString().CompareTo(other.ToString());
        }

        public int CompareToType<T>() where T : GuiScreen
        {
            return ToString().CompareTo(typeof(T).ToString());
        }
    }
}