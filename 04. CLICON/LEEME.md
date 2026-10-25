# 04. CLICON — Cliente consola

.NET Framework 4.7.2. MVC en `ec/edu/monster`: `modelo/TipoOperacion.cs`, `vista/VistaConsola.cs`, `controlador/ControladorConsola.cs`, `servicio/ClienteEureka.cs` (SINGLETON, `ChannelFactory<WSEureka>`) y `servicio/WSEurekaReference.cs` (contrato y datos **generados desde el WSDL** con SvcUtil). Sin reglas del banco: las valida el servidor.

Login `monster`/`monster` (3 intentos) → menú: consultar cuenta y movimientos, depósito, retiro, transferencia.
La URL del servidor está en `App.config` (`endpoint name="WSEureka"`).
