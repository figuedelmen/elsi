using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Data
{
    public class llave_maestra
    {
        private readonly IConfiguration _configuration;

        public llave_maestra(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MySqlConnection conexion()
        {
            string? servidor = _configuration.GetConnectionString("conexion1");

            var conexion = new MySqlConnection(servidor);
            conexion.Open();
            return conexion;
        }
    }
}
