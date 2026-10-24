using ec.edu.monster.controlador;
using ec.edu.monster.servicio;

namespace ec.edu.monster.vista;

/// <summary>Pantalla de inicio de sesión (3 intentos) y configuración de la URL del servidor.</summary>
public partial class LoginPage : ContentPage
{
    private readonly ControladorEureka _controlador = ControladorEureka.Instancia;
    private int _intentos;

    public LoginPage()
    {
        InitializeComponent();
        TxtUrl.Text = _controlador.UrlServidor;
        MostrarIntentos();
    }

    private async void OnIngresarClicked(object? sender, EventArgs e)
    {
        string usuario = TxtUsuario.Text ?? "";
        string clave = TxtClave.Text ?? "";
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
        {
            MostrarError("Ingrese usuario y clave.");
            return;
        }
        if (string.IsNullOrWhiteSpace(TxtUrl.Text))
        {
            MostrarError("Ingrese la URL del servidor.");
            return;
        }

        _controlador.UrlServidor = TxtUrl.Text;
        Ocupado(true);
        try
        {
            if (await _controlador.IniciarSesionAsync(usuario, clave))
            {
                Window.Page = new NavigationPage(new MenuPage())
                {
                    BarBackgroundColor = Color.FromArgb("#0B4F8A"),
                    BarTextColor = Colors.White
                };
                return;
            }

            _intentos++;
            if (_intentos >= ControladorEureka.MaximoIntentos)
            {
                await DisplayAlert("Acceso bloqueado",
                    $"Superó los {ControladorEureka.MaximoIntentos} intentos permitidos. La aplicación se cerrará.", "Aceptar");
                Application.Current?.Quit();
                return;
            }
            TxtClave.Text = "";
            MostrarError("Usuario o clave incorrectos.");
            MostrarIntentos();
        }
        catch (ErrorServicioException ex)
        {
            // Un error de conexión no cuenta como intento fallido.
            MostrarError(ex.Message);
        }
        finally
        {
            Ocupado(false);
        }
    }

    private void MostrarIntentos() =>
        LblIntentos.Text = $"Intento {_intentos + 1} de {ControladorEureka.MaximoIntentos}";

    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        LblError.IsVisible = true;
    }

    private void Ocupado(bool ocupado)
    {
        BtnIngresar.IsEnabled = !ocupado;
        Cargando.IsRunning = Cargando.IsVisible = ocupado;
        if (ocupado)
        {
            LblError.IsVisible = false;
        }
    }
}
