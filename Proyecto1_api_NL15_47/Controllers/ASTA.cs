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

        [HttpGet("materias/{idUsuario}")]
        public async Task<IActionResult> materias(int idUsuario)
        {
            try
            {
                List<Materias> Lista = new List<Materias>();

                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = @"SELECT id, nombre, profesor 
                    FROM materias WHERE id_usuario = @idUsuario";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Materias
                                {
                                    id = reader.GetInt32(0),
                                    nombre = reader.GetString(1),
                                    profesor = reader.GetString(2)
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

        [HttpDelete("eliminarMateria/{id}")]       
        public IActionResult EliminarMateria(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = "DELETE FROM materias WHERE id = @id";
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

        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarMateria([FromBody] Materias NuevaMateria)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    var sql = @"INSERT INTO materias (id_usuario, nombre, profesor) 
                    VALUES (@id, @nombre, @profesor)";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id", NuevaMateria.id);
                        comando.Parameters.AddWithValue("@nombre", NuevaMateria.nombre);
                        comando.Parameters.AddWithValue("@profesor", NuevaMateria.profesor);

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

        public class Horarios
        {
            public int id {get;set;}
            public string dia_semana { get; set; } = string.Empty;
            public TimeSpan hora_inicio { get; set; }
            public TimeSpan hora_fin { get; set; }
            public string aula { get; set; } = string.Empty;      
        }

        [HttpGet("horarios/{id_materia}")]
        public async Task<IActionResult> horarios(int id_materia)
        {
            try
            {
                List<Horarios> Lista = new List<Horarios>();

                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = @"SELECT id, dia_semana, hora_inicio,
                    hora_fin, aula
                    FROM horarios WHERE id_materia = @id_materia";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_materia", id_materia);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Horarios
                                {
                                    id = reader.GetInt32(0),
                                    dia_semana = reader.GetString(1),
                                    hora_inicio = reader.GetFieldValue<TimeSpan>(2),
                                    hora_fin = reader.GetFieldValue<TimeSpan>(3),
                                    aula = reader.GetString(4)
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

        [HttpDelete("eliminarHorario/{id}")]       
        public IActionResult EliminarHorario(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = "DELETE FROM horarios WHERE id = @id";
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

        [HttpPost("guardarHorario")]
        public async Task<IActionResult> GuardarHorario([FromBody] Horarios NuevoHorario)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    var sql = @"INSERT INTO horarios (id_materia ,dia_semana, hora_inicio,
                    hora_fin, aula) 
                    VALUES (@id_materia, @dia_semana, @hora_inicio, @hora_fin, @aula)";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id_materia", NuevoHorario.id);
                        comando.Parameters.AddWithValue("@dia_semana", NuevoHorario.dia_semana);
                        comando.Parameters.AddWithValue("@hora_inicio", NuevoHorario.hora_inicio);
                        comando.Parameters.AddWithValue("@hora_fin", NuevoHorario.hora_fin);
                        comando.Parameters.AddWithValue("@aula", NuevoHorario.aula);

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

        public class Tareas
        {
            public int id {get;set;}
            public string titulo {get;set;} = string.Empty;
            public string descripcion {get;set;} = string.Empty;
            public DateTime fecha_entrega {get; set;}
            public bool completada {get;set;}
        }

        [HttpGet("tareas/{id_materia}")]
        public async Task<IActionResult> tareas(int id_materia)
        {
            try
            {
                List<Tareas> Lista = new List<Tareas>();

                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = @"SELECT id, titulo, descripcion,
                    fecha_entrega, completada
                    FROM tareas WHERE id_materia = @id_materia";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_materia", id_materia);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Tareas
                                {
                                    id = reader.GetInt32(0),
                                    titulo = reader.GetString(1),
                                    descripcion = reader.GetString(2),
                                    fecha_entrega = reader.GetDateTime(3),
                                    completada = reader.GetBoolean(4)
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

        [HttpDelete("eliminarTareas/{id}")]       
        public IActionResult EliminarTareas(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = "DELETE FROM tareas WHERE id = @id";
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

        [HttpPost("guardarTareas")]
        public async Task<IActionResult> GuardarTareas([FromBody] Tareas NuevaTareas)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    var sql = @"INSERT INTO tareas (id_materia ,titulo, descripcion,
                    fecha_entrega, completada) 
                    VALUES (@id_materia, @titulo, @descripcion, @fecha_entrega, @completada)";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id_materia", NuevaTareas.id);
                        comando.Parameters.AddWithValue("@titulo", NuevaTareas.titulo);
                        comando.Parameters.AddWithValue("@descripcion", NuevaTareas.descripcion);
                        comando.Parameters.AddWithValue("@fecha_entrega", NuevaTareas.fecha_entrega);
                        comando.Parameters.AddWithValue("@completada", NuevaTareas.completada);

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

        [HttpPost("completadoTareas/{id}")]
        public async Task<IActionResult> completado(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion2())
                {
                    string sql = @"UPDATE tareas 
                    SET completada = true
                    WHERE id = @id";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        await comando.ExecuteNonQueryAsync();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new{error = ex.Message});                
            }
        }

    }
}