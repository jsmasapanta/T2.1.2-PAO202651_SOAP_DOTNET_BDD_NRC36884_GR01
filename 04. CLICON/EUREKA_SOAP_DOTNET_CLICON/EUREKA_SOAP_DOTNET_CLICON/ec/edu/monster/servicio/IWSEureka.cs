using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.servicio
{
    /// <summary>
    /// Copia del contrato del servidor (mismo Name y Namespace).
    /// Con esto el cliente se conecta con ChannelFactory, sin generar proxies.
    /// </summary>
    [ServiceContract(Name = "WSEureka", Namespace = ConstantesServicio.Namespace)]
    public interface IWSEureka
    {
        [OperationContract(Name = "login")]
        [FaultContract(typeof(ErrorServicio))]
        Empleado Login(string usuario, string clave);

        [OperationContract(Name = "consultarCuenta")]
        [FaultContract(typeof(ErrorServicio))]
        Cuenta ConsultarCuenta(string cuenta);

        [OperationContract(Name = "traerMovimientos")]
        [FaultContract(typeof(ErrorServicio))]
        Movimiento[] TraerMovimientos(string cuenta);

        [OperationContract(Name = "registrarDeposito")]
        [FaultContract(typeof(ErrorServicio))]
        decimal RegistrarDeposito(string cuenta, decimal importe, string codEmpleado);

        [OperationContract(Name = "registrarRetiro")]
        [FaultContract(typeof(ErrorServicio))]
        decimal RegistrarRetiro(string cuenta, decimal importe, string codEmpleado);

        [OperationContract(Name = "registrarTransferencia")]
        [FaultContract(typeof(ErrorServicio))]
        decimal RegistrarTransferencia(string cuentaOrigen, string cuentaDestino, decimal importe, string codEmpleado);
    }
}
