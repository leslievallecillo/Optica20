using System;
using System.Web;
using System.Web.SessionState;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica
{
    public class KeepAlive : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            // Solo responde si hay usuario logueado
            if (context.Session["UsuarioID"] != null)
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    try
                    {
                        con.Open();
                        // Actualizamos la última vez que vimos al usuario
                        string sql = "UPDATE Usuario SET UltimaConexion = NOW(), EnLinea = 1 WHERE ID_Usuario = @UID";
                        using (MySqlCommand cmd = new MySqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@UID", context.Session["UsuarioID"]);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch { }
                }
            }
        }

        public bool IsReusable { get { return false; } }
    }
}