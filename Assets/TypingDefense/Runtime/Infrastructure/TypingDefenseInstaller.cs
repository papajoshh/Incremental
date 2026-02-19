using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class TypingDefenseInstaller : MonoInstaller
    {
        [SerializeField] LetterConfig letterConfig;
        [SerializeField] UpgradeGraphConfig upgradeGraphConfig;
        [SerializeField] BossConfig bossConfig;
        [SerializeField] CollectionPhaseConfig collectionPhaseConfig;
        [SerializeField] LevelProgressionConfig levelProgressionConfig;
        [SerializeField] DefenseWordView wordViewPrefab;
        [SerializeField] PhysicalLetter physicalLetterPrefab;
        [SerializeField] BlackHoleController blackHolePrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(letterConfig);
            Container.BindInstance(upgradeGraphConfig);
            Container.BindInstance(bossConfig);
            Container.BindInstance(collectionPhaseConfig);
            Container.BindInstance(levelProgressionConfig);

            Container.BindInterfacesAndSelfTo<WordManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnergyTracker>().AsSingle();
            Container.BindInterfacesAndSelfTo<DefenseSaveManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<CollectionPhaseController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PhysicalLetterSpawner>().AsSingle();
            Container.BindInterfacesAndSelfTo<BossDefeatSequencer>().AsSingle();

            Container.Bind<PlayerStats>().AsSingle();
            Container.Bind<LetterTracker>().AsSingle();
            Container.Bind<UpgradeTracker>().AsSingle();
            Container.Bind<WordPool>().AsSingle();
            Container.Bind<RunManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameFlowController>().AsSingle();
            Container.BindExecutionOrder<DefenseSaveManager>(-10);

            Container.Bind<DefenseSaveLifecycleHook>()
                .FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.Bind<ArenaView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<HudView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<MenuView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<WordViewBridge>().FromComponentInHierarchy().AsSingle();
            Container.Bind<UpgradeGraphView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CameraShaker>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BlackHoleController>().FromComponentInNewPrefab(blackHolePrefab).AsSingle().NonLazy();
            Container.Bind<PostProcessJuiceController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CollectionTutorialView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TypeToSelectView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<TypeToRetreatView>().FromComponentInHierarchy().AsSingle();

            Container.BindFactory<DefenseWordView, DefenseWordView.Factory>()
                .FromComponentInNewPrefab(wordViewPrefab);
            Container.BindFactory<PhysicalLetter, PhysicalLetter.Factory>()
                .FromComponentInNewPrefab(physicalLetterPrefab);
        }
    }
}
