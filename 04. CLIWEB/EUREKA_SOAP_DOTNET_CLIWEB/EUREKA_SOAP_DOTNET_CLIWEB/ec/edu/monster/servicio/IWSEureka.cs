using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio;

/// <summary>
/// Copia del contrato del servidor (mismo Name y Namespace), en versión asíncrona.
/// Con esto el cliente se conecta con ChannelFactory, sin generar proxies.
/// </summary>
[ServiceContract(Name = "WSEureka", Namespace = ConstantesServicio.Namespace)]
public interface IWSEureka
{
    [OperationContract(Name = "login")]
    [FaultContract(typeof(ErrorServicio))]
    Task<Empleado?> LoginAsync(string usuario, string clave);

    [OperationContract(Name = "consultarCuenta")]
    [FaultContract(typeof(ErrorServicio))]
    Task<Cuenta> ConsultarCuentaAsync(string cuenta);

    [OperationContract(Name = "traerMovimientos")]
    [FaultContract(typeof(ErrorServicio))]
    Task<Movimiento[]> TraerMovimientosAsync(string cuenta);

    [OperationContract(Name = "registrarDeposito")]
    [FaultContract(typeof(ErrorServicio))]
    Task<decimal> RegistrarDepositoAsync(string cuenta, decimal importe, string codEmpleado);

    [OperationContract(Name = "registrarRetiro")]
    [FaultContract(typeof(ErrorServicio))]
    Task<decimal> RegistrarRetiroAsync(string cuenta, decimal importe, string codEmpleado);

    [OperationContract(Name = "registrarTransferencia")]
    [FaultContract(typeof(ErrorServicio))]
    Task<decimal> RegistrarTransferenciaAsync(string cuentaOrigen, string cuentaDestino, decimal importe, string codEmpleado);
}
