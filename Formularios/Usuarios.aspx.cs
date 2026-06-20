using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases; // Asumo que aquí está tu clase Conexion
using System.Security.Cryptography;
using System.Text;

namespace Optica.AdministrarAccesos
{
    public partial class Usuarios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarUsuarios();
            }
        }

        // ==========================================
        // CARGA DE DATOS
        // ==========================================
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
                            u.Telefono, 
                            u.Rol, 
                            u.Estado,
                            i.NombreUsuario
                        FROM usuario u
                        LEFT JOIN iniciosesion i ON u.ID_Usuario = i.ID_Usuario
                        WHERE 1=1";

                    // Filtros dinámicos
                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                    {
                        query += " AND (u.Nombres LIKE @Buscar OR u.Apellidos LIKE @Buscar OR i.NombreUsuario LIKE @Buscar OR u.Correo LIKE @Buscar)";
                    }

                    if (!string.IsNullOrEmpty(ddlFiltroRol.SelectedValue))
                    {
                        query += " AND u.Rol = @Rol";
                    }

                    if (ddlFiltroEstado.SelectedValue != "-1")
                    {
                        query += " AND u.Estado = @Estado";
                    }

                    query += " ORDER BY u.ID_Usuario DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        if (!string.IsNullOrEmpty(txtBuscar.Text))
                            cmd.Parameters.AddWithValue("@Buscar", "%" + txtBuscar.Text + "%");

                        if (!string.IsNullOrEmpty(ddlFiltroRol.SelectedValue))
                            cmd.Parameters.AddWithValue("@Rol", ddlFiltroRol.SelectedValue);

                        if (ddlFiltroEstado.SelectedValue != "-1")
                            cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvUsuarios.DataSource = dt;
                            gvUsuarios.DataBind();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar usuarios", "error");
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        // ==========================================
        // EVENTOS DE BOTONES Y GRID
        // ==========================================
        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All")
                gvUsuarios.AllowPaging = false;
            else
            {
                gvUsuarios.AllowPaging = true;
                gvUsuarios.PageSize = int.Parse(ddlPageSize.SelectedValue);
            }
            CargarUsuarios();
        }

        protected void gvUsuarios_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsuarios.PageIndex = e.NewPageIndex;
            CargarUsuarios();
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            hfIDUsuario.Value = ""; // Vacío indica Nuevo
            lblTituloAccion.Text = "Registrar Nuevo Usuario";
            txtUsuario.Enabled = true; // Permitir editar usuario al crear
            lblPassReq.Visible = true; // Asterisco visible
            helpPass.InnerText = "Mínimo 6 caracteres.";

            PanelListado.Visible = false;
            PanelMantenimiento.Visible = true;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            PanelMantenimiento.Visible = false;
            PanelListado.Visible = true;
        }

        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                CargarDatosEdicion(id);
            }
            else if (e.CommandName == "Desactivar")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                CambiarEstadoUsuario(id, 0);
            }
            else if (e.CommandName == "Reactivar")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                CambiarEstadoUsuario(id, 1);
            }
        }

        // ==========================================
        // LÓGICA DE GUARDADO (INSERT/UPDATE)
        // ==========================================
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtCorreo.Text) || string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(ddlRol.SelectedValue))
            {
                MostrarMensaje("Por favor complete todos los campos obligatorios.", "warning");
                return;
            }

            // Validar contraseña al crear
            bool esNuevo = string.IsNullOrEmpty(hfIDUsuario.Value);
            if (esNuevo && string.IsNullOrWhiteSpace(txtClave.Text))
            {
                MostrarMensaje("La contraseña es obligatoria para nuevos usuarios.", "warning");
                return;
            }

            if (!string.IsNullOrEmpty(txtClave.Text) && txtClave.Text != txtClaveConfirm.Text)
            {
                MostrarMensaje("Las contraseñas no coinciden.", "warning");
                return;
            }

            string telefonoCompleto = "505-" + txtTelefono.Text.Trim(); // Ajuste para base de datos

            if (esNuevo)
            {
                GuardarNuevo(telefonoCompleto);
            }
            else
            {
                GuardarEdicion(Convert.ToInt32(hfIDUsuario.Value), telefonoCompleto);
            }
        }

        private void GuardarNuevo(string telefono)
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                conexion.Open();
                MySqlTransaction transaccion = conexion.BeginTransaction();

                try
                {
                    // 1. Insertar en Tabla Usuario
                    string queryUsuario = @"
                        INSERT INTO usuario (Nombres, Apellidos, Correo, Telefono, Rol, FechaRegistro, Estado)
                        VALUES (@Nombres, @Apellidos, @Correo, @Telefono, @Rol, CURDATE(), 1)";

                    long idGenerado;

                    using (MySqlCommand cmd = new MySqlCommand(queryUsuario, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@Nombres", txtNombre.Text.Trim());
                        cmd.Parameters.AddWithValue("@Apellidos", txtApellido.Text.Trim());
                        cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telefono", telefono);
                        cmd.Parameters.AddWithValue("@Rol", ddlRol.SelectedValue);
                        cmd.ExecuteNonQuery();
                        idGenerado = cmd.LastInsertedId;
                    }

                    // 2. Insertar en Tabla InicioSesion
                    string queryLogin = @"
                        INSERT INTO iniciosesion (ID_Usuario, NombreUsuario, Clave, Estado)
                        VALUES (@ID, @Usuario, @ClaveHash, 1)";

                    using (MySqlCommand cmd = new MySqlCommand(queryLogin, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@ID", idGenerado);
                        cmd.Parameters.AddWithValue("@Usuario", txtUsuario.Text.Trim());
                        // AQUI USAMOS LA FUNCION DE HASH
                        cmd.Parameters.AddWithValue("@ClaveHash", ComputarHashSHA256(txtClave.Text));
                        cmd.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    MostrarMensaje("Usuario registrado exitosamente", "success");
                    PanelMantenimiento.Visible = false;
                    PanelListado.Visible = true;
                    CargarUsuarios();
                }
                catch (MySqlException ex)
                {
                    transaccion.Rollback();
                    if (ex.Number == 1062) // Error de duplicados
                        MostrarMensaje("El Correo o Nombre de Usuario ya existen.", "error");
                    else
                        MostrarMensaje("Error al guardar: " + ex.Message, "error");
                }
            }
        }

        private void GuardarEdicion(int idUsuario, string telefono)
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                conexion.Open();
                MySqlTransaction transaccion = conexion.BeginTransaction();

                try
                {
                    // 1. Actualizar Usuario
                    string queryUsuario = @"
                        UPDATE usuario 
                        SET Nombres=@Nombres, Apellidos=@Apellidos, Correo=@Correo, Telefono=@Telefono, Rol=@Rol
                        WHERE ID_Usuario=@ID";

                    using (MySqlCommand cmd = new MySqlCommand(queryUsuario, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@ID", idUsuario);
                        cmd.Parameters.AddWithValue("@Nombres", txtNombre.Text.Trim());
                        cmd.Parameters.AddWithValue("@Apellidos", txtApellido.Text.Trim());
                        cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telefono", telefono);
                        cmd.Parameters.AddWithValue("@Rol", ddlRol.SelectedValue);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Actualizar Login (Solo si hay nueva contraseña o cambio de usuario)
                    // Nota: Normalmente el NombreUsuario no se edita, pero aquí lo permitimos.
                    string queryLogin = "UPDATE iniciosesion SET NombreUsuario=@Usuario";

                    if (!string.IsNullOrEmpty(txtClave.Text))
                    {
                        queryLogin += ", Clave=@ClaveHash";
                    }
                    queryLogin += " WHERE ID_Usuario=@ID";

                    using (MySqlCommand cmd = new MySqlCommand(queryLogin, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@ID", idUsuario);
                        cmd.Parameters.AddWithValue("@Usuario", txtUsuario.Text.Trim());
                        if (!string.IsNullOrEmpty(txtClave.Text))
                        {
                            cmd.Parameters.AddWithValue("@ClaveHash", ComputarHashSHA256(txtClave.Text));
                        }
                        cmd.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    MostrarMensaje("Usuario actualizado exitosamente", "success");
                    PanelMantenimiento.Visible = false;
                    PanelListado.Visible = true;
                    CargarUsuarios();
                }
                catch (MySqlException ex)
                {
                    transaccion.Rollback();
                    if (ex.Number == 1062)
                        MostrarMensaje("El Correo o Usuario ya está en uso por otro registro.", "error");
                    else
                        MostrarMensaje("Error al actualizar: " + ex.Message, "error");
                }
            }
        }

        // ==========================================
        // EDICIÓN Y ESTADO
        // ==========================================
        private void CargarDatosEdicion(int idUsuario)
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                conexion.Open();
                string query = @"
                    SELECT u.Nombres, u.Apellidos, u.Correo, u.Telefono, u.Rol, i.NombreUsuario
                    FROM usuario u
                    LEFT JOIN iniciosesion i ON u.ID_Usuario = i.ID_Usuario
                    WHERE u.ID_Usuario = @ID";

                using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@ID", idUsuario);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            hfIDUsuario.Value = idUsuario.ToString();
                            txtNombre.Text = r["Nombres"].ToString();
                            txtApellido.Text = r["Apellidos"].ToString();
                            txtCorreo.Text = r["Correo"].ToString();

                            string tel = r["Telefono"].ToString();
                            if (tel.StartsWith("505-")) tel = tel.Replace("505-", "");
                            txtTelefono.Text = tel;

                            ddlRol.SelectedValue = r["Rol"].ToString();
                            txtUsuario.Text = r["NombreUsuario"].ToString();

                            // Limpiar campos de contraseña
                            txtClave.Text = "";
                            txtClaveConfirm.Text = "";

                            // Ajustar UI para edición
                            lblTituloAccion.Text = "Editar Usuario";
                            lblPassReq.Visible = false; // Asterisco oculto
                            helpPass.InnerText = "Dejar en blanco para conservar la contraseña actual.";

                            PanelListado.Visible = false;
                            PanelMantenimiento.Visible = true;
                        }
                    }
                }
            }
        }

        private void CambiarEstadoUsuario(int idUsuario, int nuevoEstado)
        {
            using (MySqlConnection conexion = new MySqlConnection(Conexion.CadenaConexion))
            {
                conexion.Open();
                MySqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    // Actualizar ambas tablas
                    string q1 = "UPDATE usuario SET Estado=@Est WHERE ID_Usuario=@ID";
                    string q2 = "UPDATE iniciosesion SET Estado=@Est WHERE ID_Usuario=@ID";

                    using (MySqlCommand cmd = new MySqlCommand(q1, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@ID", idUsuario);
                        cmd.Parameters.AddWithValue("@Est", nuevoEstado);
                        cmd.ExecuteNonQuery();
                    }
                    using (MySqlCommand cmd = new MySqlCommand(q2, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@ID", idUsuario);
                        cmd.Parameters.AddWithValue("@Est", nuevoEstado);
                        cmd.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    string accion = (nuevoEstado == 1) ? "reactivado" : "desactivado";
                    MostrarMensaje($"Usuario {accion} correctamente.", "success");
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    MostrarMensaje("Error al cambiar estado: " + ex.Message, "error");
                }
            }
        }

        private string ComputarHashSHA256(string textoBruto)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(textoBruto));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void LimpiarFormulario()
        {
            hfIDUsuario.Value = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtCorreo.Text = "";
            txtTelefono.Text = "";
            ddlRol.SelectedIndex = 0;
            txtUsuario.Text = "";
            txtClave.Text = "";
            txtClaveConfirm.Text = "";
        }

        private void MostrarMensaje(string mensaje, string icono)
        {
            // icono: 'success', 'error', 'warning', 'info'
            string script = $"Swal.fire('Sistema', '{mensaje}', '{icono}');";
            ScriptManager.RegisterStartupScript(this, GetType(), "Alert", script, true);
        }
    }
}