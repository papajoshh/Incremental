using DG.Tweening;
using UnityEngine;

namespace Programental
{
    public class IDEScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        public void Show(bool animate = true)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (animate)
                canvasGroup.DOFade(1f, 0.2f);
            else
                canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, 0.2f);
        }
    }
}
