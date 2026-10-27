using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Monta a cena da partida com os scripts que já existem no projeto: a Chapéuzinho com
/// movimento, vida, lampião e interação; o Lobo com IA, mordida, estágios e áudio; os
/// sistemas da noite (relógio, fim de partida, pausa, cutscene, trilha); a HUD diegética;
/// o poste com StreetLamp e Light2D; as portas das casas que já estão no mapa (entrar e
/// sair); e os itens cujo lugar o README já define.
///
/// É idempotente: procura cada objeto pelo nome antes de criar, e só preenche campo que
/// ainda está vazio no caso das referências de cena. Rodar duas vezes não duplica nada.
///
/// Nada de arte entra aqui — os sprites estão sendo trocados, então os SpriteRenderer e
/// Animator nascem vazios de propósito. A ferramenta cuida só do que é ligação de script.
/// </summary>
public static class SceneSetupTool
{
    private const string Menu = "8 Slices of Pie/Montar a cena (scripts e ligações)";

    // ------------------------------------------------------------------ mapa
    // Posições tiradas do tilemap já montado: a rua principal ocupa x -1..0 e sobe de
    // y -13 (entrada sul) até y 95 (casa da avó), com o cruzamento em y 6..7.
    private static readonly Vector2 PlayerSpawn = new Vector2(0f, -10f);
    private static readonly Vector2 WolfSpawn = new Vector2(0f, 46f);

    /// <summary>Canto do mapa onde os interiores ficam estacionados até terem arte.</summary>
    private static readonly Vector2 InteriorsOrigin = new Vector2(300f, 0f);
    private const float InteriorSpacing = 40f;

    private static readonly string[] Buildings = { "Escola", "house npc", "store", "GrannyHouse" };

    private static StringBuilder report;
    private static int created;

    [MenuItem(Menu)]
    public static void Build()
    {
        report = new StringBuilder();
        created = 0;

        var scene = SceneManager.GetActiveScene();
        Line($"Cena: {scene.name}");

        EnsureLayers("Interactable", "Enemy", "Shelter");
        EnsureTag("Enemy");

        GameObject player = BuildPlayer();
        GameObject wolf = BuildWolf();
        GameObject systems = BuildSystems();
        GameObject hud = BuildHud();

        WireSystems(systems, player, wolf, hud);
        WireHud(hud, systems, player);
        WireWolf(wolf, player, systems);

        BuildCamera(player);
        BuildGlobalLight();
        BuildWorld(systems);
        BuildItems();

        EditorSceneManager.MarkSceneDirty(scene);

        Line($"Pronto. {created} objeto(s) novo(s). Salve a cena com Ctrl+S.");
        Debug.Log("[Montar a cena]\n" + report);
    }

    // =================================================================== player

    private static GameObject BuildPlayer()
    {
        GameObject player = Root("Player", PlayerSpawn);
        player.tag = "Player";

        var sprite = Ensure<SpriteRenderer>(Child(player, "Sprite", Vector3.zero));
        sprite.sortingOrder = 10;
        Ensure<Animator>(sprite.gameObject);

        GameObject lightGo = Child(player, "LanternLight", Vector3.zero);
        Light2D lamp = EnsureLight(lightGo, point: true, radius: 5f,
                                   color: new Color(1f, 0.95f, 0.8f), intensity: 1f);
        lamp.enabled = false;

        var body = Ensure<Rigidbody2D>(player);
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        var circle = Ensure<CircleCollider2D>(player);
        circle.radius = 0.3f;

        var audio = Ensure<AudioSource>(player);
        audio.playOnAwake = false;
        audio.spatialBlend = 0f;

        SetFields(Ensure<PlayerController>(player),
            ("moveSpeed", 3.5f),
            ("crouchSpeedMultiplier", 0.5f),
            ("crouchKey", (int)KeyCode.LeftControl));

        SetFields(Ensure<PlayerAnimatorBridge>(player),
            ("animator", sprite.GetComponent<Animator>()),
            ("spriteRenderer", sprite));

        SetFields(Ensure<PlayerFootsteps>(player),
            ("stepInterval", 0.45f),
            ("stepNoiseRadius", 6f),
            ("audioSource", audio),
            ("stepClips", Clips("step01", "step02")));

        SetFields(Ensure<PlayerHealth>(player),
            ("maxHearts", 3),
            ("invulnerabilityDuration", 1f));

        SetFields(Ensure<PlayerHitFlash>(player), ("sprite", sprite));

        SetFields(Ensure<PlayerInteractor>(player),
            ("interactRadius", 0.9f),
            ("facingOffset", 0.25f),
            ("interactableLayers", LayerBit("Interactable")),
            ("interactKey", (int)KeyCode.E));

        var lantern = Ensure<Lantern>(player);
        SetFields(lantern,
            ("lampLight", lamp),
            ("barsPerBattery", 4),
            ("secondsPerBar", 15f),
            ("spareBatteries", 0),
            ("startsWithFullCharge", true),
            ("toggleKey", (int)KeyCode.F),
            ("audioSource", audio),
            ("toggleOnClip", Clip("Lantern")),
            ("toggleOffClip", Clip("Lantern")));

        SetFields(Ensure<PlayerInventory>(player),
            ("batteryItemId", "battery"),
            ("sliceItemId", "slice"),
            ("totalSlices", 8),
            ("lantern", lantern));

        Line("Player: movimento, vida, pisca-dano, passos, interação, inventário e lampião (Light2D no filho LanternLight).");
        return player;
    }

