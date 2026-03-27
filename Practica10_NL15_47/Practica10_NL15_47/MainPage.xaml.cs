namespace Practica10_NL15_47
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void BtnVer_Clicked(object sender, EventArgs e)
        {
            var texUrl = TexUrl.Text;

            var segundaPantalla = new SegundaPantalla();

            var paginaContenedora = new ContentPage
            {
                Title = "Vista Externa",
                Content = segundaPantalla
            };

            segundaPantalla.BindingContext = new { ValorRecibido = texUrl };

            var nuevaVentana = new Window(paginaContenedora);

            Application.Current.OpenWindow(nuevaVentana);
        }
    }
}
