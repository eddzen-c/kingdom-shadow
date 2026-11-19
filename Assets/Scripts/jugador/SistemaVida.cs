
using UnityEngine;
using System;
using System.Collections;

// Controla la vida, el daño, la muerte y la reaparición de Leo.
public class SistemaVida : MonoBehaviour
{
    // Configuración de vida del personaje.
    [Header("Configuración de vida")]
    [SerializeField] private int vidaMaxima = 100;

    // Tiempo de espera antes de reaparecer.
    [Header("Configuración de reaparición")]
    [SerializeField] private float tiempoReaparicion = 2f;

    // Guarda la vida actual.
    private int vidaActual;

    // Indica si el personaje está muerto.
    private bool estaMuerto = false;

    // Posición y rotación iniciales del personaje.
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    // Posición y rotación donde debe reaparecer.
    private Vector3 posicionReaparicion;
    private Quaternion rotacionReaparicion;

    // Identificador del último punto activado.
    private string ultimoPuntoControl = "";

    // Componentes del personaje.
    private Rigidbody cuerpo;
    private SistemaPociones sistemaPociones;

    // Referencia al sistema de guardado.
    private SistemaGuardado sistemaGuardado;

    // Datos que pueden consultar otros scripts.
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool EstaMuerto => estaMuerto;
    public string UltimoPuntoControl => ultimoPuntoControl;

    // Prepara los valores iniciales del personaje.
    private void Awake()
    {
        // Evita valores incorrectos.
        vidaMaxima = Mathf.Max(1, vidaMaxima);

        // Comienza con toda la vida.
        vidaActual = vidaMaxima;
        estaMuerto = false;

        // Guarda la posición inicial.
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        // Al principio reaparece donde comenzó.
        posicionReaparicion = posicionInicial;
        rotacionReaparicion = rotacionInicial;

        // Busca los componentes necesarios.
        cuerpo = GetComponent<Rigidbody>();
        sistemaPociones = GetComponent<SistemaPociones>();

        Debug.Log("Vida inicial del personaje: " + vidaActual);
    }

    // Recupera el último punto de control guardado.
    private void Start()
    {
        // Busca el sistema de guardado de la escena.
        // Busca el sistema de guardado sin ordenar los objetos.
        sistemaGuardado =
            FindAnyObjectByType<SistemaGuardado>();
        // Comprueba que los datos estén disponibles.
        if (sistemaGuardado == null ||
            sistemaGuardado.Datos == null)
        {
            return;
        }

        // Consulta el punto de control guardado.
        string identificadorGuardado =
            sistemaGuardado.Datos.ultimoPuntoControl;

        // Si no hay un punto guardado, conserva el inicio.
        if (string.IsNullOrWhiteSpace(identificadorGuardado))
        {
            return;
        }

        
        // Busca todos los puntos de control activos de la escena.
        PuntoControl[] puntos =
            FindObjectsByType<PuntoControl>();

        // Busca el punto que coincide con el guardado.
        foreach (PuntoControl punto in puntos)
        {
            if (punto.Identificador != identificadorGuardado)
            {
                continue;
            }

            // Recupera el lugar donde debe reaparecer.
            ultimoPuntoControl = identificadorGuardado;

            posicionReaparicion =
                punto.transform.position + Vector3.up * 1.2f;

            rotacionReaparicion = transform.rotation;

            Debug.Log(
                "Punto de control recuperado: " +
                ultimoPuntoControl
            );

            return;
        }

        // Avisa si el punto no existe en la escena.
        Debug.LogWarning(
            "No se encontró el punto de control guardado: " +
            identificadorGuardado
        );
    }

    // Registra un nuevo lugar de reaparición.
    public void ActivarPuntoControl(
        string identificador,
        Vector3 posicion,
        Quaternion rotacion)
    {
        // Ignora puntos inválidos o un personaje muerto.
        if (estaMuerto ||
            string.IsNullOrWhiteSpace(identificador))
        {
            return;
        }

        // Evita activar varias veces el mismo punto.
        if (ultimoPuntoControl == identificador)
        {
            return;
        }

        // Actualiza el punto de control.
        ultimoPuntoControl = identificador;
        posicionReaparicion = posicion;
        rotacionReaparicion = rotacion;

        Debug.Log(
            "Punto de control activado: " +
            ultimoPuntoControl
        );

        
        // Busca el sistema de guardado si aún no está disponible.
        if (sistemaGuardado == null)
        {
            sistemaGuardado =
                FindAnyObjectByType<SistemaGuardado>();
        }

        // Guarda automáticamente el punto.
        if (sistemaGuardado != null)
        {
            sistemaGuardado.RegistrarPuntoControl(
                ultimoPuntoControl
            );
        }
        else
        {
            Debug.LogWarning(
                "No se encontró el sistema de guardado."
            );
        }
    }

