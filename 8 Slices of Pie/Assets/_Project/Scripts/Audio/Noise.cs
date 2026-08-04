using UnityEngine;

/// <summary>
/// Ruído no mundo. É o que alimenta o estado SUSPEITA do Lobo (passos, porta aberta,
/// máquina acionada) sem que quem emite precise conhecer a IA dele.
/// </summary>
public struct NoiseEvent
{
    public Vector2 Position;
    /// <summary>Distância máxima em que esse som ainda é notado.</summary>
    public float Radius;
    public GameObject Source;
}

/// <summary>Canal global de ruído: quem faz barulho chama <see cref="Emit"/>, o Lobo escuta.</summary>
public static class Noise
{
    public static event System.Action<NoiseEvent> OnNoise;

    public static void Emit(Vector2 position, float radius, GameObject source = null)
    {
        if (radius <= 0f)
            return;

        OnNoise?.Invoke(new NoiseEvent
        {
            Position = position,
            Radius = radius,
            Source = source
        });
    }

    /// <summary>
    /// Zera os inscritos. Necessário ao recarregar a cena com "Enter Play Mode Options"
    /// sem domain reload, senão sobram inscritos de partidas anteriores.
    /// </summary>
    public static void Clear() => OnNoise = null;
}
