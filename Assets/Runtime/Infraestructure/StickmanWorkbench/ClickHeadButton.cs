using System;
using System.Collections;
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
        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void StartToFill()
        {
            StartCoroutine(PressContinuos());
            collider.enabled = true;
        }
        private IEnumerator PressContinuos()
        {
            while (!firstStickman.HeadFullfilled)
            {
                yield return new WaitForSeconds(2);
                if (!firstStickman.HeadFullfilled) Press(false);
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (firstStickman.HeadFullfilled) return;
            Press();
        }

        private void Press(bool feedback = true)
        {
            firstStickman.PressHead();
            if(feedback)_pressFeedback.Play();
            _tween.Kill();
            _tween = _mask.DOScale(firstStickman.PercentageHeadFullfilled , 0.5f);
        }
    }
}