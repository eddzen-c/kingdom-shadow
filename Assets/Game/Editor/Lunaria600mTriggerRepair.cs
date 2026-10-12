#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Lunaria600mTriggerRepair
{
    private const string TargetName = "B-H01_Abismo_Trigger";

    [MenuItem("Tools/Kingdom Shadow/Repair 600m/Repair B-H01 Abismo Trigger")]
    public static void RepairAbismoTrigger()
    {
        GameObject target = FindLoadedObject(TargetName);

        if (target == null)
        {
            EditorUtility.DisplayDialog(
                "Reparación de trigger",
                "No se encontró el objeto '" + TargetName + "' en la escena abierta.\n\nAbre la escena PuebloLunaria_600m y vuelve a ejecutar esta opción.",
                "Aceptar");
            return;
        }

        BoxCollider box = target.GetComponent<BoxCollider>();
        bool created = false;

        if (box == null)
        {
            box = Undo.AddComponent<BoxCollider>(target);
            created = true;
        }

        box.isTrigger = true;

        if (created)
        {
            if (!TryFitToRelatedBounds(target, box))
            {
                // Conserva Size = (1,1,1). Si el GameObject ya estaba escalado,
                // el volumen efectivo del trigger respeta ese Transform.
                box.center = Vector3.zero;
                box.size = Vector3.one;
            }
        }

        EditorUtility.SetDirty(target);
        EditorUtility.SetDirty(box);
        EditorSceneManager.MarkSceneDirty(target.scene);
        EditorSceneManager.SaveScene(target.scene);

        Selection.activeGameObject = target;
        EditorGUIUtility.PingObject(target);

        EditorUtility.DisplayDialog(
            "Trigger reparado",
            "'" + TargetName + "' ya tiene un BoxCollider con Is Trigger activado.\n\nLa escena se guardó automáticamente.",
            "Aceptar");
    }

    private static GameObject FindLoadedObject(string objectName)
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in all)
        {
            if (go == null || go.name != objectName)
                continue;

            if (!go.scene.IsValid() || !go.scene.isLoaded)
                continue;

            if (EditorUtility.IsPersistent(go))
                continue;

            return go;
        }

        return null;
    }

    private static bool TryFitToRelatedBounds(GameObject target, BoxCollider box)
    {
        Bounds? bounds = GetDescendantBounds(target.transform);

        if (!bounds.HasValue && target.transform.parent != null)
        {
            string prefix = "B-H01";

            foreach (Transform sibling in target.transform.parent)
            {
                if (sibling == target.transform)
                    continue;

                if (!sibling.name.StartsWith(prefix))
                    continue;

                Bounds? siblingBounds = GetDescendantBounds(sibling);

                if (siblingBounds.HasValue)
                {
                    bounds = siblingBounds;
                    break;
                }
            }
        }

        if (!bounds.HasValue)
            return false;

        Bounds worldBounds = bounds.Value;

        Vector3 localCenter = target.transform.InverseTransformPoint(worldBounds.center);
        Vector3 localMin = target.transform.InverseTransformPoint(worldBounds.min);
        Vector3 localMax = target.transform.InverseTransformPoint(worldBounds.max);

        Vector3 localSize = new Vector3(
            Mathf.Abs(localMax.x - localMin.x),
            Mathf.Abs(localMax.y - localMin.y),
            Mathf.Abs(localMax.z - localMin.z));

        if (localSize.x < 0.01f || localSize.y < 0.01f || localSize.z < 0.01f)
            return false;

        box.center = localCenter;
        box.size = localSize;
        return true;
    }

    private static Bounds? GetDescendantBounds(Transform root)
    {
        bool hasBounds = false;
        Bounds combined = default;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (!hasBounds)
            {
                combined = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(renderer.bounds);
            }
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider == null || collider.transform == root)
                continue;

            if (!hasBounds)
            {
                combined = collider.bounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(collider.bounds);
            }
        }

        return hasBounds ? combined : (Bounds?)null;
    }
}
#endif
