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
- PlayerPrefs for persistence (no save system yet)
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
