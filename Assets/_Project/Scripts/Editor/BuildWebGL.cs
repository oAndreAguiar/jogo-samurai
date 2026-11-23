using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Gera o build WebGL em Builds/WebGL (pasta ignorada pelo Git).
/// Pelo menu "JogoJapones/Build/WebGL" ou pela linha de comando:
///   Unity.exe -batchmode -quit -projectPath ... -executeMethod BuildWebGL.Build
/// </summary>
public static class BuildWebGL
{
    public const string OutputPath = "Builds/WebGL";

    [MenuItem("JogoJapones/Build/WebGL")]
    public static void Build()
    {
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log(string.Format("[BuildWebGL] {0}: {1:0.0} MB em {2:0} s ({3} cenas)",
            summary.result, summary.totalSize / (1024f * 1024f), summary.totalTime.TotalSeconds, scenes.Length));

        if (summary.result == BuildResult.Succeeded)
        {
            MakeFullWindow();
        }
        else if (Application.isBatchMode)
        {
            EditorApplication.Exit(1);
        }
    }

    // O modelo padrão da Unity só ocupa a tela toda no celular; no PC fica numa janela fixa.
    // Aqui o index.html passa a usar o modo "tela cheia" sempre (o canvas acompanha o tamanho da aba).
    private static void MakeFullWindow()
    {
        string path = System.IO.Path.Combine(OutputPath, "index.html");
        if (!System.IO.File.Exists(path))
        {
            return;
        }
        string html = System.IO.File.ReadAllText(path);
        const string mobileCheck = "if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {";
        if (html.Contains(mobileCheck))
        {
            html = html.Replace(mobileCheck, "if (true) { // JogoJapones: tela cheia no PC e no celular");
            html = html.Replace("</head>", "<style>html, body { background: #0f0b12; overflow: hidden; }</style>\n</head>");
            System.IO.File.WriteAllText(path, html);
            Debug.Log("[BuildWebGL] index.html ajustado para ocupar a janela toda.");
        }
    }
}
