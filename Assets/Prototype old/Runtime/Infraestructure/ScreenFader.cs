using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Runtime.Infraestructure
{
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.3f;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        public Task FadeInOut(Action onBlack)
        {
            var tcs = new TaskCompletionSource<bool>();
            DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, fadeDuration))
                .AppendCallback(() =>
                {
                    canvasGroup.blocksRaycasts = true;
                    onBlack?.Invoke();
                })
                .Append(canvasGroup.DOFade(0f, fadeDuration))
                .AppendCallback(() =>
                {
                    canvasGroup.blocksRaycasts = false;
                    tcs.SetResult(true);
                });
            return tcs.Task;
        }
    }
}
