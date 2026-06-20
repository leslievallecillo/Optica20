using System;
using System.Data;
using System.IO;
using System.Web.UI;
using MySql.Data.MySqlClient;
using Optica.Clases;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Optica.Reportes
{
    public partial class ReporteUtilidades : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtF1.Text = "";
                txtF2.Text = "";
                GenerarReporte();
            }
        }

        private DataTable ObtenerDatos()
        {
            DataTable dt = new DataTable();
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = @"
                        SELECT 
                            v.NumeroDocumento as FacturaNo,
                            IF(c.TipoCliente='Natural', 
                                (SELECT CONCAT(Nombre, ' ', Apellido) FROM clientenatural WHERE ID_Cliente = c.ID_Cliente), 
                                (SELECT NombreEmpresa FROM clientejuridico WHERE ID_Cliente = c.ID_Cliente)
                            ) as Cliente,
                            v.Fecha,
                            (
                                IFNULL((SELECT SUM(Subtotal) FROM detalleventaproducto WHERE ID_Venta = v.ID_Venta AND Estado=1), 0) +
                                IFNULL((SELECT SUM(Subtotal) FROM detalleventalentes WHERE ID_Venta = v.ID_Venta AND Estado=1), 0)
                            ) as TotalVenta,
                            (
                                IFNULL((
                                    SELECT SUM(dp.Cantidad * IFNULL(
                                        (SELECT dc.PrecioUnitario FROM detallecompra dc WHERE dc.ID_Producto = dp.ID_Producto ORDER BY dc.FechaRegistro DESC LIMIT 1), 0)
                                    )
                                    FROM detalleventaproducto dp 
                                    WHERE dp.ID_Venta = v.ID_Venta AND dp.Estado=1
                                ), 0) 
                                +
                                IFNULL((
                                    SELECT SUM(
                                        IFNULL((SELECT dc.PrecioUnitario FROM detallecompra dc 
                                                JOIN expediente ex ON dl.ID_Expediente = ex.ID_Expediente 
                                                WHERE dc.ID_Producto = ex.ID_Producto ORDER BY dc.FechaRegistro DESC LIMIT 1), 0)
                                    )
                                    FROM detalleventalentes dl
                                    WHERE dl.ID_Venta = v.ID_Venta AND dl.Estado=1
                                ), 0)
                            ) as TotalCosto
                        FROM venta v
                        INNER JOIN clientes c ON v.ID_Cliente = c.ID_Cliente
                        WHERE v.Estado = 1 ";

                    if (!string.IsNullOrEmpty(txtF1.Text) && !string.IsNullOrEmpty(txtF2.Text))
                        sql += " AND v.Fecha BETWEEN @F1 AND @F2 ";

                    sql += " ORDER BY v.Fecha DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    if (!string.IsNullOrEmpty(txtF1.Text) && !string.IsNullOrEmpty(txtF2.Text))
                    {
                        cmd.Parameters.AddWithValue("@F1", txtF1.Text + " 00:00:00");
                        cmd.Parameters.AddWithValue("@F2", txtF2.Text + " 23:59:59");
                    }

                    new MySqlDataAdapter(cmd).Fill(dt);

                    dt.Columns.Add("TotalUtilidad", typeof(decimal));
                    foreach (DataRow row in dt.Rows)
                    {
                        decimal venta = Convert.ToDecimal(row["TotalVenta"]);
                        decimal costo = Convert.ToDecimal(row["TotalCosto"]);
                        row["TotalUtilidad"] = venta - costo;
                    }
                }
                catch { }
            }
            return dt;
        }

        private void GenerarReporte()
        {
            DataTable dt = ObtenerDatos();
            gvUtilidades.DataSource = dt;
            gvUtilidades.DataBind();
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            GenerarReporte();
        }

        protected void btnPdf_Click(object sender, EventArgs e)
        {
            try
            {
                string rutaLogo = Server.MapPath("~/Imagenes2/logo_optica.png.jpg");
                if (!File.Exists(rutaLogo)) rutaLogo = null;

                DataTable dt = ObtenerDatos();

                ReportePdfUtilidad helper = new ReportePdfUtilidad(rutaLogo);
                byte[] pdfBytes = helper.GenerarReporte(dt, txtF1.Text, txtF2.Text);

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=Reporte_Utilidades.pdf");
                Response.BinaryWrite(pdfBytes);
                Response.End();
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "errPdf", $"Swal.fire('Error PDF', '{ex.Message}', 'error');", true);
            }
        }
    }

    public class ReportePdfUtilidad
    {
        private string _rutaLogo;
        private BaseColor _bgDark = new BaseColor(45, 52, 54);
        private BaseColor _gold = new BaseColor(212, 175, 55);
        private BaseColor _white = new BaseColor(255, 255, 255);
        private BaseColor _black = new BaseColor(0, 0, 0);
        private BaseColor _lightGray = new BaseColor(240, 240, 240);
        private BaseColor _green = new BaseColor(40, 167, 69);
        private BaseColor _red = new BaseColor(220, 53, 69);

        public ReportePdfUtilidad(string rutaLogo) { _rutaLogo = rutaLogo; }

        public byte[] GenerarReporte(DataTable dt, string f1, string f2)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.LETTER.Rotate(), 30, 30, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                PdfPTable header = new PdfPTable(2);
                header.WidthPercentage = 100;
                header.SetWidths(new float[] { 1f, 4f });

                PdfPCell cLogo = new PdfPCell();
                cLogo.BackgroundColor = _bgDark; cLogo.Border = Rectangle.NO_BORDER; cLogo.Padding = 10;
                if (!string.IsNullOrEmpty(_rutaLogo))
                {
                    try { iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(_rutaLogo); img.ScaleToFit(50, 50); cLogo.AddElement(img); } catch { }
                }
                header.AddCell(cLogo);

                PdfPCell cTitle = new PdfPCell();
                cTitle.BackgroundColor = _bgDark; cTitle.Border = Rectangle.NO_BORDER; cTitle.PaddingRight = 10;
                cTitle.AddElement(new Paragraph("ÓPTICA 20/20", FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD, _gold)));
                cTitle.AddElement(new Paragraph("REPORTE DE UTILIDADES", FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.NORMAL, _white)));
                cTitle.AddElement(new Paragraph($"Periodo: {f1} al {f2}", FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.NORMAL, _white)));
                header.AddCell(cTitle);

                doc.Add(header);
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1.2f, 3f, 1.5f, 1.5f, 1.5f, 1.5f });

                string[] heads = { "FACTURA", "CLIENTE", "FECHA", "COSTO TOTAL", "VENTA TOTAL", "UTILIDAD" };
                foreach (string h in heads)
                {
                    PdfPCell c = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                    c.BackgroundColor = _bgDark; c.HorizontalAlignment = Element.ALIGN_CENTER; c.Padding = 6;
                    table.AddCell(c);
                }

                Font fRow = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL, _black);
                bool alternate = false;
                decimal sumCosto = 0, sumVenta = 0, sumUtilidad = 0;

                foreach (DataRow r in dt.Rows)
                {
                    BaseColor bg = alternate ? _lightGray : _white;
                    decimal costo = Convert.ToDecimal(r["TotalCosto"]);
                    decimal venta = Convert.ToDecimal(r["TotalVenta"]);
                    decimal util = Convert.ToDecimal(r["TotalUtilidad"]);

                    sumCosto += costo; sumVenta += venta; sumUtilidad += util;

                    AddCell(table, r["FacturaNo"].ToString(), fRow, bg);
                    AddCell(table, r["Cliente"].ToString(), fRow, bg);
                    AddCell(table, Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy"), fRow, bg, Element.ALIGN_CENTER);
                    AddCell(table, costo.ToString("C"), fRow, bg, Element.ALIGN_RIGHT);
                    AddCell(table, venta.ToString("C"), fRow, bg, Element.ALIGN_RIGHT);

                    PdfPCell cUtil = new PdfPCell(new Phrase(util.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.BOLD, util >= 0 ? _green : _red)));
                    cUtil.BackgroundColor = bg; cUtil.HorizontalAlignment = Element.ALIGN_RIGHT; cUtil.Padding = 4; cUtil.BorderColor = _bgDark;
                    table.AddCell(cUtil);
                    alternate = !alternate;
                }

                PdfPCell cTotalLabel = new PdfPCell(new Phrase("TOTALES GENERALES", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                cTotalLabel.Colspan = 3; cTotalLabel.BackgroundColor = _bgDark; cTotalLabel.HorizontalAlignment = Element.ALIGN_RIGHT; cTotalLabel.Padding = 6;
                table.AddCell(cTotalLabel);
                AddTotalCell(table, sumCosto.ToString("C"));
                AddTotalCell(table, sumVenta.ToString("C"));
                AddTotalCell(table, sumUtilidad.ToString("C"));

                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }

        private void AddCell(PdfPTable t, string txt, Font f, BaseColor bg, int align = Element.ALIGN_LEFT)
        {
            PdfPCell c = new PdfPCell(new Phrase(txt, f));
            c.BackgroundColor = bg; c.HorizontalAlignment = align; c.Padding = 4; c.BorderColor = _bgDark;
            t.AddCell(c);
        }

        private void AddTotalCell(PdfPTable t, string txt)
        {
            PdfPCell c = new PdfPCell(new Phrase(txt, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
            c.BackgroundColor = _bgDark; c.HorizontalAlignment = Element.ALIGN_RIGHT; c.Padding = 6;
            t.AddCell(c);
        }
    }
}