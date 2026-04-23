using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Data
{
    public class llave_maestra
    {
        public MySqlConnection conexion()
        {
            string? servidor = "Server=127.0.0.1;Port=3306;Database=ProyectoAhorro;User ID=root;Password=isma1912;";

            var conexion = new MySqlConnection(servidor);
            conexion.Open();
            return conexion;
        }
    }
}
