using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Runtime.Infraestructure
{
    public class ClickRightArmButton : MonoBehaviour, IPointerDownHandler
    {
        [Inject] private readonly FirstStickman firstStickman;
        [SerializeField] private PressFeedback _pressFeedback;
        [SerializeField] private Transform _mask;
        [SerializeField] private Vector3 maxDistance;
        
        private Vector3 initialPosition;
        private Tween _tween;

        private void Awake()
        {
            initialPosition = _mask.localPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (firstStickman.RightArmFullfilled) return;
            firstStickman.PressRightArm();
            _pressFeedback.Play();
            _tween.Kill();
            _tween = _mask.DOLocalMove(initialPosition + firstStickman.PercentageRightArmFullfilled * maxDistance,
                0.5f);
        }
    }
}