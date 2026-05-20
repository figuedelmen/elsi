using System.Text.Json;
using Microsoft.VisualBasic;
using Proyecto1_NL15_47;
using System.Text;
using System;

namespace BorrowBack;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    public class loginUser
    {
        public string nombre {get;set;} = string.Empty;
        public string contrasena {get;set;} = string.Empty;
    };

    public class loginResponse
    {
        public string message {get;set;} = string.Empty;
        public int id {get;set;}
    }


    private async void BtnInicio_Clicked(object? sender, EventArgs e)
	{
	    var datosLogin = new loginUser
        {
            nombre = TxtNombre.Text,
            contrasena = TxtContrasena.Text
        };	

        using (var client = new HttpClient())
        {
            var url = Conexion.BaseUrl + "BorrowBack/Info";

            var json = JsonSerializer.Serialize(datosLogin);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();

                    var datos = JsonSerializer.Deserialize<loginResponse>(respuesta);

                    int idUsuario = datos?.id ?? 0;

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