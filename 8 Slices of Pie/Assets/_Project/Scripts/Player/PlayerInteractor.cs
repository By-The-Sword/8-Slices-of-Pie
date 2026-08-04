using UnityEngine;

/// <summary>
/// E interage com o <see cref="IInteractable"/> válido mais próximo dentro do raio.
/// O prompt contextual é publicado por evento — quem desenha é a UI diegética.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Alcance")]
    [SerializeField] private float interactRadius = 0.9f;

    [Tooltip("Deslocamento do centro da busca na direção que ela encara.")]
    [SerializeField] private float facingOffset = 0.25f;

    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    /// <summary>Alvo atual, ou null. Muda só quando o foco muda de fato.</summary>
    public IInteractable Focused { get; private set; }

    /// <summary>(alvo, prompt) — alvo null significa esconder o prompt.</summary>
    public event System.Action<IInteractable, string> OnFocusChanged;
    public event System.Action<IInteractable> OnInteracted;

    private PlayerController controller;
    private Rigidbody2D ownBody;
    private readonly Collider2D[] hits = new Collider2D[8];

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        ownBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateFocus();

        if (Focused != null && Input.GetKeyDown(interactKey))
        {
            IInteractable target = Focused;
            target.Interact(gameObject);
            OnInteracted?.Invoke(target);

            // O alvo pode ter sumido (fatia coletada) — reavalia já.
            UpdateFocus();
        }
    }

    private void UpdateFocus()
    {
        IInteractable nearest = FindNearest();

        if (ReferenceEquals(nearest, Focused))
            return;

        Focused = nearest;
        OnFocusChanged?.Invoke(Focused, Focused != null ? Focused.Prompt : string.Empty);
    }

    private IInteractable FindNearest()
    {
        Vector2 center = (Vector2)transform.position + controller.Facing * facingOffset;
        int count = Physics2D.OverlapCircleNonAlloc(center, interactRadius, hits, interactableLayers);

        IInteractable best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || (ownBody != null && hit.attachedRigidbody == ownBody))
                continue;

            IInteractable candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null || !candidate.CanInteract(gameObject))
                continue;

            float distance = ((Vector2)hit.transform.position - center).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 facing = controller != null ? controller.Facing : Vector2.down;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + facing * facingOffset, interactRadius);
    }
}
