using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// O lado da Chapéuzinho da coleta: recebe tudo que o <see cref="PlayerInteractor"/> cata
/// e roteia por <c>itemId</c>. Bateria vai direto pro <see cref="Lantern"/>, fatia entra na
/// contagem de 0–8 (a condição de vitória do GDD) e o resto fica guardado por id, esperando
/// os puzzles pedirem. Vai na raiz do Player, junto do PlayerInteractor — é lá que o
/// <see cref="Collectible"/> procura o <see cref="IItemCollector"/>.
/// </summary>
[RequireComponent(typeof(PlayerInteractor))]
public class PlayerInventory : MonoBehaviour, IItemCollector
{
    // Os ids seguem o inglês do SliceOfPie.prefab, que é o padrão já versionado no projeto.
    [Header("Ids especiais")]
    [Tooltip("itemId dos Collectible que recarregam o lampião.")]
    [SerializeField] private string batteryItemId = "battery";

    [Tooltip("itemId das fatias de torta.")]
    [SerializeField] private string sliceItemId = "slice";

    [Header("Fatias")]
    [Tooltip("Quantas fatias fecham a noite. 8 no GDD.")]
    [Min(1)]
    [SerializeField] private int totalSlices = 8;

    [Header("Referências")]
    [Tooltip("Vazio busca no próprio objeto.")]
    [SerializeField] private Lantern lantern;

    public int SliceCount { get; private set; }
    public int TotalSlices => totalSlices;
    public bool AllSlicesCollected => SliceCount >= totalSlices;

    /// <summary>(fatias, total) — a pulseira diegética escuta isto.</summary>
    public event System.Action<int, int> OnSliceCountChanged;
    /// <summary>(itemId, quantidade total em posse) — vale pra bateria e pros itens de puzzle.</summary>
    public event System.Action<string, int> OnItemAdded;
    public event System.Action<string, int> OnItemRemoved;
    /// <summary>Oitava fatia coletada. Quem termina a noite escuta isto.</summary>
    public event System.Action OnAllSlicesCollected;

    private readonly Dictionary<string, int> items = new Dictionary<string, int>();

    private void Awake()
    {
        if (lantern == null)
            lantern = GetComponent<Lantern>();
    }

    private void Start()
    {
        OnSliceCountChanged?.Invoke(SliceCount, totalSlices);
    }

    public bool TryCollect(Collectible item)
    {
        if (item == null)
            return false;

        string id = item.ItemId;
        int amount = Mathf.Max(1, item.Amount);

        if (id == sliceItemId)
        {
            AddSlices(amount);
            return true;
        }

        if (id == batteryItemId && lantern != null)
        {
            lantern.AddBattery(amount);
            return true;
        }

        // Sem lampião no objeto o setup está errado, mas recusar deixaria a bateria presa no
        // chão sem explicação: guarda como item comum e avisa quem montou a cena.
        if (id == batteryItemId)
            Debug.LogWarning($"[PlayerInventory] '{name}' não tem Lantern: a bateria virou item guardado.", this);

        Add(id, amount);
        return true;
    }

    /// <summary>Adiciona sem passar por um Collectible — resgate de puzzle, item inicial, debug.</summary>
    public void Add(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        if (itemId == sliceItemId)
        {
            AddSlices(amount);
            return;
        }

        items.TryGetValue(itemId, out int current);
        current += amount;
        items[itemId] = current;

        OnItemAdded?.Invoke(itemId, current);
    }

    public bool HasItem(string itemId, int amount = 1)
    {
        if (itemId == sliceItemId)
            return SliceCount >= amount;

        return items.TryGetValue(itemId, out int current) && current >= amount;
    }

    public int GetAmount(string itemId)
    {
        if (itemId == sliceItemId)
            return SliceCount;

        return items.TryGetValue(itemId, out int current) ? current : 0;
    }

    /// <summary>
    /// Gasta um item. Devolve false sem tirar nada se não houver o suficiente —
    /// é o que a porta do porão chama antes de abrir. Fatias não se gastam.
    /// </summary>
    public bool Consume(string itemId, int amount = 1)
    {
        if (amount <= 0 || itemId == sliceItemId || !HasItem(itemId, amount))
            return false;

        int remaining = items[itemId] - amount;

        if (remaining > 0)
            items[itemId] = remaining;
        else
            items.Remove(itemId);

        OnItemRemoved?.Invoke(itemId, remaining);
        return true;
    }

    private void AddSlices(int amount)
    {
        if (AllSlicesCollected)
            return;

        SliceCount = Mathf.Min(totalSlices, SliceCount + amount);
        OnSliceCountChanged?.Invoke(SliceCount, totalSlices);

        if (AllSlicesCollected)
            OnAllSlicesCollected?.Invoke();
    }
}
