using Runtime.Infraestructure;
using Runtime.Infrastructure;
using UnityEngine;
using Zenject;

namespace Runtime.Main
{
    public class MainInstaller: MonoInstaller
    {
        [SerializeField] private KeyboardTextScoreFeedback feedbackPrefab;
        public override void InstallBindings()
        {
            Container.Bind<BagOfMoñecos>().AsSingle();
            Container.Bind<FirstStickman>().AsSingle();
            
            Container.Bind<ContainerShaker>().FromComponentInHierarchy().AsSingle();
            Container.Bind<AudioPlayer>().AsSingle();
        }
    }
}