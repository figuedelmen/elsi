using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Mysqlx;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class BorrowBack : ControllerBase
    {
        private readonly llave_maestra _llave_maestra;

        public BorrowBack(llave_maestra conexion)
        {
            _llave_maestra = conexion;
        }

        public class loginUser
        {
            public string nombre {get;set;} = string.Empty;
            public string contrasena {get;set;} = string.Empty;
        };

        [HttpPost("Info")]
        public IActionResult Sesion([FromBody] loginUser datos)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion1())
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

        public class Prestamos
        {
            public int id {get; set;}
            public string persona {get;set;} = string.Empty;
            public string objeto {get;set;} = string.Empty;
            public string url {get;set;} = string.Empty;
            public DateTime entrega {get;set;}
            public DateTime devolucion {get;set;}
            public string estado {get;set;} = string.Empty;
        }

        [HttpGet("datos/{idUsuarios}")]
        public async Task<IActionResult> Datos(int idUsuarios)
        {
            try
            {
                List<Prestamos> lista = new List<Prestamos>();

                using (var conexion = _llave_maestra.conexion1())
                {
                    string sqlUpdate = @"UPDATE Prestamos 
                                 SET estado = 'Retrasado' 
                                 WHERE id_usuario = @idUsuarios 
                                 AND estado = 'Pendiente' 
                                 AND fecha_devolucion < NOW()";
            
                    using (var comandoUpdate = new MySqlCommand(sqlUpdate, conexion))
                    {
                        comandoUpdate.Parameters.AddWithValue("@idUsuarios", idUsuarios);
                        await comandoUpdate.ExecuteNonQueryAsync(); // Se ejecuta en silencio antes de leer
                    }

                    string sql = @"SELECT p.id, p.nombre_persona, p.fecha_entrega, 
                    p.fecha_devolucion, p.estado, o.nombre AS nombre_objeto, 
                    o.url AS url_objeto
                    FROM Prestamos p 
                    INNER JOIN Objetos o ON p.id_objeto = o.id 
                    WHERE p.id_usuario = @idUsuarios";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuarios", idUsuarios);
                        
                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                lista.Add(new Prestamos
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    persona = reader.GetString(reader.GetOrdinal("nombre_persona")),
                                    objeto = reader.GetString(reader.GetOrdinal("nombre_objeto")),
                                    url = reader.GetString(reader.GetOrdinal("url_objeto")),
                                    entrega = reader.GetDateTime(reader.GetOrdinal("fecha_entrega")),
                                    devolucion = reader.GetDateTime(reader.GetOrdinal("fecha_devolucion")),
                                    estado = reader.GetString(reader.GetOrdinal("estado"))
                                });
                            }
                        }
                    }
                }
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});
            }
        }


        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarPrestamos([FromBody] Prestamos NuevoPrestamo)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion1())
                {
                    var sqlObjeto = @"INSERT INTO Objetos (nombre, url) VALUES (@nombre, @url);";

                    int idObjeto = 0;

                    using (var comando = new MySqlCommand(sqlObjeto, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", NuevoPrestamo.objeto);
                        comando.Parameters.AddWithValue("@url", NuevoPrestamo.url);

                        await comando.ExecuteNonQueryAsync();
                    }   

                    var sqlId = "SELECT LAST_INSERT_ID()";
                    using (var comando = new MySqlCommand(sqlId, conexion))
                    {
                        idObjeto = Convert.ToInt32(await comando.ExecuteScalarAsync());
                    }

                    var sqlPrestamo = @"INSERT INTO Prestamos ( id_usuario, nombre_persona, id_objeto, fecha_entrega, fecha_devolucion, estado) 
                    VALUES (@idUsuario, @nombrePersona, @idObjeto, @fechaEntrega, @fechaDevolucion, @estado)";

                    using (var comando = new MySqlCommand(sqlPrestamo, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", NuevoPrestamo.id);
                        comando.Parameters.AddWithValue("@nombrePersona", NuevoPrestamo.persona);
                        comando.Parameters.AddWithValue("@idObjeto", idObjeto);
                        comando.Parameters.AddWithValue("@fechaEntrega", NuevoPrestamo.entrega);
                        comando.Parameters.AddWithValue("@fechaDevolucion", NuevoPrestamo.devolucion);
                        comando.Parameters.AddWithValue("@estado", NuevoPrestamo.estado);

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

        [HttpDelete("eliminar/{id}")]
        public IActionResult Eliminar(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion1())
                {
                    string sql = "DELETE FROM Prestamos WHERE id = @id";
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

        [HttpPost("devolver/{id}")]
        public IActionResult Devolver(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion1())
                {
                    string sql = @"UPDATE Prestamos 
                                 SET estado = 'Devuelto' 
                                 WHERE id = @id";

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

    }
}