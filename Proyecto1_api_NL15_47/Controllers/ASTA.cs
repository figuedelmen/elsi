using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class ASTA : ControllerBase
    {
        private readonly llave_maestra _llave_maestra;

        public ASTA(llave_maestra conexion)
        {
            _llave_maestra = conexion;
        }

        public class loginUser
        {
            public string nombre {get;set;} = string.Empty;
            public string contraseña {get;set;} = string.Empty;
        }

        [HttpPost("Info")]
        public IActionResult sesion([FromBody] loginUser datos)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = "SELECT id FROM usuarios WHERE nombre = @nombre AND contraseña = @contraseña";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", datos.nombre);
                        comando.Parameters.AddWithValue("@contraseña", datos.contraseña);

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
                return BadRequest(new {error = ex.Message});                
            }
        }

        public class Materias
        {
            public int id {get;set;}
            public string nombre {get;set;} = string.Empty;
            public string profesor {get;set;} = string.Empty;
        }
    }
}