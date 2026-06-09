using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Controls;

namespace ASTA;

public partial class MenuDatos : ContentPage
{
    public MenuDatos(int idUsuario)
    {
        InitializeComponent();  

        this.ID = idUsuario;

        ListaMaterias = new ObservableCollection<Materias>();

        BindingContext = this;

        CargarMaterias();
    }

    public int ID { get; set; }

    public ObservableCollection<Materias> ListaMaterias { get; set; }

    public class Materias
    {
        public int id {get;set;}
        public string nombre {get;set;} = string.Empty;
        public string profesor {get;set;} = string.Empty;
    }

    public async void CargarMaterias()
    {
        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"ASTA/materias/{ID}";

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

                    var listaApi = JsonSerializer.Deserialize<List<Materias>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaMaterias.Clear();
                        foreach (var materia in listaApi)
                        {
                            ListaMaterias.Add(materia);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", "No se pudieron cargar las materias: " + ex.Message, "OK");                
            }
        }
    }

    private async void BtnEliminar_Clicked(object? sender, EventArgs e)
    {
        var boton = sender as Button;
        var MateriaSeleccionado = boton?.CommandParameter as Materias;

        if (MateriaSeleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", $"¿Estás seguro de eliminar la materia de '{MateriaSeleccionado.nombre}'?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"ASTA/eliminarMateria/{MateriaSeleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarMaterias();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }

    }

    private async void ListaMateriasCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var materiaSeleccionada = e.CurrentSelection.FirstOrDefault() as Materias;

        if (materiaSeleccionada == null)
           return;

        var menuMaterias = new MenuMaterias(ID, materiaSeleccionada. id, materiaSeleccionada.nombre, materiaSeleccionada.profesor);

        await Navigation.PushAsync(menuMaterias);

    }

    private async void BtnAgregar_Clicked(object? sender, EventArgs e)
    {
        Poput.IsVisible = true;
        Poput.Opacity = 0;
        await Poput.FadeToAsync(1, 250);   
    }

    private async void BtnCerrarPoput_Clicked(object? sender, EventArgs e)
    {
        await Poput.FadeToAsync(0, 200); 
        Poput.IsVisible = false;
    }

    private async void BtnGuardarPoput_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNombreMateria.Text) || string.IsNullOrWhiteSpace(txtNombreProfesor.Text))
        {
            await DisplayAlertAsync("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        var nuevoMateria = new Materias
        {
            id = ID,
            nombre = txtNombreMateria.Text,
            profesor = txtNombreProfesor.Text
        };

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "ASTA/guardar";

            try
            {
                var json = JsonSerializer.Serialize(nuevoMateria);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    txtNombreMateria.Text = string.Empty;
                    txtNombreProfesor.Text = string.Empty;

                    await Poput.FadeToAsync(0, 200);
                    Poput.IsVisible = false;

                    CargarMaterias();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", $"Detalle: {ex.Message}", "OK");
            }
        }
    }
}