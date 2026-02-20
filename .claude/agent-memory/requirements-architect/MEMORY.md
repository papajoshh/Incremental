# Requirements Architect - Memory

## Proyecto: Programental / TypingDefense

Juego de defensa por tipeo. Dos módulos principales coexisten en el mismo repo:
- `Assets/Programental/` - El juego incremental (idle/clicker)
- `Assets/TypingDefense/` - El juego de defensa (el foco actual)

## Arquitectura TypingDefense (confirmada leyendo código)

### Capas
- `Config/` - ScriptableObjects (UpgradeGraphConfig, LevelProgressionConfig, BossConfig, CollectionPhaseConfig)
- `Core/` - Lógica de negocio pura (WordManager, DefenseWord, WordPool)
- `Economy/` - Trackers (LetterTracker, UpgradeTracker, PlayerStats, UpgradeId enum)
- `Run/` - Estado de partida (RunManager, EnergyTracker)
- `Infrastructure/` - GameFlowController, SaveManager, Installer
- `Views/` - MonoBehaviours (ArenaView, BlackHoleController, DefenseWordView, WordViewBridge, CameraShaker)

### Patrones establecidos
- `WordManager` (clase plana, ITickable) - gestiona DefenseWord (POCOs) y emite eventos
- `WordViewBridge` (MonoBehaviour) - escucha eventos de WordManager y crea/destruye vistas via Factory
- `DefenseWordView.Factory` - PlaceholderFactory Zenject para instanciar prefabs de palabras
- `UpgradeId` (enum) - todos los upgrades pasan por este enum + PlayerStats.ApplyUpgrade()
- Upgrades se guardan como `UpgradeSaveEntry[]` en `DefenseSaveData`

### ArenaView (crítico para Wall System)
- `ClampToInterior(Vector3)` - clampea posición al interior (arenaWidth/2 - edgeMargin)
- `GetRandomEdgePosition()` - devuelve posición aleatoria en el borde
- `GetRandomInteriorPosition()` - devuelve posición aleatoria interior
- NO hay cámara dinámica - es estática actualmente

### BlackHoleController
- Llama `_arenaView.ClampToInterior(newPos)` en cada frame (línea 192)
- Esta es la línea que hay que hacer dinámica para el Wall System

### CameraShaker
- Tiene `cameraTransform` y `_rigTransform` (padre de la cámara)
- El "pan" se hace moviendo el rig parent, el shake en el hijo
- Para camera follow, hay que agregar lógica al rig o crear un sistema separado

### SaveData
- `DefenseSaveData` - JSON plano en PlayerPrefs
- Agregar campos nuevos es backward-compatible (JsonUtility ignora campos extra)
- `UpgradeSaveEntry[]` para el upgrade tree

## Patrones de upgrade (confirmado)
- `UpgradeId` enum → `PlayerStats.ApplyUpgrade(id, value)` → efecto
- Nodo en `UpgradeGraphConfig` tiene upgradeId + valuesPerLevel
- NO hay efectos "trigger-based" en upgrades - todos son stat mutations
- Para upgrades que disparan comportamiento (reveal walls) necesitaremos nuevo patrón

## Ficheros relevantes para Wall System
- `ArenaView.cs` - modificar ClampToInterior para bounds dinámicos
- `BlackHoleController.cs` - usa ArenaView.ClampToInterior, necesita actualización
- `UpgradeId.cs` - agregar RevealWallWords_Layer0, RevealWallWords_Layer1, etc.
- `PlayerStats.cs` - agregar campos para wall upgrades
- `DefenseSaveData.cs` - agregar WallSegmentStates[]
- `TypingDefenseInstaller.cs` - registrar nuevos sistemas
- `WordViewBridge.cs` - posible extensión para blue words

## Ver detalle en
- `architecture.md` - estructura de ficheros con rutas absolutas
