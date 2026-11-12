using UnityEngine;

public class CristalManager : MonoBehaviour
{
    public GameObject panelCristal;

    private void OnTriggerEnter(Collider other)
    {
        // Esta línea es la prueba:
        Debug.Log("¡Algo tocó el cristal! El tag es: " + other.tag); 

        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Es el jugador! Abriendo panel...");
            panelCristal.SetActive(true); 
            Time.timeScale = 0f; 
            Destroy(gameObject); 
        }
    }
    // ... el resto de tu código
}