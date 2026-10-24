using System;
using System.Runtime.Serialization;

namespace ec.edu.monster.modelo
{
    public static class Constantes
    {
        /// <summary>
        /// Namespace XML del servicio. Los clientes deben usar exactamente este valor
        /// (por ejemplo en el sobre SOAP que arma el cliente móvil).
        /// </summary>
        public const string NamespaceServicio = "http://espe.edu.ec/eureka/";

        /// <summary>Códigos de TipoMovimiento usados por el servicio.</summary>
        public const string TipoDeposito = "003";
        public const string TipoRetiro = "004";
        public const string TipoTransferenciaIngreso = "008";
        public const string TipoTransferenciaSalida = "009";
    }

    /// <summary>Empleado que inició sesión (tabla Empleado). Su código queda en cada movimiento.</summary>
    [DataContract(Namespace = Constantes.NamespaceServicio)]
    public class Empleado
    {
        [DataMember(Order = 1)] public string Codigo { get; set; }
        [DataMember(Order = 2)] public string Usuario { get; set; }
        [DataMember(Order = 3)] public string Nombre { get; set; }
    }

    /// <summary>Datos de una cuenta con su cliente, moneda y sucursal (tablas Cuenta, Cliente, Moneda, Sucursal).</summary>
    [DataContract(Namespace = Constantes.NamespaceServicio)]
    public class Cuenta
    {
        [DataMember(Order = 1)] public string Codigo { get; set; }
        [DataMember(Order = 2)] public string Cliente { get; set; }
        [DataMember(Order = 3)] public string Moneda { get; set; }
        [DataMember(Order = 4)] public string Sucursal { get; set; }
        [DataMember(Order = 5)] public decimal Saldo { get; set; }
        [DataMember(Order = 6)] public string Estado { get; set; }
        [DataMember(Order = 7)] public DateTime FechaCreacion { get; set; }
        [DataMember(Order = 8)] public int NumeroMovimientos { get; set; }
    }

    /// <summary>Un movimiento de la cuenta (tabla Movimiento + TipoMovimiento).</summary>
    [DataContract(Namespace = Constantes.NamespaceServicio)]
    public class Movimiento
    {
        [DataMember(Order = 1)] public int Numero { get; set; }
        [DataMember(Order = 2)] public DateTime Fecha { get; set; }
        [DataMember(Order = 3)] public string Tipo { get; set; }
        /// <summary>INGRESO o SALIDA.</summary>
        [DataMember(Order = 4)] public string Accion { get; set; }
        [DataMember(Order = 5)] public decimal Importe { get; set; }
        [DataMember(Order = 6)] public string Empleado { get; set; }
        /// <summary>En transferencias, la otra cuenta; vacío en los demás.</summary>
        [DataMember(Order = 7)] public string CuentaReferencia { get; set; }
    }

    /// <summary>
    /// Detalle que viaja dentro del SOAP Fault cuando una petición no se puede atender.
    /// Los clientes muestran "Mensaje" al usuario.
    /// </summary>
    [DataContract(Namespace = Constantes.NamespaceServicio)]
    public class ErrorServicio
    {
        [DataMember(Order = 1)] public string Codigo { get; set; }
        [DataMember(Order = 2)] public string Mensaje { get; set; }

        public ErrorServicio(string codigo, string mensaje)
        {
            Codigo = codigo;
            Mensaje = mensaje;
        }
    }
}
