using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Gera as fontes Pixeloid como atlas SDF e reaponta todo texto do jogo pra elas.
///
/// Motivo: os assets antigos foram gerados em modo Raster — bitmap puro, filtro Point — num
/// pointSize fixo (16 pro Sans e pro Bold). Um atlas bitmap só fica nítido quando é desenhado
/// exatamente no tamanho em que foi gerado. O prompt de interação desenha a 6, ou seja 0,375x,
/// e por cima disso o Canvas Scaler multiplica tudo por (altura da janela / 144), que quase
/// nunca dá inteiro. Bitmap de borda dura reamostrado em escala quebrada = traço com espessura
/// irregular e letra comida. Não é falta de resolução, é reamostragem.
///
/// SDF guarda a distância até a borda em vez do pixel aceso, então a mesma textura serve pra
/// qualquer escala: o shader reconstrói a borda já no tamanho final. Com o Sharpness no talo o
/// contorno continua duro o bastante pra ler como pixel art.
///
/// O tamanho na tela NÃO muda: o TMP desenha pelo fontSize do componente, e o pointSize do
/// atlas só decide a fidelidade. Por isso a troca é transparente e nenhum layout se mexe.
///
/// Uso: 8 Slices > Fonte > Gerar SDF e aplicar. Roda quantas vezes quiser — se os assets já
/// existirem ele só refaz a varredura. Pra mudar as constantes de atlas abaixo: rode primeiro
/// "Reverter pra bitmap", apague a pasta SDF, e só então gere de novo (apagar os assets com as
/// cenas ainda apontando pra eles deixaria os textos sem fonte nenhuma).
/// </summary>
public static class PixelFontSdfTool
{
    private const string FontsFolder = "Assets/_Project/UI/fonts";
    private const string SdfFolder = FontsFolder + "/SDF";

    /// <summary>
    /// Tamanho de amostragem do atlas. Alto o suficiente pra o campo de distância descrever
    /// bem os degraus de 90° da Pixeloid — abaixo disso os cantos começam a arredondar.
    /// </summary>
    private const int SamplingPointSize = 64;

    /// <summary>
    /// Espalhamento do campo de distância, em pixels do atlas. Fica em ~10% do
    /// <see cref="SamplingPointSize"/>: menos que isso serrilha, mais que isso come canto.
    /// </summary>
    private const int AtlasPadding = 6;

    private const int AtlasSize = 1024;

    /// <summary>
    /// Deixa o corte do SDF mais abrupto, pra borda sair dura em vez de esfumaçada — é o que
    /// segura a cara de pixel art depois da troca. O shader multiplica o gradiente por
    /// (_Sharpness + 1), então 1 é o dobro de dureza, o máximo que o slider aceita.
    /// </summary>
    private const float Sharpness = 1f;

    private const string SharpnessProperty = "_Sharpness";

    /// <summary>
    /// Pré-carrega o atlas com o que o jogo escreve hoje (ASCII imprimível) mais os acentos do
    /// português, senão o primeiro acento em cena renderizaria no meio do frame. O asset fica
    /// em modo Dynamic mesmo assim, então caractere fora desta lista continua funcionando.
    /// </summary>
    private const string Charset =
        " !\"#$%&'()*+,-./0123456789:;<=>?@" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
        "abcdefghijklmnopqrstuvwxyz{|}~" +
        "ÁÀÂÃÄÇÉÈÊËÍÌÎÏÑÓÒÔÕÖÚÙÛÜ" +
        "áàâãäçéèêëíìîïñóòôõöúùûü";

    /// <summary>
    /// As faces do projeto. Cada TTF vira um único asset SDF, e todo texto que hoje usa
    /// qualquer asset gerado a partir do mesmo TTF passa a apontar pra ele — é assim que o
    /// PixeloidSans-8 (o SDF de 8pt abandonado) também sai do caminho.
    /// </summary>
    private static readonly (string ttf, string sdf)[] Faces =
    {
        (FontsFolder + "/PixeloidSans-lxa3y.ttf", SdfFolder + "/PixeloidSans SDF.asset"),
        (FontsFolder + "/PixeloidSansBold-1jpBg.ttf", SdfFolder + "/PixeloidSansBold SDF.asset"),
        (FontsFolder + "/PixeloidMono-nAOpP.ttf", SdfFolder + "/PixeloidMono SDF.asset"),
    };

