using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace Optica.Formularios
{
    public partial class HistorialVentas : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["MsjExito"] != null)
                {
                    string m = Session["MsjExito"].ToString();
                    ClientScript.RegisterStartupScript(GetType(), "alert", $"Swal.fire('Correcto','{m}','success');", true);
                    Session["MsjExito"] = null;
                }
                CargarGrid();
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarGrid();
        }

        private void CargarGrid()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    string baseQuery = @"SELECT V.ID_Venta, V.NumeroDocumento, V.Fecha, V.Estado, V.EstadoPagoVenta, TD.Descripcion as TipoDoc,
                                       (SELECT IFNULL(SUM(Monto),0) FROM Pago WHERE ID_Venta=V.ID_Venta) as Total,
                                       CASE WHEN C.TipoCliente='Natural' THEN CONCAT(CN.Nombre,' ',CN.Apellido) ELSE CJ.RepresentanteLegal END as Cliente
                                       FROM Venta V 
                                       JOIN Clientes C ON V.ID_Cliente=C.ID_Cliente 
                                       JOIN TipoDocumento TD ON V.ID_TipoDocumento=TD.ID_TipoDocumento
                                       LEFT JOIN ClienteNatural CN ON C.ID_Cliente=CN.ID_Cliente 
                                       LEFT JOIN ClienteJuridico CJ ON C.ID_Cliente=CJ.ID_Cliente 
                                       WHERE 1=1";

                    if (!string.IsNullOrEmpty(txtBuscar.Text)) baseQuery += " AND (V.NumeroDocumento LIKE @Bus OR CN.Nombre LIKE @Bus OR CN.Apellido LIKE @Bus OR CJ.RepresentanteLegal LIKE @Bus)";
                    if (ddlEstado.SelectedValue != "-1") baseQuery += " AND V.Estado = @Est";
                    if (!string.IsNullOrEmpty(txtFIni.Text)) baseQuery += " AND V.Fecha >= @Ini";
                    if (!string.IsNullOrEmpty(txtFFin.Text)) baseQuery += " AND V.Fecha <= @Fin";

                    baseQuery += " ORDER BY V.ID_Venta DESC";

                    int limit = int.Parse(ddlLimite.SelectedValue);
                    if (limit > 0) baseQuery += " LIMIT " + limit;

                    MySqlCommand cmd = new MySqlCommand(baseQuery, con);
                    if (!string.IsNullOrEmpty(txtBuscar.Text)) cmd.Parameters.AddWithValue("@Bus", "%" + txtBuscar.Text + "%");
                    if (ddlEstado.SelectedValue != "-1") cmd.Parameters.AddWithValue("@Est", ddlEstado.SelectedValue);
                    if (!string.IsNullOrEmpty(txtFIni.Text)) cmd.Parameters.AddWithValue("@Ini", DateTime.Parse(txtFIni.Text));
                    if (!string.IsNullOrEmpty(txtFFin.Text)) cmd.Parameters.AddWithValue("@Fin", DateTime.Parse(txtFFin.Text));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvVentas.DataSource = dt;
                    gvVentas.DataBind();

                    pnlAviso.Visible = false;
                    if (limit > 0 && dt.Rows.Count < limit && dt.Rows.Count > 0)
                    {
                        pnlAviso.Visible = true;
                        lblAviso.Text = $"Se solicitaron {limit} registros, pero actualmente solo existen {dt.Rows.Count} que coinciden con los filtros.";
                    }
                    else if (dt.Rows.Count == 0)
                    {
                        pnlAviso.Visible = true;
                        lblAviso.Text = "No se encontraron registros.";
                    }
                }
            }
            catch { }
        }

        protected void gvVentas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Editar")
            {
                Response.Redirect("Venta.aspx?id=" + id);
            }
            else if (e.CommandName == "CambiarEstado")
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = "UPDATE Venta SET Estado = CASE WHEN Estado=1 THEN 0 ELSE 1 END WHERE ID_Venta=@ID";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
                CargarGrid();
                ClientScript.RegisterStartupScript(GetType(), "toast", "const Toast = Swal.mixin({toast: true, position: 'top-end', showConfirmButton: false, timer: 3000}); Toast.fire({icon: 'success', title: 'Estado actualizado correctamente'});", true);
            }
            else if (e.CommandName == "Imprimir")
            {
                GenerarReportePDF(id);
            }
        }

        private void GenerarReportePDF(int idVenta)
        {
            try
            {
                string rutaLogo = Server.MapPath("~/Imagenes2/logo_optica.png.jpg");

                if (!File.Exists(rutaLogo))
                {
                    rutaLogo = Server.MapPath("../Imagenes2/logo_optica.png.jpg");
                }

                ReportePdfHelper pdfHelper = new ReportePdfHelper(rutaLogo);
                byte[] pdfBytes = pdfHelper.GenerarDocumentoVenta(idVenta);

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", $"attachment; filename=Documento_Venta_{idVenta}.pdf");
                Response.BinaryWrite(pdfBytes);
                Response.End();
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(GetType(), "errorPdf", $"Swal.fire('Error','{ex.Message}','error');", true);
            }
        }
    }

    public class ReportePdfHelper
    {
        private string _rutaLogo;

        private BaseColor _gold = new BaseColor(218, 165, 32);
        private BaseColor _black = new BaseColor(0, 0, 0);
        private BaseColor _gray = new BaseColor(230, 230, 230);

        private Font _fNormal;
        private Font _fBold;
        private Font _fTitle;
        private Font _fSubtitle;
        private Font _fSmall;
        private Font _fRed;
        private Font _fTicket;
        private Font _fTicketBold;

        public ReportePdfHelper(string rutaLogo)
        {
            _rutaLogo = rutaLogo;
            _fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL, _black);
            _fBold = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.BOLD, _black);
            _fTitle = FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD, _black);
            _fSubtitle = FontFactory.GetFont(FontFactory.HELVETICA, 11, Font.BOLD, _black);
            _fSmall = FontFactory.GetFont(FontFactory.HELVETICA, 7, Font.NORMAL, BaseColor.DARK_GRAY);
            _fRed = FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.BOLD, BaseColor.RED);

            _fTicket = FontFactory.GetFont(FontFactory.COURIER, 8, Font.NORMAL, _black);
            _fTicketBold = FontFactory.GetFont(FontFactory.COURIER, 9, Font.BOLD, _black);
        }

        public byte[] GenerarDocumentoVenta(int idVenta)
        {
            DataSet dsDatos = ObtenerDatosVentaCompleta(idVenta);

            if (dsDatos.Tables["Venta"].Rows.Count == 0) throw new Exception("Venta no encontrada.");
            DataRow rowVenta = dsDatos.Tables["Venta"].Rows[0];

            if (Convert.ToInt32(rowVenta["Estado"]) == 0) throw new Exception("No se puede generar reporte de una venta ANULADA.");

            string tipoDoc = rowVenta["TipoDocumentoDescripcion"].ToString().ToUpper();

            using (MemoryStream ms = new MemoryStream())
            {
                Rectangle pageSize = PageSize.LETTER;
                if (tipoDoc.Contains("TICKET")) pageSize = new Rectangle(227, 800);

                Document doc = new Document(pageSize, 25, 25, 25, 25);
                if (tipoDoc.Contains("TICKET")) doc.SetMargins(10, 10, 5, 5);

                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                if (tipoDoc.Contains("FACTURA"))
                {
                    ConstruirLayoutFactura(doc, rowVenta, dsDatos);
                }
                else if (tipoDoc.Contains("BOLETA"))
                {
                    ConstruirLayoutBoleta(doc, rowVenta, dsDatos);
                }
                else
                {
                    ConstruirLayoutTicket(doc, rowVenta, dsDatos);
                }

                doc.Close();
                return ms.ToArray();
            }
        }

        private void ConstruirLayoutFactura(Document doc, DataRow v, DataSet ds)
        {
            string estadoVenta = v["EstadoPagoVenta"] != DBNull.Value ? v["EstadoPagoVenta"].ToString().ToUpper() : "";
            string textoEstadoPago = (estadoVenta == "CANCELADO" || estadoVenta == "PAGO COMPLETO" || estadoVenta == "PAGADO") ? "Pago completo" : "Pago del 50%";
            bool esPago50 = (textoEstadoPago == "Pago del 50%");

            PdfPTable header = new PdfPTable(3);
            header.WidthPercentage = 100;
            header.SetWidths(new float[] { 1.5f, 3.5f, 2f });
            header.DefaultCell.Border = Rectangle.NO_BORDER;
            header.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;

            PdfPCell cLogo = new PdfPCell();
            cLogo.Border = Rectangle.NO_BORDER;
            if (File.Exists(_rutaLogo))
            {
                try
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(_rutaLogo);
                    img.ScaleToFit(70, 70);
                    cLogo.AddElement(img);
                }
                catch { }
            }
            header.AddCell(cLogo);

            PdfPCell cInfo = new PdfPCell();
            cInfo.Border = Rectangle.NO_BORDER;
            cInfo.AddElement(new Paragraph("ÓPTICA 20/20", _fTitle));
            cInfo.AddElement(new Paragraph("RUC: 0012007940018Q", _fBold));
            cInfo.AddElement(new Paragraph("Dir: Supermercado La Colonia Monseñor Lezcano, 2c. al Norte, 1c. al oeste.", _fSmall));
            header.AddCell(cInfo);

            PdfPCell cFact = new PdfPCell();
            cFact.Border = Rectangle.BOX;
            cFact.BorderColor = _gold;
            cFact.BorderWidth = 1.5f;
            cFact.Padding = 8f;
            Paragraph pf = new Paragraph("FACTURA", _fSubtitle); pf.Alignment = Element.ALIGN_CENTER; cFact.AddElement(pf);
            Paragraph pn = new Paragraph("Nº " + v["NumeroDocumento"], _fRed); pn.Alignment = Element.ALIGN_CENTER; cFact.AddElement(pn);
            header.AddCell(cFact);

            doc.Add(header);
            doc.Add(new Paragraph("\n"));

            PdfPTable cli = new PdfPTable(1);
            cli.WidthPercentage = 100;
            PdfPCell cCli = new PdfPCell();
            cCli.Border = Rectangle.BOX;
            cCli.BorderColor = _black;
            cCli.Padding = 5f;

            PdfPTable innerCli = new PdfPTable(2);
            innerCli.WidthPercentage = 100;
            innerCli.AddCell(GetNoBorder("Cliente: " + v["NombreCliente"], _fBold));
            innerCli.AddCell(GetNoBorder("Fecha: " + Convert.ToDateTime(v["Fecha"]).ToString("dd/MM/yyyy"), _fNormal, Element.ALIGN_RIGHT));
            innerCli.AddCell(GetNoBorder("Identificación: " + v["IdentificacionCliente"], _fNormal));
            innerCli.AddCell(GetNoBorder("Dirección: " + v["Direccion"], _fNormal, Element.ALIGN_RIGHT));
            innerCli.AddCell(GetNoBorder("Condición: " + textoEstadoPago, _fBold));
            innerCli.AddCell(GetNoBorder("", _fNormal));

            cCli.AddElement(innerCli);
            cli.AddCell(cCli);
            doc.Add(cli);
            doc.Add(new Paragraph("\n"));

            PdfPTable grid = new PdfPTable(4);
            grid.WidthPercentage = 100;
            grid.SetWidths(new float[] { 1f, 4f, 1.5f, 1.5f });

            string[] headers = { "CANT.", "DESCRIPCIÓN", "P. UNIT", "TOTAL" };
            foreach (string h in headers)
            {
                PdfPCell ch = new PdfPCell(new Phrase(h, _fBold));
                ch.BackgroundColor = _gold;
                ch.HorizontalAlignment = Element.ALIGN_CENTER;
                ch.Padding = 5f;
                grid.AddCell(ch);
            }

            decimal total = 0;
            foreach (DataRow p in ds.Tables["Productos"].Rows)
            {
                decimal sub = Convert.ToDecimal(p["Subtotal"]);
                total += sub;
                AddGridRow(grid, p["Cantidad"].ToString(), p["Descripcion"].ToString(), (sub / Convert.ToDecimal(p["Cantidad"])).ToString("N2"), sub.ToString("N2"));
            }
            foreach (DataRow l in ds.Tables["Lentes"].Rows)
            {
                decimal sub = Convert.ToDecimal(l["Subtotal"]);
                decimal montoMostrar = esPago50 ? sub * 0.5m : sub;
                total += montoMostrar;
                AddGridRow(grid, "1", "Lente Completo Exp #" + l["ID_Expediente"] + " (" + textoEstadoPago + ")", montoMostrar.ToString("N2"), montoMostrar.ToString("N2"));
            }

            for (int i = 0; i < 6; i++) AddGridRow(grid, " ", " ", " ", " ");

            doc.Add(grid);
            doc.Add(new Paragraph("\n"));

            PdfPTable foot = new PdfPTable(2);
            foot.WidthPercentage = 100;
            foot.SetWidths(new float[] { 4f, 1.5f });

            PdfPCell cMsg = new PdfPCell(new Phrase("Gracias Por Preferirnos - CUOTA FIJA", _fBold));
            cMsg.Border = Rectangle.TOP_BORDER;
            foot.AddCell(cMsg);

            PdfPCell cTot = new PdfPCell(new Phrase("TOTAL C$ " + total.ToString("N2"), _fTitle));
            cTot.Border = Rectangle.BOX;
            cTot.BorderColor = _gold;
            cTot.BackgroundColor = _gray;
            cTot.HorizontalAlignment = Element.ALIGN_RIGHT;
            foot.AddCell(cTot);
            doc.Add(foot);

            doc.Add(new Paragraph("\n\n\n"));
            PdfPTable firmas = new PdfPTable(2);
            firmas.WidthPercentage = 80;
            firmas.HorizontalAlignment = Element.ALIGN_CENTER;
            PdfPCell f1 = new PdfPCell(new Paragraph("Recibí Conforme", _fNormal));
            f1.Border = Rectangle.TOP_BORDER; f1.HorizontalAlignment = Element.ALIGN_CENTER; firmas.AddCell(f1);
            PdfPCell fspace = new PdfPCell(new Phrase("")); fspace.Border = Rectangle.NO_BORDER; firmas.AddCell(fspace);
            PdfPCell f2 = new PdfPCell(new Paragraph("Entregue Conforme", _fNormal));
            f2.Border = Rectangle.TOP_BORDER; f2.HorizontalAlignment = Element.ALIGN_CENTER; firmas.AddCell(f2);
            doc.Add(firmas);
        }

        private void ConstruirLayoutBoleta(Document doc, DataRow v, DataSet ds)
        {
            string estadoVenta = v["EstadoPagoVenta"] != DBNull.Value ? v["EstadoPagoVenta"].ToString().ToUpper() : "";
            string textoEstadoPago = (estadoVenta == "CANCELADO" || estadoVenta == "PAGO COMPLETO" || estadoVenta == "PAGADO") ? "Pago completo" : "Pago del 50%";
            bool esPago50 = (textoEstadoPago == "Pago del 50%");

            PdfPTable layout = new PdfPTable(2);
            layout.WidthPercentage = 100;
            layout.SetWidths(new float[] { 2f, 1f });

            PdfPCell cLeft = new PdfPCell();
            cLeft.Border = Rectangle.NO_BORDER;
            if (File.Exists(_rutaLogo))
            {
                try
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(_rutaLogo);
                    img.ScaleToFit(50, 50);
                    cLeft.AddElement(img);
                }
                catch { }
            }
            cLeft.AddElement(new Paragraph("ÓPTICA 20/20", _fSubtitle));
            cLeft.AddElement(new Paragraph("RUC: 0012007940018Q", _fSmall));
            cLeft.AddElement(new Paragraph("Managua, Nicaragua", _fSmall));
            layout.AddCell(cLeft);

            PdfPCell cRight = new PdfPCell();
            cRight.Border = Rectangle.BOX;
            cRight.BorderWidth = 1f;
            cRight.Padding = 5f;
            cRight.BackgroundColor = _gray;

            Paragraph pRuc = new Paragraph("R.U.C.: 0012007940018Q", _fBold); pRuc.Alignment = Element.ALIGN_CENTER;
            Paragraph pBol = new Paragraph("BOLETA DE VENTA", _fSubtitle); pBol.Alignment = Element.ALIGN_CENTER;
            Paragraph pNum = new Paragraph(v["NumeroDocumento"].ToString(), _fRed); pNum.Alignment = Element.ALIGN_CENTER;

            cRight.AddElement(pRuc);
            cRight.AddElement(pBol);
            cRight.AddElement(pNum);
            layout.AddCell(cRight);
            doc.Add(layout);

            doc.Add(new Paragraph("\n"));

            PdfPTable info = new PdfPTable(2);
            info.WidthPercentage = 100;
            info.SetWidths(new float[] { 3f, 1f });
            info.AddCell(GetCellBottomBorder("Señor(es): " + v["NombreCliente"]));
            info.AddCell(GetCellBottomBorder("Fecha: " + Convert.ToDateTime(v["Fecha"]).ToString("dd/MM/yyyy")));
            info.AddCell(GetCellBottomBorder("Dirección: " + v["Direccion"]));
            info.AddCell(GetCellBottomBorder("RUC/DNI: " + v["IdentificacionCliente"]));
            info.AddCell(GetCellBottomBorder("Condición: " + textoEstadoPago));
            info.AddCell(GetCellBottomBorder(""));
            doc.Add(info);

            doc.Add(new Paragraph("\n"));

            PdfPTable grid = new PdfPTable(4);
            grid.WidthPercentage = 100;
            grid.SetWidths(new float[] { 1f, 4f, 1.5f, 1.5f });

            string[] hs = { "CANT.", "DESCRIPCION", "P. UNIT", "IMPORTE" };
            foreach (string h in hs)
            {
                PdfPCell c = new PdfPCell(new Phrase(h, _fBold));
                c.HorizontalAlignment = Element.ALIGN_CENTER;
                c.Border = Rectangle.BOX;
                c.BackgroundColor = _gray;
                grid.AddCell(c);
            }

            decimal total = 0;
            foreach (DataRow p in ds.Tables["Productos"].Rows)
            {
                decimal sub = Convert.ToDecimal(p["Subtotal"]);
                total += sub;
                AddBoletaRow(grid, p["Cantidad"].ToString(), p["Descripcion"].ToString(), (sub / Convert.ToDecimal(p["Cantidad"])).ToString("N2"), sub.ToString("N2"));
            }
            foreach (DataRow l in ds.Tables["Lentes"].Rows)
            {
                decimal sub = Convert.ToDecimal(l["Subtotal"]);
                decimal montoMostrar = esPago50 ? sub * 0.5m : sub;
                total += montoMostrar;
                AddBoletaRow(grid, "1", "Lente Exp #" + l["ID_Expediente"] + " (" + textoEstadoPago + ")", montoMostrar.ToString("N2"), montoMostrar.ToString("N2"));
            }

            for (int i = 0; i < 8; i++) AddBoletaRow(grid, "", "", "", "");

            doc.Add(grid);

            PdfPTable foot = new PdfPTable(2);
            foot.WidthPercentage = 100;
            foot.SetWidths(new float[] { 5f, 1.5f });
            PdfPCell cf = new PdfPCell(new Phrase("TOTAL C$", _fSubtitle)); cf.HorizontalAlignment = Element.ALIGN_RIGHT;
            PdfPCell cv = new PdfPCell(new Phrase(total.ToString("N2"), _fSubtitle)); cv.HorizontalAlignment = Element.ALIGN_CENTER;
            foot.AddCell(cf); foot.AddCell(cv);
            doc.Add(foot);
        }

        private void ConstruirLayoutTicket(Document doc, DataRow v, DataSet ds)
        {
            string estadoVenta = v["EstadoPagoVenta"] != DBNull.Value ? v["EstadoPagoVenta"].ToString().ToUpper() : "";
            string textoEstadoPago = (estadoVenta == "CANCELADO" || estadoVenta == "PAGO COMPLETO" || estadoVenta == "PAGADO") ? "Pago completo" : "Pago del 50%";
            bool esPago50 = (textoEstadoPago == "Pago del 50%");

            if (File.Exists(_rutaLogo))
            {
                try
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(_rutaLogo);
                    img.ScaleToFit(40, 40);
                    img.Alignment = Element.ALIGN_CENTER;
                    doc.Add(img);
                }
                catch { }
            }

            Paragraph p = new Paragraph("ÓPTICA 20/20", _fTicketBold); p.Alignment = Element.ALIGN_CENTER; doc.Add(p);
            p = new Paragraph("RUC: 0012007940018Q", _fTicket); p.Alignment = Element.ALIGN_CENTER; doc.Add(p);
            p = new Paragraph("Managua, Nicaragua", _fTicket); p.Alignment = Element.ALIGN_CENTER; doc.Add(p);
            p = new Paragraph("Gracias por su preferencia!", _fTicket); p.Alignment = Element.ALIGN_CENTER; doc.Add(p);
            p = new Paragraph("CUENTA", _fTicketBold); p.Alignment = Element.ALIGN_CENTER; doc.Add(p);
            doc.Add(new Paragraph("\n"));

            doc.Add(new Paragraph("Fecha: " + Convert.ToDateTime(v["Fecha"]).ToString("dd/MM/yyyy HH:mm"), _fTicket));
            doc.Add(new Paragraph("Ticket: " + v["NumeroDocumento"], _fTicket));
            doc.Add(new Paragraph("Cliente: " + v["NombreCliente"], _fTicket));
            doc.Add(new Paragraph("Condición: " + textoEstadoPago, _fTicket));

            DottedLineSeparator line = new DottedLineSeparator();
            doc.Add(new Chunk(line));
            doc.Add(new Paragraph("\n"));

            decimal total = 0;
            foreach (DataRow x in ds.Tables["Productos"].Rows)
            {
                decimal s = Convert.ToDecimal(x["Subtotal"]);
                total += s;
                doc.Add(new Paragraph(x["Descripcion"].ToString(), _fTicket));
                PdfPTable ln = new PdfPTable(2); ln.WidthPercentage = 100;
                ln.AddCell(GetTicketCell($"{x["Cantidad"]} x {s / Convert.ToDecimal(x["Cantidad"]):N2}"));
                ln.AddCell(GetTicketCell(s.ToString("N2"), Element.ALIGN_RIGHT));
                doc.Add(ln);
            }
            foreach (DataRow l in ds.Tables["Lentes"].Rows)
            {
                decimal sub = Convert.ToDecimal(l["Subtotal"]);
                decimal montoMostrar = esPago50 ? sub * 0.5m : sub;
                total += montoMostrar;
                doc.Add(new Paragraph("Lente Exp #" + l["ID_Expediente"] + " (" + textoEstadoPago + ")", _fTicket));
                PdfPTable ln = new PdfPTable(2); ln.WidthPercentage = 100;
                ln.AddCell(GetTicketCell($"1 x {montoMostrar:N2}"));
                ln.AddCell(GetTicketCell(montoMostrar.ToString("N2"), Element.ALIGN_RIGHT));
                doc.Add(ln);
            }

            doc.Add(new Chunk(line));

            PdfPTable t = new PdfPTable(2); t.WidthPercentage = 100;
            t.AddCell(GetTicketCell("TOTAL C$", Element.ALIGN_LEFT, true));
            t.AddCell(GetTicketCell(total.ToString("N2"), Element.ALIGN_RIGHT, true));
            doc.Add(t);

            doc.Add(new Chunk(line));
            doc.Add(new Paragraph("NO SE ACEPTAN DEVOLUCIONES", _fTicket) { Alignment = Element.ALIGN_CENTER });
        }

        private void AddGridRow(PdfPTable t, string c1, string c2, string c3, string c4)
        {
            t.AddCell(new PdfPCell(new Phrase(c1, _fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5f });
            t.AddCell(new PdfPCell(new Phrase(c2, _fNormal)) { Padding = 5f });
            t.AddCell(new PdfPCell(new Phrase(c3, _fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5f });
            t.AddCell(new PdfPCell(new Phrase(c4, _fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5f });
        }

        private void AddBoletaRow(PdfPTable t, string c1, string c2, string c3, string c4)
        {
            t.AddCell(new PdfPCell(new Phrase(c1, _fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER });
            t.AddCell(new PdfPCell(new Phrase(c2, _fNormal)) { Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER });
            t.AddCell(new PdfPCell(new Phrase(c3, _fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER });
            t.AddCell(new PdfPCell(new Phrase(c4, _fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER });
        }

        private PdfPCell GetNoBorder(string t, Font f, int align = Element.ALIGN_LEFT)
        {
            return new PdfPCell(new Phrase(t, f)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = align, Padding = 2f };
        }

        private PdfPCell GetCellBottomBorder(string t)
        {
            PdfPCell c = new PdfPCell(new Phrase(t, _fNormal));
            c.Border = Rectangle.BOTTOM_BORDER;
            c.PaddingBottom = 2f;
            return c;
        }

        private PdfPCell GetTicketCell(string t, int align = Element.ALIGN_LEFT, bool bold = false)
        {
            PdfPCell c = new PdfPCell(new Phrase(t, bold ? _fTicketBold : _fTicket));
            c.Border = Rectangle.NO_BORDER;
            c.HorizontalAlignment = align;
            return c;
        }

        private DataSet ObtenerDatosVentaCompleta(int idVenta)
        {
            DataSet ds = new DataSet();
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                string sqlVenta = @"SELECT V.*, TD.Descripcion as TipoDocumentoDescripcion, 
                                    CASE WHEN C.TipoCliente='Natural' THEN CONCAT(CN.Nombre,' ',CN.Apellido) ELSE CJ.RepresentanteLegal END as NombreCliente,
                                    CASE WHEN C.TipoCliente='Natural' THEN CN.Cedula ELSE CJ.RUC END as IdentificacionCliente,
                                    C.Direccion, C.Telefono
                                    FROM Venta V 
                                    JOIN TipoDocumento TD ON V.ID_TipoDocumento=TD.ID_TipoDocumento
                                    JOIN Clientes C ON V.ID_Cliente=C.ID_Cliente
                                    LEFT JOIN ClienteNatural CN ON C.ID_Cliente=CN.ID_Cliente
                                    LEFT JOIN ClienteJuridico CJ ON C.ID_Cliente=CJ.ID_Cliente
                                    WHERE V.ID_Venta=@ID";
                MySqlDataAdapter daV = new MySqlDataAdapter(sqlVenta, con);
                daV.SelectCommand.Parameters.AddWithValue("@ID", idVenta);
                daV.Fill(ds, "Venta");

                string sqlProd = @"SELECT DV.*, P.Descripcion FROM DetalleVentaProducto DV 
                                   JOIN Producto P ON DV.ID_Producto=P.ID_Producto WHERE DV.ID_Venta=@ID";
                MySqlDataAdapter daP = new MySqlDataAdapter(sqlProd, con);
                daP.SelectCommand.Parameters.AddWithValue("@ID", idVenta);
                daP.Fill(ds, "Productos");

                string sqlLente = @"SELECT * FROM DetalleVentaLentes WHERE ID_Venta=@ID";
                MySqlDataAdapter daL = new MySqlDataAdapter(sqlLente, con);
                daL.SelectCommand.Parameters.AddWithValue("@ID", idVenta);
                daL.Fill(ds, "Lentes");
            }
            return ds;
        }
    }
}