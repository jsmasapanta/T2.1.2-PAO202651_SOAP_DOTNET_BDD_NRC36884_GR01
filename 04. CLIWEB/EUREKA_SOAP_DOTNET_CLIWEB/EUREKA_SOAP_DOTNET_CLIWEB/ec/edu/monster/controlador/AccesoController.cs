using ec.edu.monster.modelo;
using ec.edu.monster.servicio;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controlador;

/// <summary>Inicio y cierre de sesión (3 intentos por sesión del navegador).</summary>
public class AccesoController : Controller
{
    private const int IntentosMaximos = 3;
    private const string ClaveIntentos = "IntentosFallidos";

    private readonly ClienteEureka _cliente;

    public AccesoController(ClienteEureka cliente)
    {
        _cliente = cliente;
    }

    /// <summary>La raíz del sitio (/) lleva al login.</summary>
    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Login));

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(RequiereSesionAttribute.ClaveEmpleado)))
        {
            return RedirectToAction("Movimientos", "Banco");
        }
        return View(CrearModelo());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel entrada)
    {
        LoginViewModel modelo = CrearModelo();
        modelo.Usuario = entrada.Usuario;

        if (modelo.Bloqueado)
        {
            return View(modelo);
        }
        if (string.IsNullOrWhiteSpace(entrada.Usuario) || string.IsNullOrEmpty(entrada.Clave))
        {
            modelo.Error = "Ingrese usuario y clave.";
            return View(modelo);
        }

        try
        {
            Empleado? empleado = await _cliente.LoginAsync(entrada.Usuario.Trim(), entrada.Clave);
            if (empleado != null)
            {
                HttpContext.Session.Remove(ClaveIntentos);
                HttpContext.Session.SetString(RequiereSesionAttribute.ClaveEmpleado, empleado.Codigo);
                HttpContext.Session.SetString(RequiereSesionAttribute.ClaveNombre, empleado.Nombre);
                return RedirectToAction("Movimientos", "Banco");
            }

            int fallidos = (HttpContext.Session.GetInt32(ClaveIntentos) ?? 0) + 1;
            HttpContext.Session.SetInt32(ClaveIntentos, fallidos);
            modelo = CrearModelo();
            modelo.Usuario = entrada.Usuario;
            modelo.Error = modelo.Bloqueado
                ? $"Se superaron los {IntentosMaximos} intentos. Cierre el navegador para volver a intentarlo."
                : $"Usuario o clave incorrectos. Intentos restantes: {modelo.IntentosRestantes}.";
        }
        catch (ErrorServicioException ex)
        {
            // Un error de conexión no cuenta como intento fallido.
            modelo.Error = ex.Message;
        }
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    private LoginViewModel CrearModelo()
    {
        int fallidos = HttpContext.Session.GetInt32(ClaveIntentos) ?? 0;
        return new LoginViewModel
        {
            IntentosRestantes = Math.Max(0, IntentosMaximos - fallidos),
            Bloqueado = fallidos >= IntentosMaximos
        };
    }
}
