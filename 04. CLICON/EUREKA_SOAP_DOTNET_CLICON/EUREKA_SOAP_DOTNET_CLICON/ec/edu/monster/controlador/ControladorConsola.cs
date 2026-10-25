using System.Collections.Generic;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;
using ec.edu.monster.vista;

namespace ec.edu.monster.controlador
{
    /// <summary>
    /// Flujo de la aplicación: login → menú → consulta de movimientos, depósito, retiro o transferencia.
    /// No aplica reglas del banco (formato de cuenta, importe, saldo...): las valida el servidor
    /// y el controlador solo muestra su mensaje.
    /// </summary>
    public class ControladorConsola
    {
        private const int IntentosLogin = 3;

        private readonly VistaConsola _vista;
        private readonly ClienteEureka _cliente;
        private Empleado _empleado;

        public ControladorConsola(VistaConsola vista, ClienteEureka cliente)
        {
            _vista = vista;
            _cliente = cliente;
        }

        public void Iniciar()
        {
            if (!IniciarSesion())
            {
                return;
            }
            MenuPrincipal();
            _vista.MostrarEncabezado("Sesión finalizada. ¡Hasta pronto!");
        }

        private bool IniciarSesion()
        {
            for (int intento = 1; intento <= IntentosLogin; intento++)
            {
                _vista.MostrarEncabezado($"INICIO DE SESIÓN  (intento {intento} de {IntentosLogin})");
                _vista.MostrarServidor(_cliente.UrlServidor);

                string usuario = _vista.PedirTexto("Usuario");
                string clave = _vista.PedirClave("Clave");

                try
                {
                    _empleado = _cliente.Login(usuario, clave);
                    if (_empleado != null)
                    {
                        _vista.MostrarExito($"Bienvenido, {_empleado.Nombre} (empleado {_empleado.Codigo}).");
                        _vista.Pausa();
                        return true;
                    }
                    _vista.MostrarError("Usuario o clave incorrectos.");
                }
                catch (ErrorServicioException ex)
                {
                    _vista.MostrarError(ex.Message);
                }
                _vista.Pausa();
            }

            _vista.MostrarEncabezado("ACCESO BLOQUEADO");
            _vista.MostrarError($"Se superaron los {IntentosLogin} intentos. La aplicación se cerrará.");
            _vista.Pausa();
            return false;
        }

        private void MenuPrincipal()
        {
            var opciones = new List<string>
            {
                "Consultar cuenta y movimientos",
                "Depósito",
                "Retiro",
                "Transferencia"
            };
            while (true)
            {
                _vista.MostrarEncabezado($"MENÚ PRINCIPAL · {_empleado.Nombre}");
                int opcion = _vista.PedirOpcion(opciones, "Cerrar sesión");
                switch (opcion)
                {
                    case 0: _empleado = null; return;
                    case 1: ConsultarMovimientos(); break;
                    case 2: RealizarOperacion(TipoOperacion.Deposito); break;
                    case 3: RealizarOperacion(TipoOperacion.Retiro); break;
                    case 4: RealizarOperacion(TipoOperacion.Transferencia); break;
                }
            }
        }

        private void ConsultarMovimientos()
        {
            do
            {
                _vista.MostrarEncabezado("CONSULTAR CUENTA Y MOVIMIENTOS");
                string cuenta = _vista.PedirCuenta("Número de cuenta");
                try
                {
                    Cuenta datos = _cliente.ConsultarCuenta(cuenta);
                    Movimiento[] movimientos = _cliente.TraerMovimientos(cuenta);
                    _vista.MostrarCuenta(datos);
                    _vista.MostrarMovimientos(movimientos);
                }
                catch (ErrorServicioException ex)
                {
                    _vista.MostrarError(ex.Message);
                }
            }
            while (_vista.PreguntarSiNo("¿Consultar otra cuenta?"));
        }

        private void RealizarOperacion(TipoOperacion tipo)
        {
            do
            {
                _vista.MostrarEncabezado(VistaConsola.Titulo(tipo).ToUpperInvariant());
                string cuenta = _vista.PedirCuenta(tipo == TipoOperacion.Transferencia ? "Cuenta de origen" : "Número de cuenta");
                string destino = tipo == TipoOperacion.Transferencia ? _vista.PedirCuenta("Cuenta de destino") : null;
                decimal importe = _vista.PedirImporte("Importe");

                if (!_vista.PreguntarSiNo($"¿Confirma {VistaConsola.Titulo(tipo).ToLowerInvariant()} de {VistaConsola.Dinero(importe)}?"))
                {
                    _vista.MostrarError("Operación cancelada.");
                    continue;
                }

                try
                {
                    decimal saldo;
                    switch (tipo)
                    {
                        case TipoOperacion.Deposito:
                            saldo = _cliente.RegistrarDeposito(cuenta, importe, _empleado.Codigo);
                            break;
                        case TipoOperacion.Retiro:
                            saldo = _cliente.RegistrarRetiro(cuenta, importe, _empleado.Codigo);
                            break;
                        default:
                            saldo = _cliente.RegistrarTransferencia(cuenta, destino, importe, _empleado.Codigo);
                            break;
                    }
                    _vista.MostrarOperacionExitosa(tipo, cuenta, destino, importe, saldo);
                }
                catch (ErrorServicioException ex)
                {
                    _vista.MostrarError(ex.Message);
                }
            }
            while (_vista.PreguntarSiNo("¿Realizar otra operación de este tipo?"));
        }
    }
}
