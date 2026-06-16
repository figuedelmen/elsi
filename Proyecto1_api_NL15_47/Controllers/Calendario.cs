using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Calendario : ControllerBase
    {
        private readonly llave_maestra _llave_maestra;

        public Calendario(llave_maestra conexion)
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
                using (var conexion = _llave_maestra.conexion4())
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

        public class Eventos
        {
            public int id {get;set;}
            public string titulo {get;set;} = string.Empty;
            public string descripcion {get;set;} = string.Empty;
            public DateTime fecha {get;set;}
            public TimeSpan? hora {get;set;}
            public string color {get;set;} = string.Empty;
        }

        [HttpGet("eventos/{idUsuario}")]
        public async Task<IActionResult> Evento(int idUsuario)
        {
            try
            {
                List<Eventos> Lista = new List<Eventos>();

                using (var conexion = _llave_maestra.conexion4())
                {
                    string sql = @"SELECT id, titulo, descripcion, fecha, 
                    hora, color 
                    FROM eventos WHERE id_usuario = @idUsuario";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Eventos
                                {
                                    id = reader.GetInt32(0),
                                    titulo = reader.GetString(1),
                                    descripcion = reader.GetString(2),
                                    fecha = reader.GetDateTime(3),
                                    hora = reader.GetFieldValue<TimeSpan>(4),
                                    color = reader.GetString(5)
                                });
                            }    
                        }
                    }
                }
                return Ok(Lista);
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});                
            }
        }

        [HttpDelete("eliminarEventos/{id}")]       
        public IActionResult EliminarEventos(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion4())
                {
                    string sql = "DELETE FROM eventos WHERE id = @id";
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

        [HttpPost("guardarEvento")]
        public async Task<IActionResult> GuardarEvento([FromBody] Eventos Nuevo)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion4())
                {
                    var sql = @"INSERT INTO eventos (id_usuario, titulo, descripcion, fecha, 
                    hora, color) 
                    VALUES (@id_usuario, @titulo, @descripcion, @fecha, 
                    @hora, @color)";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id_usuario", Nuevo.id);
                        comando.Parameters.AddWithValue("@titulo", Nuevo.titulo);
                        comando.Parameters.AddWithValue("@descripcion", Nuevo.descripcion);
                        comando.Parameters.AddWithValue("@fecha", Nuevo.fecha);
                        comando.Parameters.AddWithValue("@hora", Nuevo.hora);
                        comando.Parameters.AddWithValue("@color", Nuevo.color);

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
