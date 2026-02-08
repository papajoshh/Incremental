using UnityEngine;
using Zenject;

namespace Programental
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private MilestonesConfig milestonesConfig;
        [SerializeField] private GoldenCodeConfig goldenCodeConfig;
        [SerializeField] private SoundLibrary soundLibrary;

        public override void InstallBindings()
        {
            Container.Bind<CodeTyper>().AsSingle();
            Container.BindInstance(milestonesConfig);
            Container.BindInstance(goldenCodeConfig);
            Container.BindInstance(soundLibrary);
            Container.Bind<BonusMultipliers>().AsSingle();
            Container.Bind<MilestoneTracker>().AsSingle();
            Container.Bind<LinesTracker>().AsSingle();
            Container.Bind<CodeTyperMonoBehaviour>().FromComponentInHierarchy().AsSingle();
            Container.Bind<LineCounterView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<DeleteCodeButtonView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GoldenCodeManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<AudioPlayer>().FromComponentInHierarchy().AsSingle();
        }
    }
}
