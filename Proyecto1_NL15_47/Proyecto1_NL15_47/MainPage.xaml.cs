namespace Proyecto1_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnIniciarSesion_Clicked(object sender, EventArgs e)
        {
            var segundapantalla = new segundaVentana();

            await Navigation.PushAsync(segundapantalla);
        }
    }
}
 