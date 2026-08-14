using UnityEngine;

/// <summary>Estados de IA do Lobo (GDD). RECUO entra junto com a regra do lampião.</summary>
public enum WolfState
{
    Patrulha,
    Suspeita,
    Perseguicao,
    Recuo
}

/// <summary>
/// Movimentação do Lobo Mau. PATRULHA por pontos aleatórios do mapa até perceber a
/// Chapéuzinho — por proximidade ou por ruído — e aí entra em SUSPEITA e vai até o
/// último lugar onde ela esteve. Com linha de visão limpa vira PERSEGUIÇÃO.
/// Agachada ela não é percebida (GDD): nem pela proximidade, nem pela visão.
/// Entrar no círculo do lampião aceso interrompe qualquer estado e joga ele em RECUO,
/// onde ele não morde — é a fraqueza dele, válida até a 7ª fatia (ver <see cref="FearsLight"/>).
/// Nunca é derrotável e não tem combate aqui — quem morde é o EnemyAtk, que chama
/// <see cref="Retreat"/> pra ele disparar pro ponto livre mais longe dela.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMov : MonoBehaviour
{
    [Header("Alvo")]
    [Tooltip("Deixe vazio pra achar sozinho pela tag Player.")]
    [SerializeField] private Transform player;

    [Header("Velocidades")]
    [SerializeField] private float patrolSpeed = 1.8f;
    [SerializeField] private float suspicionSpeed = 2.8f;

    [Tooltip("Acima da velocidade da Chapéuzinho (3.5): a fuga é por rota, não por corrida.")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Percepção")]
    [Tooltip("Sente a presença dela mesmo sem enxergar — dispara a SUSPEITA.")]
    [SerializeField] private float senseRadius = 6f;

    [Tooltip("Enxerga e parte pra cima dela. Precisa de linha de visão limpa.")]
    [SerializeField] private float sightRadius = 10f;

    [Tooltip("Camadas que bloqueiam visão e movimento (paredes, fachadas, cenário).")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Patrulha")]
    [Tooltip("Raio ao redor do ponto onde ele nasce em que os destinos são sorteados.")]
    [SerializeField] private float patrolRadius = 14f;

    [Tooltip("Descarta destinos perto demais, senão ele fica se arrastando no mesmo canto.")]
    [SerializeField] private float minPatrolDistance = 4f;

    [SerializeField] private Vector2 patrolPauseRange = new Vector2(0.5f, 2f);

    [Header("Suspeita")]
    [Tooltip("Tempo farejando no ponto investigado antes de voltar a patrulhar.")]
    [SerializeField] private float searchDuration = 3f;

    [Tooltip("Quanto tempo ele ainda corre atrás do último ponto conhecido depois de perdê-la.")]
    [SerializeField] private float loseSightGrace = 2f;

    [Header("Recuo")]
    [Tooltip("Ele foge mais rápido do que persegue: a mordida é o fim da caçada.")]
    [SerializeField] private float retreatSpeed = 5f;

    [Tooltip("Até onde ele procura o ponto de fuga no sentido contrário ao dela.")]
    [SerializeField] private float retreatDistance = 16f;

    [Tooltip("Tempo mínimo fugindo. Segura a janela dela mesmo se ele estiver num beco e o " +
             "ponto de fuga sair logo ali — sem isso ele voltaria a morder na sequência.")]
    [SerializeField] private float retreatMinDuration = 3f;

    [Tooltip("Prende a fuga na área de ronda. Sem isto, e sem paredes no Obstacle Mask, " +
             "ele corre a distância inteira em linha reta e vai parar fora do mapa.")]
    [SerializeField] private bool keepRetreatInPatrolArea = true;

    [Tooltip("Abertura do leque de direções testadas em volta do sentido contrário.")]
    [Range(0f, 340f)]
    [SerializeField] private float retreatConeAngle = 160f;

    [Tooltip("Quantas direções são testadas dentro do leque.")]
    [Range(3, 21)]
    [SerializeField] private int retreatSamples = 11;

    [Header("Lampião")]
    [Tooltip("Foge do círculo de luz acesa. O EnemyHabilities desliga isto na 7ª fatia, quando " +
             "a luz deixa de assustá-lo e passa a atraí-lo (GDD).")]
    [SerializeField] private bool fearsLight = true;

    [Header("Corpo")]
    [Tooltip("Raio usado pra desviar de parede e pra validar destino de patrulha.")]
    [SerializeField] private float bodyRadius = 0.4f;

    [Tooltip("Distância à frente em que ele já começa a contornar o obstáculo.")]
    [SerializeField] private float avoidLookAhead = 1.2f;

    [SerializeField] private float arrivalDistance = 0.35f;

    public WolfState State { get; private set; } = WolfState.Patrulha;

    /// <summary>A Chapéuzinho, já resolvida pela tag — o EnemyAtk reaproveita esta.</summary>
    public Transform Target => player;

    /// <summary>Direção que ele encara, travada nas 4 cardeais pras animações.</summary>
    public Vector2 Facing { get; private set; } = Vector2.down;

    public bool IsMoving { get; private set; }

    /// <summary>Trava a IA sem desligar o componente (cutscene, 8ª fatia, menu).</summary>
    public bool MovementLocked { get; set; }

    /// <summary>Teme a luz. Vira false na 7ª fatia, quando o lampião passa a atraí-lo (GDD).</summary>
    public bool FearsLight
    {
        get => fearsLight;
        set => fearsLight = value;
    }

    /// <summary>Sente a presença dela sem enxergar. O <see cref="EnemyHabilities"/> abre isto na 4ª.</summary>
    public float SenseRadius
    {
        get => senseRadius;
        set => senseRadius = Mathf.Max(0f, value);
    }

    /// <summary>Alcance da visão. Aberto pelo <see cref="EnemyHabilities"/> conforme a noite avança.</summary>
    public float SightRadius
    {
        get => sightRadius;
        set => sightRadius = Mathf.Max(0f, value);
    }

    /// <summary>Velocidade de perseguição — o que deixa ele "mais rápido" a cada hora (GDD).</summary>
    public float ChaseSpeed
    {
        get => chaseSpeed;
        set => chaseSpeed = Mathf.Max(0f, value);
    }

    /// <summary>
    /// Está dentro do círculo do lampião aceso: não morde e foge. Calculado na hora, e não
    /// guardado num campo, pra valer no mesmo frame em que ela aperta F — senão a ordem de
    /// Update entre este script e o EnemyAtk decidiria se a mordida ainda sai.
    /// </summary>
    public bool IsRepelledByLight =>
        fearsLight && lantern != null && lantern.Illuminates(transform.position);

    public event System.Action<WolfState> OnStateChanged;

    private Rigidbody2D body;
    private PlayerController playerController;
    private Lantern lantern;
    private Vector2 patrolAnchor;
    private Vector2 destination;
    private float stateTimer;
    private float repathTimer;
    private float lastSeenTime = float.NegativeInfinity;
    private bool patrolWaiting;
    private bool searching;

    /// <summary>De onde ele fugiu — serve pra reorientar a fuga se ele chegar cedo demais.</summary>
    private Vector2 retreatThreat;

    /// <summary>Ângulos de desvio testados em ordem quando o caminho reto está bloqueado.</summary>
    private static readonly float[] AvoidAngles = { 0f, 30f, -30f, 60f, -60f, 90f, -90f };

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        patrolAnchor = transform.position;
        destination = patrolAnchor;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found.transform;
        }

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            lantern = player.GetComponent<Lantern>();
        }

        PickPatrolDestination();
    }

    private void OnEnable() => Noise.OnNoise += HandleNoise;
    private void OnDisable() => Noise.OnNoise -= HandleNoise;

    private void Update()
    {
        if (MovementLocked)
            return;

        stateTimer -= Time.deltaTime;

        // A luz vem antes de tudo: pegou ele dentro do círculo, larga o que estava fazendo
        // — inclusive a perseguição, a um passo da mordida — e sai correndo.
        if (IsRepelledByLight && State != WolfState.Recuo)
        {
            Retreat(lantern.LightCenter);
            return;
        }

        switch (State)
        {
            case WolfState.Patrulha:
                TickPatrol();
                break;
            case WolfState.Suspeita:
                TickSuspicion();
                break;
            case WolfState.Perseguicao:
                TickChase();
                break;
            case WolfState.Recuo:
                TickRetreat();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (MovementLocked)
        {
            body.velocity = Vector2.zero;
            IsMoving = false;
            return;
        }

        // Parado de propósito: pausa da patrulha ou farejando o ponto investigado.
        if (IsHolding())
        {
            body.velocity = Vector2.zero;
            IsMoving = false;
            return;
        }

        Vector2 direction = SteerTowards(destination);
        body.velocity = direction * CurrentSpeed();

        IsMoving = direction.sqrMagnitude > 0.01f;
        if (IsMoving)
            Facing = ToCardinal(direction);
    }

    // ---------------------------------------------------------------- estados

    private void TickPatrol()
    {
        if (TrySpotPlayer())
            return;

        repathTimer -= Time.deltaTime;

        if (patrolWaiting)
        {
            if (stateTimer <= 0f)
            {
                patrolWaiting = false;
                PickPatrolDestination();
            }
            return;
        }

        // Chegou, ou empacou tentando chegar — para um tempo antes do próximo destino.
        if (ReachedDestination() || repathTimer <= 0f)
        {
            patrolWaiting = true;
            stateTimer = Random.Range(patrolPauseRange.x, patrolPauseRange.y);
        }
    }

    private void TickSuspicion()
    {
        if (TrySpotPlayer())
            return;

        repathTimer -= Time.deltaTime;

        // A contagem do farejo só começa quando ele chega — ou desiste de chegar — no ponto.
        if (!searching)
        {
            if (!ReachedDestination() && repathTimer > 0f)
                return;

            searching = true;
            stateTimer = searchDuration;
            return;
        }

        // Fareja no lugar; se não achar nada, a caçada esfria e ele volta à rotina.
        if (stateTimer <= 0f)
        {
            SetState(WolfState.Patrulha);
            patrolWaiting = false;
            PickPatrolDestination();
        }
    }

    private void TickChase()
    {
        if (player == null)
        {
            BeginSuspicion(destination);
            return;
        }

        if (CanSeePlayer())
        {
            lastSeenTime = Time.time;
            destination = player.position;
            return;
        }

        // Perdeu de vista: ainda insiste um pouco no último ponto conhecido.
        if (Time.time - lastSeenTime >= loseSightGrace)
            BeginSuspicion(destination);
    }

    /// <summary>
    /// Fugindo. Enquanto corre ele não vê nem escuta nada — é a janela de escape dela.
    /// Só volta a patrulhar depois de chegar no ponto E de cumprir o tempo mínimo de fuga:
    /// uma mordida por encontro, senão ela morre no mesmo canto sem chance de reagir.
    /// </summary>
    private void TickRetreat()
    {
        repathTimer -= Time.deltaTime;

        if (!ReachedDestination() && repathTimer > 0f)
            return;

        // Ainda dentro da luz: reorienta a partir de onde ela está agora e segue fugindo,
        // sem prazo. Enquanto o lampião estiver aceso em cima dele, não existe volta à ronda.
        if (IsRepelledByLight)
        {
            retreatThreat = lantern.LightCenter;
            ExtendRetreat();
            return;
        }

        // Chegou (ou empacou) antes da hora: escolhe outro ponto e continua fugindo.
        if (stateTimer > 0f)
        {
            ExtendRetreat();
            return;
        }

        // A âncora nunca muda: a ronda dele é aquela área e a fuga é só um desvio dela.
        patrolWaiting = false;
        SetState(WolfState.Patrulha);
        PickPatrolDestination();
    }

    /// <summary>Reorienta a fuga a partir de onde ele está, sem reiniciar o tempo mínimo.</summary>
    private void ExtendRetreat()
    {
        Vector2 origin = transform.position;
        Vector2 away = origin - retreatThreat;
        away = away.sqrMagnitude < 0.0001f ? Facing : away.normalized;

        destination = FindRetreatPoint(origin, away, retreatThreat);

        // Piso no orçamento: encurralado, o destino cai em cima dele e isso repetiria todo frame.
        repathTimer = Mathf.Max(0.5f, EstimateTravelTime(destination));
    }

    /// <summary>
    /// Dispara pro ponto livre mais longe da ameaça, no sentido contrário a ela.
    /// Dois gatilhos: o EnemyAtk logo depois da mordida conectar, e o círculo do lampião
    /// aceso — este último segura ele em fuga enquanto a luz continuar em cima dele.
    /// </summary>
    public void Retreat(Vector2 threatPosition)
    {
        Vector2 origin = transform.position;
        Vector2 away = origin - threatPosition;

        // Colados em cima um do outro: usa a cara dele como sentido de fuga.
        away = away.sqrMagnitude < 0.0001f ? -Facing : away.normalized;

        retreatThreat = threatPosition;
        destination = FindRetreatPoint(origin, away, threatPosition);
        searching = false;
        patrolWaiting = false;
        SetState(WolfState.Recuo);
        stateTimer = retreatMinDuration;
        repathTimer = EstimateTravelTime(destination);
    }

    /// <summary>
    /// Abre um leque de direções em volta do sentido contrário, mede até onde cada uma vai
    /// sem bater em parede e fica com o ponto que mais afasta ele da Chapéuzinho.
    /// </summary>
    private Vector2 FindRetreatPoint(Vector2 origin, Vector2 away, Vector2 threat)
    {
        Vector2 best = origin;
        float bestDistance = float.NegativeInfinity;

        // Teto de afastamento da âncora. Se a caçada já o tirou da área, o limite é onde ele
        // está: a fuga não o puxa de volta pra cima dela, só não o deixa ir mais pra longe.
        float leash = Mathf.Max(patrolRadius, Vector2.Distance(origin, patrolAnchor));

        for (int i = 0; i < retreatSamples; i++)
        {
            float t = retreatSamples > 1 ? i / (retreatSamples - 1f) : 0.5f;
            float angle = Mathf.Lerp(-retreatConeAngle * 0.5f, retreatConeAngle * 0.5f, t);
            Vector2 direction = Rotate(away, angle);

            RaycastHit2D hit = Physics2D.CircleCast(origin, bodyRadius, direction, retreatDistance, obstacleMask);
            float reach = hit ? Mathf.Max(0f, hit.distance - bodyRadius) : retreatDistance;

            Vector2 candidate = origin + direction * reach;

            if (keepRetreatInPatrolArea)
                candidate = ClampToLeash(candidate, leash);

            float distance = Vector2.Distance(candidate, threat);

            if (distance > bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>Promove pra PERSEGUIÇÃO se enxerga, ou pra SUSPEITA se só sente a presença.</summary>
    private bool TrySpotPlayer()
    {
        if (player == null || !CanSensePlayer())
            return false;

        if (CanSeePlayer())
        {
            lastSeenTime = Time.time;
            destination = player.position;
            patrolWaiting = false;
            SetState(WolfState.Perseguicao);
            return true;
        }

        if (Vector2.Distance(transform.position, player.position) <= senseRadius)
        {
            BeginSuspicion(player.position);
            return true;
        }

        return false;
    }

    private void BeginSuspicion(Vector2 point)
    {
        destination = point;
        searching = false;
        patrolWaiting = false;
        SetState(WolfState.Suspeita);
        repathTimer = EstimateTravelTime(point);
    }

    private void SetState(WolfState next)
    {
        if (State == next)
            return;

        State = next;
        OnStateChanged?.Invoke(State);
    }

    // -------------------------------------------------------------- percepção

    /// <summary>Agachada ela some da percepção dele — é a regra do agachar (GDD).</summary>
    private bool CanSensePlayer()
    {
        return player != null && (playerController == null || !playerController.IsCrouched);
    }

    private bool CanSeePlayer()
    {
        if (!CanSensePlayer())
            return false;

        Vector2 origin = transform.position;
        Vector2 target = player.position;

        if (Vector2.Distance(origin, target) > sightRadius)
            return false;

        return Physics2D.Linecast(origin, target, obstacleMask).collider == null;
    }

    private void HandleNoise(NoiseEvent noise)
    {
        // Perseguindo ele já sabe onde ela está; fugindo ele está surdo de propósito.
        if (MovementLocked || noise.Source == gameObject)
            return;

        if (State == WolfState.Perseguicao || State == WolfState.Recuo)
            return;

        if (Vector2.Distance(transform.position, noise.Position) > noise.Radius)
            return;

        BeginSuspicion(noise.Position);
    }

    // -------------------------------------------------------------- navegação

    private void PickPatrolDestination()
    {
        Vector2 current = transform.position;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 candidate = patrolAnchor + Random.insideUnitCircle * patrolRadius;

            if (Vector2.Distance(candidate, current) < minPatrolDistance)
                continue;

            if (Physics2D.OverlapCircle(candidate, bodyRadius, obstacleMask) != null)
                continue;

            destination = candidate;
            repathTimer = EstimateTravelTime(candidate);
            return;
        }

        // Cercado de parede por todo lado: volta pro ponto de origem e tenta de novo depois.
        destination = patrolAnchor;
        repathTimer = EstimateTravelTime(patrolAnchor);
    }

    /// <summary>
    /// Desvio simples: tenta ir reto e, se tiver parede no caminho, vai abrindo o ângulo
    /// até achar passagem. Não é pathfinding — o mapa é aberto e as casas são trancadas.
    /// </summary>
    private Vector2 SteerTowards(Vector2 target)
    {
        Vector2 origin = transform.position;
        Vector2 toTarget = target - origin;

        if (toTarget.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        Vector2 desired = toTarget.normalized;
        float lookAhead = Mathf.Min(avoidLookAhead, toTarget.magnitude);

        foreach (float angle in AvoidAngles)
        {
            Vector2 candidate = Rotate(desired, angle);
            if (!Physics2D.CircleCast(origin, bodyRadius, candidate, lookAhead, obstacleMask))
                return candidate;
        }

        return desired;
    }

    /// <summary>Puxa o ponto pra borda da coleira, mantendo a direção da fuga.</summary>
    private Vector2 ClampToLeash(Vector2 point, float leash)
    {
        Vector2 offset = point - patrolAnchor;

        if (offset.sqrMagnitude <= leash * leash)
            return point;

        return patrolAnchor + offset.normalized * leash;
    }

    private float CurrentSpeed()
    {
        switch (State)
        {
            case WolfState.Recuo: return retreatSpeed;
            case WolfState.Perseguicao: return chaseSpeed;
            case WolfState.Suspeita: return suspicionSpeed;
            default: return patrolSpeed;
        }
    }

    private bool ReachedDestination()
    {
        return Vector2.Distance(transform.position, destination) <= arrivalDistance;
    }

    /// <summary>Parado de propósito — pausa da patrulha ou farejando o ponto investigado.</summary>
    private bool IsHolding()
    {
        switch (State)
        {
            case WolfState.Patrulha: return patrolWaiting || ReachedDestination();
            case WolfState.Suspeita: return searching;
            default: return false;
        }
    }

    /// <summary>Orçamento de tempo pra chegar num destino — se estourar, ele empacou.</summary>
    private float EstimateTravelTime(Vector2 target)
    {
        float speed = Mathf.Max(0.1f, CurrentSpeed());
        return Vector2.Distance(transform.position, target) / speed * 2f + 1f;
    }

    private static Vector2 Rotate(Vector2 direction, float degrees)
    {
        if (Mathf.Approximately(degrees, 0f))
            return direction;

        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos);
    }

    private static Vector2 ToCardinal(Vector2 direction)
    {
        return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
            ? new Vector2(Mathf.Sign(direction.x), 0f)
            : new Vector2(0f, Mathf.Sign(direction.y));
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 anchor = Application.isPlaying ? (Vector3)patrolAnchor : transform.position;

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(anchor, patrolRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, senseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, destination);
        Gizmos.DrawWireSphere(destination, arrivalDistance);
    }
}
