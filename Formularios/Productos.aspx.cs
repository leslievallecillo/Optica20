using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Productos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            gvProductos.RowDataBound += new GridViewRowEventHandler(gvProductos_RowDataBound);

            if (!IsPostBack)
            {
                txtFecIni.Text = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");
                txtFecFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarCategorias();
                CargarProductos();
            }
        }

        protected void gvProductos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView drv = (DataRowView)e.Row.DataItem;
                if (drv != null)
                {
                    int stock = Convert.ToInt32(drv["Stock"]);
                    int estado = Convert.ToInt32(drv["Estado"]);

                    if (stock <= 3 && estado == 1)
                    {
                        e.Row.BackColor = System.Drawing.Color.FromArgb(255, 204, 204);
                        e.Row.ForeColor = System.Drawing.Color.DarkRed;
                        e.Row.Font.Bold = true;
                    }
                }
            }
        }

        protected void ddlCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cat = ddlCategoria.SelectedItem.Text.ToLower();
            if (cat.Contains("aro") || cat.Contains("lente") || cat.Contains("marco") || cat.Contains("montura"))
            {
                pnlDetallesOptica.Visible = true;
            }
            else
            {
                pnlDetallesOptica.Visible = false;
                txtMarca.Text = ""; txtModelo.Text = ""; txtTipoAro.Text = ""; txtColor.Text = "";
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "open", "openModal();", true);
        }

        private void CargarCategorias()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT ID_Categoria, Descripcion FROM categoria WHERE Estado=1", con);
                DataTable dt = new DataTable(); da.Fill(dt);

                ddlFiltroCat.DataSource = dt; ddlFiltroCat.DataTextField = "Descripcion"; ddlFiltroCat.DataValueField = "ID_Categoria";
                ddlFiltroCat.DataBind(); ddlFiltroCat.Items.Insert(0, new ListItem("Todas", "0"));

                ddlCategoria.DataSource = dt; ddlCategoria.DataTextField = "Descripcion"; ddlCategoria.DataValueField = "ID_Categoria";
                ddlCategoria.DataBind(); ddlCategoria.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
            }
        }

        private void CargarProductos()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                string sql = @"SELECT p.ID_Producto, p.Codigo, c.Descripcion as Categoria, p.Descripcion, 
                                      p.TipoAro, p.Marca, p.Modelo, p.Color,
                                      p.Precio, p.Stock, p.FechaRegistro, p.Estado, p.RutaImagen 
                               FROM producto p 
                               INNER JOIN categoria c ON p.ID_Categoria = c.ID_Categoria 
                               WHERE 1=1";

                if (!string.IsNullOrEmpty(txtBuscar.Text))
                    sql += " AND (p.Codigo LIKE @B OR p.Descripcion LIKE @B OR p.Marca LIKE @B OR p.Modelo LIKE @B)";

                if (ddlFiltroCat.SelectedValue != "0") sql += " AND p.ID_Categoria = @C";
                if (ddlFiltroEstado.SelectedValue != "-1") sql += " AND p.Estado = @E";
                if (!string.IsNullOrEmpty(txtFecIni.Text) && !string.IsNullOrEmpty(txtFecFin.Text))
                    sql += " AND p.FechaRegistro BETWEEN @F1 AND @F2";

                sql += " ORDER BY p.Descripcion ASC";

                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@B", "%" + txtBuscar.Text + "%");
                cmd.Parameters.AddWithValue("@C", ddlFiltroCat.SelectedValue);
                cmd.Parameters.AddWithValue("@E", ddlFiltroEstado.SelectedValue);
                cmd.Parameters.AddWithValue("@F1", txtFecIni.Text);
                cmd.Parameters.AddWithValue("@F2", txtFecFin.Text);

                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                gvProductos.DataSource = dt;
                gvProductos.DataBind();

                System.Collections.Generic.List<string> productosBajos = new System.Collections.Generic.List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToInt32(row["Stock"]) <= 3 && Convert.ToInt32(row["Estado"]) == 1)
                    {
                        productosBajos.Add(row["Descripcion"].ToString());
                    }
                }

                if (productosBajos.Count > 0)
                {
                    string lista = string.Join(", ", productosBajos);
                    string mensaje = "Alerta: Se tiene que comprar producto. Los siguientes artículos han llegado al stock mínimo (3 unidades o menos): " + lista;
                    ScriptManager.RegisterStartupScript(this, GetType(), "StockAlert", $"Swal.fire('Stock Bajo', '{mensaje}', 'warning');", true);
                }
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e) { CargarProductos(); }
        protected void gvProductos_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvProductos.PageIndex = e.NewPageIndex; CargarProductos(); }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarModal();
            txtFechaReg.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ScriptManager.RegisterStartupScript(this, GetType(), "open", "openModal();", true);
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCodigo.Text) || string.IsNullOrEmpty(txtDescripcion.Text) || ddlCategoria.SelectedValue == "0")
            {
                MostrarMensaje("Los campos obligatorios están vacíos.", "warning"); return;
            }

            string nuevaRuta = GuardarImagenProducto();

            if (fuImagen.HasFile && string.IsNullOrEmpty(nuevaRuta))
            {
                MostrarMensaje("Error al subir la imagen o formato inválido (solo .png, .jpg, .jpeg).", "error");
                return;
            }

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                try
                {
                    string query = "";
                    bool esNuevo = string.IsNullOrEmpty(hfIdProducto.Value);

                    if (esNuevo)
                        query = @"INSERT INTO producto (Codigo, ID_Categoria, Descripcion, TipoAro, Marca, Modelo, Color, Precio, Stock, FechaRegistro, RutaImagen, Estado) 
                                  VALUES (@Cod, @Cat, @Desc, @Tip, @Mar, @Mod, @Col, @Pre, @Stk, CURDATE(), @Img, 1)";
                    else
                        query = @"UPDATE producto SET Codigo=@Cod, ID_Categoria=@Cat, Descripcion=@Desc, 
                                  TipoAro=@Tip, Marca=@Mar, Modelo=@Mod, Color=@Col, Precio=@Pre, Stock=@Stk" +
                                  (nuevaRuta != null ? ", RutaImagen=@Img" : "") +
                                  " WHERE ID_Producto=@ID";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Cod", txtCodigo.Text.Trim().ToUpper());
                    cmd.Parameters.AddWithValue("@Cat", ddlCategoria.SelectedValue);
                    cmd.Parameters.AddWithValue("@Desc", txtDescripcion.Text.Trim());

                    if (pnlDetallesOptica.Visible)
                    {
                        cmd.Parameters.AddWithValue("@Tip", txtTipoAro.Text.Trim());
                        cmd.Parameters.AddWithValue("@Mar", txtMarca.Text.Trim());
                        cmd.Parameters.AddWithValue("@Mod", txtModelo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Col", txtColor.Text.Trim());
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Tip", DBNull.Value);
                        cmd.Parameters.AddWithValue("@Mar", DBNull.Value);
                        cmd.Parameters.AddWithValue("@Mod", DBNull.Value);
                        cmd.Parameters.AddWithValue("@Col", DBNull.Value);
                    }

                    decimal precioVal = 0;
                    decimal.TryParse(txtPrecio.Text, out precioVal);
                    cmd.Parameters.AddWithValue("@Pre", precioVal);

                    cmd.Parameters.AddWithValue("@Stk", txtStock.Text);

                    if (esNuevo) cmd.Parameters.AddWithValue("@Img", nuevaRuta ?? (object)DBNull.Value);
                    else if (nuevaRuta != null) cmd.Parameters.AddWithValue("@Img", nuevaRuta);

                    if (!esNuevo) cmd.Parameters.AddWithValue("@ID", hfIdProducto.Value);

                    cmd.ExecuteNonQuery();
                    CargarProductos();
                    ScriptManager.RegisterStartupScript(this, GetType(), "ok", "closeModal(); Swal.fire('Producto', 'Información guardada correctamente.', 'success');", true);
                }
                catch (Exception ex) { MostrarMensaje("Error al guardar: " + ex.Message, "error"); }
            }
        }

        private string GuardarImagenProducto()
        {
            if (fuImagen.HasFile)
            {
                string ext = Path.GetExtension(fuImagen.FileName).ToLower();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") return null;
                if (fuImagen.PostedFile.ContentLength > 5 * 1024 * 1024) return null;

                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("Authorization", "Client-ID 546c25a59c58ad7");
                        System.Collections.Specialized.NameValueCollection req = new System.Collections.Specialized.NameValueCollection();
                        req.Add("image", Convert.ToBase64String(fuImagen.FileBytes));
                        byte[] response = wc.UploadValues("https://api.imgur.com/3/image", "POST", req);
                        string json = Encoding.UTF8.GetString(response);

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        dynamic data = js.Deserialize<dynamic>(json);
                        return data["data"]["link"];
                    }
                }
                catch { return null; }
            }
            return null;
        }

        protected void gvProductos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                CargarDatosEditar(Convert.ToInt32(e.CommandArgument));
            }
            else if (e.CommandName == "DarBaja")
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    new MySqlCommand("UPDATE producto SET Estado=0 WHERE ID_Producto=" + e.CommandArgument, con).ExecuteNonQuery();
                }
                CargarProductos();
            }
        }

        private void CargarDatosEditar(int id)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM producto WHERE ID_Producto=" + id, con);
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    hfIdProducto.Value = id.ToString();
                    txtCodigo.Text = dr["Codigo"].ToString();
                    txtDescripcion.Text = dr["Descripcion"].ToString();
                    txtTipoAro.Text = dr["TipoAro"].ToString();
                    txtMarca.Text = dr["Marca"].ToString();
                    txtModelo.Text = dr["Modelo"].ToString();
                    txtColor.Text = dr["Color"].ToString();
                    txtPrecio.Text = dr["Precio"].ToString();
                    txtStock.Text = dr["Stock"].ToString();
                    ddlCategoria.SelectedValue = dr["ID_Categoria"].ToString();
                    txtFechaReg.Text = Convert.ToDateTime(dr["FechaRegistro"]).ToString("yyyy-MM-dd");

                    string rutaDB = dr["RutaImagen"].ToString();
                    if (!string.IsNullOrEmpty(rutaDB))
                    {
                        if (rutaDB.StartsWith("http"))
                        {
                            imgProductoPreview.ImageUrl = rutaDB;
                        }
                        else if (File.Exists(Server.MapPath(rutaDB)))
                        {
                            string rutaConCache = rutaDB + "?t=" + DateTime.Now.Ticks;
                            imgProductoPreview.ImageUrl = rutaConCache;
                        }
                        else
                        {
                            imgProductoPreview.ImageUrl = "~/Images/default-product.png";
                        }
                    }
                    else
                    {
                        imgProductoPreview.ImageUrl = "~/Images/default-product.png";
                    }

                    ddlCategoria_SelectedIndexChanged(null, null);
                    upModal.Update();
                    ScriptManager.RegisterStartupScript(this, GetType(), "open", "openModal();", true);
                }
            }
        }

        private void LimpiarModal()
        {
            hfIdProducto.Value = "";
            txtCodigo.Text = ""; txtDescripcion.Text = ""; txtPrecio.Text = "0.00"; txtStock.Text = "0";
            txtTipoAro.Text = ""; txtMarca.Text = ""; txtModelo.Text = ""; txtColor.Text = "";
            ddlCategoria.SelectedIndex = 0;
            imgProductoPreview.ImageUrl = "~/Images/default-product.png";
            pnlDetallesOptica.Visible = false;
            upModal.Update();
        }

        private void MostrarMensaje(string texto, string icono)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Toast", $"Swal.fire('Producto', '{texto}', '{icono}');", true);
        }
    }
}