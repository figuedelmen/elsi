using System.Collections.ObjectModel; // Permite usar colecciones dinámicas que actualizan la UI automáticamente
using System.Text.Json; // Permite trabajar con JSON (serializar y deserializar)
using System.Threading.Tasks; // Permite usar programación asíncrona (async/await)

namespace Proyecto1_NL15_47 // Espacio de nombres del proyecto
{
    public partial class segundaVentana : ContentPage // Clase que representa la página y hereda de ContentPage
    {
        public segundaVentana(int id) // Constructor que recibe el id del usuario
        {
            InitializeComponent(); // Inicializa los componentes definidos en el XAML

            idUsuario = id; // Guarda el id del usuario recibido

            ListaAhorro = new ObservableCollection<AhorroModel>(); // Inicializa la lista de ahorros

            BindingContext = this; // Establece el contexto de datos para enlazar con el XAML

            CargarDatos(); // Llama al método para cargar datos desde la API
        }

        protected override void OnAppearing() // Método que se ejecuta cuando la página aparece
        {
            base.OnAppearing(); // Llama al método base

            CargarDatos(); // Recarga los datos cada vez que la pantalla se muestra
        }

        public int idUsuario; // Variable pública que almacena el id del usuario

        public async void CargarDatos() // Método asíncrono para obtener datos de la API
        {
            using (var client = new HttpClient()) // Crea un cliente HTTP para hacer peticiones
            {
                var url = Conexion.BaseUrl + $"Ahorro/cargar/{idUsuario}"; // Construye la URL con el id

                try // Manejo de errores
                {
                    var response = await client.GetAsync(url); // Realiza la petición GET

                    if (response.IsSuccessStatusCode) // Verifica si la respuesta fue exitosa
                    {
                        var json = await response.Content.ReadAsStringAsync(); // Obtiene el contenido en formato texto

                        var opciones = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true }; 
                        // Permite ignorar mayúsculas/minúsculas al convertir JSON

                        var listaApi = JsonSerializer.Deserialize<List<AhorroModel>>(json, opciones);
                        // Convierte el JSON a una lista de objetos AhorroModel

                        if (listaApi != null) // Verifica que no sea null
                        {
                            ListaAhorro.Clear(); // Limpia la lista actual

                            foreach (var item in listaApi) // Recorre cada elemento recibido
                            {
                                ListaAhorro.Add(item); // Lo agrega a la lista observable
                            }
                        }
                    }
                    else // Si la respuesta no fue exitosa
                    {
                        await DisplayAlertAsync("API Error", $"Código: {response.StatusCode}", "OK");
                        // Muestra mensaje con el código de error
                    }
                }
                catch (Exception ex) // Captura errores de conexión
                {
                    await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");
                    // Muestra el mensaje del error
                }
            }
        }

        public ObservableCollection<AhorroModel> ListaAhorro { get; set; }
        // Lista enlazada al CollectionView en XAML

        public class AhorroModel // Clase que representa un ahorro
        {
            public int id { get; set; } // ID del ahorro
            public int id_usuario { get; set; } // ID del usuario
            public string nombre { get; set; } = string.Empty; // Nombre del ahorro
            public string ahorrado { get; set; } = string.Empty; // Cantidad ahorrada
            public string logo { get; set; } = string.Empty; // Nombre del archivo del logo
        }

        private async void ListAhorro_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        // Evento que se ejecuta al seleccionar un elemento de la lista
        {
            var AhorroSeleccionado = e.CurrentSelection.FirstOrDefault() as AhorroModel;
            // Obtiene el primer elemento seleccionado

            if (AhorroSeleccionado != null) // Verifica que exista selección
            { 
                var mostrarAhorro = new MostrarAhorro( // Crea nueva pantalla
                    AhorroSeleccionado.id_usuario,
                    AhorroSeleccionado.id,
                    AhorroSeleccionado.logo,
                    AhorroSeleccionado.nombre,
                    AhorroSeleccionado.ahorrado
                );

                await Navigation.PushAsync(mostrarAhorro); // Navega a la nueva pantalla
                
                var collectionView = sender as CollectionView; // Convierte el sender a CollectionView
                if (collectionView != null)
                {
                    collectionView.SelectedItem = null; // Quita la selección visual
                }
            }
        }

        private async void BtnAgregar_Clicked(object? sender, EventArgs e)
        // Evento al presionar el botón agregar
        {
            Poput.IsVisible = true; // Muestra el popup
        }

        public class AhorroAgregar // Clase para enviar datos al servidor
        {
            public int id_usuario { get; set; } // ID del usuario
            public string nombre { get; set; } = string.Empty; // Nombre
            public string logo { get; set; } = string.Empty; // Logo
        }

        private async void BtnGuardar_Clicked(object? sender, EventArgs e)
        // Evento al presionar guardar
        {
            if (pickerLogos.SelectedItem == null) // Verifica que se haya seleccionado un logo
            {
                await DisplayAlertAsync("Error", "Selecciona un logo", "OK");
                return; // Sale del método
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text)) // Verifica que haya nombre
            {
                await DisplayAlertAsync("Error", "Escribe un Nombre", "OK");
                return; 
            }

            string seleccion = pickerLogos.SelectedItem?.ToString() ?? "General";
            // Obtiene el valor seleccionado o usa "General" por defecto

            string nombreArchivoLogo = $"{seleccion.ToLower()}.png";
            // Convierte a minúsculas y agrega extensión .png

            var datos = new AhorroAgregar // Crea objeto con datos
            {
                id_usuario = idUsuario,
                nombre = txtNombre.Text,
                logo = nombreArchivoLogo
            };

            using (var client = new HttpClient()) // Cliente HTTP
            {
                var url = Conexion.BaseUrl + "Ahorro/agregar"; // URL API

                var json = JsonSerializer.Serialize(datos); // Convierte objeto a JSON

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                // Prepara contenido para enviar

                try
                {
                    var response = await client.PostAsync(url, content); // Envía POST

                    if (response.IsSuccessStatusCode) // Si fue exitoso
                    {
                        txtNombre.Text = string.Empty; // Limpia campo
                        pickerLogos.SelectedIndex = -1; // Limpia selección
                        CargarDatos(); // Recarga lista
                        Poput.IsVisible = false; // Oculta popup
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
        // Evento del botón cerrar
        {
            Poput.IsVisible = false; // Oculta popup
        }

        private async void BtnEliminar_Clicked(object? sender, EventArgs e)
        // Evento para eliminar un ahorro
        {
            var boton = sender as Button; // Convierte el sender a botón
            if (boton == null) return; // Si falla, sale

            var ahorro = boton.CommandParameter as AhorroModel; // Obtiene el objeto asociado
            if (ahorro == null) return; // Si no existe, sale

            bool confirmar = await DisplayAlertAsync("Eliminar", $"¿Seguro que quieres borrar '{ahorro.nombre}'?", "Sí, borrar", "Cancelar");
            // Muestra confirmación

            if (confirmar) // Si el usuario acepta
            {
                using (var client = new HttpClient())
                {
                    var url = Conexion.BaseUrl + $"Ahorro/eliminar/{ahorro.id}";
                    // URL para eliminar

                    try
                    {
                        var response = await client.DeleteAsync(url); // Envía DELETE

                        if (response.IsSuccessStatusCode) // Si fue exitoso
                        {
                            CargarDatos(); // Recarga lista
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