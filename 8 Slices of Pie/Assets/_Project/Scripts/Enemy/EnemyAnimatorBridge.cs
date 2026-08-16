using UnityEngine;

/// <summary>
/// Traduz o que o Lobo está fazendo em parâmetros do Animator. Fica separado de propósito:
/// a IA não sabe que existe animação e a arte pode mudar sem encostar nela — mesma divisão
/// do <see cref="PlayerAnimatorBridge"/>. Funciona com o Animator vazio, só não faz nada.
///
/// A arte dele é de vista lateral, com uma direção só: quem resolve o lado é o <c>flipX</c>.
/// Andando na vertical o <see cref="EnemyMov.Facing"/> zera o X, e aí o último lado é mantido
/// — virar o bicho pro nada seria pior do que não virar.
///
/// A passada acelera sozinha: em vez de um clipe por estado, a caminhada toca mais rápido
/// conforme a velocidade real do corpo. Mesmo princípio dos passos do <see cref="EnemyAudio"/>,
/// que saem por distância percorrida e não por tempo — a ronda arrastada e a perseguição
/// saem diferentes de graça, sem configurar nada por estado.
///
/// Vai no mesmo objeto do EnemyMov, no Wolf.prefab.
/// </summary>
[RequireComponent(typeof(EnemyMov))]
public class EnemyAnimatorBridge : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Vazio busca no próprio objeto e nos filhos.")]
    [SerializeField] private Animator animator;

    [Tooltip("Quem é virado pelo flipX. Vazio busca no próprio objeto e nos filhos.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Nomes dos parâmetros")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string moveSpeedParam = "MoveSpeed";
    [SerializeField] private string attackParam = "Attack";

    [Header("Passada")]
    [Tooltip("Velocidade em que a caminhada toca no ritmo desenhado. O padrão é a da patrulha: " +
             "daí pra cima (suspeita, perseguição, recuo) as patas aceleram junto.")]
    [SerializeField] private float baseSpeed = 1.8f;

    [Tooltip("Piso e teto do quanto a passada acelera. Sem teto, o recuo vira um borrão.")]
    [SerializeField] private Vector2 speedRange = new Vector2(0.6f, 2f);

    [Header("Direção")]
    [Tooltip("A arte foi desenhada olhando pra direita. Desmarque se trocarem os sprites por " +
             "outros virados pro outro lado.")]
    [SerializeField] private bool artFacesRight = true;

    private EnemyMov movement;
    private EnemyAtk attack;
    private Rigidbody2D body;
    private int isMovingHash, moveSpeedHash, attackHash;

    /// <summary>Último lado com X de verdade — é o que segura o corpo enquanto ele anda na vertical.</summary>
    private bool facingRight = true;

    private void Awake()
    {
        movement = GetComponent<EnemyMov>();
        attack = GetComponent<EnemyAtk>();
        body = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        isMovingHash = Animator.StringToHash(isMovingParam);
        moveSpeedHash = Animator.StringToHash(moveSpeedParam);
        attackHash = Animator.StringToHash(attackParam);
    }

    private void OnEnable()
    {
        if (attack != null)
            attack.OnBite += HandleBite;
    }

    private void OnDisable()
    {
        if (attack != null)
            attack.OnBite -= HandleBite;
    }

    private void Update()
    {
        // Fora do Animator: o lado vale mesmo sem controller, senão o Lobo anda de ré.
        UpdateFacing();

        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        animator.SetBool(isMovingHash, movement.IsMoving);
        animator.SetFloat(moveSpeedHash, StepRate());
    }

    /// <summary>Vira o corpo pro lado em que ele anda, guardando o último lado válido.</summary>
    private void UpdateFacing()
    {
        if (spriteRenderer == null)
            return;

        float x = movement.Facing.x;

        if (!Mathf.Approximately(x, 0f))
            facingRight = x > 0f;

        spriteRenderer.flipX = facingRight != artFacesRight;
    }

    /// <summary>Multiplicador do clipe de caminhada: velocidade real sobre a da patrulha.</summary>
    private float StepRate()
    {
        if (body == null || baseSpeed <= 0f)
            return 1f;

        float rate = body.velocity.magnitude / baseSpeed;
        return Mathf.Clamp(rate, speedRange.x, Mathf.Max(speedRange.x, speedRange.y));
    }

    /// <summary>A mordida conectou — o clipe de ataque entra por cima do que estiver tocando.</summary>
    private void HandleBite(int damage)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger(attackHash);
    }
}
