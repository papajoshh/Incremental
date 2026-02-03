# Refactor: Patrón State para MoñecoMonoBehaviour

## Problema actual

Cada vez que se añade un estado nuevo al Moñeco hay que:
1. Añadir al enum `State`
2. Añadir hash estático del animator
3. Añadir case en `UpdateAnimation()`
4. Añadir método `OnXxxComplete()`

Esto viola el principio Open/Closed.

---

## Solución: Patrón State con Zenject

### Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│                  MoñecoMonoBehaviour                    │
│  - Host de Unity (Transform, Animator)                  │
│  - Recibe callbacks de animación y delega al estado     │
│  - Expone propiedades para que los estados las usen     │
└─────────────────────────┬───────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    IMoñecoState                         │
│  + GetAnimationHash(moñeco): int?                       │
│  + OnEnter(moñeco)                                      │
│  + OnStep(moñeco)                                       │
│  + OnComplete(moñeco)                                   │
│  + OnUpdate(moñeco)                                     │
└─────────────────────────────────────────────────────────┘
                          △
          ┌───────┬───────┼───────┬───────┐
          │       │       │       │       │
       Walking Falling Landing  Birth  GoToBag ...
```

---

## Archivos a crear/modificar

### 1. Nueva interface: `IMoñecoState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/IMoñecoState.cs`

```csharp
namespace Runtime.Infraestructure.MoñecoStates
{
    public interface IMoñecoState
    {
        int? GetAnimationHash(MoñecoMonoBehaviour moñeco);
        void OnEnter(MoñecoMonoBehaviour moñeco) { }
        void OnStep(MoñecoMonoBehaviour moñeco) { }
        void OnComplete(MoñecoMonoBehaviour moñeco) { }
        void OnUpdate(MoñecoMonoBehaviour moñeco) { }
    }
}
```

---

### 2. Hashes de animación: `AnimHashes.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/AnimHashes.cs`

```csharp
using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public static class AnimHashes
    {
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int WalkRight = Animator.StringToHash("WalkRight");
        public static readonly int WalkLeft = Animator.StringToHash("WalkLeft");
        public static readonly int Falling = Animator.StringToHash("Falling");
        public static readonly int Landing = Animator.StringToHash("Landing");
        public static readonly int Birth = Animator.StringToHash("Birth");
        public static readonly int Interacting = Animator.StringToHash("Interacting");
        public static readonly int InteractingMachine = Animator.StringToHash("RepairComputer");
        public static readonly int GoToBag = Animator.StringToHash("GoToBag");
    }
}
```

---

### 3. Estados concretos

#### `IdleState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/IdleState.cs`

```csharp
namespace Runtime.Infraestructure.MoñecoStates
{
    public class IdleState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco) => AnimHashes.Idle;

        public void OnUpdate(MoñecoMonoBehaviour moñeco)
        {
            if (!moñeco.IsGrounded())
                moñeco.ChangeState<FallingState>();
        }
    }
}
```

#### `WalkingState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/WalkingState.cs`

```csharp
using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public class WalkingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco) =>
            moñeco.Direction > 0 ? AnimHashes.WalkRight : AnimHashes.WalkLeft;

        public void OnStep(MoñecoMonoBehaviour moñeco)
        {
            Vector2 center = moñeco.BodyPosition != null
                ? moñeco.BodyPosition.position
                : moñeco.transform.position;
            Vector2 moveDirection = Vector2.right * moñeco.Direction;
            Vector2 origin = center + moveDirection * moñeco.BodyHalfWidth;

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                moveDirection,
                moñeco.StepDistance,
                moñeco.WallLayer
            );

            if (hit.collider != null)
            {
                moñeco.ChangeState<TurningState>();
                return;
            }

            moñeco.transform.position += Vector3.right * moñeco.Direction * moñeco.StepDistance;
            moñeco.CheckWalkTarget();
        }

        public void OnUpdate(MoñecoMonoBehaviour moñeco)
        {
            if (!moñeco.IsGrounded())
                moñeco.ChangeState<FallingState>();
        }
    }
}
```

#### `FallingState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/FallingState.cs`

```csharp
using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public class FallingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco) => AnimHashes.Falling;

        public void OnStep(MoñecoMonoBehaviour moñeco)
        {
            Vector2 origin = moñeco.FeetPosition.position;
            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                Vector2.down,
                moñeco.FallStepDistance,
                moñeco.GroundLayer
            );

            if (hit.collider != null)
            {
                float distanceToGround = hit.distance;
                moñeco.transform.position += Vector3.down * distanceToGround;
                moñeco.ChangeState<LandingState>();
                return;
            }

            moñeco.transform.position += Vector3.down * moñeco.FallStepDistance;
        }
    }
}
```

#### `LandingState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/LandingState.cs`

```csharp
namespace Runtime.Infraestructure.MoñecoStates
{
    public class LandingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco) => AnimHashes.Landing;

        public void OnComplete(MoñecoMonoBehaviour moñeco)
        {
            moñeco.StartWalking(moñeco.Direction);
        }
    }
}
```

#### `TurningState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/TurningState.cs`

```csharp
namespace Runtime.Infraestructure.MoñecoStates
{
    public class TurningState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco) => null;

        public void OnComplete(MoñecoMonoBehaviour moñeco)
        {
            moñeco.Direction *= -1;
            moñeco.ChangeState<WalkingState>();
        }
    }
}
```

#### `BirthState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/BirthState.cs`

```csharp
namespace Runtime.Infraestructure.MoñecoStates
{
    public class BirthState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco) => AnimHashes.Birth;

        public void OnComplete(MoñecoMonoBehaviour moñeco)
        {
            moñeco.BirthTcs?.TrySetResult(true);
            moñeco.Air();
        }
    }
}
```

#### `GoToBagState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/GoToBagState.cs`

```csharp
using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public class GoToBagState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco) => AnimHashes.GoToBag;

        public void OnComplete(MoñecoMonoBehaviour moñeco)
        {
            Object.Destroy(moñeco.gameObject);
        }
    }
}
```

#### `InteractingState.cs`

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoStates/InteractingState.cs`

