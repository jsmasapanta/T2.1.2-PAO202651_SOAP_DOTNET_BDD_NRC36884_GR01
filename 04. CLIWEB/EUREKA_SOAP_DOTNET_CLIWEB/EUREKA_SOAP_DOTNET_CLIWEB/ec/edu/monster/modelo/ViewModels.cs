using System.Globalization;
using ec.edu.monster.servicio;

namespace ec.edu.monster.modelo;

public class LoginViewModel
{
    public string? Usuario { get; set; }
    public string? Clave { get; set; }
    public string? Error { get; set; }
    public int IntentosRestantes { get; set; }
    public bool Bloqueado { get; set; }
}

public class MovimientosViewModel
{
    public string? Cuenta { get; set; }
    public Cuenta? Datos { get; set; }
    public IList<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    public string? Error { get; set; }
}

public class OperacionViewModel
{
    public TipoOperacion Tipo { get; set; }
    public string? Cuenta { get; set; }
    public string? CuentaDestino { get; set; }
    public string? Importe { get; set; }

    public string? Exito { get; set; }
    public string? Detalle { get; set; }
    public string? Error { get; set; }

    /// <summary>Operaciones registradas en esta sesión del navegador (la más reciente primero).</summary>
    public IList<string> Sesion { get; set; } = new List<string>();

    public string Titulo => Formato.Titulo(Tipo);
}

/// <summary>Formatos compartidos por controladores y vistas.</summary>
public static class Formato
{
    /// <summary>Importe con separador de miles y 2 decimales: 6,900.00</summary>
    public static string Dinero(decimal importe) => importe.ToString("#,##0.00", CultureInfo.InvariantCulture);

    public static string Titulo(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => "Depósito",
        TipoOperacion.Retiro => "Retiro",
        _ => "Transferencia"
    };

    /// <summary>
    /// Convierte el texto a número (punto o coma decimal). Solo comprueba que sea un número, porque el
    /// servicio recibe un decimal; si es cero, negativo o tiene más de 2 decimales lo rechaza el servidor.
    /// </summary>
    public static bool IntentarLeerImporte(string? texto, out decimal importe)
    {
        string normalizado = (texto ?? "").Trim().Replace(',', '.');
        return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out importe);
    }

    /// <summary>La acción (INGRESO o SALIDA) viene del servidor, de la tabla TipoMovimiento.</summary>
    public static bool EsIngreso(Movimiento m) => m.Accion == "INGRESO";
}
