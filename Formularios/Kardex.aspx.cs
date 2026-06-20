using System;
using System.Data;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Reportes
{
    public partial class Kardex : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) CargarProductos();
        }

        private void CargarProductos()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT ID_Producto, CONCAT(Codigo, ' - ', Descripcion) as Nombre FROM producto ORDER BY Descripcion", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlProducto.DataSource = dt;
                ddlProducto.DataTextField = "Nombre";
                ddlProducto.DataValueField = "ID_Producto";
                ddlProducto.DataBind();
                ddlProducto.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
            }
        }

        protected void ddlProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlProducto.SelectedValue != "0") GenerarKardex(ddlProducto.SelectedValue);
            else
            {
                gvKardex.DataSource = null;
                gvKardex.DataBind();
                txtStockActual.Text = "";
            }
        }

        private void GenerarKardex(string idProd)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();

                // 1. Obtener Stock Actual
                MySqlCommand cmdStock = new MySqlCommand("SELECT Stock FROM producto WHERE ID_Producto=" + idProd, con);
                object stock = cmdStock.ExecuteScalar();
                txtStockActual.Text = stock != null ? stock.ToString() : "0";

                // 2. Obtener Movimientos (UNION de Compras y Ventas)
                string sql = @"
                    SELECT * FROM (
                        -- ENTRADAS (Compras)
                        SELECT c.FechaRegistro as Fecha, 'ENTRADA (COMPRA)' as Tipo, c.NumeroCompra as Documento, dc.Cantidad
                        FROM detallecompra dc
                        INNER JOIN compra c ON dc.ID_Compra = c.ID_Compra
                        WHERE dc.ID_Producto = @ID AND c.Estado = 1

                        UNION ALL

                        -- SALIDAS (Ventas Productos)
                        SELECT v.FechaRegistro as Fecha, 'SALIDA (VENTA)' as Tipo, v.NumeroDocumento as Documento, dv.Cantidad
                        FROM detalleventaproducto dv
                        INNER JOIN venta v ON dv.ID_Venta = v.ID_Venta
                        WHERE dv.ID_Producto = @ID AND v.Estado = 1
                    ) as Movimientos
                    ORDER BY Fecha DESC";

                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ID", idProd);
                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                gvKardex.DataSource = dt;
                gvKardex.DataBind();
            }
        }
    }
}