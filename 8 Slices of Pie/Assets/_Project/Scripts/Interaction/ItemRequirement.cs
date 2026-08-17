using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A tranca de um interagível: *"isto só funciona pra quem estiver com o galho na mão"*. A
/// porta do porão e a torta em cima da árvore são o mesmo problema — falta um item, e o E não
/// pode simplesmente não fazer nada. Só muda o que acontece depois de destrancar, então a
/// regra mora aqui e o <see cref="Collectible"/> e o <see cref="Teleporter"/> só a carregam
/// num campo.
///
/// Não é um componente: é um campo <c>[SerializeField]</c> dentro deles, que aparece no
/// Inspector como uma seção dobrável.
///
/// A regra de ouro pra quem usa: <b>trancado continua respondendo ao E</b>. Deixe o
/// <c>CanInteract</c> devolver true e recuse dentro do <c>Interact</c>. Se o objeto recusasse o
/// foco, o <see cref="PlayerInteractor"/> passaria por cima dele, o prompt sumiria e a
/// Chapéuzinho ficaria olhando pra uma torta muda, sem pista de que existe um galho no mapa.
/// </summary>
[System.Serializable]
public class ItemRequirement
{
    [Tooltip("itemId que destranca — o mesmo do Collectible que serve de ferramenta ou chave. " +
             "Vazio deixa o objeto sempre liberado.")]
    [SerializeField] private string itemId;

    [Tooltip("Quantos desse item ela precisa carregar.")]
    [Min(1)]
    [SerializeField] private int amount = 1;

    [Tooltip("Gasta o item no uso. Uma chave que abre uma porta só; um galho que quebra ao " +
             "derrubar a torta. Desmarcado ela continua com ele pro próximo puzzle.")]
    [SerializeField] private bool consume;

    [Tooltip("O prompt curto enquanto falta o item, no lugar do normal. É por aqui que ela " +
             "descobre que o objeto existe e que falta alguma coisa.")]
    [SerializeField] private string lockedPrompt = "Está trancado";

    [Tooltip("O recado maior, que aparece no canvas quando ela tenta mesmo assim: \"preciso " +
             "de um galho pra alcançar\". Vazio repete o prompt acima.")]
    [SerializeField] private string lockedMessage;

    [Tooltip("Som da recusa. Sai pelo AudioSource de quem tentou, e não do objeto trancado: " +
             "ele pode não ter um.")]
    [SerializeField] private AudioClip lockedClip;

    [Tooltip("O que mais acontece na tentativa frustrada: a câmera tremendo, a torta " +
             "balançando no galho, uma fala dela.")]
    [SerializeField] private UnityEvent onLockedAttempt;

    /// <summary>
    /// Trava extra pra condição que não é item — uma alavanca, uma hora do relógio, um NPC que
    /// ainda não falou. Ligue e desligue pelo <c>SetLocked(bool)</c> do componente dono, que dá
    /// pra chamar de qualquer <see cref="UnityEvent"/> da cena. Não é serializada: a cena
    /// começa sempre destravada por aqui.
    /// </summary>
    public bool Blocked { get; set; }

    public ItemRequirement() { }

    /// <summary>Pra quem usa muda o texto de fábrica: uma torta fora de alcance não está
    /// "trancada", ela está alta. O Inspector continua mandando por cima.</summary>
    public ItemRequirement(string defaultLockedPrompt) => lockedPrompt = defaultLockedPrompt;

    /// <summary>O prompt curto de quando está trancado.</summary>
    public string LockedPrompt => lockedPrompt;

    /// <summary>O recado do canvas. Cai no <see cref="LockedPrompt"/> quando não foi escrito.</summary>
    public string LockedMessage => string.IsNullOrEmpty(lockedMessage) ? lockedPrompt : lockedMessage;

    /// <summary>
    /// (quem tentou, o recado) — uma tentativa recusada, de qualquer objeto trancado do mapa.
    /// O <see cref="InteractPrompt"/> escuta isto: escrever o recado na tela não precisa de
    /// nenhuma ligação no Inspector, objeto por objeto.
    /// </summary>
    public static event System.Action<GameObject, string> OnAnyDenied;

    /// <summary>Falta o que é preciso. Sem <see cref="itemId"/> e sem <see cref="Blocked"/>
    /// nunca está trancado.</summary>
    public bool IsLockedFor(GameObject interactor)
    {
        if (Blocked)
            return true;

        if (string.IsNullOrEmpty(itemId))
            return false;

        PlayerInventory inventory = InventoryOf(interactor);

        // Quem não tem inventário não tem como provar que achou o item: fica trancado.
        return inventory == null || !inventory.HasItem(itemId, amount);
    }

    /// <summary>A recusa: o som, o UnityEvent da cena e o recado pra tela. Nada acontece
    /// além disso — quem chamou já saiu do caminho.</summary>
    public void Deny(GameObject interactor)
    {
        Play(interactor, lockedClip);
        onLockedAttempt?.Invoke();
        OnAnyDenied?.Invoke(interactor, LockedMessage);
    }

    /// <summary>Cobra o item, se for pra gastar. Chame depois do uso ter dado certo.</summary>
    public void Spend(GameObject interactor)
    {
        if (consume && !string.IsNullOrEmpty(itemId))
            InventoryOf(interactor)?.Consume(itemId, amount);
    }

    /// <summary>Avisa no Awake do dono o que ficou meio configurado.</summary>
    public void Warn(MonoBehaviour owner)
    {
        // Sem item exigido não há nada pra gastar: quem marcou isto queria uma tranca.
        if (consume && string.IsNullOrEmpty(itemId))
            Debug.LogWarning($"[{owner.GetType().Name}] '{owner.name}' gasta o item mas não exige " +
                             "nenhum: preencha o Item Id da tranca.", owner);
    }

    private static PlayerInventory InventoryOf(GameObject interactor) =>
        interactor != null ? interactor.GetComponentInParent<PlayerInventory>() : null;

    /// <summary>Toca pelo AudioSource de quem tentou — o objeto trancado pode não ter um.</summary>
    private static void Play(GameObject who, AudioClip clip)
    {
        if (clip == null || who == null)
            return;

        AudioSource source = who.GetComponentInChildren<AudioSource>();
        if (source != null)
            source.PlayOneShot(clip);
    }
}
