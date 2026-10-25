using System;
using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio
{
    /// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje que envió el servidor).</summary>
    public class ErrorServicioException : Exception
    {
        public ErrorServicioException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// Llama a las operaciones del servicio SOAP con el endpoint de App.config. No tiene reglas del banco:
    /// si el servidor rechaza algo, su mensaje (ErrorServicio) se entrega a la vista.
    /// Patrón Singleton: toda la aplicación usa ClienteEureka.Instancia (una sola ChannelFactory).
    /// </summary>
    public sealed class ClienteEureka
    {
        // Nombre del endpoint definido en App.config (system.serviceModel/client).
        private const string NombreEndpoint = "WSEureka";

        private static readonly Lazy<ClienteEureka> _instancia =
            new Lazy<ClienteEureka>(() => new ClienteEureka());

        public static ClienteEureka Instancia => _instancia.Value;

        private readonly ChannelFactory<IWSEureka> _fabrica;

        // Constructor privado: solo la propia clase crea la instancia.
        private ClienteEureka()
        {
            _fabrica = new ChannelFactory<IWSEureka>(NombreEndpoint);
        }

        public string UrlServidor => _fabrica.Endpoint.Address.Uri.ToString();

        public Empleado Login(string usuario, string clave) => Invocar(c => c.Login(usuario, clave));

        public Cuenta ConsultarCuenta(string cuenta) => Invocar(c => c.ConsultarCuenta(cuenta));

        public Movimiento[] TraerMovimientos(string cuenta) =>
            Invocar(c => c.TraerMovimientos(cuenta)) ?? new Movimiento[0];

        public decimal RegistrarDeposito(string cuenta, decimal importe, string codEmpleado) =>
            Invocar(c => c.RegistrarDeposito(cuenta, importe, codEmpleado));

        public decimal RegistrarRetiro(string cuenta, decimal importe, string codEmpleado) =>
            Invocar(c => c.RegistrarRetiro(cuenta, importe, codEmpleado));

        public decimal RegistrarTransferencia(string origen, string destino, decimal importe, string codEmpleado) =>
            Invocar(c => c.RegistrarTransferencia(origen, destino, importe, codEmpleado));

        private T Invocar<T>(Func<IWSEureka, T> llamada)
        {
            IWSEureka canal = _fabrica.CreateChannel();
            var comunicacion = (ICommunicationObject)canal;
            try
            {
                T resultado = llamada(canal);
                comunicacion.Close();
                return resultado;
            }
            catch (FaultException<ErrorServicio> ex)
            {
                comunicacion.Abort();
                throw new ErrorServicioException(ex.Detail.Mensaje);
            }
            catch (FaultException ex)
            {
                comunicacion.Abort();
                throw new ErrorServicioException("El servidor rechazó la petición: " + ex.Message);
            }
            catch (EndpointNotFoundException)
            {
                comunicacion.Abort();
                throw new ErrorServicioException(
                    "No se pudo conectar con el servidor en " + UrlServidor + ". Verifique que esté encendido y la URL en App.config.");
            }
            catch (TimeoutException)
            {
                comunicacion.Abort();
                throw new ErrorServicioException("El servidor tardó demasiado en responder. Intente de nuevo.");
            }
            catch (CommunicationException ex)
            {
                comunicacion.Abort();
                throw new ErrorServicioException("Error de comunicación con el servidor: " + ex.Message);
            }
        }
    }
}
