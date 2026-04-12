using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class TipoPago : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFiltroFechaInicio.Text = new DateTime(2023, 7, 1).ToString("yyyy-MM-dd");
                txtFiltroFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarDatos();
            }
        }

        private void CargarDatos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = "SELECT * FROM TipoPago WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                        query += " AND Descripcion LIKE @Busq ";

                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND Estado = @Estado ";

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                        query += " AND FechaRegistro BETWEEN @FecIni AND @FecFin ";

                    query += " ORDER BY ID_TipoPago DESC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaInicio.Text) ? "1900-01-01" : txtFiltroFechaInicio.Text).ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@FecFin", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaFin.Text) ? "2100-01-01" : txtFiltroFechaFin.Text).ToString("yyyy-MM-dd"));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvTipoPago.DataSource = dt;
                    gvTipoPago.DataBind();

                    // Lógica del mensaje de registros
                    pnlMensajeGrid.Visible = false;
                    if (ddlPageSize.SelectedValue != "All")
                    {
                        int requestedSize = int.Parse(ddlPageSize.SelectedValue);
                        if (dt.Rows.Count > 0 && dt.Rows.Count < requestedSize)
                        {
                            lblMensajeGrid.Text = $"Nota: Solicitó ver {requestedSize} registros, pero solo existen {dt.Rows.Count} registros disponibles con los filtros actuales.";
                            pnlMensajeGrid.Visible = true;
                        }
                    }
                    if (dt.Rows.Count == 0)
                    {
                        lblMensajeGrid.Text = "No hay registros disponibles con los filtros seleccionados.";
                        pnlMensajeGrid.Visible = true;
                        if (FindControl("divSinDatos") != null) FindControl("divSinDatos").Visible = true;
                    }
                    else if (FindControl("divSinDatos") != null)
                    {
                        FindControl("divSinDatos").Visible = false;
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        private bool ValidarEntradas()
        {
            bool esValido = true;
            LimpiarErrores();

            string desc = txtDescripcion.Text.Trim();

            // Validación estricta: Solo letras, un solo espacio entre palabras, sin números/símbolos
            if (string.IsNullOrEmpty(desc))
            {
                MostrarError(errDescripcion, "Requerido.");
                esValido = false;
            }
            else if (!Regex.IsMatch(desc, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚ]+)*$"))
            {
                MostrarError(errDescripcion, "Solo letras y un espacio entre palabras. Sin números ni símbolos.");
                esValido = false;
            }
            else if (ExisteDescripcion(desc, hfIDTipoPago.Value))
            {
                MostrarError(errDescripcion, "Este tipo de pago ya existe.");
                esValido = false;
            }

            // Validar Fecha solo en edición si viniera de input (aquí es readonly, pero validamos integridad)
            if (!string.IsNullOrEmpty(hfIDTipoPago.Value))
            {
                if (DateTime.TryParse(txtFechaRegistro.Text, out DateTime fr))
                {
                    if (fr < new DateTime(2023, 6, 1) || fr > DateTime.Now)
                    {
                        esValido = false; // Error interno de fecha
                    }
                }
            }

            return esValido;
        }

        private bool ExisteDescripcion(string descripcion, string idExcluir)
        {
            int count = 0;
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM TipoPago WHERE Descripcion = @Desc AND ID_TipoPago != @ID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Desc", descripcion);
                    cmd.Parameters.AddWithValue("@ID", string.IsNullOrEmpty(idExcluir) ? "0" : idExcluir);
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return false; }
            return count > 0;
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            PanelListado.Visible = false;
            PanelMantenimiento.Visible = true;
            lblTituloAccion.Text = "Registrar Nuevo Tipo de Pago";
            txtFechaRegistro.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            PanelListado.Visible = true;
            PanelMantenimiento.Visible = false;
            LimpiarErrores();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarEntradas()) return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd;

                    if (string.IsNullOrEmpty(hfIDTipoPago.Value))
                    {
                        string sql = "INSERT INTO TipoPago (Descripcion, FechaRegistro, Estado) VALUES (@Desc, CURRENT_DATE, 1)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Desc", txtDescripcion.Text.Trim());
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Tipo de pago registrado correctamente.", "success");
                    }
                    else
                    {
                        string sql = "UPDATE TipoPago SET Descripcion=@Desc WHERE ID_TipoPago=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Desc", txtDescripcion.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID", hfIDTipoPago.Value);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Tipo de pago actualizado correctamente.", "success");
                    }

                    PanelListado.Visible = true;
                    PanelMantenimiento.Visible = false;
                    LimpiarErrores();
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al guardar: " + ex.Message, "error"); }
        }

        protected void gvTipoPago_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Editar") CargarParaEditar(id);
            else if (e.CommandName == "DarBaja") CambiarEstado(id, 0);
            else if (e.CommandName == "Reactivar") CambiarEstado(id, 1);
        }

        private void CargarParaEditar(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM TipoPago WHERE ID_TipoPago=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        hfIDTipoPago.Value = id.ToString();
                        txtDescripcion.Text = r["Descripcion"].ToString();
                        if (r["FechaRegistro"] != DBNull.Value)
                            txtFechaRegistro.Text = Convert.ToDateTime(r["FechaRegistro"]).ToString("yyyy-MM-dd");

                        txtFechaRegistro.Enabled = true;
                        PanelListado.Visible = false;
                        PanelMantenimiento.Visible = true;
                        lblTituloAccion.Text = "Editar Tipo de Pago";
                        LimpiarErrores();
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        private void CambiarEstado(int id, int estado)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE TipoPago SET Estado=@Est WHERE ID_TipoPago=@ID", con);
                    cmd.Parameters.AddWithValue("@Est", estado);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    string accion = estado == 1 ? "reactivado" : "inactivado";
                    MostrarMensaje($"Tipo de pago {accion} correctamente.", "success");
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        protected void btnBuscar_Click(object sender, EventArgs e) => CargarDatos();

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            txtFiltroFechaInicio.Text = "";
            txtFiltroFechaFin.Text = "";

            // Resetear Paginación
            ddlPageSize.SelectedValue = "All";
            gvTipoPago.AllowPaging = false;

            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvTipoPago.AllowPaging = false;
            else { gvTipoPago.AllowPaging = true; gvTipoPago.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void gvTipoPago_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTipoPago.PageIndex = e.NewPageIndex;
            CargarDatos();
        }

        private void LimpiarFormulario()
        {
            hfIDTipoPago.Value = "";
            txtDescripcion.Text = ""; txtFechaRegistro.Text = "";
            LimpiarErrores();
        }

        private void LimpiarErrores()
        {
            if (errDescripcion != null) errDescripcion.Visible = false;
        }

        private void MostrarError(Label l, string m)
        {
            l.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {m}";
            l.Visible = true;
        }

        private void MostrarMensaje(string t, string i)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Toast", $"Swal.fire('Sistema', '{t}', '{i}');", true);
        }
    }
}