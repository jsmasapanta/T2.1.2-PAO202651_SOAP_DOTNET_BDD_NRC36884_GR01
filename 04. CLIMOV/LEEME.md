# 04. CLIMOV — Cliente móvil (.NET MAUI Android, sucesor de Xamarin.Forms)

```
ec/edu/monster/modelo/                        Empleado, Cuenta, Movimiento (solo get/set) y TipoOperacion
ec/edu/monster/servicio/ClienteSoap.cs        sobre SOAP con HttpClient; llena los modelos · SINGLETON
ec/edu/monster/controlador/ControladorEureka.cs  solo pasa las llamadas y guarda la sesión · SINGLETON
ec/edu/monster/vista/Formato.cs               dinero, títulos y colores
ec/edu/monster/vista/LoginPage                login (mensajes e intentos del servidor) + URL + imagen
ec/edu/monster/vista/MenuPage                 4 opciones + operaciones de la sesión
ec/edu/monster/vista/MovimientosPage          saldo y movimientos de una cuenta
ec/edu/monster/vista/OperacionPage            depósito, retiro o transferencia
```

URL en la app: `http://IP-DE-LA-PC:8087/WSEureka.svc` (WiFi) o `http://localhost:8087/WSEureka.svc` con `adb reverse tcp:8087 tcp:8087`.
`Directory.Build.props` envía `obj` a `%LOCALAPPDATA%\EUREKA_CLIMOV\obj` por el límite de 260 caracteres.
