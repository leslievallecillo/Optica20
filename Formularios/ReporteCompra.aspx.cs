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
    public partial class ReporteCompra : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtF1.Text = "";
                txtF2.Text = "";
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
                            c.NumeroCompra, 
                            c.Fecha,
                            p.RazonSocial AS Proveedor,
                            CONCAT(u.Nombres, ' ', u.Apellidos) AS Usuario,
                            (
                                SELECT IFNULL(SUM(PrecioTotal), 0) 
                                FROM detallecompra 
                                WHERE ID_Compra = c.ID_Compra AND Estado = 1
                            ) AS Total
                        FROM compra c
                        INNER JOIN proveedor p ON c.ID_Proveedor = p.ID_Proveedor
                        INNER JOIN usuario u ON c.ID_Usuario = u.ID_Usuario
                        WHERE c.Estado = 1 ";

                    if (!string.IsNullOrEmpty(txtF1.Text) && !string.IsNullOrEmpty(txtF2.Text))
                    {
                        sql += " AND c.Fecha BETWEEN @F1 AND @F2 ";
                    }

                    sql += " ORDER BY c.Fecha DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);

                    if (!string.IsNullOrEmpty(txtF1.Text) && !string.IsNullOrEmpty(txtF2.Text))
                    {
                        cmd.Parameters.AddWithValue("@F1", txtF1.Text + " 00:00:00");
                        cmd.Parameters.AddWithValue("@F2", txtF2.Text + " 23:59:59");
                    }

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    gvReporte.DataSource = dt;
                    gvReporte.DataBind();

                    decimal totalGastado = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        totalGastado += Convert.ToDecimal(r["Total"]);
                    }

                    lblTotalComprado.Text = "C$ " + totalGastado.ToString("N2");
                    lblCantCompras.Text = dt.Rows.Count.ToString();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "err", $"Swal.fire('Error', '{ex.Message}', 'error');", true);
                }
            }
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            CargarReporte();
        }

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
                            c.NumeroCompra, 
                            c.Fecha,
                            p.RazonSocial AS Proveedor,
                            CONCAT(u.Nombres, ' ', u.Apellidos) AS Usuario,
                            (
                                SELECT IFNULL(SUM(PrecioTotal), 0) 
                                FROM detallecompra 
                                WHERE ID_Compra = c.ID_Compra AND Estado = 1
                            ) AS Total
                        FROM compra c
                        INNER JOIN proveedor p ON c.ID_Proveedor = p.ID_Proveedor
                        INNER JOIN usuario u ON c.ID_Usuario = u.ID_Usuario
                        WHERE c.Estado = 1 ";

                    if (!string.IsNullOrEmpty(txtF1.Text) && !string.IsNullOrEmpty(txtF2.Text))
                    {
                        sql += " AND c.Fecha BETWEEN @F1 AND @F2 ";
                    }

                    sql += " ORDER BY c.Fecha DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);

                    if (!string.IsNullOrEmpty(txtF1.Text) && !string.IsNullOrEmpty(txtF2.Text))
                    {
                        cmd.Parameters.AddWithValue("@F1", txtF1.Text + " 00:00:00");
                        cmd.Parameters.AddWithValue("@F2", txtF2.Text + " 23:59:59");
                    }

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    ReportePdfCompra helper = new ReportePdfCompra(rutaLogo);

                    string fInicio = string.IsNullOrEmpty(txtF1.Text) ? "Inicio" : txtF1.Text;
                    string fFin = string.IsNullOrEmpty(txtF2.Text) ? "Actualidad" : txtF2.Text;

                    byte[] pdfBytes = helper.GenerarReporte(dt, fInicio, fFin);

                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", "attachment; filename=Reporte_Compras.pdf");
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

    public class ReportePdfCompra
    {
        private string _rutaLogo;
        private BaseColor _bgDark = new BaseColor(45, 52, 54);
        private BaseColor _gold = new BaseColor(212, 175, 55);
        private BaseColor _white = new BaseColor(255, 255, 255);
        private BaseColor _black = new BaseColor(0, 0, 0);
        private BaseColor _lightGray = new BaseColor(240, 240, 240);

        public ReportePdfCompra(string rutaLogo)
        {
            _rutaLogo = rutaLogo;
        }

        public byte[] GenerarReporte(DataTable dt, string fechaInicio, string fechaFin)
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
                Paragraph p2 = new Paragraph("REPORTE DE COMPRAS", fSub); p2.Alignment = Element.ALIGN_RIGHT;
                Paragraph p3 = new Paragraph($"Del {fechaInicio} al {fechaFin}", fSub); p3.Alignment = Element.ALIGN_RIGHT;

                cTitle.AddElement(p1); cTitle.AddElement(p2); cTitle.AddElement(p3);
                header.AddCell(cTitle);

                doc.Add(header);
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1.5f, 1.5f, 3f, 2f, 1.5f });

                string[] heads = { "DOC N°", "FECHA", "PROVEEDOR", "USUARIO", "TOTAL" };
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
                    DateTime fecha = Convert.ToDateTime(r["Fecha"]);
                    decimal total = Convert.ToDecimal(r["Total"]);
                    granTotal += total;

                    AddCell(table, r["NumeroCompra"].ToString(), fRow, bg, Element.ALIGN_CENTER);
                    AddCell(table, fecha.ToString("dd/MM/yyyy"), fRow, bg, Element.ALIGN_CENTER);
                    AddCell(table, r["Proveedor"].ToString(), fRow, bg);
                    AddCell(table, r["Usuario"].ToString(), fRow, bg);
                    AddCell(table, total.ToString("C"), fRow, bg, Element.ALIGN_RIGHT);

                    alternate = !alternate;
                }

                PdfPCell cTotalLabel = new PdfPCell(new Phrase("TOTAL PERIODO:", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _black)));
                cTotalLabel.Colspan = 4;
                cTotalLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
                cTotalLabel.Padding = 5;
                table.AddCell(cTotalLabel);

                PdfPCell cTotalValue = new PdfPCell(new Phrase(granTotal.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _black)));
                cTotalValue.HorizontalAlignment = Element.ALIGN_RIGHT;
                cTotalValue.Padding = 5;
                cTotalValue.BackgroundColor = _gold;
                table.AddCell(cTotalValue);

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