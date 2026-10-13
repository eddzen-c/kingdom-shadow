#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/Game/Scenes/MainMenu.unity";
    private const string UiFolder = "Assets/Game/Art/UI/MainMenu";
    private const string SettingsFolder = "Assets/Game/Settings";
    private const string GoogleSettingsPath = SettingsFolder + "/GoogleOAuthSettings.asset";

    [MenuItem("Tools/Kingdom Shadow/Build Main Menu")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder("Assets/Game");
        EnsureFolder("Assets/Game/Scenes");
        EnsureFolder("Assets/Game/Art");
        EnsureFolder("Assets/Game/Art/UI");
        EnsureFolder(UiFolder);
        EnsureFolder(SettingsFolder);

        Texture2D background = FindBackgroundTexture();
        if (background == null)
        {
            EditorUtility.DisplayDialog(
                "Falta la imagen del menú",
                "Coloca la imagen de fondo en:\n\n" + UiFolder +
                "\n\nNombre recomendado:\nfondo_menu_principal_1920x1080.png\n\n" +
                "Después vuelve a ejecutar Tools > Kingdom Shadow > Build Main Menu.",
                "Aceptar");
            return;
        }

        GoogleOAuthSettings oauthSettings = LoadOrCreateGoogleSettings();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera cam = cameraObject.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        GameObject canvasObject = new GameObject(
            "Canvas_MainMenu",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RawImage backgroundImage = CreateRawImage(
            "Background",
            canvasObject.transform,
            background,
            Vector2.zero,
            new Vector2(1920f, 1080f));
        StretchFull(backgroundImage.rectTransform);

        Image leftShade = CreatePanel(
            "Left_Shadow",
            canvasObject.transform,
            new Color(0.015f, 0.035f, 0.065f, 0.86f),
            new Vector2(0f, 0f),
            new Vector2(690f, 1080f));
        SetTopLeft(leftShade.rectTransform, 0f, 0f, 690f, 1080f);

        Image gradientSoftener = CreatePanel(
            "Left_Shadow_Softener",
            canvasObject.transform,
            new Color(0.015f, 0.035f, 0.065f, 0.25f),
            new Vector2(0f, 0f),
            new Vector2(180f, 1080f));
        SetTopLeft(gradientSoftener.rectTransform, 650f, 0f, 180f, 1080f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Text titleSmall = CreateText(
            "Title_Small",
            canvasObject.transform,
            "E L   R E I N O   D E   L A S",
            font,
            34,
            FontStyle.Bold,
            new Color(0.95f, 0.93f, 0.88f, 1f),
            TextAnchor.MiddleLeft);
        SetTopLeft(titleSmall.rectTransform, 92f, 78f, 560f, 58f);

        Text titleLarge = CreateText(
            "Title_Large",
            canvasObject.transform,
            "SOMBRAS",
            font,
            72,
            FontStyle.Bold,
            new Color(0.88f, 0.69f, 0.35f, 1f),
            TextAnchor.MiddleLeft);
        SetTopLeft(titleLarge.rectTransform, 86f, 126f, 550f, 96f);

        Image divider = CreatePanel(
            "Divider",
            canvasObject.transform,
            new Color(0.83f, 0.65f, 0.33f, 0.95f),
            Vector2.zero,
            new Vector2(470f, 2f));
        SetTopLeft(divider.rectTransform, 100f, 250f, 470f, 2f);

        Text subtitle = CreateText(
            "Subtitle",
            canvasObject.transform,
            "S E L E C C I O N A   U N   M O D O   D E   J U E G O",
            font,
            18,
            FontStyle.Normal,
            new Color(0.82f, 0.84f, 0.88f, 1f),
            TextAnchor.MiddleLeft);
        SetTopLeft(subtitle.rectTransform, 102f, 295f, 520f, 40f);

        Button solo = CreateModeButton(
            "Button_Solo",
            canvasObject.transform,
            "◀   SOLITARIO",
            font,
            90f,
            365f,
            true,
            out Text soloLabel);

        Button multiplayer = CreateModeButton(
            "Button_Multiplayer",
            canvasObject.transform,
            "MULTIJUGADOR",
            font,
            90f,
            492f,
            false,
            out Text multiplayerLabel);

        Button google = CreateGoogleButton(
            canvasObject.transform,
            font,
            90f,
            625f,
            out Text googleLabel);

        Text status = CreateText(
            "Google_Status",
            canvasObject.transform,
            "Google se usa para identificar tu cuenta en Multijugador.",
            font,
            15,
            FontStyle.Normal,
            new Color(0.70f, 0.74f, 0.82f, 1f),
            TextAnchor.MiddleLeft);
        SetTopLeft(status.rectTransform, 92f, 695f, 520f, 34f);

        Text controls = CreateText(
            "Controls",
            canvasObject.transform,
            "◀ ▶   CAMBIAR OPCIÓN\n\nENTER   SELECCIONAR",
            font,
            15,
            FontStyle.Normal,
            new Color(0.72f, 0.75f, 0.82f, 1f),
            TextAnchor.UpperLeft);
        controls.lineSpacing = 1.1f;
        SetTopLeft(controls.rectTransform, 92f, 760f, 390f, 90f);

        Text footer = CreateText(
            "Footer",
            canvasObject.transform,
            "L A   L U Z   D E   L U N A R I A   T E   E S P E R A",
            font,
            14,
            FontStyle.Normal,
            new Color(0.58f, 0.61f, 0.69f, 1f),
            TextAnchor.MiddleCenter);
        SetTopLeft(footer.rectTransform, 120f, 965f, 500f, 40f);

        GameObject authObject = new GameObject("GoogleOAuth", typeof(GoogleOAuthClient));
        GoogleOAuthClient auth = authObject.GetComponent<GoogleOAuthClient>();
        auth.Configure(oauthSettings, status, googleLabel);

        GameObject controllerObject = new GameObject("MainMenuController", typeof(MainMenuController));
        MainMenuController controller = controllerObject.GetComponent<MainMenuController>();
        controller.Configure(
            solo,
            multiplayer,
            google,
            soloLabel,
            multiplayerLabel,
            status,
            auth,
            "PuebloLunaria_600m");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        ConfigureBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = canvasObject;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

        EditorUtility.DisplayDialog(
            "Menú principal creado",
            "Se creó:\n\n" + ScenePath +
            "\n\nLa composición replica la referencia y añade 'Continuar con Google' entre Multijugador y los controles.\n\n" +
            "Solo falta colocar tu OAuth Client ID en:\n" + GoogleSettingsPath,
            "Aceptar");
    }

    [MenuItem("Tools/Kingdom Shadow/Open Main Menu")]
    public static void Open()
    {
        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog(
                "Menú no generado",
                "Ejecuta primero Tools > Kingdom Shadow > Build Main Menu.",
                "Aceptar");
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static Texture2D FindBackgroundTexture()
    {
        string[] guids = AssetDatabase.FindAssets("fondo_menu_principal_1920x1080 t:Texture2D", new[] { UiFolder });

        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("fondo_menu_principal t:Texture2D", new[] { UiFolder });

        if (guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static GoogleOAuthSettings LoadOrCreateGoogleSettings()
    {
        GoogleOAuthSettings settings = AssetDatabase.LoadAssetAtPath<GoogleOAuthSettings>(GoogleSettingsPath);

        if (settings != null)
            return settings;

        settings = ScriptableObject.CreateInstance<GoogleOAuthSettings>();
        AssetDatabase.CreateAsset(settings, GoogleSettingsPath);
        return settings;
    }

    private static Button CreateModeButton(
        string name,
        Transform parent,
        string labelText,
        Font font,
        float x,
        float y,
        bool selected,
        out Text label)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = selected
            ? new Color(0.055f, 0.10f, 0.18f, 0.97f)
            : new Color(0.025f, 0.055f, 0.095f, 0.93f);

        Outline outline = buttonObject.GetComponent<Outline>();
        outline.effectColor = selected
            ? new Color(0.88f, 0.69f, 0.35f, 1f)
            : new Color(0.42f, 0.48f, 0.60f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.96f);
        colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
        colors.selectedColor = Color.white;
        button.colors = colors;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetTopLeft(rect, x, y, 510f, 100f);

        label = CreateText(
            "Label",
            buttonObject.transform,
            labelText,
            font,
            34,
            FontStyle.Bold,
            selected
                ? new Color(0.94f, 0.82f, 0.57f, 1f)
                : new Color(0.82f, 0.85f, 0.91f, 1f),
            TextAnchor.MiddleCenter);
        StretchFull(label.rectTransform);
        label.raycastTarget = false;

        return button;
    }

    private static Button CreateGoogleButton(
        Transform parent,
        Font font,
        float x,
        float y,
        out Text label)
    {
        GameObject buttonObject = new GameObject(
            "Button_Google",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(Outline));

        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.96f, 0.97f, 0.99f, 0.98f);

        Outline outline = buttonObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.50f, 0.55f, 0.64f, 1f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetTopLeft(rect, x, y, 510f, 70f);

        Text googleG = CreateText(
            "Google_G",
            buttonObject.transform,
            "G",
            font,
            27,
            FontStyle.Bold,
            new Color(0.20f, 0.45f, 0.85f, 1f),
            TextAnchor.MiddleCenter);
        SetCentered(googleG.rectTransform, -190f, 0f, 44f, 50f);
        googleG.raycastTarget = false;

        label = CreateText(
            "Label",
            buttonObject.transform,
            "CONTINUAR CON GOOGLE",
            font,
            21,
            FontStyle.Bold,
            new Color(0.16f, 0.19f, 0.25f, 1f),
            TextAnchor.MiddleCenter);
        SetCentered(label.rectTransform, 24f, 0f, 420f, 60f);
        label.raycastTarget = false;

        return buttonObject.GetComponent<Button>();
    }

    private static RawImage CreateRawImage(
        string name,
        Transform parent,
        Texture texture,
        Vector2 position,
        Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);
        RawImage image = go.GetComponent<RawImage>();
        image.texture = texture;
        image.color = Color.white;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return image;
    }

    private static Image CreatePanel(
        string name,
        Transform parent,
        Color color,
        Vector2 position,
        Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return image;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        string text,
        Font font,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text label = go.GetComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.color = color;
        label.alignment = alignment;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetCentered(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void ConfigureBuildSettings()
    {
        string[] allowedScenes =
        {
            ScenePath,
            "Assets/Game/Scenes/Levels/PuebloLunaria_600m.unity",
            "Assets/Game/Scenes/Levels/BosqueEncantado_600m.unity"
        };

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        foreach (string path in allowedScenes)
        {
            if (File.Exists(path))
                scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
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
}
#endif
