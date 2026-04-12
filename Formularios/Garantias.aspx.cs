using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Garantias : System.Web.UI.Page
    {
        private int IDGarantiaActual
        {
            get { return (int)(ViewState["IDGarantiaActual"] ?? 0); }
            set { ViewState["IDGarantiaActual"] = value; }
        }

        private string NOMBRE_USUARIO_ACTUAL
        {
            get
            {
                if (Session["ID_Usuario"] != null)
                {
                    int idUsuario = Convert.ToInt32(Session["ID_Usuario"]);
                    return ObtenerNombreUsuarioDesdeBD(idUsuario);
                }
                return "Usuario Desconocido";
            }
        }

        private string ObtenerNombreUsuarioDesdeBD(int idUsuario)
        {
            string nombreCompleto = "Usuario";
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = "SELECT CONCAT(Nombres, ' ', Apellidos) FROM Usuario WHERE ID_Usuario = @ID";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", idUsuario);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        nombreCompleto = result.ToString();
                    }
                }
            }
            catch { }
            return nombreCompleto;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFiltroFechaInicio.Text = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
                txtFiltroFechaFin.Text = DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd");

                CargarCombosGarantia();
                CargarGarantias();
            }
        }

        private void CargarGarantias()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();

                    string sql = @"SELECT g.ID_Garantia, g.FechaFin, g.FechaRegistro, g.Estado, 
                                   CONCAT(
                                      IF(c.TipoCliente='Natural', CONCAT(cn.Nombre,' ',cn.Apellido), cj.NombreEmpresa),
                                      ' - ', p.Descripcion
                                   ) as DescripcionProducto,
                                   e.Responsable as ResponsableEntrega
                                   FROM Garantia g
                                   INNER JOIN DetalleVentaLentes dl ON g.ID_DetalleVentaLente = dl.ID_DetalleVentaLentes
                                   INNER JOIN Expediente exp ON dl.ID_Expediente = exp.ID_Expediente
                                   INNER JOIN Producto p ON exp.ID_Producto = p.ID_Producto
                                   INNER JOIN Venta v ON dl.ID_Venta = v.ID_Venta
                                   INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
                                   LEFT JOIN ClienteNatural cn ON c.ID_Cliente = cn.ID_Cliente
                                   LEFT JOIN ClienteJuridico cj ON c.ID_Cliente = cj.ID_Cliente
                                   INNER JOIN Entrega e ON g.ID_Entrega = e.ID_Entrega
                                   WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscarGarantia.Text))
                    {
                        sql += @" AND (v.NumeroDocumento LIKE @Busq 
                                   OR e.Responsable LIKE @Busq 
                                   OR cn.Nombre LIKE @Busq 
                                   OR cj.NombreEmpresa LIKE @Busq) ";
                    }

                    if (ddlFiltroEstadoG.SelectedValue != "-1")
                        sql += " AND g.Estado = @Est ";

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                        sql += " AND g.FechaRegistro BETWEEN @FecIni AND @FecFin ";

                    sql += " ORDER BY g.ID_Garantia DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscarGarantia.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Est", ddlFiltroEstadoG.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", ConvertirFechaMySQL(txtFiltroFechaInicio.Text));
                    cmd.Parameters.AddWithValue("@FecFin", ConvertirFechaMySQL(txtFiltroFechaFin.Text));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvGarantias.DataSource = dt;
                    gvGarantias.DataBind();

                    pnlMensajeGridG.Visible = false;
                    if (ddlPageSize.SelectedValue != "All")
                    {
                        int pageSize = int.Parse(ddlPageSize.SelectedValue);
                        if (dt.Rows.Count > 0 && dt.Rows.Count < pageSize)
                        {
                            lblMensajeGridG.Text = $"Nota: Solicitó ver {pageSize} registros, pero solo existen {dt.Rows.Count} disponibles.";
                            pnlMensajeGridG.Visible = true;
                        }
                    }
                    if (dt.Rows.Count == 0)
                    {
                        lblMensajeGridG.Text = "No hay garantías registradas con estos filtros.";
                        pnlMensajeGridG.Visible = true;
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error cargando garantías: " + ex.Message, "error"); }
        }

        private void CargarCombosGarantia()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sqlL = @"SELECT dl.ID_DetalleVentaLentes, 
                                    CONCAT(
                                        IF(c.TipoCliente='Natural', CONCAT(cn.Nombre,' ',cn.Apellido), cj.NombreEmpresa),
                                        ' - ', p.Descripcion, ' (Doc: ', v.NumeroDocumento, ')'
                                    ) as Descrip 
                                    FROM DetalleVentaLentes dl
                                    INNER JOIN Venta v ON dl.ID_Venta = v.ID_Venta
                                    INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
                                    LEFT JOIN ClienteNatural cn ON c.ID_Cliente = cn.ID_Cliente
                                    LEFT JOIN ClienteJuridico cj ON c.ID_Cliente = cj.ID_Cliente
                                    INNER JOIN Expediente exp ON dl.ID_Expediente = exp.ID_Expediente
                                    INNER JOIN Producto p ON exp.ID_Producto = p.ID_Producto
                                    INNER JOIN Entrega ent ON v.ID_Venta = ent.ID_Venta
                                    WHERE dl.Estado = 1 AND ent.Estado = 1
                                    ORDER BY dl.ID_DetalleVentaLentes DESC";

                    MySqlDataAdapter daL = new MySqlDataAdapter(sqlL, con);
                    DataTable dtL = new DataTable();
                    daL.Fill(dtL);
                    ddlDetalleLente.DataSource = dtL;
                    ddlDetalleLente.DataTextField = "Descrip";
                    ddlDetalleLente.DataValueField = "ID_DetalleVentaLentes";
                    ddlDetalleLente.DataBind();
                    ddlDetalleLente.Items.Insert(0, new ListItem("-- Seleccione Producto --", "0"));

                    string sqlE = "SELECT ID_Entrega, CONCAT('ID: ', ID_Entrega, ' - ', Responsable) as Descrip FROM Entrega WHERE Estado = 1 ORDER BY ID_Entrega DESC";
                    MySqlDataAdapter daE = new MySqlDataAdapter(sqlE, con);
                    DataTable dtE = new DataTable();
                    daE.Fill(dtE);
                    ddlEntrega.DataSource = dtE;
                    ddlEntrega.DataTextField = "Descrip";
                    ddlEntrega.DataValueField = "ID_Entrega";
                    ddlEntrega.DataBind();
                    ddlEntrega.Items.Insert(0, new ListItem("-- Seleccione Entrega --", "0"));
                }
            }
            catch (Exception ex) { MostrarMensaje("Error listas: " + ex.Message, "error"); }
        }

        protected void ddlDetalleLente_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlDetalleLente.SelectedValue != "0")
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                    {
                        con.Open();
                        string sql = @"SELECT v.Fecha, ent.ID_Entrega 
                                       FROM DetalleVentaLentes dl 
                                       JOIN Venta v ON dl.ID_Venta = v.ID_Venta 
                                       JOIN Entrega ent ON v.ID_Venta = ent.ID_Venta
                                       WHERE dl.ID_DetalleVentaLentes = @ID AND ent.Estado = 1 LIMIT 1";
                        MySqlCommand cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@ID", ddlDetalleLente.SelectedValue);

                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                DateTime fechaVenta = Convert.ToDateTime(r["Fecha"]);
                                txtFechaFinG.Text = fechaVenta.AddMonths(6).ToString("yyyy-MM-dd");

                                if (r["ID_Entrega"] != DBNull.Value)
                                {
                                    string idEnt = r["ID_Entrega"].ToString();
                                    if (ddlEntrega.Items.FindByValue(idEnt) != null)
                                    {
                                        ddlEntrega.SelectedValue = idEnt;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            else
            {
                txtFechaFinG.Text = "";
                ddlEntrega.SelectedValue = "0";
            }
        }

        protected void btnNuevoReclamo_Click(object sender, EventArgs e)
        {
            hfIDReclamo.Value = "";
            txtFechaReclamo.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtFechaReclamo.Enabled = false;

            txtResponsableR.Text = NOMBRE_USUARIO_ACTUAL;
            txtResponsableR.Enabled = false;

            txtMotivo.Text = "";
            txtSolucion.Text = "";
            txtFechaSolucion.Text = "";
            ddlEstadoReclamo.SelectedValue = "Pendiente";
            MostrarPanel("MantReclamo");
        }

        protected void btnNuevaGarantia_Click(object sender, EventArgs e)
        {
            if (ddlDetalleLente.Items.Count <= 1)
            {
                MostrarMensaje("No hay ventas de lentes entregadas disponibles para aplicar garantía.", "warning");
                return;
            }
            if (ddlEntrega.Items.Count <= 1)
            {
                MostrarMensaje("No hay entregas registradas en el sistema.", "warning");
                return;
            }

            hfIDGarantia.Value = "";
            ddlDetalleLente.SelectedIndex = 0;
            ddlEntrega.SelectedIndex = 0;
            txtFechaFinG.Text = "";
            txtFechaFinG.Enabled = false;
            txtFechaRegG.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ddlEstadoG.SelectedValue = "Activa";
            MostrarPanel("MantGarantia");
        }

        protected void btnGuardarG_Click(object sender, EventArgs e)
        {
            if (!ValidarGarantia()) return;
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd;
                    if (string.IsNullOrEmpty(hfIDGarantia.Value))
                    {
                        string sql = @"INSERT INTO Garantia (ID_DetalleVentaLente, ID_Entrega, FechaFin, Estado, FechaRegistro)
                                       VALUES (@Lente, @Entrega, @Fin, @Est, CURRENT_DATE)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Lente", ddlDetalleLente.SelectedValue);
                        cmd.Parameters.AddWithValue("@Entrega", ddlEntrega.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fin", ConvertirFechaMySQL(txtFechaFinG.Text));
                        cmd.Parameters.AddWithValue("@Est", ddlEstadoG.SelectedValue);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Garantía creada.", "success");
                    }
                    else
                    {
                        string sql = @"UPDATE Garantia SET ID_DetalleVentaLente=@Lente, ID_Entrega=@Entrega, 
                                       FechaFin=@Fin, Estado=@Est WHERE ID_Garantia=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Lente", ddlDetalleLente.SelectedValue);
                        cmd.Parameters.AddWithValue("@Entrega", ddlEntrega.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fin", ConvertirFechaMySQL(txtFechaFinG.Text));
                        cmd.Parameters.AddWithValue("@Est", ddlEstadoG.SelectedValue);
                        cmd.Parameters.AddWithValue("@ID", hfIDGarantia.Value);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Garantía actualizada.", "success");
                    }
                    MostrarPanel("ListadoGarantias");
                    CargarGarantias();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        private bool ValidarReclamo()
        {
            bool v = true;
            LimpiarErrores();
            string rgx = @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚ]+)*$";
            if (string.IsNullOrWhiteSpace(txtMotivo.Text) || !Regex.IsMatch(txtMotivo.Text, rgx))
            {
                MostrarError(errMotivo, "Solo letras, sin números/símbolos, un espacio."); v = false;
            }
            if (ddlEstadoReclamo.SelectedValue == "Resuelto")
            {
                if (string.IsNullOrWhiteSpace(txtSolucion.Text) || !Regex.IsMatch(txtSolucion.Text, rgx))
                {
                    MostrarError(errSolucion, "Requerido (Solo letras)."); v = false;
                }
                if (DateTime.TryParse(txtFechaSolucion.Text, out DateTime fs) && DateTime.TryParse(txtFechaReclamo.Text, out DateTime fr))
                {
                    if (fs > fr.AddDays(7)) { MostrarError(errFechaSolucion, "Máximo 1 semana plazo."); v = false; }
                    if (fs < fr) { MostrarError(errFechaSolucion, "No puede ser anterior al reclamo."); v = false; }
                }
                else { MostrarError(errFechaSolucion, "Fecha requerida."); v = false; }
            }
            return v;
        }

        private bool ValidarGarantia()
        {
            bool v = true;
            LimpiarErrores();
            if (ddlDetalleLente.SelectedValue == "0") { MostrarError(errLente, "Seleccione un lente."); v = false; }
            if (ddlEntrega.SelectedValue == "0") { MostrarError(errEntrega, "Seleccione una entrega."); v = false; }
            if (string.IsNullOrEmpty(txtFechaFinG.Text)) { MostrarError(errFechaFinG, "Fecha fin inválida."); v = false; }
            return v;
        }

        protected void btnGuardarR_Click(object sender, EventArgs e)
        {
            if (!ValidarReclamo()) return;
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd;
                    if (string.IsNullOrEmpty(hfIDReclamo.Value))
                    {
                        string sql = @"INSERT INTO ReclamoGarantia (ID_Garantia, FechaReclamo, Motivo, SolucionAplicada, 
                                       EstadoReclamo, Responsable, FechaSolucion, FechaRegistro, Estado)
                                       VALUES (@Garantia, @FecRec, @Mot, @Sol, @EstRec, @Resp, @FecSol, CURRENT_DATE, 1)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Garantia", IDGarantiaActual);
                        cmd.Parameters.AddWithValue("@FecRec", ConvertirFechaMySQL(txtFechaReclamo.Text));
                        cmd.Parameters.AddWithValue("@Mot", txtMotivo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Sol", txtSolucion.Text.Trim());
                        cmd.Parameters.AddWithValue("@EstRec", ddlEstadoReclamo.SelectedValue);
                        cmd.Parameters.AddWithValue("@Resp", txtResponsableR.Text.Trim());
                        object fecSol = DBNull.Value;
                        if (!string.IsNullOrEmpty(txtFechaSolucion.Text)) fecSol = ConvertirFechaMySQL(txtFechaSolucion.Text);
                        cmd.Parameters.AddWithValue("@FecSol", fecSol);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Reclamo agregado.", "success");
                    }
                    else
                    {
                        string sql = @"UPDATE ReclamoGarantia SET Motivo=@Mot, SolucionAplicada=@Sol,
                                       EstadoReclamo=@EstRec, Responsable=@Resp, FechaSolucion=@FecSol WHERE ID_Reclamo=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Mot", txtMotivo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Sol", txtSolucion.Text.Trim());
                        cmd.Parameters.AddWithValue("@EstRec", ddlEstadoReclamo.SelectedValue);
                        cmd.Parameters.AddWithValue("@Resp", txtResponsableR.Text.Trim());
                        object fecSol = DBNull.Value;
                        if (!string.IsNullOrEmpty(txtFechaSolucion.Text)) fecSol = ConvertirFechaMySQL(txtFechaSolucion.Text);
                        cmd.Parameters.AddWithValue("@FecSol", fecSol);
                        cmd.Parameters.AddWithValue("@ID", hfIDReclamo.Value);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Reclamo actualizado.", "success");
                    }
                    CargarReclamos(IDGarantiaActual);
                    MostrarPanel("ListadoReclamos");
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        protected void gvGarantias_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.IsNullOrEmpty(e.CommandArgument?.ToString())) return;
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditarG")
            {
                hfIDGarantia.Value = id.ToString();
                CargarDatosGarantia(id);
                MostrarPanel("MantGarantia");
            }
            else if (e.CommandName == "DarBajaG")
            {
                CambiarEstadoGeneral("Garantia", "Estado", "Anulada", "ID_Garantia", id);
                CargarGarantias();
            }
            else if (e.CommandName == "VerReclamos")
            {
                IDGarantiaActual = id;
                lblIDGarantiaRef.Text = id.ToString();
                CargarReclamos(id);
                MostrarPanel("ListadoReclamos");
            }
        }

        protected void gvReclamos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditarR")
            {
                CargarDatosReclamo(id);
                MostrarPanel("MantReclamo");
            }
        }

        private void CambiarEstadoGeneral(string tabla, string campoEstado, string valorEstado, string campoID, int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = $"UPDATE {tabla} SET {campoEstado} = @Val WHERE {campoID} = @ID";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Val", valorEstado);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Registro dado de baja/anulado.", "success");
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al dar baja: " + ex.Message, "error"); }
        }

        private void CargarDatosGarantia(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Garantia WHERE ID_Garantia=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        string idLente = r["ID_DetalleVentaLente"].ToString();
                        if (ddlDetalleLente.Items.FindByValue(idLente) != null) ddlDetalleLente.SelectedValue = idLente;
                        string idEnt = r["ID_Entrega"].ToString();
                        if (ddlEntrega.Items.FindByValue(idEnt) != null) ddlEntrega.SelectedValue = idEnt;
                        if (r["FechaFin"] != DBNull.Value) txtFechaFinG.Text = Convert.ToDateTime(r["FechaFin"]).ToString("yyyy-MM-dd");
                        ddlEstadoG.SelectedValue = r["Estado"].ToString();
                        if (r["FechaRegistro"] != DBNull.Value) txtFechaRegG.Text = Convert.ToDateTime(r["FechaRegistro"]).ToString("yyyy-MM-dd");
                    }
                }
            }
            catch { }
        }

        private void CargarDatosReclamo(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM ReclamoGarantia WHERE ID_Reclamo=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        hfIDReclamo.Value = id.ToString();
                        if (r["FechaReclamo"] != DBNull.Value) txtFechaReclamo.Text = Convert.ToDateTime(r["FechaReclamo"]).ToString("yyyy-MM-dd");
                        txtMotivo.Text = r["Motivo"].ToString();
                        txtSolucion.Text = r["SolucionAplicada"].ToString();
                        ddlEstadoReclamo.SelectedValue = r["EstadoReclamo"].ToString();
                        txtResponsableR.Text = r["Responsable"].ToString();
                        txtResponsableR.Enabled = true;
                        if (r["FechaSolucion"] != DBNull.Value) txtFechaSolucion.Text = Convert.ToDateTime(r["FechaSolucion"]).ToString("yyyy-MM-dd");
                    }
                }
            }
            catch { }
        }

        private void CargarReclamos(int idGarantia)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = "SELECT * FROM ReclamoGarantia WHERE ID_Garantia = @ID ORDER BY FechaReclamo DESC";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", idGarantia);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvReclamos.DataSource = dt;
                    gvReclamos.DataBind();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al cargar reclamos: " + ex.Message, "error"); }
        }

        private void MostrarPanel(string panel)
        {
            pnlListadoGarantias.Visible = (panel == "ListadoGarantias");
            pnlMantGarantia.Visible = (panel == "MantGarantia");
            pnlListadoReclamos.Visible = (panel == "ListadoReclamos");
            pnlMantReclamo.Visible = (panel == "MantReclamo");
            LimpiarErrores();
        }

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscarGarantia.Text = ""; ddlFiltroEstadoG.SelectedIndex = 0; txtFiltroFechaInicio.Text = ""; txtFiltroFechaFin.Text = "";
            ddlPageSize.SelectedValue = "All"; gvGarantias.AllowPaging = false;
            CargarGarantias();
        }
        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvGarantias.AllowPaging = false; else { gvGarantias.AllowPaging = true; gvGarantias.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarGarantias();
        }
        protected void btnFiltrarGarantia_Click(object sender, EventArgs e) { CargarGarantias(); }
        protected void gvGarantias_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvGarantias.PageIndex = e.NewPageIndex; CargarGarantias(); }
        protected void btnCancelarG_Click(object sender, EventArgs e) { MostrarPanel("ListadoGarantias"); }
        protected void btnVolverG_Click(object sender, EventArgs e) { MostrarPanel("ListadoGarantias"); }
        protected void btnCancelarR_Click(object sender, EventArgs e) { MostrarPanel("ListadoReclamos"); }

        private void LimpiarErrores() { if (errLente != null) errLente.Visible = false; if (errEntrega != null) errEntrega.Visible = false; if (errFechaFinG != null) errFechaFinG.Visible = false; if (errFechaReclamo != null) errFechaReclamo.Visible = false; if (errResponsableR != null) errResponsableR.Visible = false; if (errMotivo != null) errMotivo.Visible = false; if (errSolucion != null) errSolucion.Visible = false; if (errFechaSolucion != null) errFechaSolucion.Visible = false; }
        private void MostrarError(Label l, string m) { if (l != null) { l.Text = m; l.Visible = true; } }
        private void MostrarMensaje(string t, string i) { ScriptManager.RegisterStartupScript(this, GetType(), "T", $"Swal.fire('Sistema', '{t}', '{i}');", true); }
        private string ConvertirFechaMySQL(string f) { if (DateTime.TryParse(f, out DateTime d)) return d.ToString("yyyy-MM-dd"); return f; }
        public string ObtenerClaseEstado(string estado) { if (estado == "Activa") return "bg-activa"; if (estado == "Vencida") return "bg-vencida"; return "bg-anulada"; }
    }
}