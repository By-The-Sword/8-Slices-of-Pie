using UnityEngine;

/// <summary>
/// Traduz o estado do player em parâmetros do Animator (caminhada em 8 direções, agachar).
/// Fica separado de propósito: a arte entra depois e não deve mexer na lógica de movimento.
/// Funciona com o Animator vazio — só não faz nada.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimatorBridge : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Nomes dos parâmetros")]
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isCrouchedParam = "IsCrouched";

    [Header("Espelhamento")]
    [Tooltip("Desmarque se a arte de lado foi desenhada virada para a esquerda.")]
    [SerializeField] private bool spriteFacesRight = true;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlayerController controller;
    private int moveXHash, moveYHash, isMovingHash, isCrouchedHash;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        moveXHash = Animator.StringToHash(moveXParam);
        moveYHash = Animator.StringToHash(moveYParam);
        isMovingHash = Animator.StringToHash(isMovingParam);
        isCrouchedHash = Animator.StringToHash(isCrouchedParam);
    }

    private void Update()
    {
        Vector2 facing = controller.Facing;

        // Só espelha quando há lado de verdade: cos(90°) volta como ~1e-16 negativo
        // e faria ela piscar espelhada ao andar reto pra cima ou pra baixo.
        if (spriteRenderer != null && Mathf.Abs(facing.x) > 0.01f)
            spriteRenderer.flipX = (facing.x < 0f) == spriteFacesRight;

        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        // MoveX sempre positivo: os 5 clipes cobrem as 8 direções via flipX.
        animator.SetFloat(moveXHash, Mathf.Abs(facing.x));
        animator.SetFloat(moveYHash, facing.y);
        animator.SetBool(isMovingHash, controller.IsMoving);
        animator.SetBool(isCrouchedHash, controller.IsCrouched);
    }
}
