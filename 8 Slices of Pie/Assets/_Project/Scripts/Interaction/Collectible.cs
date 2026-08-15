using UnityEngine;

/// <summary>
/// Quem recebe o que a Chapéuzinho cata do chão. O inventário ainda não existe —
/// quando existir, é ele que implementa isto no mesmo objeto do PlayerInteractor.
/// </summary>
public interface IItemCollector
{
    /// <summary>Devolve false pra recusar (cheio, item duplicado): aí o objeto fica no chão.</summary>
    bool TryCollect(Collectible item);
}

/// <summary>
/// Objeto catável: fatia, chave, bateria, papel. Entra no raio do E, ela aperta,
/// o objeto some e vai pro inventário. Enquanto o inventário não estiver pronto,
/// ele some do mesmo jeito e avisa por evento — nada aqui depende dele pra rodar.
/// </summary>
public class Collectible : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [Tooltip("Chave que o inventário vai usar pra identificar o item. Ex.: fatia, chave_porao.")]
    [SerializeField] private string itemId = "item";

    [Tooltip("Nome que aparece pra jogadora no prompt.")]
    [SerializeField] private string displayName = "item";

    [Min(1)]
    [SerializeField] private int amount = 1;

    [Header("Prompt")]
    [Tooltip("Vazio monta 'Pegar <nome>' sozinho.")]
    [SerializeField] private string promptOverride;

    [Header("Ao coletar")]
    [Tooltip("Desmarque pra só esconder o objeto — útil pra quem precisa reaparecer depois.")]
    [SerializeField] private bool destroyOnCollect = true;

    [Tooltip("Som de pegar. Sai pelo AudioSource de quem catou, e não daqui: este objeto é " +
             "destruído no mesmo quadro e levaria o som junto. Por item, pra chave poder " +
             "soar diferente da fatia.")]
    [SerializeField] private AudioClip pickupClip;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public int Amount => amount;

    public string Prompt => string.IsNullOrEmpty(promptOverride)
        ? $"Pegar {displayName}"
        : promptOverride;

    /// <summary>Já foi pego — evita o E duplo no mesmo frame do Destroy.</summary>
    public bool Collected { get; private set; }

    /// <summary>(item, quem pegou) — a UI e o contador de fatias escutam isto.</summary>
    public static event System.Action<Collectible, GameObject> OnAnyCollected;

    private void Awake()
    {
        // Sem collider ele nunca entra no OverlapCircle do PlayerInteractor: ficaria invisível.
        if (GetComponentInChildren<Collider2D>() == null)
            Debug.LogWarning($"[Collectible] '{name}' não tem Collider2D: o E nunca vai achar ele.", this);
    }

    public bool CanInteract(GameObject interactor) => !Collected;

    public void Interact(GameObject interactor)
    {
        if (Collected)
            return;

        // Inventário ainda não implementado: cata do mesmo jeito e deixa o evento avisar.
        IItemCollector collector = interactor.GetComponentInParent<IItemCollector>();
        if (collector != null && !collector.TryCollect(this))
            return;

        Collected = true;
        PlayPickup(interactor);
        OnAnyCollected?.Invoke(this, interactor);

        if (destroyOnCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void PlayPickup(GameObject interactor)
    {
        if (pickupClip == null || interactor == null)
            return;

        AudioSource source = interactor.GetComponentInParent<AudioSource>();
        if (source != null)
            source.PlayOneShot(pickupClip);
    }

    private void Reset()
    {
        displayName = name;
        itemId = name.ToLowerInvariant().Replace(' ', '_');
    }
}
