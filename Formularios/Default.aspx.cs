using System;
using System.Data;
using System.Web.UI;
using System.Web.Script.Serialization; // Para convertir arrays a JSON para el gráfico
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Default : System.Web.UI.Page
    {
        // Variables para enviar datos al JavaScript
        public string JsonVentas = "[]";
        public string JsonCompras = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarKPIs();
                CargarGrafico();
                CargarTablas();
            }
        }

        private void CargarKPIs()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();

                    // 1. Inventario Neto (Stock * Último Precio Venta registrado en compras)
                    // Nota: Si no hay precio en compra, asume 0.
                    string sqlInv = @"SELECT 
                                        SUM(p.Stock) as StockTotal,
                                        SUM(p.Stock * IFNULL((SELECT PrecioVenta FROM DetalleCompra WHERE ID_Producto = p.ID_Producto ORDER BY ID_DetalleCompra DESC LIMIT 1), 0)) as ValorNeto
                                      FROM Producto p WHERE p.Estado = 1";

                    MySqlCommand cmdInv = new MySqlCommand(sqlInv, con);
                    using (MySqlDataReader r = cmdInv.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblStock.Text = r["StockTotal"] != DBNull.Value ? r["StockTotal"].ToString() : "0";
                            lblInvNeto.Text = r["ValorNeto"] != DBNull.Value ? Convert.ToDecimal(r["ValorNeto"]).ToString("N2") : "0.00";
                        }
                    }

                    // 2. Ventas (Año Actual)
                    string sqlVentas = @"SELECT 
                                            COUNT(*) as Cantidad,
                                            SUM(
                                                (SELECT IFNULL(SUM(Subtotal),0) FROM DetalleVentaProducto WHERE ID_Venta=v.ID_Venta) + 
                                                (SELECT IFNULL(SUM(Subtotal),0) FROM DetalleVentaLentes WHERE ID_Venta=v.ID_Venta)
                                            ) as Total
                                         FROM Venta v WHERE YEAR(Fecha) = YEAR(CURDATE()) AND Estado = 1";

                    MySqlCommand cmdVentas = new MySqlCommand(sqlVentas, con);
                    using (MySqlDataReader r = cmdVentas.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblVentasCount.Text = r["Cantidad"].ToString();
                            lblVentasTotal.Text = r["Total"] != DBNull.Value ? Convert.ToDecimal(r["Total"]).ToString("N2") : "0.00";
                        }
                    }

                    // 3. Compras (Año Actual)
                    string sqlCompras = @"SELECT 
                                            COUNT(*) as Cantidad,
                                            SUM((SELECT IFNULL(SUM(PrecioTotal),0) FROM DetalleCompra WHERE ID_Compra=c.ID_Compra)) as Total
                                          FROM Compra c WHERE YEAR(Fecha) = YEAR(CURDATE()) AND Estado = 1";

                    MySqlCommand cmdCompras = new MySqlCommand(sqlCompras, con);
                    using (MySqlDataReader r = cmdCompras.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblComprasCount.Text = r["Cantidad"].ToString();
                            lblComprasTotal.Text = r["Total"] != DBNull.Value ? Convert.ToDecimal(r["Total"]).ToString("N2") : "0.00";
                        }
                    }

                    // 4. Clientes Totales
                    string sqlCli = "SELECT COUNT(*) FROM Clientes WHERE Estado = 1";
                    MySqlCommand cmdCli = new MySqlCommand(sqlCli, con);
                    lblClientesTotal.Text = cmdCli.ExecuteScalar().ToString();
                }
                catch { }
            }
        }

        private void CargarGrafico()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    decimal[] ventasMensuales = new decimal[12];
                    decimal[] comprasMensuales = new decimal[12];

                    // Ventas por mes
                    string qV = @"SELECT MONTH(Fecha) as Mes, 
                                  SUM(
                                    (SELECT IFNULL(SUM(Subtotal),0) FROM DetalleVentaProducto WHERE ID_Venta=v.ID_Venta) + 
                                    (SELECT IFNULL(SUM(Subtotal),0) FROM DetalleVentaLentes WHERE ID_Venta=v.ID_Venta)
                                  ) as Total
                                  FROM Venta v WHERE YEAR(Fecha) = YEAR(CURDATE()) AND Estado = 1 GROUP BY MONTH(Fecha)";

                    MySqlCommand cmdV = new MySqlCommand(qV, con);
                    using (MySqlDataReader r = cmdV.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int m = Convert.ToInt32(r["Mes"]);
                            ventasMensuales[m - 1] = Convert.ToDecimal(r["Total"]);
                        }
                    }

                    // Compras por mes
                    string qC = @"SELECT MONTH(Fecha) as Mes, 
                                  SUM((SELECT IFNULL(SUM(PrecioTotal),0) FROM DetalleCompra WHERE ID_Compra=c.ID_Compra)) as Total
                                  FROM Compra c WHERE YEAR(Fecha) = YEAR(CURDATE()) AND Estado = 1 GROUP BY MONTH(Fecha)";

                    MySqlCommand cmdC = new MySqlCommand(qC, con);
                    using (MySqlDataReader r = cmdC.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int m = Convert.ToInt32(r["Mes"]);
                            comprasMensuales[m - 1] = Convert.ToDecimal(r["Total"]);
                        }
                    }

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    JsonVentas = js.Serialize(ventasMensuales);
                    JsonCompras = js.Serialize(comprasMensuales);
                }
                catch { }
            }
        }

        private void CargarTablas()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();

                    // Próximas 5 Citas (En lugar de Ventas)
                    string sqlC = @"SELECT cit.Fecha, cit.Hora, cit.Motivo,
                                    CASE 
                                        WHEN c.TipoCliente = 'Natural' THEN CONCAT(cn.Nombre, ' ', cn.Apellido)
                                        WHEN c.TipoCliente = 'Juridico' THEN cj.NombreEmpresa
                                        ELSE '---'
                                    END as Cliente
                                    FROM Cita cit
                                    INNER JOIN Clientes c ON cit.ID_Cliente = c.ID_Cliente
                                    LEFT JOIN ClienteNatural cn ON c.ID_Cliente = cn.ID_Cliente
                                    LEFT JOIN ClienteJuridico cj ON c.ID_Cliente = cj.ID_Cliente
                                    WHERE cit.Estado = 1 AND cit.Fecha >= CURDATE()
                                    ORDER BY cit.Fecha ASC, cit.Hora ASC LIMIT 5";

                    DataTable dtC = new DataTable();
                    new MySqlDataAdapter(sqlC, con).Fill(dtC);
                    gvCitasProximas.DataSource = dtC;
                    gvCitasProximas.DataBind();

                    // Productos con Menos Stock (En lugar de Nuevos Productos)
                    string sqlP = @"SELECT p.Descripcion, p.Marca, p.Modelo, p.RutaImagen, p.Stock
                                    FROM Producto p
                                    WHERE p.Estado = 1
                                    ORDER BY p.Stock ASC LIMIT 5";

                    DataTable dtP = new DataTable();
                    new MySqlDataAdapter(sqlP, con).Fill(dtP);
                    rptProductos.DataSource = dtP;
                    rptProductos.DataBind();
                }
                catch { }
            }
        }
    }
}