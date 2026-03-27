namespace Practica10_NL15_47
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnVer_Clicked(object sender, EventArgs e)
        {
            var texUrl = TexUrl.Text;
            var segundaPantalla = new SegundaVentana();

            segundaPantalla.UrlDestino = texUrl;

            await Navigation.PushAsync(segundaPantalla);
        }
    }
}
