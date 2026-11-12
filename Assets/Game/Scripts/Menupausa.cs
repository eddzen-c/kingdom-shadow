using UnityEngine;

public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausa;
    private bool estaPausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        estaPausado = !estaPausado;
        
        if (panelPausa != null)
        {
            panelPausa.SetActive(estaPausado);
        }
        
        Time.timeScale = estaPausado ? 0f : 1f;
    }

    public void BotonReanudar()
    {
        AlternarPausa();
    }

    public void BotonSalir()
    {
        Application.Quit(); 
    }
}