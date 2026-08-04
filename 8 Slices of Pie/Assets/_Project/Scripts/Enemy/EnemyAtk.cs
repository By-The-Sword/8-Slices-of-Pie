using UnityEngine;

/// <summary>
/// A mordida do Lobo Mau. Ele não tem combate de verdade: encosta na Chapéuzinho,
/// tira coração e sai correndo — quem corre é o <see cref="EnemyMov"/>, chamado aqui.
/// Só morde perseguindo: farejando ou patrulhando ele ainda não a identificou (GDD).
/// </summary>
[RequireComponent(typeof(EnemyMov))]
public class EnemyAtk : MonoBehaviour
{
    [Header("Mordida")]
    [Tooltip("Corações tirados por mordida. Vira 2 a partir da 5ª fatia (GDD).")]
    [SerializeField] private int damage = 1;

    [Tooltip("Distância de encostar nela — a partir daqui ele morde.")]
    [SerializeField] private float range = 0.9f;

    [Tooltip("Descanso mínimo entre mordidas. Uma mordida por encontro já vem do Recuo do " +
             "EnemyMov; isto é a segunda trava, caso algo tire ele do Recuo antes da hora.")]
    [SerializeField] private float cooldown = 3f;

    /// <summary>Corações da mordida — o gerenciador de fatias sobe isso pra 2 na 5ª.</summary>
    public int Damage
    {
        get => damage;
        set => damage = Mathf.Max(0, value);
    }

    public event System.Action<int> OnBite;

    private EnemyMov movement;
    private PlayerHealth playerHealth;
    private Transform player;
    private float nextBiteTime;
    private bool warnedMissingHealth;

    private void Awake()
    {
        movement = GetComponent<EnemyMov>();
    }

    private void Update()
    {
        // O EnemyMov resolve a Chapéuzinho no Start dele, então a busca fica aqui no Update.
        if (!ResolveTarget())
            return;

        if (movement.MovementLocked || movement.State != WolfState.Perseguicao)
            return;

        if (Time.time < nextBiteTime || playerHealth.IsDead)
            return;

        if (Vector2.Distance(transform.position, player.position) > range)
            return;

        Bite();
    }

    private void Bite()
    {
        // Dano recusado (i-frames dela) não vale como mordida: ele continua em cima.
        if (!playerHealth.TakeDamage(damage, transform.position))
            return;

        nextBiteTime = Time.time + cooldown;
        OnBite?.Invoke(damage);

        // Mordeu e sumiu: a caçada acaba aqui e ele dispara pro ponto mais longe dela.
        movement.Retreat(player.position);
    }

    private bool ResolveTarget()
    {
        if (playerHealth != null)
            return true;

        player = movement.Target;
        if (player == null)
            return false;

        playerHealth = player.GetComponent<PlayerHealth>();

        // Falhar calado aqui vira "o Lobo encosta e não faz nada" — avisa uma vez e segue.
        if (playerHealth == null && !warnedMissingHealth)
        {
            warnedMissingHealth = true;
            Debug.LogWarning($"[EnemyAtk] '{player.name}' está com a tag Player mas não tem " +
                             "PlayerHealth: o Lobo nunca vai morder.", player);
        }

        return playerHealth != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
