using UnityEngine;

/// <summary>
/// Contrato de tudo que responde ao E: portas, fatias, chaves, baterias,
/// papéis, a máquina da loja e o galho.
/// </summary>
public interface IInteractable
{
    /// <summary>Texto do prompt contextual ("Pegar fatia", "Abrir porta"...).</summary>
    string Prompt { get; }

    bool CanInteract(GameObject interactor);

    void Interact(GameObject interactor);
}
