# 04. CLIMOV — Cliente móvil (.NET MAUI Android, sucesor de Xamarin.Forms)

```
ec/edu/monster/modelo/Modelos.cs              Empleado, Cuenta, Movimiento, TipoOperacion
ec/edu/monster/servicio/ClienteSoap.cs        SOAP 1.1 con HttpClient · SINGLETON
ec/edu/monster/controlador/ControladorEureka.cs  sesión, validación, formato · SINGLETON
ec/edu/monster/vista/LoginPage                login (3 intentos) + URL del servidor
ec/edu/monster/vista/MenuPage                 4 opciones + operaciones de la sesión
ec/edu/monster/vista/MovimientosPage          saldo y movimientos de una cuenta
ec/edu/monster/vista/OperacionPage            depósito, retiro o transferencia
```

URL en la app: `http://IP-DE-LA-PC:8087/WSEureka.svc` (WiFi) o `http://localhost:8087/WSEureka.svc` con `adb reverse tcp:8087 tcp:8087`.
`Directory.Build.props` envía `obj` a `%LOCALAPPDATA%\EUREKA_CLIMOV\obj` por el límite de 260 caracteres.
