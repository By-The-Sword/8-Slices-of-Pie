using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A HUD diegética: o braço da Chapéuzinho, com o relógio, a pulseira de corações e as
/// baterias amarradas. Não fica na tela — ela levanta o braço junto com a pausa, então
/// conferir a hora é o mesmo gesto de abrir o menu: ESC mostra o braço na direita e as
/// opções na esquerda. Substitui a HUD de cantos do GDD.
///
/// Não lê teclado: quem escuta o ESC é o <see cref="PauseMenu"/>, e o braço só segue o
/// <see cref="PauseMenu.IsPaused"/> dele. Sem PauseMenu na cena o braço fica abaixado.
///
/// Vai no objeto raiz do braço, dentro de um Canvas Screen Space – Overlay. A posição em
/// que ele estiver no Editor é a de braço levantado; a de escondido é ela mais o
/// <see cref="hiddenOffset"/>.
///
/// Só troca sprite quando um evento chega — nada aqui varre estado por frame.
/// </summary>
public class ArmHUD : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Onde fica o braço abaixado, a partir da posição de repouso, em unidades do " +
             "Canvas. Uma altura de quadro (144) já tira o braço todo da tela.")]
    [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -144f);

    [Tooltip("Tempo do sobe-e-desce, em segundos.")]
    [SerializeField] private float slideSeconds = 0.18f;

    [Header("Mostradores")]
    [Tooltip("As Image filhas. Cada uma pode ficar vazia enquanto a arte não estiver montada.")]
    [SerializeField] private Image clockImage;
    [SerializeField] private Image braceletImage;
    [SerializeField] private Image batteryImage;

    [Header("Sprites")]
    [Tooltip("Na ordem da folha: 23:00, 00:00... até 06:00. São as 8 fatias da noite.")]
    [SerializeField] private Sprite[] clockSprites;

    [Tooltip("Quadro de 22:00, a hora em que a noite abre. Fica fora do array porque a " +
             "folha do relógio começa em 23:00 — é o clock22.png. Vazio, ela sai de casa " +
             "com o relógio apagado.")]
    [SerializeField] private Sprite nightStartSprite;

    [Tooltip("Na ordem da folha, do cheio pro vazio: 3, 2 e 1 coração. Um quadro por coração.")]
    [SerializeField] private Sprite[] braceletSprites;

    [Tooltip("Na ordem da folha, do cheio pro vazio: 4, 3, 2, 1 e 0 baterias reserva — " +
             "as 4 que o GDD espalha pelo mapa, um slot pra cada.")]
    [SerializeField] private Sprite[] batterySprites;

    [Header("Referências")]
    [Tooltip("Todas opcionais — vazio ele acha sozinho na cena.")]
    [SerializeField] private Clock clock;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Lantern lantern;
    [SerializeField] private PauseMenu pauseMenu;

    /// <summary>0 abaixado, 1 levantado. O meio é o deslize.</summary>
    public float Raise { get; private set; }

    private RectTransform rect;
    private Vector2 restPosition;
    private float appliedRaise = -1f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogError($"[ArmHUD] '{name}' não é um objeto de UI: precisa de RectTransform, dentro de um Canvas.", this);
            enabled = false;
            return;
        }

        restPosition = rect.anchoredPosition;

        if (clock == null)
            clock = FindObjectOfType<Clock>();

        if (health == null)
            health = FindObjectOfType<PlayerHealth>();

        if (lantern == null)
            lantern = FindObjectOfType<Lantern>();

        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenu>();

        // Sem o PauseMenu não há ESC, e o braço não teria como subir — este é o único
        // que faz falta de verdade.
        if (pauseMenu == null)
            Debug.LogWarning($"[ArmHUD] '{name}' não achou um PauseMenu: o braço não vai levantar.", this);

        // Já os outros três só deixam mostradores parados: é cena de teste, não erro fatal.
        if (clock == null)
            Debug.LogWarning($"[ArmHUD] '{name}' não achou um Clock: o relógio não vai andar.", this);

        if (health == null)
            Debug.LogWarning($"[ArmHUD] '{name}' não achou um PlayerHealth: a pulseira não vai perder corações.", this);

        if (lantern == null)
            Debug.LogWarning($"[ArmHUD] '{name}' não achou um Lantern: as baterias não vão descarregar.", this);

        if (braceletSprites != null && braceletSprites.Length > 0 && health != null
            && braceletSprites.Length != health.MaxHearts)
        {
            Debug.LogWarning($"[ArmHUD] '{name}' tem {braceletSprites.Length} quadros de pulseira pra " +
                             $"{health.MaxHearts} corações: precisa de um quadro por coração.", this);
        }

        ApplyRaise();
    }

    private void OnEnable()
    {
        if (clock != null)
            clock.OnHourChanged += HandleHourChanged;

        if (health != null)
            health.OnHeartsChanged += HandleHeartsChanged;

        if (lantern != null)
            lantern.OnSpareBatteriesChanged += HandleSpareBatteriesChanged;
    }

    private void OnDisable()
    {
        if (clock != null)
            clock.OnHourChanged -= HandleHourChanged;

        if (health != null)
            health.OnHeartsChanged -= HandleHeartsChanged;

        if (lantern != null)
            lantern.OnSpareBatteriesChanged -= HandleSpareBatteriesChanged;
    }

    private void Start()
    {
        // Os três avisam sozinhos no Start deles, mas um braço ligado no meio da partida
        // perderia esse aviso e nasceria com o sprite errado. Aqui ele lê o estado atual.
        Refresh();
    }

    private void Update()
    {
        // Morta ela não levanta mais o braço, e o que estava levantado desce sozinho:
        // a tela de morte é do NightRunner, não é hora de conferir a hora.
        bool blocked = health != null && health.IsDead;

        // Levantado o tempo todo que a pausa durar — inclusive dentro das opções, que são
        // a mesma tela um passo adiante.
        bool showing = pauseMenu != null && pauseMenu.IsPaused;

        float target = showing && !blocked ? 1f : 0f;

        // unscaledDeltaTime pra ele terminar o movimento mesmo se a pausa cair no meio.
        float step = slideSeconds > 0f ? Time.unscaledDeltaTime / slideSeconds : 1f;
        Raise = Mathf.MoveTowards(Raise, target, step);

        ApplyRaise();
    }

    /// <summary>Relê os três sistemas e redesenha tudo. Serve de reset no recomeço da noite.</summary>
    public void Refresh()
    {
        ApplyClock();

        if (health != null)
            HandleHeartsChanged(health.Hearts, health.MaxHearts);

        if (lantern != null)
            HandleSpareBatteriesChanged(lantern.SpareBatteries);
    }

    private void HandleHourChanged(int hour, string text) => ApplyClock();

    private void ApplyClock()
    {
        if (clockImage == null)
            return;

        int slices = clock != null ? clock.HoursElapsed : 0;

        // Com 0 fatias são 22:00, hora que a folha não cobre: ela começa na 1ª fatia, às
        // 23:00. O quadro de abertura vem solto, num campo só dele.
        if (slices <= 0)
        {
            clockImage.enabled = nightStartSprite != null;

            if (nightStartSprite != null)
                clockImage.sprite = nightStartSprite;

            return;
        }

        // Daí em diante a fatia indexa a folha direto: a 1ª é o quadro 0.
        Show(clockImage, clockSprites, slices - 1);
    }

    private void HandleHeartsChanged(int hearts, int max)
    {
        // Array do cheio pro vazio: 3 corações é o quadro 0. Com 0 corações o índice cai
        // fora do array e a pulseira some — ela morreu.
        int index = braceletSprites != null ? braceletSprites.Length - hearts : -1;
        Show(braceletImage, braceletSprites, index);
    }

    /// <summary>
    /// Quantas baterias ela ainda carrega, não a carga da que está no lampião: o cinto tem
    /// um slot por bateria reserva, e eles vão vazando conforme a noite consome as 4 do mapa.
    /// </summary>
    private void HandleSpareBatteriesChanged(int spares)
    {
        // Aqui o vazio tem quadro: 4 reservas é o 0, nenhuma reserva é o último. Achar uma
        // 5ª bateria cairia fora do array pela esquerda, então o índice fica preso no 0.
        int index = batterySprites != null
            ? Mathf.Max(0, batterySprites.Length - 1 - spares)
            : -1;

        Show(batteryImage, batterySprites, index);
    }

    /// <summary>Desenha o quadro, ou apaga o mostrador se o índice não existir na folha.</summary>
    private static void Show(Image image, Sprite[] sprites, int index)
    {
        if (image == null)
            return;

        bool has = sprites != null && index >= 0 && index < sprites.Length && sprites[index] != null;

        image.enabled = has;

        if (has)
            image.sprite = sprites[index];
    }

    private void ApplyRaise()
    {
        if (Mathf.Approximately(Raise, appliedRaise))
            return;

        appliedRaise = Raise;
        rect.anchoredPosition = Vector2.Lerp(restPosition + hiddenOffset, restPosition,
                                             Mathf.SmoothStep(0f, 1f, Raise));
    }
}
