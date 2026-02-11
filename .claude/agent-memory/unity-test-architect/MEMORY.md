# Incremental Game - Unity Test Architect Memory

## Estructura de tests
- Tests unitarios en `Assets/Programental/Tests/Editor/`
- Assembly definition: `Programental.Tests.Editor.asmdef` (Editor only, referencias vacías porque no hay asmdefs custom)
- Namespace: `Programental.Tests`

## Anti-patterns encontrados en producción

### CodeStructuresTracker (RESUELTO)
- **PlayerPrefs directo en lógica**: Guardado/carga de estado en líneas 101-116 hace tests difíciles
  - ✅ RESUELTO: Se extrajo `CaptureState()/RestoreState()` a SaveManager con Humble Object pattern
  - Ver: GameSaveData.cs, SaveManager.cs (implementado Feb 2026)

### LinesTracker
- **Dependency innecesaria**: Necesita `MilestoneTracker` solo para `CheckMilestones()` — no relevante para tests de CodeStructuresTracker
  - WORKAROUND: Crear MilestonesConfig vacío en Setup
  - MEJOR: Desacoplar con eventos (Observer pattern)

## Convenciones de test confirmadas
- `Assert.That(actual, Is.Expected, "mensaje descriptivo")` obligatorio
- Helper `GivenAvailableLines(count)` para setup común — preferir esto sobre duplicar loops
- ScriptableObjects se crean con `ScriptableObject.CreateInstance<T>()` y se destruyen en TearDown
- **MonoBehaviours en tests**: Usar GameObject + AddComponent<T>(), destruir en TearDown con `Object.DestroyImmediate(gameObject)`
- Namespace separado `Programental.Tests` para evitar colisiones

## Cobertura crítica — Sistema de guardado (CaptureState/RestoreState)

✅ Tests escritos (Feb 2026):
- **LinesTrackerSaveTests** (7 tests): TotalLinesEver, TotalLinesDeleted, fractionalAccumulator, eventos
  - **Crítico**: `fractionalAccumulator` afecta income real del jugador — no perder progreso fraccional entre sesiones
- **MilestoneTrackerSaveTests** (8 tests): nextMilestoneIndex, llamadas a Restore() en rewards
  - **Crítico**: `RestoreState()` debe llamar `Restore()` (no `Unlock()`) en rewards ya conseguidos
- **BaseMultiplierTrackerSaveTests** (8 tests): currentLevel, availableLinesToInvest, UpdateMultiplier(), eventos
  - **Crítico**: `RestoreState()` debe actualizar BonusMultipliers.BaseMultiplier vía `UpdateMultiplier()`

❌ NO testeable sin refactoring:
- **SaveManager**: Usa PlayerPrefs y Time.deltaTime directo — es un Humble Object que orquesta, tests no aportarían valor
- **GoldenCodeManager**: MonoBehaviour con lógica acoplada al ciclo de Unity
- **CodeLineCloneManager**: MonoBehaviour que orquesta clones — lógica pura ya testeada en CodeTyper y CodeStructuresTracker

## Cobertura crítica — CodeStructuresTracker
✅ Tests escritos (20 tests en CodeStructuresTrackerTests.cs):
- TrySpendLines (éxito/fallo)
- Costos exponenciales (2^N)
- Cadena de monedas (Lines → Method → Class → System)
- Abilities (auto_type, multi_key, clone_lines)
- Scaling con flags (abilityScalesWithAvailable true/false)
- Reveal y persistencia (CaptureState/RestoreState incluido)

## Cobertura crítica — CodeTyper (clase plana)
✅ Tests escritos (5 tests en CodeTyperTests.cs):
- OnCharTyped dispara con cada carácter
- OnLineCompleted dispara al finalizar línea
- LinesCompleted incrementa correctamente
- **Múltiples CodeTypers independientes**: Crítico para clone_lines — cada clon produce líneas sin interferir
- Eventos separados entre instancias (test de aislamiento)

## Lecciones de diseño
- **Regla**: Si necesitas `PlayerPrefs.DeleteAll()` en tests, tu diseño tiene un code smell
- **Regla**: Si una clase necesita una dependencia solo para llamar UN método que no afecta su output, esa dependencia debería ser un evento
- **Preferir**: Computed properties (`GetAvailable`, `GetNextCost`) son fáciles de testear — bien hecho
- **Preferir**: Clases planas con DI constructor (LinesTracker, CodeStructuresTracker) vs MonoBehaviours
