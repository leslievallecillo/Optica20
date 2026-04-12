using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Cita : System.Web.UI.Page
    {
        private string ID_USUARIO_ACTUAL
        {
            get
            {
                if (Session["ID_Usuario"] != null) return Session["ID_Usuario"].ToString();
                return null;
            }
        }

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

                    string qClientes = @"SELECT c.ID_Cliente, 
                                        CASE 
                                            WHEN c.TipoCliente = 'Natural' THEN (SELECT CONCAT(Nombre, ' ', Apellido) FROM ClienteNatural WHERE ID_Cliente = c.ID_Cliente)
                                            WHEN c.TipoCliente = 'Juridico' THEN (SELECT NombreEmpresa FROM ClienteJuridico WHERE ID_Cliente = c.ID_Cliente)
                                            ELSE 'Cliente Sin Datos'
                                        END as NombreDisplay
                                        FROM Clientes c 
                                        WHERE c.Estado = 1 
                                        ORDER BY NombreDisplay ASC";

                    MySqlDataAdapter daC = new MySqlDataAdapter(qClientes, con);
                    DataTable dtC = new DataTable();
                    daC.Fill(dtC);

                    ddlCliente.DataSource = dtC;
                    ddlCliente.DataTextField = "NombreDisplay";
                    ddlCliente.DataValueField = "ID_Cliente";
                    ddlCliente.DataBind();
                    ddlCliente.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

                    string qUsuarios = "SELECT ID_Usuario, Nombres FROM Usuario WHERE Estado = 1 ORDER BY Nombres ASC";
                    MySqlDataAdapter daU = new MySqlDataAdapter(qUsuarios, con);
                    DataTable dtU = new DataTable();
                    daU.Fill(dtU);

                    ddlUsuario.DataSource = dtU;
                    ddlUsuario.DataTextField = "Nombres";
                    ddlUsuario.DataValueField = "ID_Usuario";
                    ddlUsuario.DataBind();
                    ddlUsuario.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error listas: " + ex.Message, "error");
            }
        }

        private void CargarDatos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = @"SELECT cit.ID_Cita, cit.Fecha, cit.Hora, cit.Motivo, cit.FechaRegistro, cit.Estado,
                                    CASE 
                                        WHEN c.TipoCliente = 'Natural' THEN CONCAT(cn.Nombre, ' ', cn.Apellido)
                                        WHEN c.TipoCliente = 'Juridico' THEN cj.NombreEmpresa
                                        ELSE '---'
                                    END as NombreCliente,
                                    u.Nombres as NombreUsuario
                                    FROM Cita cit
                                    INNER JOIN Clientes c ON cit.ID_Cliente = c.ID_Cliente
                                    LEFT JOIN ClienteNatural cn ON c.ID_Cliente = cn.ID_Cliente
                                    LEFT JOIN ClienteJuridico cj ON c.ID_Cliente = cj.ID_Cliente
                                    INNER JOIN Usuario u ON cit.ID_Usuario = u.ID_Usuario
                                    WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                    {
                        query += @" AND (cit.Motivo LIKE @Busq 
                                   OR cn.Nombre LIKE @Busq 
                                   OR cn.Apellido LIKE @Busq 
                                   OR cj.NombreEmpresa LIKE @Busq) ";
                    }

                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND cit.Estado = @Estado ";

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                        query += " AND cit.Fecha BETWEEN @FecIni AND @FecFin ";

                    query += " ORDER BY cit.Fecha DESC, cit.Hora ASC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", ConvertirFechaMySQL(txtFiltroFechaInicio.Text));
                    cmd.Parameters.AddWithValue("@FecFin", ConvertirFechaMySQL(txtFiltroFechaFin.Text));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvCitas.DataSource = dt;
                    gvCitas.DataBind();

                    pnlMensajeGrid.Visible = false;
                    if (ddlPageSize.SelectedValue != "All")
                    {
                        int requestedSize = int.Parse(ddlPageSize.SelectedValue);
                        if (dt.Rows.Count > 0 && dt.Rows.Count < requestedSize)
                        {
                            lblMensajeGrid.Text = $"Nota: Solicitó ver {requestedSize} registros, pero solo existen {dt.Rows.Count}.";
                            pnlMensajeGrid.Visible = true;
                        }
                    }
                    if (dt.Rows.Count == 0)
                    {
                        lblMensajeGrid.Text = "No hay registros disponibles.";
                        pnlMensajeGrid.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error grid: " + ex.Message, "error");
            }
        }

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedValue = "-1";
            txtFiltroFechaInicio.Text = "";
            txtFiltroFechaFin.Text = "";
            ddlPageSize.SelectedValue = "All";
            gvCitas.AllowPaging = false;
            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvCitas.AllowPaging = false;
            else { gvCitas.AllowPaging = true; gvCitas.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            if (ddlCliente.Items.Count <= 1)
            {
                MostrarMensaje("No hay clientes registrados.", "warning");
                return;
            }

            LimpiarFormulario();
            PanelListado.Visible = false;
            PanelMantenimiento.Visible = true;
            lblTituloAccion.Text = "Programar Nueva Cita";

            if (!string.IsNullOrEmpty(ID_USUARIO_ACTUAL) && ddlUsuario.Items.FindByValue(ID_USUARIO_ACTUAL) != null)
            {
                ddlUsuario.SelectedValue = ID_USUARIO_ACTUAL;
            }

            ddlUsuario.Enabled = true;

            txtFechaRegistro.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtFechaRegistro.Enabled = false;
            txtFechaCita.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        }

        protected void gvCitas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar") CargarParaEditar(Convert.ToInt32(e.CommandArgument));
            else if (e.CommandName == "DarBaja") CambiarEstadoCita(Convert.ToInt32(e.CommandArgument), 0);
        }

        private void CargarParaEditar(int idCita)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Cita WHERE ID_Cita = @ID", con);
                    cmd.Parameters.AddWithValue("@ID", idCita);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        hfIDCita.Value = idCita.ToString();

                        string dbIdCliente = reader["ID_Cliente"].ToString();
                        if (ddlCliente.Items.FindByValue(dbIdCliente) != null)
                            ddlCliente.SelectedValue = dbIdCliente;
                        else
                            ddlCliente.SelectedIndex = 0;

                        string dbIdUsuario = reader["ID_Usuario"].ToString();
                        if (ddlUsuario.Items.FindByValue(dbIdUsuario) != null)
                            ddlUsuario.SelectedValue = dbIdUsuario;

                        ddlUsuario.Enabled = true;

                        if (reader["Fecha"] != DBNull.Value)
                            txtFechaCita.Text = Convert.ToDateTime(reader["Fecha"]).ToString("yyyy-MM-dd");

                        if (reader["Hora"] != DBNull.Value)
                            SetHoraUI((TimeSpan)reader["Hora"]);

                        // Cargar los motivos guardados
                        string motivosGuardados = reader["Motivo"].ToString();
                        CargarMotivosDesdeTexto(motivosGuardados);

                        if (reader["FechaRegistro"] != DBNull.Value)
                            txtFechaRegistro.Text = Convert.ToDateTime(reader["FechaRegistro"]).ToString("yyyy-MM-dd");

                        txtFechaRegistro.Enabled = true;

                        PanelListado.Visible = false;
                        PanelMantenimiento.Visible = true;
                        lblTituloAccion.Text = "Modificar Cita";
                        LimpiarErrores();
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarEntradas()) return;

            string motivosTexto = ObtenerMotivosTexto();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd;
                    TimeSpan horaFinal = ObtenerHoraFormatoSQL();

                    if (string.IsNullOrEmpty(hfIDCita.Value))
                    {
                        string insert = @"INSERT INTO Cita (ID_Cliente, ID_Usuario, Fecha, Hora, Motivo, FechaRegistro, Estado) 
                                          VALUES (@Cli, @Usu, @Fecha, @Hora, @Motivo, CURRENT_DATE, 1)";
                        cmd = new MySqlCommand(insert, con);
                        cmd.Parameters.AddWithValue("@Cli", ddlCliente.SelectedValue);
                        cmd.Parameters.AddWithValue("@Usu", ddlUsuario.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fecha", ConvertirFechaMySQL(txtFechaCita.Text));
                        cmd.Parameters.AddWithValue("@Hora", horaFinal);
                        cmd.Parameters.AddWithValue("@Motivo", motivosTexto);

                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Cita creada.", "success");
                    }
                    else
                    {
                        string update = @"UPDATE Cita SET ID_Cliente=@Cli, ID_Usuario=@Usu, Fecha=@Fecha, Hora=@Hora, 
                                          Motivo=@Motivo, FechaRegistro=@FecReg 
                                          WHERE ID_Cita=@ID";
                        cmd = new MySqlCommand(update, con);
                        cmd.Parameters.AddWithValue("@Cli", ddlCliente.SelectedValue);
                        cmd.Parameters.AddWithValue("@Usu", ddlUsuario.SelectedValue);
                        cmd.Parameters.AddWithValue("@Fecha", ConvertirFechaMySQL(txtFechaCita.Text));
                        cmd.Parameters.AddWithValue("@Hora", horaFinal);
                        cmd.Parameters.AddWithValue("@Motivo", motivosTexto);
                        cmd.Parameters.AddWithValue("@FecReg", ConvertirFechaMySQL(txtFechaRegistro.Text));
                        cmd.Parameters.AddWithValue("@ID", hfIDCita.Value);

                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Cita actualizada.", "success");
                    }

                    PanelListado.Visible = true;
                    PanelMantenimiento.Visible = false;
                    LimpiarErrores();
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error guardar: " + ex.Message, "error"); }
        }

        private bool ValidarEntradas()
        {
            bool esValido = true;
            LimpiarErrores();

            if (ddlCliente.SelectedValue == "0" || string.IsNullOrEmpty(ddlCliente.SelectedValue))
            {
                MostrarError(errCliente, "Requerido.");
                ddlCliente.CssClass += " is-invalid";
                esValido = false;
            }

            if (ddlUsuario.SelectedValue == "0" || string.IsNullOrEmpty(ddlUsuario.SelectedValue))
            {
                MostrarError(errUsuario, "Requerido.");
                ddlUsuario.CssClass += " is-invalid";
                esValido = false;
            }

            if (DateTime.TryParse(txtFechaCita.Text, out DateTime fechaCita))
            {
                if (fechaCita.Date < DateTime.Today)
                {
                    MostrarError(errFechaCita, "No puede ser pasado.");
                    txtFechaCita.CssClass += " is-invalid";
                    esValido = false;
                }
                else if (fechaCita.Date > DateTime.Today.AddDays(7))
                {
                    MostrarError(errFechaCita, "Max 7 días futuro.");
                    txtFechaCita.CssClass += " is-invalid";
                    esValido = false;
                }
            }
            else { MostrarError(errFechaCita, "Fecha inválida."); txtFechaCita.CssClass += " is-invalid"; esValido = false; }

            string horaRaw = txtHoraNum.Text.Trim();
            if (!Regex.IsMatch(horaRaw, @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$"))
            {
                MostrarError(errHora, "Formato HH:mm.");
                txtHoraNum.CssClass += " is-invalid";
                esValido = false;
            }
            else
            {
                TimeSpan hora = ObtenerHoraFormatoSQL();
                if (hora < new TimeSpan(8, 0, 0) || hora > new TimeSpan(18, 0, 0))
                {
                    MostrarError(errHora, "08:00 AM - 06:00 PM.");
                    txtHoraNum.CssClass += " is-invalid";
                    esValido = false;
                }
            }

            // Validación para motivos (NUEVO)
            string motivosTexto = ObtenerMotivosTexto();
            if (string.IsNullOrWhiteSpace(motivosTexto))
            {
                MostrarError(errMotivo, "Debe seleccionar al menos un motivo.");
                esValido = false;
            }
            else if (chkOtros.Checked && string.IsNullOrWhiteSpace(txtOtrosMotivo.Text))
            {
                MostrarError(errMotivo, "Si selecciona 'Otros', debe especificar el motivo.");
                esValido = false;
            }
            else if (chkOtros.Checked && !Regex.IsMatch(txtOtrosMotivo.Text.Trim(), @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ0-9\s,.-]+$"))
            {
                MostrarError(errMotivo, "El motivo 'Otros' contiene caracteres inválidos.");
                esValido = false;
            }

            if (!string.IsNullOrEmpty(hfIDCita.Value))
            {
                if (DateTime.TryParse(txtFechaRegistro.Text, out DateTime fechaReg))
                {
                    if (fechaReg < new DateTime(2023, 6, 1)) { MostrarError(errFechaRegistro, "Fecha antigua."); esValido = false; }
                    if (fechaReg.Date > DateTime.Now.Date) { MostrarError(errFechaRegistro, "Fecha futura."); esValido = false; }
                }
                else { MostrarError(errFechaRegistro, "Inválida."); esValido = false; }
            }

            return esValido;
        }

        private bool TieneCaracteresRepetidos(string texto)
        {
            return Regex.IsMatch(texto, @"(.)\1\1");
        }

        private void MostrarError(Label lbl, string msj) { if (lbl != null) { lbl.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {msj}"; lbl.Visible = true; } }

        private void LimpiarErrores()
        {
            if (errCliente != null) errCliente.Visible = false;
            if (errUsuario != null) errUsuario.Visible = false;
            if (errFechaCita != null) errFechaCita.Visible = false;
            if (errHora != null) errHora.Visible = false;
            if (errMotivo != null) errMotivo.Visible = false;
            if (errFechaRegistro != null) errFechaRegistro.Visible = false;
            QuitarMarcas();
        }

        private void QuitarMarcas()
        {
            ddlCliente.CssClass = ddlCliente.CssClass.Replace(" is-invalid", "");
            ddlUsuario.CssClass = ddlUsuario.CssClass.Replace(" is-invalid", "");
            txtFechaCita.CssClass = txtFechaCita.CssClass.Replace(" is-invalid", "");
            txtHoraNum.CssClass = txtHoraNum.CssClass.Replace(" is-invalid", "");
        }

        private void LimpiarFormulario()
        {
            hfIDCita.Value = "";
            ddlCliente.SelectedIndex = 0;

            // Limpiar motivos
            chkExamenVisual.Checked = false;
            chkGraduacion.Checked = false;
            chkSeleccionMontura.Checked = false;
            chkAjusteLentes.Checked = false;
            chkReparacion.Checked = false;
            chkEntregaLentes.Checked = false;
            chkGarantia.Checked = false;
            chkAsesoria.Checked = false;
            chkSeguro.Checked = false;
            chkLimpieza.Checked = false;
            chkSegundaOpinion.Checked = false;
            chkControl.Checked = false;
            chkDolorOcular.Checked = false;
            chkReceta.Checked = false;
            chkProteccion.Checked = false;
            chkOtros.Checked = false;
            txtOtrosMotivo.Text = "";

            if (!string.IsNullOrEmpty(ID_USUARIO_ACTUAL) && ddlUsuario.Items.FindByValue(ID_USUARIO_ACTUAL) != null)
                ddlUsuario.SelectedValue = ID_USUARIO_ACTUAL;

            txtFechaCita.Text = "";
            txtHoraNum.Text = "";
            ddlAmPm.SelectedIndex = 0;
            txtFechaRegistro.Text = "";
            LimpiarErrores();
        }

        private string ConvertirFechaMySQL(string fechaRaw)
        {
            if (DateTime.TryParse(fechaRaw, out DateTime fecha)) return fecha.ToString("yyyy-MM-dd");
            return fechaRaw;
        }

        private TimeSpan ObtenerHoraFormatoSQL()
        {
            string[] partes = txtHoraNum.Text.Trim().Split(':');
            int horas = int.Parse(partes[0]);
            int minutos = int.Parse(partes[1]);
            if (ddlAmPm.SelectedValue == "PM" && horas < 12) horas += 12;
            if (ddlAmPm.SelectedValue == "AM" && horas == 12) horas = 0;
            return new TimeSpan(horas, minutos, 0);
        }

        private void SetHoraUI(TimeSpan ts)
        {
            DateTime dt = DateTime.Today.Add(ts);
            txtHoraNum.Text = dt.ToString("hh:mm");
            ddlAmPm.SelectedValue = dt.ToString("tt", CultureInfo.InvariantCulture).ToUpper();
        }

        protected void btnBuscar_Click(object sender, EventArgs e) { CargarDatos(); }
        protected void btnCancelar_Click(object sender, EventArgs e) { PanelListado.Visible = true; PanelMantenimiento.Visible = false; LimpiarErrores(); }
        protected void gvCitas_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvCitas.PageIndex = e.NewPageIndex; CargarDatos(); }

        private void CambiarEstadoCita(int idCita, int nuevoEstado)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Cita SET Estado = @Est WHERE ID_Cita = @ID", con);
                    cmd.Parameters.AddWithValue("@Est", nuevoEstado);
                    cmd.Parameters.AddWithValue("@ID", idCita);
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Estado actualizado.", "success");
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        private void MostrarMensaje(string texto, string icono)
        {
            string script = $"Swal.fire('Gestión de Citas', '{texto}', '{icono}');";
            ScriptManager.RegisterStartupScript(this, GetType(), "Toast", script, true);
        }

        // NUEVOS MÉTODOS PARA MANEJO DE MOTIVOS
        private string ObtenerMotivosTexto()
        {
            System.Collections.Generic.List<string> motivos = new System.Collections.Generic.List<string>();

            var motivosMap = new System.Collections.Generic.Dictionary<CheckBox, string>
            {
                { chkExamenVisual, "Examen Visual" },
                { chkGraduacion, "Actualización de Graduación" },
                { chkSeleccionMontura, "Selección de Montura" },
                { chkAjusteLentes, "Ajuste de Lentes" },
                { chkReparacion, "Reparación de Gafas" },
                { chkEntregaLentes, "Entrega de Lentes Terminados" },
                { chkGarantia, "Gestión de Garantía" },
                { chkAsesoria, "Asesoría en Lentes de Contacto" },
                { chkSeguro, "Gestión de Seguro Médico" },
                { chkLimpieza, "Limpieza y Mantenimiento" },
                { chkSegundaOpinion, "Segunda Opinión" },
                { chkControl, "Control de Rutina" },
                { chkDolorOcular, "Dolor Ocular / Molestias" },
                { chkReceta, "Actualización de Receta" },
                { chkProteccion, "Protección Visual" }
            };

            foreach (var kvp in motivosMap)
            {
                if (kvp.Key.Checked)
                {
                    motivos.Add(kvp.Value);
                }
            }

            if (chkOtros.Checked && !string.IsNullOrWhiteSpace(txtOtrosMotivo.Text))
            {
                motivos.Add("Otros: " + txtOtrosMotivo.Text.Trim());
            }

            return motivos.Count > 0 ? string.Join(", ", motivos) : "";
        }

        private void CargarMotivosDesdeTexto(string motivosTexto)
        {
            // Limpiar todos los checkboxes
            chkExamenVisual.Checked = false;
            chkGraduacion.Checked = false;
            chkSeleccionMontura.Checked = false;
            chkAjusteLentes.Checked = false;
            chkReparacion.Checked = false;
            chkEntregaLentes.Checked = false;
            chkGarantia.Checked = false;
            chkAsesoria.Checked = false;
            chkSeguro.Checked = false;
            chkLimpieza.Checked = false;
            chkSegundaOpinion.Checked = false;
            chkControl.Checked = false;
            chkDolorOcular.Checked = false;
            chkReceta.Checked = false;
            chkProteccion.Checked = false;
            chkOtros.Checked = false;
            txtOtrosMotivo.Text = "";

            if (string.IsNullOrWhiteSpace(motivosTexto)) return;

            string[] motivos = motivosTexto.Split(new[] { ", " }, StringSplitOptions.None);

            foreach (string motivo in motivos)
            {
                if (motivo.Contains("Otros:"))
                {
                    chkOtros.Checked = true;
                    txtOtrosMotivo.Text = motivo.Replace("Otros: ", "").Trim();
                }
                else
                {
                    switch (motivo)
                    {
                        case "Examen Visual": chkExamenVisual.Checked = true; break;
                        case "Actualización de Graduación": chkGraduacion.Checked = true; break;
                        case "Selección de Montura": chkSeleccionMontura.Checked = true; break;
                        case "Ajuste de Lentes": chkAjusteLentes.Checked = true; break;
                        case "Reparación de Gafas": chkReparacion.Checked = true; break;
                        case "Entrega de Lentes Terminados": chkEntregaLentes.Checked = true; break;
                        case "Gestión de Garantía": chkGarantia.Checked = true; break;
                        case "Asesoría en Lentes de Contacto": chkAsesoria.Checked = true; break;
                        case "Gestión de Seguro Médico": chkSeguro.Checked = true; break;
                        case "Limpieza y Mantenimiento": chkLimpieza.Checked = true; break;
                        case "Segunda Opinión": chkSegundaOpinion.Checked = true; break;
                        case "Control de Rutina": chkControl.Checked = true; break;
                        case "Dolor Ocular / Molestias": chkDolorOcular.Checked = true; break;
                        case "Actualización de Receta": chkReceta.Checked = true; break;
                        case "Protección Visual": chkProteccion.Checked = true; break;
                    }
                }
            }
        }
    }
}