using System;
using System.Globalization;
using System.Threading.Tasks;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.controlador
{
    /// <summary>
    /// Intermediario entre las ventanas y el servicio SOAP. Guarda el empleado que inició sesión
    /// (su código va en cada depósito, retiro o transferencia).
    /// Las llamadas son asíncronas para que la ventana no se congele mientras responde el servidor.
    /// </summary>
    public class ControladorEureka
    {
        private readonly ClienteEureka _cliente = ClienteEureka.Instancia;

        public string UrlServidor => _cliente.UrlServidor;

        public Empleado Empleado { get; private set; }

        public async Task<bool> IniciarSesionAsync(string usuario, string clave)
        {
            Empleado = await Task.Run(() => _cliente.Login(usuario, clave));
            return Empleado != null;
        }

        public void CerrarSesion()
        {
            Empleado = null;
        }

        public Task<Cuenta> ConsultarCuentaAsync(string cuenta)
        {
            return Task.Run(() => _cliente.ConsultarCuenta(cuenta));
        }

        public Task<Movimiento[]> TraerMovimientosAsync(string cuenta)
        {
            return Task.Run(() => _cliente.TraerMovimientos(cuenta));
        }

        /// <summary>Registra la operación y devuelve el nuevo saldo (de la cuenta de origen en transferencias).</summary>
        public Task<decimal> RegistrarAsync(TipoOperacion tipo, string cuenta, string destino, decimal importe)
        {
            string empleado = Empleado?.Codigo;
            switch (tipo)
            {
                case TipoOperacion.Deposito:
                    return Task.Run(() => _cliente.RegistrarDeposito(cuenta, importe, empleado));
                case TipoOperacion.Retiro:
                    return Task.Run(() => _cliente.RegistrarRetiro(cuenta, importe, empleado));
                default:
                    return Task.Run(() => _cliente.RegistrarTransferencia(cuenta, destino, importe, empleado));
            }
        }

        /// <summary>Valida que la cuenta tenga 8 dígitos antes de llamar al servidor.</summary>
        public static bool EsCuentaValida(string cuenta)
        {
            string limpia = (cuenta ?? string.Empty).Trim();
            if (limpia.Length != 8) return false;
            foreach (char c in limpia)
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;
        }

        /// <summary>Acepta punto o coma decimal; exige un valor mayor que cero con hasta 2 decimales.</summary>
        public static bool IntentarLeerImporte(string texto, out decimal importe)
        {
            string normalizado = (texto ?? string.Empty).Trim().Replace(',', '.');
            return decimal.TryParse(normalizado, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out importe)
                && importe > 0 && decimal.Round(importe, 2) == importe;
        }

        /// <summary>Importe con separador de miles y 2 decimales: 6,900.00</summary>
        public static string Dinero(decimal importe)
        {
            return importe.ToString("#,##0.00", CultureInfo.InvariantCulture);
        }

        public static string Titulo(TipoOperacion tipo)
        {
            switch (tipo)
            {
                case TipoOperacion.Deposito: return "Depósito";
                case TipoOperacion.Retiro: return "Retiro";
                default: return "Transferencia";
            }
        }
    }
}
