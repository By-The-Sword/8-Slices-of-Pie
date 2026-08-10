using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESC congela a noite e abre o pop-up de pausa por cima da própria cena — nada de trocar
/// de cena pra pausar, o mapa continua ali atrás. O painel oferece continuar, abrir as
/// opções ou voltar pro menu principal; ESC de novo é o mesmo que continuar.
///
/// Vai num objeto de cena da partida — o mesmo que guarda o <see cref="Clock"/>.
///
/// Pausar aqui é <c>Time.timeScale = 0</c>: a física e tudo que anda por deltaTime param
/// sozinhos. O que NÃO para é <c>Update</c>, então quem lê teclado (lampião, interação,
/// agachar) precisa ser desligado na mão — senão dá pra acender o lampião com o jogo pausado.
/// É o que <see cref="disableWhilePaused"/> faz.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Pop-up")]
    [Tooltip("O painel do menu de pausa. Começa desativado; é ligado e desligado por aqui.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("O painel de opções, irmão do de pausa. Um substitui o outro — nunca os dois juntos. " +
             "Pode ficar vazio enquanto a tela de opções não existir; aí o botão avisa no console.")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Destino")]
    [Tooltip("Nome da cena do menu principal. Precisa estar em File > Build Settings.")]
    [SerializeField] private string menuSceneName = "MainMenu";

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Enquanto pausado")]
    [Tooltip("Scripts que leem teclado e precisam calar a boca na pausa. " +
             "Vazio pega sozinho os da Chapéuzinho (movimento, lampião, interação).")]
    [SerializeField] private MonoBehaviour[] disableWhilePaused;

    [Tooltip("Silencia o jogo inteiro enquanto o pop-up está aberto.")]
    [SerializeField] private bool muteWhilePaused = true;

    public bool IsPaused { get; private set; }

    /// <summary>Opções abertas por cima da pausa. Sempre implica <see cref="IsPaused"/>.</summary>
    public bool IsOptionsOpen { get; private set; }

    /// <summary>Avisa quem mais precisa reagir à pausa (HUD, som ambiente).</summary>
    public event System.Action<bool> OnPauseChanged;

    /// <summary>Só o que esta pausa desligou volta a ligar — não se reativa o que já estava off.</summary>
    private readonly List<MonoBehaviour> disabledByPause = new List<MonoBehaviour>();

    /// <summary>Saindo pro menu, ESC não deve mais reabrir nada durante o carregamento.</summary>
    private bool leaving;

    private void Awake()
    {
        if (disableWhilePaused == null || disableWhilePaused.Length == 0)
            disableWhilePaused = FindPlayerInputScripts();

        // Garante que os pop-ups nasçam fechados, mesmo que tenham ficado ativos por descuido.
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    private void Update()
    {
        if (leaving)
            return;

        if (!Input.GetKeyDown(pauseKey))
            return;

        // Dentro das opções, ESC volta um passo em vez de despausar direto:
        // quem entrou por engano não perde o menu inteiro.
        if (IsOptionsOpen)
            CloseOptions();
        else
            TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (IsPaused || leaving)
            return;

        IsPaused = true;

        // A pausa sempre abre na tela de cima, nunca nas opções de onde se saiu da última vez.
        ShowOptions(false);

        if (pausePanel == null)
            Debug.LogWarning($"[PauseMenu] '{name}' está sem painel: o jogo pausa, mas nada aparece.", this);

        SetInputScriptsEnabled(false);

        Time.timeScale = 0f;
        if (muteWhilePaused)
            AudioListener.pause = true;

        OnPauseChanged?.Invoke(true);
    }

    /// <summary>Botão "Continuar" do pop-up.</summary>
    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        IsOptionsOpen = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        RestoreTime();
        SetInputScriptsEnabled(true);

        OnPauseChanged?.Invoke(false);
    }

    /// <summary>Botão "Opções" do pop-up. Troca a tela de pausa pela de opções, sem despausar.</summary>
    public void OpenOptions()
    {
        if (leaving)
            return;

        // Chamado de fora com o jogo rodando (atalho, botão de HUD), pausa antes:
        // opções abertas com o Lobo andando atrás seria injusto.
        if (!IsPaused)
            Pause();

        if (optionsPanel == null)
        {
            Debug.LogWarning($"[PauseMenu] '{name}' está sem painel de opções — o botão não tem o que abrir.", this);
            return;
        }

        ShowOptions(true);
    }

    /// <summary>Botão "Voltar" das opções. Devolve pra tela de pausa, ainda pausado.</summary>
    public void CloseOptions()
    {
        if (!IsOptionsOpen)
            return;

        ShowOptions(false);
    }

    /// <summary>Alterna as duas telas do pop-up — só uma fica visível por vez.</summary>
    private void ShowOptions(bool show)
    {
        IsOptionsOpen = show && optionsPanel != null;

        if (optionsPanel != null)
            optionsPanel.SetActive(IsOptionsOpen);

        if (pausePanel != null)
            pausePanel.SetActive(!IsOptionsOpen);
    }

    /// <summary>Botão "Menu principal" do pop-up. Abandona a run: a noite recomeça do zero.</summary>
    public void GoToMainMenu()
    {
        if (leaving)
            return;

        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogWarning($"[PauseMenu] '{name}' está sem nome de cena: não há pra onde voltar.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            Debug.LogError($"[PauseMenu] A cena '{menuSceneName}' não está no Build Settings — adicione em File > Build Settings.", this);
            return;
        }

        leaving = true;

        // O menu tem que nascer rodando e com som, senão herda a pausa da partida.
        RestoreTime();

        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>Botão "Sair" — no editor não faz nada visível, é normal.</summary>
    public void QuitGame()
    {
        RestoreTime();
        Application.Quit();
    }

    private void OnDisable()
    {
        // Se este objeto morrer pausado (troca de cena, fim de partida), o jogo não pode
        // ficar congelado pra sempre — o timeScale é global.
        if (IsPaused)
        {
            IsPaused = false;
            IsOptionsOpen = false;
            RestoreTime();
            disabledByPause.Clear();
        }
    }

    private void RestoreTime()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void SetInputScriptsEnabled(bool enable)
    {
        if (enable)
        {
            foreach (MonoBehaviour script in disabledByPause)
            {
                // Pode ter sido destruído no meio da pausa; o == null do Unity cobre isso.
                if (script != null)
                    script.enabled = true;
            }

            disabledByPause.Clear();
            return;
        }

        disabledByPause.Clear();

        if (disableWhilePaused == null)
            return;

        foreach (MonoBehaviour script in disableWhilePaused)
        {
            if (script == null || !script.enabled)
                continue;

            script.enabled = false;
            disabledByPause.Add(script);
        }
    }

    /// <summary>Acha os scripts de input da Chapéuzinho, pra pausa funcionar sem configurar nada.</summary>
    private static MonoBehaviour[] FindPlayerInputScripts()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null)
            return new MonoBehaviour[0];

        var found = new List<MonoBehaviour> { player };

        Lantern lantern = player.GetComponentInChildren<Lantern>();
        if (lantern != null)
            found.Add(lantern);

        PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
        if (interactor != null)
            found.Add(interactor);

        return found.ToArray();
    }
}
