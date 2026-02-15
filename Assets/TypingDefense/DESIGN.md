# Typing Defense - Documento de Diseno Completo

## Resumen

Juego incremental de tipeo donde "wordsteroids" vienen desde los bordes de la pantalla hacia el centro. El jugador tipea para destruirlos, gana letras como recurso, y las invierte en un arbol de mejoras permanentes. Dos presiones simultaneas: HP (1 golpe inicial) y Energia (drena con tiempo). Objetivo del prototipo: llegar al nivel 10 en ~20 min acumulados.

## Contexto Tecnico

- Dentro del proyecto Unity "Programental", carpeta separada `Assets/TypingDefense/`
- Namespace: `TypingDefense`
- Stack: Zenject DI, DOTween, ScriptableObjects, TMPro
- Convenciones: `var` siempre, guard clauses, cero programacion defensiva, juice obligatorio (Vlambeer)
- Clases planas para logica, MonoBehaviours solo para vistas/escena
- Sin tests (prioridad prototipado)

---

## 1. CORE GAMEPLAY

### Mecanica de tipeo
- Palabras ("wordsteroids") aparecen desde CUALQUIER borde de pantalla y se mueven hacia el centro
- TODAS las palabras reciben input simultaneamente (no hay target individual)
- Al completar una palabra entera = se destruye y da recompensa
- Si te equivocas en una letra = no pasa nada, se ignora (sin penalizacion para el prototipo)
- Diferentes velocidades por palabra (varianza random + escalado por nivel)
- Sin limite de palabras en pantalla
- Frecuencia de spawn fija al inicio, puede aumentar

### HP y Energia (dos sistemas separados)
- **HP** = vidas (empieza en 1, mejorable hasta 5). Palabra llega al centro = -1 HP
- **Energia** = 5 (drena con el tiempo). En niveles superiores drena mas rapido
- 0 HP o 0 Energia = game over (fin de la run)
- HP persiste entre niveles dentro de una run
- Al morir -> menu de mejoras -> nueva run con HP completo
- Energia recuperable con futuras mejoras (al destruir palabras, al pasar de nivel, etc.)

### Niveles
- Destruir 20 palabras completa el nivel, pero siguen spawneando mas
- Puedes quedarte farmeando sin avanzar
- Para avanzar al siguiente nivel: TIPEAR "warp" (palabra especial que aparece tras 20 kills)
- Niveles fijos para el prototipo (eventualmente infinitos)
- Spawn infinito por nivel. La energia es la presion para avanzar

### Palabras
- Tematica de programacion/tech (var, int, public, interface, coroutine, etc.)
- Pool minimo de 100 palabras distribuidas por longitud
- Idioma: ingles (keywords de programacion)

---

## 2. SISTEMA DE RECURSOS: LETRAS Y CONVERSION

### Tipos de Letras

| Letra | Valor conversion | Se desbloquea con |
|-------|-----------------|-------------------|
| a     | 1               | Inicio (gratis)   |
| b     | 3               | Upgrade ECO2      |
| c     | 9               | Upgrade ECO3      |
| d     | 27              | Upgrade ECO5      |
| e     | 81              | Upgrade ECO7      |

### Mecanica de obtencion
- Base: 1 letra por palabra destruida (mejorable con Letter Boost upgrades)
- Tipo de letra: roll en CASCADA desde la mas alta desbloqueada
  - Se intenta la letra mas alta primero (probabilidad del upgrade)
  - Si falla, baja a la siguiente. Letra 'a' es fallback garantizado
  - Cada letra dropeada hace su roll individual si tienes Letter Boost
- Almacenamiento ilimitado
- Las letras se CONVIERTEN en moneda en el menu principal (zona de conversion)
- Conversion instantanea, ratio directo (1 letra tipo X = X monedas)

### Tabla de probabilidades de drop (roll en cascada)

