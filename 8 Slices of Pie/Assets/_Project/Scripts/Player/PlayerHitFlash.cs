using System.Collections;
using UnityEngine;

/// <summary>
/// Pisca o sprite durante os i-frames do <see cref="PlayerHealth"/>. Sem isso o único
/// sinal de dano é o knockback, que dura 0,15 s, e não dá pra saber que ela está
/// temporariamente invulnerável. A duração não é configurada aqui de propósito: segue
/// o <c>IsInvulnerable</c>, então mexer no i-frame não desincroniza os dois.
/// </summary>
[RequireComponent(typeof(PlayerHealth))]
public class PlayerHitFlash : MonoBehaviour
{
    [Tooltip("Segundos de cada meio-ciclo (apagado/aceso).")]
    [Min(0.01f)]
    [SerializeField] private float flashInterval = 0.08f;

    [Tooltip("Vazio busca o SpriteRenderer nos filhos.")]
    [SerializeField] private SpriteRenderer sprite;

    private PlayerHealth health;
    private Coroutine routine;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();

        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        health.OnDamaged += Flash;
    }

    // Desabilitar o componente mata a corrotina no meio: sem isto ela ficaria invisível.
    private void OnDisable()
    {
        if (sprite != null)
            sprite.enabled = true;
    }

    private void Flash(int amount)
    {
        if (sprite == null)
            return;

        // Dano em cima de dano: a segunda rotina apagaria o sprite pra sempre.
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Blinking());
    }

    private IEnumerator Blinking()
    {
        var wait = new WaitForSeconds(flashInterval);

        while (health.IsInvulnerable)
        {
            sprite.enabled = !sprite.enabled;
            yield return wait;
        }

        sprite.enabled = true;
        routine = null;
    }
}
