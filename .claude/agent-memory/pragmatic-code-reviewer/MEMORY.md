# Programental - Code Review Memory

## Architecture
- Zenject DI, plain classes for logic, MonoBehaviours for views
- MilestoneReward (abstract MonoBehaviour): RewardId + auto-register via [Inject] method
- ScriptableObjects for config (MilestonesConfig, GoldenCodeConfig, BaseMultiplierConfig)
- DOTween for all animations/juice
- BonusMultipliers: shared mutable state bag for multipliers (BaseMultiplier, CharsPerKeypress, GoldenCodeTimeBonus)
- LinesTracker: centralizes line counting (TotalLinesEver, TotalLinesDeleted, AvailableLines)
- GameInstaller: single MonoInstaller in scene, binds everything
- BaseMultiplierTracker: writes directly to BonusMultipliers.BaseMultiplier, no abstraction layer
- IGoldenCodeBonus: 3 impls (LineMultiplier, Speed, Time) - justified because Apply/Revert have real logic variation

## Key Patterns
- Views subscribe in OnEnable, unsubscribe in OnDisable
- Rewards: OnUnlock() for first-time effect, Restore() for reload
- PlayerPrefs for persistence scattered across trackers (migration to centralized SaveManager pending)
- Save system decision: direct SaveManager (no ISaveable interface) — 5 known classes, all Zenject-injected
- Each tracker gets a typed Restore() method, SaveManager reads public props directly
- GameSaveData: single flat DTO + StructureStateData[] for array state
- MilestoneTracker._nextMilestoneIndex is recalculable from TotalLinesEver, no persistence needed
- Prototype old/ has ISaveable pattern (FindObjectsOfType-based) — NOT suitable for current arch
- IGoldenCodeBonus interface: Apply()/Revert() pattern for temporary bonuses
- Event-driven UI updates, NOT polling in Update (BaseMultiplierCounterView pattern)
- Trackers modify BonusMultipliers directly, no abstraction needed

## Known Issues / Watch For
- BonusMultipliers.BaseMultiplier has ownership conflict: both BaseMultiplierTracker (permanent) and LineMultiplierBonus (temporary) write to it. Fixed with TemporaryLineMultiplier split.
- PlayerPrefs.Save() called in tight loops (BaseMultiplierTracker.AutoInvestLines)
- SpeedBonus uses "- 1" magic number assuming BaseCharsPerKeypress=1, will break with structures

## Conventions (from CLAUDE.md)
- var always, guard clauses, zero defensive programming
- Juice/screenshake on everything player-facing (Vlambeer style)
- No I prefix on interfaces (IGoldenCodeBonus is legacy exception)
- No XML doc comments unless requested

## Review Decisions Log
- IStructureAbility rejected: 2 impls each 1 line, switch is sufficient. Refactor when >3 complex impls.
- CheckReveals in Update rejected: project uses events, not polling
- Feature flags in prototypes rejected: hardcode decision, iterate later
- BonusMultipliers at ~9 props acceptable for incremental genre
- TrySpendLines if-check is business logic, not defensive programming
- ISaveable interface rejected for save system: only 5 known classes, all compile-time, interface adds indirection without benefit
- RestoreOrder rejected: explicit call order in SaveManager.Load() is simpler and debuggable
- ITickable auto-save rejected: save on-change or InvokeRepeating is sufficient for incremental

## TypingDefense Subproject
- Path: Assets/TypingDefense/Runtime/
- Installer: TypingDefenseInstaller (MonoInstaller), separate from Programental's GameInstaller
- PlayerStats: mutable bag, ResetToBase() + ApplyUpgrade(UpgradeId) switch, ~40 upgrade cases (ECO/OFF/DEF/SUR/UTI + DMG/BDMG/BEHP + CONV_*)
- WordManager: ITickable, 11 events (added Boss*, WordTextChanged), handles spawning/input/matching/crits/warp/auto-type/boss
- DefenseWord: HP system (MaxHp, CurrentHp, TakeDamage), ChangeText for multi-hit words, IsBoss flag
- WordViewBridge: bridges WordManager events to DefenseWordView + BossWordView
- GameFlowController: LazyInject to break circular deps, orchestrates state transitions
- GameState enum: Menu, Playing, GameOver, Paused (Converting removed -- converter is a menu-phase activity, not a game state)
- UpgradeId enum: type-safe, ~40 values (added DMG, BDMG, BEHP, CONV_SPEED/SIZE/AUTO/EXTRA)
- UpgradeGraphConfig: DAG with nodeId strings, lazy cached lookup + parentMap, InvalidateCache() for editor
- UpgradeTracker: fog of war (_revealedNodes), parent validation, CaptureState/RestoreState with UpgradeSaveEntry[]
- ConverterManager: ITickable, BlackHole physics, letter suction/collection, converts letters to coins
- ConverterView: manages BlackHole + ConverterLetterView lifecycle, uses Factory pattern
- BossConfig: bossLevel, bossHp, orbitalSpeed/Radius, prestigeReward
- ConverterConfig: level arrays for speed/size/autoMove/extraHoles, suction/collect radius
- DefenseSaveManager: IInitializable + ITickable, auto-save 5s, PlayerPrefs JSON
- UpgradeGraphEditorWindow: visual DAG editor, drag nodes, ctrl+click connections, inspector panel (~350 lines)