| Upgrades comprados | %e | %d | %c | %b | %a (fallback) |
|--------------------|-----|-----|-----|-----|---------------|
| Ninguno            | -   | -   | -   | -   | 100%          |
| Letter b Unlock    | -   | -   | -   | 30% | 70%           |
| + Letter c Unlock  | -   | -   | 20% | 30% | 50%           |
| + Letter d Unlock  | -   | 15% | 20% | 30% | 35%           |
| + Letter e Unlock  | 10% | 15% | 20% | 30% | 25%           |
| + Letter Ascension | 20% | 25% | 30% | 40% | ~0%           |

Letter Ascension (ECO6) suma +10% a TODAS las letras superiores.

### Ejemplo de economia
- Run en nivel 1 sin upgrades: destruyes 25 palabras -> 25 letras 'a' -> 25 monedas
- Run en nivel 5 con Letter b+c: destruyes 30 palabras -> mix de a/b/c -> ~150 monedas
- Run en nivel 8 con todo: destruyes 40 palabras x3 (boost) -> 120 letras mix d/e -> ~2000+ monedas

---

## 3. DISENO DE NIVELES (1-10)

| Nivel | Palabras min-max | Velocidad | Spawn cada | Drain energia | Kills para warp |
|-------|-----------------|-----------|------------|---------------|-----------------|
| 1     | 3-5 chars       | 0.8       | 3.0s       | 1/5s          | 20              |
| 2     | 3-6             | 0.9       | 2.8s       | 1/5s          | 20              |
| 3     | 4-6             | 1.0       | 2.6s       | 1/4.5s        | 20              |
| 4     | 4-7             | 1.1       | 2.4s       | 1/4.5s        | 20              |
| 5     | 5-7             | 1.2       | 2.2s       | 1/4s          | 20              |
| 6     | 5-8             | 1.3       | 2.0s       | 1/4s          | 20              |
| 7     | 6-8             | 1.4       | 1.8s       | 1/3.5s        | 20              |
| 8     | 6-9             | 1.5       | 1.6s       | 1/3.5s        | 20              |
| 9     | 7-9             | 1.6       | 1.4s       | 1/3s          | 20              |
| 10    | 7-10            | 1.8       | 1.2s       | 1/3s          | 20              |

- Las letras NO se desbloquean por nivel. Se compran como upgrades
- 20 kills completan el nivel. Siguen spawneando si te quedas
- Tipear "warp" para avanzar
- Energia inicial: 5 (todos los niveles)

---

## 4. ARBOL DE MEJORAS (30 upgrades, 5 tiers, 5 categorias internas)

Arbol UNIFICADO por tiers. El jugador ve un arbol lineal por filas, NO ramas separadas.
Para desbloquear Tier N: necesitas CUALQUIER 2 upgrades del Tier N-1 (excepto Tier 1, siempre disponible).
Categorias internas (solo para diseno): Economica, Ofensiva, Defensiva, Supervivencia, Utility.
Moneda: letras convertidas en el menu.

### TIER 1 (Costo: 50-90m) — Siempre disponible

| ID   | Nombre             | Cat.   | Efecto                                          | Costo |
|------|--------------------|--------|------------------------------------------------|-------|
| ECO1 | Letter Boost I     | Eco    | +1 letra por palabra destruida (total: 2)       | 60    |
| OFF1 | Auto-Type Unlock   | Off    | Auto-tipea 1 letra correcta cada 12s            | 80    |
| DEF1 | Extra Health I     | Def    | +1 HP maximo (total: 2)                          | 70    |
| SUR1 | Energy Harvest I   | Surv   | +0.5 energia por kill                            | 90    |
| UTI1 | Power-up Frequency | Util   | Power-ups cada 8 palabras (en vez de 10)         | 50    |

### TIER 2 (Costo: 120-250m) — Requiere 2 upgrades de Tier 1

