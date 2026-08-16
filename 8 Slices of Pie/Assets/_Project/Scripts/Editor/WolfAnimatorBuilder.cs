using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Monta os clipes e o Animator Controller do Lobo a partir das sheets já fatiadas, e liga
/// tudo no Wolf.prefab. É ferramenta de montagem, não parte do jogo: roda uma vez pelo menu
/// e pode ser apagado depois — os assets gerados continuam.
///
/// Existe pra evitar o trabalho manual de arrastar frame por frame na janela de Animação,
/// que é onde entram os erros chatos de achar (um frame fora de ordem, um clipe sem loop,
/// uma transição com blend borrando o pixel art). Rodar de novo refaz tudo do zero — os
/// assets antigos são apagados, então quem apontar pro controller fora do prefab (uma cena,
/// outro prefab) perde a referência e precisa ser religado na mão.
///
/// Menu: Tools ▸ 8 Slices of Pie ▸ Montar animações do Lobo.
/// </summary>
public static class WolfAnimatorBuilder
{
    private const string ArtFolder = "Assets/_Project/Art/WolfSprites";
    private const string PrefabPath = "Assets/_Project/Prefabs/Wolf.prefab";

    private const string IsMovingParam = "IsMoving";
    private const string MoveSpeedParam = "MoveSpeed";
    private const string AttackParam = "Attack";

    // Ritmo de cada clipe. Mexer aqui e rodar de novo é o jeito de ajustar a animação.
    private const float IdleFps = 10f;
    private const float WalkFps = 12f;
    private const float AttackFps = 14f;

    /// <summary>Um conjunto pronto: os três clipes já ligados num controller.</summary>
    private struct WolfSet
    {
        public AnimatorController controller;
        public Sprite firstIdleFrame;
        public int idleFrames, walkFrames, attackFrames;
    }

    [MenuItem("Tools/8 Slices of Pie/Montar animações do Lobo")]
    public static void Build()
    {
        if (!BuildSet("Wolf", "Wolfidle.png", "WalkWolf-sheet.png", "AttackWolf.png",
                      required: true, out WolfSet normal))
            return;

        // O Lobo de garras maiores (5ª fatia). Opcional de propósito: sem as sheets, o
        // builder monta só o comum em vez de falhar inteiro.
        bool transformed = BuildSet("WolfTransformed", "WolfIdleTransformation.png",
                                    "WalkTransformationWolf.png", "AttackWolfTransformation.png",
                                    required: false, out WolfSet clawed);

        AssetDatabase.SaveAssets();

        // Só o comum vai pro prefab: o das garras entra pelo campo do estágio 5 do
        // EnemyHabilities, e é ele que troca o controller na hora certa.
        WirePrefab(normal.controller, normal.firstIdleFrame);

        string extra = transformed
            ? $" Transformado: {clawed.idleFrames}/{clawed.walkFrames}/{clawed.attackFrames} " +
              "— ponha o 'WolfTransformed.controller' no campo Controller dos estágios 5 a 8."
            : " Sem as sheets de transformação: só o Lobo comum foi montado.";

        Debug.Log($"[WolfAnimatorBuilder] Pronto. Comum: idle {normal.idleFrames} frames, " +
                  $"walk {normal.walkFrames}, attack {normal.attackFrames}.{extra}");
    }

    /// <summary>Monta os três clipes de um Lobo e o controller que os liga.</summary>
    private static bool BuildSet(string prefix, string idleSheet, string walkSheet,
                                 string attackSheet, bool required, out WolfSet set)
    {
        set = default;

        Sprite[] idleFrames = LoadFrames(idleSheet, required);
        Sprite[] walkFrames = LoadFrames(walkSheet, required);
        Sprite[] attackFrames = LoadFrames(attackSheet, required);

        if (idleFrames == null || walkFrames == null || attackFrames == null)
            return false;

        AnimationClip idle = BuildClip($"{prefix}Idle", idleFrames, IdleFps, loop: true);
        AnimationClip walk = BuildClip($"{prefix}Walk", walkFrames, WalkFps, loop: true);

        // O ataque não repete: toca uma vez e devolve o Lobo pro que ele estava fazendo.
        AnimationClip attack = BuildClip($"{prefix}Attack", attackFrames, AttackFps, loop: false);

        set.controller = BuildController(prefix, idle, walk, attack);
        set.firstIdleFrame = idleFrames[0];
        set.idleFrames = idleFrames.Length;
        set.walkFrames = walkFrames.Length;
        set.attackFrames = attackFrames.Length;

        return true;
    }

    /// <summary>
    /// Os frames de uma sheet, em ordem de leitura. A ordem sai do X do recorte, e não do
    /// nome: renomear sprite no Sprite Editor é comum e não pode bagunçar a animação.
    /// </summary>
    private static Sprite[] LoadFrames(string file, bool required)
    {
        string path = $"{ArtFolder}/{file}";

        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.rect.y * -1f)
            .ThenBy(s => s.rect.x)
            .ToArray();

        if (frames.Length > 0)
            return frames;

        if (!required)
            return null;

