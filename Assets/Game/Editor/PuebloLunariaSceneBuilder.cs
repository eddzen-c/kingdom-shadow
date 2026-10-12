#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PuebloLunariaSceneBuilder
{
    private const string ScenePath = "Assets/Game/Scenes/Levels/PuebloLunaria_Main.unity";
    private const string MaterialFolder = "Assets/Game/Art/Enviroment/PuebloLunaria/Generated/Materials";

    private sealed class Palette
    {
        public Material Ground;
        public Material Ramp;
        public Material Plaster;
        public Material Stone;
        public Material Timber;
        public Material Roof;
        public Material Door;
        public Material Window;
        public Material MarketCloth;
        public Material MarketWood;
        public Material Foliage;
        public Material Trunk;
        public Material Metal;
        public Material LampGlow;
        public Material Crystal;
        public Material Foreground;
    }

    [MenuItem("Tools/Kingdom Shadow/Build Pueblo de Lunaria")]
    public static void BuildPuebloLunaria()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder("Assets/Game/Scenes");
        EnsureFolder("Assets/Game/Scenes/Levels");
        EnsureFolder("Assets/Game/Art");
        EnsureFolder("Assets/Game/Art/Enviroment");
        EnsureFolder("Assets/Game/Art/Enviroment/PuebloLunaria");
        EnsureFolder("Assets/Game/Art/Enviroment/PuebloLunaria/Generated");
        EnsureFolder(MaterialFolder);

        Palette palette = BuildPalette();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = CreateEmpty("SCN_PuebloLunaria", null);
        GameObject gameplay = CreateEmpty("Gameplay_Geo", root.transform);
        CreateEmpty("Hazards", root.transform);
        GameObject background = CreateEmpty("Visual_Background", root.transform);
        GameObject foreground = CreateEmpty("Visual_Foreground", root.transform);
        GameObject markers = CreateEmpty("Markers", root.transform);
        CreateEmpty("Enemies", root.transform);
        GameObject lightingVfx = CreateEmpty("Lighting_VFX", root.transform);
        GameObject cameraRig = CreateEmpty("Camera_Rig", root.transform);

        BuildGameplay(gameplay.transform, palette);
        GameObject player = BuildPlayer(root.transform, palette);
        BuildMarkers(markers.transform, palette);
        BuildHouseA(background.transform, palette);
        BuildHouseB(background.transform, palette);
        BuildMarket(background.transform, palette);
        BuildBridge(background.transform, palette);
        BuildFaroles(background.transform, lightingVfx.transform, palette);
        BuildScenicBackground(background.transform, palette);
        BuildForeground(foreground.transform, palette);
        BuildLighting(lightingVfx.transform);
        BuildCamera(cameraRig.transform, player.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EnsureSceneInBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

        EditorUtility.DisplayDialog(
            "Pueblo de Lunaria",
            "Escena generada correctamente en:\n\n" + ScenePath +
            "\n\nEl layout jugable usa las medidas exactas del CSV. Los detalles arquitectónicos y decorativos son un blockout artístico basado en el boceto y pueden reemplazarse después por modelos FBX sin cambiar el gameplay.",
            "Aceptar");
    }

    [MenuItem("Tools/Kingdom Shadow/Open Pueblo de Lunaria")]
    public static void OpenPuebloLunaria()
    {
        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog(
                "Pueblo de Lunaria",
                "La escena todavía no existe. Ejecuta primero Tools > Kingdom Shadow > Build Pueblo de Lunaria.",
                "Aceptar");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static void BuildGameplay(Transform parent, Palette p)
    {
        CreateCube("P-G01", parent, new Vector3(12f, -0.5f, 0f), new Vector3(24f, 1f, 4f), Vector3.zero, p.Ground, true);
        CreateCube("P-R01", parent, new Vector3(27f, 0.75f, 0f), new Vector3(6f, 0.4f, 4f), new Vector3(0f, 0f, 14.04f), p.Ramp, true);
        CreateCube("P-G02", parent, new Vector3(43f, 1f, 0f), new Vector3(26f, 1f, 4f), Vector3.zero, p.Ground, true);
        CreateCube("P-R02", parent, new Vector3(60f, 2.25f, 0f), new Vector3(8f, 0.4f, 4f), new Vector3(0f, 0f, 10.62f), p.Ramp, true);
        CreateCube("P-G03", parent, new Vector3(80f, 2.5f, 0f), new Vector3(32f, 1f, 6f), Vector3.zero, p.Ground, true);
    }

    private static GameObject BuildPlayer(Transform parent, Palette p)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player_Blockout";
        player.transform.SetParent(parent, false);
        player.transform.localPosition = new Vector3(4f, 0.9f, 0f);
        player.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
        ApplyMaterial(player, p.Crystal);

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        player.AddComponent<PlayerBlockoutController>();
        return player;
    }

    private static void BuildMarkers(Transform parent, Palette p)
    {
        CreateMarker("INICIO", parent, new Vector3(4f, 0.9f, 0f), false, Vector3.one);
        CreateMarker("CP-01", parent, new Vector3(50f, 2.4f, 0f), true, Vector3.one);
        CreateMarker("TR-BOSS", parent, new Vector3(70f, 4f, 0f), true, Vector3.one);

        GameObject crystal = CreateMarker("CRISTAL-01", parent, new Vector3(93f, 4f, 0f), true, Vector3.one);
        GameObject visual = CreateCube("Crystal_Visual", crystal.transform, Vector3.zero, new Vector3(0.55f, 0.9f, 0.55f), Vector3.zero, p.Crystal, false);
        visual.transform.localRotation = Quaternion.Euler(20f, 45f, 45f);
    }

    private static void BuildHouseA(Transform parent, Palette p)
    {
        GameObject house = CreateEmpty("CASA-A", parent);
        house.transform.localPosition = new Vector3(14f, 4f, 7f);
        CreateReferenceCube("CASA-A_REF", house.transform, new Vector3(10f, 8f, 7f));
        GameObject art = CreateEmpty("ART_Blockout", house.transform);

        CreateCube("Body_Lower", art.transform, new Vector3(0f, -2.5f, 0f), new Vector3(8.2f, 3f, 5.4f), Vector3.zero, p.Stone, false);
        CreateCube("Body_Upper", art.transform, new Vector3(0f, 0.1f, 0f), new Vector3(7.4f, 2.2f, 5.2f), Vector3.zero, p.Plaster, false);
        CreateCube("Roof_Left", art.transform, new Vector3(-2.05f, 2.4f, 0f), new Vector3(5f, 0.4f, 6.2f), new Vector3(0f, 0f, 35f), p.Roof, false);
        CreateCube("Roof_Right", art.transform, new Vector3(2.05f, 2.4f, 0f), new Vector3(5f, 0.4f, 6.2f), new Vector3(0f, 0f, -35f), p.Roof, false);
        CreateCube("Chimney", art.transform, new Vector3(-2.6f, 2.7f, 0.7f), new Vector3(0.9f, 2.4f, 0.9f), Vector3.zero, p.Stone, false);

        GameObject gable = CreateEmpty("Gable_Front", art.transform);
        CreateCube("Gable_Fill_01", gable.transform, new Vector3(0f, 1.5f, -2.65f), new Vector3(6.8f, 0.6f, 0.35f), Vector3.zero, p.Plaster, false);
        CreateCube("Gable_Fill_02", gable.transform, new Vector3(0f, 2.1f, -2.65f), new Vector3(5f, 0.6f, 0.35f), Vector3.zero, p.Plaster, false);
        CreateCube("Gable_Fill_03", gable.transform, new Vector3(0f, 2.7f, -2.65f), new Vector3(3.3f, 0.6f, 0.35f), Vector3.zero, p.Plaster, false);
        CreateCube("Gable_Fill_04", gable.transform, new Vector3(0f, 3.3f, -2.65f), new Vector3(1.6f, 0.6f, 0.35f), Vector3.zero, p.Plaster, false);

        GameObject timber = CreateEmpty("Timber_Frame", art.transform);
        CreateBeam("Beam_Upper_Bottom", timber.transform, new Vector3(0f, -0.9f, -2.69f), new Vector3(7.4f, 0.22f, 0.18f), 0f, p.Timber);
        CreateBeam("Beam_Upper_Top", timber.transform, new Vector3(0f, 1.1f, -2.69f), new Vector3(7.4f, 0.22f, 0.18f), 0f, p.Timber);
        CreateBeam("Beam_Upper_Center", timber.transform, new Vector3(0f, 0.1f, -2.69f), new Vector3(0.22f, 2.2f, 0.18f), 0f, p.Timber);
        CreateBeam("Beam_Upper_Left", timber.transform, new Vector3(-3.55f, 0.1f, -2.69f), new Vector3(0.22f, 2.2f, 0.18f), 0f, p.Timber);
        CreateBeam("Beam_Upper_Right", timber.transform, new Vector3(3.55f, 0.1f, -2.69f), new Vector3(0.22f, 2.2f, 0.18f), 0f, p.Timber);
        CreateBeam("Beam_Upper_Diag_Left", timber.transform, new Vector3(-1.72f, 0.1f, -2.72f), new Vector3(3.9f, 0.18f, 0.16f), 27f, p.Timber);
        CreateBeam("Beam_Upper_Diag_Right", timber.transform, new Vector3(1.72f, 0.1f, -2.72f), new Vector3(3.9f, 0.18f, 0.16f), -27f, p.Timber);
        CreateBeam("Beam_Gable_Center", timber.transform, new Vector3(0f, 2.25f, -2.84f), new Vector3(0.22f, 2.3f, 0.16f), 0f, p.Timber);
        CreateBeam("Beam_Gable_Left", timber.transform, new Vector3(-1.7f, 2.25f, -2.84f), new Vector3(3.9f, 0.18f, 0.16f), 31f, p.Timber);
        CreateBeam("Beam_Gable_Right", timber.transform, new Vector3(1.7f, 2.25f, -2.84f), new Vector3(3.9f, 0.18f, 0.16f), -31f, p.Timber);

        CreateCube("Door", art.transform, new Vector3(-1.7f, -2.55f, -2.78f), new Vector3(1.35f, 2.5f, 0.18f), Vector3.zero, p.Door, false);
        CreateCube("Window_Lower", art.transform, new Vector3(1.55f, -2.3f, -2.78f), new Vector3(1.4f, 1.2f, 0.16f), Vector3.zero, p.Window, false);
        CreateCube("Window_Upper_Left", art.transform, new Vector3(-1.95f, 0.15f, -2.82f), new Vector3(1.25f, 1.05f, 0.15f), Vector3.zero, p.Window, false);
        CreateCube("Window_Upper_Right", art.transform, new Vector3(1.95f, 0.15f, -2.82f), new Vector3(1.25f, 1.05f, 0.15f), Vector3.zero, p.Window, false);
        CreateCube("Stone_Base", art.transform, new Vector3(0f, -3.82f, -2.75f), new Vector3(8.5f, 0.35f, 0.35f), Vector3.zero, p.Stone, false);
    }

    private static void BuildHouseB(Transform parent, Palette p)
    {
        GameObject house = CreateEmpty("CASA-B", parent);
        house.transform.localPosition = new Vector3(37f, 5f, 8f);
        CreateReferenceCube("CASA-B_REF", house.transform, new Vector3(12f, 9f, 8f));
        GameObject art = CreateEmpty("ART_Blockout", house.transform);

        CreateCube("Body_Lower", art.transform, new Vector3(0f, -2.9f, 0f), new Vector3(9.8f, 3.2f, 6.2f), Vector3.zero, p.Stone, false);
        CreateCube("Body_Upper", art.transform, new Vector3(0f, 0f, 0f), new Vector3(10.6f, 2.8f, 6f), Vector3.zero, p.Plaster, false);
        CreateCube("Roof_Left", art.transform, new Vector3(-2.55f, 2.85f, 0f), new Vector3(6.35f, 0.45f, 7.2f), new Vector3(0f, 0f, 34f), p.Roof, false);
        CreateCube("Roof_Right", art.transform, new Vector3(2.55f, 2.85f, 0f), new Vector3(6.35f, 0.45f, 7.2f), new Vector3(0f, 0f, -34f), p.Roof, false);
        CreateCube("Chimney", art.transform, new Vector3(3.5f, 3.1f, 0.9f), new Vector3(1f, 2.8f, 1f), Vector3.zero, p.Stone, false);

        GameObject timber = CreateEmpty("Timber_Frame", art.transform);
        CreateBeam("Beam_Bottom", timber.transform, new Vector3(0f, -1.25f, -3.08f), new Vector3(10.5f, 0.25f, 0.2f), 0f, p.Timber);
        CreateBeam("Beam_Top", timber.transform, new Vector3(0f, 1.25f, -3.08f), new Vector3(10.5f, 0.25f, 0.2f), 0f, p.Timber);
        CreateBeam("Beam_Center", timber.transform, new Vector3(0f, 0f, -3.08f), new Vector3(0.25f, 2.8f, 0.2f), 0f, p.Timber);
        CreateBeam("Beam_Left", timber.transform, new Vector3(-5.05f, 0f, -3.08f), new Vector3(0.25f, 2.8f, 0.2f), 0f, p.Timber);
        CreateBeam("Beam_Right", timber.transform, new Vector3(5.05f, 0f, -3.08f), new Vector3(0.25f, 2.8f, 0.2f), 0f, p.Timber);
        CreateBeam("Diag_Left", timber.transform, new Vector3(-2.5f, 0f, -3.11f), new Vector3(5.35f, 0.2f, 0.18f), 28f, p.Timber);
        CreateBeam("Diag_Right", timber.transform, new Vector3(2.5f, 0f, -3.11f), new Vector3(5.35f, 0.2f, 0.18f), -28f, p.Timber);

        CreateCube("Door", art.transform, new Vector3(2.6f, -2.9f, -3.2f), new Vector3(1.5f, 2.7f, 0.18f), Vector3.zero, p.Door, false);
        CreateCube("Window_Lower_Left", art.transform, new Vector3(-2.5f, -2.6f, -3.2f), new Vector3(1.6f, 1.3f, 0.16f), Vector3.zero, p.Window, false);
        CreateCube("Window_Upper_Left", art.transform, new Vector3(-2.5f, 0.05f, -3.18f), new Vector3(1.45f, 1.15f, 0.16f), Vector3.zero, p.Window, false);
        CreateCube("Window_Upper_Right", art.transform, new Vector3(2.5f, 0.05f, -3.18f), new Vector3(1.45f, 1.15f, 0.16f), Vector3.zero, p.Window, false);
        CreateCube("Balcony_Slab", art.transform, new Vector3(0f, -1.25f, -3.55f), new Vector3(5.8f, 0.25f, 1.1f), Vector3.zero, p.Timber, false);
        CreateCube("Balcony_Rail", art.transform, new Vector3(0f, -0.55f, -4f), new Vector3(5.8f, 0.15f, 0.15f), Vector3.zero, p.Timber, false);
    }

    private static void BuildMarket(Transform parent, Palette p)
    {
        GameObject market = CreateEmpty("MERCADO", parent);
        market.transform.localPosition = new Vector3(49f, 3.5f, 5.5f);
        CreateReferenceCube("MERCADO_REF", market.transform, new Vector3(10f, 5f, 4f));

        GameObject art = CreateEmpty("ART_Blockout", market.transform);
        CreateCube("Counter", art.transform, new Vector3(0f, -1.55f, -1.15f), new Vector3(6.5f, 1.1f, 1.1f), Vector3.zero, p.MarketWood, false);
        CreateCube("Back_Table", art.transform, new Vector3(0f, -1.7f, 0.7f), new Vector3(5.8f, 0.8f, 1.2f), Vector3.zero, p.MarketWood, false);
        CreateCube("Post_Left", art.transform, new Vector3(-3.35f, -0.1f, -0.7f), new Vector3(0.25f, 3.8f, 0.25f), Vector3.zero, p.Timber, false);
        CreateCube("Post_Right", art.transform, new Vector3(3.35f, -0.1f, -0.7f), new Vector3(0.25f, 3.8f, 0.25f), Vector3.zero, p.Timber, false);
        CreateCube("Canopy", art.transform, new Vector3(0f, 1.65f, -0.25f), new Vector3(7.6f, 0.25f, 3.2f), new Vector3(0f, 0f, -4f), p.MarketCloth, false);
        CreateCube("Crate_01", art.transform, new Vector3(-3.9f, -1.75f, -1.25f), new Vector3(1.1f, 1f, 1f), Vector3.zero, p.MarketWood, false);
        CreateCube("Crate_02", art.transform, new Vector3(3.85f, -1.8f, -1f), new Vector3(0.9f, 0.9f, 0.9f), new Vector3(0f, 12f, 0f), p.MarketWood, false);
    }

    private static void BuildBridge(Transform parent, Palette p)
    {
        GameObject bridge = CreateEmpty("PUENTE", parent);
        bridge.transform.localPosition = new Vector3(81f, 0.5f, 7f);
        CreateReferenceCube("PUENTE_REF", bridge.transform, new Vector3(14f, 4f, 6f));

        GameObject art = CreateEmpty("ART_Blockout", bridge.transform);
        CreateCube("Deck", art.transform, new Vector3(0f, 1.4f, 0f), new Vector3(14f, 0.55f, 5f), Vector3.zero, p.Stone, false);
        CreateCube("Pier_Left", art.transform, new Vector3(-5.1f, -0.2f, 0f), new Vector3(2.1f, 3.2f, 5f), Vector3.zero, p.Stone, false);
        CreateCube("Pier_Right", art.transform, new Vector3(5.1f, -0.2f, 0f), new Vector3(2.1f, 3.2f, 5f), Vector3.zero, p.Stone, false);
        CreateCube("Arch_Header", art.transform, new Vector3(0f, 0.65f, 0f), new Vector3(8.1f, 0.8f, 5f), Vector3.zero, p.Stone, false);
        CreateCube("Rail_Front", art.transform, new Vector3(0f, 2.05f, -2.35f), new Vector3(14f, 0.3f, 0.3f), Vector3.zero, p.Stone, false);
        CreateCube("Rail_Back", art.transform, new Vector3(0f, 2.05f, 2.35f), new Vector3(14f, 0.3f, 0.3f), Vector3.zero, p.Stone, false);
    }

    private static void BuildFaroles(Transform parent, Transform lightingParent, Palette p)
    {
        GameObject group = CreateEmpty("FAROLES", parent);
        float[] xs = { 8f, 30f, 55f, 68f, 92f };
        float[] ys = { 0f, 1.5f, 1.5f, 3f, 3f };

        for (int i = 0; i < xs.Length; i++)
        {
            GameObject anchor = CreateEmpty("FAROL-0" + (i + 1), group.transform);
            anchor.transform.localPosition = new Vector3(xs[i], ys[i], 2.8f);
            CreateCylinder("Pole", anchor.transform, new Vector3(0f, 1.55f, 0f), new Vector3(0.12f, 1.55f, 0.12f), Vector3.zero, p.Metal, false);
            CreateCube("Arm", anchor.transform, new Vector3(-0.35f, 2.85f, -0.02f), new Vector3(0.8f, 0.12f, 0.12f), new Vector3(0f, 0f, 10f), p.Metal, false);
            CreateCube("Lamp", anchor.transform, new Vector3(-0.7f, 2.55f, -0.02f), new Vector3(0.45f, 0.6f, 0.45f), new Vector3(0f, 45f, 0f), p.LampGlow, false);

            GameObject lightObject = CreateEmpty("LampLight_0" + (i + 1), lightingParent);
            lightObject.transform.position = anchor.transform.TransformPoint(new Vector3(-0.7f, 2.55f, -0.02f));
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.72f, 0.38f);
            light.intensity = 1.6f;
            light.range = 5.5f;
            light.shadows = LightShadows.Soft;
        }
    }

    private static void BuildScenicBackground(Transform parent, Palette p)
    {
        GameObject scenic = CreateEmpty("SCENIC_Background", parent);
        CreateTree("Tree_Bridge", scenic.transform, new Vector3(90f, 3f, 11f), 1.25f, p);
        CreateTree("Tree_Market", scenic.transform, new Vector3(57f, 2.7f, 10.5f), 0.85f, p);
        CreateTree("Tree_Entrance", scenic.transform, new Vector3(2f, 1.5f, 9.5f), 0.95f, p);
        CreateDistantHouse("DistantHouse_01", scenic.transform, new Vector3(4f, 5.5f, 14f), 0.8f, p);
        CreateDistantHouse("DistantHouse_02", scenic.transform, new Vector3(63f, 6.7f, 15f), 1f, p);
        CreateDistantHouse("DistantHouse_03", scenic.transform, new Vector3(99f, 6.2f, 14f), 0.9f, p);
        CreateCube("StoneWall_Entrance", scenic.transform, new Vector3(7f, 0.7f, 4.4f), new Vector3(7f, 1.4f, 0.8f), Vector3.zero, p.Stone, false);
        CreateCube("StoneWall_Market", scenic.transform, new Vector3(58f, 2.3f, 4.6f), new Vector3(5f, 1.2f, 0.8f), Vector3.zero, p.Stone, false);
    }

    private static void BuildForeground(Transform parent, Palette p)
    {
        GameObject scenic = CreateEmpty("SCENIC_Foreground", parent);
        CreateSphere("Bush_FG_01", scenic.transform, new Vector3(18f, 0.45f, -4.8f), new Vector3(1.8f, 0.9f, 0.9f), p.Foreground, false);
        CreateSphere("Bush_FG_02", scenic.transform, new Vector3(52f, 1.9f, -5f), new Vector3(2.2f, 1.1f, 1f), p.Foreground, false);
        CreateSphere("Bush_FG_03", scenic.transform, new Vector3(87f, 3.45f, -5.2f), new Vector3(2f, 1f, 1f), p.Foreground, false);
        CreateCube("Rock_FG_01", scenic.transform, new Vector3(34f, 1.7f, -4.6f), new Vector3(1.5f, 0.9f, 1f), new Vector3(7f, 20f, 10f), p.Stone, false);
        CreateCube("Rock_FG_02", scenic.transform, new Vector3(74f, 3.35f, -4.7f), new Vector3(1.8f, 1f, 1.2f), new Vector3(-8f, -18f, 5f), p.Stone, false);
    }

    private static void BuildLighting(Transform parent)
    {
        GameObject directionalObject = CreateEmpty("Directional Light", parent);
        directionalObject.transform.localRotation = Quaternion.Euler(42f, -32f, 0f);
        Light directional = directionalObject.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(1f, 0.91f, 0.78f);
        directional.intensity = 1.15f;
        directional.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.24f, 0.28f, 0.35f);
        RenderSettings.ambientEquatorColor = new Color(0.18f, 0.17f, 0.18f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.07f, 0.08f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.16f, 0.18f, 0.22f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 95f;
    }

    private static void BuildCamera(Transform parent, Transform target)
    {
        GameObject cameraObject = CreateEmpty("Main Camera", parent);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(4f, 3.9f, -14f);
        cameraObject.transform.rotation = Quaternion.Euler(8f, 0f, 0f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 35f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 250f;

        cameraObject.AddComponent<AudioListener>();
        CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();

        SerializedObject serialized = new SerializedObject(follow);
        serialized.FindProperty("target").objectReferenceValue = target;
        serialized.FindProperty("smoothTime").floatValue = 0.15f;
        serialized.FindProperty("followVertical").boolValue = false;
        serialized.FindProperty("lookAheadDistance").floatValue = 2f;
        serialized.FindProperty("lookAheadSmoothTime").floatValue = 0.2f;
        serialized.FindProperty("minX").floatValue = 0f;
        serialized.FindProperty("maxX").floatValue = 96f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateMarker(string name, Transform parent, Vector3 position, bool trigger, Vector3 size)
    {
        GameObject marker = CreateEmpty(name, parent);
        marker.transform.localPosition = position;
        if (trigger)
        {
            BoxCollider collider = marker.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
        }
        return marker;
    }

    private static GameObject CreateReferenceCube(string name, Transform parent, Vector3 size)
    {
        GameObject reference = CreateCube(name, parent, Vector3.zero, size, Vector3.zero, null, false);
        Renderer renderer = reference.GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
        reference.SetActive(false);
        return reference;
    }

    private static GameObject CreateBeam(string name, Transform parent, Vector3 position, Vector3 scale, float zRotation, Material material)
    {
        return CreateCube(name, parent, position, scale, new Vector3(0f, 0f, zRotation), material, false);
    }

    private static void CreateTree(string name, Transform parent, Vector3 position, float scale, Palette p)
    {
        GameObject tree = CreateEmpty(name, parent);
        tree.transform.localPosition = position;
        tree.transform.localScale = Vector3.one * scale;
        CreateCylinder("Trunk", tree.transform, new Vector3(0f, 2f, 0f), new Vector3(0.45f, 2f, 0.45f), Vector3.zero, p.Trunk, false);
        CreateSphere("Canopy_01", tree.transform, new Vector3(0f, 5f, 0f), new Vector3(3.2f, 2.4f, 2.3f), p.Foliage, false);
        CreateSphere("Canopy_02", tree.transform, new Vector3(-1.4f, 4.4f, 0.2f), new Vector3(2.2f, 1.8f, 1.8f), p.Foliage, false);
        CreateSphere("Canopy_03", tree.transform, new Vector3(1.4f, 4.6f, -0.1f), new Vector3(2.2f, 1.9f, 1.8f), p.Foliage, false);
    }

    private static void CreateDistantHouse(string name, Transform parent, Vector3 position, float scale, Palette p)
    {
        GameObject house = CreateEmpty(name, parent);
        house.transform.localPosition = position;
        house.transform.localScale = Vector3.one * scale;
        CreateCube("Body", house.transform, Vector3.zero, new Vector3(6f, 5f, 4f), Vector3.zero, p.Plaster, false);
        CreateCube("Roof_L", house.transform, new Vector3(-1.5f, 3.1f, 0f), new Vector3(4f, 0.35f, 4.8f), new Vector3(0f, 0f, 35f), p.Roof, false);
        CreateCube("Roof_R", house.transform, new Vector3(1.5f, 3.1f, 0f), new Vector3(4f, 0.35f, 4.8f), new Vector3(0f, 0f, -35f), p.Roof, false);
    }

    private static GameObject CreateEmpty(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Vector3 localEuler, Material material, bool keepCollider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler);
        go.transform.localScale = localScale;
        ApplyMaterial(go, material);
        RemoveColliderIfNeeded(go, keepCollider);
        return go;
    }

    private static GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Vector3 localEuler, Material material, bool keepCollider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(localEuler);
        go.transform.localScale = localScale;
        ApplyMaterial(go, material);
        RemoveColliderIfNeeded(go, keepCollider);
        return go;
    }

    private static GameObject CreateSphere(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, bool keepCollider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        ApplyMaterial(go, material);
        RemoveColliderIfNeeded(go, keepCollider);
        return go;
    }

    private static void ApplyMaterial(GameObject go, Material material)
    {
        if (material == null) return;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
    }

    private static void RemoveColliderIfNeeded(GameObject go, bool keepCollider)
    {
        if (keepCollider) return;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
    }

    private static Palette BuildPalette()
    {
        return new Palette
        {
            Ground = CreateOrUpdateMaterial("MAT_Ground", new Color(0.30f, 0.29f, 0.27f), 0f, 0.22f),
            Ramp = CreateOrUpdateMaterial("MAT_Ramp", new Color(0.34f, 0.32f, 0.29f), 0f, 0.18f),
            Plaster = CreateOrUpdateMaterial("MAT_Plaster", new Color(0.72f, 0.66f, 0.54f), 0f, 0.15f),
            Stone = CreateOrUpdateMaterial("MAT_Stone", new Color(0.36f, 0.38f, 0.40f), 0f, 0.18f),
            Timber = CreateOrUpdateMaterial("MAT_Timber", new Color(0.18f, 0.10f, 0.06f), 0f, 0.12f),
            Roof = CreateOrUpdateMaterial("MAT_Roof", new Color(0.20f, 0.19f, 0.20f), 0f, 0.18f),
            Door = CreateOrUpdateMaterial("MAT_Door", new Color(0.24f, 0.12f, 0.06f), 0f, 0.12f),
            Window = CreateOrUpdateMaterial("MAT_Window", new Color(0.18f, 0.31f, 0.38f), 0.05f, 0.55f),
            MarketCloth = CreateOrUpdateMaterial("MAT_MarketCloth", new Color(0.42f, 0.12f, 0.10f), 0f, 0.25f),
            MarketWood = CreateOrUpdateMaterial("MAT_MarketWood", new Color(0.31f, 0.18f, 0.09f), 0f, 0.15f),
            Foliage = CreateOrUpdateMaterial("MAT_Foliage", new Color(0.18f, 0.28f, 0.18f), 0f, 0.08f),
            Trunk = CreateOrUpdateMaterial("MAT_Trunk", new Color(0.20f, 0.12f, 0.07f), 0f, 0.08f),
            Metal = CreateOrUpdateMaterial("MAT_Metal", new Color(0.12f, 0.13f, 0.15f), 0.65f, 0.35f),
            LampGlow = CreateOrUpdateMaterial("MAT_LampGlow", new Color(0.92f, 0.58f, 0.22f), 0f, 0.48f),
            Crystal = CreateOrUpdateMaterial("MAT_Crystal", new Color(0.18f, 0.65f, 0.78f), 0.2f, 0.7f),
            Foreground = CreateOrUpdateMaterial("MAT_Foreground", new Color(0.10f, 0.16f, 0.11f), 0f, 0.06f)
        };
    }

    private static Material CreateOrUpdateMaterial(string name, Color color, float metallic, float smoothness)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        if (material == null)
        {
            material = new Material(shader);
            material.name = name;
            AssetDatabase.CreateAsset(material, path);
        }
        else if (shader != null && material.shader != shader)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);

        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
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

    private static void EnsureSceneInBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == scenePath)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