    // ===================================================================== lobo

    private static GameObject BuildWolf()
    {
        GameObject wolf = Root("Wolf", WolfSpawn);

        var sprite = Ensure<SpriteRenderer>(Child(wolf, "Sprite", Vector3.zero));
        sprite.sortingOrder = 9;
        Ensure<Animator>(sprite.gameObject);

        var body = Ensure<Rigidbody2D>(wolf);
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        var circle = Ensure<CircleCollider2D>(wolf);
        circle.radius = 0.4f;

        Line("Lobo: IA, mordida, estágios do GDD e áudio em três fontes (Breath, Steps, Voice).");
        return wolf;
    }

    private static void WireWolf(GameObject wolf, GameObject player, GameObject systems)
    {
        var sprite = wolf.transform.Find("Sprite").GetComponent<SpriteRenderer>();

        AudioSource breath = EnsureSource(Child(wolf, "Breath", Vector3.zero));
        AudioSource steps = EnsureSource(Child(wolf, "Steps", Vector3.zero));
        AudioSource voice = EnsureSource(Child(wolf, "Voice", Vector3.zero));
        breath.loop = true;

        GameObject spawn = Root("Wolf Spawn", WolfSpawn);

        SetFields(Ensure<EnemyMov>(wolf),
            ("player", player.transform),
            ("patrolSpeed", 1.8f),
            ("suspicionSpeed", 2.8f),
            ("chaseSpeed", 4f),
            ("senseRadius", 6f),
            ("sightRadius", 10f),
            ("obstacleMask", LayerBit("Obstacle")),
            ("patrolRadius", 14f),
            ("fearsLight", true),
            ("bodyRadius", 0.4f));

        SetFields(Ensure<EnemyAtk>(wolf),
            ("damage", 1),
            ("range", 0.9f),
            ("cooldown", 3f));

        SetFields(Ensure<EnemyHabilities>(wolf),
            ("clock", systems.GetComponent<Clock>()),
            ("spawnPoint", spawn.transform),
            ("audioSource", voice));

        SetFields(Ensure<EnemyAudio>(wolf),
            ("breathSource", breath),
            ("stepSource", steps),
            ("voiceSource", voice),
            ("stepClips", Clips("step01", "step02")),
            ("growlClips", Clips("grunhido1")),
            ("attackClips", Clips("ataque1")),
            ("howlClips", Clips("uivo1", "uivo2")),
            ("lamentClips", Clips("lamento1")),
            ("grassClips", Clips("Grass")));

        SetFields(Ensure<EnemyAnimatorBridge>(wolf),
            ("animator", sprite.GetComponent<Animator>()),
            ("spriteRenderer", sprite));
    }

    // ================================================================= sistemas

    private static GameObject BuildSystems()
    {
        GameObject systems = Root("Systems", Vector2.zero);

        Ensure<Clock>(systems);
        Ensure<NightRunner>(systems);
        Ensure<PauseMenu>(systems);
        Ensure<EndingSequence>(systems);
        Ensure<MusicDirector>(systems);

        return systems;
    }

