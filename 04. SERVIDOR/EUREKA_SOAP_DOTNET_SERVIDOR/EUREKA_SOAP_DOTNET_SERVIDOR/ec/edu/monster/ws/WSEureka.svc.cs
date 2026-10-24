using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ServiceModel;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.ws
{
    /// <summary>
    /// Implementación del servicio SOAP. Solo delega en EurekaServicio y
    /// traduce los errores a un SOAP Fault con un mensaje claro para el usuario.
    /// </summary>
    // Mismo namespace que el contrato: así el WSDL sale en un solo documento (más fácil para los clientes).
    [ServiceBehavior(Namespace = Constantes.NamespaceServicio)]
    public class WSEureka : IWSEureka
    {
        public Empleado Login(string usuario, string clave) =>
            Ejecutar(() => EurekaServicio.Instancia.Login(usuario, clave));

        public Cuenta ConsultarCuenta(string cuenta) =>
            Ejecutar(() => EurekaServicio.Instancia.ConsultarCuenta(cuenta));

        public List<Movimiento> TraerMovimientos(string cuenta) =>
            Ejecutar(() => EurekaServicio.Instancia.TraerMovimientos(cuenta));

        public decimal RegistrarDeposito(string cuenta, decimal importe, string codEmpleado) =>
            Ejecutar(() => EurekaServicio.Instancia.RegistrarDeposito(cuenta, importe, codEmpleado));

        public decimal RegistrarRetiro(string cuenta, decimal importe, string codEmpleado) =>
            Ejecutar(() => EurekaServicio.Instancia.RegistrarRetiro(cuenta, importe, codEmpleado));

        public decimal RegistrarTransferencia(string cuentaOrigen, string cuentaDestino, decimal importe, string codEmpleado) =>
            Ejecutar(() => EurekaServicio.Instancia.RegistrarTransferencia(cuentaOrigen, cuentaDestino, importe, codEmpleado));

        private static T Ejecutar<T>(Func<T> operacion)
        {
            try
            {
                return operacion();
            }
            catch (ArgumentException ex)
            {
                throw Falla("VALOR_INVALIDO", ex.Message);
            }
            catch (ReglaNegocioException ex)
            {
                throw Falla("REGLA_NEGOCIO", ex.Message);
            }
            catch (SqlException ex)
            {
                // El detalle técnico queda en la salida de depuración; al cliente le llega un mensaje entendible.
                Trace.TraceError("SQL Server: " + ex.Message);
                throw Falla("ERROR_BD", "No se pudo acceder a la base de datos EUREKABANK. Verifique que SQL Server esté encendido.");
            }
            catch (InvalidOperationException ex)
            {
                throw Falla("ERROR_SERVIDOR", ex.Message);
            }
        }

        private static FaultException<ErrorServicio> Falla(string codigo, string mensaje)
        {
            // ArgumentException agrega "Nombre del parámetro: ..." al mensaje; nos quedamos con la primera línea.
            string limpio = mensaje.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
            return new FaultException<ErrorServicio>(new ErrorServicio(codigo, limpio), new FaultReason(limpio));
        }
    }
}
