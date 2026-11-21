#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Crea los dos niveles pendientes con el mismo formato que Lunaria y Bosque:
/// 600 metros, seis sectores de 100 m y estilo lateral 2.5D.
/// Basado en los planos originales de 144 m (montañas) y 176 m (castillo).
/// Esta fase prepara geometría y marcadores; NO integra enemigos ni recompensas.
/// </summary>
public static class KingdomShadowRemaining600mBuilder
{
    private const string SceneFolder = "Assets/Game/Scenes/Levels";
    private const string ArtRoot = "Assets/Game/Art/Environment";
    private const float LevelLength = 600f;
    private const float SectorLength = 100f;
    private static readonly string[] MountainSectors =
    {
        "01_SenderoDeNieve", "02_Cornisas", "03_Abismos",
        "04_Cumbres", "05_PasosHelados", "06_ArenaHelada"
    };
    private static readonly string[] CastleSectors =
    {
        "01_PuertaExterior", "02_Galerias", "03_Puentes",
        "04_Torres", "05_Antecamaras", "06_ArenaFinal"
    };

    private sealed class Palette
    {
        public Material ground, ramp, ice, bridge, danger, stone, decoration, glow;
        public PhysicsMaterial icePhysics;
    }

    [MenuItem("Tools/Kingdom Shadow/Build Montañas Heladas 600m (6 sectores)")]
    public static void BuildMountains()
    {
        Build(true);
    }

    [MenuItem("Tools/Kingdom Shadow/Build Castillo de las Sombras 600m (6 sectores)")]
    public static void BuildCastle()
    {
        Build(false);
    }

