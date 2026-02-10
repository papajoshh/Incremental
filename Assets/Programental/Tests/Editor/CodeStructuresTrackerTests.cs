using NUnit.Framework;
using UnityEngine;

namespace Programental.Tests
{
    public class CodeStructuresTrackerTests
    {
        private MilestonesConfig _milestonesConfig;
        private CodeStructuresConfig _structuresConfig;
        private MilestoneTracker _milestoneTracker;
        private BonusMultipliers _bonusMultipliers;
        private LinesTracker _linesTracker;
        private CodeStructuresTracker _tracker;

        [SetUp]
        public void Setup()
        {
            PlayerPrefs.DeleteAll();

            _milestonesConfig = ScriptableObject.CreateInstance<MilestonesConfig>();
            _milestonesConfig.milestones = new Milestone[0];

            _structuresConfig = ScriptableObject.CreateInstance<CodeStructuresConfig>();
            _structuresConfig.abilityScalesWithAvailable = false;
            _structuresConfig.structures = new[]
            {
                new StructureDefinition
                {
                    id = "method",
                    displayName = "Method",
                    costBase = 2f,
                    abilityId = "auto_type"
                },
                new StructureDefinition
                {
                    id = "class",
                    displayName = "Class",
                    costBase = 2f,
                    abilityId = "multi_key"
                }
            };

            _bonusMultipliers = new BonusMultipliers();
            _milestoneTracker = new MilestoneTracker(_milestonesConfig);
            _linesTracker = new LinesTracker(_milestoneTracker, _bonusMultipliers);

            _bonusMultipliers.BaseMultiplier = 1f;
            _bonusMultipliers.TemporaryLineMultiplier = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteAll();
            Object.DestroyImmediate(_milestonesConfig);
            Object.DestroyImmediate(_structuresConfig);
        }

        [Test]
        public void TrySpendLines_ConSuficientesLineas_GastaYDisparaEvento()
        {
            GivenAvailableLines(10);
            var eventFired = false;
            var availableAfterSpend = 0;
            _linesTracker.OnAvailableLinesChanged += (available) =>
            {
                eventFired = true;
                availableAfterSpend = available;
            };

            var result = _linesTracker.TrySpendLines(5);

            Assert.That(result, Is.True, "TrySpendLines debe devolver true cuando hay suficientes líneas");
            Assert.That(_linesTracker.AvailableLines, Is.EqualTo(5), "Debe gastar 5 líneas de las 10 disponibles");
            Assert.That(eventFired, Is.True, "Debe disparar OnAvailableLinesChanged");
            Assert.That(availableAfterSpend, Is.EqualTo(5), "El evento debe reportar las líneas correctas");
        }

        [Test]
        public void TrySpendLines_SinSuficientesLineas_FallaYNoModificaNada()
        {
            GivenAvailableLines(3);
            var eventFired = false;
            _linesTracker.OnAvailableLinesChanged += _ => eventFired = true;

            var result = _linesTracker.TrySpendLines(5);

            Assert.That(result, Is.False, "TrySpendLines debe devolver false cuando no hay suficientes líneas");
            Assert.That(_linesTracker.AvailableLines, Is.EqualTo(3), "No debe modificar las líneas disponibles");
            Assert.That(eventFired, Is.False, "No debe disparar evento si falla");
        }

        [Test]
        public void TryPurchase_MethodNivel1_Gasta2Lineas()
        {
            GivenAvailableLines(10);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);

            var result = _tracker.TryPurchase(0);

            Assert.That(result, Is.True, "Debe comprar exitosamente Method nivel 1");
            Assert.That(_tracker.GetLevel(0), Is.EqualTo(1), "Method debe estar en nivel 1");
            Assert.That(_linesTracker.AvailableLines, Is.EqualTo(8), "Debe gastar 2 líneas (2^1)");
        }

        [Test]
        public void TryPurchase_MethodNivel2_Gasta4Lineas()
        {
            GivenAvailableLines(20);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);

