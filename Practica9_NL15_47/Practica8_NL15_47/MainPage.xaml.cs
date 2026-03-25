namespace Practica8_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnVerificar_Clicked(object sender, EventArgs e)
        {
            if (TexCodigo.Text == "123")
            {
                await Shell.Current.GoToAsync("SegundaPagina");
            }
            else
            {
                await DisplayAlertAsync("Aviso","Codigo incorrecto", "Ok");
            }
        }
    }
}
