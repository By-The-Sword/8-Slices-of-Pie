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
- **Item** → `pickupClip` de cada `Collectible`. Toca pelo AudioSource de quem catou,
  porque o objeto some no mesmo quadro.
- **Interface** → `UISounds`, um por Canvas. Ele acha os `Button` filhos sozinho e pendura
  hover e clique; `pauseInClip`/`pauseOutClip` vêm do `PauseMenu`. O `startClip` é o único
  que não sai sozinho: ligue `PlayStart()` no OnClick do botão da primeira tela.
- **Grama** → `EnemyAudio`: `grassClips` só sai com o Lobo perto e **fora do quadro**.

## Animação

O `PlayerAnimatorBridge` escreve no Animator do filho `Sprite`. Os nomes têm que bater:
`MoveX` e `MoveY` (Float, sempre -1, 0 ou 1) · `IsMoving` e `IsCrouched` (Bool).

4 estados — `Idle`, `Walk`, `CrouchIdle`, `CrouchWalk` — cada um um Blend Tree
*2D Simple Directional* com os clipes em `(0,-1) (0,1) (-1,0) (1,0)`.
Ligar por `Any State`, com `Has Exit Time` desmarcado, duração 0 e
**`Can Transition To Self` desmarcado** (senão a animação congela no 1º quadro).

## Pausa

`PauseMenu` não tem código de UI: os botões chamam `Resume()`, `OpenOptions()`,
`GoToMainMenu()` e `QuitGame()` pelo `OnClick` do Inspector. O painel vai no campo
`pausePanel` e nasce desligado sozinho; `optionsPanel` pode ficar vazio.

A cena precisa de um **EventSystem**, senão nenhum botão responde a clique.

O ESC é a única tecla de menu do jogo: a mesma pausa que abre os botões levanta o braço
(veja *HUD diegética*). Por isso os botões ficam na **metade esquerda** da tela — a
direita é do braço.

Os botões vão dentro de um objeto vazio com `CanvasGroup` + `UIFadeIn`, irmão do fundo
escuro:

```
Pause          ← pausePanel do PauseMenu
├── Background ← Image preta, entra seca junto com a pausa
└── Buttons    ← CanvasGroup + UIFadeIn, os 4 botões dentro
```

`UIFadeIn` deixa o grupo transparente e o traz de volta toda vez que o objeto é ligado —
`delay` pra esperar o braço subir, `duration` pro fade. Ele conta em tempo real, senão o
`timeScale = 0` da pausa travaria o fade. Enquanto está aparecendo os botões não aceitam
clique, pra ninguém acertar o *Sair* sem ter visto onde ele estava. Serve em qualquer UI,
não só na pausa.

> O menu principal ainda não tem botões — a primeira tela é só arte.

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

O que **não** é do Lobo — sangue nas ruas (6ª) e o resto da ambientação — sai pelo
`UnityEvent onReached` de cada estágio: ligue o objeto de cena no Inspector, sem código.
O `cue` toca o som da hora (o uivo distante da 1ª). Os postes são a exceção: eles escutam
o relógio por conta própria (veja *Postes da cidade*), senão daria pra arrastar poste por
poste pro `onReached` de duas fatias.

Dois campos de cena valem configurar: `spawnPoint`, por onde ele entra na 3ª fatia, e o
`audioSource`. Fora do mapa (antes da 3ª e depois da 8ª) ele não é destruído — a IA trava
e renderers e colliders desligam, pra ele poder voltar.

## Postes da cidade

`StreetLamp` vai na raiz de cada poste, com o Light2D num filho:

```
Poste
├── Sprite     ← o poste1.aseprite
└── LampLight  ← Light2D (Spot, Inner/Outer Angle 360, como o do lampião)
```

Um script por poste, cada um achando o `Clock` sozinho — não precisa ligar nada no
Inspector. Ele é a tabela de iluminação da seção 05 do GDD:

| Fatia | Hora | Poste |
|---|---|---|
| 0–1 | 22:00 · 23:00 | aceso, a cidade é legível |
| 2 | 00:00 | piscada curta de aviso |
| 3+ | 01:00 em diante | apagado, e não volta |

As duas fatias vêm dos campos `flickerAtSlice` e `blackoutAtSlice`. O `flickerJitter`
sorteia um atraso por poste, senão a cidade inteira pisca no mesmo quadro e vira um flash
só. `alwaysOn` ignora o relógio — é a regra da conveniência, o único ponto de luz que o
GDD mantém a noite toda.

`StartFlicker()` dispara a piscada sozinha, pra testar sem coletar duas fatias.

> Poste com `Scale` diferente de 1 multiplica o raio do Light2D filho junto. Ajuste o
> *Outer Radius* no olho, ou tire a escala do pai.

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

## HUD diegética — o braço

`ArmHUD` é o braço dela: relógio, pulseira e as baterias amarradas. Não fica na tela —
sobe junto com a pausa e desce ao voltar pro jogo. Ele **não lê teclado**: só segue o
`IsPaused` do `PauseMenu`, então o **ESC** abre as duas coisas de uma vez — braço na
direita, botões na esquerda. Monte num Canvas *Screen Space – Overlay*:

```
ArmHUD      ← RectTransform + ArmHUD, na posição de braço LEVANTADO
├── Arm     ← Image, Arm.png
├── Clock   ← Image ┐
├── Bracelet ← Image ├ por cima do braço, na ordem de desenho
└── Battery ← Image ┘
```

Todo quadro é **160×144** com o elemento já posicionado: Canvas Scaler em *Scale With
Screen Size*, `160 × 144`, Match **Height**, e as 4 Images com o mesmo rect do `ArmHUD` —
aí relógio, pulseira e baterias caem sozinhos no lugar certo do braço.

Os três arrays vão **na ordem da folha**, do cheio pro vazio: relógio `23:00 → 06:00` (8),
pulseira `3 → 1` coração (3), bateria `4 → 0` reservas (5). As referências de `Clock`,
`PlayerHealth`, `Lantern` e `PauseMenu` podem ficar vazias — ele acha na cena.

Cada mostrador tem sua fonte, e todos se redesenham por evento, não por frame:

| Mostrador | Lê de | Evento |
|---|---|---|
| Relógio | `Clock.HoursElapsed` | `OnHourChanged` |
| Pulseira | `PlayerHealth.Hearts` | `OnHeartsChanged` |
| Bateria | `Lantern.SpareBatteries` | `OnSpareBatteriesChanged` |

A folha do relógio começa em **23:00**, a 1ª fatia — as 22:00 da abertura ficam de fora.
O quadro delas é o `clock22.png`, um arquivo só dele, que vai no campo `nightStartSprite`.
Vazio, ela sai de casa com o relógio apagado. A bateria conta as **reservas** — os 4 slots são as 4 baterias do mapa —, então
ela começa a noite vazia mesmo com o lampião cheio. A carga da bateria em uso
(`BarsRemaining`) não aparece em lugar nenhum: pra mostrar ela no lugar das reservas,
troque a fonte de `HandleSpareBatteriesChanged` no `ArmHUD`.
