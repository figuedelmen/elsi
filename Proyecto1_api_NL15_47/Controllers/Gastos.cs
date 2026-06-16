using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Gastos : ControllerBase
    {
        private readonly llave_maestra _llave_maestra;

        public Gastos(llave_maestra conexion)
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
                using (var conexion = _llave_maestra.conexion5())
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

        public class Gasto
        {
            public int id {get;set;}
            public decimal monto {get;set;}
            public string descripcion {get;set;} = string.Empty;
            public DateTime fecha {get;set;}
            public int semana {get;set;}
            public int año {get;set;}
        }

        [HttpGet("gastos/{idUsuario}/{semana}/{año}")]
        public async Task<IActionResult> Gastos_get_semana(int idUsuario, int semana, int año)
        {
            try
            {
                List<Gasto> Lista = new List<Gasto>();

                using (var conexion = _llave_maestra.conexion5())
                {
                    string sql = @"SELECT id, monto, descripcion, fecha, 
                        semana, año 
                        FROM gastos WHERE id_usuario = @idUsuario
                        AND semana = @semana AND año = @año";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);
                        comando.Parameters.AddWithValue("@semana", semana);
                        comando.Parameters.AddWithValue("@año", año);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Gasto
                                {
                                    id = reader.GetInt32(0),
                                    monto = reader.GetDecimal(1),
                                    descripcion = reader.GetString(2),
                                    fecha = reader.GetDateTime(3),
                                    semana = reader.GetInt32(4),
                                    año = reader.GetInt32(5)
                                });
                            }
                        }
                    }
                }
                return Ok(Lista);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("eliminarGastos/{id}")]       
        public IActionResult EliminarGastos(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion5())
                {
                    string sql = "DELETE FROM gastos WHERE id = @id";
                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.ExecuteNonQuery();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});                
            }
        }

        [HttpPost("guardarGastos")]
        public async Task<IActionResult> GuardarGastos([FromBody] Gasto Nuevo)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion5())
                {
                    var sql = @"INSERT INTO gastos (id_usuario, monto, descripcion, fecha, 
                    semana, año) 
                    VALUES (@id_usuario, @monto, @descripcion, @fecha, 
                    @semana, @año)";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id_usuario", Nuevo.id);
                        comando.Parameters.AddWithValue("@monto", Nuevo.monto);
                        comando.Parameters.AddWithValue("@descripcion", Nuevo.descripcion);
                        comando.Parameters.AddWithValue("@fecha", Nuevo.fecha);
                        comando.Parameters.AddWithValue("@semana", Nuevo.semana);
                        comando.Parameters.AddWithValue("@año", Nuevo.año);

                        await comando.ExecuteNonQueryAsync();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});                
            }
        }
    }
}
