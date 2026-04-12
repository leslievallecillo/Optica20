using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;
using System.Globalization;
using System.IO;

namespace Optica.Formularios
{
    public partial class Venta : System.Web.UI.Page
    {
        private int IdUsuario
        {
            get { return Session["ID_Usuario"] != null ? Convert.ToInt32(Session["ID_Usuario"]) : 1; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Now.ToString("yyyy-MM-dd");

                try
                {
                    txtUsuarioForm.Text = Session["Usuario"] != null ? Session["Usuario"].ToString() : GetValor("SELECT CONCAT(Nombres, ' ', Apellidos) FROM Usuario WHERE ID_Usuario=" + IdUsuario);
                }
                catch
                {
                    txtUsuarioForm.Text = "";
                }

                CargarDatos();

                if (Request.QueryString["id"] != null)
                {
                    hfIdVenta.Value = Request.QueryString["id"];
                    CargarVenta(hfIdVenta.Value);
                }
                else
                {
                    GenerarFactura();
                }
            }
        }

        private void CargarVenta(string id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();

                    decimal dbMontoRecibido = 0;
                    decimal dbCambio = 0;
                    decimal dbTotal = 0;
                    string dbEstadoPagoVenta = "Cancelado";
                    decimal dbTotalSaldo = 0;
                    decimal dbMontoRecibidoSaldo = 0;
                    decimal dbCambioSaldo = 0;

                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Venta WHERE ID_Venta=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        lblTitulo.Text = "Modificar Venta #" + id;
                        lblNoFactura.Text = dr["NumeroDocumento"].ToString();

                        SetComboValue(ddlCliente, dr["ID_Cliente"].ToString());
                        CargarExpedientesCliente(dr["ID_Cliente"].ToString());

                        SetComboValue(ddlTipoDoc, dr["ID_TipoDocumento"].ToString());
                        SetComboValue(ddlTipoPago, dr["ID_TipoPago"].ToString());
                        txtFecha.Text = Convert.ToDateTime(dr["Fecha"]).ToString("yyyy-MM-dd");

                        if (dr["MontoRecibido"] != DBNull.Value) dbMontoRecibido = Convert.ToDecimal(dr["MontoRecibido"]);
                        if (dr["Cambio"] != DBNull.Value) dbCambio = Convert.ToDecimal(dr["Cambio"]);
                        if (dr["Total"] != DBNull.Value) dbTotal = Convert.ToDecimal(dr["Total"]);

                        if (dr["EstadoPagoVenta"] != DBNull.Value) dbEstadoPagoVenta = dr["EstadoPagoVenta"].ToString();
                        if (dr["TotalSaldo"] != DBNull.Value) dbTotalSaldo = Convert.ToDecimal(dr["TotalSaldo"]);
                        if (dr["MontoRecibidoSaldo"] != DBNull.Value) dbMontoRecibidoSaldo = Convert.ToDecimal(dr["MontoRecibidoSaldo"]);
                        if (dr["CambioSaldo"] != DBNull.Value) dbCambioSaldo = Convert.ToDecimal(dr["CambioSaldo"]);

                        btnFinalizar.Text = "Actualizar Venta";
                    }
                    dr.Close();

                    MySqlCommand cmdP = new MySqlCommand("SELECT * FROM DetalleVentaProducto WHERE ID_Venta=@ID", con);
                    cmdP.Parameters.AddWithValue("@ID", id);
                    DataTable dtP = new DataTable();
                    new MySqlDataAdapter(cmdP).Fill(dtP);
                    if (dtP.Rows.Count > 0)
                    {
                        chkProducto.Checked = true;
                        pnlProducto.Visible = true;
                        SetComboValue(ddlProducto, dtP.Rows[0]["ID_Producto"].ToString());
                        txtCantProd.Text = dtP.Rows[0]["Cantidad"].ToString();
                        decimal sub = Convert.ToDecimal(dtP.Rows[0]["Subtotal"]);
                        decimal cant = Convert.ToDecimal(dtP.Rows[0]["Cantidad"]);
                        if (cant > 0)
                        {
                            txtPrecioProd.Text = (sub / cant).ToString("0.00", CultureInfo.InvariantCulture);
                        }
                    }

