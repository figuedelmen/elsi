using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Proyecto1_NL15_47
{
    public partial class segundaVentana : ContentPage
    {
        public segundaVentana()
        {
            InitializeComponent();

            ListaAhorro = new ObservableCollection<Ahorros>
            {
                new Ahorros { Nombre = "Viaje", Ahorrado = $"Ahorro:{200}" },
                new Ahorros { Nombre = "Compras", Ahorrado = $"Ahorro:{200}" },
            };

            BindingContext = this;
        }

        public ObservableCollection<Ahorros> ListaAhorro { get; set; }

        public class Ahorros
        {
            public string Id_usuario { get; set; } = string.Empty;
            public int Id { get; set; }
            public string Logo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Ahorrado { get; set; } = string.Empty;
        }

        private async void ListAhorro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var AhorroSeleccionado = e.CurrentSelection.FirstOrDefault() as Ahorros;

            if (AhorroSeleccionado != null)
            { 
                var mostrarAhorro = new MostrarAhorro();

                mostrarAhorro.Id_usuario_c = AhorroSeleccionado.Id_usuario;

                mostrarAhorro.Id_c = AhorroSeleccionado.Id;

                mostrarAhorro.Logo_c = AhorroSeleccionado.Logo;

                mostrarAhorro.Nombre_c = AhorroSeleccionado.Nombre;

                mostrarAhorro.Ahorrado_c = AhorroSeleccionado.Ahorrado;

                await Navigation.PushAsync(mostrarAhorro);
            }
        }

        private async void BtnAgregar_Clicked(object sender, EventArgs e)
        {
            var agregarAhorro = new AgregarAhorro();

            await Navigation.PushAsync(agregarAhorro);
        }
    }
}
 