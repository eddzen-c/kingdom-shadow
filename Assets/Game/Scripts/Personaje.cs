using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;
    
    private Rigidbody rb;
    private bool enSuelo = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movimiento con WASD o Flechas
        float movimientoH = Input.GetAxis("Horizontal"); // A/D o Flechas
        float movimientoV = Input.GetAxis("Vertical");   // W/S o Flechas

        Vector3 movimiento = new Vector3(movimientoH, 0, movimientoV);
        
        // Mueve al personaje
        transform.Translate(movimiento * velocidad * Time.deltaTime);

        // Salto con la barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enSuelo = false;
        }
    }

    // Detecta cuando toca el suelo de nuevo
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo") || collision.gameObject.CompareTag("Untagged"))
        {
            enSuelo = true;
        }
    }
}
