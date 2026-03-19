namespace Practica3_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void BtnSumar_Clicked(object sender, EventArgs e)
        {
            if (RbSumar.IsChecked == true && RbRestar.IsChecked == true)
            {
                var numero1 = Convert.ToInt32(TexNumero1.Text);

                var numero2 = Convert.ToInt32(TexNumero2.Text);

                var suma = numero1 + numero2;

                var resta = numero1 - numero2;

                TexResultado.Text = $"La suma es:{suma} y la resta es:{resta}";
            }
            else if (RbSumar.IsChecked == true)
            {
                var numero1 = Convert.ToInt32(TexNumero1.Text);

                var numero2 = Convert.ToInt32(TexNumero2.Text);

                var suma = numero1 + numero2;

                TexResultado.Text = $"La suma es:{suma}";

            }
            else if (RbRestar.IsChecked == true)
            {
                var numero1 = Convert.ToInt32(TexNumero1.Text);

                var numero2 = Convert.ToInt32(TexNumero2.Text);

                var resta = numero1 - numero2;

                TexResultado.Text = $"La resta es:{resta}";
            }

        }
    }
}
