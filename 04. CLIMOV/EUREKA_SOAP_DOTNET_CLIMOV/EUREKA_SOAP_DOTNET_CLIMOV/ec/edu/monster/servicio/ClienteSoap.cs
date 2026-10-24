using System.Globalization;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Xml.Linq;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario.</summary>
public class ErrorServicioException : Exception
{
    public ErrorServicioException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Cliente SOAP 1.1 hecho a mano con HttpClient (sin referencias de servicio).
/// Por cada operación arma el sobre SOAP, lo envía por POST con la cabecera SOAPAction
/// y lee &lt;operacionResult&gt; o, si es un SOAP Fault, el &lt;Mensaje&gt; de ErrorServicio.
/// Patrón Singleton: toda la app usa ClienteSoap.Instancia (un solo HttpClient).
/// </summary>
public sealed class ClienteSoap
{
    public const string Namespace = "http://espe.edu.ec/eureka/";
    private const string Contrato = "WSEureka";
    private const string ClavePreferencia = "url_servidor_eureka";

    // 10.0.2.2 es la PC vista desde el emulador; en un celular real se cambia por la IP de la PC.
    public const string UrlPorDefecto = "http://10.0.2.2:8087/WSEureka.svc";

    private static readonly Lazy<ClienteSoap> _instancia = new(() => new ClienteSoap());

    public static ClienteSoap Instancia => _instancia.Value;

    private readonly HttpClient _http;

    // Constructor privado: solo la propia clase crea la instancia.
    private ClienteSoap()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }

    /// <summary>URL del servicio; se guarda en el teléfono para no escribirla cada vez.</summary>
    public string Url
    {
        get => Preferences.Default.Get(ClavePreferencia, UrlPorDefecto);
        set => Preferences.Default.Set(ClavePreferencia, value.Trim());
    }

    /// <summary>Devuelve el empleado o null si usuario/clave no coinciden (loginResult llega con i:nil).</summary>
    public async Task<Empleado?> LoginAsync(string usuario, string clave)
    {
        XElement? r = await InvocarAsync("login", ("usuario", usuario), ("clave", clave));
        if (r == null || !r.HasElements)
        {
            return null;
        }
        return new Empleado(Texto(r, "Codigo"), Texto(r, "Usuario"), Texto(r, "Nombre"));
    }

    public async Task<Cuenta> ConsultarCuentaAsync(string cuenta)
    {
        XElement r = await InvocarAsync("consultarCuenta", ("cuenta", cuenta))
                     ?? throw new ErrorServicioException("Respuesta vacía del servidor.");
        return new Cuenta(Texto(r, "Codigo"), Texto(r, "Cliente"), Texto(r, "Moneda"), Texto(r, "Sucursal"),
            Decimal(Texto(r, "Saldo")), Texto(r, "Estado"), DateTime.Parse(Texto(r, "FechaCreacion"), CultureInfo.InvariantCulture),
            int.Parse(Texto(r, "NumeroMovimientos"), CultureInfo.InvariantCulture));
    }

    public async Task<List<Movimiento>> TraerMovimientosAsync(string cuenta)
    {
        XElement? r = await InvocarAsync("traerMovimientos", ("cuenta", cuenta));
        var lista = new List<Movimiento>();
        if (r == null)
        {
            return lista;
        }
        foreach (XElement m in r.Elements().Where(e => e.Name.LocalName == "Movimiento"))
        {
            lista.Add(new Movimiento(
                int.Parse(Texto(m, "Numero"), CultureInfo.InvariantCulture),
                DateTime.Parse(Texto(m, "Fecha"), CultureInfo.InvariantCulture),
                Texto(m, "Tipo"), Texto(m, "Accion"), Decimal(Texto(m, "Importe")),
                Texto(m, "Empleado"), Texto(m, "CuentaReferencia")));
        }
        return lista;
    }

