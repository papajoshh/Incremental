using NUnit.Framework;

namespace Programental.Tests
{
    public class BaseMultiplierTrackerSaveTests
    {
        private const float CostBase = 2f;
        private const float LevelIncrement = 0.1f;
        private BonusMultipliers _bonusMultipliers;

        [SetUp]
        public void Setup()
        {
            _bonusMultipliers = new BonusMultipliers();
        }

        [Test]
        public void CaptureState_ConEstadoInicial_DevuelveCeros()
        {
            var tracker = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);

            var data = tracker.CaptureState();

            Assert.That(data.currentLevel, Is.EqualTo(0), "Nivel inicial debe ser 0");
            Assert.That(data.availableLinesToInvest, Is.EqualTo(0), "Sin líneas invertidas inicialmente");
        }

        [Test]
        public void CaptureState_ConNivelYLineasPendientes_CapturaTodo()
        {
            var tracker = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);
            tracker.AddDeletedLines(5);

            var data = tracker.CaptureState();

            Assert.That(data.currentLevel, Is.EqualTo(1), "Nivel 1 cuesta 2 (2^1), nivel 2 cuesta 4 (2^2). 5 >= 2 pero 3 < 4, solo alcanza nivel 1");
            Assert.That(data.availableLinesToInvest, Is.EqualTo(3), "5 - 2 = 3 lineas sobrantes");
        }

        [Test]
        public void RestoreState_ConEstadoPrevio_RestableceNivelYLineas()
        {
            var tracker1 = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);
            tracker1.AddDeletedLines(10);
            var captured = tracker1.CaptureState();

            var tracker2 = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);
            tracker2.RestoreState(captured);

            Assert.That(tracker2.CurrentLevel, Is.EqualTo(captured.currentLevel), "Debe restaurar currentLevel");
            Assert.That(tracker2.AvailableLinesToInvest, Is.EqualTo(captured.availableLinesToInvest), "Debe restaurar availableLinesToInvest");
        }

        [Test]
        public void RestoreState_ActualizaBonusMultipliers()
        {
            var tracker1 = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);
            tracker1.AddDeletedLines(10);
            var captured = tracker1.CaptureState();

            var bonusMultipliers2 = new BonusMultipliers();
            var tracker2 = new BaseMultiplierTracker(CostBase, LevelIncrement, bonusMultipliers2);
            tracker2.RestoreState(captured);

            var expectedMultiplier = 1f + captured.currentLevel * LevelIncrement;
            Assert.That(bonusMultipliers2.BaseMultiplier, Is.EqualTo(expectedMultiplier), "RestoreState debe actualizar BaseMultiplier vía UpdateMultiplier()");
        }

        [Test]
        public void RestoreState_DisparaEventoOnMultiplierChanged()
        {
            var tracker1 = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);
            tracker1.AddDeletedLines(10);
            var captured = tracker1.CaptureState();

            var tracker2 = new BaseMultiplierTracker(CostBase, LevelIncrement, new BonusMultipliers());
            var eventFired = false;
            tracker2.OnMultiplierChanged += () => eventFired = true;

            tracker2.RestoreState(captured);

            Assert.That(eventFired, Is.True, "RestoreState debe disparar OnMultiplierChanged");
        }

        [Test]
        public void RestoreState_ConLineasSobrantes_PermiteInvertirMas()
        {
            var tracker1 = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);
            tracker1.AddDeletedLines(5);
            var captured = tracker1.CaptureState();

            var tracker2 = new BaseMultiplierTracker(CostBase, LevelIncrement, new BonusMultipliers());
            tracker2.RestoreState(captured);
            tracker2.AddDeletedLines(3);

            Assert.That(tracker2.CurrentLevel, Is.EqualTo(2), "Restore nivel 1 (avail 3) + 3 nuevas = 6 avail, nivel 2 cuesta 4 (2^2), 6 >= 4");
            Assert.That(tracker2.AvailableLinesToInvest, Is.EqualTo(2), "6 - 4 = 2 lineas sobrantes tras subir a nivel 2");
        }

        [Test]
        public void CaptureRestore_PreservaEstadoComplejo()
        {
            var tracker1 = new BaseMultiplierTracker(CostBase, LevelIncrement, _bonusMultipliers);
            tracker1.AddDeletedLines(7);
            tracker1.AddDeletedLines(3);
            var level1 = tracker1.CurrentLevel;
            var available1 = tracker1.AvailableLinesToInvest;
            var captured = tracker1.CaptureState();

            var tracker2 = new BaseMultiplierTracker(CostBase, LevelIncrement, new BonusMultipliers());
            tracker2.RestoreState(captured);

            Assert.That(tracker2.CurrentLevel, Is.EqualTo(level1), "Nivel debe preservarse exactamente");
            Assert.That(tracker2.AvailableLinesToInvest, Is.EqualTo(available1), "Líneas sobrantes deben preservarse exactamente");
            Assert.That(tracker2.CurrentMultiplier, Is.EqualTo(1f + level1 * LevelIncrement), "Multiplicador debe recalcularse correctamente");
        }
    }
}
