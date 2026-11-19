
using UnityEngine;

// Permite que el personaje recoja el primer cristal.
public class PrimerCristal : MonoBehaviour
{
    // Referencia al sistema de guardado.
    [Header("Sistema de guardado")]
    [SerializeField] private SistemaGuardado sistemaGuardado;

    // Busca automáticamente el sistema de guardado.
    private void Awake()
    {
        // Solo lo busca si todavía no está conectado.
        if (sistemaGuardado == null)
        {
            sistemaGuardado = FindAnyObjectByType<SistemaGuardado>();
        }
    }
    // Comprueba si el cristal ya fue obtenido anteriormente.
    private void Start()
    {
        // Verifica que exista el sistema de guardado.
        if (sistemaGuardado == null)
        {
            Debug.LogWarning("No se encontró el sistema de guardado.");
            return;
        }

        // Si Leo ya obtuvo el cristal, lo elimina de la escena.
        if (sistemaGuardado.Datos.primerCristalObtenido)
        {
            Debug.Log("El primer cristal ya fue obtenido en esta partida.");

            Destroy(gameObject);
        }
    }

    // Comprueba si el jugador toca el cristal.
    private void OnTriggerEnter(Collider otro)
    {
        // Comprueba que el objeto tenga el sistema de vida de Leo.
        SistemaVida jugador = otro.GetComponent<SistemaVida>();

        if (jugador == null)
        {
            return;
        }

        // Impide recoger el cristal si el personaje está muerto.
        if (jugador.EstaMuerto)
        {
            return;
        }

        // Comprueba que el sistema de guardado esté conectado.
        if (sistemaGuardado == null)
        {
            Debug.LogWarning("No se encontró el sistema de guardado.");
            return;
        }
        // Muestra qué objeto ha tocado el cristal.
        Debug.Log("El cristal detectó contacto con: " + otro.name);

        // Registra la obtención del primer cristal.
        sistemaGuardado.RegistrarPrimerCristal();

        // Elimina el cristal de la escena después de recogerlo.
        Destroy(gameObject);
    }
}
