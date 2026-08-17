using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// O texto do E na tela: "Pegar fatia", "Entrar", "Está trancado". O
/// <see cref="PlayerInteractor"/> publica o prompt por evento e não desenha nada — este é o
/// lado que desenha.
///
/// Vai num objeto de UI dentro de um Canvas, com o <see cref="TextMeshProUGUI"/> nele mesmo ou
/// num filho. Some sozinho quando não há nada em foco: esconde o texto em vez de desligar o
/// objeto, senão este script pararia de rodar junto e nunca veria o próximo foco.
///
/// Não lê teclado nem varre a cena por frame — só reage ao evento.
/// </summary>
public class InteractPrompt : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Vazio busca no próprio objeto e nos filhos.")]
    [SerializeField] private TextMeshProUGUI label;

    [Tooltip("Faixa escura atrás da legenda, pra ela ser legível em cima do cenário. " +
             "Opcional, e liga e desliga junto com o texto — senão sobraria uma tarja vazia " +
             "no rodapé o resto da noite.")]
    [SerializeField] private Graphic background;

    [Tooltip("Vazio acha o Player sozinho na cena.")]
    [SerializeField] private PlayerInteractor interactor;

    [Tooltip("Vazio acha sozinho. Serve pra esconder o prompt na pausa — o braço levantado " +
             "já ocupa a tela.")]
    [SerializeField] private PauseMenu pauseMenu;

    [Header("Recado avulso")]
    [Tooltip("Quanto tempo dura um Flash() antes do prompt normal voltar.")]
    [Min(0.1f)]
    [SerializeField] private float flashSeconds = 1.2f;

    private string prompt = string.Empty;
    private string flashText;
    private float flashUntil;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (interactor == null)
            interactor = FindObjectOfType<PlayerInteractor>();

        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenu>();

        if (label == null)
            Debug.LogError($"[InteractPrompt] '{name}' não achou um TextMeshProUGUI: não tem onde escrever.", this);

        if (interactor == null)
            Debug.LogWarning($"[InteractPrompt] '{name}' não achou um PlayerInteractor: o prompt vai ficar sempre vazio.", this);

        Apply();
    }

    private void OnEnable()
    {
        if (interactor != null)
            interactor.OnFocusChanged += HandleFocusChanged;

        ItemRequirement.OnAnyDenied += HandleDenied;
    }

    private void OnDisable()
    {
        if (interactor != null)
            interactor.OnFocusChanged -= HandleFocusChanged;

        ItemRequirement.OnAnyDenied -= HandleDenied;
    }

    private void Update()
    {
        // Só existe pro Flash() expirar e pra pausa esconder o texto. Sem recado no ar e sem
        // pausa na cena isto não faz nada.
        if (flashText != null && Time.unscaledTime >= flashUntil)
            flashText = null;

        Apply();
    }

    /// <summary>
    /// Um recado por cima do prompt, que sai sozinho depois do <see cref="flashSeconds"/>.
    /// Recebe uma string só, de propósito: assim dá pra ligar direto no
    /// <c>onLockedAttempt</c> de uma passagem trancada pelo Inspector, com o texto digitado lá.
    /// </summary>
    public void Flash(string message)
    {
        // Recado vazio apagaria o prompt normal por um segundo e meio, sem escrever nada.
        if (string.IsNullOrEmpty(message))
            return;

        flashText = message;

        // unscaledTime pro recado não congelar junto com um timeScale = 0.
        flashUntil = Time.unscaledTime + flashSeconds;
        Apply();
    }

    private void HandleFocusChanged(IInteractable target, string text)
    {
        prompt = text ?? string.Empty;
        Apply();
    }

    /// <summary>
    /// Uma tentativa recusada em qualquer objeto trancado do mapa — a torta alta demais, a
    /// porta sem chave. Vem por evento estático justamente pra não precisar arrastar este
    /// objeto pro <c>onLockedAttempt</c> de cada tranca da cena.
    /// </summary>
    private void HandleDenied(GameObject interactor, string message) => Flash(message);

    private void Apply()
    {
        if (label == null)
            return;

        bool paused = pauseMenu != null && pauseMenu.IsPaused;
        string text = paused ? string.Empty : flashText ?? prompt;

        label.enabled = !string.IsNullOrEmpty(text);

        if (background != null)
            background.enabled = label.enabled;

        // Escrever no TMP força um rebuild da malha do texto: só quando muda de fato.
        if (label.enabled && label.text != text)
            label.text = text;
    }
}
