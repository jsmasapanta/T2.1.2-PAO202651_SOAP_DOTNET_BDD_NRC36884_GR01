# 04. CLIESC — Cliente escritorio (Windows Forms)

.NET Framework 4.7.2. MVC en `ec/edu/monster`:
- `vista/` FormLogin, FormPrincipal (menú lateral), PanelMovimientos (tabla de movimientos), PanelOperacion (depósito / retiro / transferencia), Tema.
- `controlador/ControladorEureka.cs` llamadas asíncronas, empleado de la sesión, validaciones.
- `servicio/ClienteEureka.cs` SINGLETON con `ChannelFactory<WSEureka>`; `servicio/WSEurekaReference.cs` generado desde el WSDL con SvcUtil (Empleado, Cuenta, Movimiento, contrato).
- `modelo/TipoOperacion.cs` (opciones del menú). Sin reglas del banco: las valida el servidor.

La URL del servidor está en `App.config`.
