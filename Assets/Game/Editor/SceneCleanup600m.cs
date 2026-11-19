#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneCleanup600m
{
    private const string MainMenuPath = "Assets/Game/Scenes/MainMenu.unity";
    private const string Pueblo600mPath = "Assets/Game/Scenes/Levels/PuebloLunaria_600m.unity";
    private const string Bosque600mPath = "Assets/Game/Scenes/Levels/BosqueEncantado_600m.unity";

    private static readonly HashSet<string> AllowedScenePaths =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            MainMenuPath,
            Pueblo600mPath,
            Bosque600mPath
        };

    [MenuItem("Tools/Kingdom Shadow/Maintenance/Clean Legacy Scenes - Keep MainMenu + 600m")]
    public static void CleanLegacyScenes()
    {
        if (!EditorUtility.DisplayDialog(
                "Limpiar escenas antiguas",
                "Se eliminarán del proyecto TODAS las escenas .unity excepto:\n\n" +
                "- MainMenu.unity\n" +
                "- PuebloLunaria_600m.unity\n" +
                "- BosqueEncantado_600m.unity\n\n" +
                "También se limpiará la lista de escenas del Build.\n\n" +
                "Esta operación coincide con la estructura actual del juego.",
                "Limpiar",
                "Cancelar"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // Abrimos una escena temporal para no intentar borrar una escena actualmente abierta.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        List<string> deleted = new List<string>();

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');

            if (AllowedScenePaths.Contains(path))
                continue;

            if (AssetDatabase.DeleteAsset(path))
                deleted.Add(path);
        }

        ConfigureBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = deleted.Count == 0
            ? "No había escenas antiguas adicionales."
            : "Escenas eliminadas: " + deleted.Count;

        if (File.Exists(MainMenuPath))
        {
            EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            summary += "\n\nSe abrió MainMenu.";
        }
        else
        {
            summary += "\n\nMainMenu todavía no existe. Genéralo desde Tools > Kingdom Shadow > Build Main Menu.";
        }

        summary += "\n\nIMPORTANTE: vuelve a generar PuebloLunaria_600m con el builder de 6 sectores para garantizar que sea la versión actual.";

        EditorUtility.DisplayDialog(
            "Limpieza completada",
            summary,
            "Aceptar");
    }

    [MenuItem("Tools/Kingdom Shadow/Maintenance/Reset Build Scene List to MainMenu + 600m")]
    public static void ConfigureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        AddIfExists(scenes, MainMenuPath);
        AddIfExists(scenes, Pueblo600mPath);
        AddIfExists(scenes, Bosque600mPath);

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddIfExists(List<EditorBuildSettingsScene> scenes, string path)
    {
        if (File.Exists(path))
            scenes.Add(new EditorBuildSettingsScene(path, true));
    }
}
#endif