```csharp
namespace Runtime.Infraestructure.MoñecoStates
{
    public class InteractingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour moñeco)
        {
            var animationName = moñeco.CurrentInteractable?.CurrentInteractionInfo?.InteractionAnimation;

            return animationName switch
            {
                "RepairComputer" => AnimHashes.InteractingMachine,
                _ => AnimHashes.Interacting
            };
        }

        public void OnStep(MoñecoMonoBehaviour moñeco)
        {
            moñeco.CurrentInteractable?.OnInteractionTick(moñeco);
        }
    }
}
```

---

### 4. Installer de Zenject: `MoñecoStatesInstaller.cs`

**Ruta:** `Assets/Runtime/Main/MoñecoStatesInstaller.cs`

```csharp
using Runtime.Infraestructure.MoñecoStates;
using Zenject;

namespace Runtime.Main
{
    public class MoñecoStatesInstaller : Installer<MoñecoStatesInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IdleState>().AsSingle();
            Container.Bind<WalkingState>().AsSingle();
            Container.Bind<FallingState>().AsSingle();
            Container.Bind<LandingState>().AsSingle();
            Container.Bind<TurningState>().AsSingle();
            Container.Bind<BirthState>().AsSingle();
            Container.Bind<GoToBagState>().AsSingle();
            Container.Bind<InteractingState>().AsSingle();
        }
    }
}
```

**Nota:** Añadir en el installer principal:
```csharp
MoñecoStatesInstaller.Install(Container);
```

---

