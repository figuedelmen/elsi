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

        public int idUsuario;

        public async void CargarDatos()
        {
            using (var client = new HttpClient())
            {
                var url = $"http://192.168.101.120:5256/Ahorro/cargar/{idUsuario}"; 

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
                var mostrarAhorro = new MostrarAhorro();

                mostrarAhorro.Id_usuario_c = AhorroSeleccionado.id_usuario;

                mostrarAhorro.Id_c = AhorroSeleccionado.id;

                mostrarAhorro.Logo_c = AhorroSeleccionado.logo;

                mostrarAhorro.Nombre_c = AhorroSeleccionado.nombre;

                mostrarAhorro.Ahorrado_c = AhorroSeleccionado.ahorrado;

                await Navigation.PushAsync(mostrarAhorro);
            }
        }

        private async void BtnAgregar_Clicked(object? sender, EventArgs e)
        {
            Poput.IsVisible = true;
        }

        private void BtnGuardar_Clicked(object? sender, EventArgs e)
        {
            
        }

        private void BtnCerrar_Clicked(object sender, EventArgs e)
        {
            Poput.IsVisible = false;
        }
    }
}
 