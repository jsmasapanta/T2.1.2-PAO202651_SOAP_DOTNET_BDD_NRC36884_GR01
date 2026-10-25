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
/// (WSEurekaReference.cs, el mismo de la web) y traduce las fallas a un mensaje entendible.
/// El cliente no valida reglas del banco: las valida el servidor y su mensaje llega en el SOAP Fault.
/// Patrón Singleton: toda la app usa ClienteSoap.Instancia.
/// </summary>
public sealed class ClienteSoap
{
    private const string ClavePreferencia = "url_servidor_eureka";

    // 10.0.2.2 es la PC vista desde el emulador; en un celular real se cambia por la IP de la PC.
    public const string UrlPorDefecto = "http://10.0.2.2:8087/WSEureka.svc";

    private static readonly Lazy<ClienteSoap> _instancia = new(() => new ClienteSoap());

    public static ClienteSoap Instancia => _instancia.Value;

    private ChannelFactory<WSEureka>? _fabrica;
    private string? _urlFabrica;

    // Constructor privado: solo la propia clase crea la instancia.
    private ClienteSoap() { }

    /// <summary>URL del servicio; se guarda en el teléfono para no escribirla cada vez.</summary>
    public string Url
    {
        get => Preferences.Default.Get(ClavePreferencia, UrlPorDefecto);
        set => Preferences.Default.Set(ClavePreferencia, value.Trim());
    }

    /// <summary>Devuelve el empleado o null si usuario y clave no coinciden.</summary>
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

    /// <summary>La ChannelFactory se crea una vez por URL (se vuelve a crear solo si el usuario cambia la URL).</summary>
    private ChannelFactory<WSEureka> Fabrica()
    {
        string url = Url;
        if (_fabrica == null || _urlFabrica != url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? destino))
            {
                throw new ErrorServicioException($"La URL del servidor no es válida: {url}");
            }
            _fabrica?.Abort();
            var binding = new BasicHttpBinding
            {
                OpenTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(20),
                ReceiveTimeout = TimeSpan.FromSeconds(20),
                MaxReceivedMessageSize = 10 * 1024 * 1024
            };
            _fabrica = new ChannelFactory<WSEureka>(binding, new EndpointAddress(destino));
            _urlFabrica = url;
        }
        return _fabrica;
    }

    private async Task<T> InvocarAsync<T>(Func<WSEureka, Task<T>> llamada)
    {
        WSEureka canal = Fabrica().CreateChannel();
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
        catch (TimeoutException)
        {
            comunicacion.Abort();
            throw new ErrorServicioException("El servidor tardó demasiado en responder. Intente de nuevo.");
        }
        catch (CommunicationException)
        {
            comunicacion.Abort();
            throw new ErrorServicioException(
                $"No se pudo conectar con el servidor en {Url}. Verifique que esté encendido, la IP y que estén en la misma red WiFi.");
        }
    }
}
