using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public sealed class GoogleOAuthClient : MonoBehaviour
{
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string UserInfoEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";

    [SerializeField] private GoogleOAuthSettings settings;
    [SerializeField] private Text statusText;
    [SerializeField] private Text buttonLabel;

    private readonly ConcurrentQueue<Action> mainThreadActions = new();

    private TcpListener listener;
    private bool loginInProgress;
    private string codeVerifier;
    private string redirectUri;

    public bool IsAuthenticated { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string UserEmail { get; private set; } = string.Empty;

    public event Action<bool> AuthenticationChanged;

    public void Configure(
        GoogleOAuthSettings newSettings,
        Text newStatusText,
        Text newButtonLabel)
    {
        settings = newSettings;
        statusText = newStatusText;
        buttonLabel = newButtonLabel;
    }

    private void Start()
    {
        RefreshButtonLabel();
    }

    private void Update()
    {
        while (mainThreadActions.TryDequeue(out Action action))
            action?.Invoke();
    }

    private void OnDestroy()
    {
        try
        {
            listener?.Stop();
        }
        catch
        {
            // No hay nada que liberar si el listener ya se cerró.
        }
    }

    public async void BeginLogin()
    {
        if (loginInProgress)
            return;

        if (settings == null || string.IsNullOrWhiteSpace(settings.clientId))
        {
            SetStatus("Falta configurar el Client ID de Google OAuth.");
            return;
        }

        int port = Mathf.Clamp(settings.loopbackPort, 1024, 65535);
        redirectUri = $"http://127.0.0.1:{port}/";

        string state = CreateRandomUrlSafeString(32);
        codeVerifier = CreateRandomUrlSafeString(64);
        string codeChallenge = CreateCodeChallenge(codeVerifier);

        string scope = settings.scopes != null && settings.scopes.Length > 0
            ? string.Join(" ", settings.scopes)
            : "openid email profile";

        string authorizationUrl =
            AuthorizationEndpoint +
            "?client_id=" + Escape(settings.clientId.Trim()) +
            "&redirect_uri=" + Escape(redirectUri) +
            "&response_type=code" +
            "&scope=" + Escape(scope) +
            "&code_challenge=" + Escape(codeChallenge) +
            "&code_challenge_method=S256" +
            "&state=" + Escape(state) +
            "&prompt=select_account";

        try
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            loginInProgress = true;

            SetStatus("Abriendo Google en tu navegador...");
            Application.OpenURL(authorizationUrl);

            string authorizationCode = await WaitForAuthorizationCodeAsync(state);

            mainThreadActions.Enqueue(() =>
            {
                StartCoroutine(ExchangeAuthorizationCode(authorizationCode));
            });
        }
        catch (Exception ex)
        {
            mainThreadActions.Enqueue(() =>
            {
                loginInProgress = false;
                SetStatus("No se pudo iniciar sesión con Google: " + ShortMessage(ex.Message));
            });
        }
        finally
        {
            try
            {
                listener?.Stop();
            }
            catch
            {
                // Ignorado intencionalmente.
            }

            listener = null;
        }
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        UserName = string.Empty;
        UserEmail = string.Empty;
        PlayerPrefs.DeleteKey("KingdomShadow.GoogleDisplayName");
        PlayerPrefs.DeleteKey("KingdomShadow.GoogleEmail");
        PlayerPrefs.Save();
        RefreshButtonLabel();
        SetStatus("Sesión de Google cerrada.");
        AuthenticationChanged?.Invoke(false);
    }

    private async Task<string> WaitForAuthorizationCodeAsync(string expectedState)
    {
        using TcpClient client = await listener.AcceptTcpClientAsync();
        using NetworkStream stream = client.GetStream();
        using StreamReader reader = new StreamReader(
            stream,
            Encoding.UTF8,
            false,
            4096,
            leaveOpen: true);

        string requestLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(requestLine))
            throw new InvalidOperationException("Google no devolvió una respuesta válida.");

        string[] requestParts = requestLine.Split(' ');

        if (requestParts.Length < 2)
            throw new InvalidOperationException("Respuesta OAuth inválida.");

        string target = requestParts[1];

        string line;
        do
        {
            line = await reader.ReadLineAsync();
        }
        while (!string.IsNullOrEmpty(line));

        Dictionary<string, string> query = ParseQuery(target);

        string html;

        if (query.TryGetValue("error", out string oauthError))
        {
            html = HtmlResponse(
                "Inicio de sesión cancelado",
                "Puedes cerrar esta ventana y volver al juego.");

            await WriteHttpResponseAsync(stream, html);
            throw new InvalidOperationException(oauthError);
        }

        if (!query.TryGetValue("state", out string returnedState) ||
            !string.Equals(returnedState, expectedState, StringComparison.Ordinal))
        {
            html = HtmlResponse(
                "No se pudo validar la sesión",
                "Puedes cerrar esta ventana y volver al juego.");

            await WriteHttpResponseAsync(stream, html);
            throw new InvalidOperationException("El estado OAuth no coincide.");
        }

        if (!query.TryGetValue("code", out string code) || string.IsNullOrWhiteSpace(code))
        {
            html = HtmlResponse(
                "No se recibió autorización",
                "Puedes cerrar esta ventana y volver al juego.");

            await WriteHttpResponseAsync(stream, html);
            throw new InvalidOperationException("Google no devolvió un código de autorización.");
        }

        html = HtmlResponse(
            "Inicio de sesión recibido",
            "Regresa a El Reino de las Sombras. Ya puedes cerrar esta ventana.");

        await WriteHttpResponseAsync(stream, html);
        return code;
    }

    private IEnumerator ExchangeAuthorizationCode(string authorizationCode)
    {
        SetStatus("Validando tu cuenta de Google...");

        string body =
            "client_id=" + Escape(settings.clientId.Trim()) +
            "&code=" + Escape(authorizationCode) +
            "&code_verifier=" + Escape(codeVerifier) +
            "&grant_type=authorization_code" +
            "&redirect_uri=" + Escape(redirectUri);

        using UnityWebRequest request = new UnityWebRequest(TokenEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            loginInProgress = false;
            SetStatus("Google rechazó la autenticación. Revisa el Client ID.");
            yield break;
        }

        TokenResponse token = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);

        if (token == null || string.IsNullOrWhiteSpace(token.access_token))
        {
            loginInProgress = false;
            SetStatus("Google no devolvió un token de acceso válido.");
            yield break;
        }

        yield return FetchUserInfo(token.access_token);
    }

    private IEnumerator FetchUserInfo(string accessToken)
    {
        using UnityWebRequest request = UnityWebRequest.Get(UserInfoEndpoint);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        loginInProgress = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            SetStatus("La cuenta se autenticó, pero no pude leer tu perfil.");
            yield break;
        }

        UserInfo info = JsonUtility.FromJson<UserInfo>(request.downloadHandler.text);

        if (info == null || string.IsNullOrWhiteSpace(info.email))
        {
            SetStatus("Google no devolvió un perfil válido.");
            yield break;
        }

        IsAuthenticated = true;
        UserName = string.IsNullOrWhiteSpace(info.name) ? info.email : info.name;
        UserEmail = info.email;

        PlayerPrefs.SetString("KingdomShadow.GoogleDisplayName", UserName);
        PlayerPrefs.SetString("KingdomShadow.GoogleEmail", UserEmail);
        PlayerPrefs.Save();

        RefreshButtonLabel();
        SetStatus("Sesión iniciada como " + UserName + ".");
        AuthenticationChanged?.Invoke(true);
    }

    private void RefreshButtonLabel()
    {
        if (buttonLabel == null)
            return;

        buttonLabel.text = IsAuthenticated
            ? "✓  " + UserName.ToUpperInvariant()
            : "G   CONTINUAR CON GOOGLE";
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private static string CreateRandomUrlSafeString(int byteCount)
    {
        byte[] bytes = new byte[byteCount];

        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return Base64Url(bytes);
    }

    private static string CreateCodeChallenge(string verifier)
    {
        using SHA256 sha = SHA256.Create();
        byte[] digest = sha.ComputeHash(Encoding.ASCII.GetBytes(verifier));
        return Base64Url(digest);
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string Escape(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }

    private static Dictionary<string, string> ParseQuery(string target)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        int question = target.IndexOf('?');
        if (question < 0 || question >= target.Length - 1)
            return result;

        string query = target.Substring(question + 1);
        string[] pairs = query.Split('&');

        foreach (string pair in pairs)
        {
            if (string.IsNullOrWhiteSpace(pair))
                continue;

            string[] parts = pair.Split(new[] { '=' }, 2);
            string key = Decode(parts[0]);
            string value = parts.Length > 1 ? Decode(parts[1]) : string.Empty;
            result[key] = value;
        }

        return result;
    }

    private static string Decode(string value)
    {
        return Uri.UnescapeDataString((value ?? string.Empty).Replace("+", " "));
    }

    private static async Task WriteHttpResponseAsync(NetworkStream stream, string html)
    {
        byte[] content = Encoding.UTF8.GetBytes(html);
        string headers =
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/html; charset=utf-8\r\n" +
            "Content-Length: " + content.Length + "\r\n" +
            "Connection: close\r\n\r\n";

        byte[] headerBytes = Encoding.UTF8.GetBytes(headers);
        await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
        await stream.WriteAsync(content, 0, content.Length);
        await stream.FlushAsync();
    }

    private static string HtmlResponse(string title, string body)
    {
        return
            "<!doctype html><html><head><meta charset=\"utf-8\">" +
            "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" +
            "<title>" + title + "</title>" +
            "<style>body{margin:0;background:#07101d;color:#f4e7c7;font-family:Arial,sans-serif;" +
            "display:grid;place-items:center;min-height:100vh}main{max-width:620px;padding:48px;" +
            "text-align:center}h1{color:#d8ad59}p{color:#c9d0dd;font-size:18px}</style></head>" +
            "<body><main><h1>" + title + "</h1><p>" + body + "</p></main></body></html>";
    }

    private static string ShortMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "error desconocido";

        return message.Length <= 90 ? message : message.Substring(0, 90) + "...";
    }

    [Serializable]
    private sealed class TokenResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string scope;
        public string id_token;
    }

    [Serializable]
    private sealed class UserInfo
    {
        public string sub;
        public string name;
        public string given_name;
        public string family_name;
        public string picture;
        public string email;
        public bool email_verified;
    }
}
