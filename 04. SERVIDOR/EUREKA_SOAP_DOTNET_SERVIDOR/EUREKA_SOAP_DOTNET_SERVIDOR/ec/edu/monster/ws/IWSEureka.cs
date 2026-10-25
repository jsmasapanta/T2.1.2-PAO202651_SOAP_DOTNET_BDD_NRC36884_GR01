using System.Collections.Generic;
using System.ServiceModel;
using ec.edu.monster.modelo;

namespace ec.edu.monster.ws
{
    /// <summary>
    /// Contrato SOAP de EurekaBank. Los importes y saldos viajan como xs:decimal.
    /// Si algo falla (cuenta inexistente, saldo insuficiente, base de datos caída...)
    /// la operación responde un SOAP Fault con ErrorServicio (Codigo, Mensaje).
    /// </summary>
    [ServiceContract(Name = "WSEureka", Namespace = Constantes.NamespaceServicio)]
    public interface IWSEureka
    {
        /// <summary>Devuelve el empleado; si usuario o clave no coinciden responde un Fault con los intentos restantes.</summary>
        [OperationContract(Name = "login")]
        [FaultContract(typeof(ErrorServicio))]
        Empleado Login(string usuario, string clave);

        [OperationContract(Name = "consultarCuenta")]
        [FaultContract(typeof(ErrorServicio))]
        Cuenta ConsultarCuenta(string cuenta);

        [OperationContract(Name = "traerMovimientos")]
        [FaultContract(typeof(ErrorServicio))]
        List<Movimiento> TraerMovimientos(string cuenta);

        /// <summary>Devuelve el nuevo saldo de la cuenta.</summary>
        [OperationContract(Name = "registrarDeposito")]
        [FaultContract(typeof(ErrorServicio))]
        decimal RegistrarDeposito(string cuenta, decimal importe, string codEmpleado);

        /// <summary>Devuelve el nuevo saldo de la cuenta.</summary>
        [OperationContract(Name = "registrarRetiro")]
        [FaultContract(typeof(ErrorServicio))]
        decimal RegistrarRetiro(string cuenta, decimal importe, string codEmpleado);

        /// <summary>Devuelve el nuevo saldo de la cuenta de origen.</summary>
        [OperationContract(Name = "registrarTransferencia")]
        [FaultContract(typeof(ErrorServicio))]
        decimal RegistrarTransferencia(string cuentaOrigen, string cuentaDestino, decimal importe, string codEmpleado);
    }
}
