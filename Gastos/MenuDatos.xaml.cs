using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace Gastos;

public partial class MenuDatos : ContentPage
{
    public MenuDatos(int idUsuario)
    {
        InitializeComponent();  

        this.ID = idUsuario;

        ListaGastos = new ObservableCollection<Gasto>();

        BindingContext = this;

        CargarGastos();
    }

    public int ID;

    public ObservableCollection<Gasto> ListaGastos { get; set; } 

    public class Gasto
    {
        public int id {get;set;}
        public decimal monto {get;set;}
        public string descripcion {get;set;} = string.Empty;
        public DateTime fecha {get;set;}
        public int semana {get;set;}
        public int año {get;set;}
    }

    private async void CargarGastos()
    {
        using (var client = new HttpClient())
        {
            // Cambia esta línea — agrega /{_semana}/{_año}
            string url = Conexion.BaseUrl + $"Gastos/gastos/{ID}/{_semana}/{_año}";

            try
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var listaApi = JsonSerializer.Deserialize<List<Gasto>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaGastos.Clear();
                        foreach (var lisCarg in listaApi) ListaGastos.Add(lisCarg);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("¡Excepción Capturada!", "Error interno: " + ex.Message, "OK");
            }
        }

        ActualizarHeader(); 
    }

    private int _semana = ISOWeek.GetWeekOfYear(DateTime.Today);
    private int _año = DateTime.Now.Year;

    private void ActualizarHeader()
    {
        var lunes = ISOWeek.ToDateTime(_año, _semana, DayOfWeek.Monday);
        var domingo = lunes.AddDays(6);
        LblSemana.Text = $"{lunes:dd MMM} - {domingo:dd MMM}";

        var total = ListaGastos.Sum(g => g.monto);
        LblTotal.Text = $"Total: ${total:N2}";
    }

    private async void SemanaAnterior_Clicked(object? sender, EventArgs e)
    {
        if (--_semana < 1) { _semana = 52; _año--; }
        CargarGastos();
    }

    private async void SemanaSiguiente_Clicked(object? sender, EventArgs e)
    {
        if (++_semana > 52) { _semana = 1; _año++; }
        CargarGastos();
    }

    private void ListaGastosCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    private async void BtnEliminarGastos_Clicked(object? sender, EventArgs e)
    {
        var buton = sender as Button;
        var Seleccionado = buton?.CommandParameter as Gasto;

        if(Seleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", "¿Estás seguro de eliminar este Gastos?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"Gastos/eliminarGastos/{Seleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarGastos();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }
    }

    private async void BtnAgregarGastos_Clicked(object? sender, EventArgs e)
    {
        Popup.IsVisible = true;
        Popup.Opacity = 0;
        await Popup.FadeToAsync(1, 250);
    }

    private async void BtnCerrarPopup_Clicked(object? sender, EventArgs e)
    {
        await Popup.FadeToAsync(0, 200); 
        Popup.IsVisible = false;
    }

    private async void BtnGuardarPopup_Clicked(object? sender, EventArgs e)
    {
        var Gastos = new Gasto
        {
            id = ID,
            monto = Convert.ToDecimal(txtMonto.Text),
            descripcion = txtDescripcion.Text,
            fecha = DateTime.Now,
            semana = ISOWeek.GetWeekOfYear(DateTime.Now),
            año = DateTime.Now.Year
        };  

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "Gastos/guardarGastos";

            try
            {
                var json = JsonSerializer.Serialize(Gastos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    txtMonto.Text = string.Empty;
                    txtDescripcion.Text = string.Empty;

                    await Popup.FadeToAsync(0, 200);
                    Popup.IsVisible = false;

                    CargarGastos();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de conexion", ex.Message, "OK");                
            }
        }
    }
}