### 5. MoñecoMonoBehaviour refactorizado

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoMonoBehaviour.cs`

```csharp
using System.Threading.Tasks;
using Runtime.Application;
using Runtime.Domain;
using Runtime.Infraestructure.MoñecoStates;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class MoñecoMonoBehaviour : MonoBehaviour, Interactor
    {
        [Header("Movement")]
        [SerializeField] private float stepDistance = 0.1f;
        [SerializeField] private float fallStepDistance = 0.15f;

        [Header("Ground Detection")]
        [SerializeField] private Transform feetPosition;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Wall Detection")]
        [SerializeField] private Transform bodyPosition;
        [SerializeField] private float bodyHalfWidth = 0.25f;
        [SerializeField] private LayerMask wallLayer;

        [Header("Initial State")]
        [SerializeField] private bool startWalking = true;

        public int Direction { get; set; } = 1;
        public Transform FeetPosition => feetPosition;
        public Transform BodyPosition => bodyPosition;
        public float StepDistance => stepDistance;
        public float FallStepDistance => fallStepDistance;
        public float GroundCheckDistance => groundCheckDistance;
        public float BodyHalfWidth => bodyHalfWidth;
        public LayerMask GroundLayer => groundLayer;
        public LayerMask WallLayer => wallLayer;
        public bool StartWalkingOnAir => startWalking;

        public Interactable CurrentInteractable { get; set; }
        public Interactable PendingInteractable { get; set; }
        public float? WalkToTargetX { get; set; }
        public TaskCompletionSource<bool> BirthTcs { get; set; }

        public bool IsWalking => _currentState is WalkingState;

        private IMoñecoState _currentState;
        private bool _restored;
        private Animator _animator;
        private DiContainer _container;

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }

        void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        void Start()
        {
            if (_currentState is BirthState) return;
            if (_restored) return;
            Air();
        }

        void Update()
        {
            _currentState?.OnUpdate(this);
        }

        public void OnStep() => _currentState?.OnStep(this);
        public void OnFallStep() => _currentState?.OnStep(this);
        public void OnLandingComplete() => _currentState?.OnComplete(this);
        public void OnWalkCycleComplete() => _currentState?.OnComplete(this);
        public void OnBirthComplete() => _currentState?.OnComplete(this);
        public void OnGoToBagComplete() => _currentState?.OnComplete(this);
        public void OnInteractionTick() => _currentState?.OnStep(this);

        public void ChangeState<T>() where T : IMoñecoState
        {
            var newState = _container.Resolve<T>();
            _currentState = newState;
            _currentState.OnEnter(this);

            var hash = _currentState.GetAnimationHash(this);
            if (hash.HasValue && _animator != null)
                _animator.Play(hash.Value);
        }

        public void Air()
        {
            if (IsGrounded())
            {
                if (startWalking)
                    StartWalking(1);
                else
                    ChangeState<IdleState>();
            }
            else
            {
                ChangeState<FallingState>();
            }
        }

        public void StartWalking(int newDirection)
        {
            Direction = newDirection;
            ChangeState<WalkingState>();
        }

        public void GoToBag()
        {
            ChangeState<GoToBagState>();
        }

        public bool IsGrounded()
        {
            Vector2 origin = feetPosition.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
            return hit.collider != null;
        }

        public bool IsWalkingToInteraction() => WalkToTargetX.HasValue;

        public bool HasReachedTarget()
        {
            if (!WalkToTargetX.HasValue) return false;
            return (Direction > 0 && transform.position.x >= WalkToTargetX.Value)
                || (Direction < 0 && transform.position.x <= WalkToTargetX.Value);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CheckToInteract(collision);
            CheckToCrossDoor(collision);
        }

        private void CheckToInteract(Collider2D collision)
        {
            if (!IsWalking) return;
            if (IsWalkingToInteraction()) return;

            var interactable = collision.GetComponent<Interactable>();
            if (interactable == null) return;
            if (!interactable.CanInteract(this)) return;

            PendingInteractable = interactable;
            PendingInteractable.StartInteraction(this);
        }

        private void CheckToCrossDoor(Collider2D collision)
        {
            if (!IsWalking) return;
            var door = collision.GetComponent<Door>();
            door?.CrossTo(gameObject.transform);
        }

        public void SetPositionToInteract(Transform interactPosition)
        {
            WalkToTargetX = interactPosition.position.x;
            if (HasReachedTarget())
                ArriveAtInteraction();
        }

        public void CheckWalkTarget()
        {
            if (HasReachedTarget())
                ArriveAtInteraction();
        }

        private void ArriveAtInteraction()
        {
            transform.position = new Vector3(WalkToTargetX.Value, transform.position.y, transform.position.z);
            CurrentInteractable = PendingInteractable;
            PendingInteractable = null;
            ClearWalkTarget();
            ChangeState<InteractingState>();
        }

        private void ClearWalkTarget()
        {
            WalkToTargetX = null;
            PendingInteractable = null;
        }

        public void PauseInteraction()
        {
            if (_currentState is not InteractingState) return;
            _animator.speed = 0;
        }

        public void ResumeInteraction()
        {
            if (_currentState is not InteractingState) return;
            _animator.speed = 1;
        }

        public void StopInteraction()
        {
            CurrentInteractable?.EndInteraction(this);
            CurrentInteractable = null;
            ClearWalkTarget();
            StartWalking(Direction);
        }

        public async Task Birth()
        {
            ChangeState<BirthState>();
            BirthTcs = new TaskCompletionSource<bool>();
            await BirthTcs.Task;
        }

        public MoñecoSaveData CaptureState()
        {
            string machineId = null;
            if (_currentState is InteractingState && CurrentInteractable is MonoBehaviour mb)
            {
                if (mb is SingleMoñecoCreatingMachineGameObject machine)
                    machineId = machine.SaveId;
                else if (mb is RepairableComputerGameObject computer)
                    machineId = computer.SaveId;
            }

            return new MoñecoSaveData
            {
                x = transform.position.x,
                y = transform.position.y,
                direction = Direction,
                isInteracting = _currentState is InteractingState,
                assignedMachineId = machineId,
            };
        }

        public void RestoreInteraction(Interactable interactable, int savedDirection)
        {
            _restored = true;
            Direction = savedDirection;
            CurrentInteractable = interactable;
            PendingInteractable = null;
            WalkToTargetX = null;
            ChangeState<InteractingState>();
        }

        public void RestoreWalking(int savedDirection)
        {
            _restored = true;
            Direction = savedDirection;
            StartWalking(savedDirection);
        }

        private void OnDrawGizmosSelected()
        {
            if (feetPosition == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(feetPosition.position, feetPosition.position + Vector3.down * groundCheckDistance);
        }
    }
}
```

---

### 6. Actualizar MoñecoInstantiator

**Ruta:** `Assets/Runtime/Infraestructure/MoñecoInstantiator.cs`

```csharp
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class MoñecoInstantiator
    {
        private readonly GameObject _moñecoPrefab;

        [Inject] private readonly DiContainer _container;
        [Inject] private readonly BagOfMoñecos _bagOfMoñecos;
        [Inject] private readonly MoñecosSaveHandler _saveHandler;

        public MoñecoInstantiator(GameObject prefab)
        {
            _moñecoPrefab = prefab;
        }

        public async Task<MoñecoMonoBehaviour> GiveBirth(Vector3 positionToSpawn)
        {
            var moñeco = _container.InstantiatePrefabForComponent<MoñecoMonoBehaviour>(
                _moñecoPrefab,
                positionToSpawn,
                Quaternion.identity,
                null
            );
            _saveHandler.Track(moñeco);
            await moñeco.Birth();
            _bagOfMoñecos.Add();
            return moñeco;
        }

        public MoñecoMonoBehaviour Spawn(Vector3 position)
        {
            var moñeco = _container.InstantiatePrefabForComponent<MoñecoMonoBehaviour>(
                _moñecoPrefab,
                position,
                Quaternion.identity,
                null
            );
            _saveHandler.Track(moñeco);
            return moñeco;
        }
    }
}
```

---

## Estructura de carpetas final

```
Assets/Runtime/
├── Infraestructure/
│   ├── MoñecoMonoBehaviour.cs        (modificado)
│   ├── MoñecoInstantiator.cs         (modificado)
│   └── MoñecoStates/                  (nuevo)
│       ├── IMoñecoState.cs
│       ├── AnimHashes.cs
│       ├── IdleState.cs
│       ├── WalkingState.cs
│       ├── FallingState.cs
│       ├── LandingState.cs
│       ├── TurningState.cs
│       ├── BirthState.cs
│       ├── GoToBagState.cs
│       └── InteractingState.cs
└── Main/
    └── MoñecoStatesInstaller.cs       (nuevo)
```

---

## Para añadir un estado nuevo

1. Crear clase `NewState.cs` en `MoñecoStates/`:
```csharp
public class NewState : IMoñecoState
{
    public int? GetAnimationHash(MoñecoMonoBehaviour m) => AnimHashes.New;
    public void OnComplete(MoñecoMonoBehaviour m) => m.ChangeState<WalkingState>();
}
```

2. Añadir hash en `AnimHashes.cs`:
```csharp
public static readonly int New = Animator.StringToHash("New");
```

3. Registrar en `MoñecoStatesInstaller.cs`:
```csharp
Container.Bind<NewState>().AsSingle();
```

4. Usar donde necesites:
```csharp
moñeco.ChangeState<NewState>();
```

**No hay que tocar:** enums, switches, ni crear métodos `OnXxxComplete`.

---

## Verificación

1. **Compilación:** El proyecto debe compilar sin errores
2. **Play mode:**
   - El moñeco debe nacer correctamente (Birth → Air → Walking/Falling)
   - Debe caminar y girar al chocar con paredes
   - Debe caer cuando no hay suelo
   - Debe interactuar con máquinas
   - GoToBag debe destruir el objeto
3. **Save/Load:** Guardar y restaurar debe funcionar igual que antes
