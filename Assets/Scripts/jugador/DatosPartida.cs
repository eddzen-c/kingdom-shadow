
using System;

// Almacena la información que se guardará de una partida.
[Serializable]
public class DatosPartida
{
    // Indica si el primer cristal fue obtenido.
    public bool primerCristalObtenido = false;

    // Indica si el Bosque Encantado está desbloqueado.
    public bool bosqueEncantadoDesbloqueado = false;

    // Guarda el último punto de control activado.
    public string ultimoPuntoControl = "";

    // Indica si la partida tiene progreso iniciado.
    public bool partidaIniciada = false;
}
