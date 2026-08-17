using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objeto que leva quem apertou o E para outro canto do mapa: escada, bueiro, buraco na
/// cerca, porta que não abre uma cena nova. Continua sendo um <see cref="IInteractable"/>
/// comum — entra no raio do <see cref="PlayerInteractor"/>, aparece o prompt, ela aperta E.
///
/// Teleportar um corpo com física não é só mudar o <c>transform.position</c>: o
/// <see cref="Rigidbody2D"/> dela anda com <b>Interpolate</b> ligado (veja o
/// <see cref="PlayerController"/>), e interpolação desenha o caminho entre a posição velha e
/// a nova — a Chapéuzinho apareceria borrada atravessando a cidade inteira em um quadro. E a
/// câmera tem amortecimento (<see cref="CameraFollow"/>), então ela viajaria até o destino
/// mostrando todo o cenário no meio. Este script cuida das duas coisas.
///
/// Precisa de um <c>Collider2D</c> com <b>Is Trigger</b>, na layer que o
/// <see cref="PlayerInteractor"/> procura — igual a qualquer item do mapa.
///
/// Pode ficar trancada: com o <see cref="requirement"/> pedindo uma chave, ela só leva quem
/// tiver o item no <see cref="PlayerInventory"/> — o prompt vira "Está trancado" e o E só solta
/// a recusa. Veja o <see cref="ItemRequirement"/>.
/// </summary>
public class Teleporter : MonoBehaviour, IInteractable
{
    [Header("Destino")]
    [Tooltip("Para onde ela vai. Um objeto vazio na cena é o jeito mais fácil: dá pra " +
             "arrastar no editor e o gizmo mostra a linha até ele.")]
    [SerializeField] private Transform destination;

    [Tooltip("Usado só quando o campo acima está vazio: posição em coordenadas de mundo.")]
    [SerializeField] private Vector2 destinationPoint;

    [Header("Prompt")]
    [Tooltip("O que aparece na tela quando ela chega perto.")]
    [SerializeField] private string prompt = "Entrar";

    [Header("Tranca")]
    [Tooltip("Item que abre a passagem — a chave. Item Id vazio deixa a passagem sempre aberta.")]
    [SerializeField] private ItemRequirement requirement = new ItemRequirement();

    [Header("Chegada")]
    [Tooltip("Corta o amortecimento da câmera na chegada. Desmarcado, ela atravessa o mapa " +
             "deslizando e entrega todo o caminho — o que estraga qualquer passagem secreta.")]
    [SerializeField] private bool snapCamera = true;

    [Tooltip("Zera a velocidade na chegada. Desmarcado, ela desembarca ainda correndo pro " +
             "lado em que estava indo.")]
    [SerializeField] private bool stopOnArrival = true;

    [Tooltip("Som da passagem. Sai pelo AudioSource de quem viajou, e não daqui: o destino " +
             "pode estar longe demais pra este objeto ser ouvido de lá.")]
    [SerializeField] private AudioClip travelClip;

    [Tooltip("Uma vez só: depois de usada, a passagem se fecha e o prompt some.")]
    [SerializeField] private bool singleUse;

    [Tooltip("O que mais acontece na chegada — apagar o poste da entrada, tocar uma trilha, " +
             "abrir a grade do outro lado. Ligue os objetos de cena aqui.")]
    [SerializeField] private UnityEvent onTeleported;

    /// <summary>Já foi usada — só importa quando <see cref="singleUse"/> está marcado.</summary>
    public bool Used { get; private set; }

    /// <summary>(quem viajou) — pra quem precisa reagir sem passar pelo Inspector.</summary>
    public static event System.Action<GameObject> OnAnyTeleport;

    private CameraFollow cameraFollow;

    /// <summary>
    /// Estado da tranca visto no último <see cref="CanInteract"/>. O <see cref="Prompt"/> não
    /// recebe quem está perguntando, e é o <see cref="PlayerInteractor"/> que decide a hora de
    /// ler os dois — sempre CanInteract primeiro, no mesmo quadro, com o mesmo interator.
    /// </summary>
    private bool lockedForLastAsked;

    public string Prompt => lockedForLastAsked ? requirement.LockedPrompt : prompt;

