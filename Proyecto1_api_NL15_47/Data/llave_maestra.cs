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

        public MySqlConnection conexion1()
        {
            string? servidor = "Server=127.0.0.1;Port=3306;Database=BorrowBack;User ID=root;Password=isma1912;";

            var conexion = new MySqlConnection(servidor);
            conexion.Open();
            return conexion;
        }

        public MySqlConnection conexion2()
        {
            string? servidor = "Server=127.0.0.1;Port=3306;Database=ASTA;User ID=root;Password=isma1912;";

            var conexion = new MySqlConnection(servidor);
            conexion.Open();
            return conexion;
        }

        public MySqlConnection conexion3()
        {
            string? servidor = "Server=127.0.0.1;Port=3306;Database=Bazar;User ID=root;Password=isma1912;";

            var conexion = new MySqlConnection(servidor);
            conexion.Open();
            return conexion;
        }

        public MySqlConnection conexion4()
        {
            string? servidor = "Server=127.0.0.1;Port=3306;Database=Calendario;User ID=root;Password=isma1912;";

            var conexion = new MySqlConnection(servidor);
            conexion.Open();
            return conexion;
        }

        public MySqlConnection conexion5()
        {
            string? servidor = "Server=127.0.0.1;Port=3306;Database=Gastos;User ID=root;Password=isma1912;";

            var conexion = new MySqlConnection(servidor);
            conexion.Open();
            return conexion;
        }

        
    }
}
