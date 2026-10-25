namespace ec.edu.monster.modelo
{
    /// <summary>
    /// Opciones de dinero que muestra el menú. Los datos (Empleado, Cuenta, Movimiento) no se definen
    /// en el cliente: vienen del servicio en servicio/WSEurekaReference.cs, generado desde el WSDL.
    /// </summary>
    public enum TipoOperacion
    {
        Deposito,
        Retiro,
        Transferencia
    }
}
