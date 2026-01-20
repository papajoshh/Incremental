using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Runtime.Infraestructure
{
    public class ClickHeadButton: MonoBehaviour, IPointerDownHandler
    {
        [Inject] private readonly FirstStickman firstStickman;
        [SerializeField] private PressFeedback _pressFeedback;
        [SerializeField] private Transform _mask;
        [SerializeField] private Collider2D collider;

        private Tween _tween;

        private void Awake()
        {
            collider.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (firstStickman.HeadFullfilled) return;
            firstStickman.PressHead();
            _pressFeedback.Play();
            _tween.Kill();
            _tween = _mask.DOScale(firstStickman.PercentageHeadFullfilled , 0.5f);
        }
    }
}