### TypingDefense Review Decisions
- ConverterStats rejected: use PlayerStats for all player stats (combat + converter)
- UpgradeGraphEditorWindow: implemented and compact, justified now that graph is a DAG (not linear tiers)
- Boss HP logic IN WordManager: boss is a DefenseWord with IsBoss=true, ApplyDamageToWord handles both
- DefenseWord: HP is NOT boss-only anymore (normal words get HP scaling with level)
- BossWordView justified: orbital vs linear movement is fundamental difference
- Graph nodeId as string OK for topology, keep UpgradeId enum for effects (type safety)
- ConverterManager split: logic (ITickable) + view (MonoBehaviour), same as WordManager/WordViewBridge
- ApplyUpgrade(id, level=1): default param for backwards compat with 30 legacy upgrades
- RunManager.PrestigeCurrency: currently in RunManager, should move to own tracker (different lifecycle)

### TypingDefense Known Issues (from review)
- ConverterManager depends on ArenaView (view in logic class) -- should receive center position as param
- ConverterManager has Input.GetKey + Camera.main in logic class -- move to ConverterView or InputHandler
- ConverterView subscribes in Construct but SetActive(false) -- events fire when inactive (bug)
- ConverterView calls StartConverting (orchestration in view) -- move to GameFlowController
- LetterTracker.RemoveLetter fires N events for bulk clear -- needs ClearLetters() method
- Magic number 5 for LetterType count scattered in 5+ files
- GetSize() formula duplicated in ConverterView and ConverterManager
- Subscription pattern inconsistent: ConverterView/HudView/MenuView use Construct+OnDestroy, UpgradeGraphView uses OnEnable+OnDisable

### TypingDefense Review Decisions (Feb 2026 - Tabs/Retreat/Juice)
- GameState.Converting REMOVED: converter is a menu activity, not a game state. ConverterManager uses _isConverting bool
- GameFlowController.OnReturnedFromRun event added (replaces SetState(Converting))
- HandleConvertingComplete() removed from GameFlowController
- ScreenShakeService rejected: merge subs into CameraShaker MonoBehaviour (Construct+OnDestroy for cleanup)
- MenuTab enum lives inside MenuView (2 values, impl detail)
- MenuView controls tab panels via [SerializeField] not Zenject injection of ConverterView
- HoldButton: standalone MonoBehaviour (~30 lines) with IPointerDown/Up/ExitHandler, reusable
- Juice values hardcoded OK for prototype, move to SO when stabilized
- ConverterView.CleanUp null checks flagged as defensive programming -- remove them

### TypingDefense Review Decisions (Feb 2026 - UpgradeGraph Redesign)
- 5 visual states -> 3 (Max, Available, Locked). purchased+affordable vs unpurchased+affordable indistinguishable to player
- Node colors as [SerializeField] on prefab, NOT static readonly in code (consistent with positionScale pattern)
- Initialize() params grouped into NodeDisplayData readonly struct (avoids 8+ param method)
- Tooltip managed by node directly (IPointerEnter/Exit), NOT via callback to GraphView parent
- Only 1 callback needed: OnClicked(nodeId). Hover handled locally by node.
- Pan/zoom inline in UpgradeGraphView (~30 lines), no PanZoomController extraction (1 consumer)
- ConnectionLine as readonly struct internal to UpgradeGraphView (Image + FromId + ToId)
- No NodeState enum for 3 states, if-else on IsMaxLevel/CanAfford sufficient
- UpgradeNode needs Sprite icon field added to UpgradeGraphConfig.cs
- Refresh diferencial: nodes persist, Refresh(data) updates visual. No destroy/recreate.
- DOComplete() before every new tween (established pattern in HudView)

### TypingDefense Review Decisions (Feb 2026 - Collection Phase)
- GameState.Collecting added to enum (real game state, unlike Converting)
- CollectionPhaseController: ITickable, timer + slowmo + charge sequence orchestration
- PhysicalLetterSpawner: IInitializable+IDisposable, subs to WordManager kill events, spawns via Factory
- BlackHoleController: distance-check collection (no triggers), 9 deps (flag: consider splitting)
- PhysicalLetter: static Active list OK (pool/tracker pattern), but static config state NOT OK

### TypingDefense Review Decisions (Feb 2026 - Juice Effects Review)
- PostProcessJuiceController: null checks on PP settings violate zero-defensive-programming rule. Decide if effects are optional or mandatory.
- PostProcessJuiceController: DOTween.To on PP values must be killed before starting new ones (use SetTarget+DOKill pattern)
- BlackHoleController: CollectionPhaseController injected but never used -- dead dependency, remove
- BlackHoleController: null checks on SerializeFields (trail, ambientParticles) and on Zenject deps in OnDestroy -- remove
- DOTween SetUpdate(true) needed everywhere during collection phase (Time.timeScale manipulated) -- correct pattern
- Scale punch with collection streak (Lerp 0.08-0.25 over 20 hits) is good game feel escalation
- Micro-shake every 5 collections reinforces momentum without annoyance
- Game over implosion sequence (expand 1.6x -> crush to 0 + 720deg spin) is solid juice
- CollectionPhaseController.OnFreezeReleased event justified: PP ramp needs to start AFTER freeze delay, not on state change

