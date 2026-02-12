using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Programental.Tests
{
    public class AutoTypePlayModeTests
    {
        private GameObject _go;
        private CodeTyper _codeTyper;
        private BonusMultipliers _bonusMultipliers;
        private CodeStructuresConfig _structuresConfig;
        private CodeTyperMonoBehaviour _typerMono;

        [SetUp]
        public void Setup()
        {
            _codeTyper = new CodeTyper();
            _bonusMultipliers = new BonusMultipliers();
            _structuresConfig = ScriptableObject.CreateInstance<CodeStructuresConfig>();
            _structuresConfig.autoKeystrokesPerSecPerLevel = 10;

            _go = new GameObject("TestTyper");
            _typerMono = _go.AddComponent<CodeTyperMonoBehaviour>();

            Inject(_typerMono, "_codeTyper", _codeTyper);
            Inject(_typerMono, "_bonusMultipliers", _bonusMultipliers);
            Inject(_typerMono, "_structuresConfig", _structuresConfig);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_go);
            Object.DestroyImmediate(_structuresConfig);
        }

        [UnityTest]
        public IEnumerator AutoType_ConNivel1_TecleaCaracteres()
        {
            _bonusMultipliers.AutoTypeLevel = 1;
            var charCount = 0;
            _codeTyper.OnCharTyped += (c, s) => { if (c != '\0') charCount++; };

            yield return null;
            yield return new WaitForSeconds(0.25f);

            Assert.That(charCount, Is.GreaterThan(0), "Auto-type debe haber tecleado al menos un carácter");
        }

        [UnityTest]
        public IEnumerator AutoType_ConNivelCero_NoTeclea()
        {
            _bonusMultipliers.AutoTypeLevel = 0;
            var charCount = 0;
            _codeTyper.OnCharTyped += (c, s) => { if (c != '\0') charCount++; };

            yield return null;
            yield return new WaitForSeconds(0.25f);

            Assert.That(charCount, Is.EqualTo(0), "Sin auto-type no debe teclear nada");
        }

        [UnityTest]
        public IEnumerator AutoType_NivelAlto_TecleaMasRapido()
        {
            _bonusMultipliers.AutoTypeLevel = 5;
            var charCountFast = 0;
            _codeTyper.OnCharTyped += (c, s) => { if (c != '\0') charCountFast++; };

            yield return null;
            yield return new WaitForSeconds(0.25f);

            // Nivel 5: 5 * 10 = 50 keystrokes/sec → ~12 chars en 0.25s
            Assert.That(charCountFast, Is.GreaterThanOrEqualTo(4), "Nivel alto debe teclear más rápido (más keystrokes/sec)");
        }

        [UnityTest]
        public IEnumerator AutoType_UsaCharsPerKeypress()
        {
            _bonusMultipliers.AutoTypeLevel = 1;
            _bonusMultipliers.BaseCharsPerKeypress = 3;
            var charCount = 0;
            _codeTyper.OnCharTyped += (c, s) => { if (c != '\0') charCount++; };

            yield return null;
            yield return new WaitForSeconds(0.25f);

            Assert.That(charCount, Is.GreaterThanOrEqualTo(3), "Cada tick de auto-type debe escribir CharsPerKeypress caracteres");
        }

        private static void Inject(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }
    }
}
