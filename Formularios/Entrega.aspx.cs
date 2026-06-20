using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;
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
                txtFiltroFechaInicio.Text = "";
                txtFiltroFechaFin.Text = "";

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
                        FROM venta v 
                        INNER JOIN clientes c ON v.ID_Cliente = c.ID_Cliente
                        LEFT JOIN clientenatural cn ON c.ID_Cliente = cn.ID_Cliente
                        LEFT JOIN clientejuridico cj ON c.ID_Cliente = cj.ID_Cliente
                        WHERE v.Estado = 1 AND EXISTS (SELECT 1 FROM detalleventalentes dl WHERE dl.ID_Venta = v.ID_Venta)
                        ORDER BY v.ID_Venta DESC";

                    MySqlDataAdapter daV = new MySqlDataAdapter(sqlVentas, con);
                    DataTable dtV = new DataTable();
                    daV.Fill(dtV);

                    ddlVenta.DataSource = dtV;
                    ddlVenta.DataTextField = "NombreCliente";
                    ddlVenta.DataValueField = "ID_Venta";
                    ddlVenta.DataBind();
                    ddlVenta.Items.Insert(0, new ListItem("-- Seleccione Venta --", "0"));

                    string sqlResp = "SELECT DISTINCT Responsable FROM entrega WHERE Responsable IS NOT NULL AND Responsable != '' AND Responsable != 'ANULADO' ORDER BY Responsable";
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

        private void CargarDashboardLentes()
        {
            lblDashSolicitado.Text = "0";
            lblDashProceso.Text = "0";
            lblDashTerminado.Text = "0";

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = "SELECT EstadoLente, COUNT(*) as Total FROM entrega WHERE Estado = 1 GROUP BY EstadoLente";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string estado = r["EstadoLente"].ToString();
                            string total = r["Total"].ToString();

                            if (estado == "Solicitado") lblDashSolicitado.Text = total;
                            else if (estado == "En Proceso") lblDashProceso.Text = total;
                            else if (estado == "Terminado") lblDashTerminado.Text = total;
                        }
                    }
                }
            }
            catch { }
        }

        private void CargarDatos()
        {
            CargarDashboardLentes();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = @"SELECT e.ID_Entrega, e.ID_Venta, e.Fecha, e.Responsable, e.Observaciones, e.FechaRegistro, e.Estado, e.EstadoLente, v.NumeroDocumento 
                                     FROM entrega e 
                                     INNER JOIN venta v ON e.ID_Venta = v.ID_Venta 
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

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                    {
                        cmd.Parameters.AddWithValue("@FecIni", ConvertirFechaMySQL(txtFiltroFechaInicio.Text));
                        cmd.Parameters.AddWithValue("@FecFin", ConvertirFechaMySQL(txtFiltroFechaFin.Text));
                    }

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

        protected void ddlEstadoLente_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlEstadoLente.SelectedValue == "Terminado")
            {
                divNotificar.Visible = true;
            }
            else
            {
                divNotificar.Visible = false;
            }
        }

        protected void btnEnviarNotificacion_Click(object sender, EventArgs e)
        {
            if (ddlVenta.SelectedValue == "0" || string.IsNullOrEmpty(ddlVenta.SelectedValue))
            {
                MostrarMensaje("Debe seleccionar una venta primero.", "warning");
                return;
            }

            string idVenta = ddlVenta.SelectedValue;
            string correoDestino = "";
            string nombreCliente = "";

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = @"SELECT c.Correo, COALESCE(cn.Nombre, cj.NombreEmpresa, 'Cliente') as Nombre
                                   FROM venta v
                                   INNER JOIN clientes c ON v.ID_Cliente = c.ID_Cliente
                                   LEFT JOIN clientenatural cn ON c.ID_Cliente = cn.ID_Cliente
                                   LEFT JOIN clientejuridico cj ON c.ID_Cliente = cj.ID_Cliente
                                   WHERE v.ID_Venta = @ID_Venta";
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@ID_Venta", idVenta);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                correoDestino = r["Correo"].ToString();
                                nombreCliente = r["Nombre"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al buscar cliente: " + ex.Message, "error");
                    return;
                }
            }

            if (string.IsNullOrEmpty(correoDestino))
            {
                MostrarMensaje("El cliente seleccionado no tiene un correo registrado en el sistema.", "error");
                return;
            }

            if (EnviarNotificacionEmail(correoDestino, nombreCliente))
            {
                MostrarMensaje("Notificación enviada exitosamente al cliente.", "success");
            }
            else
            {
                MostrarMensaje("Error al enviar el correo. Verifique la conexión o las credenciales.", "error");
            }
        }

        private bool EnviarNotificacionEmail(string destino, string nombre)
        {
            try
            {
                string correoEmisor = "zelayaomeany6@gmail.com";
                string claveAplicacion = "gptmcunrlsoafdka";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(correoEmisor, "Óptica 20/20");
                mail.To.Add(destino);
                mail.Subject = "¡Tus lentes están listos para retirar!";
                mail.Body = $@"
                    <div style='background:#f4f6f9; padding:20px; font-family:sans-serif;'>
                        <div style='background:#fff; padding:30px; border-radius:8px; border-top: 5px solid #0056b3; max-width:500px; margin:auto;'>
                            <h2 style='color:#333; text-align:center;'>¡Tus lentes están listos!</h2>
                            <p>Hola <strong>{nombre}</strong>,</p>
                            <p>Te informamos que tus lentes ya se encuentran <strong>terminados</strong> y listos para ser retirados.</p>
                            <p>Puedes pasar a recogerlos en nuestra sucursal dentro de los siguientes horarios:</p>
                            
                            <hr style='border:1px solid #eee; margin:20px 0;' />
                            
                            <h4 style='color:#0056b3; margin-bottom:5px;'>Dirección</h4>
                            <p style='margin-top:0; color:#555;'>Av. Principal #123 Colonia Centro Managua, Nicaragua</p>
                            
                            <h4 style='color:#0056b3; margin-bottom:5px;'>Horarios</h4>
                            <p style='margin-top:0; color:#555;'>
                                Lunes a Viernes: 8:00 AM - 6:00 PM<br/>
                                Sábados: 8:00 AM - 2:00 PM<br/>
                                Domingos: Cerrado
                            </p>
                            
                            <h4 style='color:#0056b3; margin-bottom:5px;'>Contacto</h4>
                            <p style='margin-top:0; color:#555;'>
                                Teléfono: (505) 2233-4455<br/>
                                WhatsApp: (505) 8877-6655<br/>
                                Email: info@optica2020.com
                            </p>
                        </div>
                    </div>";
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(correoEmisor, claveAplicacion);
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return true;
            }
            catch
            {
                return false;
            }
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
                    EjecutarSQL("UPDATE entrega SET Responsable=@New WHERE Responsable=@Old", valor, anterior);
                    CargarCombos();
                    ddlResponsable.SelectedValue = valor;
                    CargarDatos();
                    MostrarMensaje("Responsable actualizado exitosamente.", "success");
                }
            }
            else if (action == "DELETE")
            {
                EjecutarSQL("UPDATE entrega SET Responsable='ANULADO' WHERE Responsable=@Old", "", valor);
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
                        string sql = @"INSERT INTO entrega (ID_Venta, Fecha, Responsable, Observaciones, FechaRegistro, Estado, EstadoLente) 
                                       VALUES (@Venta, @Fecha, @Resp, @Obs, CURRENT_DATE, 1, @EstLente)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Venta", ddlVenta.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fecha", ConvertirFechaMySQL(txtFechaEntrega.Text));
                        cmd.Parameters.AddWithValue("@Resp", ddlResponsable.SelectedValue);
                        cmd.Parameters.AddWithValue("@Obs", txtObservaciones.Text.Trim());
                        cmd.Parameters.AddWithValue("@EstLente", ddlEstadoLente.SelectedValue);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Entrega registrada exitosamente.", "success");
                    }
                    else
                    {
                        string sql = @"UPDATE entrega SET ID_Venta=@Venta, Fecha=@Fecha, Responsable=@Resp, 
                                       Observaciones=@Obs, FechaRegistro=@FecReg, EstadoLente=@EstLente 
                                       WHERE ID_Entrega=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Venta", ddlVenta.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fecha", ConvertirFechaMySQL(txtFechaEntrega.Text));
                        cmd.Parameters.AddWithValue("@Resp", ddlResponsable.SelectedValue);
                        cmd.Parameters.AddWithValue("@Obs", txtObservaciones.Text.Trim());
                        cmd.Parameters.AddWithValue("@FecReg", ConvertirFechaMySQL(txtFechaRegistro.Text));
                        cmd.Parameters.AddWithValue("@EstLente", ddlEstadoLente.SelectedValue);
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
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM entrega WHERE ID_Entrega=@ID", con);
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

                        if (r["EstadoLente"] != DBNull.Value)
                        {
                            string est = r["EstadoLente"].ToString();
                            if (ddlEstadoLente.Items.FindByValue(est) != null)
                            {
                                ddlEstadoLente.SelectedValue = est;
                            }
                        }
                        ddlEstadoLente_SelectedIndexChanged(null, null);

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
                    MySqlCommand cmd = new MySqlCommand("UPDATE entrega SET Estado=@Est WHERE ID_Entrega=@ID", con);
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
                        MySqlCommand cmd = new MySqlCommand("SELECT EstadoPagoVenta FROM venta WHERE ID_Venta = @ID", con);
                        cmd.Parameters.AddWithValue("@ID", ddlVenta.SelectedValue);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result.ToString() != "Cancelado")
                        {
                            MostrarError(errVenta, "La venta tiene saldo pendiente. No se puede entregar.");
                            v = false;
                        }

                        string sqlDup = "SELECT COUNT(*) FROM entrega WHERE ID_Venta = @IDVenta AND Estado = 1";
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
            ddlEstadoLente.SelectedIndex = 0;
            divNotificar.Visible = false;
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