            var result = _tracker.TryPurchase(0);

            Assert.That(result, Is.True, "Debe comprar exitosamente Method nivel 2");
            Assert.That(_tracker.GetLevel(0), Is.EqualTo(2), "Method debe estar en nivel 2");
            Assert.That(_linesTracker.AvailableLines, Is.EqualTo(14), "Debe gastar 2 + 4 = 6 líneas total");
        }

        [Test]
        public void TryPurchase_ClassNivel1_GastaMethodAvailable()
        {
            GivenAvailableLines(20);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            var methodAvailableBefore = _tracker.GetAvailable(0);

            var result = _tracker.TryPurchase(1);

            Assert.That(result, Is.True, "Debe comprar exitosamente Class nivel 1");
            Assert.That(_tracker.GetLevel(1), Is.EqualTo(1), "Class debe estar en nivel 1");
            Assert.That(_tracker.GetAvailable(0), Is.EqualTo(methodAvailableBefore - 2), "Debe gastar 2 de Method.Available (2^1)");
            Assert.That(_linesTracker.AvailableLines, Is.EqualTo(14), "No debe gastar líneas directamente");
        }

        [Test]
        public void AutoTypeAbility_ConFlagFalse_EscalaConLevel()
        {
            _structuresConfig.abilityScalesWithAvailable = false;
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);

            Assert.That(_bonusMultipliers.AutoTypeCount, Is.EqualTo(2), "auto_type debe ser Level (2) cuando flag=false");
        }

        [Test]
        public void AutoTypeAbility_ConFlagTrue_EscalaConAvailable()
        {
            _structuresConfig.abilityScalesWithAvailable = true;
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            var methodLevel = _tracker.GetLevel(0);
            var methodAvailable = _tracker.GetAvailable(0);

            _tracker.TryPurchase(1);

            Assert.That(methodLevel, Is.EqualTo(3), "Method debe estar en nivel 3");
            Assert.That(_bonusMultipliers.AutoTypeCount, Is.EqualTo(methodAvailable - 2), "auto_type debe ser Available después de gastar en Class");
        }

        [Test]
        public void AutoTypeAbility_ConFlagTrue_PierdePotenciaAlGastar()
        {
            _structuresConfig.abilityScalesWithAvailable = true;
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            var autoTypeBeforeSpend = _bonusMultipliers.AutoTypeCount;

            _tracker.TryPurchase(1);
            var autoTypeAfterSpend = _bonusMultipliers.AutoTypeCount;

            Assert.That(autoTypeBeforeSpend, Is.EqualTo(3), "Antes de gastar: auto_type = Available = Level = 3");
            Assert.That(autoTypeAfterSpend, Is.LessThan(autoTypeBeforeSpend), "Después de gastar en Class, auto_type debe bajar");
            Assert.That(autoTypeAfterSpend, Is.EqualTo(1), "auto_type = 3 - 2 (cost Class nivel 1) = 1");
        }

        [Test]
        public void MultiKeyAbility_ModificaBaseCharsPerKeypress()
        {
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            var initialBase = _bonusMultipliers.BaseCharsPerKeypress;

            _tracker.TryPurchase(1);

            Assert.That(_bonusMultipliers.BaseCharsPerKeypress, Is.EqualTo(2), "multi_key nivel 1 debe setear BaseCharsPerKeypress = 1 + 1");
            Assert.That(_bonusMultipliers.CharsPerKeypress, Is.EqualTo(2), "CharsPerKeypress = BaseCharsPerKeypress + BonusCharsPerKeypress");
        }

        [Test]
        public void MultiKeyAbility_Nivel2_ModificaCorrectamente()
        {
            GivenAvailableLines(200);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            for (var i = 0; i < 6; i++) _tracker.TryPurchase(0);
            _tracker.TryPurchase(1);

            _tracker.TryPurchase(1);

            Assert.That(_bonusMultipliers.BaseCharsPerKeypress, Is.EqualTo(3), "multi_key nivel 2 debe setear BaseCharsPerKeypress = 1 + 2");
        }

        [Test]
        public void TryPurchase_SinMonedaSuficiente_FallaYNoModificaNada()
        {
            GivenAvailableLines(1);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);

            var result = _tracker.TryPurchase(0);

            Assert.That(result, Is.False, "No debe poder comprar Method nivel 1 con solo 1 línea (cuesta 2)");
            Assert.That(_tracker.GetLevel(0), Is.EqualTo(0), "Method debe seguir en nivel 0");
            Assert.That(_linesTracker.AvailableLines, Is.EqualTo(1), "No debe gastar líneas si falla");
        }

        [Test]
        public void TryPurchase_ClassSinMethodAvailable_Falla()
        {
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);

            var result = _tracker.TryPurchase(1);

            Assert.That(result, Is.False, "No debe poder comprar Class nivel 1 con solo 1 Method.Available (cuesta 2)");
            Assert.That(_tracker.GetLevel(1), Is.EqualTo(0), "Class debe seguir en nivel 0");
            Assert.That(_tracker.GetAvailable(0), Is.EqualTo(1), "Method.Available no debe cambiar");
        }

        [Test]
        public void ComprarClass_ReAplicaAbilityDeTierAnterior()
        {
            _structuresConfig.abilityScalesWithAvailable = true;
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            _tracker.TryPurchase(0);
            var autoTypeBeforeClassPurchase = _bonusMultipliers.AutoTypeCount;

            var eventFiredForMethod = false;
            _tracker.OnStructureChanged += index =>
            {
                if (index == 0) eventFiredForMethod = true;
            };

            _tracker.TryPurchase(1);

            Assert.That(eventFiredForMethod, Is.True, "Debe disparar OnStructureChanged para Method (tier anterior)");
            Assert.That(_bonusMultipliers.AutoTypeCount, Is.LessThan(autoTypeBeforeClassPurchase), "auto_type debe reducirse porque Method.Available bajó");
        }

        [Test]
        public void GetNextCost_EscalaCorrectamente()
        {
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);

            var cost0 = _tracker.GetNextCost(0);
            _tracker.TryPurchase(0);
            var cost1 = _tracker.GetNextCost(0);
            _tracker.TryPurchase(0);
            var cost2 = _tracker.GetNextCost(0);

            Assert.That(cost0, Is.EqualTo(2), "Nivel 0 → Nivel 1: 2^1 = 2");
            Assert.That(cost1, Is.EqualTo(4), "Nivel 1 → Nivel 2: 2^2 = 4");
            Assert.That(cost2, Is.EqualTo(8), "Nivel 2 → Nivel 3: 2^3 = 8");
        }

        [Test]
        public void IsRevealed_SePoneATrueAlComprar()
        {
            GivenAvailableLines(100);
            _tracker = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);

            Assert.That(_tracker.IsRevealed(0), Is.False, "Method debe estar oculto inicialmente");

            _tracker.TryPurchase(0);

            Assert.That(_tracker.IsRevealed(0), Is.True, "Method debe revelarse después de comprar");
        }

        [Test]
        public void Constructor_PersistenciaDePlayerPrefs()
        {
            GivenAvailableLines(100);
            var tracker1 = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);
            tracker1.TryPurchase(0);
            tracker1.TryPurchase(0);
            var level = tracker1.GetLevel(0);

            var tracker2 = new CodeStructuresTracker(_structuresConfig, _linesTracker, _bonusMultipliers);

            Assert.That(tracker2.GetLevel(0), Is.EqualTo(level), "El nivel debe persistir entre instancias via PlayerPrefs");
            Assert.That(tracker2.IsRevealed(0), Is.True, "El revealed debe persistir");
        }

        private void GivenAvailableLines(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _linesTracker.AddCompletedLine();
            }
        }
    }
}
