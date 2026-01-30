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
            Container.Bind<ScreenFader>().FromComponentInHierarchy().AsSingle();
            Container.Bind<AudioPlayer>().AsSingle();
            Container.Bind<SaveManager>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<StickmanWorkbench>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SalaDeCargaPrincipalMonoBehaviour>().FromComponentInHierarchy().AsSingle();
        }
    }
}