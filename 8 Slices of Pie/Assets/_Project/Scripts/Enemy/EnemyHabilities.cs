using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Uma linha da tabela de evolução do Lobo (GDD, seção 05). Cada estágio descreve o Lobo
/// <b>inteiro</b> naquela hora, e não o que mudou: aplicar um estágio é idempotente, então
/// morrer e recomeçar a noite, ou carregar um save, é só reaplicar o estágio da vez.
/// </summary>
[System.Serializable]
public class WolfStage
{
    [Tooltip("Só pra ler no Inspector — o nome do evento daquela hora no GDD.")]
    public string label = "";

    [Header("Presença")]
    [Tooltip("Está no mapa. Falso até a 3ª fatia e de novo na 8ª, quando ele desaparece.")]
    public bool present;

    [Header("Percepção")]
    public float senseRadius = 6f;
    public float sightRadius = 10f;
    public float chaseSpeed = 4f;

    [Header("Mordida")]
    [Min(0)]
    [Tooltip("Corações por mordida. Vira 2 na 5ª fatia, quando ele cria garras.")]
    public int biteDamage = 1;

    [Header("Lampião")]
    [Tooltip("Foge do círculo de luz. Desliga na 7ª fatia.")]
    public bool fearsLight = true;

    [Tooltip("Inversão da 7ª: a luz acesa vira isca e o chama de longe.")]
    public bool attractedByLight;

    [Header("Rastreio")]
    [Tooltip("Segue a trilha de sangue dela (4ª fatia). O sistema de trilha ainda não existe — " +
             "por enquanto quem representa isso é o senseRadius aberto deste estágio.")]
    public bool tracksBlood;

    [Header("Ambientação")]
    [Tooltip("Som da virada de hora: o uivo distante da 1ª, por exemplo.")]
    public AudioClip cue;

    [Tooltip("O que não é do Lobo: postes piscando, luzes da cidade apagando, sangue nas ruas. " +
             "Ligue aqui os objetos de cena que fazem cada um desses eventos.")]
    public UnityEvent onReached;
}

