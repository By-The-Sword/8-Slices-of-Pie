using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A cutscene final: o Ato III do GDD, do momento em que a Chapéuzinho entra na casa da avó
/// até os créditos. Não há mais jogo aqui, só consequência — o input fica travado o tempo todo
/// e nada do que acontece depende do jogador.
///
/// A sequência: a "avó" na cama é o Lobo e fica alguns segundos em idle · a imagem pisca e por
/// baixo do pelo está o caçador · corte para preto · o som de uma arma sendo carregada · o tiro ·
/// créditos. O relógio da HUD já marca 06:00 sozinho na oitava fatia (<see cref="Clock"/>), então
/// não há nada a travar: ele para porque não existe nona fatia.
///
/// Os dois personagens dividem um único <see cref="SpriteRenderer"/> — a troca é a piscada, e
/// usar o mesmo objeto garante que o caçador nasça exatamente onde o Lobo estava. Os frames de
/// idle são tocados aqui, sem Animator: são 4 quadros por personagem e o beat precisa durar um
/// tempo exato, coisa que um clipe em loop não controla.
///
/// Vai no mesmo objeto de cena do <see cref="NightRunner"/>. Quem chama o <see cref="Play"/> é o
/// <see cref="Teleporter"/> da porta da casa, pelo <c>onTeleported</c>.
/// </summary>
public class EndingSequence : MonoBehaviour
{
    [Header("Palco")]
    [Tooltip("O objeto que agrupa o que a câmera mostra na casa: o corpo na cama, a luz. " +
             "Ligado no início da sequência, então pode ficar desativado na hierarquia até lá.")]
    [SerializeField] private GameObject stage;

    [Tooltip("O corpo na cama. Recebe os frames do Lobo-vovó primeiro e os do caçador depois — " +
             "é o mesmo objeto, é por isso que a troca cai certinha no lugar.")]
    [SerializeField] private SpriteRenderer actor;

    [Header("Idle")]
    [Tooltip("Os 4 frames de WolfVóvozinhaIdle, em ordem.")]
    [SerializeField] private Sprite[] grannyWolfFrames;

    [Tooltip("Os 4 frames de Hunteridle, em ordem.")]
    [SerializeField] private Sprite[] hunterFrames;

    [Tooltip("Quadros por segundo dos dois idles.")]
    [Min(1f)]
    [SerializeField] private float frameRate = 8f;

    [Header("Tempos")]
    [Tooltip("Quanto tempo o Lobo vestido de vovó fica na tela antes de piscar.")]
    [SerializeField] private float grannyWolfDuration = 4f;

    [Tooltip("Quanto tempo o caçador fica na tela antes do corte para preto.")]
    [SerializeField] private float hunterDuration = 4f;

    [Header("Piscada")]
    [Tooltip("Quantas vezes a imagem some e volta na troca de um personagem para o outro.")]
    [Min(0)]
    [SerializeField] private int blinkCount = 3;

    [Tooltip("Duração de cada apagada e de cada acesa da piscada.")]
    [SerializeField] private float blinkInterval = 0.08f;

