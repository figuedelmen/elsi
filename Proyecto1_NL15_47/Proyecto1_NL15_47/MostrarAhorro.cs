using System.Collections.ObjectModel;
using System.Text.Json;

namespace Proyecto1_NL15_47
{
    public partial class MostrarAhorro : ContentPage
    {
        public MostrarAhorro(int idUsuario, int idAhorro, string logo, string nombre, string ahorrado)
        {
            InitializeComponent();

            Id_usuario = idUsuario;
            IdAhorro = idAhorro;
            Logo = logo;
            Nombre = nombre;
            Ahorrado = ahorrado;

            listaDepositos = new ObservableCollection<ListaDepositos>();

            this.BindingContext = this;

            CargarDatos();
        }

        public async void CargarDatos()
        {
            using (var client = new HttpClient())
            {
                var url = Conexion.BaseUrl + $"Ahorro/deposito/{IdAhorro}";

                try
                {
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var opciones = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true };
                        var listaApi = JsonSerializer.Deserialize<List<ListaDepositos>>(json, opciones);

                        if (listaApi != null)
                        {
                            listaDepositos.Clear();
                            foreach (var item in listaApi)
                            {
                                listaDepositos.Add(item);
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

        public int Id_usuario { get; set; }
        public int IdAhorro { get; set; }
        public string Logo { get; set; }
        public string Nombre { get; set; }
        public string Ahorrado { get; set; }


        public ObservableCollection<ListaDepositos> listaDepositos { get; set; }

        public class ListaDepositos
        {
            public decimal deposito{get; set;}
            public DateTime fecha_deposito{get; set;}
        };

        private void BtnDeposito_Clicked(object? sender, EventArgs e)
        {
            Poput.IsVisible = true;
        }

        public class NuevoDeposito
        {
            public int id_ahorro { get; set; }
            public decimal monto { get; set; }
            public string ahorrado { get; set; } = string.Empty;
        }


        private async void BtnGuardar_Clicked(object? sender, EventArgs e)
        {
            var envio = new NuevoDeposito
            {
                id_ahorro = IdAhorro,
                monto =  Convert.ToDecimal(txtDeposito.Text),
                ahorrado = Ahorrado
            };

            decimal nuevoTotal = Convert.ToDecimal(Ahorrado) + envio.monto;
            Ahorrado = nuevoTotal.ToString();
            OnPropertyChanged(nameof(Ahorrado));

            using (var client = new HttpClient())
            {
                var url = Conexion.BaseUrl + "Ahorro/depositar";
                var json = JsonSerializer.Serialize(envio);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Poput.IsVisible = false;
                    CargarDatos();
                }
            }
        }

        private void BtnCerrar_Clicked(object? sender, EventArgs e)
        {
            Poput.IsVisible = false;
        }
    }   
}