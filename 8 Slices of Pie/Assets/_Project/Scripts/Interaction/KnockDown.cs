using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A queda de um item que está alto demais pra mão dela: a torta em cima da árvore. Sem isto,
/// o E com o galho na mão manda a fatia da copa direto pro inventário — o galho aparece no
/// puzzle e some da tela. Aqui o E derruba, a fatia cai, quica no chão, e só então ela abaixa
/// e pega.
///
/// Anda junto de um <see cref="Collectible"/>, no mesmo objeto: ele continua sendo quem
/// responde ao E e quem guarda a tranca do galho, e chama este componente sozinho ao ver que
/// ele está aqui. Não é preciso ligar nada no Inspector além dos números da queda.
///
/// A tranca do Collectible passa a valer só pra derrubada: no chão, a fatia se pega sem o
/// galho — que a essa altura provavelmente quebrou no uso.
///
/// A queda é feita no braço, sem <c>Rigidbody2D</c>: a torta precisa parar num ponto escolhido
/// do chão, e um corpo com física pararia onde os colliders do cenário deixassem — ou
/// atravessaria tudo, já que a fatia não tem corpo nenhum. Quem tiver um clipe desenhado pra
/// queda usa o <see cref="animator"/> e o resto dos números aqui é ignorado.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collectible))]
public class KnockDown : MonoBehaviour
{
    [Header("Prompt")]
    [Tooltip("O que aparece na tela enquanto ela ainda está lá em cima e a jogadora já tem o " +
             "item da tranca. Depois de cair, quem manda no texto é o Collectible.")]
    [SerializeField] private string prompt = "Derrubar";

    [Header("Onde cai")]
    [Tooltip("O ponto do chão onde ela para. Um objeto vazio na cena é o jeito mais fácil: dá " +
             "pra arrastar no editor e o gizmo mostra a linha até ele. Vazio, cai reto pra " +
             "baixo pela distância abaixo.")]
    [SerializeField] private Transform landingPoint;

    [Tooltip("Usada só quando não há ponto de queda: quantas unidades ela desce. Não exagere — " +
             "a fatia precisa continuar dentro do alcance do E de quem a derrubou.")]
    [SerializeField] private float fallDistance = 1.5f;

    [Header("Queda")]
    [Tooltip("Quanto tempo leva do galho até o chão.")]
    [Min(0f)]
    [SerializeField] private float fallDuration = 0.45f;

    [Tooltip("Quantos graus ela roda no caminho. 0 faz a fatia descer reta, sem tombar.")]
    [SerializeField] private float spin = 160f;

    [Tooltip("Quantas vezes ela quica ao bater no chão. 0 faz a queda terminar seca.")]
    [Min(0)]
    [SerializeField] private int bounces = 2;

    [Tooltip("Altura do primeiro quique. Os seguintes valem metade do anterior.")]
    [Min(0f)]
    [SerializeField] private float bounceHeight = 0.3f;

    [Header("Animator (opcional)")]
    [Tooltip("Só pra quem desenhou um clipe de queda. Preenchido, o trigger abaixo é disparado " +
             "e a animação faz o movimento inteiro — a queda de cima não roda.")]
    [SerializeField] private Animator animator;

    [Tooltip("Nome do Trigger no Animator Controller.")]
    [SerializeField] private string fallTrigger = "Fall";

    [Header("No chão")]
    [Tooltip("Marcado, ela pega a fatia no mesmo instante em que a torta bate no chão. " +
             "Desmarcado, a fatia fica lá e é preciso apertar E de novo pra catar.")]
    [SerializeField] private bool collectOnLand;

    [Tooltip("Corrige a ordem de desenho na aterrissagem: lá em cima a torta fica na frente da " +
             "árvore, no chão ela precisa passar por trás da Chapéuzinho.")]
    [SerializeField] private bool changeSortingOnLand = true;

    [SerializeField] private int landedSortingOrder;

    [Tooltip("Som da batida no chão. Sai pelo AudioSource de quem derrubou: com o " +
             "Collect On Land marcado, este objeto é destruído no mesmo quadro e levaria o " +
             "som junto.")]
    [SerializeField] private AudioClip landClip;

    [Tooltip("O que mais acontece no instante em que ela solta do galho — um som de folhas, a " +
             "árvore balançando.")]
    [SerializeField] private UnityEvent onKnockedDown;

    [Tooltip("O que mais acontece quando ela para no chão.")]
    [SerializeField] private UnityEvent onLanded;

    /// <summary>Já está no chão. A tranca do <see cref="Collectible"/> não vale mais.</summary>
    public bool HasFallen { get; private set; }

    /// <summary>Está no ar agora — ninguém interage com ela no meio da queda.</summary>
    public bool Falling { get; private set; }

    /// <summary>Ainda pendurada: o E derruba, em vez de coletar.</summary>
    public bool Pending => !HasFallen && !Falling;