    private void Awake()
    {
        // Sem collider ele nunca entra no OverlapCircle do PlayerInteractor: ficaria invisível.
        if (GetComponentInChildren<Collider2D>() == null)
            Debug.LogWarning($"[Teleporter] '{name}' não tem Collider2D: o E nunca vai achar ele.", this);

        if (destination == null && destinationPoint == Vector2.zero)
            Debug.LogWarning($"[Teleporter] '{name}' está sem destino: o E não vai fazer nada.", this);

        requirement.Warn(this);
    }

    /// <summary>
    /// A tranca <b>não</b> entra aqui: uma passagem trancada aceita o E e responde que está
    /// trancada. Recusar o foco só a deixaria invisível pra jogadora.
    /// </summary>
    public bool CanInteract(GameObject interactor)
    {
        lockedForLastAsked = requirement.IsLockedFor(interactor);
        return !Used && HasDestination;
    }

    /// <summary>Falta a chave pra passar. Veja o <see cref="ItemRequirement"/>.</summary>
    public bool IsLockedFor(GameObject interactor) => requirement.IsLockedFor(interactor);

    /// <summary>Tranca e destranca por condição que não é item, pra ligar num UnityEvent da cena.</summary>
    public void SetLocked(bool locked) => requirement.Blocked = locked;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        if (requirement.IsLockedFor(interactor))
        {
            requirement.Deny(interactor);
            return;
        }

        // O collider pode estar num filho; quem viaja é o objeto inteiro, com o corpo físico.
        Rigidbody2D body = interactor.GetComponentInParent<Rigidbody2D>();
        Transform root = body != null ? body.transform : interactor.transform;

        Move(root, body, Target);

        if (stopOnArrival)
            Halt(root, body);

        if (snapCamera && root.CompareTag("Player"))
            SnapCamera();

        PlayTravel(root);

        requirement.Spend(interactor);

        Used = singleUse;
        onTeleported?.Invoke();
        OnAnyTeleport?.Invoke(root.gameObject);
    }

    /// <summary>Onde ela vai parar: o objeto de destino, ou o ponto solto quando não há objeto.</summary>
    public Vector2 Target => destination != null ? (Vector2)destination.position : destinationPoint;

    private bool HasDestination => destination != null || destinationPoint != Vector2.zero;

    /// <summary>
    /// A mudança de posição em si. O Z do objeto é preservado — em 2D ele costuma ser a
    /// ordem de desenho, e zerar aqui enfiaria a Chapéuzinho atrás ou na frente do cenário.
    /// </summary>
    private static void Move(Transform root, Rigidbody2D body, Vector2 target)
    {
        Vector3 position = new Vector3(target.x, target.y, root.position.z);

        if (body == null)
        {
            root.position = position;
            return;
        }

        // Interpolação guarda a posição do quadro anterior pra desenhar o meio do caminho.
        // Desligar e religar limpa essa memória: sem isso, o teleporte vira um rastro.
        RigidbodyInterpolation2D interpolation = body.interpolation;
        body.interpolation = RigidbodyInterpolation2D.None;

        body.position = target;
        root.position = position;

        // A física só lê os transforms no próximo passo; sem isto o primeiro FixedUpdate
        // depois do teleporte ainda testaria colisão no lugar antigo.
        Physics2D.SyncTransforms();

        body.interpolation = interpolation;
    }

    /// <summary>Chega parada. Passa pelo <see cref="PlayerController.Halt"/> quando existe, pro
    /// controle não continuar achando que ela está andando.</summary>
    private static void Halt(Transform root, Rigidbody2D body)
    {
        PlayerController controller = root.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.Halt();
            return;
        }

        if (body != null)
            body.velocity = Vector2.zero;
    }

    private void SnapCamera()
    {
        // Achada tarde e guardada: teleporte é raro, mas a busca não precisa repetir.
        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.SnapToTarget();
    }

    private void PlayTravel(Transform root) => Play(root.gameObject, travelClip);

    /// <summary>Toca pelo AudioSource de quem interagiu — o desta passagem pode estar longe.</summary>
    private static void Play(GameObject who, AudioClip clip)
    {
        if (clip == null || who == null)
            return;

        AudioSource source = who.GetComponentInChildren<AudioSource>();
        if (source != null)
            source.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        if (!HasDestination)
            return;

        Vector3 target = new Vector3(Target.x, Target.y, transform.position.z);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, target);
        Gizmos.DrawWireSphere(target, 0.35f);
    }
}
