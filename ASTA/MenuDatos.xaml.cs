using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using Microsoft.Maui.Controls;

namespace ASTA;

public partial class MenuDatos : ContentPage
{
    public MenuDatos(int idUsuario)
    {
        InitializeComponent();  

        this.ID = idUsuario;

        ListaMaterias = new ObservableCollection<Materias>();

        BindingContext = this;

        CargarMaterias();
    }

    public int ID { get; set; }

    public ObservableCollection<Materias> ListaMaterias { get; set; }

    public class Materias
    {
        public int id {get;set;}
        public string nombre {get;set;} = string.Empty;
        public string profesor {get;set;} = string.Empty;
    }

    public async void CargarMaterias()
    {
        using (var client = new HttpClient())
        {
            string url = Conexion.BaseUrl + $"materias/{ID}";

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

                    var listaApi = JsonSerializer.Deserialize<List<Materias>>(json, opciones);

                    if (listaApi != null)
                    {
                        ListaMaterias.Clear();
                        foreach (var materia in listaApi)
                        {
                            ListaMaterias.Add(materia);
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

    public class Horarios
    {
        public int id {get;set;}
        public int idUsuario {get;set;}
        public int idMateria {get;set;}
        public string DiaSemana { get; set; } = string.Empty;
        public TimeSpan horaInicio { get; set; }
        public TimeSpan horaFin { get; set; }
        public string aula { get; set; } = string.Empty;      
    }
}