using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Os sons de interface: passar o mouse, clicar, abrir e fechar a pausa. Vai num Canvas e
/// se vira sozinho — no Start ele varre os <see cref="Button"/> filhos e pendura hover e
/// clique em cada um, então botão novo na tela não precisa de configuração nenhuma.
/// Um por Canvas: o do menu principal e o da partida têm clipes diferentes.
///
/// A fonte usa <c>ignoreListenerPause</c>: o <see cref="PauseMenu"/> silencia o jogo inteiro
/// ao pausar, e sem isso o próprio som de abrir a pausa sairia mudo.
/// </summary>
public class UISounds : MonoBehaviour
{
    [Header("Clipes")]
    [Tooltip("Mouse entrando em cima de um botão.")]
    [SerializeField] private AudioClip hoverClip;

    [Tooltip("Clique em qualquer botão deste Canvas.")]
    [SerializeField] private AudioClip clickClip;

    [Tooltip("O 'começar' da primeira tela. Não sai sozinho e ainda não tem quem chame: " +
             "quando o menu principal existir, é ligar PlayStart() em quem inicia o jogo.")]
    [SerializeField] private AudioClip startClip;

    [SerializeField] private AudioClip pauseInClip;
    [SerializeField] private AudioClip pauseOutClip;

    [Header("Referências")]
    [Tooltip("Vazio usa o AudioSource deste objeto, e cria um se não houver.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Vazio acha sozinho na cena. Sem PauseMenu, os dois clipes de pausa ficam parados.")]
    [SerializeField] private PauseMenu pauseMenu;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    /// <summary>Botão já pendurado — o <see cref="Rescan"/> não pode dobrar o som.</summary>
    private readonly HashSet<Button> registered = new HashSet<Button>();

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        // O pulo do gato: pausado, o AudioListener está mudo, e o som de pausar sairia junto.
        audioSource.ignoreListenerPause = true;

        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenu>();
    }

    private void OnEnable()
    {
        if (pauseMenu != null)
            pauseMenu.OnPauseChanged += HandlePauseChanged;
    }

    private void OnDisable()
    {
        if (pauseMenu != null)
            pauseMenu.OnPauseChanged -= HandlePauseChanged;
    }

    private void Start() => Rescan();

    /// <summary>
    /// Pendura hover e clique em todo botão filho, inclusive nos desativados — o painel de
    /// pausa nasce desligado. Chame de novo se algum botão for criado depois.
    /// </summary>
    public void Rescan()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
            Register(button);
    }

    public void Register(Button button)
    {
        if (button == null || !registered.Add(button))
            return;

        button.onClick.AddListener(PlayClick);

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ => PlayHover());
        trigger.triggers.Add(entry);
    }

    public void PlayHover() => Play(hoverClip);
    public void PlayClick() => Play(clickClip);

    /// <summary>Pro botão da primeira tela. Ligue no OnClick dele, pelo Inspector.</summary>
    public void PlayStart() => Play(startClip);

    private void HandlePauseChanged(bool paused) => Play(paused ? pauseInClip : pauseOutClip);

    private void Play(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