### TypingDefense Review Decisions (Feb 2026 - ZoomCharge/PP Buildup)
- ZoomCharge 5 float params -> ZoomChargeParams readonly struct (CameraShaker stays decoupled from CollectionPhaseConfig)
- InsertCallback loop of 6 discrete shakes REJECTED: shakes DOComplete each other (bug), single continuous DOShakePosition instead
- OnChargeStarted(float) event REJECTED: PP subscribes to existing OnCollectionStarted, controls its own ramp duration locally
- PostProcessJuiceController single class for all PP effects CORRECT: splitting would cause 3 classes fighting over same 4 PP params
- TweenPP private helper to deduplicate 4x DOTween.To per method in PostProcessJuiceController
- CameraShaker needs Camera ref + _baseOrthographicSize for ZoomCharge (missing in current impl)
- CollectionPhaseConfig Headers for inspector UX grouping
- CameraShaker.Shake uiContainer null check flagged: decide if optional or mandatory, don't silently skip

### TypingDefense Review Decisions (Feb 2026 - BH Features)
- Static AttractionTarget/AttractionSpeed REJECTED: pass via Setup params from PhysicalLetterSpawner
- BlackHoleController injected into CollectionPhaseController REJECTED: pass BH position as StartCollection param via GameFlowController
- BH movable during Playing APPROVED: separate CheckLetterCollection (Collecting-only) from MoveWithArrowKeys (both states)
- CollectionSpeed + LetterAttraction: add to PlayerStats + UpgradeId + BaseStatsData (standard upgrade pattern)
- letterDriftDelay: if in config, USE it from config. Don't hardcode a value that exists in SO.
- CameraShaker.ZoomCharge targetPosition param: APPROVED, pan+zoom is valid juice
- InverseTransformPoint for camera target: APPROVED (CameraShaker uses localPosition, needs conversion)
- PhysicalLetter.Setup null checks on SerializeFields: remove (zero defensive programming rule)

### TypingDefense Review Decisions (Feb 2026 - Boss Level System)
- LevelProgressionConfig: single SO with LevelConfig[] APPROVED. Absorb WordSpawnConfig + BossConfig + RunConfig into it (no half-empty configs)
- BossDefeatSequencer as plain class REJECTED: extend BossWordView.OnDefeated() + orchestrate from GameFlowController. View juice stays in views.
- GameState.Farming APPROVED as enum value (real game state, not a bool). Words spawn, no kill bar, no energy drain.
- TypeToRetreatView via OnInputError REJECTED: fragile (most chars match active words). Put retreat matcher inline in WordManager.ProcessInput (~15 lines)
- TypeToSelectView APPROVED: MonoBehaviour reading Input.inputString in Menu state. Level names must be short+unique.
- RunConfig ELIMINATED: baseMaxHp/baseMaxEnergy go to BaseStatsConfig or PlayerStats base
- DefeatedBossLevels bool[] REJECTED: HighestUnlockedLevel int is sufficient (sequential unlock = YAGNI)
- WordManager at 8 deps + growing responsibilities: monitor, split spawn/input if >400 lines post-changes
- AutoCollect post-boss: define mechanism (PhysicalLetter AutoCollect mode vs full CollectionPhase reuse) before implementing

### TypingDefense Review Decisions (Feb 2026 - Breakable Wall System)
- InputDispatcher REJECTED: over-engineered for 2 char consumers. WallManager reads Input.inputString itself or subscribes to WordManager.OnInputError for unmatched chars
- WallSegmentId readonly struct REJECTED: flat int index, derive ring/side with math if needed
- BlueWordSpawner REJECTED: merge spawn timer into WallManager (~30 lines, bounded complexity)
- CameraFollower REJECTED: add FollowTarget(Vector3) to CameraShaker, call from BlackHoleController.Update
- DefenseWord.IsBlue REJECTED: view concern, not domain. Track blue identity in WallManager/WordViewBridge
- Asymmetric per-side arena bounds REJECTED: uniform expansion when full ring broken. Add asymmetry only if playtesting demands it
- WallConfig SO APPROVED: WallRingConfig[] + BlueWordConfig, standard config pattern
- WallTracker APPROVED: HashSet<int> broken indices, CaptureState/RestoreState with bool[] in DefenseSaveData
- WallManager APPROVED: ITickable plain class, absorbs blue word spawning, manages wall word matching
- WallSegmentView APPROVED: MonoBehaviour prefab, standard view pattern
- WallViewBridge SIMPLIFIED: static segment views spawned once at init, no Factory. Refresh state, no destroy/recreate
- WallRevealLevel in PlayerStats + UpgradeId APPROVED: standard upgrade pattern
- Final class count: 4 new files (WallConfig, WallTracker, WallManager, WallSegmentView) + WallViewBridge, not 9
