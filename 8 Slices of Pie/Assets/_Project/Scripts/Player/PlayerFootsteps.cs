using UnityEngine;

/// <summary>
/// Passos da Chapéuzinho: som + emissão de ruído em intervalo fixo enquanto ela anda.
/// Agachada ela não emite ruído nenhum — é assim que o Lobo deixa de percebê-la (GDD).
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Ritmo")]
    [SerializeField] private float stepInterval = 0.45f;

    [Header("Ruído")]
    [Tooltip("Alcance do ruído de um passo em pé. Agachada não emite ruído.")]
    [SerializeField] private float stepNoiseRadius = 6f;

    [Header("Áudio")]
    [Tooltip("Opcionais — a arte de som entra depois.")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] stepClips;

    [Range(0f, 1f)]
    [SerializeField] private float crouchVolume = 0.35f;

    private PlayerController controller;
    private float stepTimer;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!controller.IsMoving)
        {
            // Rearma pro próximo passo sair assim que ela voltar a andar.
            stepTimer = stepInterval;
            return;
        }

        stepTimer += Time.deltaTime;
        if (stepTimer < stepInterval)
            return;

        stepTimer = 0f;
        Step();
    }

    private void Step()
    {
        PlayStepClip();

        if (!controller.IsCrouched)
            Noise.Emit(transform.position, stepNoiseRadius, gameObject);
    }

    private void PlayStepClip()
    {
        if (audioSource == null || stepClips == null || stepClips.Length == 0)
            return;

        AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip, controller.IsCrouched ? crouchVolume : 1f);
    }
}