    /// <summary>Registra la operación y devuelve el nuevo saldo (de la cuenta de origen en transferencias).</summary>
    public async Task<decimal> RegistrarAsync(TipoOperacion tipo, string cuenta, string? destino, decimal importe, string codEmpleado)
    {
        string monto = importe.ToString(CultureInfo.InvariantCulture);
        XElement? r = tipo switch
        {
            TipoOperacion.Deposito => await InvocarAsync("registrarDeposito",
                ("cuenta", cuenta), ("importe", monto), ("codEmpleado", codEmpleado)),
            TipoOperacion.Retiro => await InvocarAsync("registrarRetiro",
                ("cuenta", cuenta), ("importe", monto), ("codEmpleado", codEmpleado)),
            _ => await InvocarAsync("registrarTransferencia",
                ("cuentaOrigen", cuenta), ("cuentaDestino", destino ?? ""), ("importe", monto), ("codEmpleado", codEmpleado))
        };
        return Decimal(r?.Value ?? "0");
    }

    /// <summary>Envía la petición y devuelve el elemento &lt;operacionResult&gt; (null si llegó vacío).</summary>
    private async Task<XElement?> InvocarAsync(string operacion, params (string Nombre, string Valor)[] parametros)
    {
        string url = Url;
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? destino))
        {
            throw new ErrorServicioException($"La URL del servidor no es válida: {url}");
        }

        var peticion = new HttpRequestMessage(HttpMethod.Post, destino)
        {
            Content = new StringContent(ConstruirSobre(operacion, parametros), Encoding.UTF8, "text/xml")
        };
        peticion.Headers.Add("SOAPAction", $"\"{Namespace}{Contrato}/{operacion}\"");

        try
        {
            using HttpResponseMessage respuesta = await _http.SendAsync(peticion);
            string cuerpo = await respuesta.Content.ReadAsStringAsync();
            return LeerRespuesta(cuerpo, (int)respuesta.StatusCode, operacion);
        }
        catch (ErrorServicioException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new ErrorServicioException("El servidor tardó demasiado en responder. Intente de nuevo.");
        }
        catch (Exception e) when (e is HttpRequestException || e is SocketException || e is IOException)
        {
            throw new ErrorServicioException(
                $"No se pudo conectar con el servidor en {url}. Verifique que esté encendido, la IP y que estén en la misma red WiFi.");
        }
    }

    private static string ConstruirSobre(string operacion, (string Nombre, string Valor)[] parametros)
    {
        var cuerpo = new StringBuilder();
        foreach (var (nombre, valor) in parametros)
        {
            cuerpo.Append($"<{nombre}>{SecurityElement.Escape(valor)}</{nombre}>");
        }
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
            + "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">"
            + $"<s:Body><{operacion} xmlns=\"{Namespace}\">{cuerpo}</{operacion}></s:Body>"
            + "</s:Envelope>";
    }

    private static XElement? LeerRespuesta(string xml, int codigoHttp, string operacion)
    {
        XDocument documento;
        try
        {
            documento = XDocument.Parse(xml);
        }
        catch (Exception)
        {
            throw new ErrorServicioException($"Respuesta inesperada del servidor (HTTP {codigoHttp}).");
        }

        XElement? Buscar(string nombre) =>
            documento.Descendants().FirstOrDefault(e => e.Name.LocalName == nombre);

        XElement? resultado = Buscar(operacion + "Result");
        if (resultado != null)
        {
            return resultado;
        }
        if (Buscar(operacion + "Response") != null)
        {
            return null;
        }
        throw new ErrorServicioException(
            Buscar("Mensaje")?.Value ?? Buscar("faultstring")?.Value ?? $"Respuesta inesperada del servidor (HTTP {codigoHttp}).");
    }

    private static string Texto(XElement padre, string hijo) =>
        padre.Elements().FirstOrDefault(e => e.Name.LocalName == hijo)?.Value.Trim() ?? "";

    private static decimal Decimal(string texto) =>
        decimal.Parse(texto, NumberStyles.Float, CultureInfo.InvariantCulture);
}
