using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TypingDefense
{
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] Image fillImage;
        [SerializeField] float holdDuration = 2f;

        float progress;
        bool holding;

        public event Action OnHoldCompleted;

        void Awake()
        {
            fillImage.fillAmount = 0f;
        }

        void Update()
        {
            if (!holding) return;

            progress += Time.deltaTime / holdDuration;
            fillImage.fillAmount = progress;

            if (progress < 1f) return;

            holding = false;
            fillImage.fillAmount = 1f;
            OnHoldCompleted?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            holding = true;
            progress = 0f;
            fillImage.fillAmount = 0f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetHold();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetHold();
        }

        void ResetHold()
        {
            if (!holding) return;
            holding = false;
            progress = 0f;
            fillImage.fillAmount = 0f;
        }
    }
}
