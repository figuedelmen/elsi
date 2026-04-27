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

        // Estructura para definir qué datos de usuario enviaremos (nombre y clave)
        public class loginUser
        {
            public string nombre { get; set; } = string.Empty;
            public string contrasena { get; set; } = string.Empty;
        }

        // Estructura para recibir la respuesta del servidor (un mensaje y el ID del usuario)
        public class loginResponse
        {
            public string message {get;set;} = string.Empty;
            public int id {get;set;}
        }

        // Método que se ejecuta al hacer clic en el botón de iniciar sesión
        private async void BtnIniciarSesion_Clicked(object? sender, EventArgs e)
        {
            // Creamos un objeto con los datos capturados de los campos de texto de la pantalla
            var datoslogin = new loginUser
            {
                nombre = TexNombre.Text,
                contrasena = TexContraseña.Text
            };

            // Iniciamos un cliente HTTP para realizar la comunicación con la red
            using (var client = new HttpClient())
            {
                // Construimos la dirección completa: IP Global + ruta de la API
                var url = Conexion.BaseUrl + "Ahorro/Info";

                // Convertimos el objeto de C# a un formato de texto JSON
                var json = JsonSerializer.Serialize(datoslogin);

                // Preparamos el contenido para el envío, indicando que es tipo JSON y usa UTF8
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    // Enviamos los datos mediante un método POST y esperamos la respuesta del servidor
                    var response = await client.PostAsync(url,content);

                    // Si el servidor responde que todo está bien (Código 200)
                    if (response.IsSuccessStatusCode)
                    {
                        // Leemos el contenido de la respuesta como una cadena de texto
                        var respuesta = await response.Content.ReadAsStringAsync();

                        // Convertimos ese texto JSON de vuelta a un objeto de C# (loginResponse)
                        var datos = JsonSerializer.Deserialize<loginResponse>(respuesta);

                        // Obtenemos el ID del usuario o 0 si no existe
                        int idUsuario = datos?.id ?? 0;
                        
                        // Preparamos la navegación a la segunda ventana enviando el ID obtenido
                        var segundapantalla = new segundaVentana(idUsuario);

                        // Cambiamos de pantalla en la aplicación
                        await Navigation.PushAsync(segundapantalla);
                    }
                    else
                    {
                        // Si el servidor rechaza los datos, mostramos una alerta de error
                        await DisplayAlertAsync("Error", "Datos Incorrectos", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Si ocurre un error de conexión (ej: no hay internet), mostramos el mensaje de error
                    await DisplayAlertAsync("Error", ex.Message, "OK");                
                }
            }
        }
    }
}