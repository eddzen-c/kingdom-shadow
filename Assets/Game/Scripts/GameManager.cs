using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Estadísticas del Jugador")]
    public int monedas = 0;
    public int pociones = 0;

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    public void SumarMoneda(int cantidad)
    {
        monedas += cantidad;
        UIManager.instancia.ActualizarMonedas(monedas);
    }

    public void SumarPocion(int cantidad)
    {
        pociones += cantidad;
        UIManager.instancia.ActualizarPociones(pociones);
    }
}