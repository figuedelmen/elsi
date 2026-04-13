using Microsoft.AspNetCore.Mvc;
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


        [HttpPost("Guardar")]
        public IActionResult Guardar([FromBody] string Id)
        {
            try
            {
                using (var conexion = _llave_maestra.conexion())
                {
                    string sql = "";

                    using (var comando = new MySqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("", Id);
                        comando.ExecuteNonQuery();

                        return BadRequest(new { message = "Ahorro guardado exitosamente" });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
