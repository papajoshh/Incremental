using Runtime.Infraestructure;
using UnityEngine;
using Zenject;

namespace Runtime.Main
{
    public class MainInstaller: MonoInstaller
    {
        [SerializeField] private KeyboardTextScoreFeedback feedbackPrefab;
        public override void InstallBindings()
        {
            Container.Bind<Keyboard>().AsSingle();
            Container.BindFactory<KeyboardTextScoreFeedback, KeyboardTextScoreFeedback.KeyboardScoreFeedbackFactory>()
                .FromComponentInNewPrefab(feedbackPrefab);
        }
    }
}