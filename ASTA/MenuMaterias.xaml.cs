using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Controls;

namespace ASTA;

public partial class MenuMaterias : ContentPage
{
    public MenuMaterias(int idUsuario, int idMateria, string nombreMateria, string nombreProfesor)
    {
        InitializeComponent();  

        this.ID = idUsuario;
        this.idMateria = idMateria;
        this.nombreMateria = nombreMateria;
        this.nombreProfesor = nombreProfesor;

        ListaHorarios = new ObservableCollection<Horarios>();
        ListaTareas = new ObservableCollection<Tareas>();

        LaNombre.Text = nombreMateria;
        LaProfesor.Text = nombreProfesor;

        CargarHorarios();
        CargarTareas();

        BindingContext = this;
    }

    public int ID;
    public int idMateria;
    public string nombreMateria;
    public string nombreProfesor;
    public ObservableCollection<Horarios> ListaHorarios {get; set;}
    public ObservableCollection<Tareas> ListaTareas {get; set;}

    public class Horarios
    {
        public int id {get;set;}
        public string dia_semana { get; set; } = string.Empty;
        public TimeSpan hora_inicio { get; set; }
        public TimeSpan hora_fin { get; set; }
        public string aula { get; set; } = string.Empty;     

        public string HoraInicioFormateada => DateTime.Today.Add(hora_inicio).ToString("hh:mm tt");
        public string HoraFinFormateada => DateTime.Today.Add(hora_fin).ToString("hh:mm tt"); 
    }