    private static void Build(bool mountains)
    {
        string sceneName = mountains ? "MontanasHeladas_600m" : "CastilloSombras_600m";
        string path = SceneFolder + "/" + sceneName + ".unity";
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        if (File.Exists(path) && !EditorUtility.DisplayDialog(
            "Regenerar el escenario",
            sceneName + " ya existe. ¿Quieres reemplazar solamente esa escena?",
            "Regenerar", "Cancelar"))
            return;

        EnsureFolder(SceneFolder);
        string folder = ArtRoot + "/" + (mountains ? "MontanasHeladas" : "CastilloSombras")
            + "/Generated/Materials";
        EnsureFolder(folder);
        Palette palette = MakePalette(folder, mountains);
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Transform root = Empty("SCN_" + sceneName, null);
        Transform sectorRoot = Empty("SECTORES_600M", root);
        Transform markerRoot = Empty("Markers_PendientesIntegracion", root);
        Transform cameraRoot = Empty("Camera_Rig", root);
        Transform lightsRoot = Empty("Lighting_VFX", root);
        Empty("Enemies_PendientesIntegracion", root);

        string[] names = mountains ? MountainSectors : CastleSectors;
        for (int i = 0; i < 6; i++)
        {
            Transform sector = Empty(names[i], sectorRoot);
            sector.localPosition = new Vector3(i * SectorLength, 0f, 0f);
            Transform gameplay = Empty("Gameplay_Geo", sector);
            Transform hazards = Empty("Hazards_SinLogicaDeDano", sector);
            Transform visuals = Empty("Visual_Background", sector);
            Empty("Visual_Foreground", sector);
            if (mountains)
                MakeMountainSector(i, gameplay, hazards, visuals, palette);
            else
                MakeCastleSector(i, gameplay, hazards, visuals, palette);
        }

        BuildMarkers(markerRoot, mountains, palette);
        BuildBoundaries(root, palette.ground);
        GameObject player = MakePlayer(root, mountains ? 1f : 1f, palette);
        MakeCamera(cameraRoot, player.transform, mountains);
        MakeLights(lightsRoot, mountains);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, path))
        {
            Debug.LogError("No se pudo guardar la escena: " + path);
            return;
        }
        AddToBuildSettings(path);
        AssetDatabase.SaveAssets();
        Debug.Log("Kingdom Shadow: creado " + sceneName + " con 6 sectores de 100 m.");
        EditorUtility.DisplayDialog("Escenario guardado",
            "Se creó " + path + "\n6 sectores, recorrido total: 600 m.\n" +
            "Marcadores, enemigos y peligros requieren integración de gameplay.\n" +
            "Prueba los saltos, rampas y cámara en Play Mode antes de fusionar.",
            "Aceptar");
    }

    private static void MakeMountainSector(int index, Transform g, Transform h,
        Transform v, Palette p)
    {
        // El trazado continúa a través de los bordes de cada sector.
        // Las discontinuidades de 4 m representan los saltos del plano original.
        switch (index)
        {
            case 0:
                Flat(g, "Sendero_de_entrada", 0, 25, 0, p.ground);
                Slope(g, "Primera_rampa_de_hielo", 25, 40, 0, 3, p.ice, p.icePhysics);
                Flat(g, "Cornisa_de_nieve", 40, 70, 3, p.ground);
                Slope(g, "Segunda_subida", 70, 85, 3, 5, p.ice, p.icePhysics);
                Flat(g, "Mirador", 85, 100, 5, p.ground);
                break;
            case 1:
                Flat(g, "Sendero_alto", 0, 28, 5, p.ground);
                Slope(g, "Pendiente_resbaladiza", 28, 45, 5, 8, p.ice, p.icePhysics);
                Flat(g, "Cornisa_01", 45, 68, 8, p.ground);
                Pit(h, "Abismo_01", 68, 72, p.danger);
                Flat(g, "Cornisa_02", 72, 95, 8, p.ground);
                Slope(g, "Subida_a_refugio", 95, 100, 8, 9, p.ice, p.icePhysics);
                Icicles(h, v, 50, 8, 5, p);
                break;
            case 2:
                Flat(g, "Altura_del_refugio", 0, 22, 9, p.ground);
                Slope(g, "Descenso_de_hielo", 22, 42, 9, 4, p.ice, p.icePhysics);
                Flat(g, "Cornisa_03", 42, 60, 4, p.ground);
                Pit(h, "Abismo_02", 60, 64, p.danger);
                Flat(g, "Cornisa_04", 64, 83, 4, p.ground);
                Slope(g, "Rampa_a_cumbres", 83, 100, 4, 7, p.ice, p.icePhysics);
                Icicles(h, v, 52, 4, 4, p);
                break;
            case 3:
                Flat(g, "Pico_cercano", 0, 22, 7, p.ground);
                Slope(g, "Rampa_alta", 22, 44, 7, 11, p.ice, p.icePhysics);
                Flat(g, "Cumbre_01", 44, 70, 11, p.ground);
                Slope(g, "Descenso_entre_picos", 70, 84, 11, 9, p.ice, p.icePhysics);
                Flat(g, "Cumbre_02", 84, 100, 9, p.ground);
                Icicles(h, v, 59, 11, 4, p);
                break;
            case 4:
                Flat(g, "Paso_de_nieve", 0, 24, 9, p.ground);
                Pit(h, "Abismo_03", 24, 28, p.danger);
                Flat(g, "Pasaje_de_hielo", 28, 56, 9, p.ice, physical: p.icePhysics);
                Slope(g, "Subida_final", 56, 76, 9, 12, p.ice, p.icePhysics);
                Flat(g, "Antesala_de_la_arena", 76, 100, 12, p.ground);
                Icicles(h, v, 38, 9, 5, p);
                break;
            case 5:
                Flat(g, "Alto_de_la_arena", 0, 24, 12, p.ground);
                Slope(g, "Bajada_a_arena", 24, 40, 12, 10, p.ice, p.icePhysics);
                Flat(g, "Arena_helada_del_jefe", 40, 100, 10, p.ground, 6f);
                break;
        }
        MountainBackdrop(v, index, p);
    }

    private static void MakeCastleSector(int index, Transform g, Transform h,
        Transform v, Palette p)
    {
        switch (index)
        {
            case 0:
                Flat(g, "Entrada_al_castillo", 0, 30, 0, p.stone, 6f);
                Slope(g, "Escalinata_exterior", 30, 48, 0, 5, p.ramp);
                Flat(g, "Galeria_de_entrada", 48, 78, 5, p.stone);
                Bridge(g, v, "Puente_de_cadenas_01", 78, 100, 5, p);
                Spikes(h, "Espinas_bajo_puente", 82, 98, 2.1f, p);
                break;
            case 1:
                Bridge(g, v, "Puente_de_cadenas_01_continuacion", 0, 10, 5, p);
                Flat(g, "Galeria_superior", 10, 46, 5, p.stone);
                Slope(g, "Descenso_al_patio", 46, 64, 5, 3, p.ramp);
                Flat(g, "Patio_interior", 64, 100, 3, p.stone);
                Spikes(h, "Espinas_del_patio", 81, 85, 3.3f, p);
                break;
            case 2:
                Flat(g, "Patio_elevado", 0, 30, 3, p.stone);
                Slope(g, "Escalinata_alta", 30, 48, 3, 7, p.ramp);
                Flat(g, "Pasillo_alto", 48, 81, 7, p.stone);
                Bridge(g, v, "Puente_de_cadenas_02", 81, 100, 7, p);
                Spikes(h, "Espinas_bajo_puente_02", 86, 98, 4.1f, p);
                break;
            case 3:
                Bridge(g, v, "Puente_de_cadenas_02_continuacion", 0, 20, 7, p);
                Flat(g, "Pasillo_de_torres", 20, 51, 7, p.stone);
                Spikes(h, "Espinas_de_pasillo", 27, 32, 7.3f, p);
                Slope(g, "Descenso_a_sotano", 51, 70, 7, 0, p.ramp);
                Flat(g, "Galeria_inferior", 70, 100, 0, p.stone);
                break;
            case 4:
                Flat(g, "Antesala_inferior", 0, 30, 0, p.stone);
                Slope(g, "Escalera_de_la_antesala", 30, 49, 0, 4, p.ramp);
                Flat(g, "Antesala_superior", 49, 79, 4, p.stone);
                Slope(g, "Acceso_a_la_arena", 79, 100, 4, 1, p.ramp);
                break;
            case 5:
                Flat(g, "Arena_final_del_Guardian", 0, 100, 1, p.stone, 8f);
                break;
        }
        CastleBackdrop(v, index, p);
    }

    private static void BuildMarkers(Transform parent, bool mountains, Palette p)
    {
        Mark(parent, "INICIO", new Vector3(4f, 1f, 0f), p.glow);
        if (mountains)
        {
            Mark(parent, "CP-03A_Checkpoint_Pendiente", new Vector3(208f, 10.2f, 0f), p.glow);
            Mark(parent, "CP-03_Checkpoint_Pendiente", new Vector3(485f, 13.2f, 0f), p.glow);
            Mark(parent, "TR-BOSS_Pendiente", new Vector3(550f, 11.8f, 0f), p.glow);
            Mark(parent, "CRISTAL-03_Pendiente", new Vector3(590f, 11.8f, 0f), p.glow);
        }
        else
        {
            Mark(parent, "CP-04_Checkpoint_Pendiente", new Vector3(512f, 2.3f, 0f), p.glow);
            Mark(parent, "TR-BOSS_Pendiente", new Vector3(550f, 3f, 0f), p.glow);
            Mark(parent, "GUARDIAN_Pendiente", new Vector3(562f, 2.5f, 0f), p.glow);
            Mark(parent, "CRISTAL-04_Pendiente", new Vector3(590f, 2.5f, 0f), p.glow);
        }
    }

    private static void Mark(Transform parent, string name, Vector3 position, Material color)
    {
        Transform anchor = Empty(name, parent);
        anchor.position = position;
        anchor.gameObject.layer = 2;
        BoxCollider trigger = anchor.gameObject.AddComponent<BoxCollider>();
        trigger.size = new Vector3(1.5f, 2f, 2f);
        trigger.isTrigger = true;
        GameObject bubble = Shape(PrimitiveType.Sphere, "Guia_visual_sin_logica", anchor,
            Vector3.zero, Vector3.one * 0.45f, Quaternion.identity, color, false);
        bubble.layer = 2;
    }

    private static void Flat(Transform root, string name, float x1, float x2, float top,
        Material material, float depth = 4f, PhysicsMaterial physical = null)
    {
        if (x2 <= x1) throw new ArgumentException("Tramo plano inválido: " + name);
        GameObject floor = Shape(PrimitiveType.Cube, name, root,
            new Vector3((x1 + x2) * .5f, top - .5f, 0f),
            new Vector3(x2 - x1, 1f, depth), Quaternion.identity, material, true);
        if (physical != null) floor.GetComponent<BoxCollider>().sharedMaterial = physical;
    }

    private static void Slope(Transform root, string name, float x1, float x2,
        float y1, float y2, Material material, PhysicsMaterial physical = null)
    {
        float deltaX = x2 - x1;
        if (deltaX <= 0f) throw new ArgumentException("Rampa inválida: " + name);
        float deltaY = y2 - y1;
        float angle = Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg;
        float length = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        float offsetY = .5f * Mathf.Cos(angle * Mathf.Deg2Rad);
        GameObject ramp = Shape(PrimitiveType.Cube, name, root,
            new Vector3((x1 + x2) * .5f, (y1 + y2) * .5f - offsetY, 0f),
            new Vector3(length, 1f, 4f), Quaternion.Euler(0f, 0f, angle), material, true);
        if (physical != null) ramp.GetComponent<BoxCollider>().sharedMaterial = physical;
    }

    private static void Bridge(Transform root, Transform visuals, string name,
        float x1, float x2, float top, Palette p)
    {
        Flat(root, name, x1, x2, top, p.bridge, 3f);
        // Las cadenas son solo referencias visuales. No colisionan con el jugador.
        for (float x = x1 + 3; x < x2; x += 7f)
            Shape(PrimitiveType.Cube, "Cadena_visual_" + x, visuals,
                new Vector3(x, top + 2f, 2.7f), new Vector3(.15f, 4f, .15f),
                Quaternion.identity, p.decoration, false);
    }

    private static void Pit(Transform root, string name, float x1, float x2, Material p)
    {
        GameObject trigger = Shape(PrimitiveType.Cube, name + "_Trigger_Pendiente", root,
            new Vector3((x1 + x2) * .5f, -4f, 0f),
            new Vector3(x2 - x1, 7f, 4f), Quaternion.identity, p, true);
        trigger.GetComponent<BoxCollider>().isTrigger = true;
        trigger.GetComponent<Renderer>().enabled = false;
        trigger.layer = 2;
    }

    private static void Spikes(Transform root, string name, float x1, float x2,
        float top, Palette p)
    {
        GameObject trigger = Shape(PrimitiveType.Cube, name + "_Trigger_Pendiente", root,
            new Vector3((x1 + x2) * .5f, top, 0f),
            new Vector3(x2 - x1, .65f, 3.5f), Quaternion.identity, p.danger, true);
        trigger.GetComponent<BoxCollider>().isTrigger = true;
        trigger.layer = 2;
    }

    private static void Icicles(Transform hazards, Transform visuals, float x, float groundTop,
        int count, Palette p)
    {
        for (int i = 0; i < count; i++)
        {
            float px = x - count * 1.5f + i * 3f;
            Shape(PrimitiveType.Cylinder, "Carambano_visual_" + i, visuals,
                new Vector3(px, groundTop + 4.2f, 0f),
                new Vector3(.5f, 1.1f, .5f), Quaternion.identity, p.ice, false);
        }
        GameObject trigger = Shape(PrimitiveType.Cube, "Carambanos_Trigger_Pendiente", hazards,
            new Vector3(x, groundTop + 3.1f, 0f),
            new Vector3(count * 3.1f, 2.5f, 4f),
            Quaternion.identity, p.danger, true);
        trigger.GetComponent<BoxCollider>().isTrigger = true;
        trigger.GetComponent<Renderer>().enabled = false;
        trigger.layer = 2;
    }

    private static void MountainBackdrop(Transform root, int sector, Palette p)
    {
        for (int i = 0; i < 3; i++)
        {
            float x = 18f + i * 31f;
            float height = 15f + (sector + i) % 3 * 5f;
            Shape(PrimitiveType.Cube, "Pico_rocoso_" + i, root,
                new Vector3(x, height * .25f + 2f, 10f + i * 2f),
                new Vector3(11f, height, 4f), Quaternion.Euler(0f, 0f, 32f),
                p.decoration, false);
        }
        for (int i = 0; i < 5; i++)
        {
            float x = 10f + i * 21f;
            Shape(PrimitiveType.Cylinder, "Pino_tronco_" + i, root,
                new Vector3(x, 2.1f, 7f), new Vector3(.4f, 2.3f, .4f),
                Quaternion.identity, p.stone, false);
            Shape(PrimitiveType.Capsule, "Pino_copa_" + i, root,
                new Vector3(x, 5f, 7f), new Vector3(2f, 3f, 2f),
                Quaternion.identity, p.decoration, false);
        }
        Empty("Nieve_VFX_Pendiente", root);
    }

    private static void CastleBackdrop(Transform root, int sector, Palette p)
    {
        for (int i = 0; i < 3; i++)
        {
            float x = 14f + i * 34f;
            Shape(PrimitiveType.Cube, "Muro_gotico_" + i, root,
                new Vector3(x, 4.5f, 9f),
                new Vector3(21f, 9f, 1f), Quaternion.identity, p.decoration, false);
        }
        float towerX = 25f + (sector % 2) * 45f;
        Shape(PrimitiveType.Cube, "Torre_del_castillo", root,
            new Vector3(towerX, 10f, 12f),
            new Vector3(10f, 20f, 7f), Quaternion.identity, p.stone, false);
        for (int i = 0; i < 4; i++)
            Shape(PrimitiveType.Cube, "Almena_" + i, root,
                new Vector3(towerX - 3.8f + i * 2.5f, 21f, 12f),
                new Vector3(1.2f, 2f, 7f), Quaternion.identity, p.stone, false);
        if (sector == 0 || sector == 5)
            Shape(PrimitiveType.Cube, "Porton_de_sombras_visual", root,
                new Vector3(sector == 0 ? 8f : 45f, 5f, 5f),
                new Vector3(9f, 10f, 1f), Quaternion.identity, p.bridge, false);
    }

    private static void BuildBoundaries(Transform root, Material material)
    {
        Transform boundaries = Empty("Limites_del_mapa", root);
        Shape(PrimitiveType.Cube, "Inicio_X_0", boundaries, new Vector3(-1f, 6f, 0f),
            new Vector3(2f, 26f, 4f), Quaternion.identity, material, true)
            .GetComponent<Renderer>().enabled = false;
        Shape(PrimitiveType.Cube, "Final_X_600", boundaries, new Vector3(LevelLength + 1f, 6f, 0f),
            new Vector3(2f, 26f, 4f), Quaternion.identity, material, true)
            .GetComponent<Renderer>().enabled = false;
    }

    private static GameObject MakePlayer(Transform root, float initialY, Palette p)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Leo_Blockout_Provisional";
        player.transform.SetParent(root, false);
        player.transform.position = new Vector3(4f, initialY, 0f);
        player.transform.localScale = new Vector3(.8f, .9f, .8f);
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        player.AddComponent<PlayerBlockoutController>();
        player.GetComponent<Renderer>().sharedMaterial = p.glow;
        return player;
    }

    private static void MakeCamera(Transform parent, Transform player, bool mountains)
    {
        GameObject cameraObject = new GameObject("CAM_Main_2_5D", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(parent, false);
        cameraObject.transform.position = player.position + new Vector3(0f, 3f, -14f);
        cameraObject.transform.rotation = Quaternion.Euler(7f, 0f, 0f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = false;
        camera.fieldOfView = 35f;
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 180f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = mountains
            ? new Color(.08f, .14f, .20f)
            : new Color(.035f, .04f, .08f);
        cameraObject.AddComponent<CameraFollow2D>()
            .Configure(player, 4f, LevelLength - 4f, true);
    }

    private static void MakeLights(Transform parent, bool mountains)
    {
        GameObject light = new GameObject("Luz_ambiental", typeof(Light));
        light.transform.SetParent(parent, false);
        light.transform.rotation = Quaternion.Euler(45f, -25f, 0f);
        Light directional = light.GetComponent<Light>();
        directional.type = LightType.Directional;
        directional.intensity = mountains ? 1.1f : .78f;
        directional.color = mountains
            ? new Color(.75f, .85f, 1f)
            : new Color(.53f, .60f, .83f);
    }

    private static Palette MakePalette(string folder, bool mountains)
    {
        Palette p = new Palette
        {
            ground = Material(folder, "Ground", mountains ? new Color(.52f, .59f, .64f) : new Color(.25f, .26f, .32f)),
            ramp = Material(folder, "Ramp", new Color(.35f, .34f, .42f)),
            ice = Material(folder, "Ice", new Color(.54f, .77f, .91f)),
            bridge = Material(folder, "Bridge", new Color(.22f, .18f, .23f)),
            danger = Material(folder, "Danger", new Color(.67f, .12f, .17f)),
            stone = Material(folder, "Stone", mountains ? new Color(.39f, .48f, .57f) : new Color(.28f, .27f, .34f)),
            decoration = Material(folder, "Background", mountains ? new Color(.22f, .34f, .44f) : new Color(.16f, .17f, .23f)),
            glow = Material(folder, "Marker", new Color(.90f, .70f, .23f))
        };
        if (mountains)
        {
            string path = folder + "/PHY_Hielo.physicMaterial";
            p.icePhysics = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (p.icePhysics == null)
            {
                p.icePhysics = new PhysicsMaterial("PHY_Hielo");
                p.icePhysics.dynamicFriction = .05f;
                p.icePhysics.staticFriction = .05f;
                p.icePhysics.bounciness = 0f;
                AssetDatabase.CreateAsset(p.icePhysics, path);
            }
        }
        return p;
    }

    private static Material Material(string folder, string name, Color color)
    {
        string path = folder + "/MAT_" + name + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) throw new InvalidOperationException("No se encontró shader compatible.");
        Material result = new Material(shader) { name = name, color = color };
        AssetDatabase.CreateAsset(result, path);
        return result;
    }

    private static GameObject Shape(PrimitiveType type, string name, Transform parent,
        Vector3 position, Vector3 size, Quaternion rotation, Material material, bool collider)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
        go.transform.localScale = size;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = material;
        if (!collider)
        {
            Collider c = go.GetComponent<Collider>();
            if (c != null) UnityEngine.Object.DestroyImmediate(c);
        }
        return go;
    }

    private static Transform Empty(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene scene in scenes)
            if (string.Equals(scene.path, path, StringComparison.OrdinalIgnoreCase))
                return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
