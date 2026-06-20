using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Tratamientos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarDatos();
            }
        }

        // ==========================================
        // 1. CARGA DE DATOS
        // ==========================================
        private void CargarDatos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = "SELECT * FROM tratamiento WHERE 1=1";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                        query += " AND Nombre LIKE @Buscar";

                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND Estado = @Estado";

                    query += " ORDER BY Nombre ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Buscar", "%" + txtBuscar.Text.Trim() + "%");
                        cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);

                        DataTable dt = new DataTable();
                        new MySqlDataAdapter(cmd).Fill(dt);
                        gvTratamientos.DataSource = dt;
                        gvTratamientos.DataBind();

                        pnlMensajeGrid.Visible = false;
                        if (dt.Rows.Count == 0)
                        {
                            lblMensajeGrid.Text = "No hay tratamientos disponibles con los filtros seleccionados.";
                            pnlMensajeGrid.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al cargar: " + ex.Message, "error"); }
        }

        // ==========================================
        // 2. VALIDACIONES ESTRICTAS
        // ==========================================
        private bool ValidarFormulario()
        {
            bool esValido = true;
            LimpiarErrores();

            string nombre = txtNombre.Text.Trim();
            string precioStr = txtPrecio.Text.Trim();

            // --- 1. NOMBRE (Max 50, Min 2 caracteres por palabra, Alfanumérico) ---
            // Regex permite letras y números (ej: "AntiReflejo", "UV 400", "Blue Light")
            // Pero bloquea letras sueltas como "U V 4 0 0"
            string regexNombre = @"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ]{2,}(?: [a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ]{2,})*$";

            if (string.IsNullOrEmpty(nombre))
            {
                MostrarError(errNombre, "El nombre es obligatorio."); esValido = false;
            }
            else if (nombre.Length > 50)
            {
                MostrarError(errNombre, "Máximo 50 caracteres."); esValido = false;
            }
            else if (!Regex.IsMatch(nombre, regexNombre))
            {
                MostrarError(errNombre, "Formato inválido. Mínimo 2 caracteres por palabra. Sin dobles espacios."); esValido = false;
            }
            else if (ExisteDuplicado(nombre, hfIDTratamiento.Value))
            {
                MostrarError(errNombre, "Ya existe un tratamiento con este nombre."); esValido = false;
            }

            // --- 2. PRECIO ---
            if (string.IsNullOrEmpty(precioStr))
            {
                MostrarError(errPrecio, "El precio es obligatorio."); esValido = false;
            }
            else
            {
                if (decimal.TryParse(precioStr, out decimal precio))
                {
                    if (precio < 0)
                    {
                        MostrarError(errPrecio, "El precio no puede ser negativo."); esValido = false;
                    }
                }
                else
                {
                    MostrarError(errPrecio, "Ingrese un número válido."); esValido = false;
                }
            }

            return esValido;
        }

        private bool ExisteDuplicado(string nombre, string idExcluir)
        {
            string query = "SELECT COUNT(*) FROM tratamiento WHERE Nombre = @Nombre AND ID_Tratamiento != @ID";
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@ID", string.IsNullOrEmpty(idExcluir) ? "0" : idExcluir);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ==========================================
        // 3. GUARDAR (INSERT / UPDATE)
        // ==========================================
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = "";
                    bool esNuevo = string.IsNullOrEmpty(hfIDTratamiento.Value);
                    string nombreLimpio = txtNombre.Text.Trim();
                    decimal precio = Convert.ToDecimal(txtPrecio.Text.Trim());

                    if (esNuevo)
                    {
                        query = "INSERT INTO tratamiento (Nombre, PrecioAdicional, FechaRegistro, Estado) VALUES (@Nombre, @Precio, CURDATE(), 1)";
                    }
                    else
                    {
                        query = "UPDATE tratamiento SET Nombre=@Nombre, PrecioAdicional=@Precio WHERE ID_Tratamiento=@ID";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", nombreLimpio);
                        cmd.Parameters.AddWithValue("@Precio", precio);

                        if (!esNuevo)
                        {
                            cmd.Parameters.AddWithValue("@ID", hfIDTratamiento.Value);
                        }

                        cmd.ExecuteNonQuery();

                        string accion = esNuevo ? "registrado" : "actualizado";
                        MostrarMensaje($"Tratamiento {accion} correctamente.", "success");

                        PanelMantenimiento.Visible = false;
                        PanelListado.Visible = true;
                        CargarDatos();
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error BD: " + ex.Message, "error"); }
        }

        // ==========================================
        // 4. EVENTOS DE INTERFAZ
        // ==========================================
        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            PanelListado.Visible = false;
            PanelMantenimiento.Visible = true;
            hfIDTratamiento.Value = "";
            LimpiarFormulario();
            lblTituloAccion.Text = "Registrar Nuevo Tratamiento";
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            PanelListado.Visible = true;
            PanelMantenimiento.Visible = false;
            LimpiarErrores();
        }

        protected void btnBuscar_Click(object sender, EventArgs e) => CargarDatos();

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            CargarDatos();
        }

        protected void gvTratamientos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTratamientos.PageIndex = e.NewPageIndex;
            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvTratamientos.AllowPaging = false;
            else { gvTratamientos.AllowPaging = true; gvTratamientos.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void gvTratamientos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Editar")
            {
                CargarDatosEdicion(id);
            }
            else if (e.CommandName == "Desactivar")
            {
                CambiarEstado(id, 0);
            }
            else if (e.CommandName == "Reactivar")
            {
                CambiarEstado(id, 1);
            }
        }

        // ==========================================
        // 5. FUNCIONES AUXILIARES
        // ==========================================
        private void CargarDatosEdicion(int id)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                string query = "SELECT * FROM tratamiento WHERE ID_Tratamiento = @ID";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            hfIDTratamiento.Value = id.ToString();
                            txtNombre.Text = r["Nombre"].ToString();
                            // Formato seguro para decimales en input HTML
                            txtPrecio.Text = Convert.ToDecimal(r["PrecioAdicional"]).ToString("0.00").Replace(",", ".");

                            lblTituloAccion.Text = "Editar Tratamiento";
                            PanelListado.Visible = false;
                            PanelMantenimiento.Visible = true;
                            LimpiarErrores();
                        }
                    }
                }
            }
        }

        private void CambiarEstado(int id, int estado)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                new MySqlCommand($"UPDATE tratamiento SET Estado={estado} WHERE ID_Tratamiento={id}", con).ExecuteNonQuery();
            }
            CargarDatos();
            string accion = estado == 1 ? "reactivado" : "desactivado";
            MostrarMensaje($"Tratamiento {accion} correctamente.", "success");
        }

        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtPrecio.Text = "";
            LimpiarErrores();
        }

        private void LimpiarErrores()
        {
            errNombre.Visible = false;
            errPrecio.Visible = false;
        }

        private void MostrarError(Label lbl, string msg)
        {
            lbl.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {msg}";
            lbl.Visible = true;
        }

        private void MostrarMensaje(string m, string t)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Toast", $"Swal.fire('Sistema', '{m}', '{t}');", true);
        }
    }
}