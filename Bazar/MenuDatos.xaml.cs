using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Controls;
using Java.Sql;

namespace Bazar;

public partial class MenuDatos : ContentPage
{
    public MenuDatos(int idUsuario)
    {
        InitializeComponent();  

        this.ID = idUsuario;

        ListaEmpeños = new ObservableCollection<Empeños>();
        ListaClientes = new ObservableCollection<Clientes>();

        CargarClientes();
        CargarEmpeños();

        pckClienteEmpeno.ItemsSource = ListaClientes;

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarEmpeños();
        CargarClientes();
    }

    public int ID { get; set; }

    public ObservableCollection<Empeños> ListaEmpeños{get;set;}
    public ObservableCollection<Clientes> ListaClientes{get;set;}

    public class Empeños
    {
        public int id { get; set; }
        public int id_cliente{get;set;}
        public int id_usuarios { get; set; }
        public string descripcion_articulo { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public decimal monto_prestado { get; set; }
        public decimal monto_abonado { get; set; }
        public decimal tasa_interes { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_vence { get; set; }
        public string estado { get; set; } = string.Empty;

        public double ProgresoAbono => monto_prestado > 0
        ? (double)(monto_abonado / monto_prestado)
        : 0;
        public Color EstadoColor => estado switch
        {
            "Activo"    => Color.FromArgb("#007AFF"),
            "Liquidado" => Color.FromArgb("#34C759"),
            "Vencido"   => Color.FromArgb("#FF3B30"),
            _           => Color.FromArgb("#8E8E93")
        };
    }

    public async void CargarEmpeños()
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

                    var opciones = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var listaApi = JsonSerializer.Deserialize<List<Empeños>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaEmpeños.Clear();
                        foreach (var lisCarg in listaApi)
                        {
                            ListaEmpeños.Add(lisCarg);
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

    public class Clientes
    {
        public int id { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string telefono { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
    }

    public async void CargarClientes()
    {
        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"Bazar/clientes/{ID}";

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

                    var listaApi = JsonSerializer.Deserialize<List<Clientes>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaClientes.Clear();
                        foreach (var lisCarg in listaApi)
                        {
                            ListaClientes.Add(lisCarg);
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

    private async void ListaEmpeñosCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var Seleccionada = e.CurrentSelection.FirstOrDefault() as Empeños;
        if (Seleccionada == null) return;

        var clienteAsociado = ListaClientes.FirstOrDefault(c => c.id == Seleccionada.id_cliente) ?? new Clientes();

        var collection = sender as CollectionView;
        if (collection != null)
            collection.SelectedItem = null;

        var menuEmpeños = new MenuEmpeños(ID, Seleccionada, clienteAsociado);
        await Shell.Current.Navigation.PushAsync(menuEmpeños, false);
    }

    private async void BtnEliminarEmpeños_Clicked(object? sender, EventArgs e)
    {
        var buton = sender as Button;
        var Seleccionado = buton?.CommandParameter as Empeños;

        if(Seleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", "¿Estás seguro de eliminar este Empeño?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"Bazar/eliminarEmpeños/{Seleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarEmpeños();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }
    }

    private async void BtnAgregarEmpeños_Clicked(object? sender, EventArgs e)
    {
        PopupEmpeños.IsVisible = true;
        PopupEmpeños.Opacity = 0;
        await PopupEmpeños.FadeToAsync(1, 250); 
    }

    private async void BtnCerrarPopupEmpeno_Clicked(object? sender, EventArgs e)
    {
        await PopupEmpeños.FadeToAsync(0, 200); 
        PopupEmpeños.IsVisible = false;
    }

    private async void BtnGuardarPopupEmpeno_Clicked(object? sender, EventArgs e)
    {
        var clienteSeleccionado = pckClienteEmpeno.SelectedItem as Clientes;

        var Empeño = new Empeños
        {
            id_cliente = clienteSeleccionado?.id ?? 0,
            id_usuarios = ID,
            descripcion_articulo = txtDescripcionEmpeno.Text,
            categoria = pckCategoriaEmpeno.SelectedItem?.ToString() ?? string.Empty,
            monto_prestado = decimal.Parse(txtMontoPrestadoEmpeno.Text),
            tasa_interes = decimal.Parse(txtTasaInteresEmpeno.Text),
            fecha_vence = dtpFechaVenceEmpeno.Date ?? DateTime.Now,
        };

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "Bazar/guardarEmpeños";

            try
            {
                var json = JsonSerializer.Serialize(Empeño);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    txtDescripcionEmpeno.Text = string.Empty;
                    pckCategoriaEmpeno.SelectedIndex = -1;
                    txtMontoPrestadoEmpeno.Text = string.Empty;
                    txtTasaInteresEmpeno.Text = string.Empty;
                    dtpFechaVenceEmpeno.Date = DateTime.Now;

                    await PopupEmpeños.FadeToAsync(0, 200);
                    PopupEmpeños.IsVisible = false;

                    CargarEmpeños();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de conexion", ex.Message, "OK");                
            }
        }
    }

    private void ListaClientesCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    private async void BtnEliminarClientes_Clicked(object? sender, EventArgs e)
    {
        var buton = sender as Button;
        var Seleccionado = buton?.CommandParameter as Clientes;

        if(Seleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", "¿Estás seguro de eliminar este Cliente?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"Bazar/eliminarClientes/{Seleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarClientes();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }
    }

    private async void BtnAgregarClientes_Clicked(object? sender, EventArgs e)
    {
        PopupClientes.IsVisible = true;
        PopupClientes.Opacity = 0;
        await PopupClientes.FadeToAsync(1, 250); 
    }

    private async void BtnCerrarPopupCliente_Clicked(object? sender, EventArgs e)
    {
        await PopupClientes.FadeToAsync(0, 200); 
        PopupClientes.IsVisible = false;
    }

    private async void BtnGuardarPopupCliente_Clicked(object? sender, EventArgs e)
    {
        var Cliente = new Clientes
        {
            id = ID,
            nombre = txtNombreCliente.Text,
            telefono = txtTelefonoCliente.Text,
            identificacion = txtIdentificacionCliente.Text
        };

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "Bazar/guardarClientes";

            try
            {
                var json = JsonSerializer.Serialize(Cliente);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    txtNombreCliente.Text = string.Empty;
                    txtTelefonoCliente.Text = string.Empty;
                    txtIdentificacionCliente.Text = string.Empty;

                    await PopupClientes.FadeToAsync(0, 200);
                    PopupClientes.IsVisible = false;

                    CargarClientes();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de conexion", ex.Message, "OK");                
            }
        }
    }
}
