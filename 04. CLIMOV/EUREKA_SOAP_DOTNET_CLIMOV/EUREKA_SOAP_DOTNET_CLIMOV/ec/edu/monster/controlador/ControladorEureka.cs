using System.Globalization;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.controlador;

/// <summary>
/// Intermediario entre las pantallas (vista) y el servicio SOAP.
/// Guarda el empleado de la sesión, valida la entrada y lleva el registro de operaciones de la sesión.
/// Patrón Singleton: todas las pantallas comparten ControladorEureka.Instancia.
/// </summary>
public sealed class ControladorEureka
{
    public const int MaximoIntentos = 3;
    private const int MaximoSesion = 15;

    private static readonly Lazy<ControladorEureka> _instancia =
        new(() => new ControladorEureka(ClienteSoap.Instancia));

    public static ControladorEureka Instancia => _instancia.Value;

    private readonly ClienteSoap _cliente;
    private readonly List<string> _operacionesSesion = new();

    private ControladorEureka(ClienteSoap cliente)
    {
        _cliente = cliente;
    }

    public Empleado? Empleado { get; private set; }

    public string UrlServidor
    {
        get => _cliente.Url;
        set => _cliente.Url = value;
    }

    public IReadOnlyList<string> OperacionesSesion => _operacionesSesion;

    public async Task<bool> IniciarSesionAsync(string usuario, string clave)
    {
        Empleado = await _cliente.LoginAsync(usuario.Trim(), clave);
        _operacionesSesion.Clear();
        return Empleado != null;
    }

    public void CerrarSesion()
    {
        Empleado = null;
        _operacionesSesion.Clear();
    }

    public async Task<(Cuenta Cuenta, List<Movimiento> Movimientos)> ConsultarAsync(string cuenta)
    {
        Cuenta datos = await _cliente.ConsultarCuentaAsync(cuenta);
        List<Movimiento> movimientos = await _cliente.TraerMovimientosAsync(cuenta);
        return (datos, movimientos);
    }

    /// <summary>Registra la operación, la anota en la sesión y devuelve el nuevo saldo.</summary>
    public async Task<decimal> RegistrarAsync(TipoOperacion tipo, string cuenta, string? destino, decimal importe)
    {
        if (Empleado == null)
        {
            throw new ErrorServicioException("La sesión terminó. Vuelva a iniciar sesión.");
        }
        decimal saldo = await _cliente.RegistrarAsync(tipo, cuenta, destino, importe, Empleado.Codigo);

        _operacionesSesion.Insert(0, $"{DateTime.Now:HH:mm:ss} · {Titulo(tipo)} · {cuenta}"
            + (destino != null ? $" → {destino}" : "")
            + $" · {Dinero(importe)} (saldo {Dinero(saldo)})");
        if (_operacionesSesion.Count > MaximoSesion)
        {
            _operacionesSesion.RemoveAt(_operacionesSesion.Count - 1);
        }
        return saldo;
    }

    public static bool EsCuentaValida(string? cuenta)
    {
        string limpia = (cuenta ?? "").Trim();
        return limpia.Length == 8 && limpia.All(char.IsDigit);
    }

    /// <summary>Acepta punto o coma decimal ("75.5" o "75,5"). Devuelve null si no es un importe válido.</summary>
    public static decimal? LeerImporte(string? texto)
    {
        string limpio = (texto ?? "").Trim().Replace(',', '.');
        return decimal.TryParse(limpio, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal importe)
               && importe > 0 && decimal.Round(importe, 2) == importe
            ? importe
            : null;
    }

    /// <summary>Importe con separador de miles y 2 decimales: 6,900.00</summary>
    public static string Dinero(decimal importe) => importe.ToString("#,##0.00", CultureInfo.InvariantCulture);

    public static string Titulo(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => "Depósito",
        TipoOperacion.Retiro => "Retiro",
        _ => "Transferencia"
    };

    public static Color ColorDe(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => Color.FromArgb("#198754"),
        TipoOperacion.Retiro => Color.FromArgb("#C0392B"),
        _ => Color.FromArgb("#6F42C1")
    };
}
