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

namespace Optica.Reportes
{
    public partial class ReporteVenta : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtF1.Text = DateTime.Now.ToString("yyyy-MM-01");
                txtF2.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarReporte();
            }
        }

        private void CargarReporte()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = @"
                        SELECT 
                            v.NumeroDocumento, 
                            v.Fecha,
                            IF(c.TipoCliente='Natural', (SELECT CONCAT(Nombre,' ',Apellido) FROM ClienteNatural WHERE ID_Cliente=c.ID_Cliente), (SELECT NombreEmpresa FROM ClienteJuridico WHERE ID_Cliente=c.ID_Cliente)) as Cliente,
                            CONCAT(u.Nombres, ' ', u.Apellidos) as Vendedor,
                            tp.Descripcion as TipoPago,
                            (
                                IFNULL((SELECT SUM(Subtotal) FROM DetalleVentaProducto WHERE ID_Venta = v.ID_Venta AND Estado=1), 0) +
                                IFNULL((SELECT SUM(Subtotal) FROM DetalleVentaLentes WHERE ID_Venta = v.ID_Venta AND Estado=1), 0)
                            ) as Total
                        FROM Venta v
                        INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
                        INNER JOIN Usuario u ON v.ID_Usuario = u.ID_Usuario
                        INNER JOIN TipoPago tp ON v.ID_TipoPago = tp.ID_TipoPago
                        WHERE v.Estado = 1 AND v.Fecha BETWEEN @F1 AND @F2
                        ORDER BY v.Fecha DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@F1", txtF1.Text);
                    cmd.Parameters.AddWithValue("@F2", txtF2.Text);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    if (ddlPageSize.SelectedValue != "All")
                    {
                        gvReporte.AllowPaging = true;
                        gvReporte.PageSize = int.Parse(ddlPageSize.SelectedValue);
                    }
                    else gvReporte.AllowPaging = false;

                    gvReporte.DataSource = dt;
                    gvReporte.DataBind();

                    decimal totalDinero = 0;
                    foreach (DataRow r in dt.Rows) totalDinero += Convert.ToDecimal(r["Total"]);

                    lblTotalVendido.Text = "C$ " + totalDinero.ToString("N2");
                    lblCantVentas.Text = dt.Rows.Count.ToString();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "err", $"Swal.fire('Error', '{ex.Message}', 'error');", true);
                }
            }
        }

        protected void btnGenerar_Click(object sender, EventArgs e) { CargarReporte(); }
        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e) { CargarReporte(); }
        protected void gvReporte_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvReporte.PageIndex = e.NewPageIndex; CargarReporte(); }

        protected void btnPdf_Click(object sender, EventArgs e)
        {
            try
            {
                string rutaLogo = Server.MapPath("~/Imagenes2/logo_optica.png.jpg");
                if (!File.Exists(rutaLogo)) rutaLogo = null;

                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = @"
                        SELECT 
                            v.NumeroDocumento, 
                            v.Fecha,
                            IF(c.TipoCliente='Natural', (SELECT CONCAT(Nombre,' ',Apellido) FROM ClienteNatural WHERE ID_Cliente=c.ID_Cliente), (SELECT NombreEmpresa FROM ClienteJuridico WHERE ID_Cliente=c.ID_Cliente)) as Cliente,
                            CONCAT(u.Nombres, ' ', u.Apellidos) as Vendedor,
                            tp.Descripcion as TipoPago,
                            (
                                IFNULL((SELECT SUM(Subtotal) FROM DetalleVentaProducto WHERE ID_Venta = v.ID_Venta AND Estado=1), 0) +
                                IFNULL((SELECT SUM(Subtotal) FROM DetalleVentaLentes WHERE ID_Venta = v.ID_Venta AND Estado=1), 0)
                            ) as Total
                        FROM Venta v
                        INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
                        INNER JOIN Usuario u ON v.ID_Usuario = u.ID_Usuario
                        INNER JOIN TipoPago tp ON v.ID_TipoPago = tp.ID_TipoPago
                        WHERE v.Estado = 1 AND v.Fecha BETWEEN @F1 AND @F2
                        ORDER BY v.Fecha DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@F1", txtF1.Text);
                    cmd.Parameters.AddWithValue("@F2", txtF2.Text);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    ReportePdfVentas helper = new ReportePdfVentas(rutaLogo);
                    byte[] pdfBytes = helper.GenerarReporte(dt, txtF1.Text, txtF2.Text);

                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", "attachment; filename=Reporte_Ventas.pdf");
                    Response.BinaryWrite(pdfBytes);
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "errPdf", $"Swal.fire('Error PDF', '{ex.Message}', 'error');", true);
            }
        }
    }

    public class ReportePdfVentas
    {
        private string _rutaLogo;
        private BaseColor _bgDark = new BaseColor(45, 52, 54);
        private BaseColor _gold = new BaseColor(212, 175, 55);
        private BaseColor _white = new BaseColor(255, 255, 255);
        private BaseColor _black = new BaseColor(0, 0, 0);
        private BaseColor _lightGray = new BaseColor(240, 240, 240);

        public ReportePdfVentas(string rutaLogo)
        {
            _rutaLogo = rutaLogo;
        }

        public byte[] GenerarReporte(DataTable dt, string f1, string f2)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.LETTER, 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                PdfPTable header = new PdfPTable(2);
                header.WidthPercentage = 100;
                header.SetWidths(new float[] { 1f, 4f });

                PdfPCell cLogo = new PdfPCell();
                cLogo.BackgroundColor = _bgDark;
                cLogo.Border = Rectangle.NO_BORDER;
                cLogo.Padding = 10;
                cLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
                if (!string.IsNullOrEmpty(_rutaLogo))
                {
                    try
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(_rutaLogo);
                        img.ScaleToFit(50, 50);
                        cLogo.AddElement(img);
                    }
                    catch { }
                }
                header.AddCell(cLogo);

                PdfPCell cTitle = new PdfPCell();
                cTitle.BackgroundColor = _bgDark;
                cTitle.Border = Rectangle.NO_BORDER;
                cTitle.VerticalAlignment = Element.ALIGN_MIDDLE;
                cTitle.PaddingRight = 10;

                Font fTitle = FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD, _gold);
                Font fSub = FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.NORMAL, _white);

                Paragraph p1 = new Paragraph("ÓPTICA 20/20", fTitle); p1.Alignment = Element.ALIGN_RIGHT;
                Paragraph p2 = new Paragraph("REPORTE DE VENTAS", fSub); p2.Alignment = Element.ALIGN_RIGHT;
                Paragraph p3 = new Paragraph($"Periodo: {f1} al {f2}", fSub); p3.Alignment = Element.ALIGN_RIGHT;

                cTitle.AddElement(p1); cTitle.AddElement(p2); cTitle.AddElement(p3);
                header.AddCell(cTitle);

                doc.Add(header);
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1.5f, 1.5f, 3f, 2f, 1.5f });

                string[] heads = { "FACTURA", "FECHA", "CLIENTE", "PAGO", "TOTAL" };
                foreach (string h in heads)
                {
                    PdfPCell c = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                    c.BackgroundColor = _bgDark;
                    c.HorizontalAlignment = Element.ALIGN_CENTER;
                    c.Padding = 5;
                    table.AddCell(c);
                }

                Font fRow = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL, _black);
                bool alternate = false;
                decimal granTotal = 0;

                foreach (DataRow r in dt.Rows)
                {
                    BaseColor bg = alternate ? _lightGray : _white;
                    decimal tot = Convert.ToDecimal(r["Total"]);
                    granTotal += tot;

                    AddCell(table, r["NumeroDocumento"].ToString(), fRow, bg);
                    AddCell(table, Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy"), fRow, bg, Element.ALIGN_CENTER);
                    AddCell(table, r["Cliente"].ToString(), fRow, bg);
                    AddCell(table, r["TipoPago"].ToString(), fRow, bg);
                    AddCell(table, tot.ToString("C"), fRow, bg, Element.ALIGN_RIGHT);

                    alternate = !alternate;
                }

                PdfPCell cTotalLabel = new PdfPCell(new Phrase("TOTAL GENERAL", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                cTotalLabel.Colspan = 4;
                cTotalLabel.BackgroundColor = _bgDark;
                cTotalLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
                cTotalLabel.Padding = 5;
                table.AddCell(cTotalLabel);

                PdfPCell cTotalVal = new PdfPCell(new Phrase(granTotal.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                cTotalVal.BackgroundColor = _bgDark;
                cTotalVal.HorizontalAlignment = Element.ALIGN_RIGHT;
                cTotalVal.Padding = 5;
                table.AddCell(cTotalVal);

                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }

        private void AddCell(PdfPTable t, string txt, Font f, BaseColor bg, int align = Element.ALIGN_LEFT)
        {
            PdfPCell c = new PdfPCell(new Phrase(txt, f));
            c.BackgroundColor = bg;
            c.HorizontalAlignment = align;
            c.Padding = 4;
            c.BorderColor = _bgDark;
            t.AddCell(c);
        }
    }
}