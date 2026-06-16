using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Java.Sql;

namespace Calendario;

public partial class MenuDatos : ContentPage
{
    public MenuDatos(int idUsuario)
    {
        InitializeComponent();  

        this.ID = idUsuario;

        ListaEventos = new ObservableCollection<Eventos>();

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarMes();
    }


    public int ID { get; set; }
    private int _anio = DateTime.Now.Year;
    private int _mes = DateTime.Now.Month;
    private List<Eventos> _eventosDelMes = new();

    public ObservableCollection<Eventos> ListaEventos{get;set;}

    public class Eventos
    {
        public int id {get;set;}
        public string titulo {get;set;} = string.Empty;
        public string descripcion {get;set;} = string.Empty;
        public DateTime fecha {get;set;}
        public TimeSpan? hora {get;set;}
        public string color {get;set;} = string.Empty;
    }

    private async Task CargarMes()
    {
        LblMes.Text = new DateTime(_anio, _mes, 1).ToString("MMMM yyyy").ToUpper();

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"Calendario/eventos/{ID}";

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

                    var listaApi = JsonSerializer.Deserialize<List<Eventos>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaEventos.Clear();
                        _eventosDelMes.Clear();
                        foreach (var lisCarg in listaApi)
                        {
                            ListaEventos.Add(lisCarg);
                            _eventosDelMes.Add(lisCarg);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("¡Excepción Capturada!", "Error interno: " + ex.Message, "OK");                
            }
        }

        GenerarCalendario();
    }

    public class DiaCalendario : INotifyPropertyChanged
    {
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public bool EsVisible { get; set; } = true;
        public List<Color> Puntos { get; set; } = new();
        public bool TienePuntos => Puntos.Any();

        private bool _seleccionado;
        public bool Seleccionado
        {
            get => _seleccionado;
            set { _seleccionado = value; OnPropertyChanged(nameof(Fondo)); }
        }

        public bool EsHoy => Fecha.Date == DateTime.Today;
        public Color Fondo =>
            EsHoy ? Color.FromArgb("#1A73E8") :
            Seleccionado ? Color.FromArgb("#E8F0FE") :
            Colors.Transparent;
        public Color TextoColor =>
            EsHoy ? Colors.White :
            Seleccionado ? Color.FromArgb("#1A73E8") :
            Color.FromArgb("#3C4043");

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new(n));
    }

    public ObservableCollection<DiaCalendario> DiasDelMes { get; set; } = new();
    private DiaCalendario? _seleccionado;

    private void GenerarCalendario()
    {
        DiasDelMes.Clear();
        var primero = new DateTime(_anio, _mes, 1);

        for (int i = 0; i < (int)primero.DayOfWeek; i++)
            DiasDelMes.Add(new DiaCalendario { EsVisible = false });

        for (int d = 1; d <= DateTime.DaysInMonth(_anio, _mes); d++)
        {
            var fecha = new DateTime(_anio, _mes, d);
            DiasDelMes.Add(new DiaCalendario
            {
                Numero = d,
                Fecha = fecha,
                Puntos = _eventosDelMes
                    .Where(e => e.fecha.Date == fecha.Date).Take(3)
                    .Select(e => Color.FromArgb(string.IsNullOrEmpty(e.color) ? "#1A73E8" : e.color))
                    .ToList()
            });
        }
    }

    private void DiaTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not DiaCalendario dia || !dia.EsVisible) return;

        if (_seleccionado != null) _seleccionado.Seleccionado = false;
        _seleccionado = dia;
        dia.Seleccionado = true;

        var del_dia = _eventosDelMes.Where(ev => ev.fecha.Date == dia.Fecha.Date).ToList();
        ListaEventos.Clear();
        foreach (var ev in del_dia.Any() ? del_dia : _eventosDelMes)
            ListaEventos.Add(ev);
    }

    private async void MesAnterior_Clicked(object? sender, EventArgs e)
    {
        if (--_mes < 1) { _mes = 12; _anio--; }
        await CargarMes();
    }

    private async void MesSiguiente_Clicked(object? sender, EventArgs e)
    {
        if (++_mes > 12) { _mes = 1; _anio++; }
        await CargarMes();
    }

    private void ListaEventosCollection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    private async void BtnEliminarEventos_Clicked(object? sender, EventArgs e)
    {
        var buton = sender as Button;
        var Seleccionado = buton?.CommandParameter as Eventos;

        if(Seleccionado == null) return;

        bool confirmacion = await DisplayAlertAsync("Confirmar Eliminación", "¿Estás seguro de eliminar este Evento?", "Sí", "No");

        if (!confirmacion) return;

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + $"Calendario/eliminarEventos/{Seleccionado.id}";
            try
            {
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    await CargarMes();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de Conexión", ex.Message, "OK");                

            }
        }
    }

    private async void BtnAgregarEventos_Clicked(object? sender, EventArgs e)
    {
        Popup.IsVisible = true;
        Popup.Opacity = 0;
        await Popup.FadeToAsync(1, 250); 
    }

    private async void BtnCerrarPopup_Clicked(object? sender, EventArgs e)
    {
        await Popup.FadeToAsync(0, 200); 
        Popup.IsVisible = false;
    }

    private string ColorRandom()
    {
        var colores = new[] { "#1A73E8", "#E53935", "#43A047", "#FB8C00", "#8E24AA", "#00ACC1", "#F4511E" };
        return colores[new Random().Next(colores.Length)];
    }

    private async void BtnGuardarPopup_Clicked(object? sender, EventArgs e)
    {
        var Evento = new Eventos
        {
           id = ID,
           titulo = txtTitulo.Text,
           descripcion = txtDescripcion.Text,
           fecha = dpFecha.Date ?? DateTime.Now,
           hora = tpHora.Time,
           color = ColorRandom()
        };

        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + "Calendario/guardarEvento";

            try
            {
                var json = JsonSerializer.Serialize(Evento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    txtTitulo.Text = string.Empty;
                    txtDescripcion.Text = string.Empty;
                    dpFecha.Date = DateTime.Now;
                    tpHora.Time = tpHora.Time;

                    await Popup.FadeToAsync(0, 200);
                    Popup.IsVisible = false;

                    await CargarMes();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error de conexion", ex.Message, "OK");                
            }
        }
    }
}