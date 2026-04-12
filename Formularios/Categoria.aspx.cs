using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Categoria : System.Web.UI.Page
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
                    string query = "SELECT * FROM Categoria WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                        query += " AND Descripcion LIKE @Busq ";

                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND Estado = @Estado ";

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                        query += " AND FechaRegistro BETWEEN @FecIni AND @FecFin ";

                    query += " ORDER BY ID_Categoria DESC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaInicio.Text) ? "1900-01-01" : txtFiltroFechaInicio.Text).ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@FecFin", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaFin.Text) ? "2100-01-01" : txtFiltroFechaFin.Text).ToString("yyyy-MM-dd"));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvCategorias.DataSource = dt;
                    gvCategorias.DataBind();

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
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        private bool ValidarFormulario()
        {
            bool esValido = true;
            LimpiarErrores();

            string desc = txtDescripcion.Text.Trim();

            if (string.IsNullOrEmpty(desc))
            {
                MostrarError(errDescripcion, "Requerido.");
                MarcarError(txtDescripcion);
                esValido = false;
            }
            else if (!Regex.IsMatch(desc, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+)*$"))
            {
                MostrarError(errDescripcion, "Solo letras y un espacio entre palabras.");
                MarcarError(txtDescripcion);
                esValido = false;
            }
            else if (TieneCaracteresRepetidos(desc))
            {
                MostrarError(errDescripcion, "Demasiados caracteres repetidos.");
                MarcarError(txtDescripcion);
                esValido = false;
            }
            else if (ExisteCategoria(desc, hfIDCategoria.Value))
            {
                MostrarError(errDescripcion, "Esta categoría ya existe.");
                MarcarError(txtDescripcion);
                esValido = false;
            }

            return esValido;
        }

        private bool TieneCaracteresRepetidos(string texto)
        {
            return Regex.IsMatch(texto, @"(.)\1\1");
        }

        private bool ExisteCategoria(string descripcion, string idExcluir)
        {
            int count = 0;
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Categoria WHERE Descripcion = @Desc AND ID_Categoria != @ID";
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
            lblTituloAccion.Text = "Registrar Nueva Categoría";
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
            if (!ValidarFormulario()) return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd;

                    if (string.IsNullOrEmpty(hfIDCategoria.Value))
                    {
                        string sql = "INSERT INTO Categoria (Descripcion, FechaRegistro, Estado) VALUES (@Desc, CURRENT_DATE, 1)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Desc", txtDescripcion.Text.Trim());
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Categoría registrada correctamente.", "success");
                    }
                    else
                    {
                        string sql = "UPDATE Categoria SET Descripcion=@Desc WHERE ID_Categoria=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Desc", txtDescripcion.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID", hfIDCategoria.Value);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Categoría actualizada correctamente.", "success");
                    }

                    PanelListado.Visible = true;
                    PanelMantenimiento.Visible = false;
                    LimpiarErrores();
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al guardar: " + ex.Message, "error"); }
        }

        protected void gvCategorias_RowCommand(object sender, GridViewCommandEventArgs e)
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
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Categoria WHERE ID_Categoria=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        hfIDCategoria.Value = id.ToString();
                        txtDescripcion.Text = r["Descripcion"].ToString();
                        if (r["FechaRegistro"] != DBNull.Value)
                            txtFechaRegistro.Text = Convert.ToDateTime(r["FechaRegistro"]).ToString("yyyy-MM-dd");

                        txtFechaRegistro.Enabled = true;
                        PanelListado.Visible = false;
                        PanelMantenimiento.Visible = true;
                        lblTituloAccion.Text = "Editar Categoría";
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
                    MySqlCommand cmd = new MySqlCommand("UPDATE Categoria SET Estado=@Est WHERE ID_Categoria=@ID", con);
                    cmd.Parameters.AddWithValue("@Est", estado);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    string accion = estado == 1 ? "reactivada" : "inactivada";
                    MostrarMensaje($"Categoría {accion} correctamente.", "success");
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
            ddlPageSize.SelectedValue = "All";
            gvCategorias.AllowPaging = false;
            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvCategorias.AllowPaging = false;
            else { gvCategorias.AllowPaging = true; gvCategorias.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void gvCategorias_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCategorias.PageIndex = e.NewPageIndex;
            CargarDatos();
        }

        private void LimpiarFormulario()
        {
            hfIDCategoria.Value = "";
            txtDescripcion.Text = ""; txtFechaRegistro.Text = "";
            LimpiarErrores();
            QuitarMarcas();
        }

        private void LimpiarErrores()
        {
            if (errDescripcion != null) errDescripcion.Visible = false;
            QuitarMarcas();
        }

        private void MostrarError(Label l, string m)
        {
            l.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {m}";
            l.Visible = true;
        }

        private void MarcarError(TextBox txt)
        {
            txt.CssClass += " is-invalid";
        }

        private void QuitarMarcas()
        {
            txtDescripcion.CssClass = txtDescripcion.CssClass.Replace(" is-invalid", "");
        }

        private void MostrarMensaje(string t, string i)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Toast", $"Swal.fire('Categorías', '{t}', '{i}');", true);
        }
    }
}