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
///
/// Pode exigir uma ferramenta: a torta em cima da árvore com o <see cref="requirement"/>
/// pedindo o galho não é coletada sem ele — o prompt vira "Não alcanço" e o E só solta o
/// recado no canvas. Veja o <see cref="ItemRequirement"/>.
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

    [Header("Tranca")]
    [Tooltip("Item que ela precisa estar carregando pra conseguir pegar este — o galho pra " +
             "alcançar a torta da árvore. Item Id vazio deixa o objeto livre.")]
    [SerializeField] private ItemRequirement requirement = new ItemRequirement("Não alcanço");

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

    // Montado uma vez: o PlayerInteractor relê o Prompt todo quadro em que o item está em
    // foco, e um $"..." aqui seria lixo novo pro coletor a cada um deles.
    private string composedPrompt;

    /// <summary>
    /// Estado da tranca visto no último <see cref="CanInteract"/>. O <see cref="Prompt"/> não
    /// recebe quem está perguntando, e é o <see cref="PlayerInteractor"/> que lê os dois —
    /// sempre CanInteract primeiro, no mesmo quadro, com o mesmo interator.
    /// </summary>
    private bool lockedForLastAsked;

    public string Prompt => lockedForLastAsked ? requirement.LockedPrompt : PickupPrompt;

    private string PickupPrompt => string.IsNullOrEmpty(promptOverride)
        ? composedPrompt ??= $"Pegar {displayName}"
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

        requirement.Warn(this);
    }

    /// <summary>
    /// A tranca <b>não</b> entra aqui: um item fora de alcance aceita o E e responde que falta
    /// a ferramenta. Recusar o foco só o deixaria mudo — veja o <see cref="ItemRequirement"/>.
    /// </summary>
    public bool CanInteract(GameObject interactor)
    {
        lockedForLastAsked = requirement.IsLockedFor(interactor);
        return !Collected;
    }

    /// <summary>Trava e destrava por condição que não é item, pra ligar num UnityEvent da cena.</summary>
    public void SetLocked(bool locked) => requirement.Blocked = locked;

    public void Interact(GameObject interactor)
    {
        if (Collected)
            return;

        if (requirement.IsLockedFor(interactor))
        {
            requirement.Deny(interactor);
            return;
        }

        // Inventário ainda não implementado: cata do mesmo jeito e deixa o evento avisar.
        IItemCollector collector = interactor.GetComponentInParent<IItemCollector>();
        if (collector != null && !collector.TryCollect(this))
            return;

        Collected = true;
        requirement.Spend(interactor);
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
