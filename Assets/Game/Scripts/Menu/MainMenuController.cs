using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    private const string GameModeKey = "KingdomShadow.GameMode";

    [Header("Botones")]
    [SerializeField] private Button soloButton;
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button googleButton;

    [Header("Etiquetas")]
    [SerializeField] private Text soloLabel;
    [SerializeField] private Text multiplayerLabel;
    [SerializeField] private Text statusText;

    [Header("Servicios")]
    [SerializeField] private GoogleOAuthClient googleAuth;

    [Header("Escena")]
    [SerializeField] private string preferredGameScene = "PuebloLunaria_600m";
    [SerializeField] private string fallbackGameScene = "PuebloLunaria_Main";

    private int selectedMode;

    private readonly Color selectedButton = new Color(0.055f, 0.10f, 0.18f, 0.97f);
    private readonly Color normalButton = new Color(0.025f, 0.055f, 0.095f, 0.93f);
    private readonly Color selectedText = new Color(0.94f, 0.82f, 0.57f, 1f);
    private readonly Color normalText = new Color(0.82f, 0.85f, 0.91f, 1f);

    public void Configure(
        Button newSoloButton,
        Button newMultiplayerButton,
        Button newGoogleButton,
        Text newSoloLabel,
        Text newMultiplayerLabel,
        Text newStatusText,
        GoogleOAuthClient newGoogleAuth,
        string newPreferredGameScene,
        string newFallbackGameScene)
    {
        soloButton = newSoloButton;
        multiplayerButton = newMultiplayerButton;
        googleButton = newGoogleButton;
        soloLabel = newSoloLabel;
        multiplayerLabel = newMultiplayerLabel;
        statusText = newStatusText;
        googleAuth = newGoogleAuth;
        preferredGameScene = newPreferredGameScene;
        fallbackGameScene = newFallbackGameScene;
    }

    private void Start()
    {
        if (soloButton != null)
            soloButton.onClick.AddListener(PlaySolo);

        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(PlayMultiplayer);

        if (googleButton != null && googleAuth != null)
            googleButton.onClick.AddListener(googleAuth.BeginLogin);

        if (googleAuth != null)
            googleAuth.AuthenticationChanged += OnAuthenticationChanged;

        SelectMode(0);
    }

    private void OnDestroy()
    {
        if (soloButton != null)
            soloButton.onClick.RemoveListener(PlaySolo);

        if (multiplayerButton != null)
            multiplayerButton.onClick.RemoveListener(PlayMultiplayer);

        if (googleButton != null && googleAuth != null)
            googleButton.onClick.RemoveListener(googleAuth.BeginLogin);

        if (googleAuth != null)
            googleAuth.AuthenticationChanged -= OnAuthenticationChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            SelectMode(0);

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            SelectMode(1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedMode == 0)
                PlaySolo();
            else
                PlayMultiplayer();
        }
    }

    public void PlaySolo()
    {
        SelectMode(0);
        PlayerPrefs.SetString(GameModeKey, "Solo");
        PlayerPrefs.Save();
        LoadGameScene();
    }

    public void PlayMultiplayer()
    {
        SelectMode(1);

        if (googleAuth == null || !googleAuth.IsAuthenticated)
        {
            SetStatus("Inicia sesión con Google para entrar a Multijugador.");
            return;
        }

        PlayerPrefs.SetString(GameModeKey, "Multiplayer");
        PlayerPrefs.Save();
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (Application.CanStreamedLevelBeLoaded(preferredGameScene))
        {
            SceneManager.LoadScene(preferredGameScene);
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(fallbackGameScene))
        {
            SceneManager.LoadScene(fallbackGameScene);
            return;
        }

        SetStatus("No encuentro una escena jugable en Build Settings.");
    }

    private void SelectMode(int mode)
    {
        selectedMode = Mathf.Clamp(mode, 0, 1);

        if (soloLabel != null)
        {
            soloLabel.text = selectedMode == 0 ? "◀   SOLITARIO" : "SOLITARIO";
            soloLabel.color = selectedMode == 0 ? selectedText : normalText;
        }

        if (multiplayerLabel != null)
        {
            multiplayerLabel.text = selectedMode == 1 ? "◀   MULTIJUGADOR" : "MULTIJUGADOR";
            multiplayerLabel.color = selectedMode == 1 ? selectedText : normalText;
        }

        SetButtonColor(soloButton, selectedMode == 0 ? selectedButton : normalButton);
        SetButtonColor(multiplayerButton, selectedMode == 1 ? selectedButton : normalButton);
    }

    private void OnAuthenticationChanged(bool authenticated)
    {
        if (authenticated)
            SetStatus("Google conectado. Multijugador desbloqueado.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private static void SetButtonColor(Button button, Color color)
    {
        if (button == null)
            return;

        Image image = button.targetGraphic as Image;

        if (image != null)
            image.color = color;
    }
}
