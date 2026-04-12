using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class TipoDocumento : System.Web.UI.Page
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
                    string query = "SELECT * FROM TipoDocumento WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                        query += " AND Descripcion LIKE @Busq ";

                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND Estado = @Estado ";

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                        query += " AND FechaRegistro BETWEEN @FecIni AND @FecFin ";

                    query += " ORDER BY ID_TipoDocumento DESC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaInicio.Text) ? "1900-01-01" : txtFiltroFechaInicio.Text).ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@FecFin", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaFin.Text) ? "2100-01-01" : txtFiltroFechaFin.Text).ToString("yyyy-MM-dd"));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvTipoDocumento.DataSource = dt;
                    gvTipoDocumento.DataBind();

                    // Mensaje de Registros
                    pnlMensajeGrid.Visible = false;
                    if (ddlPageSize.SelectedValue != "All")
                    {
                        int requestedSize = int.Parse(ddlPageSize.SelectedValue);
                        if (dt.Rows.Count > 0 && dt.Rows.Count < requestedSize)
                        {
                            lblMensajeGrid.Text = $"Nota: Solicitó ver {requestedSize} registros, pero solo existen {dt.Rows.Count} disponibles.";
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
            bool v = true;
            LimpiarErrores();

            string desc = txtDescripcion.Text.Trim();

            // Validación estricta: Solo letras, un solo espacio entre palabras, sin números/símbolos
            if (string.IsNullOrEmpty(desc))
            {
                MostrarError(errDescripcion, "Requerido.");
                v = false;
            }
            else if (!Regex.IsMatch(desc, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚ]+)*$"))
            {
                MostrarError(errDescripcion, "Solo letras y un espacio entre palabras. Sin números ni símbolos.");
                v = false;
            }
            else if (ExisteDescripcion(desc, hfIDTipoDocumento.Value))
            {
                MostrarError(errDescripcion, "Este tipo de documento ya existe.");
                v = false;
            }

            return v;
        }

        private bool ExisteDescripcion(string descripcion, string idExcluir)
        {
            int count = 0;
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM TipoDocumento WHERE Descripcion = @Desc AND ID_TipoDocumento != @ID";
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
            lblTituloAccion.Text = "Nuevo Tipo de Documento";
            txtFechaRegistro.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtFechaRegistro.Enabled = false;
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

                    if (string.IsNullOrEmpty(hfIDTipoDocumento.Value))
                    {
                        string sql = "INSERT INTO TipoDocumento (Descripcion, FechaRegistro, Estado) VALUES (@Desc, CURRENT_DATE, 1)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Desc", txtDescripcion.Text.Trim());
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Tipo de documento registrado.", "success");
                    }
                    else
                    {
                        string sql = "UPDATE TipoDocumento SET Descripcion=@Desc, FechaRegistro=@Fec WHERE ID_TipoDocumento=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Desc", txtDescripcion.Text.Trim());
                        cmd.Parameters.AddWithValue("@Fec", ConvertirFechaMySQL(txtFechaRegistro.Text));
                        cmd.Parameters.AddWithValue("@ID", hfIDTipoDocumento.Value);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Tipo de documento actualizado.", "success");
                    }
                    btnCancelar_Click(sender, e);
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al guardar: " + ex.Message, "error"); }
        }

        protected void gvTipoDocumento_RowCommand(object sender, GridViewCommandEventArgs e)
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
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM TipoDocumento WHERE ID_TipoDocumento=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        hfIDTipoDocumento.Value = id.ToString();
                        txtDescripcion.Text = r["Descripcion"].ToString();
                        if (r["FechaRegistro"] != DBNull.Value)
                            txtFechaRegistro.Text = Convert.ToDateTime(r["FechaRegistro"]).ToString("yyyy-MM-dd");

                        txtFechaRegistro.Enabled = true;
                        PanelListado.Visible = false;
                        PanelMantenimiento.Visible = true;
                        lblTituloAccion.Text = "Editar Tipo Documento";
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
                    MySqlCommand cmd = new MySqlCommand("UPDATE TipoDocumento SET Estado=@Est WHERE ID_TipoDocumento=@ID", con);
                    cmd.Parameters.AddWithValue("@Est", estado);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Estado actualizado.", "success");
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            txtFiltroFechaInicio.Text = "";
            txtFiltroFechaFin.Text = "";

            ddlPageSize.SelectedValue = "All";
            gvTipoDocumento.AllowPaging = false;

            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvTipoDocumento.AllowPaging = false;
            else { gvTipoDocumento.AllowPaging = true; gvTipoDocumento.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void gvTipoDocumento_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTipoDocumento.PageIndex = e.NewPageIndex;
            CargarDatos();
        }

        protected void btnBuscar_Click(object sender, EventArgs e) => CargarDatos();

        private void LimpiarFormulario()
        {
            hfIDTipoDocumento.Value = "";
            txtDescripcion.Text = ""; txtFechaRegistro.Text = "";
            LimpiarErrores();
        }

        private void LimpiarErrores()
        {
            if (errDescripcion != null) errDescripcion.Visible = false;
        }

        private void MostrarError(Label l, string m) { if (l != null) { l.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {m}"; l.Visible = true; } }
        private string ConvertirFechaMySQL(string f) { if (DateTime.TryParse(f, out DateTime d)) return d.ToString("yyyy-MM-dd"); return f; }
        private void MostrarMensaje(string t, string i) { ScriptManager.RegisterStartupScript(this, GetType(), "T", $"Swal.fire('Sistema', '{t}', '{i}');", true); }
    }
}