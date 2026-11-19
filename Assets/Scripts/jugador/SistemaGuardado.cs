
using UnityEngine;
using System;
using System.IO;

// Administra el guardado y la carga de las partidas.
public class SistemaGuardado : MonoBehaviour
{
    // Guarda la información actual de la partida.
    private DatosPartida datosPartida;

    // Ubicación del archivo de guardado.
    private string rutaGuardado;

    // Permite consultar los datos desde otros scripts.
    public DatosPartida Datos => datosPartida;

    // Prepara el sistema al comenzar el juego.
    private void Awake()
    {
        // Establece la ubicación del archivo de guardado.
        rutaGuardado = Path.Combine(
            Application.persistentDataPath,
            "partida.json"
        );

        // Prepara los datos iniciales.
        datosPartida = new DatosPartida();

        // Recupera automáticamente una partida existente.
        if (ExistePartidaGuardada())
        {
            CargarPartida();
        }

        Debug.Log("Sistema de guardado preparado.");
    }

    // Registra que Leo consiguió el primer cristal.
    public void RegistrarPrimerCristal()
    {
        // Evita registrar el mismo cristal varias veces.
        if (datosPartida.primerCristalObtenido)
        {
            Debug.Log("El primer cristal ya fue obtenido.");
            return;
        }

        // Actualiza el progreso.
        datosPartida.primerCristalObtenido = true;

        // Guarda automáticamente.
        GuardarPartida();

        Debug.Log("Primer cristal obtenido y registrado.");
    }

    // Comprueba si Leo puede desbloquear el Bosque Encantado.
    public bool PuedeDesbloquearBosque()
    {
        // Es necesario haber conseguido el primer cristal.
        return datosPartida.primerCristalObtenido;
    }

    // Desbloquea el Bosque Encantado y guarda el progreso.
    public void DesbloquearBosque()
    {
        // Evita desbloquear el bosque varias veces.
        if (datosPartida.bosqueEncantadoDesbloqueado)
        {
            Debug.Log("El Bosque Encantado ya está desbloqueado.");
            return;
        }

        // Comprueba que Leo tenga el primer cristal.
        if (!PuedeDesbloquearBosque())
        {
            Debug.Log(
                "Necesitas el primer cristal para desbloquear el Bosque Encantado."
            );
            return;
        }

        // Registra el desbloqueo.
        datosPartida.bosqueEncantadoDesbloqueado = true;

        // Guarda automáticamente.
        GuardarPartida();

        Debug.Log("¡Bosque Encantado desbloqueado!");
    }

    // Guarda el último punto de control alcanzado por Leo.
    public void RegistrarPuntoControl(string identificador)
    {
        // Evita registrar un identificador vacío.
        if (string.IsNullOrWhiteSpace(identificador))
        {
            return;
        }

        // Evita guardar nuevamente el mismo punto.
        if (datosPartida.ultimoPuntoControl == identificador)
        {
            return;
        }

        // Actualiza el último punto de control.
        datosPartida.ultimoPuntoControl = identificador;

        // Guarda el cambio inmediatamente.
        GuardarPartida();

        Debug.Log(
            "Punto de control guardado: " +
            identificador
        );
    }

    // Guarda el progreso actual de la partida.
    public void GuardarPartida()
    {
        try
        {
            // Marca que la partida ya fue iniciada.
            datosPartida.partidaIniciada = true;

            // Convierte los datos a formato JSON.
            string contenido = JsonUtility.ToJson(
                datosPartida,
                true
            );

            // Guarda la información en el archivo.
            File.WriteAllText(rutaGuardado, contenido);

            Debug.Log("Partida guardada correctamente.");
            Debug.Log("Archivo guardado en: " + rutaGuardado);
        }
        catch (Exception error)
        {
            // Muestra el error si no se puede guardar.
            Debug.LogError(
                "Error al guardar la partida: " + error.Message
            );
        }
    }

    // Recupera la información de una partida guardada.
    public void CargarPartida()
    {
        // Comprueba si existe un archivo.
        if (!File.Exists(rutaGuardado))
        {
            Debug.Log("No existe una partida guardada.");
            return;
        }

        try
        {
            // Lee el archivo existente.
            string contenido = File.ReadAllText(rutaGuardado);

            // Convierte el JSON a datos del juego.
            DatosPartida datosCargados =
                JsonUtility.FromJson<DatosPartida>(contenido);

            // Comprueba que los datos sean válidos.
            if (datosCargados == null)
            {
                Debug.LogWarning(
                    "Los datos guardados no son válidos."
                );
                return;
            }

            // Recupera el progreso guardado.
            datosPartida = datosCargados;

            Debug.Log("Partida cargada correctamente.");
        }
        catch (Exception error)
        {
            Debug.LogError(
                "Error al cargar la partida: " + error.Message
            );
        }
    }

    // Comprueba si existe una partida guardada.
    public bool ExistePartidaGuardada()
    {
        return File.Exists(rutaGuardado);
    }

    // Permite consultar dónde está guardada la partida.
    public string ObtenerRutaGuardado()
    {
        return rutaGuardado;
    }
}
