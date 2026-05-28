using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;

namespace BorrowBack;

public partial class MenuDatos : ContentPage
{
    public MenuDatos(int idUsuario)
    {
        InitializeComponent();

        this.ID = idUsuario;

        ListaPrestamo = new ObservableCollection<Prestamos>();

        BindingContext = this;
 
        CargarDatos();
    }

    public ObservableCollection<Prestamos> ListaPrestamo {get;set;}

    public int ID;

    public async void CargarDatos()
    {
        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"BorrowBack/datos/{ID}";

            try
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var opciones = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true };

                    var listaApi = JsonSerializer.Deserialize<List<Prestamos>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaPrestamo.Clear();
                        foreach (var item in listaApi)
                        {
                            ListaPrestamo.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                
            }
        }
    }

    public class Prestamos
    {
        public int id {get; set;}
        public string persona {get;set;} = string.Empty;
        public string objeto {get;set;} = string.Empty;
        public string url {get;set;} = string.Empty;
        public DateTime entrega {get;set;}
        public DateTime devolucion {get;set;}
        public string estado {get;set;} = string.Empty;
    }

    private async void ListaPrestamos_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var prestamoSeleccionado = e.CurrentSelection.FirstOrDefault() as Prestamos;

        if (prestamoSeleccionado == null) return;

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        if (prestamoSeleccionado.estado == "Devuelto")
        {
            await DisplayAlertAsync("Información", "Este objeto ya fue devuelto y está en tu posesión.", "OK");
            return;
        }

        bool confirmarDevolucion = await DisplayAlertAsync(
            "Confirmar Devolución", 
            $"¿Te acaban de entregar el '{prestamoSeleccionado.objeto}' que le prestaste a {prestamoSeleccionado.persona}?", 
            "Sí, ya lo tengo", 
            "No, todavía no"
        );

        if (!confirmarDevolucion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"BorrowBack/devolver/{prestamoSeleccionado.id}";

            try
            {
                var response = await client.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    CargarDatos();
                }
                else
                {
                    string codigoError = ((int)response.StatusCode).ToString();
                    string cuerpoError = await response.Content.ReadAsStringAsync();
                    
                    await DisplayAlertAsync($"Error {codigoError}", $"Detalle del servidor: {cuerpoError}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                
            }
        }
    }

    private async void BtnEliminar_Clicked(object? sender, EventArgs e)
    {
        var boton = sender as Button;
        var PrestamosSeleccionado = boton?.CommandParameter as Prestamos;

        if (PrestamosSeleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", $"¿Estás seguro de eliminar el préstamo de '{PrestamosSeleccionado.objeto}'?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"BorrowBack/eliminar/{PrestamosSeleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarDatos();
                }
                else
                {
                    string codigoError = ((int)response.StatusCode).ToString();
                    string cuerpoError = await response.Content.ReadAsStringAsync();
                    
                    await DisplayAlertAsync($"Error {codigoError}", $"Detalle del servidor: {cuerpoError}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                
            }
        }
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

    private async void BtnGuardarPoput_Clicked(object? sender, EventArgs? e)
    {
        if (string.IsNullOrWhiteSpace(txtNombre_Persona.Text) || string.IsNullOrWhiteSpace(txtNombre_Objeto.Text))
        {
            await DisplayAlertAsync("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        var nuevoPrestamo = new Prestamos
        {
            id = this.ID,
            persona = txtNombre_Persona.Text,
            objeto = txtNombre_Objeto.Text,
            url = RutaImagen,
            entrega = DateTime.Now,
            devolucion = DateDevolucion.Date ?? DateTime.Today,
            estado = "Pendiente"
        };

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + "BorrowBack/guardar";

            try
            {
                var json = JsonSerializer.Serialize(nuevoPrestamo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
                        {
                            await LocalNotificationCenter.Current.RequestNotificationPermission();
                        }

                        var notification = new NotificationRequest
                        {
                            NotificationId = new Random().Next(1000, 999999),
                            Title = "⏰ ¡Recordatorio de BorrowBack!",
                            Description = $"Debes recuperar tu '{nuevoPrestamo.objeto}' prestado a {nuevoPrestamo.persona}.",
                            BadgeNumber = 1,
                            Schedule = new NotificationRequestSchedule
                            {
                                NotifyTime = nuevoPrestamo.devolucion.Date.AddHours(9) 
                            },                           
                            Android = new AndroidOptions
                            {
                                ChannelId = "borrowback_alerts" // Vincula directamente con el canal de alta prioridad de MauiProgram
                            }
                        };

                        await LocalNotificationCenter.Current.Show(notification);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync("Error Plugin", ex.Message, "OK");
                    }


                    txtNombre_Persona.Text = string.Empty;
                    txtNombre_Objeto.Text = string.Empty;
                    RutaImagen = string.Empty;
                    imgSelected.Source = "camera_placeholder.png";

                    await Poput.FadeToAsync(0, 200);
                    Poput.IsVisible = false;

                    CargarDatos();
                }
                else
                {
                    string codigoError = ((int)response.StatusCode).ToString();
                    string cuerpoError = await response.Content.ReadAsStringAsync();
                    
                    await DisplayAlertAsync($"Error {codigoError}", $"Detalle del servidor: {cuerpoError}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", $"Detalle: {ex.Message}", "OK");
            }
        }     
    }

    private string RutaImagen = string.Empty;

    private async void BtnAgregarImagen_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var foto = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Selecciona una imagen"
            });

            if (foto != null && foto.Count > 0)
            {
                var archivoIndivual = foto[0]; 
                
                RutaImagen = archivoIndivual.FullPath;
                imgSelected.Source = ImageSource.FromFile(RutaImagen);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");       
        }
    }
}