# Incremental Game - Unity Test Architect Memory

## Estructura de tests
- Tests unitarios en `Assets/Programental/Tests/Editor/`
- Assembly definition: `Programental.Tests.Editor.asmdef` (Editor only, referencias vacías porque no hay asmdefs custom)
- Namespace: `Programental.Tests`

## Anti-patterns encontrados en producción

### CodeStructuresTracker
- **PlayerPrefs directo en lógica**: Guardado/carga de estado en líneas 101-116 hace tests difíciles
  - WORKAROUND: `PlayerPrefs.DeleteAll()` en SetUp/TearDown funciona pero es frágil
  - MEJOR: Extraer persistencia a interface inyectable (Humble Object)

### LinesTracker
- **Dependency innecesaria**: Necesita `MilestoneTracker` solo para `CheckMilestones()` — no relevante para tests de CodeStructuresTracker
  - WORKAROUND: Crear MilestonesConfig vacío en Setup
  - MEJOR: Desacoplar con eventos (Observer pattern)

## Convenciones de test confirmadas
- `Assert.That(actual, Is.Expected, "mensaje descriptivo")` obligatorio
- Helper `GivenAvailableLines(count)` para setup común — preferir esto sobre duplicar loops
- ScriptableObjects se crean con `ScriptableObject.CreateInstance<T>()` y se destruyen en TearDown
- Namespace separado `Programental.Tests` para evitar colisiones

## Cobertura crítica para CodeStructuresTracker
✅ Tests escritos (17 tests):
- TrySpendLines (éxito/fallo)
- Costos exponenciales (2^N)
- Cadena de monedas (Lines → Method → Class)
- Abilities (auto_type, multi_key)
- Scaling con flags (abilityScalesWithAvailable true/false)
- Reveal y persistencia

❌ NO testeado (y probablemente no vale la pena):
- PlayerPrefs internals (SaveState/LoadState) — test de persistencia cubre el comportamiento observable
- OnStructureChanged event ordering — frágil, implementación detail
- Casos de borde con >2 tiers — sin config real que lo use

## Lecciones de diseño
- **Regla**: Si necesitas `PlayerPrefs.DeleteAll()` en tests, tu diseño tiene un code smell
- **Regla**: Si una clase necesita una dependencia solo para llamar UN método que no afecta su output, esa dependencia debería ser un evento
- **Preferir**: Computed properties (`GetAvailable`, `GetNextCost`) son fáciles de testear — bien hecho
- **Preferir**: Clases planas con DI constructor (LinesTracker, CodeStructuresTracker) vs MonoBehaviours
