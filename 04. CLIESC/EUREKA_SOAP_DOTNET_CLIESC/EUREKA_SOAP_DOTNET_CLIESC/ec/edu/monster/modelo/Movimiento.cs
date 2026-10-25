using System;
using System.Runtime.Serialization;

namespace ec.edu.monster.modelo
{
    /// <summary>Un movimiento de la cuenta, tal como lo devuelve el servicio. Solo datos (get/set).</summary>
    [DataContract(Namespace = "http://espe.edu.ec/eureka/")]
    public class Movimiento
    {
        [DataMember(Order = 1)] public int Numero { get; set; }
        [DataMember(Order = 2)] public DateTime Fecha { get; set; }
        [DataMember(Order = 3)] public string Tipo { get; set; }
        /// <summary>INGRESO o SALIDA (lo decide el servidor según TipoMovimiento).</summary>
        [DataMember(Order = 4)] public string Accion { get; set; }
        [DataMember(Order = 5)] public decimal Importe { get; set; }
        [DataMember(Order = 6)] public string Empleado { get; set; }
        [DataMember(Order = 7)] public string CuentaReferencia { get; set; }
    }
}
