using UnityEngine;
using UnityEngine.SceneManagement;

public class PantallaDerrota : MonoBehaviour
{
    public GameObject panelDerrota;

    public void MostrarDerrota()
    {
        panelDerrota.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void BotonReintentar()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}