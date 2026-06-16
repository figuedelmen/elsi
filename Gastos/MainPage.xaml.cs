using System.Text.Json;
using System.Text;
using System;

namespace Gastos;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
		InitializeComponent();
	}

    public class loginUser
    {
        public string nombre {get;set;} = string.Empty;
        public string contraseña {get;set;} = string.Empty;
    }

	public class loginResponse
    {
        public string message {get;set;} = string.Empty;
        public int id {get;set;}
    }

    private async void BtnInicio_Clicked(object? sender, EventArgs e)
	{
		var datos = new loginUser
		{
			nombre = TxtNombre.Text,
			contraseña = TxtContraseña.Text
		};

		using (var client = new HttpClient())
		{
			var url = Conexion.BaseUrl + "Calendario/Info";

			var json = JsonSerializer.Serialize(datos);

			var content = new StringContent(json, Encoding.UTF8, "application/json");

			try
			{
				var response = await client.PostAsync(url, content);
				if (response.IsSuccessStatusCode)
				{
					var respuesta = await response.Content.ReadAsStringAsync();

					var datosRes = JsonSerializer.Deserialize<loginResponse>(respuesta);

					int idUsuario = datosRes?.id ?? 0;

				    var MenuDatos = new MenuDatos(idUsuario);

                    await Navigation.PushAsync(MenuDatos);
				}
			}
			catch (Exception ex) 
			{
                await DisplayAlertAsync("Error", ex.Message, "OK");
			}
		}
	}
}
