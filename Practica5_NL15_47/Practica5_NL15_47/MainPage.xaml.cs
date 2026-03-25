using System.Collections.ObjectModel;

namespace Practica5_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();


            ListaPaises = new ObservableCollection<paises>
            {
                new paises { Nombre = "México", Poblacion = "126 millones" },
                new paises { Nombre = "España", Poblacion = "47 millones" },
                new paises { Nombre = "Argentina", Poblacion = "45 millones" },
                new paises { Nombre = "Colombia", Poblacion = "51 millones" }
            };

            BindingContext = this;
        }

        public ObservableCollection<paises> ListaPaises { get; set; }

        public class paises
        {
            public string Nombre { get; set; } = string.Empty;
            public string Poblacion { get; set; } = string.Empty;
        }

        private void ListPais_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var paisSeleccionado = e.CurrentSelection.FirstOrDefault() as paises;

            if (paisSeleccionado != null)
            {
                TexPoblacion.Text = $"Población de {paisSeleccionado.Nombre}: {paisSeleccionado.Poblacion}";

                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}
