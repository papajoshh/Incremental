using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class PressFeedback: MonoBehaviour
    {
        [Inject] private readonly ContainerShaker _shaker;

        [Header("Game Feel")] 
        [SerializeField] private Transform transformToSquetch;
        [SerializeField] private float hitstopDuration = 0.05f;
        [SerializeField] private Vector3 squashPunch = new Vector3(0.1f, -0.2f, 0);
        [SerializeField] private float squashDuration = 0.2f;

        private Color _originalButtonColor;
        public void Play()
        {
            _shaker.Shake();

            ApplyHitstop();
            ApplySquashStretch();
        }

        private void ApplyHitstop()
        {
            Time.timeScale = 0f;
            DOVirtual.DelayedCall(hitstopDuration, () => Time.timeScale = 1f).SetUpdate(true);
        }

        private void ApplySquashStretch()
        {
            transformToSquetch.transform.DOComplete();
            transformToSquetch.transform.DOPunchScale(squashPunch, squashDuration, 5);
        }
    }
}