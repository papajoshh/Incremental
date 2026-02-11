using Programental;
using Runtime.Application;
using Runtime.Infraestructure;
using UnityEngine;
using Zenject;
using SaveManager = Programental.SaveManager;

namespace Runtime.Main
{
    public class MainInstaller: MonoInstaller
    {
        [SerializeField] private GameObject moñecoPrefab;
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
            Container.Bind<RepairableComputerGameObject>().FromComponentInHierarchy().AsSingle();
            Container.Bind<MoñecoInstantiator>().AsSingle().WithArguments(moñecoPrefab);
            Container.Bind<MoñecosSaveHandler>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BagOfMoñecosCanvas>().FromComponentInHierarchy().AsSingle();

            // Fases de skip en orden — el orden de Bind es el orden de ejecución
            Container.Bind<ISkippable>().To<StickmanWorkbench>().FromResolve();
            Container.Bind<ISkippable>().FromInstance(new SkippableAction(() => Container.Resolve<SalaDeCargaPrincipalMonoBehaviour>().SkipAdd2Moñecos()));
            Container.Bind<ISkippable>().FromInstance(new SkippableAction(() => Container.Resolve<SalaDeCargaPrincipalMonoBehaviour>().SkipFillAllMachines()));
            Container.Bind<ISkippable>().To<RepairableComputerGameObject>().FromResolve();
        }
    }
}