    // Permite que el personaje reciba daño.
    public void RecibirDano(int cantidad)
    {
        // Ignora daño inválido o si el personaje está muerto.
        if (estaMuerto || cantidad <= 0)
        {
            return;
        }

        // La Esencia Estelar evita todo daño.
        if (sistemaPociones != null &&
            sistemaPociones.EsInvulnerable)
        {
            Debug.Log("¡Daño bloqueado por la Esencia Estelar!");
            return;
        }

        // La resistencia reduce el daño a la mitad.
        if (sistemaPociones != null &&
            sistemaPociones.TieneResistencia)
        {
            cantidad = Mathf.CeilToInt(cantidad * 0.5f);
        }

        // Reduce la vida sin permitir valores negativos.
        vidaActual = Mathf.Max(0, vidaActual - cantidad);

        Debug.Log("Vida actual: " + vidaActual);

        // Comprueba si el personaje murió.
        if (vidaActual == 0)
        {
            Morir();
        }
    }

    // Recupera puntos de vida.
    public void Curar(int cantidad)
    {
        // Impide curarse si está muerto o la cantidad no sirve.
        if (estaMuerto || cantidad <= 0)
        {
            return;
        }

        // Recupera vida sin superar el máximo.
        vidaActual = (int)Math.Min(
            (long)vidaActual + cantidad,
            vidaMaxima
        );

        Debug.Log("Vida recuperada. Vida actual: " + vidaActual);
    }

    // Provoca la muerte instantánea del personaje.
    public void MorirInstantaneamente()
    {
        // Evita repetir la muerte.
        if (estaMuerto)
        {
            return;
        }

        // La Esencia Estelar impide la muerte instantánea.
        if (sistemaPociones != null &&
            sistemaPociones.EsInvulnerable)
        {
            Debug.Log(
                "La Esencia Estelar evitó la muerte instantánea."
            );
            return;
        }

        // Reduce la vida inmediatamente a cero.
        vidaActual = 0;

        Debug.Log("Vida actual: 0");

        // Ejecuta el proceso de muerte.
        Morir();
    }

    // Se ejecuta al morir.
    private void Morir()
    {
        // Marca al personaje como muerto.
        estaMuerto = true;

        // Elimina los efectos de las pociones.
        if (sistemaPociones != null)
        {
            sistemaPociones.LimpiarEfectos();
        }

        Debug.Log("El personaje ha muerto.");

        // Inicia la espera antes de reaparecer.
        StartCoroutine(EsperarReaparicion());
    }

    // Espera antes de hacer reaparecer al personaje.
    private IEnumerator EsperarReaparicion()
    {
        // Evita tiempos negativos.
        float espera = Mathf.Max(0f, tiempoReaparicion);

        yield return new WaitForSeconds(espera);

        Reaparecer();
    }

    // Hace reaparecer a Leo en el último punto registrado.
    public void Reaparecer()
    {
        // Solo modifica velocidades si el cuerpo no es cinemático.
        if (cuerpo != null && !cuerpo.isKinematic)
        {
            cuerpo.linearVelocity = Vector3.zero;
            cuerpo.angularVelocity = Vector3.zero;
        }

        // Coloca al personaje en el punto de reaparición.
        if (cuerpo != null)
        {
            cuerpo.position = posicionReaparicion;
            cuerpo.rotation = rotacionReaparicion;
        }
        else
        {
            transform.SetPositionAndRotation(
                posicionReaparicion,
                rotacionReaparicion
            );
        }

        // Recupera toda la vida.
        vidaActual = vidaMaxima;

        // Permite seguir jugando.
        estaMuerto = false;

        Debug.Log(
            "Leo ha reaparecido con " +
            vidaActual +
            " puntos de vida."
        );

        // Muestra dónde reapareció.
        if (!string.IsNullOrEmpty(ultimoPuntoControl))
        {
            Debug.Log(
                "Reaparición en: " +
                ultimoPuntoControl
            );
        }
        else
        {
            Debug.Log("Reaparición en la posición inicial.");
        }
    }
}
