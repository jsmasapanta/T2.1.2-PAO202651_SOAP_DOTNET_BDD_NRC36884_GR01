# 04. CLIESC — Cliente escritorio (Windows Forms)

.NET Framework 4.7.2. MVC en `ec/edu/monster`:
- `vista/` FormLogin, FormPrincipal (menú lateral), PanelMovimientos (tabla de movimientos), PanelOperacion (depósito / retiro / transferencia), Tema.
- `controlador/ControladorEureka.cs` llamadas asíncronas, empleado de la sesión, validaciones.
- `servicio/ClienteEureka.cs` SINGLETON con `ChannelFactory<IWSEureka>`.
- `modelo/Modelos.cs`.

La URL del servidor está en `App.config`.
