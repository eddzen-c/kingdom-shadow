using UnityEngine;

[CreateAssetMenu(
    fileName = "GoogleOAuthSettings",
    menuName = "Kingdom Shadow/Auth/Google OAuth Settings")]
public sealed class GoogleOAuthSettings : ScriptableObject
{
    [Tooltip("OAuth 2.0 Client ID de tipo Desktop app creado en Google Cloud.")]
    public string clientId = string.Empty;

    [Tooltip("Puerto local usado para recibir el callback de Google.")]
    [Range(1024, 65535)]
    public int loopbackPort = 53682;

    [Tooltip("Permisos mínimos solicitados a Google.")]
    public string[] scopes = { "openid", "email", "profile" };
}
