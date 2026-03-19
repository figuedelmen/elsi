namespace Practica5_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnImage_Clicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Aviso", "Llamando", "OK");
        }
    }
}
