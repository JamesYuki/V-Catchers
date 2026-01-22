using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// TagManager.assetのタグをもとにTags.csを自動生成するエディタ拡張。
/// </summary>
[InitializeOnLoad]
public static class TagClassGenerator
{
    private const string OutputPath = "Assets/MyGameFolder/Data/Tags.cs";
    static TagClassGenerator()
    {
        // Unity起動時とタグ変更時に自動生成
        EditorApplication.delayCall += GenerateTagClass;
        // Projectウィンドウでアセットが変更されたときにも監視
        AssetDatabase.importPackageCompleted += _ => GenerateTagClass();
    }

    [MenuItem("Tools/Generate Tags Class")]
    public static void GenerateTagClass()
    {
        var tags = UnityEditorInternal.InternalEditorUtility.tags;
        var sb = new StringBuilder();
        sb.AppendLine("// 自動生成: Unityタグ定数クラス");
        sb.AppendLine("public static class Tags");
        sb.AppendLine("{");
        foreach (var tag in tags)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                sb.AppendLine($"public const string {SanitizeTagName(tag)} = \"{tag}\";");
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

    private static string SanitizeTagName(string tag)
    {
        // タグ名をC#の識別子として使えるように変換
        var safe = tag.Replace(" ", "_").Replace("-", "_");
        if (char.IsDigit(safe[0])) safe = "_" + safe;
        return safe;
    }
}
