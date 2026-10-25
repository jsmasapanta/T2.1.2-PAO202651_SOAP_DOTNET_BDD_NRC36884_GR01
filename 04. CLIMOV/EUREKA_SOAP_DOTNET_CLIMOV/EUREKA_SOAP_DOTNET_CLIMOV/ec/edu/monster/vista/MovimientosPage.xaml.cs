using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.servicio;
using Microsoft.Maui.Controls.Shapes;

namespace ec.edu.monster.vista;

/// <summary>Consulta de una cuenta: datos, saldo y movimientos (el más reciente primero).</summary>
public partial class MovimientosPage : ContentPage
{
    private readonly ControladorEureka _controlador = ControladorEureka.Instancia;

    public MovimientosPage()
    {
        InitializeComponent();
    }

    private async void OnConsultarClicked(object? sender, EventArgs e)
    {
        string cuenta = (TxtCuenta.Text ?? "").Trim();
        TxtCuenta.Unfocus(); // oculta el teclado para que se vea la lista
        if (!ControladorEureka.EsCuentaValida(cuenta))
        {
            MostrarError("La cuenta debe tener 8 dígitos (ejemplo: 00100001).");
            return;
        }

        Ocupado(true);
        try
        {
            var (datos, movimientos) = await _controlador.ConsultarAsync(cuenta);
            LblCliente.Text = $"{datos.Codigo} · {datos.Cliente}";
            LblDetalle.Text = $"{datos.Moneda} · Sucursal {datos.Sucursal} · {datos.Estado}";
            LblSaldo.Text = ControladorEureka.Dinero(datos.Saldo);
            TarjetaCuenta.IsVisible = true;
            MostrarMovimientos(movimientos);
        }
        catch (ErrorServicioException ex)
        {
            TarjetaCuenta.IsVisible = false;
            LblTituloLista.IsVisible = false;
            ListaMovimientos.Children.Clear();
            MostrarError(ex.Message);
        }
        finally
        {
            Ocupado(false);
        }
    }

    private void MostrarMovimientos(List<Movimiento> movimientos)
    {
        LblTituloLista.Text = $"Movimientos ({movimientos.Count})";
        LblTituloLista.IsVisible = true;
        ListaMovimientos.Children.Clear();
        foreach (Movimiento m in movimientos)
        {
            var fila = new Grid { ColumnDefinitions = new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)) };
            var izquierda = new VerticalStackLayout { Spacing = 2 };
            izquierda.Children.Add(new Label { Text = $"#{m.Numero}  {m.Tipo}", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#212529") });
            string referencia = string.IsNullOrEmpty(m.CuentaReferencia) ? "" : $" · ref. {m.CuentaReferencia}";
            izquierda.Children.Add(new Label { Text = $"{m.Fecha:dd/MM/yyyy HH:mm} · emp. {m.Empleado}{referencia}", FontSize = 12, TextColor = Color.FromArgb("#6C757D") });
            var importe = new Label
            {
                Text = (m.EsIngreso ? "+" : "−") + ControladorEureka.Dinero(m.Importe),
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb(m.EsIngreso ? "#198754" : "#C0392B")
            };
            fila.Add(izquierda, 0, 0);
            fila.Add(importe, 1, 0);
            ListaMovimientos.Children.Add(new Border
            {
                Content = fila,
                Padding = new Thickness(14, 10),
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 10 }
            });
        }
        if (movimientos.Count == 0)
        {
            ListaMovimientos.Children.Add(new Label { Text = "La cuenta no tiene movimientos.", TextColor = Color.FromArgb("#6C757D") });
        }
    }

    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        LblError.IsVisible = true;
    }

    private void Ocupado(bool ocupado)
    {
        BtnConsultar.IsEnabled = !ocupado;
        Cargando.IsRunning = Cargando.IsVisible = ocupado;
        if (ocupado)
        {
            LblError.IsVisible = false;
        }
    }
}