        Debug.LogError($"[WolfAnimatorBuilder] '{path}' não tem sprite fatiado nenhum. " +
                       "Confira se a textura está em Sprite Mode: Multiple e já foi cortada.");
        return null;
    }

    private static AnimationClip BuildClip(string name, Sprite[] frames, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps };

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };

        // Uma chave por frame e nada além disso. O Unity estende o clipe por mais um frame
        // depois da última chave sozinho, então repetir o último desenho no fim — o reflexo
        // natural, pra ele não piscar — deixa esse frame o dobro do tempo na tela, e numa
        // caminhada de 8 isso vira manqueira.
        var keys = new ObjectReferenceKeyframe[frames.Length];

        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string path = $"{ArtFolder}/{name}.anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);

        return clip;
    }

    private static AnimatorController BuildController(string prefix, AnimationClip idle,
                                                      AnimationClip walk, AnimationClip attack)
    {
        string path = $"{ArtFolder}/{prefix}.controller";
        AssetDatabase.DeleteAsset(path);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        controller.AddParameter(IsMovingParam, AnimatorControllerParameterType.Bool);
        controller.AddParameter(AttackParam, AnimatorControllerParameterType.Trigger);

        // Padrão 1 de propósito: o parâmetro nasceria em 0 e a caminhada ficaria congelada
        // em quem não tem o EnemyAnimatorBridge alimentando (preview do editor, cena de teste).
        controller.AddParameter(new AnimatorControllerParameter
        {
            name = MoveSpeedParam,
            type = AnimatorControllerParameterType.Float,
            defaultFloat = 1f
        });

        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        AnimatorState idleState = machine.AddState("Idle", new Vector3(300f, 0f, 0f));
        idleState.motion = idle;
        machine.defaultState = idleState;

        AnimatorState walkState = machine.AddState("Walk", new Vector3(300f, 80f, 0f));
        walkState.motion = walk;

        // É isto que faz a passada acelerar sem precisar de um clipe por estado da IA.
        walkState.speedParameterActive = true;
        walkState.speedParameter = MoveSpeedParam;

        AnimatorState attackState = machine.AddState("Attack", new Vector3(560f, 40f, 0f));
        attackState.motion = attack;

        Connect(idleState, walkState, AnimatorConditionMode.If);
        Connect(walkState, idleState, AnimatorConditionMode.IfNot);

        // Any State: a mordida entra por cima de qualquer estado, inclusive do próprio ataque
        // interrompido pela metade — canTransitionToSelf desligado impede o clipe de reiniciar.
        AnimatorStateTransition bite = machine.AddAnyStateTransition(attackState);
        bite.AddCondition(AnimatorConditionMode.If, 0f, AttackParam);
        bite.hasExitTime = false;
        bite.hasFixedDuration = true;
        bite.duration = 0f;
        bite.canTransitionToSelf = false;

        // Ordem importa: terminada a mordida ele volta pra caminhada se ainda estiver andando
        // — e, logo depois de morder, ele está: o EnemyAtk manda o EnemyMov fugir na sequência.
        ExitAfterClip(attackState, walkState, requireMoving: true);
        ExitAfterClip(attackState, idleState, requireMoving: false);

        return controller;
    }

    /// <summary>Idle ⇄ Walk pelo <c>IsMoving</c>, sem blend: mistura de frame borra pixel art.</summary>
    private static void Connect(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.AddCondition(mode, 0f, IsMovingParam);
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0f;
    }

    /// <summary>Saída do ataque: espera o clipe inteiro e corta seco pro estado de destino.</summary>
    private static void ExitAfterClip(AnimatorState from, AnimatorState to, bool requireMoving)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.hasFixedDuration = true;
        transition.duration = 0f;

        if (requireMoving)
            transition.AddCondition(AnimatorConditionMode.If, 0f, IsMovingParam);
    }

    /// <summary>
    /// Põe o Animator, o <see cref="EnemyAnimatorBridge"/> e o primeiro frame do idle no
    /// prefab. Idempotente: rodar de novo reaproveita o que já estiver lá.
    /// </summary>
    private static void WirePrefab(AnimatorController controller, Sprite firstFrame)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);

        if (root == null)
        {
            Debug.LogError($"[WolfAnimatorBuilder] Não achei o prefab em '{PrefabPath}'. " +
                           "Os clipes e o controller foram gerados; só a ligação ficou faltando.");
            return;
        }

        try
        {
            Animator animator = root.GetComponent<Animator>();
            if (animator == null)
                animator = root.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            SpriteRenderer sprite = root.GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.sprite = firstFrame;

            EnemyAnimatorBridge bridge = root.GetComponent<EnemyAnimatorBridge>();
            if (bridge == null)
                bridge = root.AddComponent<EnemyAnimatorBridge>();

            // Preencher os campos no Inspector é só conforto: o Awake da ponte acha os dois
            // sozinho. Por isso nada aqui pode derrubar o resto da ligação — sem os guardas,
            // um FindProperty nulo abortava a função inteira antes de salvar.
            if (bridge != null)
            {
                var serialized = new SerializedObject(bridge);
                SetReference(serialized, "animator", animator);
                SetReference(serialized, "spriteRenderer", sprite);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        Verify();
    }

    private static void SetReference(SerializedObject target, string field, Object value)
    {
        SerializedProperty property = target.FindProperty(field);

        if (property != null)
            property.objectReferenceValue = value;
    }

    /// <summary>
    /// Relê o prefab do disco e confere o que ficou. Sem isto o builder "termina bem" mesmo
    /// deixando o Lobo travado no idle, que foi exatamente o que aconteceu na primeira rodada.
    /// </summary>
    private static void Verify()
    {
        var saved = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (saved == null)
        {
            Debug.LogError($"[WolfAnimatorBuilder] '{PrefabPath}' não abriu depois de salvo.");
            return;
        }

        var animator = saved.GetComponent<Animator>();
        bool hasController = animator != null && animator.runtimeAnimatorController != null;
        bool hasBridge = saved.GetComponent<EnemyAnimatorBridge>() != null;

        if (hasController && hasBridge)
            return;

        Debug.LogError("[WolfAnimatorBuilder] O prefab ficou incompleto — " +
                       $"Animator com controller: {hasController}, EnemyAnimatorBridge: {hasBridge}. " +
                       "Arraste na mão o que faltar pro Wolf.prefab; a ponte não precisa de " +
                       "nenhum campo preenchido, ela se resolve no Awake.", saved);
    }
}
