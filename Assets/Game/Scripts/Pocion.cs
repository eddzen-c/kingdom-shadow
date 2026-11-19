using UnityEngine;

public class Pocion : MonoBehaviour
{
    public int cantidad = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instancia.SumarPocion(cantidad);
            Destroy(gameObject);
        }
    }

}