| ID   | Nombre             | Cat.   | Efecto                                          | Costo |
|------|--------------------|--------|------------------------------------------------|-------|
| ECO2 | Letter b Unlock    | Eco    | 30% probabilidad de letra 'b' (valor: 3)        | 150   |
| OFF2 | Critical Strike I  | Off    | 12% chance de destruir palabra entera al tipear  | 200   |
| DEF2 | Extra Health II    | Def    | +1 HP maximo (total: 3)                          | 250   |
| SUR2 | Slow Drain I       | Surv   | Drenaje de energia -15%                          | 180   |
| UTI2 | Power-up Duration  | Util   | Power-ups duran +3s                              | 120   |

### TIER 3 (Costo: 400-700m) — Requiere 2 upgrades de Tier 2

| ID   | Nombre             | Cat.   | Efecto                                          | Costo |
|------|--------------------|--------|------------------------------------------------|-------|
| ECO3 | Letter c Unlock    | Eco    | 20% probabilidad de letra 'c' (valor: 9)        | 500   |
| ECO4 | Letter Boost II    | Eco    | +1 letra por palabra (total: 3)                  | 400   |
| OFF3 | Auto-Type Speed I  | Off    | Auto-tipea cada 8s (en vez de 12s)               | 450   |
| OFF4 | Critical Strike II | Off    | 25% chance de destruir palabra entera            | 600   |
| DEF3 | Extra Health III   | Def    | +1 HP maximo (total: 4)                          | 700   |
| SUR3 | Energy Harvest II  | Surv   | +1 energia por kill (total: 1.0)                 | 550   |

### TIER 4 (Costo: 900-2000m) — Requiere 2 upgrades de Tier 3

| ID   | Nombre             | Cat.   | Efecto                                          | Costo |
|------|--------------------|--------|------------------------------------------------|-------|
| ECO5 | Letter d Unlock    | Eco    | 15% probabilidad de letra 'd' (valor: 27)       | 1500  |
| ECO6 | Letter Ascension   | Eco    | +10% probabilidad a TODAS las letras superiores  | 1200  |
| OFF5 | Auto-Type Multi I  | Off    | Auto-tipea 2 letras por tick                     | 1400  |
| OFF6 | Critical Strike III| Off    | 40% chance de destruir palabra entera            | 1800  |
| DEF4 | Extra Health IV    | Def    | +1 HP maximo (total: 5)                          | 2000  |
| DEF5 | Shield Protocol    | Def    | 1a palabra que te golpee no hace dano (1x/nivel) | 1100  |
| SUR4 | Energy Reserve I   | Surv   | +2 energia maxima (total: 7)                     | 1000  |
| UTI3 | Combo Breaker      | Util   | 3 kills en <5s -> +3 letras bonus                | 900   |

### TIER 5 (Costo: 3000-5000m) — Requiere 2 upgrades de Tier 4

| ID   | Nombre             | Cat.   | Efecto                                          | Costo |
|------|--------------------|--------|------------------------------------------------|-------|
| ECO7 | Letter e Unlock    | Eco    | 10% probabilidad de letra 'e' (valor: 81)       | 4000  |
| ECO8 | Letter Boost III   | Eco    | +1 letra por palabra (total: 4)                  | 3500  |
| OFF7 | Auto-Type Speed II | Off    | Auto-tipea cada 5s                               | 3800  |
| OFF8 | Auto-Type Multi II | Off    | Auto-tipea 3 letras por tick                     | 5000  |
| SUR5 | Energy Harvest III | Surv   | +1.5 energia por kill (total: 1.5)               | 4200  |
| SUR6 | Slow Drain II      | Surv   | Drenaje de energia -30% adicional (total: -45%)  | 3000  |

### Auto-Type: Progresion completa

| Upgrade | Frecuencia | Letras/tick | Efecto endgame |
|---------|------------|-------------|----------------|
| OFF1    | 12s        | 1           | ~5 letras/min  |
| OFF3    | 8s         | 1           | ~7 letras/min  |
| OFF5    | 8s         | 2           | ~15 letras/min |
| OFF7    | 5s         | 2           | ~24 letras/min |
| OFF8    | 5s         | 3           | ~36 letras/min |

Auto-type selecciona letras random que ALGUNA palabra necesita como siguiente caracter.
Acierta en TODAS las palabras que tengan esa letra como siguiente. NO cuenta como input manual (no triggerea critical hit).