    public async void CargarHorarios()
    {
        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"ASTA/horarios/{idMateria}";

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

                    var listaApi = JsonSerializer.Deserialize<List<Horarios>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaHorarios.Clear();
                        foreach (var horarios in listaApi)
                        {
                            ListaHorarios.Add(horarios);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("¡Excepción Capturada!", "Error interno: " + ex.Message, "OK");                
            }
        }
    }

    public class Tareas
    {
        public int id {get;set;}
        public string titulo {get;set;} = string.Empty;
        public string descripcion {get;set;} = string.Empty;
        public DateTime fecha_entrega {get; set;}
        public bool completada {get;set;}
        
    }

    public async void CargarTareas()
    {
        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"ASTA/tareas/{idMateria}";

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

                    var listaApi = JsonSerializer.Deserialize<List<Tareas>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaTareas.Clear();
                        foreach (var tareas in listaApi)
                        {
                            ListaTareas.Add(tareas);
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

    private void ListaHorariosCollection_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
    {
    }

    private async void BtnEliminarHorarios_Clicked(object? sender, EventArgs e)
    {
        var buton = sender as Button;
        var HorarioSeleccionado = buton?.CommandParameter as Horarios;

        if(HorarioSeleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", "¿Estás seguro de eliminar este horario?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"ASTA/eliminarHorario/{HorarioSeleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarHorarios();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }
    }

    private async void BtnAgregarHorarios_Clicked(object? sender, EventArgs? e)
    {
        PoputHorarios.IsVisible = true;
        PoputHorarios.Opacity = 0;
        await PoputHorarios.FadeToAsync(1, 250);   
    }

    private async void BtnCerrarPoputHorario_Clicked(object? sender, EventArgs e)
    {
        await PoputHorarios.FadeToAsync(0, 200); 
        PoputHorarios.IsVisible = false;
    }

    private async void BtnGuardarPoputHorario_Clicked(object? sender, EventArgs? e)
    {
        if (pckDiaSemanaHorario.SelectedIndex == -1 || string.IsNullOrWhiteSpace(txtAulaHorario.Text))
        {
            await DisplayAlertAsync("Error", "Por favor, seleccione un día y escriba el aula.", "OK");
            return;
        }

        if (tmpHoraFinHorario.Time <= tmpHoraInicioHorario.Time)
        {
            await DisplayAlertAsync("Error", "La hora de fin debe ser después de la hora de inicio.", "OK");
            return;
        }

        var nuevoHorario = new Horarios
        {
            id = idMateria, 
            dia_semana = pckDiaSemanaHorario.SelectedItem?.ToString() ?? string.Empty,
            hora_inicio = tmpHoraInicioHorario.Time ?? TimeSpan.Zero,
            hora_fin = tmpHoraFinHorario.Time ?? TimeSpan.Zero,
            aula = txtAulaHorario.Text ?? string.Empty
        };

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "ASTA/guardarHorario";

            try
            {
                var json = JsonSerializer.Serialize(nuevoHorario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    pckDiaSemanaHorario.SelectedIndex = -1;
                    txtAulaHorario.Text = string.Empty;
                    tmpHoraInicioHorario.Time = new TimeSpan(7, 0, 0);
                    tmpHoraFinHorario.Time = new TimeSpan(8, 0, 0);

                    await PoputHorarios.FadeToAsync(0, 200);
                    PoputHorarios.IsVisible = false;

                    CargarHorarios();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", $"Detalle: {ex.Message}", "OK");
            }
        }
    }

    private async void ListaTareasCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var tareaSeleccionada = e.CurrentSelection.FirstOrDefault() as Tareas;

        if (tareaSeleccionada == null) return;

        lblVerTituloTarea.Text = tareaSeleccionada.titulo;
        lblVerDescripcionTarea.Text = tareaSeleccionada.descripcion;
        lblVerFechaEntrega.Text = Convert.ToString(tareaSeleccionada.fecha_entrega.Date);

        PoputVerTarea.IsVisible = true;
        PoputVerTarea.Opacity = 0;
        await PoputVerTarea.FadeToAsync(1, 250);  
    }

    private async void BtnEliminarTareas_Clicked(object? sender, EventArgs e)
    {
        var buton = sender as Button;
        var TareaSeleccionado = buton?.CommandParameter as Tareas;

        if(TareaSeleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", "¿Estás seguro de eliminar esta Tarea?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"ASTA/eliminarTareas/{TareaSeleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    CargarTareas();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }
    }

    private async void BtnAgregarTareas_Clicked(object? sender, EventArgs? e)
    {
        PoputTareas.IsVisible = true;
        PoputTareas.Opacity = 0;
        await PoputTareas.FadeToAsync(1, 250); 
    }

    private async void BtnCerrarPoputTareas_Clicked(object? sender, EventArgs e)
    {
        await PoputTareas.FadeToAsync(0, 200); 
        PoputTareas.IsVisible = false;
    }

    private async void BtnGuardarPoputTareas_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTituloTarea.Text) || string.IsNullOrWhiteSpace(txtDescripcionTarea.Text))
        {
            await DisplayAlertAsync("Error", "Por favor, complete todos los campos.", "OK");
            return;
        }

        var nuevaTarea = new Tareas
        {
            id = idMateria,
            titulo = txtTituloTarea.Text,
            descripcion = txtDescripcionTarea.Text,
            fecha_entrega = dtpFechaEntregaTarea.Date ?? DateTime.Now,
            completada = false
        };

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "ASTA/guardarTareas";

            try
            {
                var json = JsonSerializer.Serialize(nuevaTarea);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    txtTituloTarea.Text = string.Empty;
                    txtDescripcionTarea.Text = string.Empty;
                    dtpFechaEntregaTarea.Date = DateTime.Now;

                    await PoputTareas.FadeToAsync(0, 200);
                    PoputTareas.IsVisible = false;

                    CargarTareas();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de conexion", ex.Message, "OK");                
            }
        }
    }

    private async void BtnCerrarVerTarea_Clicked(object? sender, EventArgs e)
    {
        await PoputVerTarea.FadeToAsync(0, 200); 
        PoputVerTarea.IsVisible = false;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid contenedor && contenedor.Children.FirstOrDefault() is CheckBox checkBox && checkBox.BindingContext is Tareas tareaModificada)
        {
            if (tareaModificada.completada)
            {
                await DisplayAlertAsync("Tarea Terminada", "¡Esta tarea ya fue finalizada y no se puede modificar!", "Entendido");
                return;
            }

            bool confirmar = await DisplayAlertAsync(
                "Confirmar", 
                $"¿Seguro que ya acabaste la tarea \"{tareaModificada.titulo}\"? Una vez completada no podrás modificarla.", 
                "Sí, terminar", 
                "No, todavía no"
            );

            if (confirmar)
            {
                tareaModificada.completada = true;
                checkBox.IsChecked = true;
                checkBox.IsEnabled = false; 

                try
                {
                    using (var client = new HttpClient())
                    {
                        var url = Conexion.BaseUrl + $"ASTA/completadoTareas/{tareaModificada.id}";
                        var response = await client.PostAsync(url, null);

                        if (response.IsSuccessStatusCode)
                        {
                            CargarTareas();
                        }
                        else
                        {
                            tareaModificada.completada = false;
                            checkBox.IsChecked = false;
                            checkBox.IsEnabled = true;
                            await DisplayAlertAsync("Error", "No se pudo guardar en el servidor.", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    tareaModificada.completada = false;
                    checkBox.IsChecked = false;
                    checkBox.IsEnabled = true;
                    await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                
                }
            }
        }
    }

}