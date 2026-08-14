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

## Relógio da noite

`Clock` vai num objeto de cena (não no Player) e lê o `PlayerInventory` sozinho.
Começa em `22:00` e avança 1h por fatia — `23:00`, `00:00`, `01:00`... até `06:00`
na oitava. Quem precisa reagir a uma hora escuta `OnHourChanged(hora, texto)` em vez de
contar fatia. `clock.TimeText` já vem formatado pra HUD; `clock.HoursElapsed` é o
progresso (0–8). Quem já escuta ele: a HUD e o `EnemyHabilities`.

## Evolução do Lobo

`EnemyHabilities` (no Wolf.prefab, junto do EnemyMov) é a tabela da seção 05 do GDD em
forma de componente: um `WolfStage` por fatia, índice 0–8. A cada hora ele reescreve o
Lobo inteiro — presença no mapa, `SenseRadius`, `SightRadius`, `ChaseSpeed`,
`EnemyAtk.Damage` e `FearsLight`. Aplicar um estágio é idempotente: recomeçar a noite é
só chamar `ApplyStage(0)`.

O que **não** é do Lobo — postes piscando (2ª), luzes da cidade apagando (3ª), sangue nas
ruas (6ª) — sai pelo `UnityEvent onReached` de cada estágio: ligue o objeto de cena no
Inspector, sem código. O `cue` toca o som da hora (o uivo distante da 1ª).

Dois campos de cena valem configurar: `spawnPoint`, por onde ele entra na 3ª fatia, e o
`audioSource`. Fora do mapa (antes da 3ª e depois da 8ª) ele não é destruído — a IA trava
e renderers e colliders desligam, pra ele poder voltar.

## IA do Lobo

- `Lantern.Illuminates(ponto)` — já ligado: dentro do círculo aceso ele entra em RECUO e
  não morde. Na 7ª fatia o `EnemyHabilities` desliga a regra e passa a emitir ruído na
  posição da luz acesa, que é como ela vira isca. O raio que vale é o **Outer Radius**
  do Light2D.
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
