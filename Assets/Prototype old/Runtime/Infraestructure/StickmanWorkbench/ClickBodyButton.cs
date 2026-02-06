using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Runtime.Infraestructure
{
    public class ClickBodyButton: MonoBehaviour, IPointerDownHandler
    {
        [Inject] private readonly FirstStickman firstStickman;
        [SerializeField] private PressFeedback _pressFeedback;
        [SerializeField] private Transform _mask;
        [SerializeField] private float maxDistance;

        private Vector3 initialPosition;
        private Tween _tween;
        private void Awake()
        {
            initialPosition = _mask.localPosition;
            StartCoroutine(PressContinuos());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator PressContinuos()
        {
            while (!firstStickman.BodyFullfilled)
            {
                yield return new WaitForSeconds(2);
                if (!firstStickman.BodyFullfilled) Press(false);
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            Press();
        }

        private void Press(bool feedback = true)
        {
            if (firstStickman.BodyFullfilled) return;
            firstStickman.PressBody();
            if(feedback)_pressFeedback.Play();
            _tween.Kill();
            _tween = _mask.DOLocalMoveY(initialPosition.y + firstStickman.PercentageFullfilledBody * maxDistance, 0.5f);
        }
    }
}