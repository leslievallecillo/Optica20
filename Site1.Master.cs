using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases; // Asegúrate de que esta referencia exista

namespace Optica
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. EVITAR BUCLE INFINITO
            bool esLogin = Request.Url.AbsoluteUri.ToLower().Contains("/formularios/login.aspx");
            if (esLogin) return;

            // 2. SEGURIDAD: Validar sesión
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Formularios/Login.aspx");
                return;
            }

            // 3. CONFIGURACIÓN DE CACHÉ
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            if (!IsPostBack)
            {
                // Cargar nombre y aplicar seguridad visual
                CargarInfoUsuarioYPermisos();

                // "LATIDO": Actualizar presencia
                ActualizarUltimaConexion();

                // VALIDACIÓN BD
                ValidarSiSesionSigueActivaEnBD();

                // LIMPIEZA AUTOMÁTICA
                if (DateTime.Now.Second % 10 == 0)
                {
                    LimpiarSesionesInactivas();
                }
            }
        }

        // ================================================================
        // MÉTODO PRINCIPAL: CARGAR DATOS Y APLICAR PERMISOS
        // ================================================================
        private void CargarInfoUsuarioYPermisos()
        {
            if (Session["UsuarioID"] == null) return;

            int idUsuario = Convert.ToInt32(Session["UsuarioID"]);
            string nombre = "Usuario";
            string apellido = "";
            string rol = "";

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "SELECT Nombres, Apellidos, Rol FROM Usuario WHERE ID_Usuario = @uid";
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
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error cargando usuario: " + ex.Message);
                }
            }

            // 1. Mostrar datos en el menú lateral
            lblUsuarioNombre.InnerText = nombre + " " + apellido;
            lblUsuarioRol.InnerText = rol;

            // 2. APLICAR LÓGICA DE ROLES (OCULTAR/MOSTRAR MENÚS)

            // Primero ocultamos todo por seguridad
            liGabinete.Visible = false;
            liVentas.Visible = false;
            liCompras.Visible = false;
            liProductos.Visible = false;
            liContactos.Visible = false;
            liReportes.Visible = false;
            liConfiguracion.Visible = false;
            liAccesos.Visible = false;

            // Activamos según el rol exacto de la base de datos
            switch (rol)
            {
                case "Administrador":
                    // El Admin ve TODO
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
                    // Ve compras, reportes, productos y proveedores
                    liCompras.Visible = true;
                    liProductos.Visible = true;
                    liContactos.Visible = true; // Para proveedores
                    liReportes.Visible = true;
                    liVentas.Visible = true; // Solo para ver historial, no anular (controlar dentro de la pág)
                    break;

                case "Vendedor":
                    // Ve ventas, clientes y productos (inventario)
                    liVentas.Visible = true;
                    liProductos.Visible = true;
                    liContactos.Visible = true; // Para clientes
                    break;

                case "Optometrista":
                    // Ve gabinete y productos (lentes)
                    liGabinete.Visible = true;
                    liProductos.Visible = true;
                    liContactos.Visible = true; // Para ver pacientes/clientes
                    break;

                default:
                    // Rol desconocido: Solo verá "Inicio" (que no tiene ID ni runat=server)
                    break;
            }
        }

        // ================================================================
        // MÉTODOS DE SESIÓN (Mantener tal cual los tenías)
        // ================================================================

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
                        string sqlSesion = "UPDATE SesionUsuario SET Estado='Cerrada', FechaFin=NOW() WHERE SessionID=@Sid";
                        using (MySqlCommand cmd = new MySqlCommand(sqlSesion, con))
                        {
                            cmd.Parameters.AddWithValue("@Sid", sid);
                            cmd.ExecuteNonQuery();
                        }

                        string sqlUser = "UPDATE Usuario SET EnLinea=0 WHERE ID_Usuario=@Uid";
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
                    string sql = "UPDATE Usuario SET UltimaConexion=NOW(), EnLinea=1 WHERE ID_Usuario=@Uid";
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
                    string sql = @"SELECT COUNT(*) FROM SesionUsuario 
                                   WHERE SessionID=@Sid AND ID_Usuario=@Uid AND Estado='Activa'";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Sid", sid);
                        cmd.Parameters.AddWithValue("@Uid", uid);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count == 0)
                        {
                            string checkUser = "SELECT EnLinea FROM Usuario WHERE ID_Usuario=@Uid";
                            MySqlCommand cmdCheck = new MySqlCommand(checkUser, con);
                            cmdCheck.Parameters.AddWithValue("@Uid", uid);
                            object enLinea = cmdCheck.ExecuteScalar();

                            if (enLinea != null && Convert.ToBoolean(enLinea) == true)
                            {
                                string restore = "INSERT INTO SesionUsuario (ID_Usuario, SessionID, FechaInicio, DireccionIP, Estado) VALUES (@Uid, @Sid, NOW(), '::1', 'Activa')";
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
                    string sqlUser = @"UPDATE Usuario SET EnLinea=0 
                                       WHERE EnLinea=1 
                                       AND UltimaConexion < DATE_SUB(NOW(), INTERVAL 2 MINUTE)
                                       AND ID_Usuario != @MiID";

                    using (MySqlCommand cmd = new MySqlCommand(sqlUser, con))
                    {
                        cmd.Parameters.AddWithValue("@MiID", miID);
                        cmd.ExecuteNonQuery();
                    }

                    string sqlSesion = @"UPDATE SesionUsuario s
                                         JOIN Usuario u ON s.ID_Usuario = u.ID_Usuario
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