
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Controla el movimiento provisional de Leo en un escenario 2.5D.
[RequireComponent(typeof(Rigidbody))]
public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 5f;

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 7f;
    [SerializeField] private float gravedad = 18f;

    [Header("Detección de suelo")]
    [SerializeField] private float distanciaSuelo = 1.2f;
    [SerializeField] private LayerMask capasSuelo = ~0;

    // Componentes que ya tiene el personaje.
    private Rigidbody cuerpo;
    private SistemaVida sistemaVida;
    private SistemaPociones sistemaPociones;

    // Guarda el movimiento y la velocidad vertical.
    private float direccionHorizontal;
    private float velocidadVertical;

    // Indica si el jugador solicitó saltar.
    private bool saltoSolicitado;

    // Indica si el personaje está sobre una superficie.
    private bool estaEnSuelo;

    private void Awake()
    {
        // Busca los componentes del personaje.
        cuerpo = GetComponent<Rigidbody>();
        sistemaVida = GetComponent<SistemaVida>();
        sistemaPociones = GetComponent<SistemaPociones>();

        // Este controlador utiliza movimiento cinemático.
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;

        // Evita rotaciones accidentales por la física.
        cuerpo.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        // Impide mover al personaje cuando está muerto.
        if (sistemaVida != null && sistemaVida.EstaMuerto)
        {
            direccionHorizontal = 0f;
            velocidadVertical = 0f;
            saltoSolicitado = false;
            return;
        }

        // Lee las teclas de movimiento.
        LeerControles();

        // Comprueba si el personaje está sobre el suelo.
        ComprobarSuelo();

        // Prepara el salto si el jugador está en el suelo.
        if (saltoSolicitado && estaEnSuelo)
        {
            velocidadVertical = fuerzaSalto;
            estaEnSuelo = false;
        }

        saltoSolicitado = false;
    }

    private void FixedUpdate()
    {
        // No permite mover a un personaje muerto.
        if (sistemaVida != null && sistemaVida.EstaMuerto)
        {
            return;
        }

        // Comprueba el suelo antes de aplicar movimiento.
        ComprobarSuelo();

        // Obtiene el multiplicador de la poción de velocidad.
        float multiplicador = 1f;

        if (sistemaPociones != null)
        {
            multiplicador = sistemaPociones.MultiplicadorVelocidad;
        }

        // Calcula el desplazamiento horizontal.
        float movimientoX =
            direccionHorizontal *
            velocidadMovimiento *
            multiplicador *
            Time.fixedDeltaTime;

        // Si está sobre el suelo, evita seguir cayendo.
        if (estaEnSuelo && velocidadVertical < 0f)
        {
            velocidadVertical = 0f;
        }

        // Aplica gravedad mientras está en el aire.
        if (!estaEnSuelo)
        {
            velocidadVertical -= gravedad * Time.fixedDeltaTime;
        }

        // Calcula el movimiento vertical.
        float movimientoY =
            velocidadVertical * Time.fixedDeltaTime;

        // Conserva la profundidad del escenario 2.5D.
        Vector3 nuevaPosicion = cuerpo.position +
            new Vector3(movimientoX, movimientoY, 0f);

        // Mueve al personaje usando su Rigidbody.
        cuerpo.MovePosition(nuevaPosicion);
    }

    // Detecta si hay una superficie bajo el personaje.
    private void ComprobarSuelo()
    {
        Vector3 origen = transform.position + Vector3.up * 0.1f;

        estaEnSuelo = Physics.Raycast(
            origen,
            Vector3.down,
            distanciaSuelo,
            capasSuelo,
            QueryTriggerInteraction.Ignore
        );
    }

    // Lee los controles sin depender de una interfaz visual.
    private void LeerControles()
    {
        direccionHorizontal = 0f;

#if ENABLE_INPUT_SYSTEM
        // Utiliza el nuevo sistema de entradas de Unity.
        Keyboard teclado = Keyboard.current;

        if (teclado == null)
        {
            return;
        }

        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed)
        {
            direccionHorizontal -= 1f;
        }

        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed)
        {
            direccionHorizontal += 1f;
        }

        if (teclado.spaceKey.wasPressedThisFrame ||
            teclado.wKey.wasPressedThisFrame ||
            teclado.upArrowKey.wasPressedThisFrame)
        {
            saltoSolicitado = true;
        }

#elif ENABLE_LEGACY_INPUT_MANAGER
        // Alternativa para proyectos con el Input Manager anterior.
        direccionHorizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            saltoSolicitado = true;
        }
#endif
    }

    // Muestra el área de detección de suelo en el editor.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origen = transform.position + Vector3.up * 0.1f;

        Gizmos.DrawLine(
            origen,
            origen + Vector3.down * distanciaSuelo
        );
    }
}
