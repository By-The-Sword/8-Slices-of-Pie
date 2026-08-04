using System.Collections;
using UnityEngine;

/// <summary>
/// 3 corações, sem ataque e sem cura no mapa (GDD). Quem bate informa o dano:
/// o Lobo passa a tirar 2 corações a partir da 5ª fatia, e essa regra é dele, não daqui.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHearts = 3;

    [Header("Reação ao dano")]
    [SerializeField] private float invulnerabilityDuration = 1f;
    [SerializeField] private float knockbackSpeed = 6f;
    [SerializeField] private float knockbackDuration = 0.15f;

    public int MaxHearts => maxHearts;
    public int Hearts { get; private set; }
    public bool IsDead => Hearts <= 0;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    /// <summary>(corações atuais, máximo) — a pulseira diegética escuta isso.</summary>
    public event System.Action<int, int> OnHeartsChanged;
    public event System.Action<int> OnDamaged;
    public event System.Action OnDied;

    private PlayerController controller;
    private Rigidbody2D body;
    private float invulnerableUntil;
    private Coroutine knockbackRoutine;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        body = GetComponent<Rigidbody2D>();
        Hearts = maxHearts;
    }

    private void Start()
    {
        OnHeartsChanged?.Invoke(Hearts, maxHearts);
    }

    /// <summary>
    /// Aplica dano. Devolve false se foi ignorado (i-frames ou já morta).
    /// </summary>
    public bool TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsDead || IsInvulnerable || amount <= 0)
            return false;

        Hearts = Mathf.Max(0, Hearts - amount);
        invulnerableUntil = Time.time + invulnerabilityDuration;

        OnDamaged?.Invoke(amount);
        OnHeartsChanged?.Invoke(Hearts, maxHearts);

        if (IsDead)
        {
            Die();
            return true;
        }

        Vector2 away = ((Vector2)transform.position - sourcePosition).normalized;
        if (away.sqrMagnitude < 0.01f)
            away = -controller.Facing;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(Knockback(away));

        return true;
    }

    private IEnumerator Knockback(Vector2 direction)
    {
        controller.MovementLocked = true;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            if (body != null)
                body.velocity = direction * knockbackSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        controller.Halt();
        controller.MovementLocked = false;
        knockbackRoutine = null;
    }

    private void Die()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        controller.Halt();
        controller.MovementLocked = true;

        // Reiniciar a noite em 22:00 com 0 fatias é decisão do gerenciador de partida,
        // não do player — aqui só se avisa que ela morreu.
        OnDied?.Invoke();
    }
}