### Critical Hit: Progresion completa

| Upgrade | Probabilidad | Nota |
|---------|-------------|------|
| OFF2    | 12%         | Cada letra tipada manualmente tiene 12% de insta-kill |
| OFF4    | 25%         | ~1 de cada 4 letras mata la palabra entera |
| OFF6    | 40%         | Casi la mitad de tus keystrokes son letales |

Da reward completo al hacer critical (como si completaras toda la palabra).

### Energy Harvest: Progresion completa

| Upgrade | Energia/kill | Break-even (drain vs regen) |
|---------|-------------|----------------------------|
| Sin upgrade | 0        | Imposible |
| SUR1    | +0.5        | Necesitas ~2 kills cada 5s (nivel 1) |
| SUR3    | +1.0        | Necesitas ~1 kill cada 5s (nivel 1) |
| SUR5    | +1.5        | Infinito si matas rapido en niveles bajos |

### 4.1 Progresion sugerida (Primeras 8 Runs)

- **Run 1**: Mueres nivel 2-3. ~25 'a' = 25m. Vuelves al menu, ves conversion + arbol. No alcanza para nada pero ENTIENDES el loop.
- **Run 2**: Mueres nivel 3-4. ~30 'a' = 30m. Total: 55m. Compras UTI1 (50m). Primera mejora!
- **Run 3**: Mueres nivel 4. ~40 'a' = 40m. Total: 45m. Acumulas.
- **Run 4**: Mueres nivel 5. ~50 'a' = 50m. Total: 95m. Compras DEF1 (70m). Resto: 25m.
- **Run 5**: Llegas nivel 6-7. ~60 'a' = 60m. Total: 85m. Compras OFF1 (80m). Auto-type!
- **Run 6**: Llegas nivel 7-8. ~80 'a' = 80m. Total: 85m. Acumulas para T2.
- **Run 7**: Llegas nivel 8-9. ~100 'a' = 100m. Total: 185m. Compras ECO2 (150m). Letter b!
- **Run 8**: Llegas nivel 10. ~120 palabras x mix a/b = ~190m. VICTORIA. Compras SUR1+más.

### 4.2 Power Fantasy por fase

- **Early (Runs 1-4)**: Tipeas todo manual. Mueres rapido. Cada HP extra se siente enorme.
- **Mid (Runs 5-7)**: Auto-type ayuda. Critical empieza a destruir palabras. Energy harvest alarga runs.
- **Late (Runs 8-12)**: Auto-type cada 8s x2 letras + 25% critical. Letras c/d cayendo. Avalancha de monedas.
- **Endgame (post-Rewrite)**: Auto-type cada 5s x3 letras + 40% critical + energy infinita. Te sientas a mirar.

---

## 5. POWER-UPS

| Nombre        | Trigger        | Efecto base               | Duracion  |
|---------------|----------------|---------------------------|-----------|
| Slow Motion   | tipear "slow"  | -30% velocidad palabras   | 8s        |
| Shield        | tipear "shield"| bloquea 1 golpe           | hasta uso |
| Energy Surge  | tipear "surge" | +2 energia                | instantaneo|
| Letter Rain   | tipear "rain"  | x2 letras obtenidas       | 10s       |
| Clear Screen  | tipear "clear" | destruye todo (sin reward) | instantaneo|

- Aparecen cada 10 palabras destruidas (8 con upgrade U1)
- Palabra dorada ESTATICA en esquina superior derecha
- Aleatorio, no repite el ultimo
- Mejorables desde el arbol de upgrades (U2-U5)

---

## 6. PRESTIGIO: "REWRITES"

Se desbloquea al llegar a nivel 10 por primera vez.

### Que se resetea
- Todas las mejoras del arbol
- Todas las letras y monedas
- Progreso de niveles

### Que se gana
- 1 Rewrite Point (RP)

### Mejoras permanentes con RP

