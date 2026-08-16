using UnityEngine;

/// <summary>
/// A trilha da noite. Duas faixas que nunca se sobrepõem: a <b>de fundo</b>, que sai de vez
/// em quando com silêncio de verdade entre uma e outra, e a <b>de tensão</b>, que entra
/// enquanto o Lobo está caçando — farejando o rastro dela ou já em cima.
///
/// O silêncio entre as faixas não é economia, é regra: o GDD põe o áudio como o principal
/// jeito de ler a ameaça, e música tocando direto mascara o passo e a respiração do Lobo
/// (<see cref="EnemyAudio"/>). Música aqui é evento, não fundo permanente.
///
/// A exclusão mútua é por construção: existe uma faixa atual só, e trocar de faixa é o mesmo
/// caminho de código que apagar a anterior. Não dá pra as duas saírem juntas nem esquecendo
/// de parar uma antes da outra. As duas AudioSources existem só pro crossfade — a que está
/// saindo e a que está entrando —, e não pra duas faixas simultâneas.
///
/// Vai no mesmo objeto de cena do <see cref="Clock"/> e do <see cref="NightRunner"/>.
/// </summary>
public class MusicDirector : MonoBehaviour
{
    [Header("Fontes")]
    [Tooltip("As duas pontas do crossfade. Vazio, o script cria as duas sozinho no Awake.")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("Faixas")]
    [Tooltip("Toca de vez em quando quando o Lobo não está atrás dela. Aceita várias e sorteia " +
             "sem repetir a anterior.")]
    [SerializeField] private AudioClip[] backgroundTracks;

    [Tooltip("Entra quando ele fareja ou persegue. Fica em loop enquanto a caçada durar.")]
    [SerializeField] private AudioClip[] tensionTracks;

    [Range(0f, 1f)]
    [SerializeField] private float backgroundVolume = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Acima da de fundo: quando ela entra, é pra ser notada.")]
    [SerializeField] private float tensionVolume = 0.7f;

    [Header("Silêncio")]
    [Tooltip("Quanto tempo de silêncio entre uma faixa de fundo e a próxima (mín, máx). " +
             "É o intervalo em que ela consegue ouvir o Lobo — apertar isto demais cega o jogo.")]
    [SerializeField] private Vector2 backgroundGap = new Vector2(25f, 70f);

    [Tooltip("Espera antes da primeira faixa, pra noite não abrir com música por cima do fade.")]
    [SerializeField] private float firstTrackDelay = 12f;

    [Header("Fades")]
    [SerializeField] private float backgroundFade = 2.5f;

    [Tooltip("Rápido de propósito: o susto tem que chegar junto com ele, não depois.")]
    [SerializeField] private float tensionFadeIn = 0.5f;

    [Tooltip("Lento de propósito: o alívio é gradual, senão o silêncio de volta entrega " +
             "na hora que ele desistiu.")]
    [SerializeField] private float tensionFadeOut = 3f;

    [Header("Caçada")]
    [Tooltip("Tempo mínimo de tensão depois que ela começa. Sem piso, um farejo de 1 segundo " +
             "faria a trilha piscar entre as duas faixas.")]
    [SerializeField] private float tensionMinDuration = 6f;

    [Tooltip("Quanto a tensão ainda segura depois que ele desiste. É o que impede a música de " +
             "avisar que o perigo passou antes de ela ter certeza disso.")]
    [SerializeField] private float tensionHold = 4f;

    /// <summary>Faixa no ar. Nunca mais de uma.</summary>
    private enum Track { None, Background, Tension }

    /// <summary>A tensão está tocando — pra debug e pra quem mais quiser reagir à caçada.</summary>
    public bool IsTense => playing == Track.Tension;

    private readonly AudioSource[] sources = new AudioSource[2];
    private readonly float[] targetVolume = new float[2];
    private readonly float[] fadeRate = new float[2];

    /// <summary>Índice da fonte que carrega a faixa atual. -1 é silêncio.</summary>
    private int active = -1;

    private Track playing = Track.None;
    private float gapTimer;
    private float tensionMinUntil;
    private float tensionReleaseAt;

    private EnemyMov[] wolves;
    private EnemyHabilities[] wolfStages;

    private int lastBackground = -1;
    private int lastTension = -1;

    private void Awake()
    {
        sources[0] = Prepare(sourceA);
        sources[1] = Prepare(sourceB);

        gapTimer = firstTrackDelay;
    }

    private void Start()
    {
        // Uma varredura só: o Lobo não é instanciado no meio da noite. O EnemyHabilities
        // esconde e mostra o mesmo objeto, que existe na cena desde o começo — é por isso
        // que guardar as referências aqui basta.
        wolves = FindObjectsOfType<EnemyMov>();
        wolfStages = new EnemyHabilities[wolves.Length];

        for (int i = 0; i < wolves.Length; i++)
            wolfStages[i] = wolves[i].GetComponent<EnemyHabilities>();

        // Sem Lobo a tensão nunca entra e só a de fundo toca: cena de teste, não erro fatal.
        if (wolves.Length == 0)
            Debug.LogWarning($"[MusicDirector] '{name}' não achou nenhum Lobo na cena: " +
                             "a faixa de tensão não vai entrar.", this);
    }

    private void Update()
    {
        TickFades();

        if (IsAnyWolfHunting())
        {
            if (playing != Track.Tension)
                StartTension();

            // Empurrado a cada frame de caçada: a soltura só começa a contar quando ele para.
            tensionReleaseAt = Time.time + tensionHold;
            return;
        }

        if (playing == Track.Tension)
        {
            if (Time.time >= tensionMinUntil && Time.time >= tensionReleaseAt)
                EndTension();

            return;
        }

        TickBackground();
    }

