#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PuebloLunariaRecovery
{
    private const string ExactScenePath = "Assets/PuebloLunaria_Blockout_Exact.unity";
    private const string BackupFolder = "Assets/Game/Scenes/Levels/RecoveryBackups";

    [MenuItem("Tools/Kingdom Shadow/Maintenance/Restore Legacy Pueblo Blockout")]
    public static void Recover()
    {
        if (!File.Exists(ExactScenePath))
        {
            EditorUtility.DisplayDialog(
                "Recuperación de Lunaria",
                "No se encontró " + ExactScenePath,
                "Aceptar");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Restaurar blockout heredado",
                "Esto restaurará en PuebloLunaria_Blockout_Exact los elementos visuales base:\n\n" +
                "- CASA-A_REF\n- silueta artística de CASA-A\n- frontón\n- entramado superior y diagonales\n" +
                "- CASA-B_REF\n- MERCADO_REF\n- PUENTE_REF\n- 5 anclajes de FAROLES\n\n" +
                "Antes de modificar la escena se creará una copia de respaldo dentro del proyecto.",
                "Restaurar",
                "Cancelar"))
        {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder("Assets/Game");
        EnsureFolder("Assets/Game/Scenes");
        EnsureFolder("Assets/Game/Scenes/Levels");
        EnsureFolder(BackupFolder);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupPath = BackupFolder + "/PuebloLunaria_Blockout_Exact_before_recovery_" + timestamp + ".unity";
        AssetDatabase.CopyAsset(ExactScenePath, backupPath);

        Scene scene = EditorSceneManager.OpenScene(ExactScenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("SCN_PuebloLunaria");
        if (root == null)
        {
            EditorUtility.DisplayDialog(
                "Recuperación de Lunaria",
                "La escena no contiene SCN_PuebloLunaria. No se realizó ningún cambio.",
                "Aceptar");
            return;
        }

        Transform visualBackground = FindDirectChild(root.transform, "Visual_Background");
        if (visualBackground == null)
        {
            visualBackground = CreateEmpty("Visual_Background", root.transform).transform;
        }

        RebuildKnownManualVisuals(visualBackground);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ExactScenePath));

        EditorUtility.DisplayDialog(
            "Recuperación completada",
            "Se restauraron los elementos visuales base.\n\n" +
            "Escena restaurada:\n" + ExactScenePath + "\n\n" +
            "Respaldo previo:\n" + backupPath + "\n\n" +
            "La escena quedó restaurada en su configuración base.",
            "Aceptar");
    }

    private static void RebuildKnownManualVisuals(Transform visualBackground)
    {
        RemoveDirectChildIfExists(visualBackground, "CASA-A");
        RemoveDirectChildIfExists(visualBackground, "CASA-B");
        RemoveDirectChildIfExists(visualBackground, "MERCADO");
        RemoveDirectChildIfExists(visualBackground, "PUENTE");
        RemoveDirectChildIfExists(visualBackground, "FAROLES");

        BuildLegacyHouseA(visualBackground);
        BuildReferenceOnly("CASA-B", visualBackground, new Vector3(37f, 5f, 8f), new Vector3(12f, 9f, 8f));
        BuildReferenceOnly("MERCADO", visualBackground, new Vector3(49f, 3.5f, 5.5f), new Vector3(10f, 5f, 4f));
        BuildReferenceOnly("PUENTE", visualBackground, new Vector3(81f, 0.5f, 7f), new Vector3(14f, 4f, 6f));
        BuildFarolAnchors(visualBackground);
    }

    private static void BuildLegacyHouseA(Transform parent)
    {
        GameObject house = CreateEmpty("CASA-A", parent);
        house.transform.localPosition = new Vector3(14f, 4f, 7f);

        GameObject reference = CreateCube(
            "CASA-A_REF",
            house.transform,
            Vector3.zero,
            new Vector3(10f, 8f, 7f),
            Vector3.zero,
            false);
        reference.SetActive(false);

        GameObject art = CreateEmpty("ART_Blockout", house.transform);

        CreateCube("Body_Lower", art.transform,
            new Vector3(0f, -2.5f, 0f),
            new Vector3(8.2f, 3f, 5.4f),
            Vector3.zero, false);

        CreateCube("Body_Upper", art.transform,
            new Vector3(0f, 0.1f, 0f),
            new Vector3(7.4f, 2.2f, 5.2f),
            Vector3.zero, false);

        CreateCube("Roof_Left", art.transform,
            new Vector3(-2.05f, 2.4f, 0f),
            new Vector3(5f, 0.4f, 6.2f),
            new Vector3(0f, 0f, 35f), false);

        CreateCube("Roof_Right", art.transform,
            new Vector3(2.05f, 2.4f, 0f),
            new Vector3(5f, 0.4f, 6.2f),
            new Vector3(0f, 0f, -35f), false);

        CreateCube("Chimney", art.transform,
            new Vector3(-2.6f, 2.7f, 0.7f),
            new Vector3(0.9f, 2.4f, 0.9f),
            Vector3.zero, false);

        GameObject gable = CreateEmpty("Gable_Front", art.transform);

        CreateCube("Gable_Fill_01", gable.transform,
            new Vector3(0f, 1.5f, -2.65f),
            new Vector3(6.8f, 0.6f, 0.35f),
            Vector3.zero, false);

        CreateCube("Gable_Fill_02", gable.transform,
            new Vector3(0f, 2.1f, -2.65f),
            new Vector3(5f, 0.6f, 0.35f),
            Vector3.zero, false);

        CreateCube("Gable_Fill_03", gable.transform,
            new Vector3(0f, 2.7f, -2.65f),
            new Vector3(3.3f, 0.6f, 0.35f),
            Vector3.zero, false);

        CreateCube("Gable_Fill_04", gable.transform,
            new Vector3(0f, 3.3f, -2.65f),
            new Vector3(1.6f, 0.6f, 0.35f),
            Vector3.zero, false);

        GameObject timber = CreateEmpty("Timber_Frame", art.transform);

        CreateCube("Beam_Upper_Bottom", timber.transform,
            new Vector3(0f, -0.9f, -2.69f),
            new Vector3(7.4f, 0.22f, 0.18f),
            Vector3.zero, false);

        CreateCube("Beam_Upper_Top", timber.transform,
            new Vector3(0f, 1.1f, -2.69f),
            new Vector3(7.4f, 0.22f, 0.18f),
            Vector3.zero, false);

        CreateCube("Beam_Upper_Center", timber.transform,
            new Vector3(0f, 0.1f, -2.69f),
            new Vector3(0.22f, 2.2f, 0.18f),
            Vector3.zero, false);

        CreateCube("Beam_Upper_Left", timber.transform,
            new Vector3(-3.55f, 0.1f, -2.69f),
            new Vector3(0.22f, 2.2f, 0.18f),
            Vector3.zero, false);

        CreateCube("Beam_Upper_Right", timber.transform,
            new Vector3(3.55f, 0.1f, -2.69f),
            new Vector3(0.22f, 2.2f, 0.18f),
            Vector3.zero, false);

        CreateCube("Beam_Upper_Diag_Left", timber.transform,
            new Vector3(-1.72f, 0.1f, -2.72f),
            new Vector3(3.9f, 0.18f, 0.16f),
            new Vector3(0f, 0f, 27f), false);

        CreateCube("Beam_Upper_Diag_Right", timber.transform,
            new Vector3(1.72f, 0.1f, -2.72f),
            new Vector3(3.9f, 0.18f, 0.16f),
            new Vector3(0f, 0f, -27f), false);
    }

    private static void BuildReferenceOnly(string name, Transform parent, Vector3 position, Vector3 size)
    {
        GameObject root = CreateEmpty(name, parent);
        root.transform.localPosition = position;

        GameObject reference = CreateCube(name + "_REF", root.transform, Vector3.zero, size, Vector3.zero, false);
        reference.SetActive(false);
    }

    private static void BuildFarolAnchors(Transform parent)
    {
        GameObject group = CreateEmpty("FAROLES", parent);

        CreateFarol("FAROL-01", group.transform, new Vector3(8f, 0f, 2.8f));
        CreateFarol("FAROL-02", group.transform, new Vector3(30f, 1.5f, 2.8f));
        CreateFarol("FAROL-03", group.transform, new Vector3(55f, 1.5f, 2.8f));
        CreateFarol("FAROL-04", group.transform, new Vector3(68f, 3f, 2.8f));
        CreateFarol("FAROL-05", group.transform, new Vector3(92f, 3f, 2.8f));
    }

    private static void CreateFarol(string name, Transform parent, Vector3 position)
    {
        GameObject farol = CreateEmpty(name, parent);
        farol.transform.localPosition = position;
    }

    private static GameObject CreateEmpty(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    private static GameObject CreateCube(
        string name,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Vector3 localEuler,
        bool keepCollider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler);
        go.transform.localScale = localScale;

        if (!keepCollider)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        }

        return go;
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name) return child;
        }

        return null;
    }

    private static void RemoveDirectChildIfExists(Transform parent, string name)
    {
        Transform child = FindDirectChild(parent, name);
        if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent)) return;

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
