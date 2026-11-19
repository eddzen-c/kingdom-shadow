
using UnityEngine;

// Define los nueve tipos de pociones del juego.
public enum TipoPocion
{
    Curacion,
    Veneno,
    MuerteInstantanea,
    Resistencia,
    Velocidad,
    Regeneracion,
    Antidoto,
    Misteriosa,
    EsenciaEstelar
}

// Administra las pociones y sus efectos sobre Leo.
public class SistemaPociones : MonoBehaviour
{
    [Header("Sistema de vida")]
    [SerializeField] private SistemaVida sistemaVida;

    [Header("Inventario de pociones")]
    [SerializeField]
    private int[] cantidades =
    {
        3, 2, 1, 1, 1, 1, 1, 1, 1
    };

    [Header("Poción seleccionada para pruebas")]
    [SerializeField] private TipoPocion pocionSeleccionada;

    // Guarda cuánto tiempo queda de cada efecto.
    private float tiempoVeneno;
    private float tiempoRegeneracion;
    private float tiempoResistencia;
    private float tiempoVelocidad;
    private float tiempoInmunidad;

    // Controla los segundos para aplicar daño o curación.
    private float contadorVeneno;
    private float contadorRegeneracion;

    // Permite que otros sistemas consulten los efectos.
    public bool EsInvulnerable => tiempoInmunidad > 0f;
    public bool TieneResistencia => tiempoResistencia > 0f;
    public bool EstaEnvenenado => tiempoVeneno > 0f;

    // El controlador de movimiento podrá consultar este valor.
    public float MultiplicadorVelocidad =>
        tiempoVelocidad > 0f ? 1.4f : 1f;

    // Permite consultar el tiempo restante de inmunidad.
    public float TiempoInmunidad => tiempoInmunidad;

    // Prepara el sistema de pociones.
    private void Awake()
    {
        if (sistemaVida == null)
        {
            sistemaVida = GetComponent<SistemaVida>();
        }

        Debug.Log("Sistema de pociones preparado.");
    }

    // Actualiza los efectos temporales.
    private void Update()
    {
        if (sistemaVida == null || sistemaVida.EstaMuerto)
        {
            return;
        }

        float delta = Time.deltaTime;

        // Aplica el daño progresivo del veneno.
        if (tiempoVeneno > 0f)
        {
            float transcurrido = Mathf.Min(delta, tiempoVeneno);
            tiempoVeneno = Mathf.Max(0f, tiempoVeneno - delta);
            contadorVeneno += transcurrido;

            while (contadorVeneno >= 1f)
            {
                contadorVeneno -= 1f;
                sistemaVida.RecibirDano(5);

                if (sistemaVida.EstaMuerto)
                {
                    return;
                }
            }
        }

        // Recupera vida progresivamente.
        if (tiempoRegeneracion > 0f)
        {
            float transcurrido =
                Mathf.Min(delta, tiempoRegeneracion);

            tiempoRegeneracion =
                Mathf.Max(0f, tiempoRegeneracion - delta);

            contadorRegeneracion += transcurrido;

            while (contadorRegeneracion >= 1f)
            {
                contadorRegeneracion -= 1f;
                sistemaVida.Curar(5);
            }
        }

        // Reduce el tiempo restante de los demás efectos.
        tiempoResistencia =
            Mathf.Max(0f, tiempoResistencia - delta);

        tiempoVelocidad =
            Mathf.Max(0f, tiempoVelocidad - delta);

        if (tiempoInmunidad > 0f)
        {
            tiempoInmunidad =
                Mathf.Max(0f, tiempoInmunidad - delta);

            if (tiempoInmunidad == 0f)
            {
                Debug.Log("La Esencia Estelar terminó.");
            }
        }
    }

