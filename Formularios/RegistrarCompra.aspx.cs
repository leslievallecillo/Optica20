using System;
using System.Data;
using System.IO;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web.Services;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Compras
{
    public partial class RegistrarCompra : System.Web.UI.Page
    {
        private DataTable DtItems
        {
            get
            {
                if (ViewState["DtItems"] == null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ID_Producto");
                    dt.Columns.Add("Producto");
                    dt.Columns.Add("Cantidad", typeof(int));
                    dt.Columns.Add("PrecioUnitario", typeof(decimal));
                    dt.Columns.Add("Iva", typeof(decimal));
                    dt.Columns.Add("PrecioTotal", typeof(decimal));
                    dt.Columns.Add("PrecioVenta", typeof(decimal));
                    ViewState["DtItems"] = dt;
                }
                return (DataTable)ViewState["DtItems"];
            }
            set { ViewState["DtItems"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarListas();
                GenerarCodigoVisual();

                if (Session["UsuarioID"] != null)
                {
                    CargarUsuario();
                }
            }
        }

        private void CargarUsuario()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "SELECT CONCAT(Nombres, ' ', Apellidos) FROM usuario WHERE ID_Usuario = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@id", Session["UsuarioID"]);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        lblUsuario.Text = result.ToString();
                    }
                }
                catch { }
            }

            lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void GenerarCodigoVisual()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT IFNULL(MAX(ID_Compra), 0) + 1 FROM compra", con);
                    txtNumeroCompra.Text = "COM-" + int.Parse(cmd.ExecuteScalar().ToString()).ToString("D6");
                }
                catch { txtNumeroCompra.Text = "COM-000001"; }
            }
        }

        private void CargarListas()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlDataAdapter daP = new MySqlDataAdapter("SELECT ID_Proveedor, RazonSocial FROM proveedor WHERE Estado=1 ORDER BY RazonSocial", con);
                    DataTable dtP = new DataTable(); daP.Fill(dtP);
                    ddlProveedor.DataSource = dtP; ddlProveedor.DataTextField = "RazonSocial"; ddlProveedor.DataValueField = "ID_Proveedor";
                    ddlProveedor.DataBind(); ddlProveedor.Items.Insert(0, new ListItem("-- Seleccione --", "0"));

                    MySqlDataAdapter daC = new MySqlDataAdapter("SELECT ID_Categoria, Descripcion FROM categoria WHERE Estado=1", con);
                    DataTable dtC = new DataTable(); daC.Fill(dtC);
                    ddlCatNueva.DataSource = dtC; ddlCatNueva.DataTextField = "Descripcion"; ddlCatNueva.DataValueField = "ID_Categoria";
                    ddlCatNueva.DataBind(); ddlCatNueva.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
                }
            }
            catch (Exception ex) { MostrarAlerta("Error: " + ex.Message, "error"); }
        }

        [WebMethod]
        public static List<object> BuscarProductos(string prefijo)
        {
            List<object> result = new List<object>();
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                string query = "SELECT ID_Producto, Codigo, Descripcion FROM producto WHERE Estado = 1 AND (Codigo LIKE @p OR Descripcion LIKE @p) LIMIT 15";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@p", "%" + prefijo + "%");
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            result.Add(new
                            {
                                label = sdr["Codigo"].ToString() + " - " + sdr["Descripcion"].ToString(),
                                val = sdr["ID_Producto"].ToString(),
                                desc = sdr["Descripcion"].ToString()
                            });
                        }
                    }
                }
            }
            return result;
        }

        [WebMethod]
        public static object ObtenerDetallesProducto(int idProducto)
        {
            decimal ultimoCosto = 0, precioActual = 0;
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                object resProd = new MySqlCommand($"SELECT Precio FROM producto WHERE ID_Producto={idProducto}", con).ExecuteScalar();
                if (resProd != null && resProd != DBNull.Value) precioActual = Convert.ToDecimal(resProd);

                object resCosto = new MySqlCommand($"SELECT PrecioUnitario FROM detallecompra WHERE ID_Producto={idProducto} ORDER BY ID_DetalleCompra DESC LIMIT 1", con).ExecuteScalar();
                if (resCosto != null && resCosto != DBNull.Value) ultimoCosto = Convert.ToDecimal(resCosto);
            }
            return new { costo = ultimoCosto, precioVenta = precioActual };
        }

        // --- AGREGAR ITEM AL CARRITO ---
        protected void btnModalAdd_Click(object sender, EventArgs e)
        {
            lblErrorModal.Visible = false;
            string idProdSelec = hfIdProductoSeleccionado.Value;
            string nombreProdSelec = hfNombreProductoSeleccionado.Value;

            if (string.IsNullOrEmpty(idProdSelec) || idProdSelec == "0") { MostrarErrorModal("Seleccione un producto."); return; }
            if (string.IsNullOrEmpty(txtCantidad.Text)) { MostrarErrorModal("Ingrese cantidad."); return; }
            if (string.IsNullOrEmpty(txtPrecioUnitario.Text)) { MostrarErrorModal("Ingrese costo."); return; }

            try
            {
                int cant = int.Parse(txtCantidad.Text);
                if (!decimal.TryParse(txtPrecioUnitario.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo))
                { MostrarErrorModal("Costo inválido."); return; }

                decimal venta = 0;
                if (!string.IsNullOrEmpty(txtPrecioVenta.Text))
                    decimal.TryParse(txtPrecioVenta.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out venta);

                decimal iva = costo * 0.15m;
                decimal totalLinea = (costo + iva) * cant;
                decimal costoTotalConIva = costo + iva;

                // Validación Estricta
                if (venta <= costoTotalConIva)
                {
                    MostrarErrorModal($"ALERTA: El precio de venta (C$ {venta:N2}) debe ser MAYOR al costo con IVA (C$ {costoTotalConIva:N2}).");
                    return;
                }

                DataTable dt = DtItems;
                bool existe = false;
                foreach (DataRow row in dt.Rows)
                {
                    if (row["ID_Producto"].ToString() == idProdSelec)
                    {
                        row["Cantidad"] = Convert.ToInt32(row["Cantidad"]) + cant;
                        row["PrecioTotal"] = Convert.ToDecimal(row["PrecioTotal"]) + totalLinea;
                        row["PrecioUnitario"] = costo; row["Iva"] = iva; row["PrecioVenta"] = venta;
                        existe = true; break;
                    }
                }

                if (!existe)
                {
                    DataRow r = dt.NewRow();
                    r["ID_Producto"] = idProdSelec; r["Producto"] = nombreProdSelec; r["Cantidad"] = cant;
                    r["PrecioUnitario"] = costo; r["Iva"] = iva; r["PrecioTotal"] = totalLinea; r["PrecioVenta"] = venta;
                    dt.Rows.Add(r);
                }

                DtItems = dt; RefrescarGrid();
                txtBusquedaProducto.Text = ""; hfIdProductoSeleccionado.Value = ""; hfNombreProductoSeleccionado.Value = "";
                txtCantidad.Text = ""; txtPrecioUnitario.Text = ""; txtIva.Text = "0.00"; txtPrecioVenta.Text = "";
                ScriptManager.RegisterStartupScript(this, GetType(), "close", "closeModal();", true);
            }
            catch (Exception ex) { MostrarErrorModal("Error: " + ex.Message); }
        }

        // --- CREAR PRODUCTO AL VUELO ---
        protected void btnGuardarNuevoProd_Click(object sender, EventArgs e)
        {
            lblErrorNuevoProd.Visible = false;
            if (ddlCatNueva.SelectedValue == "0" || string.IsNullOrEmpty(txtCodNuevo.Text) || string.IsNullOrEmpty(txtDescNueva.Text))
            {
                lblErrorNuevoProd.Text = "Código, Categoría y Descripción son obligatorios.";
                lblErrorNuevoProd.Visible = true; return;
            }

            string nuevaRuta = null;
            if (fuImagenNueva.HasFile)
            {
                string ext = Path.GetExtension(fuImagenNueva.FileName).ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                {
                    string rutaFolder = Server.MapPath("~/Uploads/Productos/");
                    if (!Directory.Exists(rutaFolder)) Directory.CreateDirectory(rutaFolder);
                    string nombreArchivo = "prod_" + DateTime.Now.Ticks + ext;
                    fuImagenNueva.SaveAs(Path.Combine(rutaFolder, nombreArchivo));
                    nuevaRuta = "~/Uploads/Productos/" + nombreArchivo;
                }
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string sql = @"INSERT INTO producto (Codigo, ID_Categoria, Descripcion, Marca, Modelo, TipoAro, Color, RutaImagen, Stock, Precio, Estado, FechaRegistro)
                                   VALUES (@Cod, @Cat, @Desc, @Mar, @Mod, @Aro, @Col, @Img, 0, 0.00, 1, CURDATE()); SELECT LAST_INSERT_ID();";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Cod", txtCodNuevo.Text.Trim().ToUpper());
                    cmd.Parameters.AddWithValue("@Cat", ddlCatNueva.SelectedValue);
                    cmd.Parameters.AddWithValue("@Desc", txtDescNueva.Text.Trim());
                    cmd.Parameters.AddWithValue("@Mar", string.IsNullOrEmpty(txtMarcaNueva.Text) ? (object)DBNull.Value : txtMarcaNueva.Text.Trim());
                    cmd.Parameters.AddWithValue("@Mod", string.IsNullOrEmpty(txtModeloNuevo.Text) ? (object)DBNull.Value : txtModeloNuevo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Aro", string.IsNullOrEmpty(txtAroNuevo.Text) ? (object)DBNull.Value : txtAroNuevo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Col", string.IsNullOrEmpty(txtColorNuevo.Text) ? (object)DBNull.Value : txtColorNuevo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Img", nuevaRuta ?? (object)DBNull.Value);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());

                    hfIdProductoSeleccionado.Value = newId.ToString();
                    hfNombreProductoSeleccionado.Value = txtDescNueva.Text;
                    txtBusquedaProducto.Text = txtCodNuevo.Text.ToUpper() + " - " + txtDescNueva.Text;

                    txtCodNuevo.Text = ""; txtDescNueva.Text = ""; ddlCatNueva.SelectedIndex = 0;
                    txtMarcaNueva.Text = ""; txtModeloNuevo.Text = ""; txtAroNuevo.Text = ""; txtColorNuevo.Text = "";
                    imgNuevoProdPreview.ImageUrl = "~/Images/default-product.png";

                    ScriptManager.RegisterStartupScript(this, GetType(), "closeNew", "cerrarModalNuevoProducto(); setTimeout(function(){ document.getElementById('" + txtCantidad.ClientID + "').focus(); }, 200);", true);
                }
            }
            catch (Exception ex) { lblErrorNuevoProd.Text = "Error: " + ex.Message; lblErrorNuevoProd.Visible = true; }
        }

        // --- FINALIZAR COMPRA ---
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (ddlProveedor.SelectedValue == "0" || DtItems.Rows.Count == 0) { MostrarAlerta("Datos incompletos.", "warning"); return; }
            int idUser = Session["ID_Usuario"] != null ? Convert.ToInt32(Session["ID_Usuario"]) : 1;

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlTransaction tr = con.BeginTransaction();
                try
                {
                    string sqlGetId = "SELECT IFNULL(MAX(ID_Compra), 0) + 1 FROM compra FOR UPDATE";
                    int nextId = Convert.ToInt32(new MySqlCommand(sqlGetId, con, tr).ExecuteScalar());

                    string sqlCompra = @"INSERT INTO compra (ID_Proveedor, ID_Usuario, NumeroCompra, Fecha, FechaRegistro, Estado) VALUES (@Prov, @User, @Num, NOW(), CURDATE(), 1); SELECT LAST_INSERT_ID();";
                    MySqlCommand cmdC = new MySqlCommand(sqlCompra, con, tr);
                    cmdC.Parameters.AddWithValue("@Prov", ddlProveedor.SelectedValue);
                    cmdC.Parameters.AddWithValue("@User", idUser);
                    cmdC.Parameters.AddWithValue("@Num", "COM-" + nextId.ToString("D6"));
                    long idCompra = Convert.ToInt64(cmdC.ExecuteScalar());

                    foreach (DataRow r in DtItems.Rows)
                    {
                        string sqlDet = "INSERT INTO detallecompra (ID_Compra, ID_Producto, Cantidad, PrecioUnitario, Iva, PrecioTotal, PrecioVenta, FechaRegistro, Estado) VALUES (@IDC, @IDP, @Cant, @Uni, @Iv, @Tot, @Ven, CURDATE(), 1)";
                        MySqlCommand cmdD = new MySqlCommand(sqlDet, con, tr);
                        cmdD.Parameters.AddWithValue("@IDC", idCompra); cmdD.Parameters.AddWithValue("@IDP", r["ID_Producto"]); cmdD.Parameters.AddWithValue("@Cant", r["Cantidad"]);
                        cmdD.Parameters.AddWithValue("@Uni", r["PrecioUnitario"]); cmdD.Parameters.AddWithValue("@Iv", r["Iva"]); cmdD.Parameters.AddWithValue("@Tot", r["PrecioTotal"]); cmdD.Parameters.AddWithValue("@Ven", r["PrecioVenta"]);
                        cmdD.ExecuteNonQuery();

                        string sqlUpd = "UPDATE producto SET Stock = Stock + @Cant, Precio = @Ven WHERE ID_Producto = @IDP";
                        MySqlCommand cmdU = new MySqlCommand(sqlUpd, con, tr);
                        cmdU.Parameters.AddWithValue("@Cant", r["Cantidad"]); cmdU.Parameters.AddWithValue("@Ven", r["PrecioVenta"]); cmdU.Parameters.AddWithValue("@IDP", r["ID_Producto"]);
                        cmdU.ExecuteNonQuery();
                    }

                    tr.Commit();
                    ViewState["DtItems"] = null;
                    ScriptManager.RegisterStartupScript(this, GetType(), "Succ", "Swal.fire('Registrada', 'Inventario actualizado.', 'success').then(() => { window.location = 'GestionCompras.aspx'; });", true);
                }
                catch (Exception ex) { tr.Rollback(); MostrarAlerta("ERROR: " + ex.Message, "error"); }
            }
        }

        protected void btnAddItem_Click(object sender, EventArgs e) { }
        protected void btnVolver_Click(object sender, EventArgs e) { Response.Redirect("GestionCompras.aspx"); }
        protected void btnCancelar_Click(object sender, EventArgs e) { Response.Redirect("GestionCompras.aspx"); }

        protected void gvDetalle_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Borrar") { DataTable dt = DtItems; dt.Rows.RemoveAt(Convert.ToInt32(e.CommandArgument)); DtItems = dt; RefrescarGrid(); }
        }

        private void RefrescarGrid()
        {
            gvDetalle.DataSource = DtItems; gvDetalle.DataBind();
            decimal t = 0; foreach (DataRow r in DtItems.Rows) t += Convert.ToDecimal(r["PrecioTotal"]);
            lblTotal.Text = "C$ " + t.ToString("N2"); upDetalle.Update();
        }

        private void MostrarErrorModal(string m) { lblErrorModal.Text = m; lblErrorModal.Visible = true; ScriptManager.RegisterStartupScript(this, GetType(), "k", "openModal();", true); }
        private void MostrarAlerta(string m, string t) { ScriptManager.RegisterStartupScript(this, GetType(), "a", $"Swal.fire('Sistema', '{m}', '{t}');", true); }
    }
}