namespace Practica7_NL15_47
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var clave = TexCodigo.Text;

            if (string.IsNullOrWhiteSpace(clave))
            {
                LbVerificacion.Text = "Por favor, ingrese un código.";
            }
        }
    }
}