    // Permite utilizar una poción del inventario.
    public void UsarPocion(TipoPocion tipo)
    {
        if (sistemaVida == null || sistemaVida.EstaMuerto)
        {
            Debug.Log("Leo no puede utilizar pociones.");
            return;
        }

        int indice = (int)tipo;

        if (cantidades == null ||
            indice < 0 ||
            indice >= cantidades.Length ||
            cantidades[indice] <= 0)
        {
            Debug.Log("No quedan pociones de este tipo.");
            return;
        }

        // Evita gastar curación con la vida completa.
        if (tipo == TipoPocion.Curacion &&
            sistemaVida.VidaActual == sistemaVida.VidaMaxima)
        {
            Debug.Log("Leo ya tiene toda su vida.");
            return;
        }

        // Evita gastar regeneración con la vida completa.
        if (tipo == TipoPocion.Regeneracion &&
            sistemaVida.VidaActual == sistemaVida.VidaMaxima)
        {
            Debug.Log("Leo ya tiene toda su vida.");
            return;
        }

        // Evita gastar antídotos sin envenenamiento.
        if (tipo == TipoPocion.Antidoto && !EstaEnvenenado)
        {
            Debug.Log("Leo no está envenenado.");
            return;
        }

        // La inmunidad no se acumula.
        if (tipo == TipoPocion.EsenciaEstelar && EsInvulnerable)
        {
            Debug.Log("Leo ya tiene inmunidad activa.");
            return;
        }

        // Descuenta una poción del inventario.
        cantidades[indice]--;

        // Aplica su efecto correspondiente.
        AplicarEfecto(tipo);

        Debug.Log(
            "Poción utilizada: " + tipo +
            ". Restantes: " + cantidades[indice]
        );
    }

    // Ejecuta el efecto de cada poción.
    private void AplicarEfecto(TipoPocion tipo)
    {
        switch (tipo)
        {
            case TipoPocion.Curacion:
                sistemaVida.Curar(40);
                break;

            case TipoPocion.Veneno:
                tiempoVeneno = 6f;
                contadorVeneno = 0f;
                Debug.Log("Leo está envenenado durante 6 segundos.");
                break;

            case TipoPocion.MuerteInstantanea:
                sistemaVida.MorirInstantaneamente();
                break;

            case TipoPocion.Resistencia:
                tiempoResistencia = 10f;
                Debug.Log("Resistencia activada durante 10 segundos.");
                break;

            case TipoPocion.Velocidad:
                tiempoVelocidad = 8f;
                Debug.Log("Velocidad aumentada durante 8 segundos.");
                break;

            case TipoPocion.Regeneracion:
                tiempoRegeneracion = 10f;
                contadorRegeneracion = 0f;
                Debug.Log("Regeneración activada durante 10 segundos.");
                break;

            case TipoPocion.Antidoto:
                tiempoVeneno = 0f;
                contadorVeneno = 0f;
                Debug.Log("El veneno fue eliminado.");
                break;

            case TipoPocion.Misteriosa:
                // Selecciona uno de cinco efectos.
                int efecto = Random.Range(0, 5);

                TipoPocion resultado = efecto switch
                {
                    0 => TipoPocion.Curacion,
                    1 => TipoPocion.Veneno,
                    2 => TipoPocion.Resistencia,
                    3 => TipoPocion.Velocidad,
                    _ => TipoPocion.Regeneracion
                };

                Debug.Log("La poción misteriosa produjo: " + resultado);
                AplicarEfecto(resultado);
                break;

            case TipoPocion.EsenciaEstelar:
                tiempoInmunidad = 60f;
                Debug.Log("¡Leo es invulnerable durante 60 segundos!");
                break;
        }
    }

    // Permite recoger nuevas pociones.
    public void AgregarPociones(TipoPocion tipo, int cantidad)
    {
        int indice = (int)tipo;

        if (cantidades == null ||
            indice < 0 ||
            indice >= cantidades.Length ||
            cantidad <= 0)
        {
            return;
        }

        cantidades[indice] = (int)Mathf.Min(
            (long)cantidades[indice] + cantidad,
            int.MaxValue
        );
    }

    // Elimina todos los efectos al morir.
    public void LimpiarEfectos()
    {
        tiempoVeneno = 0f;
        tiempoRegeneracion = 0f;
        tiempoResistencia = 0f;
        tiempoVelocidad = 0f;
        tiempoInmunidad = 0f;

        contadorVeneno = 0f;
        contadorRegeneracion = 0f;
    }

    // Prueba una poción desde el Inspector.
    [ContextMenu("Pruebas/Usar poción seleccionada")]
    private void ProbarPocion()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("Primero inicia el modo Play.");
            return;
        }

        UsarPocion(pocionSeleccionada);
    }

    // Permite probar el daño sin crear enemigos.
    [ContextMenu("Pruebas/Recibir 25 puntos de daño")]
    private void ProbarDano()
    {
        if (Application.isPlaying && sistemaVida != null)
        {
            sistemaVida.RecibirDano(25);
        }
    }
}
