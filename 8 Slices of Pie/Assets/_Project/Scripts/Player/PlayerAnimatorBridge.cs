using UnityEngine;

/// <summary>
/// Traduz o estado do player em parâmetros do Animator (caminhada em 4 direções, agachar).
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

    private PlayerController controller;
    private int moveXHash, moveYHash, isMovingHash, isCrouchedHash;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        moveXHash = Animator.StringToHash(moveXParam);
        moveYHash = Animator.StringToHash(moveYParam);
        isMovingHash = Animator.StringToHash(isMovingParam);
        isCrouchedHash = Animator.StringToHash(isCrouchedParam);
    }

    private void Update()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        animator.SetFloat(moveXHash, controller.Facing.x);
        animator.SetFloat(moveYHash, controller.Facing.y);
        animator.SetBool(isMovingHash, controller.IsMoving);
        animator.SetBool(isCrouchedHash, controller.IsCrouched);
    }
}