| Nombre               | Efecto                                      | Costo RP | Prerreq             |
|----------------------|---------------------------------------------|----------|---------------------|
| Rewrite Boost I      | Empiezas con HP=2                            | 1        | -                   |
| Rewrite Boost II     | Empiezas con HP=3                            | 2        | Rewrite Boost I     |
| Letter Multiplier I  | +50% CANTIDAD de letras por kill             | 1        | -                   |
| Letter Multiplier II | +50% adicional (total: +100% cantidad)       | 2        | Letter Multiplier I |
| Letter Quality I     | +5% probabilidad a TODAS las letras superiores| 2        | -                   |
| Fast Start           | Empiezas con 100 monedas                     | 1        | -                   |
| Energy Mastery       | Drenaje 15% mas lento permanente             | 2        | -                   |

---

## 7. MENU PRINCIPAL (progresivo)

- Primera vez: Solo boton "PLAY"
- Despues de la primera run (game over): Se desbloquean conversion de letras + arbol de mejoras
  - Al volver al menu por primera vez se muestra la conversion automaticamente
  - Luego se ve el arbol con los upgrades que puedes comprar
  - Debe quedar CLARO que el loop es: jugar -> farmear letras -> convertir -> comprar mejoras -> jugar mejor
- Despues de llegar a nivel 10: Se desbloquea zona de Rewrite

Escena unica con paneles que se muestran/ocultan.

---

## 8. BALANCE Y CURVA DE PROGRESION

| Hito                              | Tiempo acumulado | Runs |
|-----------------------------------|------------------|------|
| Primera muerte (nivel 2-3)        | 2-3 min          | 1    |
| Primera compra (UTI1)             | 5-6 min          | 2    |
| Primer power spike (DEF1+OFF1)    | 12-14 min        | 4-5  |
| Primer Letter b Unlock            | 18-20 min        | 7    |
| Primera llegada a nivel 10        | 22-26 min        | 8    |
| Primera Rewrite                   | 28-32 min        | 9-10 |

### Farmeo vs Progresion
- **Early (Runs 1-4)**: No hay letras superiores, solo 'a'. Cada moneda cuenta.
- **Mid (Runs 5-7)**: Auto-type + critical empiezan a ayudar. Energy harvest alarga runs.
- **Late (Runs 8+)**: Letter b/c cayendo. Monedas por run suben exponencialmente.
- **Post-Rewrite**: Power fantasy total. Auto-type + critical + energy = semi-automatico.

### Riesgos de balance
- Errores se ignoran en prototipo (considerar penalizacion como feature futura)
- Farmeo en nivel 1 limitado por energia (~25s con 5 energia y drain 1/5s)
- Clear Screen no da reward (solo salva, no farmea)
- Pool de palabras minimo 100 para evitar monotonia
- Auto-type no debe trivializar el juego antes de llegar a Tier 4-5
- Critical hit endgame (40%) con auto-type puede ser OP -> es intencional (power fantasy)

---

## 9. ARQUITECTURA TECNICA

### Decisiones del reviewer aplicadas
- DefenseWord es SOLO datos de matching (sin posicion/movimiento -> eso va en DefenseWordView)
- InputHandler eliminado (lectura de input va en WordManager.Tick())
- Kill count en WordManager, no en RunManager
- UpgradeId como enum, no strings
- WarpWord no es clase separada (deteccion por texto en WordManager)
- PrestigeData fuera del save hasta que se implemente (YAGNI)

### 9.1 Clases planas (logica)

| Clase              | Responsabilidad                                      | Zenject binding                    |
|--------------------|------------------------------------------------------|------------------------------------|
| DefenseWord        | Texto, progreso, reward, TryMatchChar/Reset          | N/A (creado por WordManager)       |
| WordManager        | Spawn, tracking, input, kill count, warp, auto-type  | BindInterfacesAndSelfTo.AsSingle   |
| RunManager         | HP, nivel actual, game over                          | Bind.AsSingle                      |
| EnergyTracker      | Drain pasivo, refill                                 | BindInterfacesAndSelfTo.AsSingle   |
| LetterTracker      | Inventario letras por tipo, roll cascada, conversion | Bind.AsSingle                      |
| UpgradeTracker     | Niveles de upgrades, compra, apply effects           | Bind.AsSingle                      |
| PlayerStats        | Data holder mutable: MaxHp, Energy, CritChance, etc  | Bind.AsSingle                      |
| GameFlowController | Estado {Menu, Playing, GameOver, Paused}             | Bind.AsSingle                      |
| WordPool           | Carga txt, GetRandomWord(minLen, maxLen)             | Bind.AsSingle                      |

