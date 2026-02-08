# Convenciones de código - Programental

## Estilo
- Usar `var` siempre que sea posible
- Guard clauses (early return) en vez de ifs anidados
- No programacion defensiva: nada de null checks innecesarios. Si algo es null es un bug, que pete
- Juice/feedback visual en todo lo que el jugador vea (filosofia Vlambeer "The Art of Screenshake")

## Arquitectura
- Zenject para DI
- Clases planas para logica de negocio (inyeccion por constructor)
- MonoBehaviours solo cuando se necesitan referencias de escena o ciclo de vida de Unity
- MilestoneReward como base abstracta: cada subclase implementa `RewardId` (propiedad abstracta) y se auto-registra en MilestoneTracker via `[Inject]`
- ScriptableObjects para configuracion (MilestonesConfig, GoldenCodeConfig)
- DOTween para animaciones
