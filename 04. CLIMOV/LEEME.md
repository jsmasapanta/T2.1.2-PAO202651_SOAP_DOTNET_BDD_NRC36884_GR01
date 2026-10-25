# 04. CLIMOV — Cliente móvil (.NET MAUI Android, sucesor de Xamarin.Forms)

```
ec/edu/monster/modelo/TipoOperacion.cs        opciones del menú
ec/edu/monster/servicio/ClienteSoap.cs        ChannelFactory<WSEureka> (WCF, System.ServiceModel.Http) · SINGLETON
ec/edu/monster/servicio/WSEurekaReference.cs  contrato y datos GENERADOS desde el WSDL (dotnet-svcutil)
ec/edu/monster/controlador/ControladorEureka.cs  sesión y formato (sin reglas del banco) · SINGLETON
ec/edu/monster/vista/LoginPage                login (3 intentos) + URL del servidor
ec/edu/monster/vista/MenuPage                 4 opciones + operaciones de la sesión
ec/edu/monster/vista/MovimientosPage          saldo y movimientos de una cuenta
ec/edu/monster/vista/OperacionPage            depósito, retiro o transferencia
```

URL en la app: `http://IP-DE-LA-PC:8087/WSEureka.svc` (WiFi) o `http://localhost:8087/WSEureka.svc` con `adb reverse tcp:8087 tcp:8087`.
`Directory.Build.props` envía `obj` a `%LOCALAPPDATA%\EUREKA_CLIMOV\obj` por el límite de 260 caracteres.
