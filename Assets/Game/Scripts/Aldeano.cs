using UnityEngine;

public class Aldeano : MonoBehaviour
{
    public string nombreAldeano = "Lupita";
    
    [TextArea(3, 5)] // Esto hace que la caja de texto en Unity sea grande y fácil de leer
    public string[] dialogos;

    private bool enRango = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enRango = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enRango = false;
        }
    }

    void Update()
    {
        // Si el jugador está cerca y presiona la tecla E
        if (enRango && Input.GetKeyDown(KeyCode.E))
        {
            // Busca el sistema de diálogos en la escena y lo activa
           FindAnyObjectByType<SistemaDialogos>().IniciarDialogo(nombreAldeano, dialogos);
        }
    }
}