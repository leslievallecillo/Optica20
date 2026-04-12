using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Entrega : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFiltroFechaInicio.Text = DateTime.Now.AddMonths(-2).ToString("yyyy-MM-dd");
                txtFiltroFechaFin.Text = DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd");

                CargarCombos();
                CargarDatos();
            }
        }

        private void CargarCombos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();

                    string sqlVentas = @"
                        SELECT 
                            v.ID_Venta, 
                            CONCAT('Venta #', v.ID_Venta, ' - ', COALESCE(cn.Nombre, cj.NombreEmpresa, ''), ' ', COALESCE(cn.Apellido, '')) AS NombreCliente 
                        FROM Venta v 
                        INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
                        LEFT JOIN ClienteNatural cn ON c.ID_Cliente = cn.ID_Cliente
                        LEFT JOIN ClienteJuridico cj ON c.ID_Cliente = cj.ID_Cliente
                        WHERE v.Estado = 1 AND EXISTS (SELECT 1 FROM DetalleVentaLentes dl WHERE dl.ID_Venta = v.ID_Venta)
                        ORDER BY v.ID_Venta DESC";

                    MySqlDataAdapter daV = new MySqlDataAdapter(sqlVentas, con);
                    DataTable dtV = new DataTable();
                    daV.Fill(dtV);

                    ddlVenta.DataSource = dtV;
                    ddlVenta.DataTextField = "NombreCliente";
                    ddlVenta.DataValueField = "ID_Venta";
                    ddlVenta.DataBind();
                    ddlVenta.Items.Insert(0, new ListItem("-- Seleccione Venta --", "0"));

                    string sqlResp = "SELECT DISTINCT Responsable FROM Entrega WHERE Responsable IS NOT NULL AND Responsable != '' AND Responsable != 'ANULADO' ORDER BY Responsable";
                    MySqlDataAdapter daR = new MySqlDataAdapter(sqlResp, con);
                    DataTable dtR = new DataTable();
                    daR.Fill(dtR);

                    ddlResponsable.DataSource = dtR;
                    ddlResponsable.DataTextField = "Responsable";
                    ddlResponsable.DataValueField = "Responsable";
                    ddlResponsable.DataBind();
                    ddlResponsable.Items.Insert(0, new ListItem("-- Seleccione --", ""));
                }
            }
            catch (Exception ex) { MostrarMensaje("Error cargando combos: " + ex.Message, "error"); }
        }

        private void CargarDatos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = @"SELECT e.ID_Entrega, e.ID_Venta, e.Fecha, e.Responsable, e.Observaciones, e.FechaRegistro, e.Estado, v.NumeroDocumento 
                                     FROM Entrega e 
                                     INNER JOIN Venta v ON e.ID_Venta = v.ID_Venta 
                                     WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                        query += " AND (e.Responsable LIKE @Busq OR e.Observaciones LIKE @Busq OR v.NumeroDocumento LIKE @Busq) ";

                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND e.Estado = @Estado ";

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                        query += " AND e.Fecha BETWEEN @FecIni AND @FecFin ";

                    query += " ORDER BY e.Fecha DESC, e.ID_Entrega DESC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", ConvertirFechaMySQL(txtFiltroFechaInicio.Text));
                    cmd.Parameters.AddWithValue("@FecFin", ConvertirFechaMySQL(txtFiltroFechaFin.Text));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvEntregas.DataSource = dt;
                    gvEntregas.DataBind();

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
            catch (Exception ex) { MostrarMensaje("Error al cargar listado: " + ex.Message, "error"); }
        }

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            txtFiltroFechaInicio.Text = "";
            txtFiltroFechaFin.Text = "";

            ddlPageSize.SelectedValue = "All";
            gvEntregas.AllowPaging = false;

            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvEntregas.AllowPaging = false;
            else { gvEntregas.AllowPaging = true; gvEntregas.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void btnGestorResponsable_Click(object sender, EventArgs e)
        {
            string action = hfActionResponsable.Value;
            string valor = hfValorResponsable.Value.Trim();
            string anterior = hfOldResponsable.Value.Trim();

            if (action == "ADD")
            {
                if (ValidarFormatoNombre(valor))
                {
                    if (ddlResponsable.Items.FindByValue(valor) == null)
                    {
                        ddlResponsable.Items.Add(new ListItem(valor, valor));
                    }
                    ddlResponsable.SelectedValue = valor;
                }
            }
            else if (action == "EDIT")
            {
                if (ValidarFormatoNombre(valor))
                {
                    EjecutarSQL("UPDATE Entrega SET Responsable=@New WHERE Responsable=@Old", valor, anterior);
                    CargarCombos();
                    ddlResponsable.SelectedValue = valor;
                    CargarDatos();
                    MostrarMensaje("Responsable actualizado exitosamente.", "success");
                }
            }
            else if (action == "DELETE")
            {
                EjecutarSQL("UPDATE Entrega SET Responsable='ANULADO' WHERE Responsable=@Old", "", valor);
                CargarCombos();
                CargarDatos();
                MostrarMensaje("Responsable dado de baja.", "success");
            }
        }

        private bool ValidarFormatoNombre(string nombre)
        {
            if (!Regex.IsMatch(nombre, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ]+\s[a-zA-ZñÑáéíóúÁÉÍÓÚ]+$"))
            {
                MostrarError(errResponsable, "Formato inválido (Nombre Apellido, sin símbolos).");
                return false;
            }
            return true;
        }

        private void EjecutarSQL(string sql, string paramNew, string paramOld)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(sql, con);
                if (!string.IsNullOrEmpty(paramNew)) cmd.Parameters.AddWithValue("@New", paramNew);
                cmd.Parameters.AddWithValue("@Old", paramOld);
                cmd.ExecuteNonQuery();
            }
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            if (ddlVenta.Items.Count <= 1)
            {
                MostrarMensaje("No existen ventas registradas para realizar entregas.", "warning");
                return;
            }
            LimpiarFormulario();
            PanelListado.Visible = false;
            PanelMantenimiento.Visible = true;
            lblTituloAccion.Text = "Registrar Nueva Entrega";
            txtFechaRegistro.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtFechaRegistro.Enabled = false;
            txtFechaEntrega.Text = DateTime.Now.ToString("yyyy-MM-dd");
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

                    if (string.IsNullOrEmpty(hfIDEntrega.Value))
                    {
                        string sql = @"INSERT INTO Entrega (ID_Venta, Fecha, Responsable, Observaciones, FechaRegistro, Estado) 
                                       VALUES (@Venta, @Fecha, @Resp, @Obs, CURRENT_DATE, 1)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Venta", ddlVenta.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fecha", ConvertirFechaMySQL(txtFechaEntrega.Text));
                        cmd.Parameters.AddWithValue("@Resp", ddlResponsable.SelectedValue);
                        cmd.Parameters.AddWithValue("@Obs", txtObservaciones.Text.Trim());
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Entrega registrada exitosamente.", "success");
                    }
                    else
                    {
                        string sql = @"UPDATE Entrega SET ID_Venta=@Venta, Fecha=@Fecha, Responsable=@Resp, 
                                       Observaciones=@Obs, FechaRegistro=@FecReg 
                                       WHERE ID_Entrega=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Venta", ddlVenta.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fecha", ConvertirFechaMySQL(txtFechaEntrega.Text));
                        cmd.Parameters.AddWithValue("@Resp", ddlResponsable.SelectedValue);
                        cmd.Parameters.AddWithValue("@Obs", txtObservaciones.Text.Trim());
                        cmd.Parameters.AddWithValue("@FecReg", ConvertirFechaMySQL(txtFechaRegistro.Text));
                        cmd.Parameters.AddWithValue("@ID", hfIDEntrega.Value);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Entrega actualizada exitosamente.", "success");
                    }

                    btnCancelar_Click(sender, e);
                    CargarDatos();
                    CargarCombos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al guardar: " + ex.Message, "error"); }
        }

        protected void gvEntregas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar") CargarParaEditar(Convert.ToInt32(e.CommandArgument));
            else if (e.CommandName == "DarBaja") CambiarEstado(Convert.ToInt32(e.CommandArgument), 0);
            else if (e.CommandName == "Reactivar") CambiarEstado(Convert.ToInt32(e.CommandArgument), 1);
        }

        private void CargarParaEditar(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Entrega WHERE ID_Entrega=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        hfIDEntrega.Value = id.ToString();

                        string idVenta = r["ID_Venta"].ToString();
                        if (ddlVenta.Items.FindByValue(idVenta) != null) ddlVenta.SelectedValue = idVenta;
                        else ddlVenta.SelectedIndex = 0;

                        if (r["Fecha"] != DBNull.Value)
                            txtFechaEntrega.Text = Convert.ToDateTime(r["Fecha"]).ToString("yyyy-MM-dd");

                        string resp = r["Responsable"].ToString();
                        if (ddlResponsable.Items.FindByValue(resp) != null)
                        {
                            ddlResponsable.SelectedValue = resp;
                        }
                        else
                        {
                            ddlResponsable.Items.Add(new ListItem(resp, resp));
                            ddlResponsable.SelectedValue = resp;
                        }

                        txtObservaciones.Text = r["Observaciones"].ToString();

                        if (r["FechaRegistro"] != DBNull.Value)
                            txtFechaRegistro.Text = Convert.ToDateTime(r["FechaRegistro"]).ToString("yyyy-MM-dd");

                        txtFechaRegistro.Enabled = true;
                        PanelListado.Visible = false;
                        PanelMantenimiento.Visible = true;
                        lblTituloAccion.Text = "Modificar Entrega";
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al cargar datos: " + ex.Message, "error"); }
        }

        private void CambiarEstado(int id, int estado)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Entrega SET Estado=@Est WHERE ID_Entrega=@ID", con);
                    cmd.Parameters.AddWithValue("@Est", estado);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    string accion = estado == 1 ? "reactivada" : "anulada";
                    MostrarMensaje($"Entrega {accion} correctamente.", "success");
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al cambiar estado: " + ex.Message, "error"); }
        }

        private bool ValidarEntradas()
        {
            bool v = true;
            LimpiarErrores();

            if (ddlVenta.SelectedValue == "0" || string.IsNullOrEmpty(ddlVenta.SelectedValue))
            {
                MostrarError(errVenta, "Debe seleccionar una venta.");
                v = false;
            }
            else
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                    {
                        con.Open();
                        MySqlCommand cmd = new MySqlCommand("SELECT EstadoPagoVenta FROM Venta WHERE ID_Venta = @ID", con);
                        cmd.Parameters.AddWithValue("@ID", ddlVenta.SelectedValue);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result.ToString() != "Cancelado")
                        {
                            MostrarError(errVenta, "La venta tiene saldo pendiente. No se puede entregar.");
                            v = false;
                        }

                        string sqlDup = "SELECT COUNT(*) FROM Entrega WHERE ID_Venta = @IDVenta AND Estado = 1";
                        if (!string.IsNullOrEmpty(hfIDEntrega.Value))
                        {
                            sqlDup += " AND ID_Entrega != @IDEntrega";
                        }
                        MySqlCommand cmdDup = new MySqlCommand(sqlDup, con);
                        cmdDup.Parameters.AddWithValue("@IDVenta", ddlVenta.SelectedValue);
                        if (!string.IsNullOrEmpty(hfIDEntrega.Value))
                        {
                            cmdDup.Parameters.AddWithValue("@IDEntrega", hfIDEntrega.Value);
                        }
                        int existe = Convert.ToInt32(cmdDup.ExecuteScalar());
                        if (existe > 0)
                        {
                            MostrarError(errVenta, "Ya existe una entrega activa para esta venta.");
                            v = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarError(errVenta, "Error al validar la venta: " + ex.Message);
                    v = false;
                }
            }

            if (string.IsNullOrEmpty(ddlResponsable.SelectedValue))
            {
                MostrarError(errResponsable, "Debe seleccionar un responsable."); v = false;
            }

            string obs = txtObservaciones.Text;
            if (!string.IsNullOrWhiteSpace(obs))
            {
                if (!Regex.IsMatch(obs, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚ]+)*$"))
                {
                    MostrarError(errObservaciones, "Solo letras y un espacio entre palabras."); v = false;
                }
            }

            if (DateTime.TryParse(txtFechaEntrega.Text, out DateTime fechaEnt))
            {
                DateTime fechaBase = DateTime.Today;
                if (!string.IsNullOrEmpty(hfIDEntrega.Value) && DateTime.TryParse(txtFechaRegistro.Text, out DateTime fr))
                {
                    fechaBase = fr.Date;
                }
                if (fechaEnt < fechaBase) { MostrarError(errFechaEntrega, "La fecha no puede ser anterior al registro."); v = false; }
                if (fechaEnt > DateTime.Today.AddDays(7)) { MostrarError(errFechaEntrega, "No puede superar 7 días desde hoy."); v = false; }
            }
            else { MostrarError(errFechaEntrega, "Fecha inválida."); v = false; }

            if (!string.IsNullOrEmpty(hfIDEntrega.Value))
            {
                if (DateTime.TryParse(txtFechaRegistro.Text, out DateTime fr))
                {
                    if (fr < new DateTime(2023, 6, 1)) { MostrarError(errFechaRegistro, "Fecha muy antigua."); v = false; }
                    if (fr.Date > DateTime.Now.Date) { MostrarError(errFechaRegistro, "No puede ser futura."); v = false; }
                }
                else { MostrarError(errFechaRegistro, "Requerida."); v = false; }
            }

            return v;
        }

        private void LimpiarFormulario()
        {
            hfIDEntrega.Value = "";
            ddlVenta.SelectedIndex = 0;
            ddlResponsable.SelectedIndex = 0;
            txtObservaciones.Text = "";
            txtFechaEntrega.Text = "";
            txtFechaRegistro.Text = "";
            LimpiarErrores();
        }

        private void LimpiarErrores()
        {
            if (errVenta != null) errVenta.Visible = false;
            if (errResponsable != null) errResponsable.Visible = false;
            if (errObservaciones != null) errObservaciones.Visible = false;
            if (errFechaEntrega != null) errFechaEntrega.Visible = false;
            if (errFechaRegistro != null) errFechaRegistro.Visible = false;
        }

        private void MostrarError(Label l, string m) { if (l != null) { l.Text = m; l.Visible = true; } }
        private string ConvertirFechaMySQL(string f) { if (DateTime.TryParse(f, out DateTime d)) return d.ToString("yyyy-MM-dd"); return f; }
        private void MostrarMensaje(string t, string i) { ScriptManager.RegisterStartupScript(this, GetType(), "T", $"Swal.fire('Gestión Entregas', '{t}', '{i}');", true); }
        protected void btnBuscar_Click(object sender, EventArgs e) { CargarDatos(); }
        protected void gvEntregas_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvEntregas.PageIndex = e.NewPageIndex; CargarDatos(); }
    }
}