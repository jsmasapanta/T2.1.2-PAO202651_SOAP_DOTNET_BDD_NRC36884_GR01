namespace ec.edu.monster.modelo;

/// <summary>Empleado que inició sesión; su código va en cada operación.</summary>
public record Empleado(string Codigo, string Usuario, string Nombre);

/// <summary>Datos de una cuenta tal como los devuelve consultarCuenta.</summary>
public record Cuenta(string Codigo, string Cliente, string Moneda, string Sucursal,
                     decimal Saldo, string Estado, DateTime FechaCreacion, int NumeroMovimientos);

/// <summary>Un movimiento de la cuenta (traerMovimientos).</summary>
public record Movimiento(int Numero, DateTime Fecha, string Tipo, string Accion,
                         decimal Importe, string Empleado, string CuentaReferencia)
{
    public bool EsIngreso => Accion == "INGRESO";
}

/// <summary>Operaciones de dinero que ofrece el menú.</summary>
public enum TipoOperacion
{
    Deposito,
    Retiro,
    Transferencia
}
