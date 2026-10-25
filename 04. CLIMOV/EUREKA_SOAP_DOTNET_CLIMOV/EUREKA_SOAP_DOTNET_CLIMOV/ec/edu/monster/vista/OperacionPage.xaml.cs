using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;

namespace ec.edu.monster.vista;

/// <summary>Depósito, retiro o transferencia (la misma pantalla; la transferencia pide además la cuenta de destino).</summary>
public partial class OperacionPage : ContentPage
{
    private readonly ControladorEureka _controlador = ControladorEureka.Instancia;
    private readonly TipoOperacion _tipo;
    private readonly Color _color;

    public OperacionPage(TipoOperacion tipo)
    {
        InitializeComponent();
        _tipo = tipo;
        _color = ControladorEureka.ColorDe(tipo);

        Title = ControladorEureka.Titulo(tipo);
        BtnRegistrar.Text = "REGISTRAR " + Title.ToUpperInvariant();
        BtnRegistrar.BackgroundColor = _color;
        Cargando.Color = _color;

        bool esTransferencia = tipo == TipoOperacion.Transferencia;
        LblCuenta.Text = esTransferencia ? "Cuenta de origen" : "Cuenta";
        LblDestino.IsVisible = TxtDestino.IsVisible = esTransferencia;
        LblAyuda.Text = tipo switch
        {
            TipoOperacion.Deposito => "Suma el importe al saldo de la cuenta.",
            TipoOperacion.Retiro => "Resta el importe del saldo; la cuenta debe tener saldo suficiente.",
            _ => "Pasa el importe de una cuenta a otra de la misma moneda."
        };
    }

    private async void OnRegistrarClicked(object? sender, EventArgs e)
    {
        string cuenta = (TxtCuenta.Text ?? "").Trim();
        string? destino = _tipo == TipoOperacion.Transferencia ? (TxtDestino.Text ?? "").Trim() : null;
        decimal? importe = ControladorEureka.LeerImporte(TxtImporte.Text);
        TxtCuenta.Unfocus(); TxtDestino.Unfocus(); TxtImporte.Unfocus(); // oculta el teclado para ver el resultado

        // Las reglas (formato de cuenta, importe, cuentas distintas, saldo) las valida el servidor.
        string? error = null;
        if (cuenta.Length == 0 || destino?.Length == 0)
            error = "Complete los números de cuenta.";
        else if (importe == null)
            error = "Escriba el importe con números (ejemplo: 150 o 75,50).";
        if (error != null)
        {
            MostrarError(error);
            return;
        }

        string pregunta = destino != null
            ? $"¿Transferir {ControladorEureka.Dinero(importe!.Value)} de la cuenta {cuenta} a la cuenta {destino}?"
            : $"¿Registrar {Title.ToLowerInvariant()} de {ControladorEureka.Dinero(importe!.Value)} en la cuenta {cuenta}?";
        if (!await DisplayAlert("Confirmar", pregunta, "Sí", "No"))
        {
            return;
        }

        Ocupado(true);
        try
        {
            decimal saldo = await _controlador.RegistrarAsync(_tipo, cuenta, destino, importe.Value);
            LblResultado.TextColor = _color;
            LblResultado.Text = $"{Title} registrad{(destino != null ? "a" : "o")}";
            LblDetalle.Text = $"Importe: {ControladorEureka.Dinero(importe.Value)}\nNuevo saldo de {cuenta}: {ControladorEureka.Dinero(saldo)}";
            TxtImporte.Text = "";
        }
        catch (ErrorServicioException ex)
        {
            MostrarError(ex.Message);
        }
        finally
        {
            Ocupado(false);
        }
    }

    private void MostrarError(string mensaje)
    {
        LblResultado.TextColor = Color.FromArgb("#C0392B");
        LblResultado.Text = "No se pudo registrar";
        LblDetalle.Text = mensaje;
    }

    private void Ocupado(bool ocupado)
    {
        BtnRegistrar.IsEnabled = !ocupado;
        Cargando.IsRunning = Cargando.IsVisible = ocupado;
    }
}
