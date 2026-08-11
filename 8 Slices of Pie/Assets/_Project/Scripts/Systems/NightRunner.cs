using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// O gerenciador de partida: os dois jeitos de a noite acabar.
///
/// O <see cref="PlayerHealth"/> avisa que ela morreu e para por aí, de propósito — quem decide
/// o que acontece depois é este script: segura um tempo pra mordida ser lida, escurece a tela
/// e recarrega a cena. Sem save no GDD, recomeçar a noite é literalmente reabrir a cena: o
/// relógio volta pras 22:00 com 0 fatias e o <see cref="EnemyHabilities"/> reaplica o estágio 0.
///
/// A oitava fatia sai por <see cref="onNightSurvived"/> em vez de recarregar nada: a casa da
/// avó e a cutscene final ainda não existem, e este script não inventa nenhuma das duas.
///
/// Vai no mesmo objeto de cena do <see cref="Clock"/>.
/// </summary>
public class NightRunner : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Vazio busca a Chapéuzinho sozinho na cena.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Vazio busca sozinho na cena.")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Morte")]
    [Tooltip("Tempo parado depois do último coração, antes de começar a escurecer. " +
             "É o que dá pra ela entender que morreu em vez de a tela só piscar.")]
    [SerializeField] private float deathDelay = 1.5f;

    [Tooltip("Desmarque pra debugar a morte sem a cena reiniciando por cima.")]
    [SerializeField] private bool restartOnDeath = true;

    [Tooltip("Trava o Lobo assim que ela cai. Sem isto ele fica rondando o corpo durante o fade.")]
    [SerializeField] private bool freezeWolfOnDeath = true;

    [Header("Fade")]
    [Tooltip("CanvasGroup de uma imagem preta cobrindo a tela, começando com alpha 0. " +
             "Pode ficar vazio: aí a cena recarrega seca, sem escurecer.")]
    [SerializeField] private CanvasGroup fadeGroup;

    [SerializeField] private float fadeDuration = 1f;

    [Header("Eventos de cena")]
    [Tooltip("Disparado no instante da morte — som de game over, tela de 'a noite te pegou'.")]
    [SerializeField] private UnityEvent onDeath;

    [Tooltip("Oitava fatia coletada. É aqui que a casa da avó e a cutscene final vão entrar.")]
    [SerializeField] private UnityEvent onNightSurvived;

    /// <summary>A noite já acabou (morte ou 8/8) — o segundo final não atropela o primeiro.</summary>
    public bool IsEnding { get; private set; }

    public event System.Action OnDeath;
    public event System.Action OnNightSurvived;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (inventory == null)
            inventory = FindObjectOfType<PlayerInventory>();

        // Nasce transparente mesmo que tenha ficado preto no editor depois de um teste.
        if (fadeGroup != null)
            fadeGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandleDied;

        if (inventory != null)
            inventory.OnAllSlicesCollected += HandleAllSlicesCollected;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandleDied;

        if (inventory != null)
            inventory.OnAllSlicesCollected -= HandleAllSlicesCollected;
    }

    private void Start()
    {
        // Sem player a partida não tem como acabar: é cena de teste, não erro fatal.
        if (playerHealth == null)
            Debug.LogWarning($"[NightRunner] '{name}' não achou um PlayerHealth: morrer não vai reiniciar a noite.", this);
    }

    private void HandleDied()
    {
        if (IsEnding)
            return;

        IsEnding = true;

        if (freezeWolfOnDeath)
            FreezeWolves();

        onDeath?.Invoke();
        OnDeath?.Invoke();

        StartCoroutine(DeathSequence());
    }

    private void HandleAllSlicesCollected()
    {
        if (IsEnding)
            return;

        IsEnding = true;

        onNightSurvived?.Invoke();
        OnNightSurvived?.Invoke();
    }

    /// <summary>
    /// Tempo em realtime, e não em <c>deltaTime</c>: se alguma coisa mexer no
    /// <c>timeScale</c> no meio da morte, a sequência precisa terminar do mesmo jeito —
    /// travada aqui, o jogo ficaria preso na tela do último coração.
    /// </summary>
    private IEnumerator DeathSequence()
    {
        BlockPause();

        yield return new WaitForSecondsRealtime(deathDelay);
        yield return FadeOut();

        if (restartOnDeath)
            Restart();
    }

    private IEnumerator FadeOut()
    {
        if (fadeGroup == null || fadeDuration <= 0f)
            yield break;

        fadeGroup.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 1f;
    }

    /// <summary>Recomeça a noite do zero. Sem save, reabrir a cena zera tudo junto.</summary>
    public void Restart()
    {
        // A cena nova tem que nascer rodando e com som, mesmo se a morte pegou o jogo pausado.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Scene scene = SceneManager.GetActiveScene();

        if (!Application.CanStreamedLevelBeLoaded(scene.name))
        {
            Debug.LogError($"[NightRunner] A cena '{scene.name}' não está no Build Settings — " +
                           "adicione em File > Build Settings, senão morrer não reinicia nada.", this);
            return;
        }

        SceneManager.LoadScene(scene.name);
    }

    /// <summary>
    /// Morta, o ESC não pode mais abrir a pausa: pausar no meio do fade deixaria o pop-up
    /// aberto por cima de uma partida que já acabou, e sem partida pra continuar.
    /// </summary>
    private void BlockPause()
    {
        PauseMenu pause = FindObjectOfType<PauseMenu>();
        if (pause != null)
            pause.enabled = false;
    }

    /// <summary>
    /// Congela o Lobo. O <see cref="EnemyAtk"/> já respeita o <c>MovementLocked</c> do
    /// <see cref="EnemyMov"/>, então travar a movimentação desliga a mordida junto.
    /// </summary>
    private void FreezeWolves()
    {
        foreach (EnemyMov wolf in FindObjectsOfType<EnemyMov>())
            wolf.MovementLocked = true;
    }
}
