#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PuebloLunariaCompleter
{
    private const string ScenePath = "Assets/PuebloLunaria_Blockout_Exact.unity";
    private const string BackupFolder = "Assets/Game/Scenes/Levels/RecoveryBackups";
    private const string MaterialFolder = "Assets/Game/Art/Enviroment/PuebloLunaria/Generated/Materials";

    private sealed class Palette
    {
        public Material Plaster, Stone, Timber, Roof, Door, Window, MarketCloth, MarketWood;
        public Material Foliage, Trunk, Metal, LampGlow, Crystal, Foreground;
    }

    [MenuItem("Tools/Kingdom Shadow/Complete Current Pueblo de Lunaria")]
    public static void CompleteCurrentLunaria()
    {
        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("Pueblo de Lunaria", "No se encontró " + ScenePath, "Aceptar");
            return;
        }

        if (!EditorUtility.DisplayDialog(
            "Completar Pueblo de Lunaria",
            "Se completará la escena manual existente sin reemplazar el gameplay ya construido.\n\n" +
            "Se completarán:\n- fachada de CASA-A\n- CASA-B artística\n- mercado\n- puente\n- faroles\n" +
            "- background/foreground base\n- iluminación y materiales provisionales\n\n" +
            "Antes de modificar la escena se creará un respaldo automático.",
            "Completar",
            "Cancelar"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder("Assets/Game/Scenes");
        EnsureFolder("Assets/Game/Scenes/Levels");
        EnsureFolder(BackupFolder);
        EnsureFolder("Assets/Game/Art");
        EnsureFolder("Assets/Game/Art/Enviroment");
        EnsureFolder("Assets/Game/Art/Enviroment/PuebloLunaria");
        EnsureFolder("Assets/Game/Art/Enviroment/PuebloLunaria/Generated");
        EnsureFolder(MaterialFolder);

        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupPath = BackupFolder + "/PuebloLunaria_Blockout_Exact_before_complete_" + stamp + ".unity";
        AssetDatabase.CopyAsset(ScenePath, backupPath);

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find("SCN_PuebloLunaria");
        if (root == null)
        {
            EditorUtility.DisplayDialog("Pueblo de Lunaria", "No se encontró SCN_PuebloLunaria.", "Aceptar");
            return;
        }

        Transform background = EnsureDirectChild(root.transform, "Visual_Background");
        Transform foreground = EnsureDirectChild(root.transform, "Visual_Foreground");
        Transform lighting = EnsureDirectChild(root.transform, "Lighting_VFX");

        Palette p = BuildPalette();

        CompleteHouseA(background, p);
        CompleteHouseB(background, p);
        CompleteMarket(background, p);
        CompleteBridge(background, p);
        CompleteFaroles(background, lighting, p);
        CompleteScenicBackground(background, p);
        CompleteForeground(foreground, p);
        CompleteLighting(lighting);
        TuneExistingCamera();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

        EditorUtility.DisplayDialog(
            "Pueblo de Lunaria completado",
            "Se completó el blockout artístico sobre tu escena manual existente.\n\n" +
            "Escena:\n" + ScenePath + "\n\nRespaldo previo:\n" + backupPath +
            "\n\nNo se modificó la geometría jugable P-G01/P-R01/P-G02/P-R02/P-G03.",
            "Aceptar");
    }

    private static void CompleteHouseA(Transform background, Palette p)
    {
        Transform house = EnsureDirectChild(background, "CASA-A");
        if (house.localPosition == Vector3.zero) house.localPosition = new Vector3(14f, 4f, 7f);

        Transform art = EnsureDirectChild(house, "ART_Blockout");
        Transform timber = EnsureDirectChild(art, "Timber_Frame");

        EnsureCube("Beam_Gable_Center", timber,
            new Vector3(0f, 2.25f, -2.84f), new Vector3(0.22f, 2.3f, 0.16f), Vector3.zero, p.Timber);
        EnsureCube("Beam_Gable_Left", timber,
            new Vector3(-1.7f, 2.25f, -2.84f), new Vector3(3.9f, 0.18f, 0.16f), new Vector3(0f,0f,31f), p.Timber);
        EnsureCube("Beam_Gable_Right", timber,
            new Vector3(1.7f, 2.25f, -2.84f), new Vector3(3.9f, 0.18f, 0.16f), new Vector3(0f,0f,-31f), p.Timber);

        EnsureCube("Door", art,
            new Vector3(-1.7f, -2.55f, -2.78f), new Vector3(1.35f, 2.5f, 0.18f), Vector3.zero, p.Door);
        EnsureCube("Window_Lower", art,
            new Vector3(1.55f, -2.3f, -2.78f), new Vector3(1.4f, 1.2f, 0.16f), Vector3.zero, p.Window);
        EnsureCube("Window_Upper_Left", art,
            new Vector3(-1.95f, 0.15f, -2.82f), new Vector3(1.25f, 1.05f, 0.15f), Vector3.zero, p.Window);
        EnsureCube("Window_Upper_Right", art,
            new Vector3(1.95f, 0.15f, -2.82f), new Vector3(1.25f, 1.05f, 0.15f), Vector3.zero, p.Window);
        EnsureCube("Stone_Base", art,
            new Vector3(0f, -3.82f, -2.75f), new Vector3(8.5f, 0.35f, 0.35f), Vector3.zero, p.Stone);

        ApplyMaterialToChild(art, "Body_Lower", p.Stone);
        ApplyMaterialToChild(art, "Body_Upper", p.Plaster);
        ApplyMaterialToChild(art, "Roof_Left", p.Roof);
        ApplyMaterialToChild(art, "Roof_Right", p.Roof);
        ApplyMaterialToChild(art, "Chimney", p.Stone);

        Transform gable = FindDirectChild(art, "Gable_Front");
        if (gable != null)
        {
            for (int i = 0; i < gable.childCount; i++)
                ApplyMaterial(gable.GetChild(i).gameObject, p.Plaster);
        }

        for (int i = 0; i < timber.childCount; i++)
            ApplyMaterial(timber.GetChild(i).gameObject, p.Timber);
    }

    private static void CompleteHouseB(Transform background, Palette p)
    {
        Transform house = EnsureDirectChild(background, "CASA-B");
        house.localPosition = new Vector3(37f, 5f, 8f);
        Transform art = EnsureDirectChild(house, "ART_Blockout");

        EnsureCube("Body_Lower", art, new Vector3(0f,-2.9f,0f), new Vector3(9.8f,3.2f,6.2f), Vector3.zero, p.Stone);
        EnsureCube("Body_Upper", art, new Vector3(0f,0f,0f), new Vector3(10.6f,2.8f,6f), Vector3.zero, p.Plaster);
        EnsureCube("Roof_Left", art, new Vector3(-2.55f,2.85f,0f), new Vector3(6.35f,0.45f,7.2f), new Vector3(0,0,34f), p.Roof);
        EnsureCube("Roof_Right", art, new Vector3(2.55f,2.85f,0f), new Vector3(6.35f,0.45f,7.2f), new Vector3(0,0,-34f), p.Roof);
        EnsureCube("Chimney", art, new Vector3(3.5f,3.1f,0.9f), new Vector3(1f,2.8f,1f), Vector3.zero, p.Stone);

        Transform timber = EnsureDirectChild(art, "Timber_Frame");
        EnsureCube("Beam_Bottom", timber, new Vector3(0,-1.25f,-3.08f), new Vector3(10.5f,.25f,.2f), Vector3.zero, p.Timber);
        EnsureCube("Beam_Top", timber, new Vector3(0,1.25f,-3.08f), new Vector3(10.5f,.25f,.2f), Vector3.zero, p.Timber);
        EnsureCube("Beam_Center", timber, new Vector3(0,0,-3.08f), new Vector3(.25f,2.8f,.2f), Vector3.zero, p.Timber);
        EnsureCube("Beam_Left", timber, new Vector3(-5.05f,0,-3.08f), new Vector3(.25f,2.8f,.2f), Vector3.zero, p.Timber);
        EnsureCube("Beam_Right", timber, new Vector3(5.05f,0,-3.08f), new Vector3(.25f,2.8f,.2f), Vector3.zero, p.Timber);
        EnsureCube("Diag_Left", timber, new Vector3(-2.5f,0,-3.11f), new Vector3(5.35f,.2f,.18f), new Vector3(0,0,28f), p.Timber);
        EnsureCube("Diag_Right", timber, new Vector3(2.5f,0,-3.11f), new Vector3(5.35f,.2f,.18f), new Vector3(0,0,-28f), p.Timber);

        EnsureCube("Door", art, new Vector3(2.6f,-2.9f,-3.2f), new Vector3(1.5f,2.7f,.18f), Vector3.zero, p.Door);
        EnsureCube("Window_Lower_Left", art, new Vector3(-2.5f,-2.6f,-3.2f), new Vector3(1.6f,1.3f,.16f), Vector3.zero, p.Window);
        EnsureCube("Window_Upper_Left", art, new Vector3(-2.5f,.05f,-3.18f), new Vector3(1.45f,1.15f,.16f), Vector3.zero, p.Window);
        EnsureCube("Window_Upper_Right", art, new Vector3(2.5f,.05f,-3.18f), new Vector3(1.45f,1.15f,.16f), Vector3.zero, p.Window);
        EnsureCube("Balcony_Slab", art, new Vector3(0,-1.25f,-3.55f), new Vector3(5.8f,.25f,1.1f), Vector3.zero, p.Timber);
        EnsureCube("Balcony_Rail", art, new Vector3(0,-.55f,-4f), new Vector3(5.8f,.15f,.15f), Vector3.zero, p.Timber);
    }

    private static void CompleteMarket(Transform background, Palette p)
    {
        Transform market = EnsureDirectChild(background, "MERCADO");
        market.localPosition = new Vector3(49f, 3.5f, 5.5f);
        Transform art = EnsureDirectChild(market, "ART_Blockout");

        EnsureCube("Counter", art, new Vector3(0,-1.55f,-1.15f), new Vector3(6.5f,1.1f,1.1f), Vector3.zero, p.MarketWood);
        EnsureCube("Back_Table", art, new Vector3(0,-1.7f,.7f), new Vector3(5.8f,.8f,1.2f), Vector3.zero, p.MarketWood);
        EnsureCube("Post_Left", art, new Vector3(-3.35f,-.1f,-.7f), new Vector3(.25f,3.8f,.25f), Vector3.zero, p.Timber);
        EnsureCube("Post_Right", art, new Vector3(3.35f,-.1f,-.7f), new Vector3(.25f,3.8f,.25f), Vector3.zero, p.Timber);
        EnsureCube("Canopy", art, new Vector3(0,1.65f,-.25f), new Vector3(7.6f,.25f,3.2f), new Vector3(0,0,-4f), p.MarketCloth);
        EnsureCube("Crate_01", art, new Vector3(-3.9f,-1.75f,-1.25f), new Vector3(1.1f,1f,1f), Vector3.zero, p.MarketWood);
        EnsureCube("Crate_02", art, new Vector3(3.85f,-1.8f,-1f), new Vector3(.9f,.9f,.9f), new Vector3(0,12f,0), p.MarketWood);
    }

    private static void CompleteBridge(Transform background, Palette p)
    {
        Transform bridge = EnsureDirectChild(background, "PUENTE");
        bridge.localPosition = new Vector3(81f, .5f, 7f);
        Transform art = EnsureDirectChild(bridge, "ART_Blockout");

        EnsureCube("Deck", art, new Vector3(0,1.4f,0), new Vector3(14f,.55f,5f), Vector3.zero, p.Stone);
        EnsureCube("Pier_Left", art, new Vector3(-5.1f,-.2f,0), new Vector3(2.1f,3.2f,5f), Vector3.zero, p.Stone);
        EnsureCube("Pier_Right", art, new Vector3(5.1f,-.2f,0), new Vector3(2.1f,3.2f,5f), Vector3.zero, p.Stone);
        EnsureCube("Arch_Header", art, new Vector3(0,.65f,0), new Vector3(8.1f,.8f,5f), Vector3.zero, p.Stone);
        EnsureCube("Rail_Front", art, new Vector3(0,2.05f,-2.35f), new Vector3(14f,.3f,.3f), Vector3.zero, p.Stone);
        EnsureCube("Rail_Back", art, new Vector3(0,2.05f,2.35f), new Vector3(14f,.3f,.3f), Vector3.zero, p.Stone);
    }

    private static void CompleteFaroles(Transform background, Transform lighting, Palette p)
    {
        Transform group = EnsureDirectChild(background, "FAROLES");
        float[] xs = {8f,30f,55f,68f,92f};
        float[] ys = {0f,1.5f,1.5f,3f,3f};

        for (int i = 0; i < 5; i++)
        {
            string n = "FAROL-0" + (i+1);
            Transform anchor = EnsureDirectChild(group, n);
            anchor.localPosition = new Vector3(xs[i], ys[i], 2.8f);

            EnsureCylinder("Pole", anchor, new Vector3(0,1.55f,0), new Vector3(.12f,1.55f,.12f), p.Metal);
            EnsureCube("Arm", anchor, new Vector3(-.35f,2.85f,-.02f), new Vector3(.8f,.12f,.12f), new Vector3(0,0,10f), p.Metal);
            EnsureCube("Lamp", anchor, new Vector3(-.7f,2.55f,-.02f), new Vector3(.45f,.6f,.45f), new Vector3(0,45f,0), p.LampGlow);

            string lightName = "LampLight_0" + (i+1);
            Transform lightT = EnsureDirectChild(lighting, lightName);
            lightT.position = anchor.TransformPoint(new Vector3(-.7f,2.55f,-.02f));
            Light light = lightT.GetComponent<Light>();
            if (light == null) light = lightT.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f,.72f,.38f);
            light.intensity = 1.6f;
            light.range = 5.5f;
            light.shadows = LightShadows.Soft;
        }
    }

    private static void CompleteScenicBackground(Transform background, Palette p)
    {
        Transform scenic = EnsureDirectChild(background, "SCENIC_Background");
        EnsureTree("Tree_Bridge", scenic, new Vector3(90f,3f,11f), 1.25f, p);
        EnsureTree("Tree_Market", scenic, new Vector3(57f,2.7f,10.5f), .85f, p);
        EnsureTree("Tree_Entrance", scenic, new Vector3(2f,1.5f,9.5f), .95f, p);

        EnsureSimpleHouse("DistantHouse_01", scenic, new Vector3(4f,5.5f,14f), .8f, p);
        EnsureSimpleHouse("DistantHouse_02", scenic, new Vector3(63f,6.7f,15f), 1f, p);
        EnsureSimpleHouse("DistantHouse_03", scenic, new Vector3(99f,6.2f,14f), .9f, p);

        EnsureCube("StoneWall_Entrance", scenic, new Vector3(7f,.7f,4.4f), new Vector3(7f,1.4f,.8f), Vector3.zero, p.Stone);
        EnsureCube("StoneWall_Market", scenic, new Vector3(58f,2.3f,4.6f), new Vector3(5f,1.2f,.8f), Vector3.zero, p.Stone);
    }

    private static void CompleteForeground(Transform foreground, Palette p)
    {
        Transform scenic = EnsureDirectChild(foreground, "SCENIC_Foreground");
        EnsureSphere("Bush_FG_01", scenic, new Vector3(18f,.45f,-4.8f), new Vector3(1.8f,.9f,.9f), p.Foreground);
        EnsureSphere("Bush_FG_02", scenic, new Vector3(52f,1.9f,-5f), new Vector3(2.2f,1.1f,1f), p.Foreground);
        EnsureSphere("Bush_FG_03", scenic, new Vector3(87f,3.45f,-5.2f), new Vector3(2f,1f,1f), p.Foreground);
        EnsureCube("Rock_FG_01", scenic, new Vector3(34f,1.7f,-4.6f), new Vector3(1.5f,.9f,1f), new Vector3(7f,20f,10f), p.Stone);
        EnsureCube("Rock_FG_02", scenic, new Vector3(74f,3.35f,-4.7f), new Vector3(1.8f,1f,1.2f), new Vector3(-8f,-18f,5f), p.Stone);
    }

    private static void CompleteLighting(Transform lighting)
    {
        Transform directionalT = EnsureDirectChild(lighting, "Directional Light_Lunaria");
        directionalT.localRotation = Quaternion.Euler(42f,-32f,0f);
        Light directional = directionalT.GetComponent<Light>();
        if (directional == null) directional = directionalT.gameObject.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(1f,.91f,.78f);
        directional.intensity = 1.15f;
        directional.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.24f,.28f,.35f);
        RenderSettings.ambientEquatorColor = new Color(.18f,.17f,.18f);
        RenderSettings.ambientGroundColor = new Color(.08f,.07f,.08f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.16f,.18f,.22f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 95f;
    }

    private static void TuneExistingCamera()
    {
        GameObject camObj = GameObject.Find("Main Camera");
        if (camObj == null) return;
        Camera cam = camObj.GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = 35f;
    }

    private static void EnsureTree(string name, Transform parent, Vector3 pos, float scale, Palette p)
    {
        Transform tree = EnsureDirectChild(parent, name);
        tree.localPosition = pos;
        tree.localScale = Vector3.one * scale;
        EnsureCylinder("Trunk", tree, new Vector3(0,2f,0), new Vector3(.45f,2f,.45f), p.Trunk);
        EnsureSphere("Canopy_01", tree, new Vector3(0,5f,0), new Vector3(3.2f,2.4f,2.3f), p.Foliage);
        EnsureSphere("Canopy_02", tree, new Vector3(-1.4f,4.4f,.2f), new Vector3(2.2f,1.8f,1.8f), p.Foliage);
        EnsureSphere("Canopy_03", tree, new Vector3(1.4f,4.6f,-.1f), new Vector3(2.2f,1.9f,1.8f), p.Foliage);
    }

    private static void EnsureSimpleHouse(string name, Transform parent, Vector3 pos, float scale, Palette p)
    {
        Transform house = EnsureDirectChild(parent, name);
        house.localPosition = pos;
        house.localScale = Vector3.one * scale;
        EnsureCube("Body", house, Vector3.zero, new Vector3(6f,5f,4f), Vector3.zero, p.Plaster);
        EnsureCube("Roof_L", house, new Vector3(-1.5f,3.1f,0), new Vector3(4f,.35f,4.8f), new Vector3(0,0,35f), p.Roof);
        EnsureCube("Roof_R", house, new Vector3(1.5f,3.1f,0), new Vector3(4f,.35f,4.8f), new Vector3(0,0,-35f), p.Roof);
    }

    private static Transform EnsureDirectChild(Transform parent, string name)
    {
        Transform child = FindDirectChild(parent, name);
        if (child != null) return child;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
            if (parent.GetChild(i).name == name) return parent.GetChild(i);
        return null;
    }

    private static GameObject EnsureCube(string name, Transform parent, Vector3 pos, Vector3 scale, Vector3 euler, Material mat)
    {
        Transform existing = FindDirectChild(parent, name);
        GameObject go = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (existing == null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        RemoveCollider(go);
        ApplyMaterial(go, mat);
        return go;
    }

    private static GameObject EnsureCylinder(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        Transform existing = FindDirectChild(parent, name);
        GameObject go = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        if (existing == null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = scale;
        RemoveCollider(go);
        ApplyMaterial(go, mat);
        return go;
    }

    private static GameObject EnsureSphere(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        Transform existing = FindDirectChild(parent, name);
        GameObject go = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        if (existing == null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = scale;
        RemoveCollider(go);
        ApplyMaterial(go, mat);
        return go;
    }

    private static void ApplyMaterialToChild(Transform parent, string name, Material mat)
    {
        Transform child = FindDirectChild(parent, name);
        if (child != null) ApplyMaterial(child.gameObject, mat);
    }

    private static void ApplyMaterial(GameObject go, Material mat)
    {
        if (mat == null) return;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    private static void RemoveCollider(GameObject go)
    {
        Collider c = go.GetComponent<Collider>();
        if (c != null) UnityEngine.Object.DestroyImmediate(c);
    }

    private static Palette BuildPalette()
    {
        return new Palette
        {
            Plaster = CreateOrUpdateMaterial("MAT_Plaster", new Color(.72f,.66f,.54f), 0f,.15f),
            Stone = CreateOrUpdateMaterial("MAT_Stone", new Color(.36f,.38f,.40f), 0f,.18f),
            Timber = CreateOrUpdateMaterial("MAT_Timber", new Color(.18f,.10f,.06f), 0f,.12f),
            Roof = CreateOrUpdateMaterial("MAT_Roof", new Color(.20f,.19f,.20f), 0f,.18f),
            Door = CreateOrUpdateMaterial("MAT_Door", new Color(.24f,.12f,.06f), 0f,.12f),
            Window = CreateOrUpdateMaterial("MAT_Window", new Color(.18f,.31f,.38f), .05f,.55f),
            MarketCloth = CreateOrUpdateMaterial("MAT_MarketCloth", new Color(.42f,.12f,.10f), 0f,.25f),
            MarketWood = CreateOrUpdateMaterial("MAT_MarketWood", new Color(.31f,.18f,.09f), 0f,.15f),
            Foliage = CreateOrUpdateMaterial("MAT_Foliage", new Color(.18f,.28f,.18f), 0f,.08f),
            Trunk = CreateOrUpdateMaterial("MAT_Trunk", new Color(.20f,.12f,.07f), 0f,.08f),
            Metal = CreateOrUpdateMaterial("MAT_Metal", new Color(.12f,.13f,.15f), .65f,.35f),
            LampGlow = CreateOrUpdateMaterial("MAT_LampGlow", new Color(.92f,.58f,.22f), 0f,.48f),
            Crystal = CreateOrUpdateMaterial("MAT_Crystal", new Color(.18f,.65f,.78f), .2f,.7f),
            Foreground = CreateOrUpdateMaterial("MAT_Foreground", new Color(.10f,.16f,.11f), 0f,.06f)
        };
    }

    private static Material CreateOrUpdateMaterial(string name, Color color, float metallic, float smoothness)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        if (mat == null)
        {
            mat = new Material(shader);
            mat.name = name;
            AssetDatabase.CreateAsset(mat, path);
        }
        else if (shader != null && mat.shader != shader)
        {
            mat.shader = shader;
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\','/');
        string folder = Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent)) return;
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
#endif
