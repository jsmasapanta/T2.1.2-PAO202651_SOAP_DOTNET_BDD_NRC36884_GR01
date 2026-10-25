using System.Globalization;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Xml.Linq;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio;

/// <summary>Error que la vista puede mostrar tal cual al usuario (normalmente el mensaje que envió el servidor).</summary>
public class ErrorServicioException : Exception
{
    public ErrorServicioException(string mensaje) : base(mensaje) { }
}

/// <summary>
/// Llama a las operaciones del servicio SOAP con HttpClient: arma el sobre, lo envía por POST con la
/// cabecera SOAPAction y copia la respuesta en los modelos (get/set). Si llega un SOAP Fault, entrega el
/// &lt;Mensaje&gt; de ErrorServicio. No tiene reglas del banco: todas las valida el servidor.
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

    public async Task<Empleado> LoginAsync(string usuario, string clave)
    {
        XElement r = await InvocarAsync("login", ("usuario", usuario), ("clave", clave));
        return new Empleado
        {
            Codigo = Texto(r, "Codigo"),
            Usuario = Texto(r, "Usuario"),
            Nombre = Texto(r, "Nombre")
        };
    }

    public async Task<Cuenta> ConsultarCuentaAsync(string cuenta)
    {
        XElement r = await InvocarAsync("consultarCuenta", ("cuenta", cuenta));
        return new Cuenta
        {
            Codigo = Texto(r, "Codigo"),
            Cliente = Texto(r, "Cliente"),
            Moneda = Texto(r, "Moneda"),
            Sucursal = Texto(r, "Sucursal"),
            Saldo = Decimal(Texto(r, "Saldo")),
            Estado = Texto(r, "Estado"),
            FechaCreacion = DateTime.Parse(Texto(r, "FechaCreacion"), CultureInfo.InvariantCulture),
            NumeroMovimientos = int.Parse(Texto(r, "NumeroMovimientos"), CultureInfo.InvariantCulture)
        };
    }

    public async Task<Movimiento[]> TraerMovimientosAsync(string cuenta)
    {
        XElement r = await InvocarAsync("traerMovimientos", ("cuenta", cuenta));
        return r.Elements().Where(e => e.Name.LocalName == "Movimiento").Select(m => new Movimiento
        {
            Numero = int.Parse(Texto(m, "Numero"), CultureInfo.InvariantCulture),
            Fecha = DateTime.Parse(Texto(m, "Fecha"), CultureInfo.InvariantCulture),
            Tipo = Texto(m, "Tipo"),
            Accion = Texto(m, "Accion"),
            Importe = Decimal(Texto(m, "Importe")),
            Empleado = Texto(m, "Empleado"),
            CuentaReferencia = Texto(m, "CuentaReferencia")
        }).ToArray();
    }

    public async Task<decimal> RegistrarDepositoAsync(string cuenta, decimal importe, string codEmpleado) =>
        Decimal((await InvocarAsync("registrarDeposito",
            ("cuenta", cuenta), ("importe", Numero(importe)), ("codEmpleado", codEmpleado))).Value);

    public async Task<decimal> RegistrarRetiroAsync(string cuenta, decimal importe, string codEmpleado) =>
        Decimal((await InvocarAsync("registrarRetiro",
            ("cuenta", cuenta), ("importe", Numero(importe)), ("codEmpleado", codEmpleado))).Value);

    public async Task<decimal> RegistrarTransferenciaAsync(string origen, string destino, decimal importe, string codEmpleado) =>
        Decimal((await InvocarAsync("registrarTransferencia",
            ("cuentaOrigen", origen), ("cuentaDestino", destino), ("importe", Numero(importe)), ("codEmpleado", codEmpleado))).Value);

    /// <summary>Envía la petición y devuelve el elemento &lt;operacionResult&gt;, o lanza el mensaje del Fault.</summary>
    private async Task<XElement> InvocarAsync(string operacion, params (string Nombre, string Valor)[] parametros)
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

    private static XElement LeerRespuesta(string xml, int codigoHttp, string operacion)
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

        return Buscar(operacion + "Result")
            ?? throw new ErrorServicioException(
                Buscar("Mensaje")?.Value ?? Buscar("faultstring")?.Value ?? $"Respuesta inesperada del servidor (HTTP {codigoHttp}).");
    }

    private static string Texto(XElement padre, string hijo) =>
        padre.Elements().FirstOrDefault(e => e.Name.LocalName == hijo)?.Value.Trim() ?? "";

    private static string Numero(decimal valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static decimal Decimal(string texto) =>
        decimal.Parse(texto, NumberStyles.Float, CultureInfo.InvariantCulture);
}
