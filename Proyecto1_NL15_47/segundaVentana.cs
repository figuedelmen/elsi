using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace Proyecto1_NL15_47
{
    public partial class segundaVentana : ContentPage
    {
        public segundaVentana(int id)
        {
            InitializeComponent();

            idUsuario = id; 

            ListaAhorro = new ObservableCollection<AhorroModel>();

            BindingContext = this;

            CargarDatos();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            CargarDatos();
        }

        public int idUsuario;

        public async void CargarDatos()
        {
            using (var client = new HttpClient())
            {
                var url = Conexion.BaseUrl + $"Ahorro/cargar/{idUsuario}";

                try
                {
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var opciones = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true };
                        var listaApi = JsonSerializer.Deserialize<List<AhorroModel>>(json, opciones);

                        if (listaApi != null)
                        {
                            ListaAhorro.Clear();
                            foreach (var item in listaApi)
                            {
                                ListaAhorro.Add(item);
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlertAsync("API Error", $"Código: {response.StatusCode}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");
                }
            }
        }

        public ObservableCollection<AhorroModel> ListaAhorro { get; set; }

        public class AhorroModel
        {
            public int id { get; set; }
            public int id_usuario { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string ahorrado { get; set; } = string.Empty;
            public string logo { get; set; } = string.Empty;
        }

        private async void ListAhorro_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var AhorroSeleccionado = e.CurrentSelection.FirstOrDefault() as AhorroModel;

            if (AhorroSeleccionado != null)
            { 
                var mostrarAhorro = new  MostrarAhorro(
                    AhorroSeleccionado.id_usuario,
                    AhorroSeleccionado.id,
                    AhorroSeleccionado.logo,
                    AhorroSeleccionado.nombre,
                    AhorroSeleccionado.ahorrado
                );

                await Navigation.PushAsync(mostrarAhorro);
                
                var collectionView = sender as CollectionView;
                if (collectionView != null)
                {
                    collectionView.SelectedItem = null;
                }
            }
        }

        private async void BtnAgregar_Clicked(object? sender, EventArgs e)
        {
            Poput.IsVisible = true;
        }

        public class AhorroAgregar
        {
            public int id_usuario { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string logo { get; set; } = string.Empty;
        }

        private async void BtnGuardar_Clicked(object? sender, EventArgs e)
        {
            if (pickerLogos.SelectedItem == null)
            {
                await DisplayAlertAsync("Error", "Selecciona un logo", "OK");
                return;
            }

             if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                await DisplayAlertAsync("Error", "Escribe un Nombre", "OK");
                return; 
            }

            string seleccion = pickerLogos.SelectedItem?.ToString() ?? "General";

            string nombreArchivoLogo = $"{seleccion.ToLower()}.png";

            var datos = new AhorroAgregar
            {
                id_usuario = idUsuario,
                nombre = txtNombre.Text,
                logo = nombreArchivoLogo
            };

            using (var client = new HttpClient())
            {
                var url = Conexion.BaseUrl + "Ahorro/agregar";

                var json = JsonSerializer.Serialize(datos);

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        txtNombre.Text = string.Empty;
                        pickerLogos.SelectedIndex = -1;
                        CargarDatos();
                        Poput.IsVisible = false;
                    }
                    else
                    {
                        await DisplayAlertAsync("Error", "Error al agregar ahorro", "OK");
                        Poput.IsVisible = false;
                    }
                }
                catch (System.Exception ex)
                {
                    await DisplayAlertAsync("Error", ex.Message, "OK");
                }
            }

        }

        private void BtnCerrar_Clicked(object? sender, EventArgs e)
        {
            Poput.IsVisible = false;
        }

        private async void BtnEliminar_Clicked(object? sender, EventArgs e)
        {
            var boton = sender as Button;
            if (boton == null) return;

            var ahorro = boton.CommandParameter as AhorroModel;
            if (ahorro == null) return;

            bool confirmar = await DisplayAlertAsync("Eliminar", $"¿Seguro que quieres borrar '{ahorro.nombre}'?", "Sí, borrar", "Cancelar");

            if (confirmar)
            {
                using (var client = new HttpClient())
                {
                    var url = Conexion.BaseUrl + $"Ahorro/eliminar/{ahorro.id}";

                    try
                    {
                        var response = await client.DeleteAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            CargarDatos();
                        }
                        else
                        {
                            await DisplayAlertAsync("Error", "No se pudo eliminar el ahorro", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync("Error de red", ex.Message, "OK");
                    }
                }
            }
        }
    }
}
 