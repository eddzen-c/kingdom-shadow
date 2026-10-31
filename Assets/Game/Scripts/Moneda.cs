using UnityEngine;

public class Moneda : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("¡Algo tocó la moneda! El tag es: " + other.tag);
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Es el jugador! Sumando moneda...");
            // GameManager.instancia.SumarMoneda(1);
            GetComponent<Renderer>().material.color = Color.black; // Se pone negra para probar
        }
    }
}

