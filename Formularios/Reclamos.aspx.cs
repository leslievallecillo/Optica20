using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Reclamos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFiltroFechaInicio.Text = "";
                txtFiltroFechaFin.Text = "";
                CargarReclamos();
            }
        }

        private void CargarReclamos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = @"SELECT r.ID_Reclamo, r.FechaReclamo, r.Motivo, r.EstadoReclamo, r.Responsable, 
                                          r.FechaSolucion, r.SolucionAplicada, g.ID_Garantia,
                                          CONCAT('Garantía #', g.ID_Garantia, ' - Venta: ', v.NumeroDocumento) as RefGarantia
                                   FROM reclamogarantia r
                                   INNER JOIN garantia g ON r.ID_Garantia = g.ID_Garantia
                                   INNER JOIN detalleventalentes dl ON g.ID_DetalleVentaLente = dl.ID_DetalleVentaLentes
                                   INNER JOIN venta v ON dl.ID_Venta = v.ID_Venta
                                   WHERE r.Estado = 1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                    {
                        sql += @" AND (r.Motivo LIKE @Busq OR r.Responsable LIKE @Busq OR v.NumeroDocumento LIKE @Busq) ";
                    }

                    if (ddlFiltroEstado.SelectedValue != "-1")
                    {
                        sql += " AND r.EstadoReclamo = @Est ";
                    }

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                    {
                        sql += " AND r.FechaReclamo BETWEEN @FecIni AND @FecFin ";
                    }

                    sql += " ORDER BY r.FechaReclamo DESC, r.ID_Reclamo DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Est", ddlFiltroEstado.SelectedValue);

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                    {
                        cmd.Parameters.AddWithValue("@FecIni", ConvertirFechaMySQL(txtFiltroFechaInicio.Text) + " 00:00:00");
                        cmd.Parameters.AddWithValue("@FecFin", ConvertirFechaMySQL(txtFiltroFechaFin.Text) + " 23:59:59");
                    }

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvReclamos.DataSource = dt;
                    gvReclamos.DataBind();

                    pnlMensajeGrid.Visible = false;
                    if (ddlPageSize.SelectedValue != "All")
                    {
                        int pageSize = int.Parse(ddlPageSize.SelectedValue);
                        if (dt.Rows.Count > 0 && dt.Rows.Count < pageSize)
                        {
                            lblMensajeGrid.Text = $"Nota: Solicitó ver {pageSize} registros, pero solo existen {dt.Rows.Count} disponibles.";
                            pnlMensajeGrid.Visible = true;
                        }
                    }
                    if (dt.Rows.Count == 0)
                    {
                        lblMensajeGrid.Text = "No hay reclamos registrados con estos filtros.";
                        pnlMensajeGrid.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar reclamos: " + ex.Message, "error");
            }
        }

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            txtFiltroFechaInicio.Text = "";
            txtFiltroFechaFin.Text = "";
            ddlPageSize.SelectedValue = "All";
            gvReclamos.AllowPaging = false;
            CargarReclamos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvReclamos.AllowPaging = false;
            else { gvReclamos.AllowPaging = true; gvReclamos.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarReclamos();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarReclamos();
        }

        protected void gvReclamos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReclamos.PageIndex = e.NewPageIndex;
            CargarReclamos();
        }

        protected void gvReclamos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ResolverR")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                CargarDatosReclamo(id);
                MostrarPanel("MantReclamo");
            }
        }

        private void CargarDatosReclamo(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = @"SELECT r.*, CONCAT('Garantía #', g.ID_Garantia, ' - Doc: ', v.NumeroDocumento) as RefGarantia
                                   FROM reclamogarantia r
                                   INNER JOIN garantia g ON r.ID_Garantia = g.ID_Garantia
                                   INNER JOIN detalleventalentes dl ON g.ID_DetalleVentaLente = dl.ID_DetalleVentaLentes
                                   INNER JOIN venta v ON dl.ID_Venta = v.ID_Venta
                                   WHERE r.ID_Reclamo=@ID";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();

                    if (r.Read())
                    {
                        hfIDReclamo.Value = id.ToString();
                        lblGarantiaRef.Text = r["RefGarantia"].ToString();
                        lblFechaReclamo.Text = r["FechaReclamo"] != DBNull.Value ? Convert.ToDateTime(r["FechaReclamo"]).ToString("dd/MM/yyyy") : "N/A";
                        lblResponsable.Text = r["Responsable"].ToString();
                        lblMotivo.Text = r["Motivo"].ToString();

                        txtSolucion.Text = r["SolucionAplicada"].ToString();
                        if (ddlEstadoReclamo.Items.FindByValue(r["EstadoReclamo"].ToString()) != null)
                        {
                            ddlEstadoReclamo.SelectedValue = r["EstadoReclamo"].ToString();
                        }

                        if (r["FechaSolucion"] != DBNull.Value)
                        {
                            txtFechaSolucion.Text = Convert.ToDateTime(r["FechaSolucion"]).ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            txtFechaSolucion.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar datos del reclamo: " + ex.Message, "error");
            }
        }

        private bool ValidarResolucion()
        {
            bool valido = true;
            LimpiarErrores();
            string rgx = @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚ]+)*$";

            if (ddlEstadoReclamo.SelectedValue == "Resuelto")
            {
                if (string.IsNullOrWhiteSpace(txtSolucion.Text) || !Regex.IsMatch(txtSolucion.Text, rgx))
                {
                    MostrarError(errSolucion, "Requerido (Solo letras y un espacio entre palabras).");
                    valido = false;
                }

                if (DateTime.TryParse(txtFechaSolucion.Text, out DateTime fs))
                {
                    DateTime fr = DateTime.ParseExact(lblFechaReclamo.Text, "dd/MM/yyyy", null);
                    if (fs > fr.AddDays(7))
                    {
                        MostrarError(errFechaSolucion, "Máximo 1 semana de plazo desde el reclamo.");
                        valido = false;
                    }
                    if (fs < fr)
                    {
                        MostrarError(errFechaSolucion, "No puede ser anterior a la fecha del reclamo.");
                        valido = false;
                    }
                }
                else
                {
                    MostrarError(errFechaSolucion, "Fecha requerida para reclamos resueltos.");
                    valido = false;
                }
            }
            return valido;
        }

        protected void btnGuardarR_Click(object sender, EventArgs e)
        {
            if (!ValidarResolucion()) return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = @"UPDATE reclamogarantia SET SolucionAplicada=@Sol, EstadoReclamo=@EstRec, FechaSolucion=@FecSol WHERE ID_Reclamo=@ID";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Sol", txtSolucion.Text.Trim());
                    cmd.Parameters.AddWithValue("@EstRec", ddlEstadoReclamo.SelectedValue);

                    object fecSol = DBNull.Value;
                    if (!string.IsNullOrEmpty(txtFechaSolucion.Text))
                    {
                        fecSol = ConvertirFechaMySQL(txtFechaSolucion.Text);
                    }
                    cmd.Parameters.AddWithValue("@FecSol", fecSol);
                    cmd.Parameters.AddWithValue("@ID", hfIDReclamo.Value);

                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Reclamo actualizado exitosamente.", "success");

                    MostrarPanel("ListadoReclamos");
                    CargarReclamos();
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al actualizar: " + ex.Message, "error");
            }
        }

        protected void btnCancelarR_Click(object sender, EventArgs e)
        {
            MostrarPanel("ListadoReclamos");
        }

        private void MostrarPanel(string panel)
        {
            pnlListadoReclamos.Visible = (panel == "ListadoReclamos");
            pnlMantReclamo.Visible = (panel == "MantReclamo");
            LimpiarErrores();
        }

        private void LimpiarErrores()
        {
            if (errSolucion != null) errSolucion.Visible = false;
            if (errFechaSolucion != null) errFechaSolucion.Visible = false;
        }

        private void MostrarError(Label lbl, string msj)
        {
            if (lbl != null)
            {
                lbl.Text = msj;
                lbl.Visible = true;
            }
        }

        private void MostrarMensaje(string titulo, string tipo)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Alert", $"Swal.fire('Gestión de Reclamos', '{titulo}', '{tipo}');", true);
        }

        private string ConvertirFechaMySQL(string fecha)
        {
            if (DateTime.TryParse(fecha, out DateTime d)) return d.ToString("yyyy-MM-dd");
            return fecha;
        }

        public string ObtenerClaseEstadoReclamo(string estado)
        {
            if (estado == "Resuelto") return "bg-activa";
            if (estado == "Pendiente") return "bg-vencida";
            if (estado == "En Proceso") return "bg-proceso";
            return "bg-anulada";
        }
    }
}