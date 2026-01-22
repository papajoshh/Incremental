using System;
using System.Collections;
using DG.Tweening;
using Runtime.Infraestructure;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class ClickLeftArmButton : MonoBehaviour, IPointerDownHandler
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
        while (!firstStickman.LeftArmFullfilled)
        {
            yield return new WaitForSeconds(2);
            Press();
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (firstStickman.LeftArmFullfilled) return;
        Press();
    }

    private void Press()
    {
        firstStickman.PressLeftArm();
        _pressFeedback.Play();
        _tween.Kill();
        _tween = _mask.DOLocalMove(initialPosition + firstStickman.PercentageLeftArmFullfilled * maxDistance, 0.5f);
    }
}