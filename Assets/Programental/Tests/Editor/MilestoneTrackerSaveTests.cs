using NUnit.Framework;
using UnityEngine;

namespace Programental.Tests
{
    public class MilestoneTrackerSaveTests
    {
        private MilestonesConfig _config;
        private GameObject _gameObject;
        private TestReward _reward1;
        private TestReward _reward2;
        private TestReward _reward3;

        [SetUp]
        public void Setup()
        {
            _config = ScriptableObject.CreateInstance<MilestonesConfig>();
            _config.milestones = new[]
            {
                new Milestone { linesRequired = 10, rewardId = "reward1" },
                new Milestone { linesRequired = 50, rewardId = "reward2" },
                new Milestone { linesRequired = 100, rewardId = "reward3" }
            };

            _gameObject = new GameObject("TestRewards");
            _reward1 = _gameObject.AddComponent<TestReward>();
            _reward1.SetRewardId("reward1");
            _reward2 = _gameObject.AddComponent<TestReward>();
            _reward2.SetRewardId("reward2");
            _reward3 = _gameObject.AddComponent<TestReward>();
            _reward3.SetRewardId("reward3");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void CaptureState_SinMilestonesDesbloqueados_DevuelveIndice0()
        {
            var tracker = new MilestoneTracker(_config);

            var data = tracker.CaptureState();

            Assert.That(data.nextMilestoneIndex, Is.EqualTo(0), "Sin milestones desbloqueados, índice debe ser 0");
        }

        [Test]
        public void CaptureState_ConUnMilestoneDesbloqueado_DevuelveIndice1()
        {
            var tracker = new MilestoneTracker(_config);
            tracker.Register("reward1", _reward1);
            tracker.CheckMilestones(15);

            var data = tracker.CaptureState();

            Assert.That(data.nextMilestoneIndex, Is.EqualTo(1), "Desbloqueado milestone 0, siguiente debe ser índice 1");
        }

        [Test]
        public void CaptureState_ConTodosMilestonesDesbloqueados_DevuelveIndiceFinal()
        {
            var tracker = new MilestoneTracker(_config);
            tracker.Register("reward1", _reward1);
            tracker.Register("reward2", _reward2);
            tracker.Register("reward3", _reward3);
            tracker.CheckMilestones(1000);

            var data = tracker.CaptureState();

            Assert.That(data.nextMilestoneIndex, Is.EqualTo(3), "Todos los milestones desbloqueados, índice debe ser 3 (longitud del array)");
        }

        [Test]
        public void RestoreState_ConUnMilestoneDesbloqueado_LlamaRestoreEnReward()
        {
            var tracker1 = new MilestoneTracker(_config);
            tracker1.Register("reward1", _reward1);
            tracker1.CheckMilestones(15);
            var captured = tracker1.CaptureState();

            var freshReward = _gameObject.AddComponent<TestReward>();
            freshReward.SetRewardId("reward1");
            var tracker2 = new MilestoneTracker(_config);
            tracker2.Register("reward1", freshReward);
            tracker2.RestoreState(captured);

            Assert.That(freshReward.RestoreCalled, Is.True, "RestoreState debe llamar Restore() en rewards desbloqueados");
            Assert.That(freshReward.UnlockCalled, Is.False, "RestoreState NO debe llamar Unlock(), solo Restore()");
        }

        [Test]
        public void RestoreState_ConVariosRewards_LlamaRestoreEnTodos()
        {
            var tracker1 = new MilestoneTracker(_config);
            tracker1.Register("reward1", _reward1);
            tracker1.Register("reward2", _reward2);
            tracker1.Register("reward3", _reward3);
            tracker1.CheckMilestones(75);
            var captured = tracker1.CaptureState();

            var tracker2 = new MilestoneTracker(_config);
            tracker2.Register("reward1", _reward1);
            tracker2.Register("reward2", _reward2);
            tracker2.Register("reward3", _reward3);
            tracker2.RestoreState(captured);

            Assert.That(_reward1.RestoreCalled, Is.True, "Milestone 0 (10 líneas) debe llamar Restore()");
            Assert.That(_reward2.RestoreCalled, Is.True, "Milestone 1 (50 líneas) debe llamar Restore()");
            Assert.That(_reward3.RestoreCalled, Is.False, "Milestone 2 (100 líneas) NO alcanzado, no debe llamar Restore()");
        }

        [Test]
        public void RestoreState_NoDesbloquea_MilestonesYaCompletados()
        {
            var tracker1 = new MilestoneTracker(_config);
            tracker1.Register("reward1", _reward1);
            tracker1.CheckMilestones(15);
            var captured = tracker1.CaptureState();

            var freshReward = _gameObject.AddComponent<TestReward>();
            freshReward.SetRewardId("reward1");
            var tracker2 = new MilestoneTracker(_config);
            tracker2.Register("reward1", freshReward);
            tracker2.RestoreState(captured);
            tracker2.CheckMilestones(20);

            Assert.That(freshReward.UnlockCalled, Is.False, "CheckMilestones después de RestoreState NO debe volver a desbloquear milestone ya conseguido");
        }

        [Test]
        public void RestoreState_ConRewardNoRegistrado_NoFalla()
        {
            var tracker1 = new MilestoneTracker(_config);
            tracker1.Register("reward1", _reward1);
            tracker1.CheckMilestones(15);
            var captured = tracker1.CaptureState();

            var tracker2 = new MilestoneTracker(_config);

            Assert.DoesNotThrow(() => tracker2.RestoreState(captured), "RestoreState no debe fallar si un reward no está registrado");
        }

        [Test]
        public void RestoreState_YCheckMilestones_SoloDesbloqueaNuevos()
        {
            var tracker1 = new MilestoneTracker(_config);
            tracker1.Register("reward1", _reward1);
            tracker1.CheckMilestones(15);
            var captured = tracker1.CaptureState();

            var freshReward1 = _gameObject.AddComponent<TestReward>();
            freshReward1.SetRewardId("reward1");
            var freshReward2 = _gameObject.AddComponent<TestReward>();
            freshReward2.SetRewardId("reward2");
            var tracker2 = new MilestoneTracker(_config);
            tracker2.Register("reward1", freshReward1);
            tracker2.Register("reward2", freshReward2);
            tracker2.RestoreState(captured);
            tracker2.CheckMilestones(75);

            Assert.That(freshReward1.RestoreCalled, Is.True, "reward1 debe haberse restaurado");
            Assert.That(freshReward1.UnlockCalled, Is.False, "reward1 NO debe Unlock() de nuevo");
            Assert.That(freshReward2.RestoreCalled, Is.False, "reward2 no se habia desbloqueado antes del save");
            Assert.That(freshReward2.UnlockCalled, Is.True, "reward2 debe Unlock() porque ahora si se alcanza con 75 lineas");
        }

        private class TestReward : MilestoneReward
        {
            private string _rewardId;
            public bool UnlockCalled { get; private set; }
            public bool RestoreCalled { get; private set; }

            public override string RewardId => _rewardId;

            public void SetRewardId(string id) => _rewardId = id;

            public override void OnUnlock() => UnlockCalled = true;
            public override void Restore()
            {
                base.Restore();
                RestoreCalled = true;
            }
        }
    }
}
