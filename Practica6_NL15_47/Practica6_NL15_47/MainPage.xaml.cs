namespace Practica6_NL15_47
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            GenerarNumero();
        }

        async void GenerarNumero()
        {
            Random rnd = new Random();
            numero = rnd.Next(1, 100001);

            LbNumero.Text = numero.ToString();

            await Task.Delay(3000);

            LbNumero.Text = "???";
        }

        public int numero;

        private void BtnControlar_Clicked(object sender, EventArgs e)
        {
            var texnumero = Convert.ToInt32(TexNumero.Text);

            if (texnumero == numero)
            {
                 DisplayAlertAsync("Resultado", "Muy bien recordaste el número", "OK");
            }
            else
            {
                DisplayAlertAsync("Resultado", "Incorrecto 😢", "OK");
            }
        }
    }
}
