using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Proveedor : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFiltroFechaInicio.Text = new DateTime(2023, 7, 1).ToString("yyyy-MM-dd");
                txtFiltroFechaFin.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CargarDatos();
            }
        }

        private void CargarDatos()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = "SELECT * FROM Proveedor WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                        query += " AND (Nombre LIKE @Busq OR Apellido LIKE @Busq OR RazonSocial LIKE @Busq OR Correo LIKE @Busq) ";

                    if (ddlFiltroEstado.SelectedValue != "-1")
                        query += " AND Estado = @Estado ";

                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                        query += " AND FechaRegistro BETWEEN @FecIni AND @FecFin ";

                    query += " ORDER BY ID_Proveedor DESC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", ConvertirFechaMySQL(txtFiltroFechaInicio.Text));
                    cmd.Parameters.AddWithValue("@FecFin", ConvertirFechaMySQL(txtFiltroFechaFin.Text));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvProveedores.DataSource = dt;
                    gvProveedores.DataBind();

                    pnlMensajeGrid.Visible = false;
                    if (ddlPageSize.SelectedValue != "All")
                    {
                        int requestedSize = int.Parse(ddlPageSize.SelectedValue);
                        if (dt.Rows.Count > 0 && dt.Rows.Count < requestedSize)
                        {
                            lblMensajeGrid.Text = $"Nota: Solicitó ver {requestedSize} registros, pero solo existen {dt.Rows.Count}.";
                            pnlMensajeGrid.Visible = true;
                        }
                    }
                    if (dt.Rows.Count == 0)
                    {
                        lblMensajeGrid.Text = "No hay registros disponibles.";
                        pnlMensajeGrid.Visible = true;
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        private bool ValidarEntradas()
        {
            bool v = true;
            LimpiarErrores();

            string rgxUnaPalabra = @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+$";

            if (!Regex.IsMatch(txtNombre.Text.Trim(), rgxUnaPalabra))
            {
                MostrarError(errNombre, "Solo una palabra (sin espacios).");
                MarcarError(txtNombre);
                v = false;
            }

            if (!Regex.IsMatch(txtApellido.Text.Trim(), rgxUnaPalabra))
            {
                MostrarError(errApellido, "Solo una palabra (sin espacios).");
                MarcarError(txtApellido);
                v = false;
            }

            string rs = txtRazonSocial.Text.Trim();
            if (string.IsNullOrWhiteSpace(rs))
            {
                MostrarError(errRazonSocial, "Requerido.");
                MarcarError(txtRazonSocial);
                v = false;
            }
            else if (!Regex.IsMatch(rs, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+)*$"))
            {
                MostrarError(errRazonSocial, "Solo letras y espacios.");
                MarcarError(txtRazonSocial);
                v = false;
            }
            else if (TieneCaracteresRepetidos(rs))
            {
                MostrarError(errRazonSocial, "Caracteres repetidos inválidos.");
                MarcarError(txtRazonSocial);
                v = false;
            }
            else if (ExisteDatoEnBD("RazonSocial", rs, hfIDProveedor.Value))
            {
                MostrarError(errRazonSocial, "Ya existe.");
                MarcarError(txtRazonSocial);
                v = false;
            }

            string correo = txtCorreo.Text.Trim();
            if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarError(errCorreo, "Inválido.");
                MarcarError(txtCorreo);
                v = false;
            }
            else if (ExisteDatoEnBD("Correo", correo, hfIDProveedor.Value))
            {
                MostrarError(errCorreo, "Ya registrado.");
                MarcarError(txtCorreo);
                v = false;
            }

            string tel = txtTelefono.Text.Trim();
            if (!Regex.IsMatch(tel, @"^[2578]\d{7}$"))
            {
                MostrarError(errTelefono, "8 dígitos válidos.");
                MarcarError(txtTelefono);
                v = false;
            }
            else if (ExisteDatoEnBD("Telefono", "505-" + tel, hfIDProveedor.Value))
            {
                MostrarError(errTelefono, "Ya registrado.");
                MarcarError(txtTelefono);
                v = false;
            }

            if (!string.IsNullOrEmpty(hfIDProveedor.Value))
            {
                if (DateTime.TryParse(txtFechaRegistro.Text, out DateTime fr))
                {
                    if (fr < new DateTime(2023, 6, 1) || fr > DateTime.Now) { MostrarError(errFecha, "Fecha inválida."); v = false; }
                }
                else { MostrarError(errFecha, "Requerida."); v = false; }
            }

            return v;
        }

        private bool TieneCaracteresRepetidos(string texto)
        {
            return Regex.IsMatch(texto, @"(.)\1\1");
        }

        private bool ExisteDatoEnBD(string campo, string valor, string idExcluir)
        {
            int count = 0;
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    string query = $"SELECT COUNT(*) FROM Proveedor WHERE {campo} = @Val AND ID_Proveedor != @ID";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Val", valor);
                    cmd.Parameters.AddWithValue("@ID", string.IsNullOrEmpty(idExcluir) ? "0" : idExcluir);
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return false; }
            return count > 0;
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            PanelListado.Visible = false;
            PanelMantenimiento.Visible = true;
            lblTituloAccion.Text = "Registrar Proveedor";
            txtFechaRegistro.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtFechaRegistro.Enabled = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            PanelListado.Visible = true;
            PanelMantenimiento.Visible = false;
            LimpiarErrores();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarEntradas()) return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd;
                    string telFull = "505-" + txtTelefono.Text.Trim();

                    if (string.IsNullOrEmpty(hfIDProveedor.Value))
                    {
                        string sql = @"INSERT INTO Proveedor (Nombre, Apellido, RazonSocial, Correo, Telefono, FechaRegistro, Estado) 
                                       VALUES (@Nom, @Ape, @Raz, @Cor, @Tel, CURRENT_DATE, 1)";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Nom", txtNombre.Text.Trim());
                        cmd.Parameters.AddWithValue("@Ape", txtApellido.Text.Trim());
                        cmd.Parameters.AddWithValue("@Raz", txtRazonSocial.Text.Trim());
                        cmd.Parameters.AddWithValue("@Cor", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tel", telFull);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Proveedor registrado correctamente.", "success");
                    }
                    else
                    {
                        string sql = @"UPDATE Proveedor SET Nombre=@Nom, Apellido=@Ape, RazonSocial=@Raz, Correo=@Cor, 
                                       Telefono=@Tel, FechaRegistro=@Fec WHERE ID_Proveedor=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@Nom", txtNombre.Text.Trim());
                        cmd.Parameters.AddWithValue("@Ape", txtApellido.Text.Trim());
                        cmd.Parameters.AddWithValue("@Raz", txtRazonSocial.Text.Trim());
                        cmd.Parameters.AddWithValue("@Cor", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tel", telFull);
                        cmd.Parameters.AddWithValue("@Fec", ConvertirFechaMySQL(txtFechaRegistro.Text));
                        cmd.Parameters.AddWithValue("@ID", hfIDProveedor.Value);
                        cmd.ExecuteNonQuery();
                        MostrarMensaje("Proveedor actualizado correctamente.", "success");
                    }
                    btnCancelar_Click(sender, e);
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error al guardar: " + ex.Message, "error"); }
        }

        protected void gvProveedores_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Editar") CargarParaEditar(id);
            else if (e.CommandName == "DarBaja") CambiarEstado(id, 0);
            else if (e.CommandName == "Reactivar") CambiarEstado(id, 1);
        }

        private void CargarParaEditar(int id)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Proveedor WHERE ID_Proveedor=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    MySqlDataReader r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        hfIDProveedor.Value = id.ToString();
                        txtNombre.Text = r["Nombre"].ToString();
                        txtApellido.Text = r["Apellido"].ToString();
                        txtRazonSocial.Text = r["RazonSocial"].ToString();
                        txtCorreo.Text = r["Correo"].ToString();
                        txtTelefono.Text = r["Telefono"].ToString().Replace("505-", "");
                        if (r["FechaRegistro"] != DBNull.Value)
                            txtFechaRegistro.Text = Convert.ToDateTime(r["FechaRegistro"]).ToString("yyyy-MM-dd");

                        txtFechaRegistro.Enabled = true;
                        PanelListado.Visible = false;
                        PanelMantenimiento.Visible = true;
                        lblTituloAccion.Text = "Editar Proveedor";
                        LimpiarErrores();
                    }
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        private void CambiarEstado(int id, int estado)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("UPDATE Proveedor SET Estado=@Est WHERE ID_Proveedor=@ID", con);
                    cmd.Parameters.AddWithValue("@Est", estado);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    string accion = estado == 1 ? "reactivado" : "inactivado";
                    MostrarMensaje($"Proveedor {accion} correctamente.", "success");
                    CargarDatos();
                }
            }
            catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, "error"); }
        }

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroEstado.SelectedIndex = 0;
            txtFiltroFechaInicio.Text = "";
            txtFiltroFechaFin.Text = "";
            ddlPageSize.SelectedValue = "All";
            gvProveedores.AllowPaging = false;
            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvProveedores.AllowPaging = false;
            else { gvProveedores.AllowPaging = true; gvProveedores.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void gvProveedores_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProveedores.PageIndex = e.NewPageIndex;
            CargarDatos();
        }

        protected void btnBuscar_Click(object sender, EventArgs e) => CargarDatos();

        private void LimpiarFormulario()
        {
            hfIDProveedor.Value = "";
            txtNombre.Text = ""; txtApellido.Text = ""; txtRazonSocial.Text = "";
            txtCorreo.Text = ""; txtTelefono.Text = ""; txtFechaRegistro.Text = "";
            LimpiarErrores();
        }

        private void LimpiarErrores()
        {
            if (errNombre != null) errNombre.Visible = false;
            if (errApellido != null) errApellido.Visible = false;
            if (errRazonSocial != null) errRazonSocial.Visible = false;
            if (errCorreo != null) errCorreo.Visible = false;
            if (errTelefono != null) errTelefono.Visible = false;
            if (errFecha != null) errFecha.Visible = false;
            QuitarMarcas();
        }

        private void QuitarMarcas()
        {
            txtNombre.CssClass = txtNombre.CssClass.Replace(" is-invalid", "");
            txtApellido.CssClass = txtApellido.CssClass.Replace(" is-invalid", "");
            txtRazonSocial.CssClass = txtRazonSocial.CssClass.Replace(" is-invalid", "");
            txtCorreo.CssClass = txtCorreo.CssClass.Replace(" is-invalid", "");
            txtTelefono.CssClass = txtTelefono.CssClass.Replace(" is-invalid", "");
        }

        private void MarcarError(TextBox txt)
        {
            txt.CssClass += " is-invalid";
        }

        private void MostrarError(Label l, string m) { if (l != null) { l.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {m}"; l.Visible = true; } }
        private string ConvertirFechaMySQL(string f) { if (DateTime.TryParse(f, out DateTime d)) return d.ToString("yyyy-MM-dd"); return f; }
        private void MostrarMensaje(string t, string i) { ScriptManager.RegisterStartupScript(this, GetType(), "Toast", $"Swal.fire('Proveedores', '{t}', '{i}');", true); }
    }
}