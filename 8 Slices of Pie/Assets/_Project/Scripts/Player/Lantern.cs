using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Lampião elétrico: F alterna, ilumina em círculo e afasta o Lobo — até a 7ª fatia,
/// quando a luz passa a atraí-lo. Essa inversão é regra do Lobo: aqui só se expõe <see cref="IsOn"/>.
/// Bateria: 4 barras de 15 s = 60 s de luz contínua, drena só com o lampião ligado,
/// e a troca por uma bateria reserva é automática quando a carga acaba.
/// </summary>
public class Lantern : MonoBehaviour
{
    [Header("Luz")]
    [Tooltip("Light2D do círculo de luz. Pode ficar vazio enquanto a arte não existe.")]
    [SerializeField] private Light2D lampLight;

    [Header("Bateria")]
    [SerializeField] private int barsPerBattery = 4;
    [SerializeField] private float secondsPerBar = 15f;

    [Tooltip("Baterias reserva no início. As 4 do mapa são coletadas em jogo.")]
    [SerializeField] private int spareBatteries = 0;

    [SerializeField] private bool startsWithFullCharge = true;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    [Header("Áudio")]
    [Tooltip("Opcionais — a arte de som entra depois.")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleOnClip;
    [SerializeField] private AudioClip toggleOffClip;
    [SerializeField] private AudioClip batterySwapClip;

    public bool IsOn { get; private set; }
    public int SpareBatteries => spareBatteries;
    public bool HasCharge => chargeSeconds > 0f;

    /// <summary>Duração total de uma bateria cheia, em segundos (4 × 15 = 60).</summary>
    public float FullChargeSeconds => barsPerBattery * secondsPerBar;

    /// <summary>Barras cheias ou parciais restantes — o que a pulseira desenha.</summary>
    public int BarsRemaining => Mathf.CeilToInt(chargeSeconds / secondsPerBar);

    /// <summary>Preenchimento 0–1 da barra que está drenando, pra animar o esvaziamento.</summary>
    public float CurrentBarFill
    {
        get
        {
            if (chargeSeconds <= 0f) return 0f;
            float within = chargeSeconds % secondsPerBar;
            return Mathf.Approximately(within, 0f) ? 1f : within / secondsPerBar;
        }
    }

    public event System.Action<bool> OnToggled;
    /// <summary>(barras restantes, preenchimento da barra atual)</summary>
    public event System.Action<int, float> OnChargeChanged;
    public event System.Action<int> OnSpareBatteriesChanged;
    public event System.Action OnBatterySwapped;
    /// <summary>Carga zerou e não havia reserva — o lampião apagou sozinho.</summary>
    public event System.Action OnOutOfPower;

    private float chargeSeconds;
    private int lastReportedBars = -1;

    private void Awake()
    {
        chargeSeconds = startsWithFullCharge ? FullChargeSeconds : 0f;
        ApplyLightState();
    }

    private void Start()
    {
        ReportCharge(true);
        OnSpareBatteriesChanged?.Invoke(spareBatteries);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();

        if (!IsOn)
            return;

        chargeSeconds -= Time.deltaTime;

        if (chargeSeconds <= 0f)
        {
            chargeSeconds = 0f;

            if (spareBatteries > 0)
            {
                spareBatteries--;
                chargeSeconds = FullChargeSeconds;
                OnSpareBatteriesChanged?.Invoke(spareBatteries);
                OnBatterySwapped?.Invoke();
                Play(batterySwapClip);
            }
            else
            {
                SetOn(false);
                OnOutOfPower?.Invoke();
            }
        }

        ReportCharge(false);
    }

    public void Toggle() => SetOn(!IsOn);

    public void SetOn(bool on)
    {
        if (on && !HasCharge)
            on = false;

        if (IsOn == on)
            return;

        IsOn = on;
        ApplyLightState();
        Play(IsOn ? toggleOnClip : toggleOffClip);
        OnToggled?.Invoke(IsOn);
    }

    /// <summary>Chamado pelas baterias espalhadas pelo mapa.</summary>
    public void AddBattery(int amount = 1)
    {
        if (amount <= 0)
            return;

        spareBatteries += amount;
        OnSpareBatteriesChanged?.Invoke(spareBatteries);

        // Se estava sem carga nenhuma, a primeira reserva já entra no lampião.
        if (!HasCharge && spareBatteries > 0)
        {
            spareBatteries--;
            chargeSeconds = FullChargeSeconds;
            OnSpareBatteriesChanged?.Invoke(spareBatteries);
            OnBatterySwapped?.Invoke();
            ReportCharge(true);
        }
    }

    private void ApplyLightState()
    {
        if (lampLight != null)
            lampLight.enabled = IsOn;
    }

    private void ReportCharge(bool force)
    {
        int bars = BarsRemaining;
        if (!force && bars == lastReportedBars)
            return;

        lastReportedBars = bars;
        OnChargeChanged?.Invoke(bars, CurrentBarFill);
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
