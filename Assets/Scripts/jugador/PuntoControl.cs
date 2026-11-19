
using UnityEngine;

// Detecta cuándo Leo activa un punto de control.
//DETECTA CUANDO EL PERSONAJA ACTIVA UN PUNTO DE CONTROL 
public class PuntoControl : MonoBehaviour
{
    //IDENTICADOR PARA EL UNICO PUNTO DE CONTROL
    [Header("Identificación")]
    [SerializeField] private string identificador = "PuntoControl_01";

    //CONSULTA EL IDENTIFICADOR 
    public string Identificador => identificador;

    // Detecta cuando el personaje entra en contacto.
    //DETECTA CUANDO EL PERSONAKE ENTRA EN CONTACTO 
    private void OnTriggerEnter(Collider otro)
    {
        // Comprueba si quien entró tiene el sistema de vida.
        //COMPRUEBA EL SISTEMA DE VIDA 
        SistemaVida jugador = otro.GetComponent<SistemaVida>();

        //IGNORA CUALQUIER OBJETO MENOS AL JUGADOR
        if (jugador == null)
        {
            return;
        }

        if (jugador.EstaMuerto)
        {
            return;
        }

        //LUGAR PARA LA REAPARICION EN EL PUNTO DE CONTROL 
        jugador.ActivarPuntoControl(
            identificador,
            transform.position + Vector3.up * 1.2f,
            jugador.transform.rotation
        );
    }
}
