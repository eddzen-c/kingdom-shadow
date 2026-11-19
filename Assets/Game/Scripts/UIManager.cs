using UnityEngine;
using UnityEngine.UI; 
public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("HUD Elementos")]
    public Image barraVida; 
    public Text textoMonedas; 
    public Text textoPociones; 

    private void Awake()
    {
        if (instancia == null) instancia = this;
    }

    public void ActualizarVida(float vidaActual, float vidaMaxima)
    {
        barraVida.fillAmount = vidaActual / vidaMaxima; 
    }

    public void ActualizarMonedas(int cantidad)
    {
        textoMonedas.text = "x " + cantidad.ToString();
    }

    public void ActualizarPociones(int cantidad)
    {
        textoPociones.text = "x " + cantidad.ToString();
    }
}