    private static void WireSystems(GameObject systems, GameObject player, GameObject wolf, GameObject hud)
    {
        var inventory = player.GetComponent<PlayerInventory>();
        var health = player.GetComponent<PlayerHealth>();

        var fade = hud.transform.Find("Fade").GetComponent<CanvasGroup>();
        GameObject pausePanel = hud.transform.Find("Pause").gameObject;
        GameObject optionsPanel = hud.transform.Find("Options").gameObject;
        GameObject arm = hud.transform.Find("ArmHUD").gameObject;
        GameObject credits = hud.transform.Find("Credits").gameObject;
        var creditsContent = credits.transform.Find("Content").GetComponent<RectTransform>();

        SetFields(systems.GetComponent<Clock>(),
            ("startHour", 22),
            ("inventory", inventory));

        SetFields(systems.GetComponent<NightRunner>(),
            ("playerHealth", health),
            ("inventory", inventory),
            ("deathDelay", 1.5f),
            ("restartOnDeath", true),
            ("freezeWolfOnDeath", true),
            ("fadeGroup", fade),
            ("fadeDuration", 1f),
            ("fadeInOnStart", true));

        SetFields(systems.GetComponent<PauseMenu>(),
            ("pausePanel", pausePanel),
            ("optionsPanel", optionsPanel),
            ("menuSceneName", "MainMenu"),
            ("pauseKey", (int)KeyCode.Escape),
            ("muteWhilePaused", true));

        GameObject stage = Child(systems, "Stage", Vector3.zero);
        var actor = Ensure<SpriteRenderer>(Child(stage, "Ator", Vector3.zero));
        actor.sortingOrder = 20;
        stage.SetActive(false);

        SetFields(systems.GetComponent<EndingSequence>(),
            ("stage", stage),
            ("actor", actor),
            ("fadeGroup", fade),
            ("fadeDuration", 1.5f),
            ("hudContent", arm),
            ("gunLoadClip", Clip("load_gun")),
            ("gunShotClip", Clip("gun_shooting")),
            ("creditsRoot", credits),
            ("creditsContent", creditsContent),
            ("menuSceneName", "MainMenu"));

        SetFields(systems.GetComponent<MusicDirector>(),
            ("backgroundTracks", Clips("backgroundmusic1")),
            ("tensionTracks", Clips("tens")),
            ("backgroundVolume", 0.5f),
            ("tensionVolume", 0.7f));

        Line("Systems: Clock (22:00 + 1h por fatia), NightRunner (morte/8 fatias + fade), PauseMenu (ESC), EndingSequence (cutscene + créditos) e MusicDirector (fundo + tensão).");
    }

    // ====================================================================== hud

    private static GameObject BuildHud()
    {
        GameObject hud = Root("HUD", Vector2.zero);

        var canvas = Ensure<Canvas>(hud);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = Ensure<CanvasScaler>(hud);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(160f, 144f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        Ensure<GraphicRaycaster>(hud);

        // O prompt fica acima da pausa, do fade e dos créditos na ordem de irmãos (README 4.5).
        BuildPrompt(hud);
        BuildArm(hud);
        BuildPause(hud);
        BuildOptions(hud);
        BuildFade(hud);
        BuildCredits(hud);

        GameObject events = Root("EventSystem", Vector2.zero);
        Ensure<EventSystem>(events);
        Ensure<StandaloneInputModule>(events);

        var sounds = Ensure<UISounds>(hud);
        SetFields(sounds,
            ("hoverClip", Clip("Mouse_Hover")),
            ("clickClip", Clip("Press_Start")),
            ("startClip", Clip("Press_Start")),
            ("pauseInClip", Clip("Pause_IN")),
            ("pauseOutClip", Clip("Pause_OUT")));

        Line("HUD: Canvas 160x144, Prompt do E, braço diegético, painel de pausa com botões ligados, opções, fade preto, créditos, EventSystem e UISounds.");
        return hud;
    }

    private static void BuildPrompt(GameObject hud)
    {
        GameObject prompt = UiChild(hud, "Prompt");
        RectTransform rect = prompt.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 12f);
        rect.sizeDelta = new Vector2(150f, 16f);

        GameObject bgGo = UiChild(prompt, "Background");
        Stretch(bgGo.GetComponent<RectTransform>());
        var bg = Ensure<Image>(bgGo);
        bg.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject labelGo = UiChild(prompt, "Label");
        Stretch(labelGo.GetComponent<RectTransform>());
        var label = Ensure<TextMeshProUGUI>(labelGo);
        label.fontSize = 10f;
        label.alignment = TextAlignmentOptions.Center;
        label.text = string.Empty;

        SetFields(Ensure<InteractPrompt>(prompt),
            ("label", label),
            ("background", bg),
            ("onlyOnInteract", true));
    }

    private static void BuildArm(GameObject hud)
    {
        GameObject arm = UiChild(hud, "ArmHUD");
        RectTransform rect = arm.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-4f, 4f);
        rect.sizeDelta = new Vector2(60f, 48f);

        Image clock = Mostrador(arm, "Clock");
        Image bracelet = Mostrador(arm, "Bracelet");
        Image battery = Mostrador(arm, "Battery");

