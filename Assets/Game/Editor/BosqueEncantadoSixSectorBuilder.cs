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
/// Genera el Bosque Encantado como nivel 2.5D de 600 metros.
/// La geometría jugable coincide con los planos estructurales y el arte
/// provisional se puede sustituir posteriormente por modelos de Blender.
/// </summary>
public static class BosqueEncantadoSixSectorBuilder
{
    private const string ScenePath = "Assets/Game/Scenes/Levels/BosqueEncantado_600m.unity";
    private const string MaterialFolder = "Assets/Game/Art/Environment/BosqueEncantado/Generated/Materials";

    private sealed class Palette
    {
        public Material ground;
        public Material root;
        public Material bark;
        public Material foliage;
        public Material stone;
        public Material moss;
        public Material shadow;
        public Material crystal;
        public Material trap;
        public Material foreground;
        public Material warmLight;
    }

    [MenuItem("Tools/Kingdom Shadow/Build Bosque Encantado 600m (6 sectores)")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder("Assets/Game/Scenes/Levels");
        EnsureFolder(MaterialFolder);

        Palette palette = LoadPalette();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = Empty("SCN_BosqueEncantado_600m", null);
        Transform sectors = Empty("SECTORES_600M", root.transform).transform;

        BuildEntranceTrail(sectors, 0f, palette);
        BuildTwistedGrove(sectors, 90f, palette);
        BuildTrapZone(sectors, 200f, palette);
        BuildOvergrownRuins(sectors, 300f, palette);
        BuildDarkHeart(sectors, 400f, palette);
        BuildBossArena(sectors, 520f, palette);

        BuildContinuousForest(root.transform, palette);
        GameObject player = BuildPlayer(root.transform, palette);
        BuildCamera(root.transform, player.transform);
        BuildLighting(root.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EnsureSceneInBuildSettings(ScenePath);
        Selection.activeGameObject = root;

        Debug.Log("Bosque Encantado 600m generado en: " + ScenePath);
        EditorUtility.DisplayDialog(
            "Bosque Encantado listo",
            "Se creó el nivel 2.5D de 600 m con seis sectores, trampas, criaturas provisionales, arena del Guardián Corrompido y segundo cristal.\n\n" + ScenePath,
            "Aceptar");
    }

    [MenuItem("Tools/Kingdom Shadow/Open Bosque Encantado 600m")]
    public static void OpenGeneratedScene()
    {
        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("Escena no generada", "Ejecuta primero Build Bosque Encantado 600m.", "Aceptar");
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static void BuildEntranceTrail(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S01_Sendero_de_Entrada_090m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform hazards = Empty("04_Peligros", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("SE-G01_Sendero_Bajo", geo, new Vector3(11f, -0.5f, 0f), new Vector3(22f, 1f, 6f), 0f, p.ground);
        GameplayCube("SE-R01_Raiz_Ascendente", geo, new Vector3(26f, 0.75f, 0f), new Vector3(8.25f, 0.55f, 6f), 14.04f, p.root);
        GameplayCube("SE-G02_Claro_Elevado", geo, new Vector3(40f, 1.5f, 0f), new Vector3(20f, 1f, 6f), 0f, p.ground);
        GameplayCube("SE-P01_Raiz_Plataforma", geo, new Vector3(53f, 2.2f, 0f), new Vector3(6f, 0.7f, 5f), -4f, p.root);
        GameplayCube("SE-G03_Cornisa_Natural", geo, new Vector3(64f, 2.5f, 0f), new Vector3(16f, 1f, 6f), 0f, p.ground);
        GameplayCube("SE-R02_Descenso", geo, new Vector3(75f, 1.25f, 0f), new Vector3(8.25f, 0.55f, 6f), -14.04f, p.root);
        GameplayCube("SE-G04_Salida", geo, new Vector3(84f, -0.5f, 0f), new Vector3(12f, 1f, 6f), 0f, p.ground);
        HazardCube("SE-H01_Grieta", hazards, new Vector3(53f, -3f, 0f), new Vector3(6f, 6f, 7f), p.shadow);

        BuildRootArch(architecture, "Arco_Natural_Entrada", new Vector3(24f, 0f, 8f), 1.25f, p);
        BuildTwistedTree(architecture, "Arbol_Salida", new Vector3(82f, 0f, 9f), 1.1f, p);
        BuildFence(decoration, new Vector3(8f, 0f, 4f), 11f, p);
        BuildWolf(decoration, new Vector3(39f, 2f, 0f), 0.75f, p);
        BuildMushrooms(decoration, new Vector3(66f, 3f, 4.2f), p.crystal);

        Marker("ENTRADA_Lunaria_X003", markers, new Vector3(3f, 0.8f, 0f), p.crystal);
        Marker("CHECKPOINT_Raiz_X066", markers, new Vector3(66f, 3.8f, 0f), p.warmLight);
        Marker("SALIDA_Arboleda_X087", markers, new Vector3(87f, 0.8f, 0f), p.crystal);
    }

    private static void BuildTwistedGrove(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S02_Arboleda_Torcida_110m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform hazards = Empty("04_Peligros", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("AT-G01_Sendero", geo, new Vector3(10f, -0.5f, 0f), new Vector3(20f, 1f, 6f), 0f, p.ground);
        GameplayCube("AT-P01_Tronco_Hueco", geo, new Vector3(28f, 1.2f, 0f), new Vector3(18f, 2.4f, 6f), 3f, p.root);
        GameplayCube("AT-G02_Claro_Central", geo, new Vector3(45f, -0.5f, 0f), new Vector3(16f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "AT-S", 53f, 0f, 8, 1f, 0.5f, 6f, p.root);
        GameplayCube("AT-G03_Ruta_Superior", geo, new Vector3(72f, 3.5f, 0f), new Vector3(22f, 1f, 5f), 0f, p.root);
        GameplayCube("AT-G04_Ruta_Inferior", geo, new Vector3(73f, -0.5f, 0f), new Vector3(24f, 1f, 6f), 0f, p.ground);
        GameplayCube("AT-P02_Raiz_Puente_A", geo, new Vector3(89f, 2.6f, 0f), new Vector3(7f, 0.7f, 5f), -5f, p.root);
        GameplayCube("AT-P03_Raiz_Puente_B", geo, new Vector3(97f, 1.7f, 0f), new Vector3(7f, 0.7f, 5f), -7f, p.root);
        GameplayCube("AT-G05_Salida", geo, new Vector3(105f, 0.5f, 0f), new Vector3(10f, 1f, 6f), 0f, p.ground);
        HazardCube("AT-H01_Hondonada", hazards, new Vector3(93f, -4f, 0f), new Vector3(14f, 7f, 8f), p.shadow);

        BuildTwistedTree(architecture, "Arbol_Retorcido_A", new Vector3(18f, 0f, 9f), 1.3f, p);
        BuildFallenTrunk(architecture, new Vector3(31f, 3.5f, 8f), 24f, p);
        BuildTwistedTree(architecture, "Arbol_Retorcido_B", new Vector3(63f, 0f, 10f), 1.55f, p);
        BuildTwistedTree(architecture, "Arbol_Retorcido_C", new Vector3(101f, 1f, 9f), 1.2f, p);
        BuildShadowCreature(decoration, new Vector3(82f, 0f, 0f), 0.9f, p);
        BuildMushrooms(decoration, new Vector3(78f, 4f, 3.3f), p.crystal);

        Marker("ENTRADA_X003", markers, new Vector3(3f, 0.8f, 0f), p.crystal);
        Marker("RUTA_ALTA_X063", markers, new Vector3(63f, 4.8f, 0f), p.warmLight);
        Marker("SALIDA_Trampas_X107", markers, new Vector3(107f, 1.8f, 0f), p.crystal);
    }

    private static void BuildTrapZone(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S03_Zona_de_Trampas_100m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform hazards = Empty("04_Peligros", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("ZT-G01_Acceso", geo, new Vector3(9f, -0.5f, 0f), new Vector3(18f, 1f, 6f), 0f, p.ground);
        GameplayCube("ZT-P01_Inestable_A", geo, new Vector3(22f, 1.4f, 0f), new Vector3(6f, 0.6f, 5f), 0f, p.root);
        GameplayCube("ZT-P02_Inestable_B", geo, new Vector3(29f, 2.1f, 0f), new Vector3(5f, 0.6f, 5f), 0f, p.root);
        GameplayCube("ZT-G02_Isla_Segura", geo, new Vector3(39f, -0.5f, 0f), new Vector3(14f, 1f, 6f), 0f, p.ground);
        GameplayCube("ZT-G03_Paso_Espinas", geo, new Vector3(51f, -0.5f, 0f), new Vector3(10f, 1f, 6f), 0f, p.ground);
        GameplayCube("ZT-P03_Puente_Roto_A", geo, new Vector3(61f, 1.2f, 0f), new Vector3(6f, 0.6f, 5f), 0f, p.root);
        GameplayCube("ZT-P04_Puente_Roto_B", geo, new Vector3(69f, 2.1f, 0f), new Vector3(6f, 0.6f, 5f), 0f, p.root);
        GameplayCube("ZT-P05_Puente_Roto_C", geo, new Vector3(77f, 1.2f, 0f), new Vector3(6f, 0.6f, 5f), 0f, p.root);
        GameplayCube("ZT-G04_Salida_Segura", geo, new Vector3(91f, -0.5f, 0f), new Vector3(18f, 1f, 6f), 0f, p.ground);

        HazardCube("ZT-H01_Foso_Raices", hazards, new Vector3(25f, -4f, 0f), new Vector3(14f, 7f, 8f), p.shadow);
        HazardCube("ZT-H02_Espinas_Centrales", hazards, new Vector3(51f, 0.8f, 0f), new Vector3(7f, 1.6f, 5f), p.trap, true);
        HazardCube("ZT-H03_Foso_Profundo", hazards, new Vector3(69f, -4f, 0f), new Vector3(22f, 7f, 8f), p.shadow);

        BuildTwistedTree(architecture, "Arbol_Polea_A", new Vector3(18f, 0f, 8f), 1.15f, p);
        BuildSwingingLog(architecture, new Vector3(39f, 7f, 0f), p);
        BuildTwistedTree(architecture, "Arbol_Polea_B", new Vector3(58f, 0f, 8f), 1.3f, p);
        BuildThorns(decoration, new Vector3(51f, 0f, 0f), 7, p);
        BuildThorns(decoration, new Vector3(83f, 0f, 4f), 9, p);

        Marker("ENTRADA_X003", markers, new Vector3(3f, 0.8f, 0f), p.crystal);
        Marker("TRAMPA_Tronco_X039", markers, new Vector3(39f, 1f, 0f), p.trap);
        Marker("CHECKPOINT_X043", markers, new Vector3(43f, 0.8f, 0f), p.warmLight);
        Marker("SALIDA_Ruinas_X097", markers, new Vector3(97f, 0.8f, 0f), p.crystal);
    }

    private static void BuildOvergrownRuins(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S04_Ruinas_Cubiertas_100m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform hazards = Empty("04_Peligros", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("RV-G01_Acceso", geo, new Vector3(10f, -0.5f, 0f), new Vector3(20f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "RV-S", 20.5f, 0f, 8, 1f, 0.5f, 6f, p.stone);
        GameplayCube("RV-G02_Galeria_Elevada", geo, new Vector3(42f, 3.5f, 0f), new Vector3(26f, 1f, 7f), 0f, p.stone);
        GameplayCube("RV-P01_Arco_Caido", geo, new Vector3(59f, 2.4f, 0f), new Vector3(7f, 0.7f, 5f), -8f, p.stone);
        GameplayCube("RV-G03_Patio_Hundido", geo, new Vector3(70f, -0.5f, 0f), new Vector3(16f, 1f, 7f), 0f, p.ground);
        BuildStairs(geo, "RV-T", 78.5f, 0f, 8, 1f, 0.5f, 6f, p.stone);
        GameplayCube("RV-G04_Terraza_Final", geo, new Vector3(94f, 3.5f, 0f), new Vector3(12f, 1f, 6f), 0f, p.stone);
        HazardCube("RV-H01_Derrumbe", hazards, new Vector3(59f, -3.5f, 0f), new Vector3(7f, 6f, 7f), p.shadow);

        BuildRuinColumns(architecture, new Vector3(15f, 0f, 8f), p);
        BuildRuinArch(architecture, "Arco_Central", new Vector3(42f, 4f, 8f), 1.4f, p);
        BuildRuinShelter(architecture, new Vector3(58f, 0f, 10f), p);
        BuildAqueduct(architecture, new Vector3(72f, 0f, 13f), p);
        BuildRuinArch(architecture, "Arco_Salida", new Vector3(95f, 4f, 8f), 1f, p);
        BuildVines(decoration, new Vector3(42f, 9f, 6f), 8, p);
        BuildMushrooms(decoration, new Vector3(69f, 0f, 4f), p.warmLight);

        Marker("ENTRADA_X003", markers, new Vector3(3f, 0.8f, 0f), p.crystal);
        Marker("INFORMANTE_X052", markers, new Vector3(52f, 4.8f, 0f), p.warmLight);
        Marker("CHECKPOINT_X069", markers, new Vector3(69f, 0.8f, 0f), p.crystal);
        Marker("SALIDA_Corazon_X097", markers, new Vector3(97f, 4.8f, 0f), p.crystal);
    }

    private static void BuildDarkHeart(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S05_Corazon_Oscuro_120m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform hazards = Empty("04_Peligros", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("CO-G01_Acceso", geo, new Vector3(12f, -0.5f, 0f), new Vector3(24f, 1f, 6f), 0f, p.ground);
        GameplayCube("CO-P01_Raiz_Puente", geo, new Vector3(37f, 1.6f, 0f), new Vector3(26f, 0.8f, 5f), 2f, p.root);
        GameplayCube("CO-G02_Isla_Cristalina", geo, new Vector3(58f, 2.5f, 0f), new Vector3(16f, 1f, 7f), 0f, p.ground);
        GameplayCube("CO-P02_Raiz_Flotante_A", geo, new Vector3(70f, 3.1f, 0f), new Vector3(6f, 0.7f, 5f), -4f, p.root);
        GameplayCube("CO-P03_Raiz_Flotante_B", geo, new Vector3(78f, 4.1f, 0f), new Vector3(6f, 0.7f, 5f), 4f, p.root);
        GameplayCube("CO-P04_Raiz_Flotante_C", geo, new Vector3(86f, 3.1f, 0f), new Vector3(6f, 0.7f, 5f), -4f, p.root);
        GameplayCube("CO-G03_Cornisa_Alta", geo, new Vector3(99f, 3.5f, 0f), new Vector3(18f, 1f, 6f), 0f, p.ground);
        GameplayCube("CO-R01_Descenso_Final", geo, new Vector3(111f, 1.5f, 0f), new Vector3(8.94f, 0.6f, 6f), -26.57f, p.root);
        GameplayCube("CO-G04_Umbral", geo, new Vector3(117f, -0.5f, 0f), new Vector3(6f, 1f, 6f), 0f, p.ground);
        HazardCube("CO-H01_Poza_Sombra", hazards, new Vector3(37f, -4f, 0f), new Vector3(26f, 7f, 8f), p.shadow);
        HazardCube("CO-H02_Abismo_Central", hazards, new Vector3(78f, -5f, 0f), new Vector3(26f, 9f, 8f), p.shadow);

        BuildRootArch(architecture, "Raices_Envolventes", new Vector3(20f, 0f, 9f), 1.4f, p);
        BuildCrystalCluster(architecture, new Vector3(54f, 3f, 7f), 1.2f, p);
        BuildTwistedTree(architecture, "Arbol_Corazon", new Vector3(76f, 1f, 11f), 1.9f, p);
        BuildCrystalCluster(architecture, new Vector3(97f, 4f, 7f), 1.4f, p);
        BuildRootArch(architecture, "Umbral_Arena", new Vector3(116f, 0f, 8f), 1f, p);
        BuildShadowCreature(decoration, new Vector3(57f, 3f, 0f), 0.9f, p);
        BuildShadowCreature(decoration, new Vector3(99f, 4f, 0f), 1.2f, p);

        Marker("ENTRADA_X003", markers, new Vector3(3f, 0.8f, 0f), p.crystal);
        Marker("CHECKPOINT_PreJefe_X108", markers, new Vector3(108f, 3.1f, 0f), p.warmLight);
        Marker("SALIDA_Arena_X118", markers, new Vector3(118f, 0.8f, 0f), p.crystal);
    }

    private static void BuildBossArena(Transform parent, float offsetX, Palette p)
    {
        Transform sector = Sector(parent, "S06_Arena_Guardian_Corrompido_080m", offsetX);
        Transform geo = Empty("01_Gameplay", sector).transform;
        Transform architecture = Empty("02_Arquitectura", sector).transform;
        Transform decoration = Empty("03_Decoracion", sector).transform;
        Transform gameplay = Empty("04_Jefe_y_Cristal", sector).transform;
        Transform markers = Empty("05_Marcadores", sector).transform;

        GameplayCube("AJ-G01_Acceso", geo, new Vector3(5f, -0.5f, 0f), new Vector3(10f, 1f, 6f), 0f, p.ground);
        BuildStairs(geo, "AJ-S", 10.75f, 0f, 4, 1.5f, 0.5f, 6f, p.root);
        GameplayCube("AJ-G02_Arena_Natural", geo, new Vector3(43f, 1.5f, 0f), new Vector3(54f, 1f, 12f), 0f, p.ground);
        GameplayCube("AJ-P01_Raiz_Baja_A", geo, new Vector3(27f, 2.5f, 0f), new Vector3(7f, 2f, 7f), 0f, p.root);
        GameplayCube("AJ-P02_Raiz_Baja_B", geo, new Vector3(57f, 2.5f, 0f), new Vector3(7f, 2f, 7f), 0f, p.root);
        GameplayCube("AJ-G03_Pedestal", geo, new Vector3(72f, 3f, 0f), new Vector3(4f, 4f, 4f), 0f, p.stone);

        BuildArenaRootPillars(architecture, p);
        BuildRootWall(architecture, new Vector3(43f, 5f, 11f), 56f, p);
        BuildRuinArch(architecture, "Arco_Segundo_Cristal", new Vector3(72f, 4f, 6f), 1.1f, p);
        BuildCorruptedGuardian(gameplay, new Vector3(48f, 3f, 0f), p);
        BuildLightCrystal(gameplay, new Vector3(72f, 6f, 0f), p);
        BuildVines(decoration, new Vector3(43f, 11f, 8f), 12, p);

        Marker("ENTRADA_Leo_X538", markers, new Vector3(18f, 2.8f, 0f), p.crystal);
        Marker("TRIGGER_Cierre_X542", markers, new Vector3(22f, 3f, 0f), p.trap);
        Marker("JEFE_X568", markers, new Vector3(48f, 3.2f, 0f), p.shadow);
        Marker("SEGUNDO_CRISTAL_X592", markers, new Vector3(72f, 5.8f, 0f), p.crystal);
    }

    private static void BuildContinuousForest(Transform parent, Palette p)
    {
        Transform background = Empty("FONDO_CONTINUO_BOSQUE", parent).transform;
        Primitive("Niebla_Fondo", PrimitiveType.Cube, background, new Vector3(300f, 6f, 24f), new Vector3(640f, 30f, 2f), Quaternion.identity, p.foreground, false);

        for (int i = 0; i < 34; i++)
        {
            float x = 5f + i * 18f;
            float scale = 0.6f + (i % 5) * 0.1f;
            BuildBackgroundTree(background, new Vector3(x, -2f + i % 3, 13f + i % 3), scale, p);
        }

        Transform foreground = Empty("PRIMER_PLANO", parent).transform;
        for (int i = 0; i < 22; i++)
        {
            float x = 8f + i * 28f;
            Primitive("Raiz_Silueta_" + i.ToString("00"), PrimitiveType.Capsule, foreground,
                new Vector3(x, -1.5f, -4f), new Vector3(5f + i % 3, 1.1f, 0.8f),
                Quaternion.Euler(0f, 0f, 78f + (i % 2) * 8f), p.foreground, false);
        }
    }

    private static GameObject BuildPlayer(Transform parent, Palette p)
    {
        GameObject player = Empty("PLAYER_Leo", parent);
        player.transform.position = new Vector3(3f, 1.3f, 0f);
        player.tag = "Player";

        Rigidbody body = player.AddComponent<Rigidbody>();
        body.mass = 1f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        body.constraints = RigidbodyConstraints.FreezePositionZ |
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
        Primitive("Cuerpo", PrimitiveType.Capsule, visual, new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.5f), Quaternion.identity, p.moss, false);
        Primitive("Cabeza", PrimitiveType.Sphere, visual, new Vector3(0f, 1.9f, 0f), Vector3.one * 0.55f, Quaternion.identity, p.warmLight, false);
        Primitive("Espada", PrimitiveType.Cube, visual, new Vector3(0.5f, 0.95f, 0f), new Vector3(0.12f, 1.2f, 0.12f), Quaternion.Euler(0f, 0f, -18f), p.stone, false);
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
        camera.backgroundColor = new Color(0.018f, 0.035f, 0.04f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();
        follow.Configure(target, 4f, 596f, true);
    }

    private static void BuildLighting(Transform parent)
    {
        Transform lighting = Empty("LIGHTING_Bosque", parent).transform;
        GameObject moon = new GameObject("Luz_Lunar", typeof(Light));
        moon.transform.SetParent(lighting, false);
        moon.transform.rotation = Quaternion.Euler(38f, -28f, 0f);
        Light directional = moon.GetComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(0.38f, 0.58f, 0.63f);
        directional.intensity = 0.62f;
        directional.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.055f, 0.11f, 0.12f);
        RenderSettings.ambientEquatorColor = new Color(0.035f, 0.075f, 0.065f);
        RenderSettings.ambientGroundColor = new Color(0.012f, 0.02f, 0.018f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.045f, 0.09f, 0.09f);
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 105f;
    }

    private static void BuildTwistedTree(Transform parent, string name, Vector3 position, float scale, Palette p)
    {
        Transform tree = Empty(name, parent).transform;
        tree.localPosition = position;
        tree.localScale = Vector3.one * scale;
        Primitive("Tronco", PrimitiveType.Cylinder, tree, new Vector3(0f, 4.2f, 0f), new Vector3(1.6f, 4.2f, 1.6f), Quaternion.Euler(0f, 0f, 7f), p.bark, false);
        Primitive("Rama_I", PrimitiveType.Cylinder, tree, new Vector3(-2.3f, 7.4f, 0f), new Vector3(0.65f, 3.5f, 0.65f), Quaternion.Euler(0f, 0f, -58f), p.bark, false);
        Primitive("Rama_D", PrimitiveType.Cylinder, tree, new Vector3(2.2f, 7.1f, 0.2f), new Vector3(0.55f, 3.2f, 0.55f), Quaternion.Euler(0f, 0f, 54f), p.bark, false);
        Primitive("Copa_I", PrimitiveType.Sphere, tree, new Vector3(-3.8f, 9f, 0f), new Vector3(4.8f, 3.4f, 3.5f), Quaternion.identity, p.foliage, false);
        Primitive("Copa_D", PrimitiveType.Sphere, tree, new Vector3(3.6f, 8.8f, 0.3f), new Vector3(5f, 3.6f, 3.8f), Quaternion.identity, p.foliage, false);
    }

    private static void BuildBackgroundTree(Transform parent, Vector3 position, float scale, Palette p)
    {
        Transform tree = Empty("Arbol_Fondo", parent).transform;
        tree.localPosition = position;
        tree.localScale = Vector3.one * scale;
        Primitive("Tronco", PrimitiveType.Cylinder, tree, new Vector3(0f, 7f, 0f), new Vector3(1.5f, 7f, 1.5f), Quaternion.Euler(0f, 0f, -4f), p.foreground, false);
        Primitive("Copa", PrimitiveType.Sphere, tree, new Vector3(0f, 14f, 0f), new Vector3(8f, 5f, 5f), Quaternion.identity, p.foreground, false);
    }

    private static void BuildRootArch(Transform parent, string name, Vector3 position, float scale, Palette p)
    {
        Transform root = Empty(name, parent).transform;
        root.localPosition = position;
        root.localScale = Vector3.one * scale;
        Primitive("Pilar_I", PrimitiveType.Cylinder, root, new Vector3(-4f, 4f, 0f), new Vector3(1.4f, 4f, 1.4f), Quaternion.Euler(0f, 0f, -15f), p.bark, false);
        Primitive("Pilar_D", PrimitiveType.Cylinder, root, new Vector3(4f, 4f, 0f), new Vector3(1.4f, 4f, 1.4f), Quaternion.Euler(0f, 0f, 15f), p.bark, false);
        Primitive("Arco", PrimitiveType.Capsule, root, new Vector3(0f, 8.2f, 0f), new Vector3(1.2f, 5f, 1.2f), Quaternion.Euler(0f, 0f, 90f), p.bark, false);
    }

    private static void BuildFallenTrunk(Transform parent, Vector3 position, float length, Palette p)
    {
        Transform trunk = Empty("Tronco_Caido", parent).transform;
        trunk.localPosition = position;
        Primitive("Cuerpo", PrimitiveType.Cylinder, trunk, Vector3.zero, new Vector3(2.7f, length * 0.5f, 2.7f), Quaternion.Euler(0f, 0f, 90f), p.bark, false);
        Primitive("Hueco", PrimitiveType.Cylinder, trunk, new Vector3(-length * 0.49f, 0f, 0f), new Vector3(1.6f, 0.25f, 1.6f), Quaternion.Euler(0f, 0f, 90f), p.shadow, false);
    }

    private static void BuildSwingingLog(Transform parent, Vector3 position, Palette p)
    {
        Transform trap = Empty("Trampa_Tronco_Oscilante", parent).transform;
        trap.localPosition = position;
        Primitive("Cuerda_I", PrimitiveType.Cylinder, trap, new Vector3(-6f, 3f, 0f), new Vector3(0.08f, 3f, 0.08f), Quaternion.identity, p.trap, false);
        Primitive("Cuerda_D", PrimitiveType.Cylinder, trap, new Vector3(6f, 3f, 0f), new Vector3(0.08f, 3f, 0.08f), Quaternion.identity, p.trap, false);
        Primitive("Tronco", PrimitiveType.Cylinder, trap, Vector3.zero, new Vector3(1.1f, 8f, 1.1f), Quaternion.Euler(0f, 0f, 90f), p.bark, false);
        for (int i = 0; i < 7; i++)
            Primitive("Espina_" + i, PrimitiveType.Cube, trap, new Vector3(-6f + i * 2f, -1.2f, 0f), new Vector3(0.35f, 1.5f, 0.35f), Quaternion.Euler(0f, 0f, 45f), p.trap, false);
    }

    private static void BuildThorns(Transform parent, Vector3 position, int count, Palette p)
    {
        Transform thorns = Empty("Espinas", parent).transform;
        thorns.localPosition = position;
        for (int i = 0; i < count; i++)
            Primitive("Espina_" + i.ToString("00"), PrimitiveType.Cube, thorns, new Vector3((i - count * 0.5f) * 0.9f, 0.8f + i % 2 * 0.25f, 0f), new Vector3(0.45f, 1.8f, 0.45f), Quaternion.Euler(0f, 0f, 45f), p.trap, false);
    }

    private static void BuildRuinColumns(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Columnas_Acceso", parent).transform;
        root.localPosition = position;
        for (int i = 0; i < 3; i++)
            Primitive("Columna_" + i, PrimitiveType.Cylinder, root, new Vector3(-4f + i * 4f, 4f + i % 2, 0f), new Vector3(1f, 4f + i % 2, 1f), Quaternion.Euler(0f, 0f, i == 2 ? 7f : 0f), p.stone, false);
    }

    private static void BuildRuinArch(Transform parent, string name, Vector3 position, float scale, Palette p)
    {
        Transform root = Empty(name, parent).transform;
        root.localPosition = position;
        root.localScale = Vector3.one * scale;
        Primitive("Columna_I", PrimitiveType.Cube, root, new Vector3(-4f, 3.5f, 0f), new Vector3(2f, 7f, 3f), Quaternion.identity, p.stone, false);
        Primitive("Columna_D", PrimitiveType.Cube, root, new Vector3(4f, 3.5f, 0f), new Vector3(2f, 7f, 3f), Quaternion.identity, p.stone, false);
        Primitive("Dintel", PrimitiveType.Cube, root, new Vector3(0f, 7f, 0f), new Vector3(10f, 2f, 3f), Quaternion.Euler(0f, 0f, 3f), p.stone, false);
        Primitive("Musgo", PrimitiveType.Cube, root, new Vector3(0f, 8.1f, 0f), new Vector3(9f, 0.25f, 3.2f), Quaternion.identity, p.moss, false);
    }

    private static void BuildRuinShelter(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Refugio_Ruinoso", parent).transform;
        root.localPosition = position;
        Primitive("Muro", PrimitiveType.Cube, root, new Vector3(0f, 4f, 1.5f), new Vector3(16f, 8f, 2f), Quaternion.identity, p.stone, false);
        Primitive("Techo_Roto", PrimitiveType.Cube, root, new Vector3(-2f, 8.2f, 0f), new Vector3(11f, 0.8f, 6f), Quaternion.Euler(0f, 0f, -7f), p.stone, false);
        Primitive("Raiz", PrimitiveType.Capsule, root, new Vector3(3f, 5f, -1f), new Vector3(0.7f, 5f, 0.7f), Quaternion.Euler(0f, 0f, -35f), p.root, false);
    }

    private static void BuildAqueduct(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Acueducto_Fondo", parent).transform;
        root.localPosition = position;
        for (int i = 0; i < 6; i++)
        {
            float x = -20f + i * 8f;
            Primitive("Pilar_" + i, PrimitiveType.Cube, root, new Vector3(x, 7f, 0f), new Vector3(1.6f, 14f, 2.5f), Quaternion.identity, p.stone, false);
            if (i < 5)
                Primitive("Tramo_" + i, PrimitiveType.Cube, root, new Vector3(x + 4f, 13.4f, 0f), new Vector3(8f, 1.8f, 2.5f), Quaternion.identity, p.stone, false);
        }
    }

    private static void BuildVines(Transform parent, Vector3 position, int count, Palette p)
    {
        Transform root = Empty("Enredaderas", parent).transform;
        root.localPosition = position;
        for (int i = 0; i < count; i++)
            Primitive("Liana_" + i.ToString("00"), PrimitiveType.Cylinder, root, new Vector3((i - count * 0.5f) * 1.2f, -(i % 4), 0f), new Vector3(0.08f, 1.5f + i % 4, 0.08f), Quaternion.Euler(0f, 0f, -5f + i % 3 * 5f), p.moss, false);
    }

    private static void BuildCrystalCluster(Transform parent, Vector3 position, float scale, Palette p)
    {
        Transform root = Empty("Cristales_Oscuros", parent).transform;
        root.localPosition = position;
        root.localScale = Vector3.one * scale;
        for (int i = 0; i < 6; i++)
        {
            float x = -2.5f + i;
            float height = 2.2f + i % 3 * 1.6f;
            Primitive("Cristal_" + i, PrimitiveType.Cube, root, new Vector3(x, height * 0.5f, 0f), new Vector3(0.8f, height, 0.8f), Quaternion.Euler(0f, 45f, 35f + i % 2 * 10f), p.shadow, false);
        }
    }

    private static void BuildArenaRootPillars(Transform parent, Palette p)
    {
        float[] positions = { 20f, 36f, 52f };
        for (int i = 0; i < positions.Length; i++)
        {
            Transform root = Empty("Tronco_Pilar_" + (i + 1), parent).transform;
            root.localPosition = new Vector3(positions[i], 2f, 8f);
            Primitive("Tronco", PrimitiveType.Cylinder, root, new Vector3(0f, 6f, 0f), new Vector3(1.4f, 6f + i, 1.4f), Quaternion.Euler(0f, 0f, -4f + i * 4f), p.bark, false);
        }
    }

    private static void BuildRootWall(Transform parent, Vector3 position, float width, Palette p)
    {
        Transform root = Empty("Muro_de_Raices", parent).transform;
        root.localPosition = position;
        Primitive("Sombra_Fondo", PrimitiveType.Cube, root, Vector3.zero, new Vector3(width, 10f, 3f), Quaternion.identity, p.foreground, false);
        for (int i = 0; i < 12; i++)
            Primitive("Raiz_" + i.ToString("00"), PrimitiveType.Capsule, root, new Vector3(-width * 0.45f + i * width / 11f, 0f, -2f), new Vector3(0.6f, 6f, 0.6f), Quaternion.Euler(0f, 0f, -25f + i % 5 * 12f), p.root, false);
    }

    private static void BuildCorruptedGuardian(Transform parent, Vector3 position, Palette p)
    {
        Transform boss = Empty("JEFE_Guardian_Arboreo_Corrompido", parent).transform;
        boss.localPosition = position;
        Primitive("Torso", PrimitiveType.Capsule, boss, new Vector3(0f, 4f, 0f), new Vector3(3f, 4f, 2.4f), Quaternion.identity, p.bark, false);
        Primitive("Cabeza", PrimitiveType.Sphere, boss, new Vector3(0f, 8.2f, 0f), new Vector3(2.6f, 2.2f, 2.3f), Quaternion.identity, p.shadow, false);
        Primitive("Brazo_I", PrimitiveType.Capsule, boss, new Vector3(-4f, 4.3f, 0f), new Vector3(1f, 4.2f, 1f), Quaternion.Euler(0f, 0f, -52f), p.bark, false);
        Primitive("Brazo_D", PrimitiveType.Capsule, boss, new Vector3(4f, 4.3f, 0f), new Vector3(1f, 4.2f, 1f), Quaternion.Euler(0f, 0f, 52f), p.bark, false);
        Primitive("Pierna_I", PrimitiveType.Capsule, boss, new Vector3(-1.6f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 1.2f), Quaternion.Euler(0f, 0f, -10f), p.root, false);
        Primitive("Pierna_D", PrimitiveType.Capsule, boss, new Vector3(1.6f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 1.2f), Quaternion.Euler(0f, 0f, 10f), p.root, false);
        Primitive("Ojo_I", PrimitiveType.Sphere, boss, new Vector3(-0.7f, 8.5f, -2f), Vector3.one * 0.35f, Quaternion.identity, p.crystal, false);
        Primitive("Ojo_D", PrimitiveType.Sphere, boss, new Vector3(0.7f, 8.5f, -2f), Vector3.one * 0.35f, Quaternion.identity, p.crystal, false);
        for (int i = 0; i < 7; i++)
            Primitive("Rama_Cabeza_" + i, PrimitiveType.Cylinder, boss, new Vector3(-3f + i, 10f + i % 3, 0f), new Vector3(0.25f, 2.5f, 0.25f), Quaternion.Euler(0f, 0f, -45f + i * 15f), p.bark, false);
    }

    private static void BuildLightCrystal(Transform parent, Vector3 position, Palette p)
    {
        Transform root = Empty("Cristal_de_Luz_02", parent).transform;
        root.localPosition = position;
        Primitive("Nucleo", PrimitiveType.Cube, root, Vector3.zero, new Vector3(1.6f, 3.8f, 1.6f), Quaternion.Euler(0f, 45f, 45f), p.crystal, false);
        Primitive("Fragmento_I", PrimitiveType.Cube, root, new Vector3(-1.1f, -0.4f, 0f), new Vector3(0.7f, 2.1f, 0.7f), Quaternion.Euler(0f, 35f, 25f), p.crystal, false);
        Primitive("Fragmento_D", PrimitiveType.Cube, root, new Vector3(1.1f, -0.4f, 0f), new Vector3(0.7f, 2.1f, 0.7f), Quaternion.Euler(0f, -35f, -25f), p.crystal, false);
        GameObject lightObject = new GameObject("Luz_Cristal", typeof(Light));
        lightObject.transform.SetParent(root, false);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.28f, 0.95f, 0.78f);
        light.intensity = 3.2f;
        light.range = 14f;
    }

    private static void BuildWolf(Transform parent, Vector3 position, float scale, Palette p)
    {
        Transform wolf = Empty("ENEMIGO_Lobo_Blockout", parent).transform;
        wolf.localPosition = position;
        wolf.localScale = Vector3.one * scale;
        Primitive("Cuerpo", PrimitiveType.Capsule, wolf, new Vector3(0f, 1.3f, 0f), new Vector3(1.3f, 1.8f, 1.1f), Quaternion.Euler(0f, 0f, 90f), p.shadow, false);
        Primitive("Cabeza", PrimitiveType.Sphere, wolf, new Vector3(1.7f, 2f, 0f), new Vector3(1.3f, 1.2f, 1.2f), Quaternion.identity, p.shadow, false);
        for (int i = 0; i < 4; i++)
            Primitive("Pata_" + i, PrimitiveType.Capsule, wolf, new Vector3(-0.8f + i / 2 * 1.7f, 0.45f, i % 2 == 0 ? -0.45f : 0.45f), new Vector3(0.32f, 0.8f, 0.32f), Quaternion.identity, p.shadow, false);
    }

    private static void BuildShadowCreature(Transform parent, Vector3 position, float scale, Palette p)
    {
        Transform creature = Empty("ENEMIGO_Criatura_Sombra_Blockout", parent).transform;
        creature.localPosition = position;
        creature.localScale = Vector3.one * scale;
        Primitive("Torso", PrimitiveType.Capsule, creature, new Vector3(0f, 1.6f, 0f), new Vector3(0.9f, 1.6f, 0.8f), Quaternion.identity, p.shadow, false);
        Primitive("Cabeza", PrimitiveType.Sphere, creature, new Vector3(0f, 3f, 0f), Vector3.one, Quaternion.identity, p.shadow, false);
        Primitive("Ojo", PrimitiveType.Sphere, creature, new Vector3(0f, 3.1f, -0.85f), Vector3.one * 0.28f, Quaternion.identity, p.crystal, false);
        Primitive("Brazo_I", PrimitiveType.Capsule, creature, new Vector3(-1.2f, 1.4f, 0f), new Vector3(0.3f, 1.4f, 0.3f), Quaternion.Euler(0f, 0f, -25f), p.shadow, false);
        Primitive("Brazo_D", PrimitiveType.Capsule, creature, new Vector3(1.2f, 1.4f, 0f), new Vector3(0.3f, 1.4f, 0.3f), Quaternion.Euler(0f, 0f, 25f), p.shadow, false);
    }

    private static void BuildFence(Transform parent, Vector3 position, float length, Palette p)
    {
        Transform root = Empty("Cerca_Rota", parent).transform;
        root.localPosition = position;
        Primitive("Travesano", PrimitiveType.Cube, root, new Vector3(0f, 1.1f, 0f), new Vector3(length, 0.18f, 0.18f), Quaternion.Euler(0f, 0f, -4f), p.bark, false);
        for (float x = -length * 0.5f; x <= length * 0.5f; x += 2.4f)
            Primitive("Poste", PrimitiveType.Cube, root, new Vector3(x, 1f, 0f), new Vector3(0.2f, 2f, 0.2f), Quaternion.Euler(0f, 0f, x % 2f), p.bark, false);
    }

    private static void BuildMushrooms(Transform parent, Vector3 position, Material glow)
    {
        Transform root = Empty("Hongos_Luminosos", parent).transform;
        root.localPosition = position;
        for (int i = 0; i < 5; i++)
        {
            float x = -1.2f + i * 0.6f;
            Primitive("Tallo_" + i, PrimitiveType.Cylinder, root, new Vector3(x, 0.3f + i % 2 * 0.12f, 0f), new Vector3(0.1f, 0.3f + i % 2 * 0.12f, 0.1f), Quaternion.identity, glow, false);
            Primitive("Sombrero_" + i, PrimitiveType.Sphere, root, new Vector3(x, 0.7f + i % 2 * 0.22f, 0f), new Vector3(0.45f, 0.2f, 0.45f), Quaternion.identity, glow, false);
        }
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
            float height = (i + 1) * rise;
            GameplayCube(prefix + "_Escalon_" + (i + 1).ToString("00"), parent,
                new Vector3(startX + i * width, baseY + height * 0.5f, 0f),
                new Vector3(width, height, depth), 0f, material);
        }
    }

    private static GameObject GameplayCube(string name, Transform parent, Vector3 position, Vector3 scale, float rotationZ, Material material)
    {
        return Primitive(name, PrimitiveType.Cube, parent, position, scale, Quaternion.Euler(0f, 0f, rotationZ), material, true);
    }

    private static GameObject HazardCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool visible = false)
    {
        GameObject hazard = Primitive(name, PrimitiveType.Cube, parent, position, scale, Quaternion.identity, material, true);
        BoxCollider collider = hazard.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        hazard.GetComponent<Renderer>().enabled = visible;
        return hazard;
    }

    private static GameObject Marker(string name, Transform parent, Vector3 position, Material material)
    {
        GameObject marker = Primitive(name, PrimitiveType.Sphere, parent, position, Vector3.one * 0.35f, Quaternion.identity, material, false);
        return marker;
    }

    private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material, bool keepCollider)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
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
            ground = LoadOrCreateMaterial("MAT_Bosque_Ground", new Color(0.18f, 0.22f, 0.17f)),
            root = LoadOrCreateMaterial("MAT_Bosque_Root", new Color(0.24f, 0.15f, 0.09f)),
            bark = LoadOrCreateMaterial("MAT_Bosque_Bark", new Color(0.14f, 0.09f, 0.065f)),
            foliage = LoadOrCreateMaterial("MAT_Bosque_Foliage", new Color(0.075f, 0.18f, 0.13f)),
            stone = LoadOrCreateMaterial("MAT_Bosque_Stone", new Color(0.30f, 0.34f, 0.31f)),
            moss = LoadOrCreateMaterial("MAT_Bosque_Moss", new Color(0.20f, 0.34f, 0.20f)),
            shadow = LoadOrCreateMaterial("MAT_Bosque_Shadow", new Color(0.045f, 0.035f, 0.07f)),
            crystal = LoadOrCreateMaterial("MAT_Bosque_Crystal", new Color(0.20f, 0.90f, 0.72f)),
            trap = LoadOrCreateMaterial("MAT_Bosque_Trap", new Color(0.42f, 0.10f, 0.12f)),
            foreground = LoadOrCreateMaterial("MAT_Bosque_Foreground", new Color(0.018f, 0.03f, 0.025f)),
            warmLight = LoadOrCreateMaterial("MAT_Bosque_WarmLight", new Color(0.85f, 0.68f, 0.25f))
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
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                return;
        }
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
