using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Globalization;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Expediente : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFecha.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarListas();
                CargarGrid();
            }
        }

        private void CargarListas()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string qC = "SELECT ID_Cliente, IF(TipoCliente='Natural', (SELECT CONCAT(Nombre, ' ', Apellido) FROM clientenatural WHERE ID_Cliente = c.ID_Cliente), (SELECT NombreEmpresa FROM clientejuridico WHERE ID_Cliente = c.ID_Cliente)) as Nombre FROM clientes c WHERE Estado = 1 ORDER BY Nombre";
                    DataTable dtC = new DataTable(); new MySqlDataAdapter(qC, con).Fill(dtC);
                    ddlCliente.DataSource = dtC; ddlCliente.DataTextField = "Nombre"; ddlCliente.DataValueField = "ID_Cliente";
                    ddlCliente.DataBind(); ddlCliente.Items.Insert(0, new ListItem("-- Seleccione --", ""));

                    string qP = "SELECT ID_Producto, Descripcion FROM producto WHERE Estado = 1";
                    DataTable dtP = new DataTable(); new MySqlDataAdapter(qP, con).Fill(dtP);
                    ddlProducto.DataSource = dtP; ddlProducto.DataTextField = "Descripcion"; ddlProducto.DataValueField = "ID_Producto";
                    ddlProducto.DataBind(); ddlProducto.Items.Insert(0, new ListItem("-- Seleccione --", ""));

                    string qT = "SELECT ID_Tratamiento, Nombre FROM tratamiento WHERE Estado = 1";
                    DataTable dtT = new DataTable(); new MySqlDataAdapter(qT, con).Fill(dtT);
                    ddlTratamiento.DataSource = dtT; ddlTratamiento.DataTextField = "Nombre"; ddlTratamiento.DataValueField = "ID_Tratamiento";
                    ddlTratamiento.DataBind(); ddlTratamiento.Items.Insert(0, new ListItem("Ninguno", ""));
                }
                catch { }
            }
        }

        protected void CargarGrid()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                string query = @"SELECT e.ID_Expediente, e.Fecha, e.Beneficiario, e.Estado, p.Descripcion as Producto,
                                IF(c.TipoCliente='Natural', cn.Nombre, cj.NombreEmpresa) as NombreCliente
                                FROM expediente e
                                INNER JOIN clientes c ON e.ID_Cliente = c.ID_Cliente
                                LEFT JOIN clientenatural cn ON c.ID_Cliente = cn.ID_Cliente
                                LEFT JOIN clientejuridico cj ON c.ID_Cliente = cj.ID_Cliente
                                INNER JOIN producto p ON e.ID_Producto = p.ID_Producto
                                WHERE 1=1";

                if (!string.IsNullOrEmpty(txtBuscar.Text)) query += " AND (cn.Nombre LIKE @Bus OR e.Beneficiario LIKE @Bus)";
                if (ddlFiltroEstado.SelectedValue != "-1") query += " AND e.Estado = @Est";
                if (!string.IsNullOrEmpty(txtFiltroDesde.Text)) query += " AND e.Fecha >= @Fini";
                if (!string.IsNullOrEmpty(txtFiltroHasta.Text)) query += " AND e.Fecha <= @Ffin";

                query += " ORDER BY e.ID_Expediente DESC";

                MySqlCommand cmd = new MySqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscar.Text)) cmd.Parameters.AddWithValue("@Bus", "%" + txtBuscar.Text + "%");
                if (ddlFiltroEstado.SelectedValue != "-1") cmd.Parameters.AddWithValue("@Est", ddlFiltroEstado.SelectedValue);
                if (!string.IsNullOrEmpty(txtFiltroDesde.Text)) cmd.Parameters.AddWithValue("@Fini", txtFiltroDesde.Text);
                if (!string.IsNullOrEmpty(txtFiltroHasta.Text)) cmd.Parameters.AddWithValue("@Ffin", txtFiltroHasta.Text);

                DataTable dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);

                if (ddlPageSize.SelectedValue != "All")
                {
                    gvExpedientes.AllowPaging = true;
                    gvExpedientes.PageSize = int.Parse(ddlPageSize.SelectedValue);
                }
                else gvExpedientes.AllowPaging = false;

                gvExpedientes.DataSource = dt;
                gvExpedientes.DataBind();
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e) { CargarGrid(); }
        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e) { CargarGrid(); }
        protected void gvExpedientes_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvExpedientes.PageIndex = e.NewPageIndex; CargarGrid(); }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            hfIDExpediente.Value = ""; LimpiarFormulario();
            lblTituloAccion.Text = "Nuevo Expediente";
            PanelListado.Visible = false; PanelMantenimiento.Visible = true;
        }

        protected void btnEditar_Click(object sender, EventArgs e)
        {
            string id = ((LinkButton)sender).CommandArgument;
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM expediente WHERE ID_Expediente = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfIDExpediente.Value = id;
                    ddlCliente.SelectedValue = r["ID_Cliente"].ToString();
                    txtBeneficiario.Text = r["Beneficiario"].ToString();
                    txtFecha.Text = Convert.ToDateTime(r["Fecha"]).ToString("yyyy-MM-dd");
                    txtOD.Text = r["OD"].ToString();
                    txtOD_AV.Text = r["OD_AV"] != DBNull.Value ? r["OD_AV"].ToString() : "";
                    txtOI.Text = r["OI"].ToString();
                    txtOI_AV.Text = r["OI_AV"] != DBNull.Value ? r["OI_AV"].ToString() : "";
                    txtADD.Text = r["AD_D"].ToString();
                    txtDP.Text = r["DP"].ToString();
                    txtALT.Text = r["ALT"].ToString();
                    ddlProducto.SelectedValue = r["ID_Producto"].ToString();
                    ddlTratamiento.SelectedValue = r["ID_Tratamiento"] != DBNull.Value ? r["ID_Tratamiento"].ToString() : "";
                    lblTituloAccion.Text = "Modificar Expediente";
                    PanelListado.Visible = false; PanelMantenimiento.Visible = true;
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validar campos obligatorios
            if (ddlCliente.SelectedValue == "")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "err", "Swal.fire('Error', 'Debe seleccionar un cliente.', 'error');", true);
                return;
            }
            if (ddlProducto.SelectedValue == "")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "err", "Swal.fire('Error', 'Debe seleccionar un producto.', 'error');", true);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = string.IsNullOrEmpty(hfIDExpediente.Value) ?
                        @"INSERT INTO expediente (ID_Cliente, Beneficiario, Fecha, OD, OD_AV, OI, OI_AV, AD_D, DP, ALT, ID_Producto, ID_Tratamiento, Estado) 
                          VALUES (@cli, @ben, @fec, @od, @odav, @oi, @oiav, @add, @dp, @alt, @prod, @trat, 1)" :
                        @"UPDATE expediente SET ID_Cliente=@cli, Beneficiario=@ben, Fecha=@fec, OD=@od, OD_AV=@odav, OI=@oi, OI_AV=@oiav, 
                          AD_D=@add, DP=@dp, ALT=@alt, ID_Producto=@prod, ID_Tratamiento=@trat WHERE ID_Expediente=@id";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@cli", ddlCliente.SelectedValue);
                    cmd.Parameters.AddWithValue("@ben", txtBeneficiario.Text.Trim());
                    cmd.Parameters.AddWithValue("@fec", txtFecha.Text);

                    // OD y OI son numéricos (con signo + o -)
                    cmd.Parameters.AddWithValue("@od", Normalizar(txtOD.Text));
                    cmd.Parameters.AddWithValue("@oi", Normalizar(txtOI.Text));

                    // OD_AV y OI_AV son TEXTO (ej: "20/20", "20/30") - NO se normalizan
                    cmd.Parameters.AddWithValue("@odav", txtOD_AV.Text.Trim());
                    cmd.Parameters.AddWithValue("@oiav", txtOI_AV.Text.Trim());

                    // ADD es numérico
                    cmd.Parameters.AddWithValue("@add", Normalizar(txtADD.Text));
                    cmd.Parameters.AddWithValue("@dp", txtDP.Text.Trim());
                    cmd.Parameters.AddWithValue("@alt", txtALT.Text.Trim());

                    cmd.Parameters.AddWithValue("@prod", ddlProducto.SelectedValue);
                    cmd.Parameters.AddWithValue("@trat", string.IsNullOrEmpty(ddlTratamiento.SelectedValue) ? (object)DBNull.Value : ddlTratamiento.SelectedValue);
                    if (!string.IsNullOrEmpty(hfIDExpediente.Value)) cmd.Parameters.AddWithValue("@id", hfIDExpediente.Value);

                    cmd.ExecuteNonQuery();
                    ScriptManager.RegisterStartupScript(this, GetType(), "swal", "Swal.fire('Éxito', 'Guardado correctamente', 'success');", true);
                    PanelMantenimiento.Visible = false; PanelListado.Visible = true; CargarGrid();
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "err", $"Swal.fire('Error', '{ex.Message.Replace("'", "\\'")}', 'error');", true);
                }
            }
        }

        private string Normalizar(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return "0.00";
            if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double res))
                return (res >= 0 ? "+" : "") + res.ToString("0.00", CultureInfo.InvariantCulture);
            return val;
        }

        protected void btnCancelar_Click(object sender, EventArgs e) { PanelMantenimiento.Visible = false; PanelListado.Visible = true; }

        private void LimpiarFormulario()
        {
            txtBeneficiario.Text = ""; txtOD.Text = ""; txtOI.Text = ""; txtOD_AV.Text = "";
            txtOI_AV.Text = ""; txtADD.Text = ""; txtDP.Text = ""; txtALT.Text = "";
            ddlCliente.SelectedIndex = 0; ddlProducto.SelectedIndex = 0; ddlTratamiento.SelectedIndex = 0;
            txtFecha.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}