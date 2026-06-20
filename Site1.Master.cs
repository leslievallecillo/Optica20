using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool esLogin = Request.Url.AbsoluteUri.ToLower().Contains("/formularios/login.aspx");
            if (esLogin) return;

            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Formularios/Login.aspx");
                return;
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (!IsPostBack)
            {
                CargarInfoUsuarioYPermisos();
                ActualizarUltimaConexion();
                ValidarSiSesionSigueActivaEnBD();

                if (DateTime.Now.Second % 10 == 0)
                {
                    LimpiarSesionesInactivas();
                }
            }
        }

        private void CargarInfoUsuarioYPermisos()
        {
            if (Session["UsuarioID"] == null) return;

            int idUsuario = Convert.ToInt32(Session["UsuarioID"]);
            string nombre = "Usuario";
            string apellido = "";
            string rol = "";
            string avatar = "";

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "SELECT Nombres, Apellidos, Rol, Avatar FROM usuario WHERE ID_Usuario = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@uid", idUsuario);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                nombre = reader["Nombres"].ToString();
                                apellido = reader["Apellidos"].ToString();
                                rol = reader["Rol"].ToString();
                                avatar = reader["Avatar"] != DBNull.Value ? reader["Avatar"].ToString() : "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error cargando usuario: " + ex.Message);
                }
            }

            lblUsuarioNombre.InnerText = nombre + " " + apellido;
            lblUsuarioRol.InnerText = rol;

            string defaultAvatar = "https://avataaars.io/?avatarStyle=Circle&topType=ShortHairShortFlat&accessoriesType=Blank&hairColor=Black&facialHairType=Blank&clotheType=BlazerShirt&eyeType=Default&eyebrowType=Default&mouthType=Default&skinColor=Light";

            avatar = avatar != null ? avatar.Trim() : "";

            if (avatar.Contains("avataaars.io") && avatar.Length < 130)
            {
                avatar = defaultAvatar;
            }

            imgUserAvatar.Src = string.IsNullOrEmpty(avatar) || !avatar.StartsWith("http") ? defaultAvatar : avatar;

            liGabinete.Visible = false;
            liVentas.Visible = false;
            liCompras.Visible = false;
            liProductos.Visible = false;
            liContactos.Visible = false;
            liReportes.Visible = false;
            liConfiguracion.Visible = false;
            liAccesos.Visible = false;

            switch (rol)
            {
                case "Administrador":
                    liGabinete.Visible = true;
                    liVentas.Visible = true;
                    liCompras.Visible = true;
                    liProductos.Visible = true;
                    liContactos.Visible = true;
                    liReportes.Visible = true;
                    liConfiguracion.Visible = true;
                    liAccesos.Visible = true;
                    break;

                case "Administrador Financiero":
                    liCompras.Visible = true;
                    liProductos.Visible = true;
                    liContactos.Visible = true;
                    liReportes.Visible = true;
                    liVentas.Visible = true;
                    break;

                case "Vendedor":
                    liVentas.Visible = true;
                    liProductos.Visible = true;
                    liContactos.Visible = true;
                    break;

                case "Optometrista":
                    liGabinete.Visible = true;
                    liProductos.Visible = true;
                    liContactos.Visible = true;
                    break;
            }
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            if (Session["UsuarioID"] != null)
            {
                int uid = Convert.ToInt32(Session["UsuarioID"]);
                string sid = Session.SessionID;

                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    try
                    {
                        con.Open();
                        string sqlSesion = "UPDATE sesionusuario SET Estado='Cerrada', FechaFin=NOW() WHERE SessionID=@Sid";
                        using (MySqlCommand cmd = new MySqlCommand(sqlSesion, con))
                        {
                            cmd.Parameters.AddWithValue("@Sid", sid);
                            cmd.ExecuteNonQuery();
                        }

                        string sqlUser = "UPDATE usuario SET EnLinea=0 WHERE ID_Usuario=@Uid";
                        using (MySqlCommand cmd = new MySqlCommand(sqlUser, con))
                        {
                            cmd.Parameters.AddWithValue("@Uid", uid);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch { }
                }
            }

            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            Response.Redirect("~/Formularios/Login.aspx");
        }

        private void ActualizarUltimaConexion()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "UPDATE usuario SET UltimaConexion=NOW(), EnLinea=1 WHERE ID_Usuario=@Uid";
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Uid", Session["UsuarioID"]);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { }
            }
        }

        private void ValidarSiSesionSigueActivaEnBD()
        {
            if (Session["UsuarioID"] == null) return;

            string sid = Session.SessionID;
            int uid = Convert.ToInt32(Session["UsuarioID"]);

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = @"SELECT COUNT(*) FROM sesionusuario 
                                   WHERE SessionID=@Sid AND ID_Usuario=@Uid AND Estado='Activa'";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Sid", sid);
                        cmd.Parameters.AddWithValue("@Uid", uid);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count == 0)
                        {
                            string checkUser = "SELECT EnLinea FROM usuario WHERE ID_Usuario=@Uid";
                            MySqlCommand cmdCheck = new MySqlCommand(checkUser, con);
                            cmdCheck.Parameters.AddWithValue("@Uid", uid);
                            object enLinea = cmdCheck.ExecuteScalar();

                            if (enLinea != null && Convert.ToBoolean(enLinea) == true)
                            {
                                string restore = "INSERT INTO sesionusuario (ID_Usuario, SessionID, FechaInicio, DireccionIP, Estado) VALUES (@Uid, @Sid, NOW(), '::1', 'Activa')";
                                MySqlCommand cmdRestore = new MySqlCommand(restore, con);
                                cmdRestore.Parameters.AddWithValue("@Uid", uid);
                                cmdRestore.Parameters.AddWithValue("@Sid", sid);
                                cmdRestore.ExecuteNonQuery();
                            }
                            else
                            {
                                Session.Clear();
                                Session.Abandon();
                                FormsAuthentication.SignOut();
                                Response.Redirect("~/Formularios/Login.aspx?motivo=forzado");
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void LimpiarSesionesInactivas()
        {
            int miID = Convert.ToInt32(Session["UsuarioID"]);

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sqlUser = @"UPDATE usuario SET EnLinea=0 
                                       WHERE EnLinea=1 
                                       AND UltimaConexion < DATE_SUB(NOW(), INTERVAL 2 MINUTE)
                                       AND ID_Usuario != @MiID";

                    using (MySqlCommand cmd = new MySqlCommand(sqlUser, con))
                    {
                        cmd.Parameters.AddWithValue("@MiID", miID);
                        cmd.ExecuteNonQuery();
                    }

                    string sqlSesion = @"UPDATE sesionusuario s
                                         JOIN usuario u ON s.ID_Usuario = u.ID_Usuario
                                         SET s.Estado='Expirada', s.FechaFin=NOW()
                                         WHERE s.Estado='Activa' AND u.EnLinea=0";

                    using (MySqlCommand cmd = new MySqlCommand(sqlSesion, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { }
            }
        }
    }
}