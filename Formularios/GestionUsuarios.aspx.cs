// (Mismos usings que antes)
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Mysqlx;
using Optica.AdministrarAccesos;

namespace Optica.Formularios
{
    public partial class GestionUsuarios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) { CargarDatos(); }
        }

        // --- NUEVO METODO: MOSTRAR TODO ---
        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroRol.SelectedIndex = 0;
            ddlFiltroEstado.SelectedValue = "-1";
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = @"SELECT u.ID_Usuario, 
                                            CONCAT(u.Nombres, ' ', u.Apellidos) AS NombreCompleto,
                                            u.Rol, u.Correo, u.Estado,
                                            i.NombreUsuario
                                     FROM Usuario u
                                     LEFT JOIN InicioSesion i ON u.ID_Usuario = i.ID_Usuario
                                     WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                        query += " AND (u.Nombres LIKE @Busq OR u.Apellidos LIKE @Busq OR i.NombreUsuario LIKE @Busq) ";
                    if (!string.IsNullOrEmpty(ddlFiltroRol.SelectedValue))
                        query += " AND u.Rol = @Rol ";
                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND u.Estado = @Estado ";

                    query += " ORDER BY u.ID_Usuario DESC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Rol", ddlFiltroRol.SelectedValue);
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvUsuarios.DataSource = dt;
                    gvUsuarios.DataBind();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        // ... (RESTO DE METODOS DE VALIDACIÓN, GUARDAR Y LIMPIEZA IGUAL QUE ANTES) ...
        // (Asegúrate de incluir LimpiarTexto, ContarPalabras, ValidarFormulario, etc.)

        private string LimpiarTexto(string entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada)) return "";
            string limpio = Regex.Replace(entrada.Trim(), @"\s+", " ");
            TextInfo textInfo = new CultureInfo("es-NI", false).TextInfo;
            return textInfo.ToTitleCase(limpio.ToLower());
        }

        private int ContarPalabras(string texto) { return string.IsNullOrWhiteSpace(texto) ? 0 : texto.Trim().Split(' ').Length; }

        private bool ValidarFormulario()
        {
            LimpiarErrores();
            bool esValido = true;
            string idActual = hfIDUsuario.Value;

            txtNombre.Text = LimpiarTexto(txtNombre.Text);
            txtApellido.Text = LimpiarTexto(txtApellido.Text);

            // Validaciones Anti-Quiebre
            if (ContarPalabras(txtNombre.Text) > 3) { MostrarError(errNombre, "Demasiadas palabras. Solo nombres."); esValido = false; }
            if (ContarPalabras(txtApellido.Text) > 3) { MostrarError(errApellido, "Demasiadas palabras."); esValido = false; }

            if (string.IsNullOrWhiteSpace(txtNombre.Text) || !Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]+$"))
            {
                MostrarError(errNombre, "Solo letras."); esValido = false;
            }
            if (string.IsNullOrWhiteSpace(txtApellido.Text) || !Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]+$"))
            {
                MostrarError(errApellido, "Solo letras."); esValido = false;
            }

            if (esValido && ExistePersona(txtNombre.Text, txtApellido.Text, idActual))
            {
                MostrarMensaje("Usuario ya existe con ese nombre.", "warning"); esValido = false;
            }

            if (!Regex.IsMatch(txtCorreo.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { MostrarError(errCorreo, "Invalido."); esValido = false; }
            else if (ExisteDatoEnBD("Correo", txtCorreo.Text.Trim(), idActual)) { MostrarError(errCorreo, "Repetido."); esValido = false; }

            if (!Regex.IsMatch(txtTelefono.Text.Trim(), @"^[2578]\d{7}$")) { MostrarError(errTelefono, "Inválido."); esValido = false; }
            else if (ExisteDatoEnBD("Telefono", "505-" + txtTelefono.Text.Trim(), idActual)) { MostrarError(errTelefono, "Repetido."); esValido = false; }

            if (txtUsuario.Text.Length < 4 || !Regex.IsMatch(txtUsuario.Text, @"^[a-zA-Z0-9]+$")) { MostrarError(errUsuario, "Mínimo 4 chars."); esValido = false; }
            else if (ExisteDatoEnBD("Usuario", txtUsuario.Text.Trim(), idActual)) { MostrarError(errUsuario, "Ocupado."); esValido = false; }

            if (ddlRol.SelectedIndex == 0) { MostrarError(errRol, "Requerido."); esValido = false; }

            if (string.IsNullOrEmpty(idActual) && string.IsNullOrEmpty(txtClave.Text)) { MostrarError(errClave, "Requerida."); esValido = false; }
            if (!string.IsNullOrEmpty(txtClave.Text))
            {
                if (txtClave.Text.Length < 6) { MostrarError(errClave, "Mínimo 6."); esValido = false; }
                if (txtClave.Text != txtClaveConfirm.Text) { MostrarError(errClaveConfirm, "No coinciden."); esValido = false; }
            }

            return esValido;
        }

        private bool ExisteDatoEnBD(string campo, string valor, string idExcluir)
        {
            string query = "";
            if (campo == "Correo") query = "SELECT COUNT(*) FROM Usuario WHERE Correo = @Val AND ID_Usuario != @ID";
            if (campo == "Telefono") query = "SELECT COUNT(*) FROM Usuario WHERE Telefono = @Val AND ID_Usuario != @ID";
            if (campo == "Usuario") query = "SELECT COUNT(*) FROM InicioSesion WHERE NombreUsuario = @Val AND ID_Usuario != @ID";

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Val", valor);
                cmd.Parameters.AddWithValue("@ID", string.IsNullOrEmpty(idExcluir) ? "0" : idExcluir);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool ExistePersona(string nom, string ape, string idExcluir)
        {
            string query = "SELECT COUNT(*) FROM Usuario WHERE Nombres = @n AND Apellidos = @a AND ID_Usuario != @ID";
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@n", nom);
                cmd.Parameters.AddWithValue("@a", ape);
                cmd.Parameters.AddWithValue("@ID", string.IsNullOrEmpty(idExcluir) ? "0" : idExcluir);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlTransaction trans = con.BeginTransaction();
                try
                {
                    string telefonoFull = "505-" + txtTelefono.Text.Trim();
                    MySqlCommand cmd;
                    long id = 0;

                    if (string.IsNullOrEmpty(hfIDUsuario.Value))
                    {
                        string sqlU = "INSERT INTO Usuario (Nombres, Apellidos, Correo, Telefono, Rol, FechaRegistro, Estado) VALUES (@n,@a,@c,@t,@r,NOW(),1); SELECT LAST_INSERT_ID();";
                        cmd = new MySqlCommand(sqlU, con, trans);
                        cmd.Parameters.AddWithValue("@n", txtNombre.Text);
                        cmd.Parameters.AddWithValue("@a", txtApellido.Text);
                        cmd.Parameters.AddWithValue("@c", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", telefonoFull);
                        cmd.Parameters.AddWithValue("@r", ddlRol.SelectedValue);
                        id = Convert.ToInt64(cmd.ExecuteScalar());

                        string sqlL = "INSERT INTO InicioSesion (ID_Usuario, NombreUsuario, Clave, Estado) VALUES (@id, @u, @p, 1)";
                        cmd = new MySqlCommand(sqlL, con, trans);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@u", txtUsuario.Text.Trim().ToLower());
                        cmd.Parameters.AddWithValue("@p", ComputarHashSHA256(txtClave.Text));
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Registrado.", "success");
                    }
                    else
                    {
                        id = long.Parse(hfIDUsuario.Value);
                        string sqlU = "UPDATE Usuario SET Nombres=@n, Apellidos=@a, Correo=@c, Telefono=@t, Rol=@r WHERE ID_Usuario=@id";
                        cmd = new MySqlCommand(sqlU, con, trans);
                        cmd.Parameters.AddWithValue("@n", txtNombre.Text);
                        cmd.Parameters.AddWithValue("@a", txtApellido.Text);
                        cmd.Parameters.AddWithValue("@c", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@t", telefonoFull);
                        cmd.Parameters.AddWithValue("@r", ddlRol.SelectedValue);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();

                        string sqlL = "UPDATE InicioSesion SET NombreUsuario=@u" + (!string.IsNullOrEmpty(txtClave.Text) ? ", Clave=@p" : "") + " WHERE ID_Usuario=@id";
                        cmd = new MySqlCommand(sqlL, con, trans);
                        cmd.Parameters.AddWithValue("@u", txtUsuario.Text.Trim().ToLower());
                        cmd.Parameters.AddWithValue("@id", id);
                        if (!string.IsNullOrEmpty(txtClave.Text)) cmd.Parameters.AddWithValue("@p", ComputarHashSHA256(txtClave.Text));
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Actualizado.", "success");
                    }
                    trans.Commit();
                    CargarDatos();
                    PanelMantenimiento.Visible = false;
                    PanelListado.Visible = true;
                }
                catch (Exception ex) { trans.Rollback(); MostrarMensaje("Error: " + ex.Message, "error"); }
            }
        }

        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Editar") CargarEdicion(id);
            else if (e.CommandName == "DarBaja") CambiarEstado(id, 0);
            else if (e.CommandName == "Reactivar") CambiarEstado(id, 1);
        }

        private void CargarEdicion(int id)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                string sql = "SELECT u.*, i.NombreUsuario FROM Usuario u LEFT JOIN InicioSesion i ON u.ID_Usuario=i.ID_Usuario WHERE u.ID_Usuario=@id";
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfIDUsuario.Value = id.ToString();
                    txtNombre.Text = r["Nombres"].ToString();
                    txtApellido.Text = r["Apellidos"].ToString();
                    txtCorreo.Text = r["Correo"].ToString();
                    txtTelefono.Text = r["Telefono"].ToString().Replace("505-", "");
                    ddlRol.SelectedValue = r["Rol"].ToString();
                    txtUsuario.Text = r["NombreUsuario"].ToString();
                    txtClave.Text = ""; txtClave.Attributes["value"] = "";
                    txtClaveConfirm.Text = ""; txtClaveConfirm.Attributes["value"] = "";
                    lblTituloAccion.Text = "Editar Usuario";
                    PanelListado.Visible = false; PanelMantenimiento.Visible = true;
                    LimpiarErrores();
                }
            }
        }

        private void CambiarEstado(int id, int estado)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                new MySqlCommand($"UPDATE Usuario SET Estado={estado} WHERE ID_Usuario={id}", con).ExecuteNonQuery();
                new MySqlCommand($"UPDATE InicioSesion SET Estado={estado} WHERE ID_Usuario={id}", con).ExecuteNonQuery();
            }
            CargarDatos();
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            hfIDUsuario.Value = "";
            txtNombre.Text = ""; txtApellido.Text = ""; txtCorreo.Text = ""; txtTelefono.Text = ""; txtUsuario.Text = "";
            txtClave.Text = ""; txtClave.Attributes["value"] = "";
            txtClaveConfirm.Text = ""; txtClaveConfirm.Attributes["value"] = "";
            ddlRol.SelectedIndex = 0;
            PanelListado.Visible = false; PanelMantenimiento.Visible = true;
            lblTituloAccion.Text = "Nuevo Usuario";
            LimpiarErrores();
        }

        protected void btnCancelar_Click(object sender, EventArgs e) { PanelMantenimiento.Visible = false; PanelListado.Visible = true; }
        protected void btnBuscar_Click(object sender, EventArgs e) { CargarDatos(); }
        protected void gvUsuarios_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvUsuarios.PageIndex = e.NewPageIndex; CargarDatos(); }

        private string ComputarHashSHA256(string rawData)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] b = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder s = new StringBuilder();
                foreach (byte x in b) s.Append(x.ToString("x2"));
                return s.ToString();
            }
        }

        private void MostrarError(Label l, string m) { l.Text = "<i class='fa-solid fa-circle-exclamation'></i> " + m; l.Visible = true; }
        private void MostrarMensaje(string m, string i) { ScriptManager.RegisterStartupScript(this, GetType(), "Msj", $"Swal.fire('Sistema','{m}','{i}');", true); }
        private void LimpiarErrores() { errNombre.Visible = false; errApellido.Visible = false; errCorreo.Visible = false; errTelefono.Visible = false; errUsuario.Visible = false; errClave.Visible = false; errClaveConfirm.Visible = false; errRol.Visible = false; }
    }
}