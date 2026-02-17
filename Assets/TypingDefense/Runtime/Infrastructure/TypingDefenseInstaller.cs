using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class TypingDefenseInstaller : MonoInstaller
    {
        [SerializeField] WordSpawnConfig wordSpawnConfig;
        [SerializeField] RunConfig runConfig;
        [SerializeField] LetterConfig letterConfig;
        [SerializeField] UpgradeGraphConfig upgradeGraphConfig;
        [SerializeField] BossConfig bossConfig;
        [SerializeField] ConverterConfig converterConfig;
        [SerializeField] DefenseWordView wordViewPrefab;
        [SerializeField] BossWordView bossViewPrefab;
        [SerializeField] ConverterLetterView converterLetterPrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(wordSpawnConfig);
            Container.BindInstance(runConfig);
            Container.BindInstance(letterConfig);
            Container.BindInstance(upgradeGraphConfig);
            Container.BindInstance(bossConfig);
            Container.BindInstance(converterConfig);

            Container.BindInterfacesAndSelfTo<WordManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnergyTracker>().AsSingle();
            Container.BindInterfacesAndSelfTo<DefenseSaveManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ConverterManager>().AsSingle();

            Container.Bind<PlayerStats>().AsSingle();
            Container.Bind<LetterTracker>().AsSingle();
            Container.Bind<UpgradeTracker>().AsSingle();
            Container.Bind<WordPool>().AsSingle();
            Container.Bind<RunManager>().AsSingle();
            Container.Bind<GameFlowController>().AsSingle();

            Container.Bind<DefenseSaveLifecycleHook>()
                .FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<ArenaView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<HudView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<MenuView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<WordViewBridge>().FromComponentInHierarchy().AsSingle();
            Container.Bind<UpgradeGraphView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ConverterView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CameraShaker>().FromComponentInHierarchy().AsSingle();

            Container.BindFactory<DefenseWordView, DefenseWordView.Factory>()
                .FromComponentInNewPrefab(wordViewPrefab);
            Container.BindFactory<BossWordView, BossWordView.Factory>()
                .FromComponentInNewPrefab(bossViewPrefab);
            Container.BindFactory<ConverterLetterView, ConverterLetterView.Factory>()
                .FromComponentInNewPrefab(converterLetterPrefab);
        }
    }
}
