namespace Practica1_NL15_47
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void BtnSumar_Clicked(object sender, EventArgs e)
        {
            var numero1 = Convert.ToInt32(TexNumero1.Text);

            var numero2 = Convert.ToInt32(TexNumero2.Text);

            var suma = numero1 + numero2;

            TexResultado.Text = suma.ToString();
        }
    }
}