    /// <summary>O texto do E enquanto ela está pendurada.</summary>
    public string Prompt => prompt;

    /// <summary>(o que caiu, quem derrubou) — pra quem precisa reagir sem passar pelo Inspector.</summary>
    public static event System.Action<KnockDown, GameObject> OnAnyKnockDown;

    private Collectible collectible;
    private SpriteRenderer sprite;

    private bool UsesAnimator => animator != null && !string.IsNullOrEmpty(fallTrigger);

    /// <summary>Onde ela vai parar. O Z é preservado: em 2D ele costuma ser ordem de desenho,
    /// e o ponto de queda é um objeto vazio que dificilmente está no mesmo plano.</summary>
    public Vector3 LandingPosition => landingPoint != null
        ? new Vector3(landingPoint.position.x, landingPoint.position.y, transform.position.z)
        : transform.position + Vector3.down * fallDistance;

    private void Awake()
    {
        collectible = GetComponent<Collectible>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        // Animator preenchido e trigger em branco é meio caminho: a queda de código não roda
        // porque tem Animator, e a animação não roda porque não tem trigger.
        if (animator != null && string.IsNullOrEmpty(fallTrigger))
            Debug.LogWarning($"[KnockDown] '{name}' tem Animator mas nenhum Trigger: a queda " +
                             "desenhada não vai disparar. Preencha o Fall Trigger ou tire o " +
                             "Animator do campo.", this);
    }

    /// <summary>
    /// Derruba. Quem chama é o <see cref="Collectible"/> deste objeto quando o E passa pela
    /// tranca — dá pra chamar de um <see cref="UnityEvent"/> da cena também, pra derrubar a
    /// torta por outro caminho que não seja o galho.
    /// </summary>
    public void Drop(GameObject interactor)
    {
        if (!Pending)
            return;

        StartCoroutine(DropRoutine(interactor));
    }

    /// <summary>Versão sem argumento, pra ligar direto num <see cref="UnityEvent"/> do Inspector.</summary>
    public void Drop() => Drop(null);

    private IEnumerator DropRoutine(GameObject interactor)
    {
        Falling = true;
        onKnockedDown?.Invoke();

        if (UsesAnimator)
        {
            animator.SetTrigger(fallTrigger);
            yield return new WaitForSeconds(fallDuration);
        }
        else
        {
            yield return Fall();
        }

        Land(interactor);
    }

    /// <summary>A descida e os quiques. A ida pro chão acelera e a subida do quique freia —
    /// é o que separa uma torta caindo de uma torta deslizando pra baixo.</summary>
    private IEnumerator Fall()
    {
        Vector3 ground = LandingPosition;

        yield return Move(transform.position, ground, fallDuration, accelerating: true, spinDegrees: spin);

        float height = bounceHeight;
        float duration = fallDuration * 0.4f;

        for (int i = 0; i < bounces && height > 0.01f; i++)
        {
            Vector3 top = ground + Vector3.up * height;

            yield return Move(ground, top, duration * 0.5f, accelerating: false, spinDegrees: 0f);
            yield return Move(top, ground, duration * 0.5f, accelerating: true, spinDegrees: 0f);

            height *= 0.5f;
            duration *= 0.7f;
        }
    }

    private IEnumerator Move(Vector3 from, Vector3 to, float duration, bool accelerating, float spinDegrees)
    {
        Quaternion fromRotation = transform.rotation;
        Quaternion toRotation = fromRotation * Quaternion.Euler(0f, 0f, -spinDegrees);

        if (duration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // deltaTime, e não unscaled: com o jogo pausado no ESC a torta para no ar junto
                // com o resto do mundo.
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                transform.position = Vector3.Lerp(from, to, accelerating ? t * t : 1f - (1f - t) * (1f - t));
                transform.rotation = Quaternion.Lerp(fromRotation, toRotation, t);
                yield return null;
            }
        }

        transform.position = to;
        transform.rotation = toRotation;
    }

    private void Land(GameObject interactor)
    {
        Falling = false;
        HasFallen = true;

        if (changeSortingOnLand && sprite != null)
            sprite.sortingOrder = landedSortingOrder;

        Play(interactor, landClip);
        onLanded?.Invoke();
        OnAnyKnockDown?.Invoke(this, interactor);

        // A tranca já não vale (Pending virou false), então isto cata de verdade em vez de
        // recusar por falta do galho que acabou de quebrar.
        if (collectOnLand && interactor != null)
            collectible.Interact(interactor);
    }

    /// <summary>Toca pelo AudioSource de quem derrubou — este objeto pode sumir no mesmo quadro.</summary>
    private static void Play(GameObject who, AudioClip clip)
    {
        if (clip == null || who == null)
            return;

        AudioSource source = who.GetComponentInParent<AudioSource>();
        if (source != null)
            source.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 ground = LandingPosition;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, ground);
        Gizmos.DrawWireSphere(ground, 0.2f);
    }
}