    // ----------------------------------------------------------------- caçada

    /// <summary>
    /// Ele está atrás dela: farejando o último ponto conhecido ou já em cima. Patrulha não
    /// conta — ele ainda não sabe que ela existe, e a trilha não pode entregar isso. Recuo
    /// também não: aí a caçada já acabou.
    /// </summary>
    private bool IsAnyWolfHunting()
    {
        if (wolves == null)
            return false;

        for (int i = 0; i < wolves.Length; i++)
        {
            EnemyMov wolf = wolves[i];

            // MovementLocked cobre o Lobo fora do mapa e o congelamento da morte (NightRunner).
            if (wolf == null || wolf.MovementLocked)
                continue;

            EnemyHabilities stage = wolfStages[i];
            if (stage != null && !stage.IsPresent)
                continue;

            if (wolf.State == WolfState.Suspeita || wolf.State == WolfState.Perseguicao)
                return true;
        }

        return false;
    }

    // ----------------------------------------------------------------- faixas

    private void StartTension()
    {
        AudioClip clip = PickRandom(tensionTracks, ref lastTension);

        // Sem clipe de tensão a de fundo continua como estava: silenciar seria pior.
        if (clip == null)
            return;

        Play(clip, tensionVolume, tensionFadeIn, tensionFadeOut, loop: true);
        playing = Track.Tension;
        tensionMinUntil = Time.time + tensionMinDuration;
    }

    private void EndTension()
    {
        Silence(tensionFadeOut);
        playing = Track.None;

        // O silêncio depois da caçada é o alívio: a de fundo não volta em cima do fade.
        gapTimer = RandomInterval(backgroundGap);
    }

    private void TickBackground()
    {
        if (playing == Track.Background)
        {
            // A de fundo não repete: quando acaba, começa o silêncio até a próxima.
            if (active < 0 || !sources[active].isPlaying)
            {
                // Zera o alvo junto: sem isto a fonte parada continuaria perseguindo o
                // volume da faixa que já acabou, e voltaria a tocar no volume errado.
                Fade(active, 0f, 0.01f);

                playing = Track.None;
                active = -1;
                gapTimer = RandomInterval(backgroundGap);
            }

            return;
        }

        gapTimer -= Time.deltaTime;
        if (gapTimer > 0f)
            return;

        AudioClip clip = PickRandom(backgroundTracks, ref lastBackground);

        // Sem clipe nenhum, rearma o relógio em vez de tentar todo frame.
        if (clip == null)
        {
            gapTimer = RandomInterval(backgroundGap);
            return;
        }

        Play(clip, backgroundVolume, backgroundFade, backgroundFade, loop: false);
        playing = Track.Background;
    }

    // --------------------------------------------------------------- crossfade

    /// <summary>
    /// Põe a faixa nova na fonte livre e manda a anterior embora no mesmo movimento. Trocar
    /// de faixa e apagar a antiga são o mesmo caminho — daí a exclusão mútua sair de graça.
    /// </summary>
    private void Play(AudioClip clip, float volume, float fadeIn, float fadeOut, bool loop)
    {
        int next = active == 0 ? 1 : 0;

        Fade(active, 0f, fadeOut);

        AudioSource source = sources[next];
        source.clip = clip;
        source.loop = loop;
        source.volume = 0f;
        source.Play();

        Fade(next, volume, fadeIn);
        active = next;
    }

    private void Silence(float fadeOut)
    {
        Fade(active, 0f, fadeOut);
        active = -1;
    }

    /// <summary>Fade de todas as faixas. Pra pendurar no <c>onDeath</c> do NightRunner.</summary>
    public void FadeOutAll()
    {
        for (int i = 0; i < sources.Length; i++)
            Fade(i, 0f, backgroundFade);

        active = -1;
        playing = Track.None;
        gapTimer = float.PositiveInfinity;
    }

    private void Fade(int index, float volume, float duration)
    {
        if (index < 0 || sources[index] == null)
            return;

        targetVolume[index] = volume;

        // Piso no tempo: duração zero viraria divisão por zero, e o corte seco é justamente
        // o que o crossfade existe pra evitar.
        fadeRate[index] = Mathf.Abs(sources[index].volume - volume) / Mathf.Max(0.01f, duration);
    }

    private void TickFades()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];

            if (source == null)
                continue;

            source.volume = Mathf.MoveTowards(source.volume, targetVolume[i], fadeRate[i] * Time.deltaTime);

            // Chegou no silêncio: para de verdade, senão a fonte fica girando o clipe mudo.
            if (source.volume <= 0.001f && targetVolume[i] <= 0f && source.isPlaying)
            {
                source.Stop();
                source.clip = null;
            }
        }
    }

    // ---------------------------------------------------------------- utilidades

    private AudioSource Prepare(AudioSource source)
    {
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 0f; // trilha é 2D: não tem posição no mundo.
        source.volume = 0f;
        source.Stop();

        return source;
    }

    /// <summary>Sorteia sem repetir a anterior — mesma regra dos clipes do <see cref="EnemyAudio"/>.</summary>
    private static AudioClip PickRandom(AudioClip[] clips, ref int last)
    {
        if (clips == null || clips.Length == 0)
            return null;

        if (clips.Length == 1)
        {
            last = 0;
            return clips[0];
        }

        int index = Random.Range(0, clips.Length);
        if (index == last)
            index = (index + 1) % clips.Length;

        last = index;
        return clips[index];
    }

    private static float RandomInterval(Vector2 range)
    {
        return Random.Range(range.x, Mathf.Max(range.x, range.y));
    }
}
