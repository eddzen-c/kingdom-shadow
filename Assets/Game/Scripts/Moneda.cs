using UnityEngine;

public class Moneda : MonoBehaviour
{
    public int valor = 1; // Esta es la variable que debe salir en el Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Moneda recogida!");
            
            if (GameManager.instancia != null)
            {
                GameManager.instancia.SumarMoneda(valor);
            }
            
            Destroy(gameObject);
        }
    }
}
