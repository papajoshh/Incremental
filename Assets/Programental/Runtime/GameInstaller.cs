using UnityEngine;
using Zenject;

namespace Programental
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private MilestonesConfig milestonesConfig;
        [SerializeField] private GoldenCodeConfig goldenCodeConfig;
        [SerializeField] private BaseMultiplierConfig baseMultiplierConfig;
        [SerializeField] private CodeStructuresConfig codeStructuresConfig;
        [SerializeField] private SoundLibrary soundLibrary;

        public override void InstallBindings()
        {
            Container.Bind<CodeTyper>().AsSingle();
            Container.BindInstance(milestonesConfig);
            Container.BindInstance(goldenCodeConfig);
            Container.BindInstance(baseMultiplierConfig);
            Container.BindInstance(codeStructuresConfig);
            Container.BindInstance(soundLibrary);
            Container.Bind<BonusMultipliers>().AsSingle();
            Container.Bind<IGoldenCodeBonus>().To<LineMultiplierBonus>().AsSingle();
            Container.Bind<IGoldenCodeBonus>().To<SpeedBonus>().AsSingle();
            Container.Bind<IGoldenCodeBonus>().To<TimeBonus>().AsSingle();
            Container.Bind<MilestoneTracker>().AsSingle();
            Container.Bind<LinesTracker>().AsSingle();
            Container.Bind<BaseMultiplierTracker>().FromMethod(ctx =>
            {
                var config = ctx.Container.Resolve<BaseMultiplierConfig>();
                var bonusMultipliers = ctx.Container.Resolve<BonusMultipliers>();
                return new BaseMultiplierTracker(config.costBase, config.levelIncrement, bonusMultipliers);
            }).AsSingle();
            Container.Bind<CodeStructuresTracker>().AsSingle();
            Container.Bind<CodeTyperMonoBehaviour>().FromComponentInHierarchy().AsSingle();
            Container.Bind<LineCounterView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<DeleteCodeButtonView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GoldenCodeManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IBonusFeedback>().To<BonusFeedbackView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<AudioPlayer>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ScreenShaker>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BaseMultiplierCounterView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CodeStructuresScreenView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TaskbarView>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<SaveManager>().AsSingle();
            Container.Bind<SaveLifecycleHook>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<QaToolMonoBehaviour>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
