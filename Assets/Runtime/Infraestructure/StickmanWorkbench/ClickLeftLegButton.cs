using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Runtime.Infraestructure
{
    public class ClickLeftLegButton: MonoBehaviour, IPointerDownHandler
    {
        [Inject] private readonly FirstStickman firstStickman;
        [SerializeField] private PressFeedback _pressFeedback;
        [SerializeField] private Transform _mask;
        [SerializeField] private Vector3 maxDistance;
        [SerializeField] private Collider2D collider;

        private Vector3 initialPosition;
        private Tween _tween;

        private void Awake()
        {
            initialPosition = _mask.localPosition;
            collider.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (firstStickman.LeftLegFullfilled) return;
            firstStickman.PressLeftLeg();
            _pressFeedback.Play();
            _tween.Kill();
            _tween = _mask.DOLocalMove(initialPosition + firstStickman.PercentageLeftLegFullfilled * maxDistance, 0.5f);
        }
    }
}