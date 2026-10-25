using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.ws
{
    /// <summary>
    /// Las mismas 6 operaciones de WSEureka.svc publicadas también como servicio web ASMX (WSEureka.asmx).
    /// ASMX es la tecnología SOAP anterior a WCF; su página en el navegador lista las operaciones
    /// y permite probarlas con un formulario. Usa la misma lógica (EurekaServicio) y la misma base de datos.
    /// </summary>
    [WebService(Name = "WSEureka", Namespace = Constantes.NamespaceServicio,
        Description = "EurekaBank · SOAP .NET con SQL Server. Usuario de prueba: monster / monster69 (código de empleado 0012). Cuentas de ejemplo: 00100001, 00200001, 00200002.")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class WSEurekaAsmx : WebService
    {
        [WebMethod(Description = "Devuelve el empleado. Si usuario o clave no coinciden responde un error con los intentos restantes (bloqueo de 2 minutos al tercero).")]
        public Empleado Login(string usuario, string clave) =>
            Ejecutar(() => EurekaServicio.Instancia.Login(usuario, clave));

        [WebMethod(Description = "Datos de la cuenta: cliente, moneda, sucursal, saldo y estado.")]
        public Cuenta ConsultarCuenta(string cuenta) =>
            Ejecutar(() => EurekaServicio.Instancia.ConsultarCuenta(cuenta));

        [WebMethod(Description = "Movimientos de la cuenta, el más reciente primero.")]
        public Movimiento[] TraerMovimientos(string cuenta) =>
            Ejecutar(() => EurekaServicio.Instancia.TraerMovimientos(cuenta).ToArray());

        [WebMethod(Description = "Suma el importe al saldo (movimiento 003). Devuelve el nuevo saldo.")]
        public decimal RegistrarDeposito(string cuenta, decimal importe, string codEmpleado) =>
            Ejecutar(() => EurekaServicio.Instancia.RegistrarDeposito(cuenta, importe, codEmpleado));

        [WebMethod(Description = "Resta el importe si alcanza el saldo (movimiento 004). Devuelve el nuevo saldo.")]
        public decimal RegistrarRetiro(string cuenta, decimal importe, string codEmpleado) =>
            Ejecutar(() => EurekaServicio.Instancia.RegistrarRetiro(cuenta, importe, codEmpleado));

        [WebMethod(Description = "Pasa el importe entre cuentas de la misma moneda (movimientos 009 y 008). Devuelve el saldo de la cuenta de origen.")]
        public decimal RegistrarTransferencia(string cuentaOrigen, string cuentaDestino, decimal importe, string codEmpleado) =>
            Ejecutar(() => EurekaServicio.Instancia.RegistrarTransferencia(cuentaOrigen, cuentaDestino, importe, codEmpleado));

        // Mismos códigos de error que WSEureka.svc: el detalle del Fault lleva <ErrorServicio><Codigo/><Mensaje/>.
        private T Ejecutar<T>(Func<T> operacion)
        {
            try
            {
                return operacion();
            }
            catch (ArgumentException ex)
            {
                throw Falla("VALOR_INVALIDO", ex.Message);
            }
            catch (ReglaNegocioException ex)
            {
                throw Falla("REGLA_NEGOCIO", ex.Message);
            }
            catch (SqlException ex)
            {
                Trace.TraceError("SQL Server: " + ex.Message);
                throw Falla("ERROR_BD", "No se pudo acceder a la base de datos EUREKABANK. Verifique que SQL Server esté encendido.");
            }
            catch (InvalidOperationException ex)
            {
                throw Falla("ERROR_SERVIDOR", ex.Message);
            }
        }

        private SoapException Falla(string codigo, string mensaje)
        {
            // ArgumentException agrega "Nombre del parámetro: ..." al mensaje; nos quedamos con la primera línea.
            string limpio = mensaje.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];

            var documento = new XmlDocument();
            XmlNode detalle = documento.CreateNode(XmlNodeType.Element,
                SoapException.DetailElementName.Name, SoapException.DetailElementName.Namespace);
            XmlElement error = documento.CreateElement("ErrorServicio", Constantes.NamespaceServicio);
            XmlElement nodoCodigo = documento.CreateElement("Codigo", Constantes.NamespaceServicio);
            nodoCodigo.InnerText = codigo;
            XmlElement nodoMensaje = documento.CreateElement("Mensaje", Constantes.NamespaceServicio);
            nodoMensaje.InnerText = limpio;
            error.AppendChild(nodoCodigo);
            error.AppendChild(nodoMensaje);
            detalle.AppendChild(error);

            return new SoapException(limpio, SoapException.ClientFaultCode, Context.Request.Url.AbsoluteUri, detalle);
        }
    }
}
