using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Compras
{
    public partial class GestionCompras : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Filtro inicial
                txtF1.Text = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd");
                txtF2.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarDatos();
            }
        }

        private void CargarDatos()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = @"SELECT c.ID_Compra, c.NumeroCompra, p.RazonSocial as Proveedor, c.Fecha, c.Estado, 
                                   (SELECT IFNULL(SUM(PrecioTotal),0) FROM DetalleCompra WHERE ID_Compra=c.ID_Compra) as Total 
                                   FROM Compra c INNER JOIN Proveedor p ON c.ID_Proveedor=p.ID_Proveedor 
                                   WHERE (c.Fecha BETWEEN @F1 AND @F2)";

                    if (txtBuscar.Text != "") sql += " AND c.NumeroCompra LIKE @B";
                    if (ddlEstado.SelectedValue != "-1") sql += " AND c.Estado=@E";

                    sql += " ORDER BY c.ID_Compra DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@F1", txtF1.Text + " 00:00:00");
                    cmd.Parameters.AddWithValue("@F2", txtF2.Text + " 23:59:59");
                    cmd.Parameters.AddWithValue("@B", "%" + txtBuscar.Text + "%");
                    cmd.Parameters.AddWithValue("@E", ddlEstado.SelectedValue);

                    DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt);
                    gvCompras.DataSource = dt; gvCompras.DataBind();
                }
                catch (Exception ex) { MostrarAlerta("Error: " + ex.Message, "error"); }
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e) { CargarDatos(); }

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlEstado.SelectedValue = "-1";
            txtF1.Text = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd");
            txtF2.Text = DateTime.Now.ToString("yyyy-MM-dd");
            CargarDatos();
        }

        protected void gvCompras_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCompras.PageIndex = e.NewPageIndex;
            CargarDatos();
        }

        protected void gvCompras_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idCompra = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Ver")
            {
                CargarDetalleModal(idCompra);
                ScriptManager.RegisterStartupScript(this, GetType(), "open", "openDetail();", true);
            }
            else if (e.CommandName == "Anular")
            {
                CambiarEstadoCompra(idCompra, 0); // Anular (Resta)
            }
            else if (e.CommandName == "Reactivar")
            {
                CambiarEstadoCompra(idCompra, 1); // Reactivar (Suma y actualiza precio)
            }
        }

        private void CambiarEstadoCompra(int idCompra, int nuevoEstado)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlTransaction tr = con.BeginTransaction();

                try
                {
                    // CORRECCIÓN AQUÍ: Se añade PrecioVenta a la consulta
                    string sqlItems = "SELECT ID_Producto, Cantidad, PrecioVenta FROM DetalleCompra WHERE ID_Compra = @ID";
                    MySqlCommand cmdItems = new MySqlCommand(sqlItems, con, tr);
                    cmdItems.Parameters.AddWithValue("@ID", idCompra);

                    using (MySqlDataReader r = cmdItems.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(r);

                        foreach (DataRow item in dt.Rows)
                        {
                            int idProd = Convert.ToInt32(item["ID_Producto"]);
                            int cant = Convert.ToInt32(item["Cantidad"]);

                            // Validamos que el precio no venga nulo de la BD
                            decimal precioVentaHist = item["PrecioVenta"] != DBNull.Value ? Convert.ToDecimal(item["PrecioVenta"]) : 0;

                            string sqlStock = "";

                            if (nuevoEstado == 0)
                            {
                                // Si anulamos, solo restamos el stock
                                sqlStock = "UPDATE Producto SET Stock = Stock - @Cant WHERE ID_Producto = @IDP";
                            }
                            else if (nuevoEstado == 1)
                            {
                                // Si reactivamos, sumamos el stock y actualizamos el precio
                                sqlStock = "UPDATE Producto SET Stock = Stock + @Cant, Precio = @PrecioV WHERE ID_Producto = @IDP";
                            }

                            if (!string.IsNullOrEmpty(sqlStock))
                            {
                                MySqlCommand cmdUpd = new MySqlCommand(sqlStock, con, tr);
                                cmdUpd.Parameters.AddWithValue("@Cant", cant);
                                cmdUpd.Parameters.AddWithValue("@IDP", idProd);

                                // Si el estado es 1 (Reactivar), pasamos el parámetro del precio
                                if (nuevoEstado == 1)
                                {
                                    cmdUpd.Parameters.AddWithValue("@PrecioV", precioVentaHist);
                                }

                                cmdUpd.ExecuteNonQuery();
                            }
                        }
                    }

                    new MySqlCommand($"UPDATE Compra SET Estado={nuevoEstado} WHERE ID_Compra={idCompra}", con, tr).ExecuteNonQuery();
                    new MySqlCommand($"UPDATE DetalleCompra SET Estado={nuevoEstado} WHERE ID_Compra={idCompra}", con, tr).ExecuteNonQuery();

                    tr.Commit();
                    string msg = nuevoEstado == 0 ? "Compra anulada." : "Compra reactivada. Precios e inventario actualizados.";
                    MostrarAlerta(msg, "success");
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    MostrarAlerta("Error: " + ex.Message, "error");
                }
            }
        }

        private void CargarDetalleModal(int idCompra)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmdT = new MySqlCommand("SELECT NumeroCompra FROM Compra WHERE ID_Compra=" + idCompra, con);
                lblFacturaModal.Text = cmdT.ExecuteScalar()?.ToString();

                string sql = @"SELECT p.Descripcion as Producto, d.Cantidad, d.PrecioUnitario, d.Iva, d.PrecioTotal 
                               FROM DetalleCompra d INNER JOIN Producto p ON d.ID_Producto=p.ID_Producto 
                               WHERE d.ID_Compra=@ID";
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ID", idCompra);
                DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt);
                gvDetalleView.DataSource = dt; gvDetalleView.DataBind();
            }
        }

        protected void btnNuevo_Click(object sender, EventArgs e) { Response.Redirect("RegistrarCompra.aspx"); }

        private void MostrarAlerta(string m, string t)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Alert", $"Swal.fire('Sistema', '{m}', '{t}');", true);
        }
    }
}