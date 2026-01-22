using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// LayerManager.assetのレイヤー名をもとにLayers.csを自動生成するエディタ拡張。
/// </summary>
[InitializeOnLoad]
public static class LayerClassGenerator
{
    private const string OutputPath = "Assets/MyGameFolder/Data/Layers.cs";
    static LayerClassGenerator()
    {
        // Unity起動時とレイヤー変更時に自動生成
        EditorApplication.delayCall += GenerateLayerClass;
        // Projectウィンドウでアセットが変更されたときにも監視
        AssetDatabase.importPackageCompleted += _ => GenerateLayerClass();
    }

    [MenuItem("Tools/Generate Layers Class")]
    public static void GenerateLayerClass()
    {
        var layers = UnityEditorInternal.InternalEditorUtility.layers;
        var sb = new StringBuilder();
        sb.AppendLine("// 自動生成: Unityレイヤー定数クラス");
        sb.AppendLine("public static class Layers");
        sb.AppendLine("{");
        foreach (var layer in layers)
        {
            if (!string.IsNullOrEmpty(layer))
            {
                sb.AppendLine($"public const string {SanitizeLayerName(layer)} = \"{layer}\";");
            }
        }
        sb.AppendLine("}");
        // Ensure the output directory exists
        var directory = Path.GetDirectoryName(OutputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(OutputPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
    }

    private static string SanitizeLayerName(string layer)
    {
        // レイヤー名をC#の識別子として使えるように変換
        var safe = layer.Replace(" ", "_").Replace("-", "_");
        if (char.IsDigit(safe[0])) safe = "_" + safe;
        return safe;
    }
}
