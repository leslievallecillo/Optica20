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
    public partial class ReporteUtilidades : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtF1.Text = DateTime.Now.ToString("yyyy-MM-01");
                txtF2.Text = DateTime.Now.ToString("yyyy-MM-dd");
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
                               (SELECT CONCAT(Nombre, ' ', Apellido) FROM ClienteNatural WHERE ID_Cliente = c.ID_Cliente), 
                               (SELECT NombreEmpresa FROM ClienteJuridico WHERE ID_Cliente = c.ID_Cliente)
                            ) as Cliente,
                            v.Fecha,
                            
                            -- Calculo Total Venta (Productos + Lentes)
                            (
                                IFNULL((SELECT SUM(Subtotal) FROM DetalleVentaProducto WHERE ID_Venta = v.ID_Venta AND Estado=1), 0) +
                                IFNULL((SELECT SUM(Subtotal) FROM DetalleVentaLentes WHERE ID_Venta = v.ID_Venta AND Estado=1), 0)
                            ) as TotalVenta,

                            -- Calculo Costo Estimado (Basado en último precio de compra de los productos vendidos)
                            (
                                -- Costo de Productos normales vendidos
                                IFNULL((
                                    SELECT SUM(dp.Cantidad * IFNULL(
                                        (SELECT dc.PrecioUnitario FROM DetalleCompra dc WHERE dc.ID_Producto = dp.ID_Producto ORDER BY dc.FechaRegistro DESC LIMIT 1), 0)
                                    )
                                    FROM DetalleVentaProducto dp 
                                    WHERE dp.ID_Venta = v.ID_Venta AND dp.Estado=1
                                ), 0) 
                                +
                                -- Costo estimado de Lentes
                                IFNULL((
                                    SELECT SUM(
                                        IFNULL((SELECT dc.PrecioUnitario FROM DetalleCompra dc 
                                                JOIN Expediente ex ON dl.ID_Expediente = ex.ID_Expediente 
                                                WHERE dc.ID_Producto = ex.ID_Producto ORDER BY dc.FechaRegistro DESC LIMIT 1), 0)
                                    )
                                    FROM DetalleVentaLentes dl
                                    WHERE dl.ID_Venta = v.ID_Venta AND dl.Estado=1
                                ), 0)
                            ) as TotalCosto

                        FROM Venta v
                        INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
                        WHERE v.Estado = 1 AND v.Fecha BETWEEN @F1 AND @F2
                        ORDER BY v.Fecha DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@F1", txtF1.Text);
                    cmd.Parameters.AddWithValue("@F2", txtF2.Text);

                    new MySqlDataAdapter(cmd).Fill(dt);

                    // Calcular utilidad en memoria
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

        public ReportePdfUtilidad(string rutaLogo)
        {
            _rutaLogo = rutaLogo;
        }

        public byte[] GenerarReporte(DataTable dt, string f1, string f2)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Documento horizontal para que quepan las cifras
                Document doc = new Document(PageSize.LETTER.Rotate(), 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // --- ENCABEZADO ---
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
                Paragraph p2 = new Paragraph("REPORTE DE UTILIDADES", fSub); p2.Alignment = Element.ALIGN_RIGHT;
                Paragraph p3 = new Paragraph($"Periodo: {f1} al {f2}", fSub); p3.Alignment = Element.ALIGN_RIGHT;

                cTitle.AddElement(p1); cTitle.AddElement(p2); cTitle.AddElement(p3);
                header.AddCell(cTitle);

                doc.Add(header);
                doc.Add(new Paragraph("\n"));

                // --- TABLA DATOS ---
                PdfPTable table = new PdfPTable(6); // Factura, Cliente, Fecha, Costo, Venta, Utilidad
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1.2f, 3f, 1.5f, 1.5f, 1.5f, 1.5f });

                string[] heads = { "FACTURA", "CLIENTE", "FECHA", "COSTO TOTAL", "VENTA TOTAL", "UTILIDAD" };
                foreach (string h in heads)
                {
                    PdfPCell c = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                    c.BackgroundColor = _bgDark;
                    c.HorizontalAlignment = Element.ALIGN_CENTER;
                    c.Padding = 6;
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

                    sumCosto += costo;
                    sumVenta += venta;
                    sumUtilidad += util;

                    AddCell(table, r["FacturaNo"].ToString(), fRow, bg);
                    AddCell(table, r["Cliente"].ToString(), fRow, bg);
                    AddCell(table, Convert.ToDateTime(r["Fecha"]).ToString("dd/MM/yyyy"), fRow, bg, Element.ALIGN_CENTER);
                    AddCell(table, costo.ToString("C"), fRow, bg, Element.ALIGN_RIGHT);
                    AddCell(table, venta.ToString("C"), fRow, bg, Element.ALIGN_RIGHT);

                    // Celda utilidad con color
                    PdfPCell cUtil = new PdfPCell(new Phrase(util.ToString("C"),
                        FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.BOLD, util >= 0 ? _green : _red)));
                    cUtil.BackgroundColor = bg;
                    cUtil.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cUtil.Padding = 4;
                    cUtil.BorderColor = _bgDark;
                    table.AddCell(cUtil);

                    alternate = !alternate;
                }

                // --- FILA DE TOTALES ---
                PdfPCell cTotalLabel = new PdfPCell(new Phrase("TOTALES GENERALES", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                cTotalLabel.Colspan = 3;
                cTotalLabel.BackgroundColor = _bgDark;
                cTotalLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
                cTotalLabel.Padding = 6;
                table.AddCell(cTotalLabel);

                AddTotalCell(table, sumCosto.ToString("C"));
                AddTotalCell(table, sumVenta.ToString("C"));

                // Total Utilidad con color en texto pero fondo oscuro
                PdfPCell cTotalUtil = new PdfPCell(new Phrase(sumUtilidad.ToString("C"),
                    FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, sumUtilidad >= 0 ? _green : _red))); // Verde o Rojo sobre fondo oscuro se ve mal, mejor usamos dorado o blanco si es positivo

                // Ajuste visual: Si es positivo, blanco o dorado. Si es negativo, rojo claro.
                BaseColor colorFinal = sumUtilidad >= 0 ? _gold : new BaseColor(255, 100, 100);
                cTotalUtil.Phrase = new Phrase(sumUtilidad.ToString("C"), FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, colorFinal));

                cTotalUtil.BackgroundColor = _bgDark;
                cTotalUtil.HorizontalAlignment = Element.ALIGN_RIGHT;
                cTotalUtil.Padding = 6;
                table.AddCell(cTotalUtil);

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

        private void AddTotalCell(PdfPTable t, string txt)
        {
            PdfPCell c = new PdfPCell(new Phrase(txt, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
            c.BackgroundColor = _bgDark;
            c.HorizontalAlignment = Element.ALIGN_RIGHT;
            c.Padding = 6;
            t.AddCell(c);
        }
    }
}