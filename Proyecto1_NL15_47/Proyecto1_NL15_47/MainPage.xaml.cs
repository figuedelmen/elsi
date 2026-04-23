using System.Text; 
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Proyecto1_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public class loginUser
        {
            public string nombre { get; set; } = string.Empty;
            public string contrasena { get; set; } = string.Empty;
        }

        public class loginResponse
        {
            public string message {get;set;} = string.Empty;
            public int id {get;set;}
        }

        private async void BtnIniciarSesion_Clicked(object? sender, EventArgs e)
        {
            var datoslogin = new loginUser
            {
                nombre = TexNombre.Text,
                contrasena = TexContraseña.Text
            };

            using (var client = new HttpClient())
            {
                var url = "http://192.168.101.120:5256/Ahorro/Info";

                var json = JsonSerializer.Serialize(datoslogin);

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(url,content);
                    if (response.IsSuccessStatusCode)
                    {
                        var respuesta = await response.Content.ReadAsStringAsync();

                        var datos = JsonSerializer.Deserialize<loginResponse>(respuesta);

                        int idUsuario = datos.id;

                        var segundapantalla = new segundaVentana(idUsuario);

                        await Navigation.PushAsync(segundapantalla);
                    }
                    else
                    {
                        await DisplayAlertAsync("Error", "Datos Incorrectos", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Error", ex.Message, "OK");                
                }
            }
        }
    }
}
 