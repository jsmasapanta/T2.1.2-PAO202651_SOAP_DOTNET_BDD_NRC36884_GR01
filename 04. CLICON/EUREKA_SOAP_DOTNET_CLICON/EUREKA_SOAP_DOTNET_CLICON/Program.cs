using System;
using ec.edu.monster.controlador;
using ec.edu.monster.servicio;
using ec.edu.monster.vista;

namespace ec.edu.monster
{
    internal static class Program
    {
        private static void Main()
        {
            var vista = new VistaConsola();
            try
            {
                var controlador = new ControladorConsola(vista, ClienteEureka.Instancia);
                controlador.Iniciar();
            }
            catch (Exception ex)
            {
                // Por ejemplo, App.config sin el endpoint "WSEureka".
                vista.MostrarError("Error inesperado: " + ex.Message);
                vista.Pausa();
            }
        }
    }
}