    [Header("Corte para preto")]
    [Tooltip("CanvasGroup da imagem preta que cobre a tela — o mesmo Fade que o NightRunner usa.")]
    [SerializeField] private CanvasGroup fadeGroup;

    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("A pulseira diegética (o objeto ArmHUD), desligada junto com o corte: os créditos " +
             "não sobem por cima do relógio. Aponte para ela, e NÃO para o Canvas HUD inteiro — " +
             "o Fade mora dentro do Canvas e sumiria junto com a tela preta.")]
    [SerializeField] private GameObject hudContent;

    [Header("O tiro")]
    [Tooltip("Por onde saem os dois sons. Vazio, um AudioSource é criado neste objeto.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("load_gun — a arma sendo carregada, ainda sobre a tela preta.")]
    [SerializeField] private AudioClip gunLoadClip;

    [Tooltip("gun_shooting — o tiro. Ele entendeu o que fez.")]
    [SerializeField] private AudioClip gunShotClip;

    [Tooltip("Silêncio entre a tela ficar preta e a arma começar a ser carregada.")]
    [SerializeField] private float delayBeforeLoad = 1f;

    [Tooltip("Espera entre carregar e atirar. É a pausa dele, não um tempo técnico: curta " +
             "demais vira acidente, longa demais vira suspense e a cena não é sobre isso.")]
    [SerializeField] private float delayBeforeShot = 1.5f;

    [Tooltip("Silêncio depois do tiro, antes dos créditos.")]
    [SerializeField] private float delayAfterShot = 2.5f;

    [Header("Créditos")]
    [Tooltip("Canvas dos créditos. Ligado só no fim, sobre a tela preta.")]
    [SerializeField] private GameObject creditsRoot;

    [Tooltip("O texto que sobe. Vazio, os créditos ficam parados na tela pelo tempo abaixo.")]
    [SerializeField] private RectTransform creditsContent;

    [Tooltip("Quanto o texto sobe, em unidades do Canvas. A rolagem parte de onde ele estiver " +
             "na hierarquia, então posicione o começo da lista no editor.")]
    [SerializeField] private float creditsScrollDistance = 900f;

    [SerializeField] private float creditsDuration = 20f;

    [Tooltip("Voltar ao menu principal quando os créditos terminarem. Desmarcado, a tela fica " +
             "nos créditos e só o ESC do menu... que já está bloqueado. Deixe marcado.")]
    [SerializeField] private bool returnToMenu = true;

    [Tooltip("Precisa estar em File > Build Settings, igual ao do PauseMenu.")]
    [SerializeField] private string menuSceneName = "MainMenu";

    /// <summary>A cutscene já começou — entrar duas vezes na casa não a reinicia.</summary>
    public bool Playing { get; private set; }

    /// <summary>Chamado pelo <c>onTeleported</c> da porta da casa da avó.</summary>
    public void Play()
    {
        if (Playing)
            return;

        Playing = true;
        StartCoroutine(Sequence());
    }

    private void Awake()
    {
        // A fonte é sempre nossa: o MusicDirector mora neste mesmo objeto, cria as dele por
        // AddComponent e mantém o volume delas em 0 entre as faixas. Um GetComponent pegaria
        // uma das duas e os tiros sairiam mudos. Pra reaproveitar outra, use o campo acima.
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // A arma toca sobre a tela preta, sem nenhum objeto de cena por perto: em 3D ela sairia
        // baixa ou muda, dependendo de onde a câmera parou.
        audioSource.spatialBlend = 0f;
    }

    /// <summary>
    /// Tudo em realtime, e não em <c>deltaTime</c>: se alguma coisa mexer no <c>timeScale</c>
    /// antes da última fatia, a cutscene precisa correr do mesmo jeito — travada aqui, o jogo
    /// ficaria preso numa tela preta sem créditos e sem menu.
    /// </summary>
    private IEnumerator Sequence()
    {
        LockPlayer();
        SilenceMusic();

        if (stage != null)
            stage.SetActive(true);

        yield return PlayIdle(grannyWolfFrames, grannyWolfDuration);
        yield return Blink();
        yield return PlayIdle(hunterFrames, hunterDuration);

        yield return Fade(0f, 1f);

        HideHud();

        if (stage != null)
            stage.SetActive(false);

        yield return new WaitForSecondsRealtime(delayBeforeLoad);
        yield return PlayClip(gunLoadClip, delayBeforeShot);
        yield return PlayClip(gunShotClip, delayAfterShot);

        yield return Credits();

        if (returnToMenu)
            GoToMenu();
    }

    /// <summary>
    /// Toca um idle em loop por um tempo fixo. O último quadro pode ficar mais curto que os
    /// outros — a duração do beat manda, e não o ciclo da animação.
    /// </summary>
    private IEnumerator PlayIdle(Sprite[] frames, float duration)
    {
        if (actor == null || frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"[EndingSequence] '{name}' está sem ator ou sem frames: " +
                             "o beat vai passar em branco.", this);
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }

        actor.enabled = true;

        float step = 1f / frameRate;
        float elapsed = 0f;
        int frame = 0;

        while (elapsed < duration)
        {
            actor.sprite = frames[frame % frames.Length];
            frame++;

            yield return new WaitForSecondsRealtime(Mathf.Min(step, duration - elapsed));
            elapsed += step;
        }
    }

    /// <summary>
    /// A troca de personagem. A imagem pisca e, no escuro da última piscada, quem volta é o
    /// caçador — a destransformação do GDD sem nenhum quadro de transição desenhado.
    /// </summary>
    private IEnumerator Blink()
    {
        if (actor == null)
            yield break;

        for (int i = 0; i < blinkCount; i++)
        {
            actor.enabled = false;
            yield return new WaitForSecondsRealtime(blinkInterval);

            actor.enabled = true;
            yield return new WaitForSecondsRealtime(blinkInterval);
        }

        actor.enabled = false;
        yield return new WaitForSecondsRealtime(blinkInterval);
    }

    /// <summary>
    /// Some com a pulseira no corte. Na cena, o Fade é filho do mesmo Canvas da HUD — se o
    /// campo apontar pro Canvas inteiro por engano, desligar levaria a tela preta junto e o
    /// tiro aconteceria à vista do quarto vazio. Melhor não desligar nada e avisar.
    /// </summary>
    private void HideHud()
    {
        if (hudContent == null)
            return;

        if (fadeGroup != null && fadeGroup.transform.IsChildOf(hudContent.transform))
        {
            Debug.LogWarning($"[EndingSequence] '{name}': o Fade está dentro de " +
                             $"'{hudContent.name}'. Aponte o campo pra pulseira (ArmHUD), " +
                             "senão a tela preta some junto. A HUD ficou ligada.", this);
            return;
        }

        hudContent.SetActive(false);
    }

    private IEnumerator PlayClip(AudioClip clip, float wait)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);

        yield return new WaitForSecondsRealtime(wait);
    }

    private IEnumerator Credits()
    {
        if (creditsRoot == null)
            yield break;

        creditsRoot.SetActive(true);

        if (creditsContent == null)
        {
            yield return new WaitForSecondsRealtime(creditsDuration);
            yield break;
        }

        Vector2 start = creditsContent.anchoredPosition;
        Vector2 end = start + Vector2.up * creditsScrollDistance;

        float elapsed = 0f;
        while (elapsed < creditsDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            creditsContent.anchoredPosition =
                Vector2.Lerp(start, end, Mathf.Clamp01(elapsed / creditsDuration));
            yield return null;
        }

        creditsContent.anchoredPosition = end;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeGroup == null)
            yield break;

        // O NightRunner desliga o objeto do Fade depois de clarear a abertura da noite.
        fadeGroup.gameObject.SetActive(true);

        if (fadeDuration <= 0f)
        {
            fadeGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        fadeGroup.alpha = to;
    }

    /// <summary>
    /// Ela para e não volta a andar. O ESC vai junto: pausar no meio da cutscene deixaria o
    /// pop-up aberto por cima de uma partida que já acabou, e sem partida pra continuar.
    /// </summary>
    private void LockPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.Halt();
            player.MovementLocked = true;
        }

        PauseMenu pause = FindObjectOfType<PauseMenu>();
        if (pause != null)
            pause.enabled = false;
    }

    /// <summary>A trilha da noite não sobrevive à porta: daqui pra frente só os dois sons.</summary>
    private void SilenceMusic()
    {
        MusicDirector music = FindObjectOfType<MusicDirector>();
        if (music != null)
            music.FadeOutAll();
    }

    private void GoToMenu()
    {
        // O menu tem que nascer rodando e com som, mesmo se a cutscene pegou o jogo em câmera lenta.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            Debug.LogError($"[EndingSequence] A cena '{menuSceneName}' não está no Build Settings — " +
                           "os créditos não vão levar a lugar nenhum.", this);
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }
}
