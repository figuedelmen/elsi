using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto1_api_NL15_47.Data;

namespace Proyecto1_api_NL15_47.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Bazar : ControllerBase
    {
        private readonly llave_maestra _llave_maestra;

        public Bazar(llave_maestra conexion)
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
                using (var conexion = _llave_maestra.conexion3())
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

        public class Empeños
        {
            public int id { get; set; }
            public int id_cliente{get;set;}
            public int id_usuarios { get; set; }
            public string descripcion_articulo { get; set; } = string.Empty;
            public string categoria { get; set; } = string.Empty;
            public decimal monto_prestado { get; set; }
            public decimal monto_abonado { get; set; }
            public decimal tasa_interes { get; set; }
            public DateTime fecha_inicio { get; set; }
            public DateTime fecha_vence { get; set; }
            public string estado { get; set; } = string.Empty;
        }

        [HttpGet("empeños/{idUsuario}")]
        public async Task<IActionResult> empeños(int idUsuario)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion3())
                {
                    string sqlVencer = @"UPDATE empeños 
                                        SET estado = 'Vencido',
                                            monto_prestado = monto_prestado + (monto_prestado * (tasa_interes / 100)),
                                            ultima_actualizacion = @hoy
                                        WHERE fecha_vence < @hoy 
                                        AND estado = 'Activo'
                                        AND id_usuario = @idUsuario";

                    using (var comando = new MySqlCommand(sqlVencer, conexion))
                    {
                        comando.Parameters.AddWithValue("@hoy", DateTime.Now);
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);
                        await comando.ExecuteNonQueryAsync();
                    }

                    string sqlInteresDiario = @"UPDATE empeños 
                                                SET monto_prestado = monto_prestado + (monto_prestado * (tasa_interes / 100)),
                                                    ultima_actualizacion = @hoy
                                                WHERE fecha_vence < @hoy 
                                                AND estado = 'Vencido'
                                                AND id_usuario = @idUsuario
                                                AND DATE(ultima_actualizacion) < DATE(@hoy)";

                    using (var comando = new MySqlCommand(sqlInteresDiario, conexion))
                    {
                        comando.Parameters.AddWithValue("@hoy", DateTime.Now);
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);
                        await comando.ExecuteNonQueryAsync();
                    }

                    List<Empeños> Lista = new List<Empeños>();

                    string sql = @"SELECT id, id_cliente, descripcion_articulo, categoria, monto_prestado,
                    monto_abonado, tasa_interes, fecha_inicio, fecha_vence, estado 
                    FROM empeños WHERE id_usuario = @idUsuario";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Empeños
                                {
                                    id = reader.GetInt32(0),
                                    id_cliente = reader.GetInt32(1),
                                    descripcion_articulo = reader.GetString(2),
                                    categoria = reader.GetString(3),
                                    monto_prestado = reader.GetDecimal(4),
                                    monto_abonado = reader.GetDecimal(5),
                                    tasa_interes = reader.GetDecimal(6),
                                    fecha_inicio = reader.GetDateTime(7),
                                    fecha_vence = reader.GetDateTime(8),
                                    estado = reader.GetString(9),
                                });
                            }    
                        }
                    }
                    return Ok(Lista);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});                
            }
        }

        [HttpDelete("eliminarEmpeños/{id}")]       
        public IActionResult EliminarEmpeños(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion3())
                {
                    string sql = "DELETE FROM empeños WHERE id = @id";
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

        [HttpPost("guardarEmpeños")]
        public async Task<IActionResult> GuardarMateria([FromBody] Empeños Nuevo)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion3())
                {
                    var sql = @"INSERT INTO empeños (id_cliente, id_usuario, descripcion_articulo, categoria, monto_prestado,
                    tasa_interes, fecha_vence) 
                    VALUES (@id_cliente, @id_usuario, @descripcion_articulo, @categoria, @monto_prestado,
                    @tasa_interes, @fecha_vence)";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id_cliente", Nuevo.id_cliente);
                        comando.Parameters.AddWithValue("@id_usuario", Nuevo.id_usuarios);
                        comando.Parameters.AddWithValue("@descripcion_articulo", Nuevo.descripcion_articulo);
                        comando.Parameters.AddWithValue("@categoria", Nuevo.categoria);
                        comando.Parameters.AddWithValue("@monto_prestado", Nuevo.monto_prestado);
                        comando.Parameters.AddWithValue("@tasa_interes", Nuevo.tasa_interes);
                        comando.Parameters.AddWithValue("@fecha_vence", Nuevo.fecha_vence);

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

        public class Clientes
        {
            public int id { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string telefono { get; set; } = string.Empty;
            public string identificacion { get; set; } = string.Empty;
        }

        [HttpGet("clientes/{idUsuario}")]
        public async Task<IActionResult> clientes(int idUsuario)
        {
            try
            {
                List<Clientes> Lista = new List<Clientes>();

                using (var conexion = _llave_maestra.conexion3())
                {
                    string sql = @"SELECT id, nombre, telefono, identificacion 
                    FROM clientes WHERE id_usuarios = @idUsuario";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@idUsuario", idUsuario);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Clientes
                                {
                                    id = reader.GetInt32(0),
                                    nombre = reader.GetString(1),
                                    telefono = reader.GetString(2),
                                    identificacion = reader.GetString(3)
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

        [HttpDelete("eliminarClientes/{id}")]       
        public IActionResult EliminarClientes(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion3())
                {
                    string sql = "DELETE FROM clientes WHERE id = @id";
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

        [HttpPost("guardarClientes")]
        public async Task<IActionResult> GuardarClientes([FromBody] Clientes Nuevo)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion3())
                {
                    var sql = @"INSERT INTO clientes (id_usuarios, nombre, telefono, identificacion ) 
                    VALUES (@id_usuarios, @nombre, @telefono, @identificacion)";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id_usuarios", Nuevo.id);
                        comando.Parameters.AddWithValue("@nombre", Nuevo.nombre);
                        comando.Parameters.AddWithValue("@telefono", Nuevo.telefono);
                        comando.Parameters.AddWithValue("@identificacion", Nuevo.identificacion);

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

        public class Pagos
        {
            public int id { get; set; }
            public int id_empeno { get; set; }
            public DateTime fecha_pago { get; set; } = DateTime.Now;
            public decimal monto { get; set; }
            public string concepto { get; set; } = string.Empty;
        }

        [HttpGet("pagos/{id_empeño}")]
        public async Task<IActionResult> pagos(int id_empeño)
        {
            try
            {
                List<Pagos> Lista = new List<Pagos>();

                using (var conexion = _llave_maestra.conexion3())
                {
                    string sql = @"SELECT id, fecha_pago, monto, concepto 
                    FROM pagos WHERE id_empeño = @id_empeño";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@id_empeño", id_empeño);

                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Lista.Add(new Pagos
                                {
                                    id = reader.GetInt32(0),
                                    fecha_pago = reader.GetDateTime(1),
                                    monto = reader.GetDecimal(2),
                                    concepto = reader.GetString(3)
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

        [HttpDelete("eliminarPago/{id}")]       
        public IActionResult EliminarPago(int id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion3())
                {
                    decimal monto = 0;
                    int idEmpeno = 0;

                    string sqlGet = "SELECT monto, id_empeño FROM pagos WHERE id = @id";
                    using (var cmd = new MySqlCommand(sqlGet, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                monto = reader.GetDecimal(0);
                                idEmpeno = reader.GetInt32(1);
                            }
                        }
                    }

                    string sqlUpdate = "UPDATE empeños SET monto_abonado = monto_abonado - @monto WHERE id = @idEmpeno";
                    using (var cmd = new MySqlCommand(sqlUpdate, conexion))
                    {
                        cmd.Parameters.AddWithValue("@monto", monto);
                        cmd.Parameters.AddWithValue("@idEmpeno", idEmpeno);
                        cmd.ExecuteNonQuery();
                    }

                    string sqlDelete = "DELETE FROM pagos WHERE id = @id";
                    using (var cmd = new MySqlCommand(sqlDelete, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});                
            }
        }

        [HttpPost("guardarPago")]
        public async Task<IActionResult> GuardarPagos([FromBody] Pagos Nuevo)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion3())
                {
                    var sql = @"INSERT INTO pagos (id_empeño, fecha_pago, monto, concepto) 
                    VALUES (@id, @fecha_pago, @monto, @concepto )";

                    using (var comando = new MySqlCommand(sql,conexion))
                    {
                        comando.Parameters.AddWithValue("@id", Nuevo.id_empeno);
                        comando.Parameters.AddWithValue("@fecha_pago", Nuevo.fecha_pago);
                        comando.Parameters.AddWithValue("@monto", Nuevo.monto);
                        comando.Parameters.AddWithValue("@concepto", Nuevo.concepto);

                        await comando.ExecuteNonQueryAsync();
                    }

                    string sql_Update = "UPDATE empeños SET monto_abonado = monto_abonado + @monto WHERE id = @idEmpeno";
                    using (var comando = new MySqlCommand(sql_Update, conexion))
                    {
                        comando.Parameters.AddWithValue("@monto", Nuevo.monto);
                        comando.Parameters.AddWithValue("@idEmpeno", Nuevo.id_empeno);
                        await comando.ExecuteNonQueryAsync();
                    }

                    string sql_Estado = "UPDATE empeños SET estado = 'Liquidado' WHERE monto_abonado >= monto_prestado AND id = @idEmpeno";
                    using (var comando = new MySqlCommand(sql_Estado, conexion))
                    {
                        comando.Parameters.AddWithValue("@idEmpeno", Nuevo.id_empeno);
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