### 9.2 MonoBehaviours (vistas)

| Clase            | Responsabilidad                                     |
|------------------|-----------------------------------------------------|
| DefenseWordView  | Renderiza TMP + MOVIMIENTO hacia centro + juice     |
| ArenaView        | Bounds, GetRandomEdgePosition(), CenterPosition     |
| HudView          | HP, energia, nivel, kills, letras                   |
| MenuView         | Conversion, arbol, prestige (paneles show/hide)     |
| ScreenShaker     | Shake de camara y UI (copiado de Programental)      |
| AudioPlayer      | Pool de AudioSources (copiado de Programental)      |

### 9.3 ScriptableObjects

| Config           | Campos clave                                                              |
|------------------|---------------------------------------------------------------------------|
| WordSpawnConfig  | baseSpawnInterval(3), baseWordSpeed(30), speedVariance(0.3), scaling/lvl  |
| RunConfig        | baseDrainRate(0.333), killsToWarp(20), baseMaxHp(1), baseMaxEnergy(5)     |
| LetterConfig     | letterTypes[]{type, conversionValue}, baseLettersPerKill(1)               |
| UpgradeTreeConfig| UpgradeDefinition[]{id(enum), cost, maxLevel, prerequisites, statEffect}  |
| SoundLibrary     | SoundEntry[]{key, clips[], volume} (copiado de Programental)             |

### 9.4 Game States

```
Menu ---[StartRun]--> Playing
Playing ---[HP=0 o Energy=0]--> GameOver
Playing ---[Escape]--> Paused
Paused ---[Escape/Resume]--> Playing
Paused ---[Quit]--> Menu
GameOver ---[Continue]--> Menu
```

### 9.5 Save Data

Solo se guarda progreso permanente. La run es efimera (cerrar = perder run).

```
DefenseSaveData {
  LetterData letters        // int[] por tipo
  UpgradeData[] upgrades    // enum id + nivel
}
```

Patron: CaptureState()/RestoreState() tipados (mismo que Programental).
Orden restauracion: LetterTracker primero, luego UpgradeTracker (aplica stats).

### 9.6 Flujo de datos

**Input -> Destruccion -> Recompensa:**
```
WordManager.Tick() lee Input.inputString
  -> ProcessChar(c) itera TODAS las DefenseWord
  -> word.TryMatchChar(c)
  -> si ninguna matchea: se ignora (sin penalizacion)
  -> si matchea: roll CriticalHit (PlayerStats.CritChance)
    -> si critical: DestroyWord() inmediatamente (reward completo)
    -> si no critical y completada: DestroyWord() normal
  -> DestroyWord: LetterTracker.EarnLetters() + EnergyTracker.OnKill() + kill count++
```

**Auto-Type (en WordManager.Tick()):**
```
autoTypeTimer -= deltaTime
si autoTypeTimer <= 0:
  -> recolecta todas las "next chars" que las palabras necesitan
  -> elige N chars random (segun PlayerStats.AutoTypeCount)
  -> ProcessAutoChar(c): itera TODAS las DefenseWord
  -> NO triggerea critical hit (solo input manual lo hace)
  -> si completada: DestroyWord() normal
  -> autoTypeTimer = PlayerStats.AutoTypeInterval
```

**Letter Drop (en LetterTracker.EarnLetters()):**
```
por cada letra a dropear (base + Letter Boost):
  -> roll cascada: e% -> d% -> c% -> b% -> fallback 'a'
  -> probabilidad viene de PlayerStats (actualizada por UpgradeTracker)
  -> suma al inventario
```