/// <summary>
/// As evoluções do Lobo Mau. O <see cref="EnemyMov"/> sabe <i>como</i> caçar e o
/// <see cref="EnemyAtk"/> sabe morder; quem decide o <i>quanto</i> em cada hora da noite é
/// este script — a curva de dificuldade inteira do jogo mora aqui, e é a única do jogo (GDD).
///
/// Escuta o <see cref="Clock"/> e, a cada fatia, aplica a linha correspondente de
/// <see cref="stages"/>: presença no mapa, alcances, velocidade, dano da mordida e a
/// relação com o lampião. O que não é do Lobo — postes piscando, luzes da cidade apagando,
/// sangue nas ruas — sai pelo <see cref="WolfStage.onReached"/>, pra cena ligar sem código.
///
/// Vai no mesmo objeto do EnemyMov, no Wolf.prefab.
/// </summary>
[RequireComponent(typeof(EnemyMov))]
public class EnemyHabilities : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Vazio busca o relógio sozinho na cena.")]
    [SerializeField] private Clock clock;

    [Tooltip("Onde ele entra no mapa na 3ª fatia. Vazio deixa ele onde já estiver.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Opcional — toca o som da virada de hora (o uivo distante da 1ª).")]
    [SerializeField] private AudioSource audioSource;

    [Header("Isca de luz (7ª fatia)")]
    [Tooltip("De quanto em quanto tempo o lampião aceso chama o Lobo. Ele é chamado pelo mesmo " +
             "canal de ruído dos passos, então a atração respeita a IA que já existe.")]
    [SerializeField] private float lightLureInterval = 0.5f;

    [Tooltip("Alcance do chamado da luz. Bem maior que o passo (6): é pra ele vir de longe.")]
    [SerializeField] private float lightLureRadius = 25f;

    [Header("Tabela do GDD")]
    [Tooltip("Índice = fatias coletadas. 0 é a noite começando às 22:00, 8 é o fim, às 06:00.")]
    [SerializeField]
    private WolfStage[] stages =
    {
        new WolfStage { label = "22:00 · 0 fatias — uivo distante, sem perigo", present = false },
        new WolfStage { label = "23:00 · Uivo distante audível", present = false },
        new WolfStage { label = "00:00 · Postes da cidade piscam", present = false },
        new WolfStage { label = "01:00 · Luzes se apagam · o Lobo entra no mapa", present = true },
        new WolfStage { label = "02:00 · Sente o cheiro do sangue dela", present = true,
                        senseRadius = 12f, tracksBlood = true },
        new WolfStage { label = "03:00 · Cria garras", present = true,
                        senseRadius = 12f, tracksBlood = true, biteDamage = 2 },
        new WolfStage { label = "04:00 · A cidade amanhece coberta de sangue", present = true,
                        senseRadius = 12f, tracksBlood = true, biteDamage = 2 },
        new WolfStage { label = "05:00 · Deixa de temer a luz — ela passa a atraí-lo", present = true,
                        senseRadius = 12f, tracksBlood = true, biteDamage = 2,
                        fearsLight = false, attractedByLight = true },
        new WolfStage { label = "06:00 · Desaparece do mapa", present = false,
                        senseRadius = 12f, tracksBlood = true, biteDamage = 2,
                        fearsLight = false, attractedByLight = true }
    };

    /// <summary>Índice do estágio em vigor — o mesmo número de fatias já coletadas.</summary>
    public int StageIndex { get; private set; } = -1;

    public WolfStage CurrentStage => GetStage(StageIndex);

    /// <summary>Está no mapa. Fora dele ele não anda, não morde e não aparece.</summary>
    public bool IsPresent { get; private set; }

    /// <summary>Fareja a trilha de sangue (4ª fatia em diante). Pro sistema de trilha, quando existir.</summary>
    public bool TracksBlood { get; private set; }

    /// <summary>(índice, estágio) — dispara também na entrada, pra quem escuta nascer sincronizado.</summary>
    public event System.Action<int, WolfStage> OnStageChanged;

    private EnemyMov movement;
    private EnemyAtk attack;
    private Lantern lantern;
    private Renderer[] renderers;
    private Collider2D[] colliders;
    private float lureTimer;

    private void Awake()
    {
        movement = GetComponent<EnemyMov>();
        attack = GetComponent<EnemyAtk>();

        // Guardados uma vez: são eles que somem do mapa antes da 3ª fatia e depois da 8ª.
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);

        if (clock == null)
            clock = FindObjectOfType<Clock>();
    }

    private void OnEnable()
    {
        if (clock != null)
            clock.OnHourChanged += HandleHourChanged;
    }

    private void OnDisable()
    {
        if (clock != null)
            clock.OnHourChanged -= HandleHourChanged;
    }

    private void Start()
    {
        // Sem relógio ele congela no estágio 0: cena de teste roda, só não evolui.
        if (clock == null)
        {
            Debug.LogWarning($"[EnemyHabilities] '{name}' não achou um Clock: o Lobo não vai evoluir.", this);
            ApplyStage(0, silent: true);
            return;
        }

        // silent: o estágio de entrada não é uma virada de hora — não toca uivo nem pisca poste.
        ApplyStage(clock.HoursElapsed, silent: true);
    }

    private void Update()
    {
        TickLightLure();
    }

    /// <summary>
    /// A inversão da 7ª fatia. Em vez de mexer na IA, o lampião aceso emite ruído pelo mesmo
    /// canal dos passos: o Lobo entra em SUSPEITA e vem até a luz por conta própria. Assim a
    /// isca não some quando ele está perseguindo ou fugindo — o EnemyMov já ignora ruído nessas
    /// horas de propósito, e essa regra continua valendo.
    /// </summary>
    private void TickLightLure()
    {
        if (!IsPresent || !CurrentStage.attractedByLight || !ResolveLantern())
            return;

        if (!lantern.IsOn)
        {
            // Apagou: o próximo acender chama na hora, sem esperar o resto do intervalo.
            lureTimer = 0f;
            return;
        }

        lureTimer -= Time.deltaTime;
        if (lureTimer > 0f)
            return;

        lureTimer = Mathf.Max(0.1f, lightLureInterval);
        Noise.Emit(lantern.LightCenter, lightLureRadius, lantern.gameObject);
    }

    /// <summary>
    /// O lampião dela, achado tarde: o EnemyMov só resolve a Chapéuzinho no Start dele, e a
    /// ordem de Start entre componentes do mesmo objeto não é garantida — mesmo motivo pelo
    /// qual o EnemyAtk resolve o alvo dele no Update.
    /// </summary>
    private bool ResolveLantern()
    {
        if (lantern != null)
            return true;

        if (movement.Target != null)
            lantern = movement.Target.GetComponent<Lantern>();

        return lantern != null;
    }

    private void HandleHourChanged(int hour, string text) => ApplyStage(clock.HoursElapsed, silent: false);

    /// <summary>
    /// Põe o Lobo no estágio pedido. Idempotente de propósito: reaplicar o mesmo estágio não
    /// muda nada, o que deixa recomeço de noite e carregamento de save saírem de graça.
    /// </summary>
    public void ApplyStage(int index, bool silent = false)
    {
        index = Mathf.Clamp(index, 0, Mathf.Max(0, stages.Length - 1));

        if (index == StageIndex)
            return;

        WolfStage stage = GetStage(index);
        StageIndex = index;

        movement.SenseRadius = stage.senseRadius;
        movement.SightRadius = stage.sightRadius;
        movement.ChaseSpeed = stage.chaseSpeed;
        movement.FearsLight = stage.fearsLight;

        if (attack != null)
            attack.Damage = stage.biteDamage;

        TracksBlood = stage.tracksBlood;
        SetPresent(stage.present);

        // A ambientação da hora só sai quando a hora vira de verdade.
        if (!silent)
        {
            if (audioSource != null && stage.cue != null)
                audioSource.PlayOneShot(stage.cue);

            stage.onReached?.Invoke();
        }

        OnStageChanged?.Invoke(StageIndex, stage);
    }

    /// <summary>
    /// Entra no mapa ou some dele. Fora do mapa a IA fica travada e o corpo desaparece —
    /// o objeto continua vivo pra poder voltar na hora certa, sem depender de spawn.
    /// </summary>
    private void SetPresent(bool present)
    {
        bool entering = present && !IsPresent;
        IsPresent = present;

        // Ele chega pela borda do mapa, e não do lugar onde o prefab ficou parado na cena.
        if (entering && spawnPoint != null)
            transform.position = spawnPoint.position;

        movement.MovementLocked = !present;

        if (attack != null)
            attack.enabled = present;

        foreach (Renderer r in renderers)
            if (r != null)
                r.enabled = present;

        foreach (Collider2D c in colliders)
            if (c != null)
                c.enabled = present;
    }

    private WolfStage GetStage(int index)
    {
        if (stages == null || stages.Length == 0)
            return new WolfStage();

        return stages[Mathf.Clamp(index, 0, stages.Length - 1)];
    }
}
