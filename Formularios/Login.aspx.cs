using System;
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
        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarBloqueo();

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

        private void VerificarBloqueo()
        {
            HttpCookie cookie = Request.Cookies["LoginOpticaState"];
            if (cookie != null && !string.IsNullOrEmpty(cookie["lockEnd"]))
            {
                long lockEndTicks;
                if (long.TryParse(cookie["lockEnd"], out lockEndTicks))
                {
                    DateTime lockEnd = new DateTime(lockEndTicks);
                    if (DateTime.Now < lockEnd)
                    {
                        int sec = (int)(lockEnd - DateTime.Now).TotalSeconds;
                        lblError.Text = "Calculando tiempo restante...";
                        lblError.Visible = true;
                        ClientScript.RegisterStartupScript(this.GetType(), "BloqueoTimer", $"iniciarTemporizador({sec});", true);
                    }
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

            HttpCookie cookie = Request.Cookies["LoginOpticaState"] ?? new HttpCookie("LoginOpticaState");
            cookie.Expires = DateTime.Now.AddDays(30);

            long lockEndTicks = 0;
            if (!string.IsNullOrEmpty(cookie["lockEnd"]))
            {
                long.TryParse(cookie["lockEnd"], out lockEndTicks);
            }

            DateTime lockEnd = new DateTime(lockEndTicks);

            if (DateTime.Now < lockEnd)
            {
                int sec = (int)(lockEnd - DateTime.Now).TotalSeconds;
                lblError.Text = "Calculando tiempo restante...";
                lblError.Visible = true;
                ClientScript.RegisterStartupScript(this.GetType(), "BloqueoTimer", $"iniciarTemporizador({sec});", true);
                return;
            }

            if (Autenticar(usuario, clave))
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
                Response.Redirect("Default.aspx", false);
            }
            else
            {
                int fails = 0;
                int.TryParse(cookie["fails"], out fails);

                int tier = 0;
                int.TryParse(cookie["tier"], out tier);
                if (tier == 0) tier = 1;

                fails++;

                if (fails >= 3)
                {
                    int minutosBloqueo = (tier == 1) ? 1 : 3;
                    lockEnd = DateTime.Now.AddMinutes(minutosBloqueo);

                    cookie["lockEnd"] = lockEnd.Ticks.ToString();
                    cookie["fails"] = "0";
                    cookie["tier"] = "2";

                    Response.Cookies.Add(cookie);

                    int sec = (int)(lockEnd - DateTime.Now).TotalSeconds;
                    lblError.Text = "Calculando tiempo restante...";
                    lblError.Visible = true;
                    ClientScript.RegisterStartupScript(this.GetType(), "BloqueoTimer", $"iniciarTemporizador({sec});", true);
                }
                else
                {
                    cookie["fails"] = fails.ToString();
                    cookie["tier"] = tier.ToString();
                    Response.Cookies.Add(cookie);

                    lblError.Text = $"Usuario o contraseña incorrectos. Intento {fails} de 3.";
                    lblError.Visible = true;
                }
            }
        }

        private bool Autenticar(string usuario, string clave)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();

                    string sql = "SELECT ID_Usuario, Clave, Estado FROM iniciosesion WHERE NombreUsuario = @User";

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
            string sqlClean = "UPDATE sesionusuario SET Estado='Cerrada' WHERE SessionID=@Sid";
            using (MySqlCommand cmd = new MySqlCommand(sqlClean, con))
            {
                cmd.Parameters.AddWithValue("@Sid", sessionID);
                cmd.ExecuteNonQuery();
            }

            string sqlInsert = @"INSERT INTO sesionusuario (ID_Usuario, SessionID, FechaInicio, DireccionIP, Estado) 
                                 VALUES (@Uid, @Sid, NOW(), @IP, 'Activa')";

            using (MySqlCommand cmd = new MySqlCommand(sqlInsert, con))
            {
                cmd.Parameters.AddWithValue("@Uid", idUsuario);
                cmd.Parameters.AddWithValue("@Sid", sessionID);
                string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                cmd.Parameters.AddWithValue("@IP", ip);
                cmd.ExecuteNonQuery();
            }

            string sqlUser = "UPDATE usuario SET EnLinea=1, UltimaConexion=NOW() WHERE ID_Usuario=@Uid";
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