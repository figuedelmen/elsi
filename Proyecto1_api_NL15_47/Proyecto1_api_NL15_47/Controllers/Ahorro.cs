using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Ahorro : ControllerBase
    {
        private readonly llave_maestra _llave_maestra;

        public Ahorro(llave_maestra conexion)
        {
            _llave_maestra = conexion;
        }

        public class loginUser
        {
            public string nombre {get; set;} = string.Empty;
            public string contrasena {get;set;} = string.Empty;
        };

        [HttpPost("Info")]
        public IActionResult Sesion([FromBody] loginUser datos)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion())
                {
                    string sql = "SELECT id FROM usuarios WHERE nombre = @nombre AND contrasena = @contrasena";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", datos.nombre);
                        comando.Parameters.AddWithValue("@contrasena", datos.contrasena);

                        var existe = comando.ExecuteScalar();

                        if (existe != null)
                        {
                            int id = Convert.ToInt32(existe);
                            return Ok(new 
                            { 
                                message = "Inicio de sesion perfecto",
                                id = id
                            });
                        }
                        else
                        {
                            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public class AhorroModel
        {
            public int id { get; set; }
            public int id_usuario { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string ahorrado { get; set; } = string.Empty;
            public string logo { get; set; } = string.Empty;
        }

        [HttpGet("cargar/{idUsuario}")]
        public IActionResult Cargar(int idUsuario)
        {
            List<AhorroModel> lista = new List<AhorroModel>();
            try
            {
                using (var conexion = _llave_maestra.conexion())
                {
                    string sql = "SELECT * FROM ahorros WHERE id_usuario = @id";
                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", idUsuario);
                        var reader = comando.ExecuteReader();
                        while (reader.Read())
                        {
                            lista.Add(new AhorroModel
                            {
                                id = reader.GetInt32(0),
                                id_usuario = reader.GetInt32(1),
                                nombre = reader.GetString(2),
                                ahorrado = reader.GetString(3),
                                logo = reader.GetString(4)
                            });
                        }
                    }
                }
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(new{error = ex.Message});                
            }
        }

        public class AhorroAgregar
        {
            public int id_usuario { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string logo { get; set; } = string.Empty;
        }

        [HttpPost("agregar")]
        public IActionResult Agregar(AhorroAgregar datos)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion())
                {
                    string sql = @"INSERT INTO ahorros (id_usuario, nombre, ahorrado, logo) 
                    values (@id,@nombre, @ahorrado, @logo)";
                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", datos.id_usuario);
                        comando.Parameters.AddWithValue("@nombre", datos.nombre);
                        comando.Parameters.AddWithValue("@logo", datos.logo);

                        comando.ExecuteNonQuery();
                    }
                }
                return Ok(datos);
            }
            catch (Exception ex)
            {
                return BadRequest(new{error = ex.Message});                
            }
        }
    }
}
