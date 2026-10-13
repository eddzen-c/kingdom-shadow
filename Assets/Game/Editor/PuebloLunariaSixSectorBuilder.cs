#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Genera la versión 2.5D de Pueblo de Lunaria: seis sectores conectados,
/// 600 metros de recorrido lateral y geometría 3D preparada para reemplazarse
/// posteriormente por modelos finales de Blender.
/// </summary>
public static class PuebloLunariaSixSectorBuilder
{
    private const string ScenePath = "Assets/Game/Scenes/Levels/PuebloLunaria_600m.unity";
    private const string MaterialFolder = "Assets/Game/Art/Enviroment/PuebloLunaria/Generated/Materials";

    private sealed class Palette
    {
        public Material ground;
        public Material ramp;
        public Material plaster;
        public Material stone;
        public Material timber;
        public Material roof;
        public Material door;
        public Material window;
        public Material cloth;
        public Material foliage;
        public Material trunk;
        public Material metal;
        public Material lamp;
        public Material crystal;
        public Material foreground;
        public Material hazard;
        public Material training;
    }

    [MenuItem("Tools/Kingdom Shadow/Build Pueblo de Lunaria 600m (6 sectores)")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder("Assets/Game/Scenes/Levels");
        EnsureFolder(MaterialFolder);

        Palette palette = LoadPalette();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = Empty("SCN_PuebloLunaria_600m", null);
        GameObject sectors = Empty("SECTORES_600M", root.transform);

        BuildEntranceSector(sectors.transform, 0f, palette);
        BuildHousingSector(sectors.transform, 80f, palette);
        BuildMarketSector(sectors.transform, 190f, palette);
        BuildTrainingSector(sectors.transform, 290f, palette);
        BuildBridgeSector(sectors.transform, 380f, palette);
        BuildArenaSector(sectors.transform, 520f, palette);

        BuildContinuousBackdrop(root.transform, palette);
        GameObject player = BuildPlayer(root.transform, palette);
        BuildCamera(root.transform, player.transform);
        BuildLighting(root.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EnsureSceneInBuildSettings(ScenePath);
        Selection.activeGameObject = root;

        Debug.Log("Pueblo de Lunaria 600m generado en: " + ScenePath);
        EditorUtility.DisplayDialog(
            "Pueblo de Lunaria listo",
            "Se creó una escena 2.5D de 600 m con seis sectores, cámara lateral, colisiones, arquitectura modular y arena del Lobo Alfa.\n\n" +
            "Escena: " + ScenePath,
            "Abrir escena");
    }

    [MenuItem("Tools/Kingdom Shadow/Open Pueblo de Lunaria 600m")]
    public static void OpenGeneratedScene()
    {
        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("Escena no generada", "Ejecuta primero Build Pueblo de Lunaria 600m.", "Aceptar");
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static void BuildEntranceSector(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S01_Entrada_del_Pueblo_080m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform markers = Empty("04_Marcadores", sector).transform;

        GameplayCube("E-G01_Camino_Bajo", geo, new Vector3(12f, -0.5f, 0f), new Vector3(24f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "E-S", 24.5f, 0f, 8, 1f, 0.25f, 6f, p.stone);
        GameplayCube("E-G02_Plataforma_Porton", geo, new Vector3(44f, 3.5f, 0f), new Vector3(24f, 1f, 6f), 0f, p.ground);
        GameplayCube("E-D01_Descenso", geo, new Vector3(58f, 2.75f, 0f), new Vector3(4f, 0.5f, 6f), -14.04f, p.ramp);
        GameplayCube("E-G03_Camino_Salida", geo, new Vector3(70f, 1.5f, 0f), new Vector3(20f, 1f, 6f), 0f, p.ground);

        BuildGate(architecture, new Vector3(43f, 4f, 7.5f), p);
        BuildHouse(architecture, "Casa_Guardia", new Vector3(63f, 2f, 8f), 10f, 8f, 7f, false, p);
        BuildWall(architecture, new Vector3(19f, 2.2f, 7.5f), 26f, 4.5f, p);
        BuildWall(architecture, new Vector3(70f, 2.2f, 7.5f), 16f, 4.5f, p);

        BuildLantern(decoration, new Vector3(8f, 0f, 3.7f), p);
        BuildLantern(decoration, new Vector3(33f, 4f, 3.7f), p);
        BuildLantern(decoration, new Vector3(54f, 4f, 3.7f), p);
        BuildTree(decoration, new Vector3(15f, 0f, 9f), 1.2f, p);
        BuildTree(decoration, new Vector3(74f, 2f, 10f), 1.35f, p);
        BuildFence(decoration, new Vector3(8f, 0f, 3.3f), 13f, p);
        BuildSign(decoration, new Vector3(5f, 0f, 2.8f), "Cartel_Lunaria", p);

        Marker("Inicio_Leo_X004", markers, new Vector3(4f, 0.7f, 0f), p.crystal);
        Marker("Checkpoint_Entrada_X044", markers, new Vector3(44f, 4.7f, 0f), p.crystal);
    }

    private static void BuildHousingSector(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S02_Zona_de_Viviendas_110m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform markers = Empty("04_Marcadores", sector).transform;

        GameplayCube("V-G01_Calle_Baja", geo, new Vector3(12f, 1.5f, 0f), new Vector3(24f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "V-S01", 24.5f, 2f, 6, 1f, 0.25f, 6f, p.stone);
        GameplayCube("V-G02_Calle_Alta", geo, new Vector3(42f, 4.5f, 0f), new Vector3(24f, 1f, 6f), 0f, p.ground);
        GameplayCube("V-R01_Pendiente", geo, new Vector3(60f, 2.75f, 0f), new Vector3(12f, 0.6f, 6f), -19.29f, p.ramp);
        GameplayCube("V-G03_Plazoleta_Baja", geo, new Vector3(75f, 0.5f, 0f), new Vector3(18f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "V-S02", 84.5f, 0f, 8, 1f, 0.25f, 6f, p.stone);
        GameplayCube("V-G04_Calle_Final", geo, new Vector3(103f, 3.5f, 0f), new Vector3(14f, 1f, 6f), 0f, p.ground);

        BuildHouse(architecture, "Casa_01_Azul", new Vector3(8f, 2f, 8.5f), 9f, 8f, 7f, true, p);
        BuildHouse(architecture, "Casa_02_Torre", new Vector3(21f, 2f, 9f), 8f, 10f, 7f, false, p);
        BuildHouse(architecture, "Casa_03_Balcon", new Vector3(38f, 5f, 9f), 11f, 9f, 7f, true, p);
        BuildHouse(architecture, "Casa_04_Estrecha", new Vector3(51f, 5f, 8.5f), 7f, 11f, 6f, false, p);
        BuildHouse(architecture, "Casa_05_Baja", new Vector3(70f, 1f, 9f), 12f, 7f, 7f, true, p);
        BuildHouse(architecture, "Casa_06_Esquina", new Vector3(94f, 4f, 8.5f), 10f, 9f, 7f, false, p);
        BuildHouse(architecture, "Casa_07_Salida", new Vector3(105f, 4f, 9f), 8f, 8f, 7f, true, p);

        for (int i = 0; i < 6; i++)
            BuildLantern(decoration, new Vector3(8f + i * 18f, SurfaceYHousing(8f + i * 18f), 3.5f), p);

        BuildTree(decoration, new Vector3(31f, 5f, 11f), 1f, p);
        BuildTree(decoration, new Vector3(79f, 1f, 10f), 1.25f, p);
        BuildWell(decoration, new Vector3(75f, 1f, 4.6f), p);
        Marker("Checkpoint_Viviendas_X155", markers, new Vector3(75f, 1.7f, 0f), p.crystal);
    }

    private static void BuildMarketSector(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S03_Plaza_y_Mercado_100m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform markers = Empty("04_Marcadores", sector).transform;

        GameplayCube("M-G01_Entrada", geo, new Vector3(10f, 1.5f, 0f), new Vector3(20f, 1f, 6f), 0f, p.ground);
        GameplayCube("M-R01_Bajada", geo, new Vector3(24f, 0.75f, 0f), new Vector3(8.25f, 0.5f, 6f), -14.04f, p.ramp);
        GameplayCube("M-G02_Plaza", geo, new Vector3(50f, -0.5f, 0f), new Vector3(44f, 1f, 10f), 0f, p.ground);
        GameplayCube("M-R02_Subida", geo, new Vector3(76f, 0.75f, 0f), new Vector3(8.25f, 0.5f, 6f), 14.04f, p.ramp);
        GameplayCube("M-G03_Salida", geo, new Vector3(90f, 1.5f, 0f), new Vector3(20f, 1f, 6f), 0f, p.ground);
        GameplayCube("M-P01_Fuente_Plataforma", geo, new Vector3(50f, 1f, 5f), new Vector3(8f, 2f, 3f), 0f, p.stone);

        BuildHouse(architecture, "Posada", new Vector3(12f, 2f, 10f), 13f, 10f, 8f, true, p);
        BuildHouse(architecture, "Comercio", new Vector3(88f, 2f, 10f), 13f, 9f, 8f, true, p);
        BuildMarketStall(architecture, new Vector3(34f, 0f, 6.8f), false, p);
        BuildMarketStall(architecture, new Vector3(44f, 0f, 7.4f), true, p);
        BuildMarketStall(architecture, new Vector3(59f, 0f, 7.2f), false, p);
        BuildMarketStall(architecture, new Vector3(68f, 0f, 6.8f), true, p);
        BuildFountain(decoration, new Vector3(50f, 2f, 5f), p);

        BuildLantern(decoration, new Vector3(27f, 0f, 4f), p);
        BuildLantern(decoration, new Vector3(73f, 0f, 4f), p);
        BuildCrates(decoration, new Vector3(38f, 0f, 3.8f), p);
        BuildCrates(decoration, new Vector3(64f, 0f, 3.8f), p);
        Marker("Checkpoint_Mercado_X240", markers, new Vector3(50f, 0.7f, 0f), p.crystal);
    }

    private static void BuildTrainingSector(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S04_Zona_de_Entrenamiento_090m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform markers = Empty("04_Marcadores", sector).transform;

        GameplayCube("T-G01_Patio", geo, new Vector3(45f, -0.5f, 0f), new Vector3(90f, 1f, 8f), 0f, p.ground);
        GameplayCube("T-P01_Plataforma_Baja", geo, new Vector3(27f, 0.75f, 0f), new Vector3(8f, 1.5f, 5f), 0f, p.training);
        GameplayCube("T-P02_Plataforma_Alta", geo, new Vector3(43f, 1.5f, 0f), new Vector3(8f, 3f, 5f), 0f, p.training);
        GameplayCube("T-P03_Plataforma_Baja", geo, new Vector3(59f, 0.75f, 0f), new Vector3(8f, 1.5f, 5f), 0f, p.training);
        GameplayCube("T-P04_Muro_Salto", geo, new Vector3(76f, 1.25f, 0f), new Vector3(10f, 2.5f, 5f), 0f, p.training);

        BuildTrainingShelter(architecture, new Vector3(12f, 0f, 8f), p);
        BuildWatchTower(architecture, new Vector3(84f, 0f, 9f), p);
        BuildFence(decoration, new Vector3(45f, 0f, 5f), 80f, p);

        float[] dummyX = { 20f, 36f, 52f, 68f };
        foreach (float x in dummyX)
            BuildTrainingDummy(decoration, new Vector3(x, 0f, 3.8f), p);

        BuildWeaponRack(decoration, new Vector3(14f, 0f, 3.7f), p);
        BuildLantern(decoration, new Vector3(5f, 0f, 4.3f), p);
        BuildLantern(decoration, new Vector3(85f, 0f, 4.3f), p);
        Marker("Checkpoint_Entrenamiento_X335", markers, new Vector3(45f, 0.7f, 0f), p.crystal);
    }

    private static void BuildBridgeSector(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S05_Puente_y_Salida_140m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform hazards = Empty("04_Peligros", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("B-G01_Aproximacion", geo, new Vector3(10f, -0.5f, 0f), new Vector3(20f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "B-S01", 20.5f, 0f, 8, 1f, 0.25f, 6f, p.stone);
        GameplayCube("B-P01_Puente", geo, new Vector3(68f, 3.6f, 0f), new Vector3(80f, 0.8f, 5f), 0f, p.stone);
        GameplayCube("B-R01_Descenso", geo, new Vector3(112f, 2f, 0f), new Vector3(8.94f, 0.6f, 6f), -26.57f, p.ramp);
        GameplayCube("B-G02_Salida", geo, new Vector3(128f, -0.5f, 0f), new Vector3(24f, 1f, 6f), 0f, p.ground);

        BuildBridgeArchitecture(architecture, p);
        BuildGatehouse(architecture, new Vector3(132f, 0f, 8f), p);
        BuildMountainCliffs(decoration, p);
        BuildLantern(decoration, new Vector3(30f, 4f, 3.3f), p);
        BuildLantern(decoration, new Vector3(68f, 4f, 3.3f), p);
        BuildLantern(decoration, new Vector3(106f, 4f, 3.3f), p);

        GameObject abyss = Primitive("B-H01_Abismo_Trigger", PrimitiveType.Cube, hazards,
            new Vector3(68f, -7f, 0f), new Vector3(80f, 8f, 8f), Quaternion.identity, p.hazard, false);
        BoxCollider abyssCollider = abyss.GetComponent<BoxCollider>();
        abyssCollider.isTrigger = true;
        abyss.GetComponent<Renderer>().enabled = false;

        Marker("Checkpoint_Puente_X448", markers, new Vector3(68f, 4.7f, 0f), p.crystal);
    }

    private static void BuildArenaSector(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S06_Arena_del_Lobo_Alfa_080m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform gameplay = Empty("04_Jefe_y_Cristal", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("A-G01_Entrada", geo, new Vector3(5f, -0.5f, 0f), new Vector3(10f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "A-S01", 10.75f, 0f, 4, 1.5f, 0.25f, 6f, p.stone);
        GameplayCube("A-G02_Arena", geo, new Vector3(44f, 1.5f, 0f), new Vector3(58f, 1f, 12f), 0f, p.ground);
        GameplayCube("A-G03_Pedestal", geo, new Vector3(72f, 3f, 0f), new Vector3(4f, 4f, 4f), 0f, p.stone);

        BuildArenaRuins(architecture, p);
        BuildBrokenTrees(decoration, p);
        BuildWolfBoss(gameplay, new Vector3(43f, 3f, 0f), p);
        BuildCrystal(gameplay, new Vector3(72f, 6f, 0f), p);

        Marker("Entrada_Arena_X530", markers, new Vector3(10f, 0.7f, 0f), p.crystal);
        Marker("Centro_Combate_X563", markers, new Vector3(43f, 2.7f, 0f), p.hazard);
        Marker("Cristal_de_Luz_X592", markers, new Vector3(72f, 5.2f, 0f), p.crystal);
    }

    private static void BuildContinuousBackdrop(Transform parent, Palette p)
    {
        Transform back = Empty("FONDO_CONTINUO", parent).transform;
        Primitive("Montanas_Lejanas", PrimitiveType.Cube, back, new Vector3(300f, 14f, 22f),
            new Vector3(640f, 34f, 2f), Quaternion.identity, p.foreground, false);
        Primitive("Niebla_Baja", PrimitiveType.Cube, back, new Vector3(300f, -5f, 14f),
            new Vector3(640f, 8f, 2f), Quaternion.identity, p.ramp, false);

        for (int i = 0; i < 28; i++)
        {
            float x = 8f + i * 22f;
            float scale = 0.7f + (i % 4) * 0.12f;
            BuildTree(back, new Vector3(x, -1f + (i % 3), 12f + (i % 2) * 2f), scale, p);
        }

        Transform foreground = Empty("Primer_Plano", parent).transform;
        for (int i = 0; i < 20; i++)
        {
            float x = 12f + i * 31f;
            Primitive("Silueta_" + i.ToString("00"), PrimitiveType.Cube, foreground,
                new Vector3(x, -1.4f, -4f), new Vector3(8f + i % 3, 2.8f, 1.2f),
                Quaternion.Euler(0f, 0f, (i % 2 == 0 ? -5f : 5f)), p.foreground, false);
        }
    }

    private static GameObject BuildPlayer(Transform parent, Palette p)
    {
        GameObject player = Empty("PLAYER_Leo", parent);
        player.transform.position = new Vector3(4f, 1.3f, 0f);
        player.tag = "Player";

        Rigidbody rigidbody = player.AddComponent<Rigidbody>();
        rigidbody.mass = 1f;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY;

        CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
        collider.height = 1.8f;
        collider.radius = 0.35f;
        collider.center = new Vector3(0f, 0.9f, 0f);

        Type controllerType = FindType("PlayerBlockoutController");
        if (controllerType != null)
            player.AddComponent(controllerType);

        Transform visual = Empty("Visual_Blockout", player.transform).transform;
        Primitive("Cuerpo", PrimitiveType.Capsule, visual, new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.5f), Quaternion.identity, p.plaster, false);
        Primitive("Cabeza", PrimitiveType.Sphere, visual, new Vector3(0f, 1.9f, 0f), new Vector3(0.55f, 0.55f, 0.55f), Quaternion.identity, p.window, false);
        Primitive("Espada", PrimitiveType.Cube, visual, new Vector3(0.5f, 0.95f, 0f), new Vector3(0.12f, 1.2f, 0.12f), Quaternion.Euler(0f, 0f, -18f), p.metal, false);
        return player;
    }

    private static void BuildCamera(Transform parent, Transform target)
    {
        GameObject cameraObject = new GameObject("CAM_Main_2_5D", typeof(Camera), typeof(AudioListener));
        cameraObject.transform.SetParent(parent, false);
        cameraObject.transform.position = new Vector3(4f, 6.2f, -16f);
        cameraObject.transform.rotation = Quaternion.Euler(7f, 0f, 0f);
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = false;
        camera.fieldOfView = 35f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 180f;
        camera.backgroundColor = new Color(0.035f, 0.055f, 0.085f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();
        follow.Configure(target, 4f, 596f, true);
    }

    private static void BuildLighting(Transform parent)
    {
        GameObject lighting = Empty("LIGHTING_Lunaria", parent);
        GameObject moon = new GameObject("Luz_Lunar", typeof(Light));
        moon.transform.SetParent(lighting.transform, false);
        moon.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
        Light directional = moon.GetComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(0.62f, 0.72f, 1f);
        directional.intensity = 0.8f;
        directional.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.11f, 0.15f, 0.23f);
        RenderSettings.ambientEquatorColor = new Color(0.08f, 0.09f, 0.13f);
        RenderSettings.ambientGroundColor = new Color(0.025f, 0.025f, 0.035f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.08f, 0.10f, 0.15f);
        RenderSettings.fogStartDistance = 45f;
        RenderSettings.fogEndDistance = 125f;
    }

    private static void BuildGate(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Porton_Principal", parent).transform;
        root.localPosition = position;
        Primitive("Torre_Izquierda", PrimitiveType.Cube, root, new Vector3(-6f, 4f, 0f), new Vector3(4f, 8f, 5f), Quaternion.identity, p.stone, true);
        Primitive("Torre_Derecha", PrimitiveType.Cube, root, new Vector3(6f, 4f, 0f), new Vector3(4f, 8f, 5f), Quaternion.identity, p.stone, true);
        Primitive("Dintel", PrimitiveType.Cube, root, new Vector3(0f, 8f, 0f), new Vector3(16f, 2.2f, 5f), Quaternion.identity, p.stone, true);
        Primitive("Puerta_Izquierda", PrimitiveType.Cube, root, new Vector3(-2.2f, 2.8f, -2.1f), new Vector3(4f, 5.6f, 0.35f), Quaternion.identity, p.door, false);
        Primitive("Puerta_Derecha", PrimitiveType.Cube, root, new Vector3(2.2f, 2.8f, -2.1f), new Vector3(4f, 5.6f, 0.35f), Quaternion.identity, p.door, false);
        Primitive("Escudo", PrimitiveType.Sphere, root, new Vector3(0f, 8f, -2.7f), new Vector3(1.3f, 1.3f, 0.35f), Quaternion.identity, p.metal, false);
        BuildLantern(root, new Vector3(-4f, 5.5f, -2.8f), p);
        BuildLantern(root, new Vector3(4f, 5.5f, -2.8f), p);
    }

    private static void BuildHouse(Transform parent, string name, Vector3 basePosition, float width, float height, float depth, bool balcony, Palette p)
    {
        Transform house = Empty(name, parent).transform;
        house.localPosition = basePosition;
        Primitive("Base_Piedra", PrimitiveType.Cube, house, new Vector3(0f, height * 0.18f, 0f), new Vector3(width, height * 0.36f, depth), Quaternion.identity, p.stone, true);
        Primitive("Piso_Enlucido", PrimitiveType.Cube, house, new Vector3(0f, height * 0.62f, 0f), new Vector3(width * 0.92f, height * 0.58f, depth * 0.92f), Quaternion.identity, p.plaster, true);
        Primitive("Tejado", PrimitiveType.Cube, house, new Vector3(0f, height + 0.25f, 0f), new Vector3(width * 1.08f, 0.65f, depth * 1.08f), Quaternion.Euler(0f, 0f, 4f), p.roof, false);
        Primitive("Viga_Horizontal", PrimitiveType.Cube, house, new Vector3(0f, height * 0.62f, -depth * 0.48f), new Vector3(width * 0.95f, 0.22f, 0.18f), Quaternion.identity, p.timber, false);
        Primitive("Viga_Diagonal_I", PrimitiveType.Cube, house, new Vector3(-width * 0.25f, height * 0.65f, -depth * 0.49f), new Vector3(0.22f, height * 0.52f, 0.18f), Quaternion.Euler(0f, 0f, -24f), p.timber, false);
        Primitive("Viga_Diagonal_D", PrimitiveType.Cube, house, new Vector3(width * 0.25f, height * 0.65f, -depth * 0.49f), new Vector3(0.22f, height * 0.52f, 0.18f), Quaternion.Euler(0f, 0f, 24f), p.timber, false);
        Primitive("Puerta", PrimitiveType.Cube, house, new Vector3(-width * 0.22f, 1.25f, -depth * 0.51f), new Vector3(1.4f, 2.5f, 0.22f), Quaternion.identity, p.door, false);
        BuildWindow(house, new Vector3(width * 0.22f, height * 0.58f, -depth * 0.51f), p);

        if (balcony)
        {
            Primitive("Balcon", PrimitiveType.Cube, house, new Vector3(0f, height * 0.55f, -depth * 0.62f), new Vector3(width * 0.55f, 0.25f, 1.5f), Quaternion.identity, p.timber, false);
            Primitive("Baranda", PrimitiveType.Cube, house, new Vector3(0f, height * 0.69f, -depth * 0.78f), new Vector3(width * 0.55f, 1.6f, 0.14f), Quaternion.identity, p.timber, false);
        }

        Primitive("Chimenea", PrimitiveType.Cube, house, new Vector3(width * 0.28f, height + 1.2f, depth * 0.15f), new Vector3(0.9f, 2.6f, 0.9f), Quaternion.identity, p.stone, false);
    }

    private static void BuildWindow(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Ventana", parent).transform;
        root.localPosition = position;
        Primitive("Cristal", PrimitiveType.Cube, root, Vector3.zero, new Vector3(1.6f, 1.7f, 0.12f), Quaternion.identity, p.window, false);
        Primitive("Marco_H", PrimitiveType.Cube, root, Vector3.zero, new Vector3(1.9f, 0.14f, 0.18f), Quaternion.identity, p.timber, false);
        Primitive("Marco_V", PrimitiveType.Cube, root, Vector3.zero, new Vector3(0.14f, 2f, 0.18f), Quaternion.identity, p.timber, false);
    }

    private static void BuildWall(Transform parent, Vector3 position, float length, float height, Palette p)
    {
        Primitive("Muralla", PrimitiveType.Cube, parent, position, new Vector3(length, height, 2.5f), Quaternion.identity, p.stone, true);
        for (float x = -length * 0.5f + 1f; x < length * 0.5f; x += 2f)
            Primitive("Almena", PrimitiveType.Cube, parent, position + new Vector3(x, height * 0.5f + 0.6f, 0f), new Vector3(1.1f, 1.2f, 2.7f), Quaternion.identity, p.stone, false);
    }

    private static void BuildMarketStall(Transform parent, Vector3 position, bool alternate, Palette p)
    {
        Transform stall = Empty("Puesto_Mercado", parent).transform;
        stall.localPosition = position;
        Material fabric = alternate ? p.roof : p.cloth;
        Primitive("Mesa", PrimitiveType.Cube, stall, new Vector3(0f, 1f, 0f), new Vector3(5f, 0.45f, 2.8f), Quaternion.identity, p.timber, true);
        Primitive("Poste_I", PrimitiveType.Cube, stall, new Vector3(-2.1f, 2.4f, 0f), new Vector3(0.18f, 4.8f, 0.18f), Quaternion.identity, p.timber, false);
        Primitive("Poste_D", PrimitiveType.Cube, stall, new Vector3(2.1f, 2.4f, 0f), new Vector3(0.18f, 4.8f, 0.18f), Quaternion.identity, p.timber, false);
        Primitive("Toldo", PrimitiveType.Cube, stall, new Vector3(0f, 4.5f, 0f), new Vector3(5.6f, 0.28f, 3.6f), Quaternion.Euler(0f, 0f, alternate ? 4f : -4f), fabric, false);
        BuildCrates(stall, new Vector3(0f, 0f, -1.7f), p);
    }

    private static void BuildFountain(Transform parent, Vector3 position, Palette p)
    {
        Transform fountain = Empty("Fuente_Lunaria", parent).transform;
        fountain.localPosition = position;
        Primitive("Pileta", PrimitiveType.Cylinder, fountain, Vector3.zero, new Vector3(3.4f, 0.5f, 3.4f), Quaternion.identity, p.stone, false);
        Primitive("Columna", PrimitiveType.Cylinder, fountain, new Vector3(0f, 1.5f, 0f), new Vector3(0.8f, 1.5f, 0.8f), Quaternion.identity, p.stone, false);
        Primitive("Luz", PrimitiveType.Sphere, fountain, new Vector3(0f, 3.1f, 0f), Vector3.one * 0.65f, Quaternion.identity, p.crystal, false);
    }

    private static void BuildTrainingShelter(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Refugio_Entrenador", parent).transform;
        root.localPosition = position;
        Primitive("Pared", PrimitiveType.Cube, root, new Vector3(0f, 3f, 1.5f), new Vector3(13f, 6f, 2f), Quaternion.identity, p.stone, true);
        Primitive("Techo", PrimitiveType.Cube, root, new Vector3(0f, 6.4f, 0f), new Vector3(15f, 0.6f, 6f), Quaternion.Euler(0f, 0f, -4f), p.roof, false);
        Primitive("Banco", PrimitiveType.Cube, root, new Vector3(0f, 0.8f, -1f), new Vector3(7f, 0.4f, 1.5f), Quaternion.identity, p.timber, false);
    }

    private static void BuildWatchTower(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Torre_Vigilancia", parent).transform;
        root.localPosition = position;
        Primitive("Base", PrimitiveType.Cylinder, root, new Vector3(0f, 3f, 0f), new Vector3(4.5f, 3f, 4.5f), Quaternion.identity, p.stone, true);
        Primitive("Puesto", PrimitiveType.Cube, root, new Vector3(0f, 7f, 0f), new Vector3(6f, 3f, 5f), Quaternion.identity, p.timber, true);
        Primitive("Techo", PrimitiveType.Cylinder, root, new Vector3(0f, 9f, 0f), new Vector3(4.2f, 1f, 4.2f), Quaternion.identity, p.roof, false);
    }

    private static void BuildTrainingDummy(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Muneco_Entrenamiento", parent).transform;
        root.localPosition = position;
        Primitive("Poste", PrimitiveType.Cylinder, root, new Vector3(0f, 1.5f, 0f), new Vector3(0.25f, 1.5f, 0.25f), Quaternion.identity, p.timber, false);
        Primitive("Torso", PrimitiveType.Cube, root, new Vector3(0f, 2.7f, 0f), new Vector3(1.1f, 1.4f, 0.5f), Quaternion.identity, p.training, false);
        Primitive("Brazo", PrimitiveType.Cube, root, new Vector3(0f, 3f, 0f), new Vector3(3f, 0.25f, 0.25f), Quaternion.identity, p.timber, false);
        Primitive("Cabeza", PrimitiveType.Sphere, root, new Vector3(0f, 3.8f, 0f), Vector3.one * 0.7f, Quaternion.identity, p.training, false);
    }

    private static void BuildWeaponRack(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Armero", parent).transform;
        root.localPosition = position;
        Primitive("Soporte", PrimitiveType.Cube, root, new Vector3(0f, 1.4f, 0f), new Vector3(4f, 2.8f, 0.4f), Quaternion.identity, p.timber, false);
        for (int i = 0; i < 4; i++)
            Primitive("Espada_" + i, PrimitiveType.Cube, root, new Vector3(-1.5f + i, 1.7f, -0.35f), new Vector3(0.12f, 2.7f, 0.12f), Quaternion.Euler(0f, 0f, -10f + i * 7f), p.metal, false);
    }

    private static void BuildBridgeArchitecture(Transform parent, Palette p)
    {
        Transform bridge = Empty("Puente_de_Piedra", parent).transform;
        for (int i = 0; i < 9; i++)
        {
            float x = 32f + i * 9f;
            Primitive("Soporte_" + i.ToString("00"), PrimitiveType.Cube, bridge, new Vector3(x, -1.5f, 0f), new Vector3(1.4f, 10f, 5.5f), Quaternion.identity, p.stone, true);
            Primitive("Baranda_Fondo_" + i.ToString("00"), PrimitiveType.Cube, bridge, new Vector3(x, 5f, 2.5f), new Vector3(1f, 2.2f, 0.5f), Quaternion.identity, p.stone, false);
            Primitive("Baranda_Frente_" + i.ToString("00"), PrimitiveType.Cube, bridge, new Vector3(x, 5f, -2.5f), new Vector3(1f, 2.2f, 0.5f), Quaternion.identity, p.stone, false);
        }
        Primitive("Pasamanos_Fondo", PrimitiveType.Cube, bridge, new Vector3(68f, 5.8f, 2.5f), new Vector3(80f, 0.35f, 0.5f), Quaternion.identity, p.stone, false);
        Primitive("Pasamanos_Frente", PrimitiveType.Cube, bridge, new Vector3(68f, 5.8f, -2.5f), new Vector3(80f, 0.35f, 0.5f), Quaternion.identity, p.stone, false);
    }

    private static void BuildGatehouse(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Casa_de_Salida", parent).transform;
        root.localPosition = position;
        Primitive("Base", PrimitiveType.Cube, root, new Vector3(0f, 3f, 0f), new Vector3(12f, 6f, 8f), Quaternion.identity, p.stone, true);
        Primitive("Paso", PrimitiveType.Cube, root, new Vector3(0f, 2f, -4.1f), new Vector3(4f, 4f, 0.4f), Quaternion.identity, p.door, false);
        Primitive("Techo", PrimitiveType.Cube, root, new Vector3(0f, 6.8f, 0f), new Vector3(13f, 0.8f, 9f), Quaternion.Euler(0f, 0f, 5f), p.roof, false);
    }

    private static void BuildMountainCliffs(Transform parent, Palette p)
    {
        for (int i = 0; i < 10; i++)
        {
            float x = 22f + i * 12f;
            float h = 7f + (i % 4) * 2.5f;
            Primitive("Risco_" + i.ToString("00"), PrimitiveType.Cube, parent, new Vector3(x, -3f, 9f), new Vector3(10f, h, 6f), Quaternion.Euler(0f, 0f, -8f + (i % 3) * 7f), p.foreground, false);
        }
    }

    private static void BuildArenaRuins(Transform parent, Palette p)
    {
        Primitive("Muro_Fondo", PrimitiveType.Cube, parent, new Vector3(44f, 6f, 9f), new Vector3(62f, 12f, 3f), Quaternion.identity, p.foreground, false);
        float[] pillarX = { 17f, 28f, 44f, 60f, 70f };
        for (int i = 0; i < pillarX.Length; i++)
        {
            float height = 6f + (i % 3) * 1.8f;
            Primitive("Columna_Rota_" + i, PrimitiveType.Cylinder, parent, new Vector3(pillarX[i], 2f + height * 0.5f, 6f), new Vector3(1.4f, height * 0.5f, 1.4f), Quaternion.Euler(0f, 0f, i % 2 == 0 ? -3f : 4f), p.stone, false);
        }
        Primitive("Arco_Izquierdo", PrimitiveType.Cube, parent, new Vector3(18f, 9f, 6f), new Vector3(14f, 2f, 3f), Quaternion.Euler(0f, 0f, -8f), p.stone, false);
        Primitive("Arco_Derecho", PrimitiveType.Cube, parent, new Vector3(67f, 8f, 6f), new Vector3(12f, 2f, 3f), Quaternion.Euler(0f, 0f, 9f), p.stone, false);
    }

    private static void BuildBrokenTrees(Transform parent, Palette p)
    {
        float[] positions = { 14f, 24f, 57f, 75f };
        for (int i = 0; i < positions.Length; i++)
        {
            Transform tree = Empty("Arbol_Quemado_" + i, parent).transform;
            tree.localPosition = new Vector3(positions[i], 2f, 8f);
            Primitive("Tronco", PrimitiveType.Cylinder, tree, new Vector3(0f, 2.5f, 0f), new Vector3(0.7f, 2.5f, 0.7f), Quaternion.Euler(0f, 0f, -8f + i * 5f), p.trunk, false);
            Primitive("Rama", PrimitiveType.Cylinder, tree, new Vector3(0.8f, 4.2f, 0f), new Vector3(0.25f, 2f, 0.25f), Quaternion.Euler(0f, 0f, -45f), p.trunk, false);
        }
    }

    private static void BuildWolfBoss(Transform parent, Vector3 position, Palette p)
    {
        Transform wolf = Empty("JEFE_Lobo_Alfa_Blockout", parent).transform;
        wolf.localPosition = position;
        Primitive("Cuerpo", PrimitiveType.Capsule, wolf, new Vector3(0f, 1.7f, 0f), new Vector3(2.5f, 2.2f, 1.8f), Quaternion.Euler(0f, 0f, 90f), p.foreground, false);
        Primitive("Pecho", PrimitiveType.Sphere, wolf, new Vector3(1.8f, 2f, 0f), new Vector3(2f, 2.5f, 2f), Quaternion.identity, p.foreground, false);
        Primitive("Cabeza", PrimitiveType.Sphere, wolf, new Vector3(3f, 3.1f, 0f), new Vector3(1.8f, 1.6f, 1.7f), Quaternion.identity, p.foreground, false);
        Primitive("Hocico", PrimitiveType.Cube, wolf, new Vector3(4f, 2.9f, 0f), new Vector3(1.3f, 0.8f, 1.1f), Quaternion.identity, p.foreground, false);
        Primitive("Oreja_I", PrimitiveType.Cube, wolf, new Vector3(2.6f, 4.2f, -0.55f), new Vector3(0.45f, 1.3f, 0.45f), Quaternion.Euler(0f, 0f, -18f), p.foreground, false);
        Primitive("Oreja_D", PrimitiveType.Cube, wolf, new Vector3(2.6f, 4.2f, 0.55f), new Vector3(0.45f, 1.3f, 0.45f), Quaternion.Euler(0f, 0f, -18f), p.foreground, false);
        Primitive("Ojo_Luz", PrimitiveType.Sphere, wolf, new Vector3(3.75f, 3.35f, -0.65f), Vector3.one * 0.28f, Quaternion.identity, p.hazard, false);

        for (int i = 0; i < 4; i++)
        {
            float x = -1f + (i / 2) * 3f;
            float z = i % 2 == 0 ? -0.7f : 0.7f;
            Primitive("Pata_" + i, PrimitiveType.Capsule, wolf, new Vector3(x, 0.6f, z), new Vector3(0.55f, 1.2f, 0.55f), Quaternion.identity, p.foreground, false);
        }
        Primitive("Cola", PrimitiveType.Capsule, wolf, new Vector3(-3f, 2.2f, 0f), new Vector3(0.7f, 2.3f, 0.7f), Quaternion.Euler(0f, 0f, -60f), p.foreground, false);
    }

    private static void BuildCrystal(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Cristal_de_Luz_01", parent).transform;
        root.localPosition = position;
        Primitive("Nucleo", PrimitiveType.Cube, root, Vector3.zero, new Vector3(1.6f, 3.8f, 1.6f), Quaternion.Euler(0f, 45f, 45f), p.crystal, false);
        Primitive("Fragmento_I", PrimitiveType.Cube, root, new Vector3(-1.1f, -0.4f, 0f), new Vector3(0.7f, 2.1f, 0.7f), Quaternion.Euler(0f, 35f, 25f), p.crystal, false);
        Primitive("Fragmento_D", PrimitiveType.Cube, root, new Vector3(1.1f, -0.4f, 0f), new Vector3(0.7f, 2.1f, 0.7f), Quaternion.Euler(0f, -35f, -25f), p.crystal, false);
        GameObject lightObject = new GameObject("Luz_Cristal", typeof(Light));
        lightObject.transform.SetParent(root, false);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.25f, 0.85f, 1f);
        light.intensity = 3f;
        light.range = 13f;
    }

    private static void BuildTree(Transform parent, Vector3 position, float scale, Palette p)
    {
        Transform root = Empty("Arbol", parent).transform;
        root.localPosition = position;
        root.localScale = Vector3.one * scale;
        Primitive("Tronco", PrimitiveType.Cylinder, root, new Vector3(0f, 2.3f, 0f), new Vector3(0.65f, 2.3f, 0.65f), Quaternion.identity, p.trunk, false);
        Primitive("Copa_01", PrimitiveType.Sphere, root, new Vector3(-1.1f, 5.5f, 0f), new Vector3(3.4f, 3.8f, 2.8f), Quaternion.identity, p.foliage, false);
        Primitive("Copa_02", PrimitiveType.Sphere, root, new Vector3(1.2f, 5.6f, 0.2f), new Vector3(3.7f, 4.2f, 3f), Quaternion.identity, p.foliage, false);
        Primitive("Copa_03", PrimitiveType.Sphere, root, new Vector3(0f, 7f, 0f), new Vector3(3.1f, 3.5f, 2.6f), Quaternion.identity, p.foliage, false);
    }

    private static void BuildLantern(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Farol", parent).transform;
        root.localPosition = position;
        Primitive("Poste", PrimitiveType.Cylinder, root, new Vector3(0f, 2f, 0f), new Vector3(0.16f, 2f, 0.16f), Quaternion.identity, p.metal, false);
        Primitive("Lampara", PrimitiveType.Cube, root, new Vector3(0f, 4.1f, 0f), new Vector3(0.7f, 1f, 0.7f), Quaternion.identity, p.lamp, false);
        GameObject lightObject = new GameObject("Luz", typeof(Light));
        lightObject.transform.SetParent(root, false);
        lightObject.transform.localPosition = new Vector3(0f, 4.1f, -0.4f);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.65f, 0.28f);
        light.intensity = 1.8f;
        light.range = 8f;
        light.shadows = LightShadows.Soft;
    }

    private static void BuildFence(Transform parent, Vector3 position, float length, Palette p)
    {
        Transform root = Empty("Cerca", parent).transform;
        root.localPosition = position;
        Primitive("Travesano_A", PrimitiveType.Cube, root, new Vector3(0f, 0.9f, 0f), new Vector3(length, 0.18f, 0.18f), Quaternion.identity, p.timber, false);
        Primitive("Travesano_B", PrimitiveType.Cube, root, new Vector3(0f, 1.6f, 0f), new Vector3(length, 0.18f, 0.18f), Quaternion.identity, p.timber, false);
        for (float x = -length * 0.5f; x <= length * 0.5f; x += 2.5f)
            Primitive("Poste", PrimitiveType.Cube, root, new Vector3(x, 1f, 0f), new Vector3(0.2f, 2f, 0.2f), Quaternion.identity, p.timber, false);
    }

    private static void BuildSign(Transform parent, Vector3 position, string text, Palette p)
    {
        Transform root = Empty(text, parent).transform;
        root.localPosition = position;
        Primitive("Poste", PrimitiveType.Cube, root, new Vector3(0f, 1.2f, 0f), new Vector3(0.25f, 2.4f, 0.25f), Quaternion.identity, p.timber, false);
        Primitive("Tabla", PrimitiveType.Cube, root, new Vector3(0.7f, 2.2f, 0f), new Vector3(2.8f, 1.1f, 0.25f), Quaternion.Euler(0f, 0f, -4f), p.door, false);
    }

    private static void BuildWell(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Pozo", parent).transform;
        root.localPosition = position;
        Primitive("Brocal", PrimitiveType.Cylinder, root, new Vector3(0f, 0.8f, 0f), new Vector3(2.3f, 0.8f, 2.3f), Quaternion.identity, p.stone, false);
        Primitive("Poste_I", PrimitiveType.Cube, root, new Vector3(-1.3f, 2.3f, 0f), new Vector3(0.25f, 3.6f, 0.25f), Quaternion.identity, p.timber, false);
        Primitive("Poste_D", PrimitiveType.Cube, root, new Vector3(1.3f, 2.3f, 0f), new Vector3(0.25f, 3.6f, 0.25f), Quaternion.identity, p.timber, false);
        Primitive("Techo", PrimitiveType.Cube, root, new Vector3(0f, 4.1f, 0f), new Vector3(3.8f, 0.35f, 2.8f), Quaternion.Euler(0f, 0f, 6f), p.roof, false);
    }

    private static void BuildCrates(Transform parent, Vector3 position, Palette p)
    {
        Primitive("Caja_A", PrimitiveType.Cube, parent, position + new Vector3(-0.6f, 0.5f, 0f), Vector3.one, Quaternion.identity, p.timber, false);
        Primitive("Caja_B", PrimitiveType.Cube, parent, position + new Vector3(0.55f, 0.65f, 0f), Vector3.one * 1.3f, Quaternion.Euler(0f, 8f, 0f), p.door, false);
    }

    private static Transform Sector(Transform parent, string name, float offsetX)
    {
        GameObject sector = Empty(name, parent);
        sector.transform.localPosition = new Vector3(offsetX, 0f, 0f);
        return sector.transform;
    }

    private static void BuildStairs(Transform parent, string prefix, float startX, float baseY, int count, float width, float rise, float depth, Material material)
    {
        for (int i = 0; i < count; i++)
        {
            float height = (i + 1) * rise * 2f;
            GameplayCube(prefix + "_Escalon_" + (i + 1).ToString("00"), parent,
                new Vector3(startX + i * width, baseY + height * 0.5f, 0f),
                new Vector3(width, height, depth), 0f, material);
        }
    }

    private static GameObject GameplayCube(string name, Transform parent, Vector3 localPosition, Vector3 scale, float rotationZ, Material material)
    {
        return Primitive(name, PrimitiveType.Cube, parent, localPosition, scale, Quaternion.Euler(0f, 0f, rotationZ), material, true);
    }

    private static GameObject Marker(string name, Transform parent, Vector3 position, Material material)
    {
        GameObject marker = Primitive(name, PrimitiveType.Sphere, parent, position, Vector3.one * 0.35f, Quaternion.identity, material, false);
        Collider collider = marker.GetComponent<Collider>();
        if (collider != null)
            UnityEngine.Object.DestroyImmediate(collider);
        return marker;
    }

    private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 scale, Quaternion rotation, Material material, bool keepCollider)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = rotation;
        go.transform.localScale = scale;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;

        if (!keepCollider)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }
        return go;
    }

    private static GameObject Empty(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent, false);
        return go;
    }

    private static float SurfaceYHousing(float x)
    {
        if (x < 24f) return 2f;
        if (x < 54f) return 5f;
        if (x < 66f) return 3f;
        if (x < 84f) return 1f;
        return 4f;
    }

    private static Type FindType(string typeName)
    {
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }
        return null;
    }

    private static Palette LoadPalette()
    {
        return new Palette
        {
            ground = LoadOrCreateMaterial("MAT_Ground", new Color(0.25f, 0.27f, 0.30f)),
            ramp = LoadOrCreateMaterial("MAT_Ramp", new Color(0.34f, 0.35f, 0.38f)),
            plaster = LoadOrCreateMaterial("MAT_Plaster", new Color(0.73f, 0.70f, 0.61f)),
            stone = LoadOrCreateMaterial("MAT_Stone", new Color(0.36f, 0.39f, 0.43f)),
            timber = LoadOrCreateMaterial("MAT_Timber", new Color(0.19f, 0.12f, 0.09f)),
            roof = LoadOrCreateMaterial("MAT_Roof", new Color(0.19f, 0.24f, 0.32f)),
            door = LoadOrCreateMaterial("MAT_Door", new Color(0.26f, 0.15f, 0.10f)),
            window = LoadOrCreateMaterial("MAT_Window", new Color(0.23f, 0.65f, 0.78f)),
            cloth = LoadOrCreateMaterial("MAT_MarketCloth", new Color(0.48f, 0.12f, 0.18f)),
            foliage = LoadOrCreateMaterial("MAT_Foliage", new Color(0.16f, 0.27f, 0.23f)),
            trunk = LoadOrCreateMaterial("MAT_Trunk", new Color(0.20f, 0.13f, 0.10f)),
            metal = LoadOrCreateMaterial("MAT_Metal", new Color(0.16f, 0.18f, 0.22f)),
            lamp = LoadOrCreateMaterial("MAT_LampGlow", new Color(1f, 0.55f, 0.18f)),
            crystal = LoadOrCreateMaterial("MAT_Crystal", new Color(0.20f, 0.82f, 1f)),
            foreground = LoadOrCreateMaterial("MAT_Foreground", new Color(0.045f, 0.06f, 0.08f)),
            hazard = LoadOrCreateMaterial("MAT_Hazard", new Color(0.70f, 0.06f, 0.10f)),
            training = LoadOrCreateMaterial("MAT_Training", new Color(0.45f, 0.32f, 0.18f))
        };
    }

    private static Material LoadOrCreateMaterial(string name, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader) { name = name, color = color };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        string normalized = path.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(normalized))
            return;

        string[] parts = normalized.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void EnsureSceneInBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        string mainMenu = "Assets/Game/Scenes/MainMenu.unity";
        string bosque600m = "Assets/Game/Scenes/Levels/BosqueEncantado_600m.unity";

        if (File.Exists(mainMenu))
            scenes.Add(new EditorBuildSettingsScene(mainMenu, true));

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));

        if (File.Exists(bosque600m))
            scenes.Add(new EditorBuildSettingsScene(bosque600m, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
