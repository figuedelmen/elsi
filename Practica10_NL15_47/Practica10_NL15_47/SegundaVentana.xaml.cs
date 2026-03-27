namespace Practica10_NL15_47;

public partial class SegundaVentana : ContentPage
{
	public SegundaVentana()
	{
		InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(UrlDestino))
        {
            Web.Source = UrlDestino;
        }
    }

    public string UrlDestino { get; set; }
}