using UnityEngine;

/// <summary>
/// A telegrafia do Lobo. O GDD é explícito: como a tela é escura, <b>o áudio é o principal
/// jeito de ler a ameaça</b> — passos pesados e respiração que sobem de volume conforme a
/// distância diminui. Este script não decide nada da caçada, só traduz o que o
/// <see cref="EnemyMov"/> já está fazendo em som.
///
/// Três camadas: a respiração, em loop enquanto ele está no mapa; os passos, disparados por
/// distância percorrida — então correr atrás dela soa mais rápido que rondar, sem configurar
/// nada por estado; e as vozes (rosnado, ataque, uivo, lamento), penduradas nas viradas de
/// estado do <see cref="EnemyMov"/> e na mordida do <see cref="EnemyAtk"/>. Cada categoria
/// aceita várias opções e sorteia sem repetir a anterior.
///
/// O volume é calculado na mão, com o som em 2D, em vez de usar áudio espacial do Unity: o
/// AudioListener vive na câmera, que tem suavização e deadzone (<see cref="CameraFollow"/>),
/// e a distância que importa é do Lobo <b>até a Chapéuzinho</b>, não até onde a câmera parou.
///
/// Antes da 3ª fatia e depois da 8ª ele não está no mapa e não faz som nenhum — quem manda
/// nisso é o <see cref="EnemyHabilities"/>. Vai no mesmo objeto do EnemyMov, no Wolf.prefab.
/// </summary>
[RequireComponent(typeof(EnemyMov))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Fontes")]
    [Tooltip("AudioSource da respiração. Fica em loop; este script cuida do volume.")]
    [SerializeField] private AudioSource breathSource;

    [Tooltip("AudioSource dos passos. Separado da respiração pra um não cortar o outro.")]
    [SerializeField] private AudioSource stepSource;

    [Tooltip("AudioSource das vozes (rosnado, ataque, uivo, lamento). Vazio usa o dos passos.")]
    [SerializeField] private AudioSource voiceSource;

    [Header("Clipes")]
    [Tooltip("Opcionais — a arte de som entra depois. Sem clipe, o script roda calado.")]
    [SerializeField] private AudioClip breathLoop;

    [SerializeField] private AudioClip[] stepClips;

    [Header("Vozes")]
    [Tooltip("Ele a enxergou e partiu pra cima. É o aviso mais importante do jogo: " +
             "separa 'ele está por perto' de 'ele te viu'. Repete durante a perseguição.")]
    [SerializeField] private AudioClip[] growlClips;

    [Tooltip("A mordida conectando.")]
    [SerializeField] private AudioClip[] attackClips;

    [Tooltip("Ambientação: sai sozinho de tempos em tempos na ronda, e é o que o " +
             "EnemyHabilities pode chamar na virada de hora pelo PlayHowl().")]
    [SerializeField] private AudioClip[] howlClips;

    [Tooltip("Ele levou o lampião na cara ou fugiu depois de morder. Avisa que o perigo passou.")]
    [SerializeField] private AudioClip[] lamentClips;

    [Header("Distância")]
    [Tooltip("Daqui pra dentro o som está no volume cheio.")]
    [SerializeField] private float nearDistance = 2f;

    [Tooltip("Daqui pra fora ele é inaudível. Bem além da visão dele (10): a ideia é ela " +
             "ouvir que ele existe antes de ele ter qualquer chance de vê-la.")]
    [SerializeField] private float farDistance = 18f;

    [Tooltip("Como o volume sobe entre longe e perto. O padrão sobe devagar e estoura no fim, " +
             "que é o que faz a aproximação assustar em vez de avisar cedo demais.")]
    [SerializeField]
    private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Range(0f, 1f)]
    [Tooltip("Volume da respiração no máximo. Abaixo dos passos: ela é o fundo, eles são o susto.")]
    [SerializeField] private float breathVolume = 0.6f;

    [Range(0f, 1f)]
    [SerializeField] private float stepVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("Piso das vozes: por mais longe que ele esteja, rosnado e ataque não somem. " +
             "A curva serve pra ler distância; as vozes são informação, e informação tem que chegar.")]
    [SerializeField] private float minVoiceVolume = 0.35f;

    [Header("Direção")]
    [Tooltip("Quanto o som vai pra esquerda ou pra direita conforme o lado em que ele está. " +
             "0 desliga. É o que deixa ela fugir pro lado certo sem enxergar nada.")]
    [Range(0f, 1f)]
    [SerializeField] private float panStrength = 0.7f;

    [Tooltip("Afastamento horizontal que já joga o som todo pra um lado.")]
    [SerializeField] private float panDistance = 8f;

    [Header("Passos")]
    [Tooltip("Distância andada entre um passo e outro. Como é por distância e não por tempo, " +
             "a passada acelera sozinha quando ele sai da ronda pra perseguição.")]
    [SerializeField] private float stepDistance = 0.9f;

    [Header("Perseguição")]
    [Tooltip("Multiplica o volume enquanto ele persegue — ele deixa de se esconder.")]
    [SerializeField] private float chaseVolumeBoost = 1.3f;

    [Tooltip("Pitch da respiração perseguindo: ofegante, e não o mesmo fôlego da ronda.")]
    [SerializeField] private float chaseBreathPitch = 1.2f;

    [Tooltip("Intervalo entre rosnados enquanto persegue (mín, máx). X em 0 rosna só na largada.")]
    [SerializeField] private Vector2 chaseGrowlInterval = new Vector2(3f, 6f);

    [Header("Grama")]
    [Tooltip("Mato mexendo: só sai com ele perto e fora do quadro. Dentro da tela ela já o vê, " +
             "e o susto de ouvir onde não se enxerga é justamente o ponto.")]
    [SerializeField] private AudioClip[] grassClips;

    [Tooltip("Distância máxima pro mato mexer. Menor que a audição geral: é 'ele está aqui do " +
             "lado', não 'ele existe na cidade'.")]
    [SerializeField] private float grassDistance = 9f;

    [Tooltip("Intervalo entre um farfalhar e outro (mín, máx). X em 0 desliga.")]
    [SerializeField] private Vector2 grassInterval = new Vector2(2f, 5f);

    [Range(0f, 1f)]
    [SerializeField] private float grassVolume = 0.8f;

    [Header("Ronda")]
    [Tooltip("Intervalo entre uivos de ambientação na patrulha (mín, máx). X em 0 desliga. " +
             "Espaçado de propósito: uivo demais vira ruído de fundo e para de assustar.")]
    [SerializeField] private Vector2 patrolHowlInterval = new Vector2(25f, 60f);

    private EnemyMov movement;
    private EnemyHabilities habilities;
    private EnemyAtk attack;
    private Rigidbody2D body;
    private Transform player;
    private Camera view;
    private float stepTravel;
    private float basePitch = 1f;
    private float growlTimer;
    private float howlTimer;
    private float grassTimer;

    /// <summary>Último sorteado de cada categoria — é o que impede o mesmo clipe duas vezes seguidas.</summary>
    private int lastStep = -1;
    private int lastGrowl = -1;
    private int lastAttack = -1;
    private int lastHowl = -1;
    private int lastLament = -1;
    private int lastGrass = -1;

    private void Awake()
    {
        movement = GetComponent<EnemyMov>();
        habilities = GetComponent<EnemyHabilities>();
        attack = GetComponent<EnemyAtk>();
        body = GetComponent<Rigidbody2D>();

        // Uma fonte só pros dois já funciona: PlayOneShot sobrepõe em vez de cortar.
        if (voiceSource == null)
            voiceSource = stepSource;

        if (breathSource != null)
        {
            basePitch = breathSource.pitch;
            breathSource.loop = true;
            breathSource.playOnAwake = false;

            // 2D: a distância que vale é a dele até ela, calculada aqui embaixo.
            breathSource.spatialBlend = 0f;

            if (breathSource.clip == null)
                breathSource.clip = breathLoop;
        }

        if (stepSource != null)
            stepSource.spatialBlend = 0f;

        if (voiceSource != null)
            voiceSource.spatialBlend = 0f;

        howlTimer = RandomInterval(patrolHowlInterval);
        grassTimer = RandomInterval(grassInterval);
    }

    private void OnEnable()
    {
        movement.OnStateChanged += HandleStateChanged;

        if (attack != null)
            attack.OnBite += HandleBite;
    }

    private void OnDisable()
    {
        movement.OnStateChanged -= HandleStateChanged;

        if (attack != null)
            attack.OnBite -= HandleBite;

        Silence();
    }

    private void Update()
    {
        // O EnemyMov só resolve a Chapéuzinho no Start dele, e a ordem de Start entre
        // componentes do mesmo objeto não é garantida — mesma razão do EnemyAtk.
        if (player == null)
            player = movement.Target;

        if (!IsAudible())
        {
            Silence();
            return;
        }

        float volume = DistanceVolume();
        float pan = StereoPan();
        bool chasing = movement.State == WolfState.Perseguicao;

        TickBreath(volume, pan, chasing);
        TickFootsteps(volume, pan, chasing);
        TickGrass(pan);
        TickVoices(chasing);
    }

    /// <summary>
    /// O mato mexendo enquanto ele está perto e fora do quadro. É o único som que depende
    /// da câmera, e não da distância: assim que ele entra na tela, cala — a partir daí quem
    /// avisa são os olhos dela.
    /// </summary>
    private void TickGrass(float pan)
    {
        if (grassInterval.x <= 0f || grassClips == null || grassClips.Length == 0)
            return;

        if (!IsOffScreen() || Vector2.Distance(transform.position, player.position) > grassDistance)
        {
            // Rearma: sumindo do quadro de novo, o mato mexe na hora em vez de esperar o ciclo.
            grassTimer = RandomInterval(grassInterval);
            return;
        }

        grassTimer -= Time.deltaTime;
        if (grassTimer > 0f)
            return;

        grassTimer = RandomInterval(grassInterval);

        AudioClip clip = PickRandom(grassClips, ref lastGrass);
        if (clip == null || stepSource == null)
            return;

        stepSource.panStereo = pan;
        stepSource.PlayOneShot(clip, grassVolume);
    }

    /// <summary>Ele está fora do que a câmera mostra. Sem câmera, assume que está à vista.</summary>
    private bool IsOffScreen()
    {
        if (view == null)
            view = Camera.main;

        if (view == null)
            return false;

        Vector3 point = view.WorldToViewportPoint(transform.position);
        return point.x < 0f || point.x > 1f || point.y < 0f || point.y > 1f;
    }

    /// <summary>
    /// As duas vozes que saem sozinhas: o rosnado que ele repete enquanto está em cima dela,
    /// e o uivo espaçado da ronda — o que faz ele existir no mapa mesmo quando não achou nada.
    /// </summary>
    private void TickVoices(bool chasing)
    {
        if (chasing)
        {
            howlTimer = RandomInterval(patrolHowlInterval);

            if (chaseGrowlInterval.x <= 0f)
                return;

            growlTimer -= Time.deltaTime;
            if (growlTimer > 0f)
                return;

            growlTimer = RandomInterval(chaseGrowlInterval);
            PlayGrowl();
            return;
        }

        // Fora da perseguição o rosnado rearma, pro próximo avistamento sair na hora.
        growlTimer = 0f;

        if (movement.State != WolfState.Patrulha || patrolHowlInterval.x <= 0f)
            return;

        howlTimer -= Time.deltaTime;
        if (howlTimer > 0f)
            return;

        howlTimer = RandomInterval(patrolHowlInterval);
        PlayHowl();
    }

    /// <summary>Ele existe, está no mapa e tem quem ouvir.</summary>
    private bool IsAudible()
    {
        if (player == null || movement.MovementLocked)
            return false;

        return habilities == null || habilities.IsPresent;
    }

    private void TickBreath(float volume, float pan, bool chasing)
    {
        if (breathSource == null || breathSource.clip == null)
            return;

        breathSource.volume = volume * breathVolume * (chasing ? chaseVolumeBoost : 1f);
        breathSource.pitch = chasing ? basePitch * chaseBreathPitch : basePitch;
        breathSource.panStereo = pan;

        if (!breathSource.isPlaying)
            breathSource.Play();
    }

    /// <summary>
    /// Passo a cada <see cref="stepDistance"/> percorrida de verdade. Parado ele não pisa,
    /// mesmo que a IA ainda esteja num estado "andando" — a pausa da ronda é silêncio.
    /// </summary>
    private void TickFootsteps(float volume, float pan, bool chasing)
    {
        if (!movement.IsMoving || body == null)
        {
            stepTravel = stepDistance; // rearma: o próximo passo sai assim que ele anda.
            return;
        }

        stepTravel += body.velocity.magnitude * Time.deltaTime;

        if (stepTravel < stepDistance)
            return;

        stepTravel = 0f;
        PlayStep(volume * stepVolume * (chasing ? chaseVolumeBoost : 1f), pan);
    }

    private void PlayStep(float volume, float pan)
    {
        if (stepSource == null || volume <= 0.001f)
            return;

        AudioClip clip = PickRandom(stepClips, ref lastStep);
        if (clip == null)
            return;

        stepSource.panStereo = pan;
        stepSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    // ------------------------------------------------------------------ vozes

    /// <summary>
    /// Rosnado. Sai quando ele a enxerga e se repete durante a perseguição — é o único
    /// aviso de que ele deixou de procurar e passou a vir.
    /// </summary>
    public void PlayGrowl() => PlayVoice(growlClips, ref lastGrowl);

    /// <summary>A mordida conectando. Chamado pelo <see cref="EnemyAtk.OnBite"/>.</summary>
    public void PlayAttack() => PlayVoice(attackClips, ref lastAttack);

    /// <summary>
    /// Uivo, sempre no volume cheio — é o som que atravessa a cidade, e o "distante" vem
    /// da mixagem do clipe, não da posição dele. Público e sem argumento de propósito: dá
    /// pra chamar pelo <c>onReached</c> de um estágio do <see cref="EnemyHabilities"/>, que
    /// é como o uivo da 1ª fatia sai sorteado em vez de ser sempre o mesmo clipe.
    /// </summary>
    public void PlayHowl() => PlayVoice(howlClips, ref lastHowl, fullVolume: true);

    /// <summary>Ganido de quem levou luz na cara ou fugiu depois de morder.</summary>
    public void PlayLament() => PlayVoice(lamentClips, ref lastLament);

    private void PlayVoice(AudioClip[] clips, ref int last, bool fullVolume = false)
    {
        if (voiceSource == null)
            return;

        AudioClip clip = PickRandom(clips, ref last);
        if (clip == null)
            return;

        voiceSource.panStereo = StereoPan();
        voiceSource.PlayOneShot(clip, fullVolume ? 1f : VoiceVolume());
    }

    private void HandleStateChanged(WolfState state)
    {
        if (!IsAudible())
            return;

        if (state == WolfState.Perseguicao)
        {
            growlTimer = RandomInterval(chaseGrowlInterval);
            PlayGrowl();
        }
        else if (state == WolfState.Recuo)
        {
            PlayLament();
        }
    }

    private void HandleBite(int damage) => PlayAttack();

    /// <summary>
    /// Sorteia sem repetir o clipe anterior: com poucas variações, duas iguais seguidas
    /// entregam na hora que é som de biblioteca e o susto vira piada.
    /// </summary>
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

    /// <summary>
    /// Volume das vozes: a mesma curva dos passos, mas com piso. Rosnado e ataque continuam
    /// dizendo "ele está perto" pelo volume, sem correr o risco de sumirem quando importa.
    /// </summary>
    private float VoiceVolume()
    {
        if (player == null)
            return minVoiceVolume;

        return Mathf.Clamp01(Mathf.Max(minVoiceVolume, DistanceVolume()));
    }

    /// <summary>1 colado nela, 0 no limite da audição.</summary>
    private float DistanceVolume()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        float t = Mathf.InverseLerp(farDistance, nearDistance, distance);
        return Mathf.Clamp01(falloff.Evaluate(t));
    }

    /// <summary>-1 todo à esquerda dela, +1 todo à direita.</summary>
    private float StereoPan()
    {
        if (player == null || panStrength <= 0f || panDistance <= 0f)
            return 0f;

        float offset = transform.position.x - player.position.x;
        return Mathf.Clamp(offset / panDistance, -1f, 1f) * panStrength;
    }

    private void Silence()
    {
        if (breathSource != null && breathSource.isPlaying)
            breathSource.Stop();

        stepTravel = stepDistance;
        grassTimer = RandomInterval(grassInterval);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, farDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, nearDistance);
    }
}
