# Scripts do player — 8 Slices of Pie

## Montagem

```
Player            ← Rigidbody2D + Collider2D + AudioSource
                    todos os scripts do player ficam aqui, na raiz
├── Sprite        ← SpriteRenderer + Animator
└── LanternLight  ← Light2D (arrastar pro campo `lampLight` do Lantern)
```

`Noise` e `IInteractable` não são componentes — o primeiro é classe estática,
o segundo é interface. Não vão em GameObject nenhum.

## Som

Tudo por Inspector, sem mexer em código. Os campos são opcionais e o jogo roda vazio.

- **Passos** → `PlayerFootsteps`: `stepClips` (array, ele sorteia um), `audioSource`,
  `stepInterval` (ritmo), `crouchVolume`.
- **Lampião** → `Lantern`: `toggleOnClip`, `toggleOffClip`, `batterySwapClip`.

## Animação

O `PlayerAnimatorBridge` escreve no Animator do filho `Sprite`. Os nomes têm que bater:
`MoveX` e `MoveY` (Float, sempre -1, 0 ou 1) · `IsMoving` e `IsCrouched` (Bool).

4 estados — `Idle`, `Walk`, `CrouchIdle`, `CrouchWalk` — cada um um Blend Tree
*2D Simple Directional* com os clipes em `(0,-1) (0,1) (-1,0) (1,0)`.
Ligar por `Any State`, com `Has Exit Time` desmarcado, duração 0 e
**`Can Transition To Self` desmarcado** (senão a animação congela no 1º quadro).

## Itens do mapa

Implemente `IInteractable`. O objeto precisa de `Collider2D` com **Is Trigger**
e da layer marcada em `interactableLayers` do `PlayerInteractor`.

```csharp
public class BatteryPickup : MonoBehaviour, IInteractable
{
    public string Prompt => "Pegar bateria";
    public bool CanInteract(GameObject interactor) => true;

    public void Interact(GameObject interactor)
    {
        interactor.GetComponent<Lantern>()?.AddBattery();
        Destroy(gameObject);
    }
}
```

`Noise.Emit(pos, raio, source)` faz barulho que o Lobo escuta.

## IA do Lobo

- `Lantern.IsOn` — recua com a luz acesa **até a 7ª fatia**; depois a luz atrai.
- `PlayerController.IsCrouched` — agachada ele não a percebe.
- `Noise.OnNoise` — assinar para o estado SUSPEITA (com `-=` no `OnDisable`).
- `playerHealth.TakeDamage(1, transform.position)` — 2 a partir da 5ª fatia.

## Balanceamento

`moveSpeed` 3.5 · `crouchSpeedMultiplier` 0.5 · `stepNoiseRadius` 6 ·
`invulnerabilityDuration` 1s · `Outer Radius` da luz ~5.

## Setup de projeto

- Sprites precisam do material **`Sprite-Lit-Default`** pra reagir à luz.
- Light2D do lampião: **Spot** com Inner/Outer Angle em **360** (senão vira cone).
- `Graphics > Transparency Sort Mode` = Custom Axis, eixo `(0,1,0)` — Y-sorting.

> HUD/pulseira ainda não definida — a combinar com quem for fazer.
> Os scripts já expõem tudo por evento, então nada aqui precisa mudar.
