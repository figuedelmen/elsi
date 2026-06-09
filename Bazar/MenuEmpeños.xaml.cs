using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Controls;

namespace Bazar;

public partial class MenuEmpeños : ContentPage
{
    public MenuEmpeños(int idUsuario, MenuDatos.Empeños empeño, MenuDatos.Clientes cliente)
    {
        InitializeComponent();

        this.ID = idUsuario;
        this.Empeño = empeño ?? new MenuDatos.Empeños();
        this.Cliente = cliente ?? new MenuDatos.Clientes();

        ListaPagos = new ObservableCollection<Pagos>();

        CargarPagos();

        BindingContext = this;
    }

    public ObservableCollection<Pagos> ListaPagos{get;set;}

    public int ID { get; set; }
    public MenuDatos.Empeños Empeño { get; set; }
    public MenuDatos.Clientes Cliente { get; set; }

    public class Pagos
    {
        public int id { get; set; }
        public int id_empeno { get; set; }
        public DateTime fecha_pago { get; set; } = DateTime.Now;
        public decimal monto { get; set; }
        public string concepto { get; set; } = string.Empty;
    }

    public async void CargarPagos()
    {
        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"Bazar/pagos/{Empeño.id}";

            try
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var opciones = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var listaApi = JsonSerializer.Deserialize<List<Pagos>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaPagos.Clear();
                        foreach (var lisCarg in listaApi)
                        {
                            ListaPagos.Add(lisCarg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("¡Excepción Capturada!", "Error interno: " + ex.Message, "OK");                
            }
        }
    }

    public async void ActualizarEmpeño()
    {
        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"Bazar/empeños/{ID}";
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var lista = JsonSerializer.Deserialize<List<MenuDatos.Empeños>>(json, opciones);
                    var actualizado = lista?.FirstOrDefault(e => e.id == Empeño.id);
                    if (actualizado != null)
                    {
                        Empeño = actualizado;
                        OnPropertyChanged(nameof(Empeño));
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("¡Excepción Capturada!", "Error interno: " + ex.Message, "OK");                
            }
        }
    }

    private async void BtnEliminar_Clicked(object? sender, EventArgs e)
    {
        var buton = sender as Button;
        var Seleccionado = buton?.CommandParameter as Pagos;

        if(Seleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", "¿Estás seguro de eliminar este Pago?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"Bazar/eliminarPago/{Seleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarPagos();
                    ActualizarEmpeño();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }
    }

    private async void BtnAgregar_Clicked(object? sender, EventArgs e)
    {
        PopupPagos.IsVisible = true;
        PopupPagos.Opacity = 0;
        await PopupPagos.FadeToAsync(1, 250);
    }

    private async void BtnCerrarPopupPago_Clicked(object? sender, EventArgs e)
    {
        await PopupPagos.FadeToAsync(0, 200);
        PopupPagos.IsVisible = false;
    }

    private async void BtnGuardarPopupPago_Clicked(object? sender, EventArgs e)
    {
        var montoIngresado = decimal.Parse(txtMontoPago.Text);
        var restante = Empeño.monto_prestado - Empeño.monto_abonado;

        if (montoIngresado > restante)
        {
            await DisplayAlertAsync("Error", $"Solo puedes abonar ${restante:F2}", "OK");
            return;
        }

        var Pago = new Pagos
        {
            id_empeno = Empeño.id,
            monto = montoIngresado,
            concepto = txtConceptoPago.Text,
            fecha_pago = DateTime.Now
        };

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "Bazar/guardarPago";
            try
            {
                var json = JsonSerializer.Serialize(Pago);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    txtMontoPago.Text = string.Empty;
                    txtConceptoPago.Text = string.Empty;

                    await PopupPagos.FadeToAsync(0, 200);
                    PopupPagos.IsVisible = false;

                    CargarPagos();
                    ActualizarEmpeño();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlertAsync("Error", error, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de conexión", ex.Message, "OK");
            }
        }
    }
}