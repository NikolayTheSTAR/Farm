using UnityEngine;
using UnityEngine.Serialization;

namespace Tutorial
{
    public class TutorialArrow : MonoBehaviour
    {
        [SerializeField] private Transform visualTran;

        [SerializeField] private float animOffset = 0.7f;
        [SerializeField] private float animTime = 1f;
        
        [SerializeField] private AnimationCurve animCurve;

        private int _animLTID = -1;
        
        public void PointTo(Transform from, Transform to)
        {
            var direction = Vector3.Normalize(to.position - from.position) * 3;
            transform.position = from.position + new Vector3(direction.x, 0, direction.z);
            
            var lookRotation = Quaternion.LookRotation(direction);
            var euler = lookRotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, euler.y, 0);
        }

        public void StartAnim()
        {
            AnimateArrow();
        }

        private void AnimateArrow()
        {
            TryBreakAnim();

            _animLTID =
            LeanTween.value(0, 1, animTime).setOnUpdate((value) =>
            {
                visualTran.localPosition = new Vector3(0, 0, animCurve.Evaluate(value) * animOffset);
            }).setOnComplete(AnimateArrow).id;
        }

        public void StopAnim()
        {
            TryBreakAnim();
        }

        private void TryBreakAnim()
        {
            if (_animLTID == -1) return;
            LeanTween.cancel(_animLTID);
            
            visualTran.localPosition = Vector3.zero;
        }
    }
}