**Palabra llega al centro:**
```
DefenseWordView detecta distancia <= threshold
  -> WordManager.HandleWordReachedCenter()
  -> RunManager.TakeDamage(1)
  -> si HP <= 0: GameFlowController.TriggerGameOver()
```

**Energia:**
```
EnergyTracker.Tick() drena energia continua (rate * PlayerStats.DrainMultiplier)
  -> si <= 0: RunManager.TriggerGameOver()
EnergyTracker.OnKill() suma PlayerStats.EnergyPerKill
```

**Warp:**
```
WordManager: killCount >= 20 -> spawnea palabra "warp"
  -> al completar "warp": RunManager.AdvanceLevel()
```

### 9.7 Zenject Installer

```csharp
public class TypingDefenseInstaller : MonoInstaller
{
    [SerializeField] private WordSpawnConfig wordSpawnConfig;
    [SerializeField] private RunConfig runConfig;
    [SerializeField] private LetterConfig letterConfig;
    [SerializeField] private UpgradeTreeConfig upgradeTreeConfig;
    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private DefenseWordView wordViewPrefab;

    public override void InstallBindings()
    {
        // Configs
        Container.BindInstance(wordSpawnConfig);
        Container.BindInstance(runConfig);
        Container.BindInstance(letterConfig);
        Container.BindInstance(upgradeTreeConfig);
        Container.BindInstance(soundLibrary);

        // Core con ITickable
        Container.BindInterfacesAndSelfTo<WordManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<EnergyTracker>().AsSingle();
        Container.BindInterfacesAndSelfTo<DefenseSaveManager>().AsSingle();

        // Core sin interfaces
        Container.Bind<PlayerStats>().AsSingle();
        Container.Bind<LetterTracker>().AsSingle();
        Container.Bind<UpgradeTracker>().AsSingle();
        Container.Bind<WordPool>().AsSingle();
        Container.Bind<RunManager>().AsSingle();
        Container.Bind<GameFlowController>().AsSingle();

        // Save lifecycle
        Container.Bind<DefenseSaveLifecycleHook>()
            .FromNewComponentOnNewGameObject().AsSingle().NonLazy();

        // Views
        Container.Bind<ArenaView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<HudView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<MenuView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ScreenShaker>().FromComponentInHierarchy().AsSingle();
        Container.Bind<AudioPlayer>().FromComponentInHierarchy().AsSingle();

        // Factory para palabras
        Container.BindFactory<DefenseWord, DefenseWordView, DefenseWordView.Factory>()
            .FromComponentInNewPrefab(wordViewPrefab);
    }
}
```

---

## 10. ESTRUCTURA DE CARPETAS

```
Assets/TypingDefense/
  Runtime/
    Core/           DefenseWord.cs, WordManager.cs, WordPool.cs
    Run/            RunManager.cs, EnergyTracker.cs, PlayerStats.cs
    Economy/        LetterTracker.cs, LetterType.cs, UpgradeTracker.cs, UpgradeId.cs, UpgradeTier.cs
    Infrastructure/ GameFlowController.cs, GameState.cs, DefenseSaveManager.cs,
                    DefenseSaveLifecycleHook.cs, DefenseSaveData.cs, TypingDefenseInstaller.cs
    Views/          DefenseWordView.cs, ArenaView.cs, HudView.cs, MenuView.cs
    Shared/         ScreenShaker.cs, AudioPlayer.cs, SoundLibrary.cs
    Config/         WordSpawnConfig.cs, RunConfig.cs, LetterConfig.cs, UpgradeTreeConfig.cs
  Resources/
    WordLists/      words_3to5.txt, words_6to7.txt, words_8to10.txt
  Scenes/           TypingDefense.unity
```

---

## 11. CODIGO REUTILIZADO DE PROGRAMENTAL

