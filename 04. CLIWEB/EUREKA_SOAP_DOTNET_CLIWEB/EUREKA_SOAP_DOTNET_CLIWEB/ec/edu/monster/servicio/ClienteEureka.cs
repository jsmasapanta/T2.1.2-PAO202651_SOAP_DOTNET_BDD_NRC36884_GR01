using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje que envió el servidor).</summary>
public class ErrorServicioException : Exception
{
    public ErrorServicioException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Llama a las operaciones del servicio SOAP con el contrato IWSEureka; la URL viene de appsettings.json
/// ("ServicioSoap:Url"). No tiene reglas del banco: si el servidor rechaza algo, su mensaje se entrega a la vista.
/// Se registra como Singleton en Program.cs: la ChannelFactory se crea una sola vez y es segura entre hilos.
/// </summary>
public class ClienteEureka : IDisposable
{
    private readonly ChannelFactory<IWSEureka> _fabrica;

    public ClienteEureka(IConfiguration configuracion)
    {
        string url = configuracion["ServicioSoap:Url"]
            ?? throw new InvalidOperationException("Falta 'ServicioSoap:Url' en appsettings.json.");

        var binding = new BasicHttpBinding
        {
            OpenTimeout = TimeSpan.FromSeconds(10),
            SendTimeout = TimeSpan.FromSeconds(30),
            ReceiveTimeout = TimeSpan.FromSeconds(30),
            // Una cuenta con muchos movimientos supera los 64 KB por defecto.
            MaxReceivedMessageSize = 10 * 1024 * 1024
        };
        _fabrica = new ChannelFactory<IWSEureka>(binding, new EndpointAddress(url));
        UrlServidor = url;
    }

    public string UrlServidor { get; }

    public Task<Empleado> LoginAsync(string usuario, string clave) =>
        InvocarAsync(c => c.LoginAsync(usuario, clave));

    public Task<Cuenta> ConsultarCuentaAsync(string cuenta) =>
        InvocarAsync(c => c.ConsultarCuentaAsync(cuenta));

    public async Task<Movimiento[]> TraerMovimientosAsync(string cuenta) =>
        await InvocarAsync(c => c.TraerMovimientosAsync(cuenta)) ?? Array.Empty<Movimiento>();

    public Task<decimal> RegistrarDepositoAsync(string cuenta, decimal importe, string codEmpleado) =>
        InvocarAsync(c => c.RegistrarDepositoAsync(cuenta, importe, codEmpleado));

    public Task<decimal> RegistrarRetiroAsync(string cuenta, decimal importe, string codEmpleado) =>
        InvocarAsync(c => c.RegistrarRetiroAsync(cuenta, importe, codEmpleado));

    public Task<decimal> RegistrarTransferenciaAsync(string origen, string destino, decimal importe, string codEmpleado) =>
        InvocarAsync(c => c.RegistrarTransferenciaAsync(origen, destino, importe, codEmpleado));

    private async Task<T> InvocarAsync<T>(Func<IWSEureka, Task<T>> llamada)
    {
        IWSEureka canal = _fabrica.CreateChannel();
        var comunicacion = (ICommunicationObject)canal;
        try
        {
            T resultado = await llamada(canal);
            comunicacion.Close();
            return resultado;
        }
        catch (FaultException<ErrorServicio> ex)
        {
            comunicacion.Abort();
            throw new ErrorServicioException(ex.Detail.Mensaje ?? ex.Message);
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
                $"No se pudo conectar con el servidor en {UrlServidor}. Verifique que esté encendido y la URL en appsettings.json.");
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

    public void Dispose()
    {
        try { _fabrica.Close(); } catch { _fabrica.Abort(); }
    }
}
