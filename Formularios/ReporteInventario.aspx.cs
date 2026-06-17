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
using ListItem = System.Web.UI.WebControls.ListItem;

namespace Optica.Reportes
{
    public partial class ReporteInventario : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCategorias();
                CargarInventario();
            }
        }

        private void CargarCategorias()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "SELECT ID_Categoria, Descripcion FROM Categoria WHERE Estado = 1 ORDER BY Descripcion";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, con);
                    DataTable dt = new DataTable(); da.Fill(dt);
                    ddlCategoria.DataSource = dt;
                    ddlCategoria.DataTextField = "Descripcion"; ddlCategoria.DataValueField = "ID_Categoria";
                    ddlCategoria.DataBind();
                    ddlCategoria.Items.Insert(0, new ListItem("Todas", "0"));
                }
                catch { }
            }
        }

        private void CargarInventario()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = @"
                        SELECT 
                            p.ID_Producto, p.Codigo, p.Descripcion, p.Stock, p.RutaImagen, p.Marca, p.Modelo,
                            c.Descripcion as Categoria,
                            IFNULL((SELECT PrecioUnitario FROM DetalleCompra WHERE ID_Producto = p.ID_Producto ORDER BY ID_DetalleCompra DESC LIMIT 1), 0) as UltimoCosto,
                            IFNULL((SELECT PrecioVenta FROM DetalleCompra WHERE ID_Producto = p.ID_Producto ORDER BY ID_DetalleCompra DESC LIMIT 1), 0) as PrecioVenta
                        FROM Producto p
                        INNER JOIN Categoria c ON p.ID_Categoria = c.ID_Categoria
                        WHERE p.Estado = 1";

                    if (!string.IsNullOrEmpty(txtBuscar.Text)) sql += " AND (p.Codigo LIKE @B OR p.Descripcion LIKE @B OR p.Marca LIKE @B)";
                    if (ddlCategoria.SelectedValue != "0") sql += " AND p.ID_Categoria = @Cat";
                    if (ddlFiltroStock.SelectedValue == "1") sql += " AND p.Stock < 5";
                    if (ddlFiltroStock.SelectedValue == "2") sql += " AND p.Stock > 0";

                    sql += " ORDER BY p.Stock ASC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    if (!string.IsNullOrEmpty(txtBuscar.Text)) cmd.Parameters.AddWithValue("@B", "%" + txtBuscar.Text + "%");
                    if (ddlCategoria.SelectedValue != "0") cmd.Parameters.AddWithValue("@Cat", ddlCategoria.SelectedValue);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    if (ddlPageSize.SelectedValue != "All")
                    {
                        gvInventario.AllowPaging = true;
                        gvInventario.PageSize = int.Parse(ddlPageSize.SelectedValue);
                    }
                    else gvInventario.AllowPaging = false;

                    gvInventario.DataSource = dt;
                    gvInventario.DataBind();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "err", $"Swal.fire('Error', '{ex.Message}', 'error');", true);
                }
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e) { CargarInventario(); }
        protected void ddlCategoria_SelectedIndexChanged(object sender, EventArgs e) { CargarInventario(); }
        protected void ddlFiltroStock_SelectedIndexChanged(object sender, EventArgs e) { CargarInventario(); }
        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e) { CargarInventario(); }
        protected void gvInventario_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvInventario.PageIndex = e.NewPageIndex; CargarInventario(); }

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
                        SELECT p.Codigo, p.Descripcion, p.Marca, p.Stock, c.Descripcion as Categoria,
                        IFNULL((SELECT PrecioVenta FROM DetalleCompra WHERE ID_Producto = p.ID_Producto ORDER BY ID_DetalleCompra DESC LIMIT 1), 0) as Precio
                        FROM Producto p
                        INNER JOIN Categoria c ON p.ID_Categoria = c.ID_Categoria
                        WHERE p.Estado = 1";

                    if (!string.IsNullOrEmpty(txtBuscar.Text)) sql += " AND (p.Codigo LIKE @B OR p.Descripcion LIKE @B OR p.Marca LIKE @B)";
                    if (ddlCategoria.SelectedValue != "0") sql += " AND p.ID_Categoria = @Cat";
                    if (ddlFiltroStock.SelectedValue == "1") sql += " AND p.Stock < 5";
                    if (ddlFiltroStock.SelectedValue == "2") sql += " AND p.Stock > 0";
                    sql += " ORDER BY p.Descripcion";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    if (!string.IsNullOrEmpty(txtBuscar.Text)) cmd.Parameters.AddWithValue("@B", "%" + txtBuscar.Text + "%");
                    if (ddlCategoria.SelectedValue != "0") cmd.Parameters.AddWithValue("@Cat", ddlCategoria.SelectedValue);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);

                    ReportePdfInventario helper = new ReportePdfInventario(rutaLogo);
                    byte[] pdfBytes = helper.GenerarReporte(dt);

                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", "attachment; filename=Reporte_Inventario.pdf");
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

    public class ReportePdfInventario
    {
        private string _rutaLogo;
        private BaseColor _bgDark = new BaseColor(45, 52, 54);
        private BaseColor _gold = new BaseColor(212, 175, 55);
        private BaseColor _white = new BaseColor(255, 255, 255);
        private BaseColor _black = new BaseColor(0, 0, 0);
        private BaseColor _lightGray = new BaseColor(240, 240, 240);

        public ReportePdfInventario(string rutaLogo) { _rutaLogo = rutaLogo; }

        public byte[] GenerarReporte(DataTable dt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.LETTER, 30, 30, 30, 30);
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
                cTitle.AddElement(new Paragraph("REPORTE DE INVENTARIO", FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.NORMAL, _white)));
                header.AddCell(cTitle);

                doc.Add(header);
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1.5f, 4f, 2f, 1f, 1.5f });

                string[] heads = { "CÓDIGO", "DESCRIPCIÓN", "CATEGORÍA", "STOCK", "PRECIO" };
                foreach (string h in heads)
                {
                    PdfPCell c = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD, _white)));
                    c.BackgroundColor = _bgDark; c.HorizontalAlignment = Element.ALIGN_CENTER; c.Padding = 5;
                    table.AddCell(c);
                }

                Font fRow = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL, _black);
                bool alternate = false;

                foreach (DataRow r in dt.Rows)
                {
                    BaseColor bg = alternate ? _lightGray : _white;
                    AddCell(table, r["Codigo"].ToString(), fRow, bg);
                    AddCell(table, $"{r["Descripcion"]} {r["Marca"]}", fRow, bg);
                    AddCell(table, r["Categoria"].ToString(), fRow, bg);
                    AddCell(table, r["Stock"].ToString(), fRow, bg, Element.ALIGN_CENTER);
                    AddCell(table, Convert.ToDecimal(r["Precio"]).ToString("C"), fRow, bg, Element.ALIGN_RIGHT);
                    alternate = !alternate;
                }
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
    }
}