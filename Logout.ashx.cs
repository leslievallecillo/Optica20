using System;
using System.Web;
using System.Web.SessionState;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica
{
    public class Logout : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            if (context.Session["UsuarioID"] != null)
            {
                string uid = context.Session["UsuarioID"].ToString();
                string sid = context.Session.SessionID;

                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    try
                    {
                        con.Open();
                        // 1. Cerrar esta sesión específica en la BD
                        string sql1 = "UPDATE SesionUsuario SET Estado='Cerrada', FechaFin=NOW() WHERE SessionID=@Sid";
                        using (MySqlCommand cmd = new MySqlCommand(sql1, con))
                        {
                            cmd.Parameters.AddWithValue("@Sid", sid);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Poner al usuario como Offline inmediatamente
                        string sql2 = "UPDATE Usuario SET EnLinea=0 WHERE ID_Usuario=@Uid";
                        using (MySqlCommand cmd = new MySqlCommand(sql2, con))
                        {
                            cmd.Parameters.AddWithValue("@Uid", uid);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception) { /* Ignorar errores al cerrar ventana */ }
                }

                // Destruir sesión del servidor
                context.Session.Abandon();
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}