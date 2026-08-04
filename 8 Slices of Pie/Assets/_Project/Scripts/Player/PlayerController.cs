using UnityEngine;

/// <summary>
/// Movimento top-down da Chapéuzinho. Velocidade única, sem stamina e sem corrida:
/// a fuga é decidida por rota, não por velocidade (pilar do GDD).
/// CTRL alterna agachar — agachada o Lobo não a percebe, ao custo de andar mais devagar.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Tooltip("Multiplicador de velocidade enquanto agachada.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Header("Input")]
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    /// <summary>Direção que ela encara, sempre travada em uma das 4 direções cardeais.</summary>
    public Vector2 Facing { get; private set; } = Vector2.down;

    public bool IsMoving { get; private set; }
    public bool IsCrouched { get; private set; }

    /// <summary>Velocidade efetiva no frame atual, já considerando o agachar.</summary>
    public float CurrentSpeed => moveSpeed * (IsCrouched ? crouchSpeedMultiplier : 1f);

    /// <summary>Trava o controle sem desligar o componente (knockback, cutscene, menu).</summary>
    public bool MovementLocked { get; set; }

    public event System.Action<bool> OnCrouchChanged;

    private Rigidbody2D body;
    private Vector2 moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (MovementLocked)
        {
            moveInput = Vector2.zero;
            IsMoving = false;
            return;
        }

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Normaliza só na diagonal, pra andar torto não ser mais rápido que andar reto.
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        IsMoving = moveInput.sqrMagnitude > 0.01f;
        if (IsMoving)
            Facing = ToCardinal(moveInput);

        if (Input.GetKeyDown(crouchKey))
            SetCrouched(!IsCrouched);
    }

    private void FixedUpdate()
    {
        // Travada, quem manda na velocidade é outro (knockback, cutscene) — não sobrescrever.
        if (MovementLocked)
            return;

        body.velocity = moveInput * CurrentSpeed;
    }

    public void SetCrouched(bool crouched)
    {
        if (IsCrouched == crouched)
            return;

        IsCrouched = crouched;
        OnCrouchChanged?.Invoke(IsCrouched);
    }

    /// <summary>Trava o movimento e zera a velocidade — usado por knockback e morte.</summary>
    public void Halt()
    {
        moveInput = Vector2.zero;
        IsMoving = false;
        if (body != null)
            body.velocity = Vector2.zero;
    }

    private static Vector2 ToCardinal(Vector2 direction)
    {
        return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
            ? new Vector2(Mathf.Sign(direction.x), 0f)
            : new Vector2(0f, Mathf.Sign(direction.y));
    }
}
