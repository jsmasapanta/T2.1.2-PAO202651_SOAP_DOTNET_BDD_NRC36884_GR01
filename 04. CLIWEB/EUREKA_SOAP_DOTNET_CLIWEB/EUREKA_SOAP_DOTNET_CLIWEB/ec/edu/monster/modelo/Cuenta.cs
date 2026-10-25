using System.Runtime.Serialization;

namespace ec.edu.monster.modelo;

/// <summary>Datos de una cuenta, tal como los devuelve el servicio. Solo datos (get/set).</summary>
[DataContract(Namespace = "http://espe.edu.ec/eureka/")]
public class Cuenta
{
    [DataMember(Order = 1)] public string Codigo { get; set; } = "";
    [DataMember(Order = 2)] public string Cliente { get; set; } = "";
    [DataMember(Order = 3)] public string Moneda { get; set; } = "";
    [DataMember(Order = 4)] public string Sucursal { get; set; } = "";
    [DataMember(Order = 5)] public decimal Saldo { get; set; }
    [DataMember(Order = 6)] public string Estado { get; set; } = "";
    [DataMember(Order = 7)] public DateTime FechaCreacion { get; set; }
    [DataMember(Order = 8)] public int NumeroMovimientos { get; set; }
}