                    MySqlCommand cmdL = new MySqlCommand("SELECT * FROM DetalleVentaLentes WHERE ID_Venta=@ID", con);
                    cmdL.Parameters.AddWithValue("@ID", id);
                    DataTable dtL = new DataTable();
                    new MySqlDataAdapter(cmdL).Fill(dtL);
                    if (dtL.Rows.Count > 0)
                    {
                        chkLente.Checked = true;
                        pnlLente.Visible = true;
                        SetComboValue(ddlExpediente, dtL.Rows[0]["ID_Expediente"].ToString());
                        txtSubtotalLente.Text = Convert.ToDecimal(dtL.Rows[0]["Subtotal"]).ToString("0.00", CultureInfo.InvariantCulture);
                    }

                    MySqlCommand cmdPag = new MySqlCommand("SELECT * FROM Pago WHERE ID_Venta=@ID ORDER BY ID_Pago DESC LIMIT 1", con);
                    cmdPag.Parameters.AddWithValue("@ID", id);
                    DataTable dtPag = new DataTable();
                    new MySqlDataAdapter(cmdPag).Fill(dtPag);
                    if (dtPag.Rows.Count > 0)
                    {
                        txtMontoPagar.Text = Convert.ToDecimal(dtPag.Rows[0]["Monto"]).ToString("0.00", CultureInfo.InvariantCulture);
                        txtDescPago.Text = dtPag.Rows[0]["Descripcion"].ToString();
                    }

                    Recalcular_Event(null, null);

                    txtPagaCon.Text = dbMontoRecibido.ToString("0.00", CultureInfo.InvariantCulture);
                    txtVuelto.Text = dbCambio.ToString("0.00", CultureInfo.InvariantCulture);
                    txtTotalVenta.Text = dbTotal.ToString("0.00", CultureInfo.InvariantCulture);

