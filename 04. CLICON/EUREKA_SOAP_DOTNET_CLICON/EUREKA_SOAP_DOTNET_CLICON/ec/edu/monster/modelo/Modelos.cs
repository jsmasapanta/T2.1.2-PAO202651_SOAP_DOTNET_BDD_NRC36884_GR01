using System;
using System.Runtime.Serialization;

namespace ec.edu.monster.modelo
{
    /// <summary>
    /// Copia de los datos que devuelve el servidor (mismo Namespace XML).
    /// WCF los llena al leer la respuesta SOAP.
    /// </summary>
    public static class ConstantesServicio
    {
        public const string Namespace = "http://espe.edu.ec/eureka/";
    }

    [DataContract(Namespace = ConstantesServicio.Namespace)]
    public class Empleado
    {
        [DataMember(Order = 1)] public string Codigo { get; set; }
        [DataMember(Order = 2)] public string Usuario { get; set; }
        [DataMember(Order = 3)] public string Nombre { get; set; }
    }

    [DataContract(Namespace = ConstantesServicio.Namespace)]
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

    [DataContract(Namespace = ConstantesServicio.Namespace)]
    public class Movimiento
    {
        [DataMember(Order = 1)] public int Numero { get; set; }
        [DataMember(Order = 2)] public DateTime Fecha { get; set; }
        [DataMember(Order = 3)] public string Tipo { get; set; }
        [DataMember(Order = 4)] public string Accion { get; set; }
        [DataMember(Order = 5)] public decimal Importe { get; set; }
        [DataMember(Order = 6)] public string Empleado { get; set; }
        [DataMember(Order = 7)] public string CuentaReferencia { get; set; }

        public bool EsIngreso => Accion == "INGRESO";
    }

    [DataContract(Namespace = ConstantesServicio.Namespace)]
    public class ErrorServicio
    {
        [DataMember(Order = 1)] public string Codigo { get; set; }
        [DataMember(Order = 2)] public string Mensaje { get; set; }
    }

    /// <summary>Operaciones de dinero que ofrece el menú.</summary>
    public enum TipoOperacion
    {
        Deposito,
        Retiro,
        Transferencia
    }
}
