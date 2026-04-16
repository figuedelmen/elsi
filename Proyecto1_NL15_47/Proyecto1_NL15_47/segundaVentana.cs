using System.Collections.ObjectModel;

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

        private void ListAhorro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void BtnAgregar_Clicked(object sender, EventArgs e)
        {
            
        }
    }
}
 