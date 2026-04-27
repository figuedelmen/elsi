using System.Collections.ObjectModel; // Permite usar listas dinámicas que notifican cambios a la interfaz
using System.Text.Json; // Permite convertir objetos a JSON y viceversa

namespace Proyecto1_NL15_47 // Espacio de nombres del proyecto
{
    public partial class MostrarAhorro : ContentPage // Clase que representa la pantalla de detalle de ahorro
    {
        public MostrarAhorro(int idUsuario, int idAhorro, string logo, string nombre, string ahorrado)
        // Constructor que recibe datos desde la pantalla anterior
        {
            InitializeComponent(); // Inicializa los componentes definidos en el XAML

            Id_usuario = idUsuario; // Guarda el id del usuario
            IdAhorro = idAhorro; // Guarda el id del ahorro seleccionado
            Logo = logo; // Guarda el logo
            Nombre = nombre; // Guarda el nombre del ahorro
            Ahorrado = ahorrado; // Guarda el monto actual ahorrado

            listaDepositos = new ObservableCollection<ListaDepositos>(); 
            // Inicializa la lista de depósitos

            this.BindingContext = this; 
            // Establece el contexto de datos para enlazar con el XAML

            CargarDatos(); 
            // Llama al método para cargar los depósitos desde la API
        }

        public async void CargarDatos() // Método para obtener los depósitos del servidor
        {
            using (var client = new HttpClient()) // Cliente HTTP para hacer peticiones
            {
                var url = Conexion.BaseUrl + $"Ahorro/deposito/{IdAhorro}";
                // Construye la URL con el id del ahorro

                try // Manejo de errores
                {
                    var response = await client.GetAsync(url); 
                    // Hace petición GET

                    if (response.IsSuccessStatusCode) 
                    // Verifica si la respuesta fue exitosa
                    {
                        var json = await response.Content.ReadAsStringAsync(); 
                        // Obtiene el JSON como texto

                        var opciones = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true };
                        // Permite ignorar mayúsculas/minúsculas

                        var listaApi = JsonSerializer.Deserialize<List<ListaDepositos>>(json, opciones);
                        // Convierte JSON a lista de objetos ListaDepositos

                        if (listaApi != null) 
                        // Verifica que no sea null
                        {
                            listaDepositos.Clear(); 
                            // Limpia la lista actual

                            foreach (var item in listaApi) 
                            // Recorre cada elemento recibido
                            {
                                listaDepositos.Add(item); 
                                // Lo agrega a la colección observable
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlertAsync("API Error", $"Código: {response.StatusCode}", "OK");
                        // Muestra error si la API falla
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");
                    // Muestra error si hay problema de red
                }
            }
        }

        public int Id_usuario { get; set; } // Propiedad para almacenar el id del usuario
        public int IdAhorro { get; set; } // Propiedad para almacenar el id del ahorro
        public string Logo { get; set; } // Propiedad del logo
        public string Nombre { get; set; } // Propiedad del nombre del ahorro
        public string Ahorrado { get; set; } // Propiedad del monto ahorrado


        public ObservableCollection<ListaDepositos> listaDepositos { get; set; }
        // Lista que se muestra en la interfaz (CollectionView)

        public class ListaDepositos // Clase que representa cada depósito
        {
            public decimal deposito{get; set;} // Monto del depósito
            public DateTime fecha_deposito{get; set;} // Fecha en que se hizo el depósito
        };

        private void BtnDeposito_Clicked(object? sender, EventArgs e)
        // Evento al presionar el botón de hacer depósito
        {
            Poput.IsVisible = true; // Muestra el popup
        }

        public class NuevoDeposito // Clase para enviar datos al servidor
        {
            public int id_ahorro { get; set; } // ID del ahorro
            public decimal monto { get; set; } // Cantidad a depositar
            public string ahorrado { get; set; } = string.Empty; // Total actualizado
        }


        private async void BtnGuardar_Clicked(object? sender, EventArgs e)
        // Evento al presionar guardar depósito
        {
            var envio = new NuevoDeposito
            {
                id_ahorro = IdAhorro, // Usa el id actual del ahorro
                monto =  Convert.ToDecimal(txtDeposito.Text), 
                // Convierte el texto ingresado a número decimal

                ahorrado = Ahorrado // Envía el total actual
            };

            decimal nuevoTotal = Convert.ToDecimal(Ahorrado) + envio.monto;
            // Calcula el nuevo total sumando el depósito

            Ahorrado = nuevoTotal.ToString();
            // Actualiza la propiedad con el nuevo valor

            OnPropertyChanged(nameof(Ahorrado));
            // Notifica a la interfaz que el valor cambió

            using (var client = new HttpClient())
            {
                var url = Conexion.BaseUrl + "Ahorro/depositar";
                // URL para enviar el depósito

                var json = JsonSerializer.Serialize(envio);
                // Convierte el objeto a JSON

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                // Prepara el contenido HTTP

                var response = await client.PostAsync(url, content);
                // Envía la petición POST

                if (response.IsSuccessStatusCode)
                {
                    Poput.IsVisible = false; 
                    // Oculta el popup

                    CargarDatos(); 
                    // Recarga la lista de depósitos
                }
            }
        }

        private void BtnCerrar_Clicked(object? sender, EventArgs e)
        // Evento del botón cerrar
        {
            Poput.IsVisible = false; // Oculta el popup
        }
    }   
}