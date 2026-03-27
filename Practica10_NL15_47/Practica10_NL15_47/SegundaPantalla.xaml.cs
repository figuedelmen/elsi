namespace Practica10_NL15_47;

public partial class SegundaPantalla : ContentView
{
    public SegundaPantalla()
    {
        InitializeComponent();

        this.BindingContextChanged += (s, e) =>
        {
            if (BindingContext != null)
            {
                dynamic datos = BindingContext;
                string urlRecibida = datos.ValorRecibido;

                Console.WriteLine($"He recibido: {urlRecibida}");
            }
        };
    }
}