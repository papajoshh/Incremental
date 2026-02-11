using NUnit.Framework;
using UnityEngine;

namespace Programental.Tests
{
    public class LinesTrackerSaveTests
    {
        private MilestonesConfig _milestonesConfig;
        private BonusMultipliers _bonusMultipliers;
        private MilestoneTracker _milestoneTracker;

        [SetUp]
        public void Setup()
        {
            _milestonesConfig = ScriptableObject.CreateInstance<MilestonesConfig>();
            _milestonesConfig.milestones = new Milestone[0];
            _bonusMultipliers = new BonusMultipliers();
            _milestoneTracker = new MilestoneTracker(_milestonesConfig);
            _bonusMultipliers.BaseMultiplier = 1f;
            _bonusMultipliers.TemporaryLineMultiplier = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_milestonesConfig);
        }

        [Test]
        public void CaptureState_ConEstadoCompleto_DevuelveDataCorrecta()
        {
            var tracker = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker.AddCompletedLine();
            tracker.AddCompletedLine();
            tracker.AddCompletedLine();
            tracker.TrySpendLines(1);

            var data = tracker.CaptureState();

            Assert.That(data.totalLinesEver, Is.EqualTo(3), "Debe capturar TotalLinesEver");
            Assert.That(data.totalLinesDeleted, Is.EqualTo(1), "Debe capturar TotalLinesDeleted");
            Assert.That(data.fractionalAccumulator, Is.EqualTo(0f), "Debe capturar fractionalAccumulator");
        }

        [Test]
        public void CaptureState_ConAcumuladorFraccional_PreservaProgreso()
        {
            _bonusMultipliers.TemporaryLineMultiplier = 1.5f;
            var tracker = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker.AddCompletedLine();

            var data = tracker.CaptureState();

            Assert.That(data.totalLinesEver, Is.EqualTo(1), "Debe ganarse 1 línea entera (1.5 → floor = 1)");
            Assert.That(data.fractionalAccumulator, Is.EqualTo(0.5f), "Debe preservar 0.5 fraccional de 1.5 - 1 = 0.5");
        }

        [Test]
        public void RestoreState_ConEstadoPrevio_RestableceCompletamente()
        {
            var tracker1 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker1.AddCompletedLine();
            tracker1.AddCompletedLine();
            tracker1.AddCompletedLine();
            tracker1.TrySpendLines(1);
            var captured = tracker1.CaptureState();

            var tracker2 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker2.RestoreState(captured);

            Assert.That(tracker2.TotalLinesEver, Is.EqualTo(3), "Debe restaurar TotalLinesEver");
            Assert.That(tracker2.TotalLinesDeleted, Is.EqualTo(1), "Debe restaurar TotalLinesDeleted");
            Assert.That(tracker2.AvailableLines, Is.EqualTo(2), "AvailableLines debe derivarse correctamente (3 - 1)");
        }

        [Test]
        public void RestoreState_ConAcumuladorFraccional_MantieneProgreso()
        {
            _bonusMultipliers.TemporaryLineMultiplier = 1.5f;
            var tracker1 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker1.AddCompletedLine();
            var captured = tracker1.CaptureState();

            _bonusMultipliers.TemporaryLineMultiplier = 1.5f;
            var tracker2 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker2.RestoreState(captured);
            tracker2.AddCompletedLine();

            Assert.That(tracker2.TotalLinesEver, Is.EqualTo(3), "0.5 (cargado) + 1.5 (nueva línea) = 2.0 → se ganan 2 líneas, total 1+2=3");
        }

        [Test]
        public void RestoreState_DisparaEventoOnAvailableLinesChanged()
        {
            var tracker1 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker1.AddCompletedLine();
            tracker1.AddCompletedLine();
            var captured = tracker1.CaptureState();

            var tracker2 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            var eventFired = false;
            var reportedLines = 0;
            tracker2.OnAvailableLinesChanged += lines =>
            {
                eventFired = true;
                reportedLines = lines;
            };

            tracker2.RestoreState(captured);

            Assert.That(eventFired, Is.True, "Debe disparar OnAvailableLinesChanged al restaurar estado");
            Assert.That(reportedLines, Is.EqualTo(2), "Debe reportar las líneas correctas");
        }

        [Test]
        public void CaptureRestore_SinModificar_PreservaEstadoVacio()
        {
            var tracker1 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            var captured = tracker1.CaptureState();

            var tracker2 = new LinesTracker(_milestoneTracker, _bonusMultipliers);
            tracker2.RestoreState(captured);

            Assert.That(tracker2.TotalLinesEver, Is.EqualTo(0), "Estado vacío debe preservarse");
            Assert.That(tracker2.TotalLinesDeleted, Is.EqualTo(0), "Estado vacío debe preservarse");
            Assert.That(tracker2.AvailableLines, Is.EqualTo(0), "Estado vacío debe preservarse");
        }
    }
}
