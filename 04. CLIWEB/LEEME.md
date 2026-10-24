# 04. CLIWEB — Cliente web (ASP.NET Core MVC .NET 8)

`http://localhost:5087`. Todo dentro de `ec/edu/monster`:
- `controlador/` AccesoController (login/logout, 3 intentos), BancoController (movimientos y operaciones), RequiereSesionAttribute.
- `vista/` Acceso/Login, Banco/Movimientos, Banco/Operacion, Shared/_Layout (Program.cs indica a Razor que busque aquí).
- `servicio/ClienteEureka.cs` registrado con `AddSingleton` (una sola ChannelFactory).
- `modelo/` Modelos.cs y ViewModels.cs.

URL del servidor SOAP en `appsettings.json` → `ServicioSoap:Url`.
