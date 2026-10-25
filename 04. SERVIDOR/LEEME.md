# 04. SERVIDOR — Servicio SOAP EurekaBank (WCF + SQL Server)

.NET Framework 4.7.2, IIS Express en `http://localhost:8087/WSEureka.svc`.

```
EUREKA_SOAP_DOTNET_SERVIDOR/
├── ec/edu/monster/db/ConexionBD.cs          SINGLETON: lee "EurekaBank" de Web.config y abre conexiones
├── ec/edu/monster/modelo/Modelos.cs         Empleado, Cuenta, Movimiento, ErrorServicio (DataContract)
├── ec/edu/monster/servicio/EurekaServicio.cs SINGLETON: login con 3 intentos y bloqueo de 2 min, validaciones, consultas y transacciones (depósito, retiro, transferencia)
├── ec/edu/monster/ws/IWSEureka.cs           contrato SOAP (namespace http://espe.edu.ec/eureka/)
├── ec/edu/monster/ws/WSEureka.svc.cs        implementación WCF: delega y convierte errores en SOAP Fault
├── ec/edu/monster/ws/WSEurekaAsmx.asmx.cs    las mismas operaciones como servicio ASMX (página con formularios de prueba)
├── WSEureka.svc
├── WSEureka.asmx
└── Web.config                               cadena de conexión (login monster/monster) + endpoint
```

Requisito: la base `EUREKABANK` creada con `03. BDD\crear_bdd.ps1`.
Publicar en red: `publicar_iis.ps1` (como administrador). Ver el README principal.
