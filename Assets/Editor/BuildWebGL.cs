using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildWebGL
{
    private const string FocusPatch = @"      canvas.tabIndex = 0;
      canvas.style.outline = ""none"";

      function focusCanvas() {
        window.focus();
        canvas.focus();
      }

      function preventBrowserScroll(event) {
        if (event.code === ""ArrowUp"" || event.code === ""ArrowDown"" || event.code === ""Space"") {
          event.preventDefault();
        }
      }

      canvas.addEventListener(""pointerdown"", focusCanvas);
      window.addEventListener(""keydown"", preventBrowserScroll, { passive: false });
";

    public static void PerformBuild()
    {
        string[] scenes = { "Assets/Scenes/Main.unity" };
        string outputPath = "Builds/WebGL";

        Directory.CreateDirectory(outputPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("WebGL build failed: " + report.summary.result);
        }

        PatchIndexHtml(outputPath);
    }

    private static void PatchIndexHtml(string outputPath)
    {
        string indexPath = Path.Combine(outputPath, "index.html");
        if (!File.Exists(indexPath))
        {
            return;
        }

        string indexContent = File.ReadAllText(indexPath);
        indexContent = indexContent.Replace("tabindex=\"-1\"", "tabindex=\"0\"");

        const string canvasDeclaration = "      var canvas = document.querySelector(\"#unity-canvas\");\n";
        if (!indexContent.Contains(FocusPatch))
        {
            indexContent = indexContent.Replace(canvasDeclaration, canvasDeclaration + FocusPatch);
        }

        const string fullscreenHandler = "                document.querySelector(\"#unity-fullscreen-button\").onclick = () => {\n                  unityInstance.SetFullscreen(1);\n                };\n";
        const string fullscreenWithFocus = "                document.querySelector(\"#unity-fullscreen-button\").onclick = () => {\n                  focusCanvas();\n                  unityInstance.SetFullscreen(1);\n                };\n\n                focusCanvas();\n";
        indexContent = indexContent.Replace(fullscreenHandler, fullscreenWithFocus);

        File.WriteAllText(indexPath, indexContent);
    }
}
