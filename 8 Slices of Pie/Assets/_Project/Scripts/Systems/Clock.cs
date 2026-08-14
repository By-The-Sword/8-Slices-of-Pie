using UnityEngine;

/// <summary>
/// O relógio da noite. Começa às 22:00 e avança uma hora por fatia coletada, virando
/// 23:00 → 00:00 → 01:00 até 06:00 na oitava — o fim da noite (GDD, seção 05).
/// Serve de contador de progresso pra HUD e de gatilho pra escalada do Lobo: quem precisa
/// reagir a uma hora específica escuta <see cref="OnHourChanged"/> em vez de contar fatia.
///
/// A hora é calculada a partir da contagem do <see cref="PlayerInventory"/>, e não somada
/// a cada evento: assim ela nunca dessincroniza do inventário, mesmo que uma fatia entre
/// por caminho que não seja o Collectible (resgate de puzzle, item inicial, debug).
/// Vai num objeto de cena — o mesmo que for guardar os outros sistemas da noite.
/// </summary>
public class Clock : MonoBehaviour
{
    [Header("Noite")]
    [Tooltip("Hora em que a noite começa, com 0 fatias. 22 no GDD.")]
    [Range(0, 23)]
    [SerializeField] private int startHour = 22;

    [Header("Referências")]
    [Tooltip("Vazio busca a Chapéuzinho sozinho na cena.")]
    [SerializeField] private PlayerInventory inventory;

    /// <summary>Hora cheia em formato 24h: 22, 23, 0, 1... Vira 0 depois das 23.</summary>
    public int Hour { get; private set; }

    /// <summary>Quantas horas já se passaram desde o início da noite — o mesmo que fatias.</summary>
    public int HoursElapsed { get; private set; }

    /// <summary>"22:00", pronto pra HUD.</summary>
    public string TimeText => FormatHour(Hour);

    /// <summary>(hora, texto) — dispara também no Start, pra HUD nascer já com o valor certo.</summary>
    public event System.Action<int, string> OnHourChanged;

    /// <summary>Primeiro aviso já saiu — o do Start sai mesmo sem mudança de hora.</summary>
    private bool announced;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindObjectOfType<PlayerInventory>();

        Hour = Normalize(startHour);
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnSliceCountChanged += HandleSliceCountChanged;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnSliceCountChanged -= HandleSliceCountChanged;
    }

    private void Start()
    {
        // Sem inventário o relógio fica parado na hora inicial: é cena de teste, não erro fatal.
        if (inventory == null)
        {
            Debug.LogWarning($"[Clock] '{name}' não achou um PlayerInventory: o relógio não vai avançar.", this);
            Apply(0);
            return;
        }

        Apply(inventory.SliceCount);
    }

    private void HandleSliceCountChanged(int slices, int total) => Apply(slices);

    /// <summary>Recalcula a hora e só avisa quem escuta se ela realmente mudou.</summary>
    private void Apply(int slices)
    {
        int elapsed = Mathf.Max(0, slices);
        int hour = Normalize(startHour + elapsed);

        bool changed = hour != Hour || elapsed != HoursElapsed;

        HoursElapsed = elapsed;
        Hour = hour;

        // O Start avisa de qualquer jeito, senão a HUD nasceria vazia esperando a 1ª fatia.
        if (changed || !announced)
        {
            announced = true;
            OnHourChanged?.Invoke(Hour, TimeText);
        }
    }

    /// <summary>Dobra a volta do dia: 24 vira 0, 25 vira 1.</summary>
    private static int Normalize(int hour) => ((hour % 24) + 24) % 24;

    private static string FormatHour(int hour) => $"{hour:00}:00";
}
