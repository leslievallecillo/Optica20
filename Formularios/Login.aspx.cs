using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using MySql.Data.MySqlClient;
using Optica.Clases;
using System.Security.Cryptography;
using System.Text;

namespace Optica.Formularios
{
    public partial class Login : System.Web.UI.Page
    {
        private class InfoBloqueo
        {
            public int Intentos { get; set; }
            public DateTime FinBloqueo { get; set; }
            public int MinutosPenalizacion { get; set; }

            public InfoBloqueo()
            {
                Intentos = 0;
                FinBloqueo = DateTime.MinValue;
                MinutosPenalizacion = 1;
            }
        }

        private static Dictionary<string, InfoBloqueo> rastreoIps = new Dictionary<string, InfoBloqueo>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UsuarioID"] != null)
                {
                    Response.Redirect("Default.aspx");
                }

                if (Request.QueryString["motivo"] == "forzado")
                {
                    lblError.Text = "Su sesión fue cerrada por un administrador.";
                    lblError.Visible = true;
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            string usuario = txtUsuario.Text.Trim();
            string clave = txtClave.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
            {
                lblError.Text = "Por favor ingrese usuario y contraseña.";
                lblError.Visible = true;
                return;
            }

            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            if (!rastreoIps.ContainsKey(ip))
            {
                rastreoIps[ip] = new InfoBloqueo();
            }

            InfoBloqueo info = rastreoIps[ip];

            if (DateTime.Now < info.FinBloqueo)
            {
                TimeSpan restante = info.FinBloqueo - DateTime.Now;
                lblError.Text = $"Sistema bloqueado. Intente en {Math.Ceiling(restante.TotalMinutes)} minuto(s).";
                lblError.Visible = true;
                return;
            }

            if (Autenticar(usuario, clave))
            {
                rastreoIps.Remove(ip);
                Response.Redirect("Default.aspx", false);
            }
            else
            {
                info.Intentos++;

                if (info.Intentos >= 3)
                {
                    info.FinBloqueo = DateTime.Now.AddMinutes(info.MinutosPenalizacion);
                    lblError.Text = $"Demasiados intentos fallidos. Sistema bloqueado por {info.MinutosPenalizacion} minuto(s).";
                    info.MinutosPenalizacion++;
                    info.Intentos = 0;
                }
                else
                {
                    lblError.Text = "Usuario o contraseña incorrectos.";
                }

                lblError.Visible = true;
            }
        }

        private bool Autenticar(string usuario, string clave)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();

                    string sql = "SELECT ID_Usuario, Clave, Estado FROM InicioSesion WHERE NombreUsuario = @User";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@User", usuario);

                    string hashBD = "";
                    int idUsuario = 0;
                    bool usuarioExiste = false;
                    bool usuarioActivo = true;

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            hashBD = r["Clave"].ToString();
                            idUsuario = Convert.ToInt32(r["ID_Usuario"]);
                            usuarioActivo = Convert.ToBoolean(r["Estado"]);
                            usuarioExiste = true;
                        }
                    }

                    if (usuarioExiste)
                    {
                        if (!usuarioActivo)
                        {
                            lblError.Text = "El usuario está desactivado.";
                            lblError.Visible = true;
                            return false;
                        }

                        if (VerificarHash(clave, hashBD))
                        {
                            Session["UsuarioID"] = idUsuario;
                            Session["NombreUsuario"] = usuario;

                            RegistrarSesionBD(idUsuario, Session.SessionID, con);

                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error Login: " + ex.Message);
                    lblError.Text = "Error de conexión con el servidor.";
                    lblError.Visible = true;
                }
            }
            return false;
        }

        private void RegistrarSesionBD(int idUsuario, string sessionID, MySqlConnection con)
        {
            string sqlClean = "UPDATE SesionUsuario SET Estado='Cerrada' WHERE SessionID=@Sid";
            using (MySqlCommand cmd = new MySqlCommand(sqlClean, con))
            {
                cmd.Parameters.AddWithValue("@Sid", sessionID);
                cmd.ExecuteNonQuery();
            }

            string sqlInsert = @"INSERT INTO SesionUsuario (ID_Usuario, SessionID, FechaInicio, DireccionIP, Estado) 
                                 VALUES (@Uid, @Sid, NOW(), @IP, 'Activa')";

            using (MySqlCommand cmd = new MySqlCommand(sqlInsert, con))
            {
                cmd.Parameters.AddWithValue("@Uid", idUsuario);
                cmd.Parameters.AddWithValue("@Sid", sessionID);
                string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                cmd.Parameters.AddWithValue("@IP", ip);
                cmd.ExecuteNonQuery();
            }

            string sqlUser = "UPDATE Usuario SET EnLinea=1, UltimaConexion=NOW() WHERE ID_Usuario=@Uid";
            using (MySqlCommand cmd = new MySqlCommand(sqlUser, con))
            {
                cmd.Parameters.AddWithValue("@Uid", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }

        private bool VerificarHash(string textoPlano, string hashBD)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString() == hashBD;
            }
        }
    }
}