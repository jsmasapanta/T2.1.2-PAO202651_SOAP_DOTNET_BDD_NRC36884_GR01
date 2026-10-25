using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario.</summary>
public class ErrorServicioException : Exception
{
    public ErrorServicioException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Punto único de acceso al servicio SOAP. Usa el contrato WSEureka generado desde el WSDL
/// (WSEurekaReference.cs); la URL viene de appsettings.json ("ServicioSoap:Url").
/// El cliente no valida reglas del banco: las valida el servidor y su mensaje llega en el SOAP Fault.
/// Se registra como Singleton en Program.cs: la ChannelFactory se crea una sola vez y es segura entre hilos.
/// </summary>
public class ClienteEureka : IDisposable
{
    private readonly ChannelFactory<WSEureka> _fabrica;

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
        _fabrica = new ChannelFactory<WSEureka>(binding, new EndpointAddress(url));
        UrlServidor = url;
    }

    public string UrlServidor { get; }

    public Task<Empleado?> LoginAsync(string usuario, string clave) =>
        InvocarAsync<Empleado?>(async c => await c.loginAsync(usuario, clave));

    public Task<Cuenta> ConsultarCuentaAsync(string cuenta) =>
        InvocarAsync(c => c.consultarCuentaAsync(cuenta));

    public async Task<Movimiento[]> TraerMovimientosAsync(string cuenta) =>
        await InvocarAsync(c => c.traerMovimientosAsync(cuenta)) ?? Array.Empty<Movimiento>();

    /// <summary>Registra la operación y devuelve el nuevo saldo (de la cuenta de origen en transferencias).</summary>
    public Task<decimal> RegistrarAsync(TipoOperacion tipo, string cuenta, string? destino, decimal importe, string codEmpleado) =>
        tipo switch
        {
            TipoOperacion.Deposito => InvocarAsync(c => c.registrarDepositoAsync(cuenta, importe, codEmpleado)),
            TipoOperacion.Retiro => InvocarAsync(c => c.registrarRetiroAsync(cuenta, importe, codEmpleado)),
            _ => InvocarAsync(c => c.registrarTransferenciaAsync(cuenta, destino ?? "", importe, codEmpleado))
        };

    private async Task<T> InvocarAsync<T>(Func<WSEureka, Task<T>> llamada)
    {
        WSEureka canal = _fabrica.CreateChannel();
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
