using UnityEngine;
using TMPro; 

public class SistemaDialogos : MonoBehaviour
{
    public GameObject panelDialogo;
    public TextMeshProUGUI textoNombre; 
    public TextMeshProUGUI textoDialogo; 

    private string[] dialogosActuales;
    private int indiceActual = 0;
    private bool enDialogo = false;

    public void IniciarDialogo(string nombre, string[] dialogos)
    {
        enDialogo = true;
        panelDialogo.SetActive(true);
        textoNombre.text = nombre;
        dialogosActuales = dialogos;
        indiceActual = 0;
        MostrarSiguienteDialogo();
    }

    void Update()
    {
        if (enDialogo && Input.GetKeyDown(KeyCode.Space))
        {
            MostrarSiguienteDialogo();
        }
    }

    void MostrarSiguienteDialogo()
    {
        if (indiceActual < dialogosActuales.Length)
        {
            textoDialogo.text = dialogosActuales[indiceActual];
            indiceActual++;
        }
        else
        {
            TerminarDialogo();
        }
    }

    void TerminarDialogo()
    {
        enDialogo = false;
        panelDialogo.SetActive(false);
    }
}