| Copiar tal cual        | Adaptar                                    | Ignorar              |
|------------------------|--------------------------------------------|----------------------|
| ScreenShaker           | GoldenCodeWord.CheckChar -> DefenseWord    | CodeTyper            |
| AudioPlayer+SoundLib   | LinesTracker -> LetterTracker              | BaseMultiplierTracker|
| SaveManager patron     | BonusMultipliers -> PlayerStats            | CodeLineCloneManager |
| Zenject installer      | CodeLinePool -> WordPool                   | DeleteCodeButtonView |
| View patterns (DOTween)| GoldenCodeManager -> WordManager           |                      |

---

## 12. ORDEN DE IMPLEMENTACION

1. **Core loop**: WordManager + DefenseWord + DefenseWordView + ArenaView
2. **Supervivencia**: RunManager + EnergyTracker + HudView (HP, energia, game over)
3. **Recursos**: LetterTracker + WordPool + conversion
4. **Menu**: GameFlowController + MenuView (transiciones Playing<->Menu)
5. **Upgrades**: UpgradeTracker + PlayerStats + UpgradeTreeConfig
6. **Power-ups**: Sistema de spawn de power-ups + efectos
7. **Save**: DefenseSaveManager con CaptureState/RestoreState
8. **Polish**: Juice en todo, sonidos, prestige

---

## 13. CRITERIOS DE ACEPTACION

### Core Gameplay
- [ ] Palabras aparecen desde cualquier borde, se mueven hacia el centro
- [ ] Todas reciben input simultaneamente
- [ ] Completar palabra = destruccion con juice
- [ ] Error = se ignora sin penalizacion (simplificado para prototipo)
- [ ] HP y Energia en HUD
- [ ] 0 HP o 0 Energia = game over -> menu

### Niveles
- [ ] 20 kills completan nivel, siguen spawneando
- [ ] Tipear "warp" avanza de nivel
- [ ] Cada nivel tiene parametros correctos de la tabla

### Economia
- [ ] Letras se acumulan durante run con roll en cascada
- [ ] Conversion letras -> monedas en menu (a=1, b=3, c=9, d=27, e=81)
- [ ] Monedas compran upgrades
- [ ] Letter unlocks comprados como upgrades (no por nivel)

### Upgrades
- [ ] 30 mejoras en 5 tiers con prerequisito (2 del tier anterior)
- [ ] Auto-type funciona (frecuencia + cantidad)
- [ ] Critical hit funciona (% por letra manual)
- [ ] Energy harvest funciona (+energia por kill)
- [ ] Efectos se aplican correctamente a PlayerStats
- [ ] Persisten entre runs

### Power-ups
- [ ] Cada 10 kills aparece power-up dorado estatico
- [ ] 5 power-ups funcionan correctamente
- [ ] Mejorables desde arbol

### Meta
- [ ] Save entre sesiones
- [ ] Menu progresivo (play -> conversion -> arbol -> prestige)
- [ ] Juice en TODO
- [ ] Sonidos para todos los eventos
- [ ] Llegar a nivel 10 en ~25 min (7-8 runs)
- [ ] Power fantasy: sentirse OP en late game

---

## 14. POOL DE PALABRAS SUGERIDO

### Longitud 3-5
```
var, int, for, new, get, set, bool, char, void, loop, func, code, byte, list,
null, true, call, push, pull, fork, join, lock, task, heap, main, enum, args,
type, node, sync, pipe, port, hash, sudo, grep, bash, mock, test, debug, class
```

### Longitud 6-7
```
public, static, return, string, struct, switch, object, method, lambda, vector,
matrix, render, shader, prefab, canvas, sprite, update, import, export, compile,
deploy, docker, kernel, module, server, client, socket, thread, stream, buffer
```

### Longitud 8-10
```
interface, abstract, property, override, delegate, namespace, component, transform,
gameobject, coroutine, scriptable, serialize, singleton, quaternion, collision,
raycasting, algorithm, exception, polymorphic, dependency, controller, middleware
```

### Palabras especiales (no van en pool normal)
```
warp, slow, shield, surge, rain, clear
```