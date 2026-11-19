
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Muestra la vida del personaje en la interfaz.
public class BarraVidaUI : MonoBehaviour
{
    // Referencia al sistema de vida del personaje.
    [Header("Sistema de vida")]
    [SerializeField] private SistemaVida sistemaVida;

    // Elementos visuales de la barra.
    [Header("Elementos de la interfaz")]
    [SerializeField] private Image rellenoBarraVida;
    [SerializeField] private TMP_Text textoVida;

    // Guarda el ancho completo de la barra verde.
    private float anchoMaximo;

    // Obtiene el ancho original del relleno.
    private void Awake()
    {
        if (rellenoBarraVida != null)
        {
            anchoMaximo = rellenoBarraVida.rectTransform.rect.width;
        }
    }

    // Actualiza la interfaz al iniciar el juego.
    private void Start()
    {
        ActualizarBarra();
    }

    // Mantiene la interfaz sincronizada con la vida del personaje.
    private void Update()
    {
        ActualizarBarra();
    }

    // Actualiza el tamaño de la barra y los números de vida.
    private void ActualizarBarra()
    {
        if (sistemaVida == null)
        {
            return;
        }

        int vidaActual = sistemaVida.VidaActual;
        int vidaMaxima = sistemaVida.VidaMaxima;

        // Calcula el porcentaje de vida restante.
        float porcentajeVida = Mathf.Clamp01(
            (float)vidaActual / vidaMaxima
        );

        // Actualiza el ancho de la barra verde.
        if (rellenoBarraVida != null)
        {
            rellenoBarraVida.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                anchoMaximo * porcentajeVida
            );
        }

        // Actualiza los puntos de vida que aparecen en pantalla.
        if (textoVida != null)
        {
            textoVida.text = vidaActual + " / " + vidaMaxima;
        }
    }
}