    [MenuItem("8 Slices/Fonte/Gerar SDF e aplicar")]
    private static void GenerateAndApply()
    {
        // Varrer as cenas abre uma por uma em Single, o que descarta o que não estiver salvo.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var log = new StringBuilder();
        var targets = new Dictionary<Font, TMP_FontAsset>();

        foreach ((string ttfPath, string sdfPath) in Faces)
        {
            var ttf = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (ttf == null)
            {
                log.AppendLine($"  ! {ttfPath}: não achei o .ttf, pulei a face");
                continue;
            }

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfPath);
            if (existing != null)
            {
                targets[ttf] = existing;
                log.AppendLine($"  = {sdfPath} já existia, reaproveitado");
                continue;
            }

            TMP_FontAsset created = CreateSdfAsset(ttf, sdfPath, log);
            if (created != null)
                targets[ttf] = created;
        }

        if (targets.Count == 0)
        {
            Debug.LogError($"[PixelFontSdfTool] Nenhuma fonte SDF disponível — nada foi trocado.\n{log}");
            return;
        }

        AssetDatabase.SaveAssets();
        Sweep(targets, log);
        Debug.Log($"[PixelFontSdfTool] Gerar SDF e aplicar:\n{log}");
    }

    /// <summary>
    /// Volta todo texto pros assets bitmap originais. Serve pra comparar o antes e o depois, e
    /// é o passo obrigatório antes de apagar a pasta SDF pra regerar com outras constantes.
    /// </summary>
    [MenuItem("8 Slices/Fonte/Reverter pra bitmap")]
    private static void RevertToRaster()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var log = new StringBuilder();
        Dictionary<Font, TMP_FontAsset> targets = RasterAssetsByTtf();

        if (targets.Count == 0)
        {
            Debug.LogError("[PixelFontSdfTool] Não achei nenhum asset bitmap em " + FontsFolder);
            return;
        }

        Sweep(targets, log);
        Debug.Log($"[PixelFontSdfTool] Reverter pra bitmap:\n{log}");
    }

    private static TMP_FontAsset CreateSdfAsset(Font ttf, string sdfPath, StringBuilder log)
    {
        EnsureFolder(SdfFolder);

        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
            ttf,
            SamplingPointSize,
            AtlasPadding,
            GlyphRenderMode.SDFAA,
            AtlasSize,
            AtlasSize,
            AtlasPopulationMode.Dynamic);

        if (asset == null)
        {
            log.AppendLine($"  ! {sdfPath}: o TMP não conseguiu gerar o asset");
            return null;
        }

        asset.name = Path.GetFileNameWithoutExtension(sdfPath);
        AssetDatabase.CreateAsset(asset, sdfPath);

        // Textura e material precisam virar sub-assets do .asset, senão ficam como objetos
        // soltos em memória e a fonte volta rosa no próximo reimport.
        if (asset.atlasTextures != null && asset.atlasTextures.Length > 0 && asset.atlasTextures[0] != null)
        {
            asset.atlasTextures[0].name = asset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
        }

        Material material = asset.material;
        if (material != null)
        {
            material.name = asset.name + " Material";

            // Por nome, e não pelos IDs do ShaderUtilities: aqueles só valem depois que o TMP
            // roda a inicialização dele, o que num menu de editor não é garantido.
            if (material.HasProperty(SharpnessProperty))
                material.SetFloat(SharpnessProperty, Sharpness);

            AssetDatabase.AddObjectToAsset(material, asset);
        }

        if (!asset.TryAddCharacters(Charset, out string missing) && !string.IsNullOrEmpty(missing))
            log.AppendLine($"  ! {asset.name}: a face não tem estes caracteres: {missing}");

        asset.ReadFontAssetDefinition();
        EditorUtility.SetDirty(asset);

        log.AppendLine($"  + {sdfPath} ({SamplingPointSize}pt SDFAA, padding {AtlasPadding}, atlas {AtlasSize}²)");
        return asset;
    }

    /// <summary>
    /// Passa em toda cena e todo prefab de <c>_Project</c> trocando a fonte de cada texto pela
    /// que <paramref name="targets"/> indica pro TTF de origem daquele texto. Textos de outras
    /// famílias (LiberationSans e afins) ficam onde estão.
    /// </summary>
    private static void Sweep(Dictionary<Font, TMP_FontAsset> targets, StringBuilder log)
    {
        string previousScene = SceneManager.GetActiveScene().path;

        try
        {
            foreach (string path in AssetPaths("t:Scene"))
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int changed = scene.GetRootGameObjects().Sum(root => Retarget(root, targets));

                if (changed == 0)
                    continue;

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                log.AppendLine($"  ~ {path}: {changed} texto(s)");
            }

            // Devolve o editor pra cena em que o usuário estava, senão a troca de fonte parece
            // ter fechado o trabalho dele.
            if (!string.IsNullOrEmpty(previousScene))
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);

            foreach (string path in AssetPaths("t:Prefab"))
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(path);

                try
                {
                    int changed = Retarget(contents, targets);
                    if (changed == 0)
                        continue;

                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    log.AppendLine($"  ~ {path}: {changed} texto(s)");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
        }
    }

    private static int Retarget(GameObject root, Dictionary<Font, TMP_FontAsset> targets)
    {
        int changed = 0;

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            TMP_FontAsset current = text.font;
            if (current == null)
                continue;

            Font source = SourceFontOf(current);
            if (source == null || !targets.TryGetValue(source, out TMP_FontAsset replacement))
                continue;

            // Sem isto o menu não seria idempotente: rodar duas vezes marcaria as cenas como
            // sujas e as salvaria de novo à toa.
            if (current == replacement)
                continue;

            text.font = replacement;
            text.fontSharedMaterial = replacement.material;
            EditorUtility.SetDirty(text);
            changed++;
        }

        return changed;
    }

    /// <summary>
    /// Os assets bitmap originais, indexados pelo TTF de que saíram. Quando duas variantes
    /// vieram do mesmo arquivo — o caso do PixeloidSans-lxa3y e do PixeloidSans-8 — vence a
    /// que é Raster de verdade; o outro era justamente a tentativa de SDF que ficou pra trás.
    /// </summary>
    private static Dictionary<Font, TMP_FontAsset> RasterAssetsByTtf()
    {
        var map = new Dictionary<Font, TMP_FontAsset>();

        foreach (string path in AssetPaths("t:TMP_FontAsset", FontsFolder))
        {
            if (path.StartsWith(SdfFolder))
                continue;

            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset == null)
                continue;

            Font source = SourceFontOf(asset);
            if (source == null)
                continue;

            if (!map.TryGetValue(source, out TMP_FontAsset chosen) || IsRaster(asset) && !IsRaster(chosen))
                map[source] = asset;
        }

        return map;
    }

    private static bool IsRaster(TMP_FontAsset asset) =>
        asset.atlasRenderMode == GlyphRenderMode.RASTER ||
        asset.atlasRenderMode == GlyphRenderMode.RASTER_HINTED;

    /// <summary>
    /// De qual TTF este asset saiu. Num asset em modo Static o <c>sourceFontFile</c> vem nulo
    /// de propósito — o atlas já basta em runtime — e a referência de verdade fica só no campo
    /// de editor, daí a leitura pelo <see cref="SerializedObject"/>.
    /// </summary>
    private static Font SourceFontOf(TMP_FontAsset asset)
    {
        if (asset.sourceFontFile != null)
            return asset.sourceFontFile;

        SerializedProperty property = new SerializedObject(asset).FindProperty("m_SourceFontFile_EditorRef");
        return property?.objectReferenceValue as Font;
    }

    private static string[] AssetPaths(string filter, string folder = "Assets/_Project") =>
        AssetDatabase.FindAssets(filter, new[] { folder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .ToArray();

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }
}
