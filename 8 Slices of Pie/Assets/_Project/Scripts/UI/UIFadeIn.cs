using System.Collections;
using UnityEngine;

/// <summary>
/// Faz um pedaço de UI nascer transparente e aparecer sozinho toda vez que é ligado.
///
/// Existe pro pop-up de pausa não dar um estalo: o ESC levanta o braço, e os botões só
/// alcançam ele um instante depois. Vai no objeto que agrupa os botões — não no painel
/// inteiro, senão o escurecido da tela entra no fade junto e a pausa perde o corte seco.
///
/// Roda em tempo real, não em <c>deltaTime</c>: pausado o <c>timeScale</c> é 0 e nada
/// disto andaria.
///
/// Enquanto está aparecendo ninguém clica: um botão invisível que já responde ao mouse
/// faz o jogador "acertar" o Sair sem ter visto onde ele estava.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIFadeIn : MonoBehaviour
{
    [Tooltip("Espera antes de começar a aparecer, em segundos. É o tempo de o braço subir.")]
    [SerializeField] private float delay = 0.12f;

    [Tooltip("Duração do fade, em segundos.")]
    [SerializeField] private float duration = 0.25f;

    [Tooltip("Bloqueia clique e teclado enquanto não terminou de aparecer.")]
    [SerializeField] private bool blockInputWhileFading = true;

    private CanvasGroup group;

    private void Awake() => group = GetComponent<CanvasGroup>();

    private void OnEnable()
    {
        // Um SetActive(false) no meio do fade mata a corrotina e deixaria o alpha pela
        // metade; por isso o estado inicial é escrito aqui, toda vez, e não só no Awake.
        StopAllCoroutines();
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        group.alpha = 0f;
        SetInteractable(false);

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        group.alpha = 1f;
        SetInteractable(true);
    }

    private void SetInteractable(bool on)
    {
        if (!blockInputWhileFading)
            return;

        group.interactable = on;
        group.blocksRaycasts = on;
    }
}
