using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// A luz de um poste da cidade. Mesmo tipo de Light2D do lampião dela, só que parada no
/// mapa e ligada no relógio, não numa tecla.
///
/// O GDD (seção 05) dá o roteiro da iluminação pública, e este script é ele inteiro:
/// até 23:00 os postes estão acesos e a cidade é legível · 00:00 eles piscam brevemente,
/// como aviso de que a iluminação não é confiável · 01:00 as luzes se apagam e não voltam
/// mais — daí em diante só existe o lampião. Como o relógio anda por fatia coletada, é o
/// jogador que apaga a própria cidade.
///
/// Vai no objeto raiz do poste, com o Light2D num filho. Um script por poste: cada um
/// escuta o <see cref="Clock"/> sozinho e pisca com um atraso próprio, senão a cidade
/// inteira pisca no mesmo quadro e vira um flash só.
/// </summary>
public class StreetLamp : MonoBehaviour
{
    [Header("Luz")]
    [Tooltip("Light2D do poste. Vazio ele pega o primeiro que achar nos filhos.")]
    [SerializeField] private Light2D lampLight;

    [Header("Roteiro da noite")]
    [Tooltip("Fatia em que os postes dão a piscada de aviso. 2 é 00:00 no GDD.")]
    [SerializeField] private int flickerAtSlice = 2;

    [Tooltip("Fatia em que as luzes se apagam pra sempre. 3 é 01:00 no GDD.")]
    [SerializeField] private int blackoutAtSlice = 3;

    [Tooltip("Ignora o relógio e fica aceso a noite toda. É a regra da conveniência, " +
             "o único ponto de luz que o GDD mantém em qualquer horário.")]
    [SerializeField] private bool alwaysOn;

    [Header("Piscada")]
    [Tooltip("Quantas apagadas rápidas dá a piscada de 00:00.")]
    [SerializeField] private int flickerBlinks = 6;

    [Tooltip("Duração da piscada inteira, em segundos.")]
    [SerializeField] private float flickerSeconds = 1.2f;

    [Tooltip("Atraso aleatório antes de começar, em segundos. É o que espalha a piscada " +
             "pela cidade em vez de todo poste apagar junto.")]
    [SerializeField] private float flickerJitter = 0.35f;

    [Header("Referências")]
    [Tooltip("Vazio ele acha o relógio sozinho na cena.")]
    [SerializeField] private Clock clock;

    /// <summary>Se este poste está iluminando agora. Piscando ele oscila.</summary>
    public bool IsLit => lampLight != null && lampLight.enabled;

    private Coroutine flicker;

    private void Awake()
    {
        if (lampLight == null)
            lampLight = GetComponentInChildren<Light2D>(true);

        if (clock == null)
            clock = FindObjectOfType<Clock>();

        if (lampLight == null)
            Debug.LogWarning($"[StreetLamp] '{name}' está sem Light2D: o poste não ilumina nada.", this);
    }

    private void OnEnable()
    {
        if (clock != null)
            clock.OnHourChanged += HandleHourChanged;
    }

    private void OnDisable()
    {
        if (clock != null)
            clock.OnHourChanged -= HandleHourChanged;
    }

    private void Start()
    {
        // Sem relógio o poste fica aceso a noite toda: é cena de teste, não erro fatal.
        if (clock == null)
        {
            Debug.LogWarning($"[StreetLamp] '{name}' não achou um Clock: o poste nunca vai apagar.", this);
            SetLit(true);
            return;
        }

        // Sem piscar: um poste ligado no meio da partida (ou a cena recarregando numa hora
        // adiantada) tem que nascer já no estado certo, não repetir o aviso de 00:00.
        Apply(clock.HoursElapsed, false);
    }

    private void HandleHourChanged(int hour, string text) => Apply(clock.HoursElapsed, true);

    private void Apply(int slices, bool animate)
    {
        if (alwaysOn)
        {
            SetLit(true);
            return;
        }

        if (slices >= blackoutAtSlice)
        {
            StopFlicker();
            SetLit(false);
            return;
        }

        SetLit(true);

        if (animate && slices == flickerAtSlice)
            StartFlicker();
    }

    /// <summary>A piscada de 00:00, sozinha. Serve pra testar sem esperar duas fatias.</summary>
    public void StartFlicker()
    {
        if (lampLight == null || flickerBlinks <= 0 || flickerSeconds <= 0f)
            return;

        StopFlicker();
        flicker = StartCoroutine(FlickerRoutine());
    }

    private void StopFlicker()
    {
        if (flicker == null)
            return;

        StopCoroutine(flicker);
        flicker = null;
    }

    /// <summary>
    /// Tempo escalado de propósito, ao contrário dos fades de UI: isto acontece no mundo,
    /// e o mundo congela na pausa. Ninguém confere a hora pra ver a cidade piscar.
    /// </summary>
    private IEnumerator FlickerRoutine()
    {
        if (flickerJitter > 0f)
            yield return new WaitForSeconds(Random.Range(0f, flickerJitter));

        // Cada piscada é apagado + aceso, daí a metade em cada estado.
        float half = flickerSeconds / (flickerBlinks * 2);

        for (int i = 0; i < flickerBlinks; i++)
        {
            SetLit(false);
            yield return new WaitForSeconds(half);
            SetLit(true);
            yield return new WaitForSeconds(half);
        }

        flicker = null;

        // A piscada é só um susto: quem decide o estado final é o relógio, não ela.
        if (clock != null)
            Apply(clock.HoursElapsed, false);
    }

    private void SetLit(bool lit)
    {
        if (lampLight != null)
            lampLight.enabled = lit;
    }
}
