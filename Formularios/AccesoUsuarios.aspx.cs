using MySql.Data.MySqlClient;
using Optica.Clases;
using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Optica.AdministrarAccesos
{
    public partial class AccesoUsuarios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // SOLO esto debe estar en Page_Load
            if (!IsPostBack)
            {
                LimpiarSesionesHuerfanas(); // Agregar este método
                CargarRoles();
                CargarUsuarios();
                ActualizarEstadisticas();
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }

        // AGREGAR este método para limpiar sesiones huérfanas
        private void LimpiarSesionesHuerfanas()
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    conexion.Open();

                    // Cerrar sesiones que llevan más de 24 horas activas
                    string query = @"
                        UPDATE sesionusuario 
                        SET Estado = 'Expirada', FechaFin = NOW() 
                        WHERE Estado = 'Activa' AND FechaInicio < DATE_SUB(NOW(), INTERVAL 24 HOUR)";

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        int sesionesCerradas = comando.ExecuteNonQuery();
                        if (sesionesCerradas > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Sesiones huérfanas limpiadas: {sesionesCerradas}");
                        }
                    }

                    // Actualizar usuarios que no deberían estar en línea
                    string queryUsuarios = @"
                        UPDATE usuario u
                        LEFT JOIN sesionusuario s ON u.ID_Usuario = s.ID_Usuario AND s.Estado = 'Activa'
                        SET u.EnLinea = 0
                        WHERE u.EnLinea = 1 AND s.ID_Sesion IS NULL";

                    using (MySqlCommand comando = new MySqlCommand(queryUsuarios, conexion))
                    {
                        comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al limpiar sesiones huérfanas: {ex.Message}");
                }
            }
        }

        // AGREGAR este método que falta
        private void RegistrarAuditoriaLogin(int usuarioID, string accion, bool exitoso, string descripcion)
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    conexion.Open();

                    string query = @"
                        INSERT INTO auditorialogin (ID_Usuario, DireccionIP, Accion, Exitoso, Descripcion) 
                        VALUES (@UsuarioID, @DireccionIP, @Accion, @Exitoso, @Descripcion)";

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@UsuarioID", usuarioID);
                        comando.Parameters.AddWithValue("@DireccionIP", ObtenerDireccionIP());
                        comando.Parameters.AddWithValue("@Accion", accion);
                        comando.Parameters.AddWithValue("@Exitoso", exitoso);
                        comando.Parameters.AddWithValue("@Descripcion", descripcion);
                        comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al registrar auditoría: {ex.Message}");
                }
            }
        }

        private void CargarRoles()
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "SELECT DISTINCT Rol FROM usuario WHERE Estado = 1 AND Rol IS NOT NULL ORDER BY Rol";

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            ddlRol.Items.Clear();
                            ddlRol.Items.Add(new ListItem("Todos los roles", ""));

                            while (reader.Read())
                            {
                                string rol = reader["Rol"].ToString();
                                if (!string.IsNullOrEmpty(rol))
                                {
                                    ddlRol.Items.Add(new ListItem(rol, rol));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al cargar roles: {ex.Message}");
                }
            }
        }

        private void CargarUsuarios()
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    conexion.Open();

                    string query = @"
                    SELECT 
                        u.ID_Usuario,
                        u.Nombres,
                        u.Apellidos,
                        u.Correo,
                        u.Rol,
                        u.EnLinea,
                        u.UltimaConexion,
                        u.Telefono,
                        u.FechaRegistro,
                        i.NombreUsuario
                    FROM usuario u
                    LEFT JOIN iniciosesion i ON u.ID_Usuario = i.ID_Usuario
                    WHERE u.Estado = 1";

                    // Aplicar filtros
                    if (!string.IsNullOrEmpty(ddlEstado.SelectedValue))
                    {
                        query += " AND u.EnLinea = @Estado";
                    }

                    if (!string.IsNullOrEmpty(ddlRol.SelectedValue))
                    {
                        query += " AND u.Rol = @Rol";
                    }

                    query += " ORDER BY u.EnLinea DESC, u.UltimaConexion DESC";

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        if (!string.IsNullOrEmpty(ddlEstado.SelectedValue))
                        {
                            comando.Parameters.AddWithValue("@Estado", ddlEstado.SelectedValue);
                        }

                        if (!string.IsNullOrEmpty(ddlRol.SelectedValue))
                        {
                            comando.Parameters.AddWithValue("@Rol", ddlRol.SelectedValue);
                        }

                        DataTable dt = new DataTable();
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(comando))
                        {
                            adapter.Fill(dt);
                        }

                        gvUsuarios.DataSource = dt;
                        gvUsuarios.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al cargar usuarios: {ex.Message}");
                }
            }
        }

        private void ActualizarEstadisticas()
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    conexion.Open();

                    string query = @"
                    SELECT 
                        COUNT(*) as TotalUsuarios,
                        SUM(CASE WHEN EnLinea = 1 THEN 1 ELSE 0 END) as EnLinea,
                        SUM(CASE WHEN EnLinea = 0 THEN 1 ELSE 0 END) as Desconectados
                    FROM usuario 
                    WHERE Estado = 1";

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        using (MySqlDataReader reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                onlineCount.InnerText = reader["EnLinea"].ToString();
                                offlineCount.InnerText = reader["Desconectados"].ToString();
                                totalUsers.InnerText = reader["TotalUsuarios"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al cargar estadísticas: {ex.Message}");
                }
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
            ActualizarEstadisticas();
        }

        protected void ddlEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void ddlRol_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void btnForzarCierre_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            int usuarioID = Convert.ToInt32(btn.CommandArgument);

            ForzarCierreSesion(usuarioID);
            CargarUsuarios();
            ActualizarEstadisticas();

            // Mostrar mensaje de éxito
            ScriptManager.RegisterStartupScript(this, this.GetType(), "success", "alert('Sesión cerrada forzadamente');", true);
        }

        private void ForzarCierreSesion(int usuarioID)
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    conexion.Open();

                    // Cerrar TODAS las sesiones activas del usuario
                    string querySesiones = @"
                        UPDATE sesionusuario 
                        SET Estado = 'Cerrada', FechaFin = NOW() 
                        WHERE ID_Usuario = @UsuarioID AND Estado = 'Activa'";

                    using (MySqlCommand comando = new MySqlCommand(querySesiones, conexion))
                    {
                        comando.Parameters.AddWithValue("@UsuarioID", usuarioID);
                        comando.ExecuteNonQuery();
                    }

                    // Actualizar estado del usuario
                    string queryUsuario = @"
                        UPDATE usuario 
                        SET EnLinea = 0 
                        WHERE ID_Usuario = @UsuarioID";

                    using (MySqlCommand comando = new MySqlCommand(queryUsuario, conexion))
                    {
                        comando.Parameters.AddWithValue("@UsuarioID", usuarioID);
                        comando.ExecuteNonQuery();
                    }

                    // Registrar en auditoría
                    string queryAuditoria = @"
                        INSERT INTO auditorialogin (ID_Usuario, DireccionIP, Accion, Exitoso, Descripcion) 
                        VALUES (@UsuarioID, @DireccionIP, 'Logout', 1, 'Cierre de sesión forzado por administrador')";

                    using (MySqlCommand comando = new MySqlCommand(queryAuditoria, conexion))
                    {
                        comando.Parameters.AddWithValue("@UsuarioID", usuarioID);
                        comando.Parameters.AddWithValue("@DireccionIP", ObtenerDireccionIP());
                        comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al forzar cierre de sesión: {ex.Message}");
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "error", "alert('Error al forzar cierre de sesión');", true);
                }
            }
        }

        // Método auxiliar para obtener iniciales
        private string GetInitials(string nombres, string apellidos)
        {
            if (string.IsNullOrEmpty(nombres) && string.IsNullOrEmpty(apellidos))
                return "??";

            string primeraLetraNombre = "";
            string primeraLetraApellido = "";

            if (!string.IsNullOrEmpty(nombres))
            {
                string[] partesNombre = nombres.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (partesNombre.Length > 0)
                {
                    primeraLetraNombre = partesNombre[0].Length > 0 ? partesNombre[0][0].ToString() : "";
                }
            }

            if (!string.IsNullOrEmpty(apellidos))
            {
                string[] partesApellido = apellidos.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (partesApellido.Length > 0)
                {
                    primeraLetraApellido = partesApellido[0].Length > 0 ? partesApellido[0][0].ToString() : "";
                }
            }

            string iniciales = $"{primeraLetraNombre}{primeraLetraApellido}".ToUpper();
            return string.IsNullOrEmpty(iniciales) ? "??" : iniciales;
        }

        // Método auxiliar para formatear última conexión
        private string FormatLastConnection(object fechaObj)
        {
            if (fechaObj == DBNull.Value || fechaObj == null)
                return "Nunca";

            try
            {
                DateTime ultimaConexion = Convert.ToDateTime(fechaObj);
                TimeSpan diferencia = DateTime.Now - ultimaConexion;

                if (diferencia.TotalMinutes < 1)
                    return "Ahora mismo";
                else if (diferencia.TotalHours < 1)
                    return $"Hace {diferencia.Minutes} min";
                else if (diferencia.TotalDays < 1)
                    return $"Hace {diferencia.Hours} h";
                else if (diferencia.TotalDays < 7)
                    return $"Hace {diferencia.Days} d";
                else
                    return ultimaConexion.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return "Fecha inválida";
            }
        }

        // Método auxiliar para contar sesiones activas
        private string GetActiveSessionsCount(int usuarioID)
        {
            try
            {
                using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
                {
                    conexion.Open();
                    string query = "SELECT COUNT(*) FROM sesionusuario WHERE ID_Usuario = @UsuarioID AND Estado = 'Activa'";

                    using (MySqlCommand comando = new MySqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@UsuarioID", usuarioID);
                        object result = comando.ExecuteScalar();
                        return result?.ToString() ?? "0";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al contar sesiones: {ex.Message}");
                return "0";
            }
        }

        protected void gvUsuarios_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Obtener los datos de la fila
                DataRowView rowView = (DataRowView)e.Row.DataItem;

                if (rowView != null)
                {
                    // Configurar iniciales
                    Literal ltIniciales = (Literal)e.Row.FindControl("ltIniciales");
                    if (ltIniciales != null)
                    {
                        string nombres = rowView["Nombres"] != DBNull.Value ? rowView["Nombres"].ToString() : "";
                        string apellidos = rowView["Apellidos"] != DBNull.Value ? rowView["Apellidos"].ToString() : "";
                        ltIniciales.Text = GetInitials(nombres, apellidos);
                    }

                    // Configurar última conexión
                    Literal ltUltimaConexion = (Literal)e.Row.FindControl("ltUltimaConexion");
                    if (ltUltimaConexion != null)
                    {
                        ltUltimaConexion.Text = FormatLastConnection(rowView["UltimaConexion"]);
                    }

                    // Configurar sesiones activas
                    Literal ltSesionesActivas = (Literal)e.Row.FindControl("ltSesionesActivas");
                    if (ltSesionesActivas != null)
                    {
                        int usuarioID = Convert.ToInt32(rowView["ID_Usuario"]);
                        ltSesionesActivas.Text = GetActiveSessionsCount(usuarioID);
                    }

                    // Configurar controles de visibilidad
                    Label spanOffline = (Label)e.Row.FindControl("spanOffline");
                    LinkButton btnForzarCierre = (LinkButton)e.Row.FindControl("btnForzarCierre");

                    bool enLinea = Convert.ToBoolean(rowView["EnLinea"]);

                    if (btnForzarCierre != null)
                        btnForzarCierre.Visible = enLinea;

                    if (spanOffline != null)
                        spanOffline.Visible = !enLinea;
                }
            }
        }

        private string ObtenerDireccionIP()
        {
            string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip ?? "Desconocida";
        }
    }
}