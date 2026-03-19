namespace Practica4_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void BtnSumar_Clicked(object sender, EventArgs e)
        {
            var opcion = pickerOperacion.SelectedItem?.ToString();

            var numero1 = Convert.ToInt32(TexNumero1.Text);

            var numero2 = Convert.ToInt32(TexNumero2.Text);

            if (opcion == "Sumar")
            {
                var Sumar = numero1 + numero2;

                TexResultado.Text = Sumar.ToString();
            }
            else if (opcion == "Restar")
            {
                var Restar = numero1 - numero2;

                TexResultado.Text = Restar.ToString();
            }
            else if (opcion == "Multiplicar")
            {
                var Multiplicar = numero1 * numero2;

                TexResultado.Text = Multiplicar.ToString();
            }
            else if (opcion == "Dividir")
            {
                var Dividir = numero1 / numero2;

                TexResultado.Text = Dividir.ToString();
            }

        }
    }
}