        SetFields(Ensure<ArmHUD>(arm),
            ("hiddenOffset", new Vector2(0f, -144f)),
            ("slideSeconds", 0.18f),
            ("clockImage", clock),
            ("braceletImage", bracelet),
            ("batteryImage", battery));
    }

    private static Image Mostrador(GameObject parent, string name)
    {
        GameObject go = UiChild(parent, name);
        Stretch(go.GetComponent<RectTransform>());
        var image = Ensure<Image>(go);
        image.enabled = false; // sem sprite ainda; o ArmHUD liga quando houver arte.
        return image;
    }

    private static void BuildPause(GameObject hud)
    {
        GameObject pause = UiChild(hud, "Pause");
        Stretch(pause.GetComponent<RectTransform>());

        GameObject bgGo = UiChild(pause, "Background");
        Stretch(bgGo.GetComponent<RectTransform>());
        var bg = Ensure<Image>(bgGo);
        bg.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject buttons = UiChild(pause, "Buttons");
        RectTransform rect = buttons.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.25f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(70f, 80f);

        Ensure<CanvasGroup>(buttons);
        Ensure<UIFadeIn>(buttons);

        var layout = Ensure<VerticalLayoutGroup>(buttons);
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = false;

        MakeButton(buttons, "Continuar", "Continuar");
        MakeButton(buttons, "Opcoes", "Opções");
        MakeButton(buttons, "Menu", "Menu principal");
        MakeButton(buttons, "Sair", "Sair");

        pause.SetActive(false);
    }

    private static void BuildOptions(GameObject hud)
    {
        GameObject options = UiChild(hud, "Options");
        Stretch(options.GetComponent<RectTransform>());

        GameObject bgGo = UiChild(options, "Background");
        Stretch(bgGo.GetComponent<RectTransform>());
        var bg = Ensure<Image>(bgGo);
        bg.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject buttons = UiChild(options, "Buttons");
        RectTransform rect = buttons.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.25f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(70f, 40f);

        Ensure<CanvasGroup>(buttons);
        Ensure<UIFadeIn>(buttons);

        var layout = Ensure<VerticalLayoutGroup>(buttons);
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = false;

        MakeButton(buttons, "Voltar", "Voltar");

        options.SetActive(false);
    }

    private static void BuildFade(GameObject hud)
    {
        GameObject fade = UiChild(hud, "Fade");
        Stretch(fade.GetComponent<RectTransform>());

        var image = Ensure<Image>(fade);
        image.color = Color.black;

        var group = Ensure<CanvasGroup>(fade);
        group.alpha = 1f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private static void BuildCredits(GameObject hud)
    {
        GameObject credits = UiChild(hud, "Credits");
        Stretch(credits.GetComponent<RectTransform>());

        GameObject bgGo = UiChild(credits, "Background");
        Stretch(bgGo.GetComponent<RectTransform>());
        Ensure<Image>(bgGo).color = Color.black;

        GameObject content = UiChild(credits, "Content");
        RectTransform rect = content.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(140f, 400f);

        var text = Ensure<TextMeshProUGUI>(content);
        text.fontSize = 8f;
        text.alignment = TextAlignmentOptions.Top;
        if (string.IsNullOrEmpty(text.text))
            text.text = "8 Slices of Pie\n\nBy The Sword";

        credits.SetActive(false);
    }

    private static void WireHud(GameObject hud, GameObject systems, GameObject player)
    {
        var pause = systems.GetComponent<PauseMenu>();

        SetFields(hud.transform.Find("Prompt").GetComponent<InteractPrompt>(),
            ("interactor", player.GetComponent<PlayerInteractor>()),
            ("pauseMenu", pause));

        SetFields(hud.transform.Find("ArmHUD").GetComponent<ArmHUD>(),
            ("clock", systems.GetComponent<Clock>()),
            ("health", player.GetComponent<PlayerHealth>()),
            ("lantern", player.GetComponent<Lantern>()),
            ("pauseMenu", pause));

        SetFields(hud.GetComponent<UISounds>(), ("pauseMenu", pause));

        Transform buttons = hud.transform.Find("Pause/Buttons");
        Bind(buttons.Find("Continuar"), pause.Resume);
        Bind(buttons.Find("Opcoes"), pause.OpenOptions);
        Bind(buttons.Find("Menu"), pause.GoToMainMenu);
        Bind(buttons.Find("Sair"), pause.QuitGame);
        Bind(hud.transform.Find("Options/Buttons/Voltar"), pause.CloseOptions);
    }

    // ==================================================================== mundo

    private static void BuildCamera(GameObject player)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Line("AVISO: nenhuma câmera com a tag MainCamera na cena — o CameraFollow não foi ligado.");
            return;
        }

        cam.orthographic = true;

        SetFields(Ensure<CameraFollow>(cam.gameObject),
            ("target", player.transform),
            ("offset", new Vector3(0f, 0f, -10f)),
            ("smoothTime", 0.15f));

        Line("Main Camera: CameraFollow seguindo a Chapéuzinho (offset Z -10).");
    }

    private static void BuildGlobalLight()
    {
        GameObject go = Root("Luz Global", Vector2.zero);
        EnsureLight(go, point: false, radius: 0f,
                    color: new Color(0.55f, 0.6f, 0.8f), intensity: 0.6f);

        Line("Luz Global 2D criada (intensidade 0.6 — baixe conforme a noite do GDD).");
    }

    private static void BuildWorld(GameObject systems)
    {
        Transform scenery = FindSceneryParent();
        if (scenery == null)
        {
            Line("AVISO: não achei o objeto 'Cenário objects' — postes e portas não foram montados.");
            return;
        }

        var clock = systems.GetComponent<Clock>();
        var ending = systems.GetComponent<EndingSequence>();

        int obstacle = LayerIndex("Obstacle");
        int interactable = LayerIndex("Interactable");

        int lamps = 0, doors = 0;
        GameObject interiors = Root("Interiores", InteriorsOrigin);
        int index = 0;

        foreach (Transform child in scenery)
        {
            GameObject go = child.gameObject;

            // Fachada e poste param a Chapéuzinho e cortam a linha de visão do Lobo.
            if (go.layer != obstacle)
            {
                Undo.RecordObject(go, "Layer Obstacle");
                go.layer = obstacle;
            }

            if (go.name.StartsWith("poste", StringComparison.OrdinalIgnoreCase))
            {
                BuildStreetLamp(go, clock);
                lamps++;
                continue;
            }

            if (!Buildings.Any(b => go.name.StartsWith(b, StringComparison.OrdinalIgnoreCase)))
                continue;

            bool granny = go.name.StartsWith("GrannyHouse", StringComparison.OrdinalIgnoreCase);
            Vector2 interiorAt = InteriorsOrigin + new Vector2(InteriorSpacing * index++, 0f);

            BuildDoorPair(go, interiors, interiorAt, interactable, granny ? ending : null);
            doors++;
        }

        Line($"Mundo: {lamps} poste(s) com StreetLamp + Light2D e {doors} porta(s) de entrada/saída. " +
             "Cenário movido pra layer Obstacle.");
        Line("Os interiores nasceram vazios em x=300 (um por casa): mova/desenhe cada quarto por lá, " +
             "as duas portas já se apontam.");
    }

    private static void BuildStreetLamp(GameObject poste, Clock clock)
    {
        Bounds bounds = BoundsOf(poste);
        Vector3 top = new Vector3(bounds.center.x, bounds.max.y - 0.3f, poste.transform.position.z);

        GameObject lightGo = Child(poste, "Luz", poste.transform.InverseTransformPoint(top));
        EnsureLight(lightGo, point: true, radius: 4.5f,
                    color: new Color(1f, 0.88f, 0.6f), intensity: 1.2f);

        SetFields(Ensure<StreetLamp>(poste),
            ("lampLight", lightGo.GetComponent<Light2D>()),
            ("flickerAtSlice", 2),
            ("blackoutAtSlice", 3),
            ("clock", clock));
    }

    private static void BuildDoorPair(GameObject building, GameObject interiors, Vector2 interiorAt,
                                      int interactableLayer, EndingSequence ending)
    {
        Bounds bounds = BoundsOf(building);
        Vector3 outside = new Vector3(bounds.center.x, bounds.min.y - 0.4f, 0f);

        GameObject interior = Child(interiors, "Interior " + building.name,
                                    (Vector3)(interiorAt - InteriorsOrigin));

        GameObject spawn = Child(interior, "Spawn", Vector3.zero);
        GameObject exit = Child(interior, "Porta (sair)", new Vector3(0f, -1.5f, 0f));

        // A porta da rua: um trigger no pé da fachada, na layer que o E procura.
        GameObject door = Child(building, "Porta", building.transform.InverseTransformPoint(outside));

        // Onde ela reaparece ao sair: um passo à frente da porta, na calçada.
        GameObject outsideReturn = Child(door, "Chegada", new Vector3(0f, -0.6f, 0f));
        door.layer = interactableLayer;
        DoorCollider(door);

        var toInside = Ensure<Teleporter>(door);
        SetFields(toInside,
            ("destination", spawn.transform),
            ("prompt", "Entrar"),
            ("snapCamera", true),
            ("stopOnArrival", true));

        if (ending != null)
        {
            // A casa da avó só abre com as 8 fatias — é a condição de vitória do GDD.
            SetFields(toInside,
                ("prompt", "Entrar"),
                ("requirement.itemId", "slice"),
                ("requirement.amount", 8),
                ("requirement.consume", false),
                ("requirement.lockedPrompt", "Está trancado"),
                ("requirement.lockedMessage", "Ainda falta torta."));

            BindEvent(toInside, "onTeleported", ending.Play);
        }

        exit.layer = interactableLayer;
        DoorCollider(exit);

        SetFields(Ensure<Teleporter>(exit),
            ("destination", outsideReturn.transform),
            ("prompt", "Sair"),
            ("snapCamera", true),
            ("stopOnArrival", true));
    }

    private static void DoorCollider(GameObject door)
    {
        var box = Ensure<BoxCollider2D>(door);
        box.isTrigger = true;
        box.size = new Vector2(1.2f, 1f);
    }

    // ==================================================================== itens

    private static void BuildItems()
    {
        GameObject items = Root("Itens", Vector2.zero);
        GameObject interiors = GameObject.Find("Interiores");

        // Só os itens cujo lugar o README 4.10 já define e que existem no mapa de hoje.
        MakeCollectible(items, "Fatia 1 (entrada sul)", "slice", "fatia de torta", new Vector2(1.2f, -10f));
        MakeCollectible(items, "Fatia 2 (cruzamento)", "slice", "fatia de torta", new Vector2(2f, 7f));
        MakeCollectible(items, "Fatia 3 (poco)", "slice", "fatia de torta", PocoSide());
        MakeCollectible(items, "Querosene 9", "battery", "lata de querosene", new Vector2(-8f, 3f));
        MakeCollectible(items, "Querosene 10", "battery", "lata de querosene", new Vector2(5f, 20f));

        if (interiors == null)
            return;

        Transform store = interiors.transform.Find("Interior store");
        Transform escola = interiors.transform.Find("Interior Escola");

        if (store != null)
        {
            MakeCollectible(store.gameObject, "Fatia 7 (máquina da loja)", "slice", "fatia de torta",
                        store.position + new Vector3(2f, 1f, 0f));
            MakeCollectible(store.gameObject, "Moeda", "moeda", "moeda",
                        store.position + new Vector3(-2f, 1f, 0f));
            MakeCollectible(store.gameObject, "Folha de estoque", "folha_estoque", "folha de estoque",
                        store.position + new Vector3(-2f, 2f, 0f));
            MakeCollectible(store.gameObject, "Querosene 6", "battery", "lata de querosene",
                        store.position + new Vector3(0f, 2f, 0f));
        }

        if (escola != null)
        {
            MakeCollectible(escola.gameObject, "Fatia 6 (armário da escola)", "slice", "fatia de torta",
                        escola.position + new Vector3(2f, 1f, 0f));
            MakeCollectible(escola.gameObject, "Querosene 7", "battery", "lata de querosene",
                        escola.position + new Vector3(-2f, 1f, 0f));
        }

        Line("Itens: fatias 1, 2, 3, 6 e 7, moeda, folha de estoque e 4 latas de querosene " +
             "(itemId 'battery', que é o que o lampião consome). As fatias 4, 5 e 8 dependem da " +
             "árvore, da cabana e da floresta — não estão no mapa ainda.");
    }

    /// <summary>Ao lado do poco, que já está na praça — é onde a fatia nº 3 fica no GDD.</summary>
    private static Vector2 PocoSide()
    {
        Transform scenery = FindSceneryParent();
        Transform poco = scenery == null ? null
            : scenery.Cast<Transform>().FirstOrDefault(t => t.name.StartsWith("po", StringComparison.OrdinalIgnoreCase)
                                                            && !t.name.StartsWith("poste", StringComparison.OrdinalIgnoreCase));

        if (poco == null)
            return new Vector2(-5.5f, 6.5f);

        Bounds b = BoundsOf(poco.gameObject);
        return new Vector2(b.min.x - 1f, b.center.y);
    }

    private static void MakeCollectible(GameObject parent, string name, string itemId, string displayName,
                                    Vector3 worldPosition)
    {
        GameObject go = Child(parent, name, Vector3.zero);
        go.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        go.layer = LayerIndex("Interactable");

        Ensure<SpriteRenderer>(go).sortingOrder = 5;

        var circle = Ensure<CircleCollider2D>(go);
        circle.isTrigger = true;
        circle.radius = 0.35f;

        SetFields(Ensure<Collectible>(go),
            ("itemId", itemId),
            ("displayName", displayName),
            ("amount", 1),
            ("destroyOnCollect", true),
            ("pickupClip", Clip("Item_Pick")));
    }

    // ================================================================== helpers

    private static Transform FindSceneryParent()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name.StartsWith("Cen", StringComparison.OrdinalIgnoreCase) && root.name.Contains("objects"))
                return root.transform;

        return null;
    }

    /// <summary>Caixa que envolve os colliders do objeto, ou os sprites quando não há collider.</summary>
    private static Bounds BoundsOf(GameObject go)
    {
        var colliders = go.GetComponentsInChildren<Collider2D>();
        if (colliders.Length > 0)
        {
            Bounds b = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                b.Encapsulate(colliders[i].bounds);
            return b;
        }

        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }

        return new Bounds(go.transform.position, Vector3.one);
    }

    private static GameObject Root(string name, Vector2 position)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == name)
                return root;

        var go = new GameObject(name);
        go.transform.position = new Vector3(position.x, position.y, 0f);
        Undo.RegisterCreatedObjectUndo(go, "Montar a cena");
        created++;
        return go;
    }

    private static GameObject Child(GameObject parent, string name, Vector3 localPosition)
    {
        Transform existing = parent.transform.Find(name);
        if (existing != null)
            return existing.gameObject;

        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPosition;
        go.layer = parent.layer;
        Undo.RegisterCreatedObjectUndo(go, "Montar a cena");
        created++;
        return go;
    }

    private static GameObject UiChild(GameObject parent, string name)
    {
        Transform existing = parent.transform.Find(name);
        GameObject go = existing != null ? existing.gameObject : null;

        if (go == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Montar a cena");
            created++;
        }

        Ensure<RectTransform>(go);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void MakeButton(GameObject parent, string name, string text)
    {
        GameObject go = UiChild(parent, name);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(70f, 16f);

        var image = Ensure<Image>(go);
        image.color = new Color(1f, 1f, 1f, 0.12f);

        Ensure<Button>(go).targetGraphic = image;

        GameObject labelGo = UiChild(go, "Label");
        Stretch(labelGo.GetComponent<RectTransform>());

        var label = Ensure<TextMeshProUGUI>(labelGo);
        label.fontSize = 9f;
        label.alignment = TextAlignmentOptions.Center;
        label.text = text;

        var layout = Ensure<LayoutElement>(go);
        layout.preferredHeight = 16f;
        layout.preferredWidth = 70f;
    }

    private static void Bind(Transform button, UnityAction call)
    {
        if (button == null)
            return;

        var component = button.GetComponent<Button>();
        if (component == null || component.onClick.GetPersistentEventCount() > 0)
            return;

        UnityEventTools.AddVoidPersistentListener(component.onClick, call);
        EditorUtility.SetDirty(component);
    }

    /// <summary>Liga um método num UnityEvent privado (o onTeleported da porta, por exemplo).</summary>
    private static void BindEvent(Component owner, string fieldName, UnityAction call)
    {
        FieldInfo field = owner.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (field == null)
        {
            Line($"AVISO: '{owner.GetType().Name}' não tem o campo '{fieldName}'.");
            return;
        }

        var evt = field.GetValue(owner) as UnityEvent;
        if (evt == null)
        {
            evt = new UnityEvent();
            field.SetValue(owner, evt);
        }

        if (evt.GetPersistentEventCount() > 0)
            return;

        UnityEventTools.AddVoidPersistentListener(evt, call);
        EditorUtility.SetDirty(owner);
    }


    /// <summary>Indice da layer, ou Default quando ela nao existe.</summary>
    private static int LayerIndex(string name)
    {
        int index = LayerMask.NameToLayer(name);

        if (index >= 0)
            return index;

        Line($"AVISO: a layer '{name}' nao existe — usei a Default.");
        return 0;
    }

    /// <summary>Mascara com so essa layer, ou "tudo" quando ela nao existe.</summary>
    private static int LayerBit(string name)
    {
        int index = LayerMask.NameToLayer(name);

        if (index >= 0)
            return 1 << index;

        Line($"AVISO: a layer '{name}' nao existe — a mascara ficou em 'tudo'.");
        return ~0;
    }

    private static T Ensure<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component != null)
            return component;

        component = Undo.AddComponent<T>(go);
        return component;
    }

    private static AudioSource EnsureSource(GameObject go)
    {
        var source = Ensure<AudioSource>(go);
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private static Light2D EnsureLight(GameObject go, bool point, float radius, Color color, float intensity)
    {
        var light = Ensure<Light2D>(go);

        var so = new SerializedObject(light);
        SetProp(so, "m_LightType", point ? 3 : 4); // 3 = Point/Spot, 4 = Global
        SetProp(so, "m_Color", color);
        SetProp(so, "m_Intensity", intensity);

        if (point)
        {
            SetProp(so, "m_PointLightInnerAngle", 360f);
            SetProp(so, "m_PointLightOuterAngle", 360f);
            SetProp(so, "m_PointLightInnerRadius", 0f);
            SetProp(so, "m_PointLightOuterRadius", radius);
        }

        // Target Sorting Layers vazio deixaria a luz sem iluminar nada.
        SerializedProperty layers = so.FindProperty("m_ApplyToSortingLayers");
        if (layers != null && layers.isArray && layers.arraySize == 0)
        {
            layers.arraySize = 1;
            layers.GetArrayElementAtIndex(0).intValue = 0; // sorting layer Default
        }

        so.ApplyModifiedProperties();
        return light;
    }

    private static void SetFields(Component component, params (string field, object value)[] fields)
    {
        if (component == null)
            return;

        var so = new SerializedObject(component);

        foreach ((string field, object value) in fields)
            SetProp(so, field, value, component);

        so.ApplyModifiedProperties();
    }

    private static void SetProp(SerializedObject so, string path, object value, Component owner = null)
    {
        SerializedProperty prop = so.FindProperty(path);

        if (prop == null)
        {
            Line($"AVISO: campo '{path}' não existe em {(owner != null ? owner.GetType().Name : so.targetObject.GetType().Name)}.");
            return;
        }

        if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
        {
            var array = value as Array;
            prop.arraySize = array?.Length ?? 0;

            for (int i = 0; i < prop.arraySize; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = array.GetValue(i) as UnityEngine.Object;

            return;
        }

        switch (prop.propertyType)
        {
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = value as UnityEngine.Object;
                break;
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.Enum:
            case SerializedPropertyType.LayerMask:
                prop.intValue = Convert.ToInt32(value);
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = Convert.ToSingle(value);
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = Convert.ToBoolean(value);
                break;
            case SerializedPropertyType.String:
                prop.stringValue = value as string;
                break;
            case SerializedPropertyType.Vector2:
                prop.vector2Value = (Vector2)value;
                break;
            case SerializedPropertyType.Vector3:
                prop.vector3Value = (Vector3)value;
                break;
            case SerializedPropertyType.Color:
                prop.colorValue = (Color)value;
                break;
            default:
                Line($"AVISO: não sei preencher '{path}' ({prop.propertyType}).");
                break;
        }
    }

    // ------------------------------------------------------------------ áudio

    private static AudioClip[] clipCache;

    private static AudioClip Clip(string startsWith)
    {
        clipCache ??= AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Project/Audio" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
            .Where(c => c != null)
            .ToArray();

        AudioClip clip = clipCache.FirstOrDefault(
            c => c.name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase));

        if (clip == null)
            Line($"AVISO: não achei nenhum áudio começando com '{startsWith}'.");

        return clip;
    }

    private static AudioClip[] Clips(params string[] names) =>
        names.Select(Clip).Where(c => c != null).ToArray();

    // ------------------------------------------------------- layers e tags

    private static void EnsureLayers(params string[] names)
    {
        SerializedObject tagManager = TagManager();
        if (tagManager == null)
            return;

        SerializedProperty layers = tagManager.FindProperty("layers");
        var existing = new HashSet<string>();

        for (int i = 0; i < layers.arraySize; i++)
            existing.Add(layers.GetArrayElementAtIndex(i).stringValue);

        foreach (string name in names)
        {
            if (existing.Contains(name))
                continue;

            bool placed = false;

            // As 8 primeiras são reservadas da Unity; as livres começam na 8.
            for (int i = 8; i < layers.arraySize && !placed; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(slot.stringValue))
                    continue;

                slot.stringValue = name;
                placed = true;
                Line($"Layer '{name}' criada no slot {i}.");
            }

            if (!placed)
                Line($"AVISO: não há slot livre pra layer '{name}'.");
        }

        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureTag(string name)
    {
        SerializedObject tagManager = TagManager();
        if (tagManager == null)
            return;

        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == name)
                return;

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = name;
        tagManager.ApplyModifiedProperties();

        Line($"Tag '{name}' criada.");
    }

    private static SerializedObject TagManager()
    {
        UnityEngine.Object asset = AssetDatabase
            .LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")
            .FirstOrDefault();

        if (asset == null)
        {
            Line("AVISO: não consegui abrir o TagManager — layers e tags não foram criadas.");
            return null;
        }

        return new SerializedObject(asset);
    }

    private static void Line(string text) => report.AppendLine("· " + text);
}