                    SetComboValue(ddlEstadoPagoVenta, dbEstadoPagoVenta);
                    txtTotalSaldo.Text = dbTotalSaldo.ToString("0.00", CultureInfo.InvariantCulture);
                    txtMontoRecibidoSaldo.Text = dbMontoRecibidoSaldo.ToString("0.00", CultureInfo.InvariantCulture);
                    txtCambioSaldo.Text = dbCambioSaldo.ToString("0.00", CultureInfo.InvariantCulture);
                }
            }
            catch { }
        }

        private void SetComboValue(DropDownList ddl, string value)
        {
            ddl.ClearSelection();
            ListItem item = ddl.Items.FindByValue(value);
            if (item != null)
            {
                item.Selected = true;
            }
        }

        protected void btnFinalizar_Click(object sender, EventArgs e)
        {
            LimpiarErrores();
            bool esValido = true;

            if (ddlCliente.SelectedValue == "0") esValido = MarcarError(ddlCliente, errCliente, "Seleccione un cliente.");
            if (ddlTipoDoc.SelectedValue == "0") esValido = MarcarError(ddlTipoDoc, errTipoDoc, "Requerido.");
            if (ddlTipoPago.SelectedValue == "0") esValido = MarcarError(ddlTipoPago, errTipoPago, "Seleccione forma de pago.");

            if (!chkProducto.Checked && !chkLente.Checked)
            {
                errSeccion.Text = "<i class='fa-solid fa-circle-exclamation'></i> Debe activar Producto o Lente.";
                errSeccion.Visible = true;
                esValido = false;
            }

            if (chkProducto.Checked)
            {
                if (ddlProducto.SelectedValue == "0") esValido = MarcarError(ddlProducto, errProducto, "Seleccione un producto.");
                if (!decimal.TryParse(txtCantProd.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal c) || c <= 0) esValido = MarcarError(txtCantProd, errCantProd, "Cantidad inválida.");
            }

            if (chkLente.Checked)
            {
                if (ddlExpediente.SelectedValue == "0") esValido = MarcarError(ddlExpediente, errExpediente, "Seleccione un expediente.");
            }

            if (!decimal.TryParse(txtTotalVenta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal total) || total <= 0)
            {
                errTotal.Text = "El total no puede ser 0."; errTotal.Visible = true; esValido = false;
            }

            decimal aPagar = 0, pagaCon = 0;
            decimal.TryParse(txtMontoPagar.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out aPagar);

            if (string.IsNullOrEmpty(txtPagaCon.Text))
            {
                esValido = MarcarError(txtPagaCon, errPagaCon, "Ingrese monto.");
            }
            else
            {
                if (!decimal.TryParse(txtPagaCon.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out pagaCon)) esValido = MarcarError(txtPagaCon, errPagaCon, "Solo números.");
                else if (pagaCon < aPagar) esValido = MarcarError(txtPagaCon, errPagaCon, "Pago insuficiente, debe cubrir el monto a pagar.");
            }

            if (!string.IsNullOrWhiteSpace(txtDescPago.Text))
            {
                string nota = txtDescPago.Text.Trim();
                if (!Regex.IsMatch(nota, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]+$")) esValido = MarcarError(txtDescPago, errNota, "Solo permitido letras.");
            }

            if (chkLente.Checked && ddlEstadoPagoVenta.SelectedValue == "Cancelado")
            {
                decimal saldo = 0, recSaldo = 0;
                decimal.TryParse(txtTotalSaldo.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out saldo);

                if (string.IsNullOrEmpty(txtMontoRecibidoSaldo.Text))
                {
                    esValido = MarcarError(txtMontoRecibidoSaldo, errMontoRecibidoSaldo, "Ingrese monto recibido.");
                }
                else
                {
                    if (!decimal.TryParse(txtMontoRecibidoSaldo.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out recSaldo))
                        esValido = MarcarError(txtMontoRecibidoSaldo, errMontoRecibidoSaldo, "Solo números.");
                    else if (recSaldo < saldo)
                        esValido = MarcarError(txtMontoRecibidoSaldo, errMontoRecibidoSaldo, "Pago insuficiente para el saldo.");
                }
            }

            if (esValido)
            {
                GuardarTransaccion();
            }
            else
            {
                MostrarAlerta("Corrija los campos marcados en rojo.");
            }
        }

        private bool MarcarError(WebControl ctrl, Label lbl, string msj)
        {
            ctrl.CssClass += " is-invalid";
            lbl.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {msj}";
            lbl.Visible = true;
            return false;
        }

        private void LimpiarErrores()
        {
            ddlCliente.CssClass = ddlCliente.CssClass.Replace(" is-invalid", "");
            ddlTipoDoc.CssClass = ddlTipoDoc.CssClass.Replace(" is-invalid", "");
            ddlTipoPago.CssClass = ddlTipoPago.CssClass.Replace(" is-invalid", "");
            ddlProducto.CssClass = ddlProducto.CssClass.Replace(" is-invalid", "");
            txtCantProd.CssClass = txtCantProd.CssClass.Replace(" is-invalid", "");
            ddlExpediente.CssClass = ddlExpediente.CssClass.Replace(" is-invalid", "");
            txtPagaCon.CssClass = txtPagaCon.CssClass.Replace(" is-invalid", "");
            txtDescPago.CssClass = txtDescPago.CssClass.Replace(" is-invalid", "");
            txtMontoRecibidoSaldo.CssClass = txtMontoRecibidoSaldo.CssClass.Replace(" is-invalid", "");

            errCliente.Visible = false; errTipoDoc.Visible = false; errTipoPago.Visible = false;
            errSeccion.Visible = false; errProducto.Visible = false; errCantProd.Visible = false;
            errExpediente.Visible = false; errTotal.Visible = false; errPagaCon.Visible = false; errNota.Visible = false;
            errMontoRecibidoSaldo.Visible = false;
        }

        private void GuardarTransaccion()
        {
            bool esEdicion = !string.IsNullOrEmpty(hfIdVenta.Value);
            bool exito = false;
            int idVenta = 0;

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlTransaction tran = con.BeginTransaction();
                try
                {
                    string epv = chkLente.Checked ? ddlEstadoPagoVenta.SelectedValue : "Cancelado";
                    decimal ts = chkLente.Checked ? decimal.Parse(txtTotalSaldo.Text, CultureInfo.InvariantCulture) : 0;
                    decimal mrs = chkLente.Checked ? decimal.Parse(string.IsNullOrEmpty(txtMontoRecibidoSaldo.Text) ? "0" : txtMontoRecibidoSaldo.Text, CultureInfo.InvariantCulture) : 0;
                    decimal cs = chkLente.Checked ? decimal.Parse(string.IsNullOrEmpty(txtCambioSaldo.Text) ? "0" : txtCambioSaldo.Text, CultureInfo.InvariantCulture) : 0;

                    if (esEdicion)
                    {
                        RestaurarStock(hfIdVenta.Value, con, tran);
                        new MySqlCommand($"DELETE FROM DetalleVentaProducto WHERE ID_Venta={hfIdVenta.Value}", con, tran).ExecuteNonQuery();
                        new MySqlCommand($"DELETE FROM DetalleVentaLentes WHERE ID_Venta={hfIdVenta.Value}", con, tran).ExecuteNonQuery();
                        new MySqlCommand($"DELETE FROM Pago WHERE ID_Venta={hfIdVenta.Value}", con, tran).ExecuteNonQuery();

                        string upV = "UPDATE Venta SET ID_Cliente=@Cli, ID_TipoDocumento=@Doc, ID_TipoPago=@Pago, Fecha=@Fec, MontoRecibido=@MR, Cambio=@Cam, Total=@Tot, EstadoPagoVenta=@EPV, TotalSaldo=@TS, MontoRecibidoSaldo=@MRS, CambioSaldo=@CS WHERE ID_Venta=@ID";
                        MySqlCommand cmdUp = new MySqlCommand(upV, con, tran);
                        cmdUp.Parameters.AddWithValue("@Cli", ddlCliente.SelectedValue);
                        cmdUp.Parameters.AddWithValue("@Doc", ddlTipoDoc.SelectedValue);
                        cmdUp.Parameters.AddWithValue("@Pago", ddlTipoPago.SelectedValue);
                        cmdUp.Parameters.AddWithValue("@Fec", DateTime.Now);
                        cmdUp.Parameters.AddWithValue("@MR", decimal.Parse(txtPagaCon.Text, CultureInfo.InvariantCulture));
                        cmdUp.Parameters.AddWithValue("@Cam", decimal.Parse(txtVuelto.Text, CultureInfo.InvariantCulture));
                        cmdUp.Parameters.AddWithValue("@Tot", decimal.Parse(txtTotalVenta.Text, CultureInfo.InvariantCulture));
                        cmdUp.Parameters.AddWithValue("@EPV", epv);
                        cmdUp.Parameters.AddWithValue("@TS", ts);
                        cmdUp.Parameters.AddWithValue("@MRS", mrs);
                        cmdUp.Parameters.AddWithValue("@CS", cs);
                        cmdUp.Parameters.AddWithValue("@ID", hfIdVenta.Value);
                        cmdUp.ExecuteNonQuery();
                    }
                    else
                    {
                        string sqlV = @"INSERT INTO Venta (ID_Cliente, ID_TipoDocumento, NumeroDocumento, ID_TipoPago, ID_Usuario, Fecha, MontoRecibido, Cambio, Total, Estado, EstadoPagoVenta, TotalSaldo, MontoRecibidoSaldo, CambioSaldo) 
                                        VALUES (@Cli, @Doc, @Num, @Pago, @User, @Fec, @MR, @Cam, @Tot, 1, @EPV, @TS, @MRS, @CS); SELECT LAST_INSERT_ID();";
                        MySqlCommand cmd = new MySqlCommand(sqlV, con, tran);
                        cmd.Parameters.AddWithValue("@Cli", ddlCliente.SelectedValue);
                        cmd.Parameters.AddWithValue("@Doc", ddlTipoDoc.SelectedValue);
                        cmd.Parameters.AddWithValue("@Num", lblNoFactura.Text);
                        cmd.Parameters.AddWithValue("@Pago", ddlTipoPago.SelectedValue);
                        cmd.Parameters.AddWithValue("@User", IdUsuario);
                        cmd.Parameters.AddWithValue("@Fec", DateTime.Now);
                        cmd.Parameters.AddWithValue("@MR", decimal.Parse(txtPagaCon.Text, CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@Cam", decimal.Parse(txtVuelto.Text, CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@Tot", decimal.Parse(txtTotalVenta.Text, CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@EPV", epv);
                        cmd.Parameters.AddWithValue("@TS", ts);
                        cmd.Parameters.AddWithValue("@MRS", mrs);
                        cmd.Parameters.AddWithValue("@CS", cs);
                        hfIdVenta.Value = cmd.ExecuteScalar().ToString();
                    }

                    idVenta = int.Parse(hfIdVenta.Value);

                    if (chkProducto.Checked)
                    {
                        decimal c = decimal.Parse(txtCantProd.Text, CultureInfo.InvariantCulture);
                        decimal p = decimal.Parse(txtPrecioProd.Text, CultureInfo.InvariantCulture);
                        string sqlP = "INSERT INTO DetalleVentaProducto (ID_Venta, ID_Producto, Cantidad, Subtotal, Estado) VALUES (@IdV, @IdP, @Cant, @Sub, 1)";
                        MySqlCommand cmdP = new MySqlCommand(sqlP, con, tran);
                        cmdP.Parameters.AddWithValue("@IdV", idVenta);
                        cmdP.Parameters.AddWithValue("@IdP", ddlProducto.SelectedValue);
                        cmdP.Parameters.AddWithValue("@Cant", c);
                        cmdP.Parameters.AddWithValue("@Sub", c * p);
                        cmdP.ExecuteNonQuery();

                        new MySqlCommand($"UPDATE Producto SET Stock = Stock - {c} WHERE ID_Producto={ddlProducto.SelectedValue}", con, tran).ExecuteNonQuery();
                    }

                    if (chkLente.Checked)
                    {
                        decimal sub = decimal.Parse(txtSubtotalLente.Text, CultureInfo.InvariantCulture);
                        decimal pagado = decimal.Parse(txtMontoPagar.Text, CultureInfo.InvariantCulture);
                        decimal adelanto = pagado;
                        if (chkProducto.Checked) adelanto -= (decimal.Parse(txtCantProd.Text, CultureInfo.InvariantCulture) * decimal.Parse(txtPrecioProd.Text, CultureInfo.InvariantCulture));

                        string sqlL = "INSERT INTO DetalleVentaLentes (ID_Venta, ID_Expediente, Adelanto, SaldoPendiente, EstadoPago, EstadoEntrega, Subtotal, Estado) VALUES (@IdV, @Exp, @Adel, @Saldo, @Est, 'Taller', @Sub, 1)";
                        MySqlCommand cmdL = new MySqlCommand(sqlL, con, tran);
                        cmdL.Parameters.AddWithValue("@IdV", idVenta);
                        cmdL.Parameters.AddWithValue("@Exp", ddlExpediente.SelectedValue);
                        cmdL.Parameters.AddWithValue("@Adel", adelanto);
                        cmdL.Parameters.AddWithValue("@Saldo", sub - adelanto);
                        cmdL.Parameters.AddWithValue("@Est", (epv == "Cancelado") ? "Pagado" : "Pendiente");
                        cmdL.Parameters.AddWithValue("@Sub", sub);
                        cmdL.ExecuteNonQuery();

                        string idProdLente = GetValor("SELECT ID_Producto FROM Expediente WHERE ID_Expediente=" + ddlExpediente.SelectedValue, con, tran);
                        if (!string.IsNullOrEmpty(idProdLente))
                            new MySqlCommand($"UPDATE Producto SET Stock = Stock - 1 WHERE ID_Producto={idProdLente}", con, tran).ExecuteNonQuery();
                    }

                    string sqlPag = "INSERT INTO Pago (ID_Venta, Monto, Fecha, ID_TipoPago, Descripcion, Estado) VALUES (@IdV, @Monto, @Fec, @Tp, @Desc, 1)";
                    MySqlCommand cmdPag = new MySqlCommand(sqlPag, con, tran);
                    cmdPag.Parameters.AddWithValue("@IdV", idVenta);
                    cmdPag.Parameters.AddWithValue("@Monto", decimal.Parse(txtMontoPagar.Text, CultureInfo.InvariantCulture));
                    cmdPag.Parameters.AddWithValue("@Fec", DateTime.Now);
                    cmdPag.Parameters.AddWithValue("@Tp", ddlTipoPago.SelectedValue);
                    cmdPag.Parameters.AddWithValue("@Desc", txtDescPago.Text);
                    cmdPag.ExecuteNonQuery();

                    tran.Commit();
                    exito = true;
                }
                catch (Exception ex)
                {
                    try { tran.Rollback(); } catch { }
                    MostrarAlerta("Error: " + ex.Message);
                }
            }

            if (exito)
            {
                Session["MsjExito"] = esEdicion ? "Venta actualizada correctamente." : "Venta registrada correctamente.";
                ScriptManager.RegisterStartupScript(this, GetType(), "alertaImpresion", $"mostrarAlertaImpresion({idVenta});", true);
            }
        }

        protected void btnImprimirOculto_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfIdImprimir.Value);
            try
            {
                string rutaLogo = Server.MapPath("~/Imagenes2/logo_optica.png.jpg");
                if (!File.Exists(rutaLogo)) rutaLogo = Server.MapPath("../Imagenes2/logo_optica.png.jpg");

                ReportePdfHelper pdfHelper = new ReportePdfHelper(rutaLogo);
                byte[] pdfBytes = pdfHelper.GenerarDocumentoVenta(id);

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"attachment; filename=Documento_Venta_{id}.pdf");
                Response.BinaryWrite(pdfBytes);
                Response.End();
            }
            catch (Exception ex)
            {
                MostrarAlerta("Error al generar PDF: " + ex.Message);
            }
        }

        private void RestaurarStock(string idVenta, MySqlConnection con, MySqlTransaction tran)
        {
            DataTable dtP = GetTable("SELECT ID_Producto, Cantidad FROM DetalleVentaProducto WHERE ID_Venta=" + idVenta, con, tran);
            foreach (DataRow r in dtP.Rows)
                new MySqlCommand($"UPDATE Producto SET Stock = Stock + {r["Cantidad"]} WHERE ID_Producto={r["ID_Producto"]}", con, tran).ExecuteNonQuery();

            DataTable dtL = GetTable("SELECT E.ID_Producto FROM DetalleVentaLentes D JOIN Expediente E ON D.ID_Expediente=E.ID_Expediente WHERE D.ID_Venta=" + idVenta, con, tran);
            foreach (DataRow r in dtL.Rows)
                if (r["ID_Producto"] != DBNull.Value) new MySqlCommand($"UPDATE Producto SET Stock = Stock + 1 WHERE ID_Producto={r["ID_Producto"]}", con, tran).ExecuteNonQuery();
        }

        protected void Recalcular_Event(object sender, EventArgs e)
        {
            pnlProducto.Visible = chkProducto.Checked;
            pnlLente.Visible = chkLente.Checked;

            decimal total = 0, aPagar = 0;
            decimal adelantoMostrar = 0;
            decimal saldoPendiente = 0;

            if (chkProducto.Checked)
            {
                decimal c = 0, p = 0;
                decimal.TryParse(txtCantProd.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out c);
                decimal.TryParse(txtPrecioProd.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out p);
                total += c * p;
                aPagar += c * p;
            }

            if (chkLente.Checked)
            {
                decimal s = 0;
                decimal.TryParse(txtSubtotalLente.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out s);
                total += s;
                decimal adelantoLente = s * 0.5m;
                aPagar += adelantoLente;
                adelantoMostrar = adelantoLente;
                saldoPendiente = s - adelantoLente;
                lblInfoPago.Text = "* Adelanto 50%";
            }
            else { lblInfoPago.Text = "* Pago Contado"; }

            decimal entrega = 0;
            decimal.TryParse(txtPagaCon.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out entrega);

            txtTotalVenta.Text = total.ToString("0.00", CultureInfo.InvariantCulture);
            txtMontoPagar.Text = aPagar.ToString("0.00", CultureInfo.InvariantCulture);
            txtAdelanto.Text = adelantoMostrar.ToString("0.00", CultureInfo.InvariantCulture);

            decimal vueltoVenta = entrega - aPagar;
            txtVuelto.Text = (vueltoVenta > 0 ? vueltoVenta : 0).ToString("0.00", CultureInfo.InvariantCulture);

            pnlSaldoLente.Visible = chkLente.Checked;

            if (chkLente.Checked)
            {
                txtTotalSaldo.Text = saldoPendiente.ToString("0.00", CultureInfo.InvariantCulture);

                decimal recibidoSaldo = 0;
                decimal.TryParse(txtMontoRecibidoSaldo.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out recibidoSaldo);

                if (ddlEstadoPagoVenta.SelectedValue == "Cancelado")
                {
                    if ((sender == ddlEstadoPagoVenta || sender == chkLente) && recibidoSaldo == 0)
                    {
                        recibidoSaldo = saldoPendiente;
                        txtMontoRecibidoSaldo.Text = saldoPendiente.ToString("0.00", CultureInfo.InvariantCulture);
                    }

                    decimal cambioS = recibidoSaldo - saldoPendiente;
                    txtCambioSaldo.Text = (cambioS > 0 ? cambioS : 0).ToString("0.00", CultureInfo.InvariantCulture);
                }
                else
                {
                    txtMontoRecibidoSaldo.Text = "0.00";
                    txtCambioSaldo.Text = "0.00";
                }
            }
            else
            {
                ddlEstadoPagoVenta.SelectedValue = "Cancelado";
                txtTotalSaldo.Text = "0.00";
                txtMontoRecibidoSaldo.Text = "0.00";
                txtCambioSaldo.Text = "0.00";
            }
        }

        private void CargarDatos()
        {
            try
            {
                using (MySqlConnection c = new MySqlConnection(Conexion.CadenaConexion))
                {
                    c.Open();
                    LlenarCombo(c, ddlCliente, "SELECT ID_Cliente, CASE WHEN TipoCliente='Natural' THEN CONCAT(IFNULL((SELECT Nombre FROM ClienteNatural WHERE ID_Cliente=Clientes.ID_Cliente),''),' ',IFNULL((SELECT Apellido FROM ClienteNatural WHERE ID_Cliente=Clientes.ID_Cliente),'')) ELSE (SELECT RepresentanteLegal FROM ClienteJuridico WHERE ID_Cliente=Clientes.ID_Cliente) END AS Nom FROM Clientes WHERE Estado=1", "Nom", "ID_Cliente");
                    LlenarCombo(c, ddlProducto, "SELECT ID_Producto, CONCAT(Descripcion, ' (Stock: ', Stock, ')') AS Info FROM Producto WHERE Estado=1", "Info", "ID_Producto");

                    ddlExpediente.Items.Clear();
                    ddlExpediente.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

                    LlenarCombo(c, ddlTipoDoc, "SELECT * FROM TipoDocumento WHERE Estado=1", "Descripcion", "ID_TipoDocumento");
                    LlenarCombo(c, ddlTipoPago, "SELECT * FROM TipoPago WHERE Estado=1", "Descripcion", "ID_TipoPago");
                }
            }
            catch { }
        }

        protected void ddlCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarExpedientesCliente(ddlCliente.SelectedValue);
        }

        private void CargarExpedientesCliente(string idCliente)
        {
            if (idCliente == "0" || string.IsNullOrEmpty(idCliente))
            {
                ddlExpediente.Items.Clear();
                ddlExpediente.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
                txtSubtotalLente.Text = "0.00";
                Recalcular_Event(null, null);
                return;
            }

            try
            {
                using (MySqlConnection c = new MySqlConnection(Conexion.CadenaConexion))
                {
                    c.Open();
                    LlenarCombo(c, ddlExpediente, "SELECT ID_Expediente, CONCAT('Exp #', ID_Expediente, ' - ', Fecha) AS Info FROM Expediente WHERE Estado=1 AND ID_Cliente=" + idCliente, "Info", "ID_Expediente");
                }

                if (ddlExpediente.Items.Count == 2)
                {
                    ddlExpediente.SelectedIndex = 1;
                    ddlExpediente_SelectedIndexChanged(null, null);
                }
                else
                {
                    txtSubtotalLente.Text = "0.00";
                    Recalcular_Event(null, null);
                }
            }
            catch { }
        }

        protected void ddlProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlProducto.SelectedValue != "0")
            {
                decimal p = GetDecimal("SELECT IFNULL(PrecioVenta, 0) FROM DetalleCompra WHERE ID_Producto=" + ddlProducto.SelectedValue + " ORDER BY ID_DetalleCompra DESC LIMIT 1");
                txtPrecioProd.Text = p.ToString("0.00", CultureInfo.InvariantCulture);
            }
            else txtPrecioProd.Text = "0.00";
            Recalcular_Event(sender, e);
        }

        protected void ddlExpediente_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlExpediente.SelectedValue != "0")
            {
                string idT = GetValor("SELECT ID_Tratamiento FROM Expediente WHERE ID_Expediente=" + ddlExpediente.SelectedValue);
                string idP = GetValor("SELECT ID_Producto FROM Expediente WHERE ID_Expediente=" + ddlExpediente.SelectedValue);
                decimal t = 0;
                if (!string.IsNullOrEmpty(idT)) t += GetDecimal("SELECT PrecioAdicional FROM Tratamiento WHERE ID_Tratamiento=" + idT);
                if (!string.IsNullOrEmpty(idP)) t += GetDecimal("SELECT IFNULL(PrecioVenta, 0) FROM DetalleCompra WHERE ID_Producto=" + idP + " ORDER BY ID_DetalleCompra DESC LIMIT 1");
                txtSubtotalLente.Text = t.ToString("0.00", CultureInfo.InvariantCulture);
            }
            else txtSubtotalLente.Text = "0.00";
            Recalcular_Event(sender, e);
        }

        protected void btnCancelar_Click(object sender, EventArgs e) { Response.Redirect("HistorialVentas.aspx"); }

        private void GenerarFactura() { try { using (MySqlConnection c = new MySqlConnection(Conexion.CadenaConexion)) { c.Open(); lblNoFactura.Text = (Convert.ToInt32(new MySqlCommand("SELECT IFNULL(MAX(ID_Venta),0)+1 FROM Venta", c).ExecuteScalar())).ToString("D4"); } } catch { lblNoFactura.Text = "0001"; } }

        private void LlenarCombo(MySqlConnection c, DropDownList d, string s, string t, string v) { MySqlDataAdapter a = new MySqlDataAdapter(s, c); DataTable dt = new DataTable(); a.Fill(dt); d.DataSource = dt; d.DataTextField = t; d.DataValueField = v; d.DataBind(); d.Items.Insert(0, new ListItem("-- Seleccione --", "0")); }
        private decimal GetDecimal(string s) { using (MySqlConnection c = new MySqlConnection(Conexion.CadenaConexion)) { c.Open(); object r = new MySqlCommand(s, c).ExecuteScalar(); return r != null ? Convert.ToDecimal(r) : 0; } }
        private string GetValor(string s, MySqlConnection c = null, MySqlTransaction t = null)
        {
            if (c == null) using (c = new MySqlConnection(Conexion.CadenaConexion)) { c.Open(); return new MySqlCommand(s, c).ExecuteScalar()?.ToString(); }
            return new MySqlCommand(s, c, t).ExecuteScalar()?.ToString();
        }
        private DataTable GetTable(string s, MySqlConnection c, MySqlTransaction t) { MySqlDataAdapter a = new MySqlDataAdapter(s, c); a.SelectCommand.Transaction = t; DataTable dt = new DataTable(); a.Fill(dt); return dt; }
        private void MostrarAlerta(string m) { ScriptManager.RegisterStartupScript(this, GetType(), "a", $"Swal.fire('Atención','{m}','warning');", true); }
    }
}