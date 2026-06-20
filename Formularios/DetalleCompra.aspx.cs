using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient; // LIBRERÍA ESPECÍFICA DE MYSQL
using Optica.Clases;

namespace Optica.Compras
{
    public partial class DetalleCompra : System.Web.UI.Page
    {
        private string IDCompra { get { return Request.QueryString["ID"]; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(IDCompra)) Response.Redirect("GestionCompras.aspx");
            if (!IsPostBack)
            {
                txtCompraID.Text = IDCompra;
                CargarInfoCompra();
                CargarDetalles();
            }
        }

        private void CargarInfoCompra()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                // Consulta escalar MySQL
                MySqlCommand cmd = new MySqlCommand("SELECT NumeroCompra FROM compra WHERE ID_Compra=" + IDCompra, con);
                object res = cmd.ExecuteScalar();
                if (res != null) lblNumCompra.Text = res.ToString();
            }
        }

        private void CargarDetalles()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                // Consulta MySQL uniendo Detalle y Producto
                string sql = @"SELECT d.ID_DetalleCompra, p.Descripcion as Producto, d.Cantidad, 
                               d.PrecioUnitario, d.Iva, d.PrecioTotal, d.PrecioVenta, d.Estado
                               FROM detallecompra d
                               INNER JOIN producto p ON d.ID_Producto = p.ID_Producto
                               WHERE d.ID_Compra = @IDC";

                if (!string.IsNullOrEmpty(txtBuscar.Text)) sql += " AND p.Descripcion LIKE @B";
                if (ddlEstado.SelectedValue != "-1") sql += " AND d.Estado = @E";

                // MySQL Order By
                sql += " ORDER BY p.Descripcion " + ddlOrden.SelectedValue;

                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@IDC", IDCompra);
                    cmd.Parameters.AddWithValue("@B", "%" + txtBuscar.Text + "%");
                    cmd.Parameters.AddWithValue("@E", ddlEstado.SelectedValue);

                    DataTable dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    gvDetalles.DataSource = dt;
                    gvDetalles.DataBind();
                }
            }
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            PanelListado.Visible = false;
            PanelRegistro.Visible = true;
            txtFechaRegistro.Text = DateTime.Now.ToString("dd/MM/yyyy");
            CargarProductos();
        }

        private void CargarProductos()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT ID_Producto, Descripcion FROM producto WHERE Estado=1", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlProducto.DataSource = dt;
                ddlProducto.DataTextField = "Descripcion";
                ddlProducto.DataValueField = "ID_Producto";
                ddlProducto.DataBind();
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            // Cálculos básicos en C#
            decimal cant = 0, unit = 0, iva = 0, venta = 0;
            decimal.TryParse(txtCantidad.Text, out cant);
            decimal.TryParse(txtPrecioUnitario.Text, out unit);
            decimal.TryParse(txtIva.Text, out iva);
            decimal.TryParse(txtPrecioVenta.Text, out venta);
            decimal total = (unit + iva) * cant;

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                // INICIO DE TRANSACCIÓN MYSQL
                MySqlTransaction trans = con.BeginTransaction();

                try
                {
                    // 1. Insertar Detalle con CURDATE()
                    string sql = @"INSERT INTO detallecompra (ID_Compra, ID_Producto, Cantidad, PrecioUnitario, Iva, PrecioTotal, PrecioVenta, FechaRegistro, Estado)
                                   VALUES (@IDC, @IDP, @Cant, @Unit, @Iva, @Total, @Venta, CURDATE(), 1)";

                    MySqlCommand cmd = new MySqlCommand(sql, con, trans);
                    cmd.Parameters.AddWithValue("@IDC", IDCompra);
                    cmd.Parameters.AddWithValue("@IDP", ddlProducto.SelectedValue);
                    cmd.Parameters.AddWithValue("@Cant", cant);
                    cmd.Parameters.AddWithValue("@Unit", unit);
                    cmd.Parameters.AddWithValue("@Iva", iva);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Venta", venta);
                    cmd.ExecuteNonQuery();

                    // 2. Actualizar Stock de Producto (SUMAR)
                    string sqlStock = "UPDATE producto SET Stock = Stock + @Cant WHERE ID_Producto = @IDP";
                    MySqlCommand cmdStock = new MySqlCommand(sqlStock, con, trans);
                    cmdStock.Parameters.AddWithValue("@Cant", cant);
                    cmdStock.Parameters.AddWithValue("@IDP", ddlProducto.SelectedValue);
                    cmdStock.ExecuteNonQuery();

                    // Confirmar transacción
                    trans.Commit();

                    PanelRegistro.Visible = false;
                    PanelListado.Visible = true;
                    CargarDetalles();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Response.Write("<script>alert('Error MySQL: " + ex.Message + "');</script>");
                }
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e) { PanelRegistro.Visible = false; PanelListado.Visible = true; }
        protected void btnBuscar_Click(object sender, EventArgs e) { CargarDetalles(); }

        protected void gvDetalles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DarBaja")
            {
                DarBajaDetalle(Convert.ToInt32(e.CommandArgument));
            }
        }

        private void DarBajaDetalle(int idDetalle)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                // Obtener datos para restar stock antes de anular
                MySqlCommand cmdSel = new MySqlCommand("SELECT ID_Producto, Cantidad FROM detallecompra WHERE ID_DetalleCompra=" + idDetalle, con);
                MySqlDataReader r = cmdSel.ExecuteReader();
                if (r.Read())
                {
                    int idProd = Convert.ToInt32(r["ID_Producto"]);
                    int cant = Convert.ToInt32(r["Cantidad"]);
                    r.Close(); // Cerrar reader para usar connection

                    MySqlTransaction trans = con.BeginTransaction();
                    try
                    {
                        // Anular Detalle
                        new MySqlCommand("UPDATE detallecompra SET Estado=0 WHERE ID_DetalleCompra=" + idDetalle, con, trans).ExecuteNonQuery();
                        // RESTAR Stock (Revertir compra)
                        new MySqlCommand($"UPDATE producto SET Stock = Stock - {cant} WHERE ID_Producto={idProd}", con, trans).ExecuteNonQuery();

                        trans.Commit();
                    }
                    catch { trans.Rollback(); }
                }
                CargarDetalles();
            }
        }
    }
}