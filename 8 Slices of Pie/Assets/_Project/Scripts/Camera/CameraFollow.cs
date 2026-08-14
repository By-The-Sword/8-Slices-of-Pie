using UnityEngine;

/// <summary>
/// Câmera que segue a Chapéuzinho com amortecimento, em vez de ser filha dela.
/// Ser filha gruda a câmera no pixel do player e deixa o movimento duro; aqui ela
/// fica um pouco para trás, o que também esconde melhor a borda do círculo de luz.
/// A área de <see cref="boundsArea"/> impede a câmera de mostrar o vazio fora da cidade.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    [Tooltip("Vazio procura pela tag Player.")]
    [SerializeField] private Transform target;

    [Tooltip("Z negativo, senão a câmera fica em cima do cenário e não enxerga nada.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Suavização")]
    [Tooltip("Tempo aproximado pra alcançar o alvo. 0 gruda igual ao parent antigo.")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothTime = 0.15f;

    [Tooltip("Meia-largura e meia-altura, em unidades, que ela anda antes da câmera reagir.")]
    [SerializeField] private Vector2 deadzone = Vector2.zero;

    [Header("Limites")]
    [Tooltip("Opcional: um Collider2D desenhando a área navegável. Vazio libera a câmera.")]
    [SerializeField] private Collider2D boundsArea;

    private Camera cam;
    private Vector3 velocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // Montagem provisória do prefab deixava a câmera como filha do player: com o follow
        // ligado os dois brigam pela posição, então ela se solta sozinha.
        if (target != null && transform.IsChildOf(target))
        {
            transform.SetParent(null, true);
            Debug.Log("[CameraFollow] Câmera desacoplada do player — quem move agora é o follow.", this);
        }
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[CameraFollow] Sem alvo: nenhum objeto com a tag Player na cena.", this);
            return;
        }

        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = Resolve(DesiredCenter());
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    /// <summary>Corta o amortecimento. Chamar em respawn e teleporte, senão a câmera atravessa o mapa.</summary>
    public void SnapToTarget()
    {
        if (target == null)
            return;

        velocity = Vector3.zero;
        transform.position = Resolve(target.position + offset);
    }

    public void SetTarget(Transform newTarget, bool snap = true)
    {
        target = newTarget;
        if (snap)
            SnapToTarget();
    }

    /// <summary>Onde a câmera quer estar, já respeitando a deadzone.</summary>
    private Vector3 DesiredCenter()
    {
        Vector3 goal = target.position + offset;

        if (deadzone == Vector2.zero)
            return goal;

        Vector3 current = transform.position;
        float x = Mathf.Abs(goal.x - current.x) > deadzone.x
            ? goal.x - Mathf.Sign(goal.x - current.x) * deadzone.x
            : current.x;
        float y = Mathf.Abs(goal.y - current.y) > deadzone.y
            ? goal.y - Mathf.Sign(goal.y - current.y) * deadzone.y
            : current.y;

        return new Vector3(x, y, goal.z);
    }

    /// <summary>Prende a posição dentro da área permitida.</summary>
    private Vector3 Resolve(Vector3 position)
    {
        if (boundsArea == null || !cam.orthographic)
            return position;

        Bounds area = boundsArea.bounds;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Área menor que a tela: centraliza, senão os dois limites se cruzam e a câmera treme.
        position.x = area.size.x <= halfWidth * 2f
            ? area.center.x
            : Mathf.Clamp(position.x, area.min.x + halfWidth, area.max.x - halfWidth);

        position.y = area.size.y <= halfHeight * 2f
            ? area.center.y
            : Mathf.Clamp(position.y, area.min.y + halfHeight, area.max.y - halfHeight);

        return position;
    }

    private void OnDrawGizmosSelected()
    {
        if (deadzone == Vector2.zero)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadzone.x * 2f, deadzone.y * 2f, 0f));
    }
}
