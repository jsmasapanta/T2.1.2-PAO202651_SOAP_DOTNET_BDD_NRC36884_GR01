using System.Text.Json;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controlador;

/// <summary>Consulta de movimientos y registro de depósitos, retiros y transferencias.</summary>
[RequiereSesion]
public class BancoController : Controller
{
    private const string ClaveSesionOperaciones = "Operaciones";
    private const int MaximoSesion = 10;

    private readonly ClienteEureka _cliente;

    public BancoController(ClienteEureka cliente)
    {
        _cliente = cliente;
    }

    // ---------- Movimientos ----------

    /// <summary>GET /Banco/Movimientos?cuenta=00100001 (sin cuenta muestra solo el formulario).</summary>
    [HttpGet]
    public async Task<IActionResult> Movimientos(string? cuenta)
    {
        PrepararVista("Movimientos");
        var modelo = new MovimientosViewModel { Cuenta = cuenta?.Trim() };
        if (string.IsNullOrWhiteSpace(cuenta))
        {
            return View(modelo);
        }

        try
        {
            modelo.Datos = await _cliente.ConsultarCuentaAsync(modelo.Cuenta!);
            modelo.Movimientos = await _cliente.TraerMovimientosAsync(modelo.Cuenta!);
        }
        catch (ErrorServicioException ex)
        {
            modelo.Error = ex.Message;
        }
        return View(modelo);
    }

    // ---------- Operaciones ----------

    [HttpGet]
    public IActionResult Deposito() => MostrarOperacion(TipoOperacion.Deposito);

    [HttpGet]
    public IActionResult Retiro() => MostrarOperacion(TipoOperacion.Retiro);

    [HttpGet]
    public IActionResult Transferencia() => MostrarOperacion(TipoOperacion.Transferencia);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(OperacionViewModel entrada)
    {
        PrepararVista(entrada.Tipo.ToString());
        var modelo = new OperacionViewModel
        {
            Tipo = entrada.Tipo,
            Cuenta = entrada.Cuenta?.Trim(),
            CuentaDestino = entrada.CuentaDestino?.Trim(),
            Importe = entrada.Importe,
            Sesion = LeerOperacionesSesion()
        };
        bool esTransferencia = modelo.Tipo == TipoOperacion.Transferencia;

        // Las reglas (formato de cuenta, importe, cuentas distintas, saldo) las valida el servidor.
        if (string.IsNullOrEmpty(modelo.Cuenta) || (esTransferencia && string.IsNullOrEmpty(modelo.CuentaDestino)))
            modelo.Error = "Complete los números de cuenta.";
        else if (!Formato.IntentarLeerImporte(modelo.Importe, out _))
            modelo.Error = "Escriba el importe con números (ejemplo: 150 o 75.50).";
        if (modelo.Error != null)
        {
            return View("Operacion", modelo);
        }

        Formato.IntentarLeerImporte(modelo.Importe, out decimal importe);
        string empleado = HttpContext.Session.GetString(RequiereSesionAttribute.ClaveEmpleado)!;
        try
        {
            decimal saldo = await _cliente.RegistrarAsync(modelo.Tipo, modelo.Cuenta!, esTransferencia ? modelo.CuentaDestino : null, importe, empleado);

            modelo.Exito = $"{modelo.Titulo} registrad{(esTransferencia ? "a" : "o")}";
            modelo.Detalle = esTransferencia
                ? $"{Formato.Dinero(importe)} de {modelo.Cuenta} a {modelo.CuentaDestino}. Nuevo saldo de {modelo.Cuenta}: {Formato.Dinero(saldo)}"
                : $"{Formato.Dinero(importe)} en la cuenta {modelo.Cuenta}. Nuevo saldo: {Formato.Dinero(saldo)}";

            modelo.Sesion.Insert(0, $"{DateTime.Now:HH:mm:ss} · {modelo.Titulo} · {modelo.Cuenta}" +
                                    (esTransferencia ? $" → {modelo.CuentaDestino}" : "") +
                                    $" · {Formato.Dinero(importe)} (saldo {Formato.Dinero(saldo)})");
            while (modelo.Sesion.Count > MaximoSesion) modelo.Sesion.RemoveAt(modelo.Sesion.Count - 1);
            HttpContext.Session.SetString(ClaveSesionOperaciones, JsonSerializer.Serialize(modelo.Sesion));
            modelo.Importe = null;
        }
        catch (ErrorServicioException ex)
        {
            modelo.Error = ex.Message;
        }
        return View("Operacion", modelo);
    }

    private IActionResult MostrarOperacion(TipoOperacion tipo)
    {
        PrepararVista(tipo.ToString());
        return View("Operacion", new OperacionViewModel { Tipo = tipo, Sesion = LeerOperacionesSesion() });
    }

    private IList<string> LeerOperacionesSesion()
    {
        string? json = HttpContext.Session.GetString(ClaveSesionOperaciones);
        return json == null ? new List<string>() : JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }

    /// <summary>Datos que usa el menú lateral del layout.</summary>
    private void PrepararVista(string opcionActiva)
    {
        ViewData["Opcion"] = opcionActiva;
        ViewData["Empleado"] = HttpContext.Session.GetString(RequiereSesionAttribute.ClaveEmpleado);
        ViewData["Nombre"] = HttpContext.Session.GetString(RequiereSesionAttribute.ClaveNombre);
        ViewData["Servidor"] = _cliente.UrlServidor;
    }
}
