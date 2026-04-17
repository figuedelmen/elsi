namespace Proyecto1_NL15_47
{
    public partial class MostrarAhorro : ContentPage
    {
        public MostrarAhorro()
        {
            InitializeComponent();

            DisplayAlertAsync("Error", $"Id usuario:{Id_usuario_c}, Id:{Id_c}, Logo:{Logo_c}, Nombre:{Nombre_c}, Ahorrado:{Ahorrado_c}", "Ok");
        }

        public string Id_usuario_c { get; set; } = string.Empty;
        public int Id_c { get; set; }
        public string Logo_c { get; set; } = string.Empty;
        public string Nombre_c { get; set; } = string.Empty;
        public string Ahorrado_c { get; set; } = string.Empty;
    }
}