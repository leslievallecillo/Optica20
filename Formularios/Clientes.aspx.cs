using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Clientes : System.Web.UI.Page
    {
        private readonly HashSet<string> codigosMunicipios = new HashSet<string>
        {
            "001","002","003","004","005","006","007","008","009",
            "041","042","043","044","045","046","047","048",
            "081","082","083","084","085","086","087","088","089","090","091","092","093",
            "121","122","123","124",
            "161","162","163","164","165","166",
            "201","202","203","204","205","206","207","208","209",
            "241","242","243","244","245","246","247","248","249","250","251","252",
            "281","282","283","284","285","286","287","288","289","290",
            "321","322","323","324","325","326","327","328","329","330",
            "361","362","363","364","365","366","367","368",
            "401","402","403","404","405","406","407","408","409",
            "441","442","443","444","445","446","447","448","449","450","451","452","453","454",
            "481","482","483","484","485","486",
            "521","522","523","524","525","526",
            "561","562","563","564","565","566","567","568","569","570",
            "601","602","603","604","605","606","607","608","609","610","611","612","615","616","624"
        };

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
                    string query = @"SELECT c.ID_Cliente, 
                                    CASE 
                                        WHEN c.TipoCliente = 'Natural' THEN CONCAT(cn.Nombre, ' ', cn.Apellido)
                                        WHEN c.TipoCliente = 'Juridico' THEN CONCAT(cj.RepresentanteLegal, '<br><small style=""color:#666;"">', cj.NombreEmpresa, '</small>') 
                                        ELSE '---' 
                                    END AS NombreCompleto,
                                    c.TipoCliente,
                                    CASE WHEN c.TipoCliente = 'Natural' THEN cn.Cedula
                                         WHEN c.TipoCliente = 'Juridico' THEN cj.RUC ELSE '---' END AS Identificacion,
                                    c.Telefono, c.Correo, c.FechaRegistro, c.Estado
                                    FROM Clientes c
                                    LEFT JOIN ClienteNatural cn ON c.ID_Cliente = cn.ID_Cliente
                                    LEFT JOIN ClienteJuridico cj ON c.ID_Cliente = cj.ID_Cliente
                                    WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(txtBuscar.Text))
                    {
                        query += " AND (cn.Nombre LIKE @Busq OR cn.Apellido LIKE @Busq OR cj.NombreEmpresa LIKE @Busq OR cn.Cedula LIKE @Busq OR cj.RUC LIKE @Busq) ";
                    }
                    if (!string.IsNullOrEmpty(ddlFiltroTipo.SelectedValue))
                    {
                        query += " AND c.TipoCliente = @Tipo ";
                    }
                    if (ddlFiltroEstado.SelectedValue != "-1")
                    {
                        query += " AND c.Estado = @Estado ";
                    }
                    if (!string.IsNullOrEmpty(txtFiltroFechaInicio.Text) && !string.IsNullOrEmpty(txtFiltroFechaFin.Text))
                    {
                        query += " AND c.FechaRegistro BETWEEN @FecIni AND @FecFin ";
                    }

                    query += " ORDER BY c.ID_Cliente DESC";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Busq", "%" + txtBuscar.Text.Trim() + "%");
                    cmd.Parameters.AddWithValue("@Tipo", ddlFiltroTipo.SelectedValue);
                    cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    cmd.Parameters.AddWithValue("@FecIni", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaInicio.Text) ? "1900-01-01" : txtFiltroFechaInicio.Text).ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@FecFin", Convert.ToDateTime(string.IsNullOrEmpty(txtFiltroFechaFin.Text) ? "2100-01-01" : txtFiltroFechaFin.Text).ToString("yyyy-MM-dd"));

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvClientes.DataSource = dt;
                    gvClientes.DataBind();

                    pnlMensajeGrid.Visible = false;
                    if (ddlPageSize.SelectedValue != "All")
                    {
                        int requestedSize = int.Parse(ddlPageSize.SelectedValue);
                        if (dt.Rows.Count > 0 && dt.Rows.Count < requestedSize)
                        {
                            lblMensajeGrid.Text = $"Nota: Solicitó ver {requestedSize} registros, pero solo existen {dt.Rows.Count} registros disponibles con los filtros actuales.";
                            pnlMensajeGrid.Visible = true;
                        }
                    }
                    if (dt.Rows.Count == 0)
                    {
                        lblMensajeGrid.Text = "No hay registros disponibles con los filtros seleccionados.";
                        pnlMensajeGrid.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar: " + ex.Message, "error");
            }
        }

        private bool ValidarFormulario()
        {
            bool esValido = true;
            LimpiarErrores();

            string direccion = txtDireccion.Text.Trim();
            if (string.IsNullOrEmpty(direccion))
            {
                MostrarError(errDireccion, "La dirección es obligatoria.");
                MarcarError(txtDireccion);
                esValido = false;
            }
            else if (direccion.Length > 200)
            {
                MostrarError(errDireccion, "Excede el límite permitido.");
                MarcarError(txtDireccion);
                esValido = false;
            }
            else if (!Regex.IsMatch(direccion, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+( [a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+)*$"))
            {
                MostrarError(errDireccion, "Solo letras y un espacio entre palabras.");
                MarcarError(txtDireccion);
                esValido = false;
            }
            else if (TieneCaracteresRepetidos(direccion))
            {
                MostrarError(errDireccion, "Demasiados caracteres repetidos.");
                MarcarError(txtDireccion);
                esValido = false;
            }

            string telefonoRaw = txtTelefono.Text.Trim();
            if (!Regex.IsMatch(telefonoRaw, @"^[2578]\d{7}$"))
            {
                MostrarError(errTelefono, "8 dígitos válidos (2,5,7,8).");
                MarcarError(txtTelefono);
                esValido = false;
            }
            else if (ExisteDatoEnBD("Telefono", "505-" + telefonoRaw, hfIDCliente.Value))
            {
                MostrarError(errTelefono, "Número ya registrado.");
                MarcarError(txtTelefono);
                esValido = false;
            }

            string correo = txtCorreo.Text.Trim();
            if (string.IsNullOrEmpty(correo) || !Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarError(errCorreo, "Correo inválido.");
                MarcarError(txtCorreo);
                esValido = false;
            }
            else if (ExisteDatoEnBD("Correo", correo, hfIDCliente.Value))
            {
                MostrarError(errCorreo, "Correo ya registrado.");
                MarcarError(txtCorreo);
                esValido = false;
            }

            if (ddlTipoCliente.SelectedValue == "Natural")
            {
                if (!Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+$"))
                {
                    MostrarError(errNombre, "Solo letras, sin espacios.");
                    MarcarError(txtNombre);
                    esValido = false;
                }

                if (!Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ]+$"))
                {
                    MostrarError(errApellido, "Solo letras, sin espacios.");
                    MarcarError(txtApellido);
                    esValido = false;
                }

                if (!ValidarCedula(txtCedula.Text.Trim())) esValido = false;
            }
            else if (ddlTipoCliente.SelectedValue == "Juridico")
            {
                if (string.IsNullOrWhiteSpace(txtNombreEmpresa.Text))
                {
                    MostrarError(errEmpresa, "Requerido.");
                    MarcarError(txtNombreEmpresa);
                    esValido = false;
                }
                else if (TieneCaracteresRepetidos(txtNombreEmpresa.Text))
                {
                    MostrarError(errEmpresa, "Caracteres repetidos inválidos.");
                    MarcarError(txtNombreEmpresa);
                    esValido = false;
                }

                if (!ValidarRUC(txtRUC.Text.Trim())) esValido = false;

                // VALIDACION GIRO DE NEGOCIO
                string giro = txtGiro.Text.Trim();
                if (!ContarPalabras(giro, 10))
                {
                    MostrarError(errGiro, "Máximo 10 palabras.");
                    MarcarError(txtGiro);
                    esValido = false;
                }
                else if (!Regex.IsMatch(giro, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚüÜ0-9]+( [a-zA-ZñÑáéíóúÁÉÍÓÚüÜ0-9]+)*$"))
                {
                    MostrarError(errGiro, "Solo letras/números y un espacio.");
                    MarcarError(txtGiro);
                    esValido = false;
                }
                else if (TienePalabrasRepetidas(giro))
                {
                    MostrarError(errGiro, "Palabras repetidas excesivas.");
                    MarcarError(txtGiro);
                    esValido = false;
                }

                if (!Regex.IsMatch(txtRepresentante.Text, @"^[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]+$"))
                {
                    MostrarError(errRepresentante, "Solo letras.");
                    MarcarError(txtRepresentante);
                    esValido = false;
                }
            }
            else
            {
                MostrarMensaje("Debe seleccionar un tipo.", "warning");
                esValido = false;
            }

            return esValido;
        }

        private bool TieneCaracteresRepetidos(string texto)
        {
            return Regex.IsMatch(texto, @"(.)\1\1");
        }

        private bool TienePalabrasRepetidas(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return false;
            var palabras = texto.ToLower().Split(' ');
            var grupos = palabras.GroupBy(x => x);
            foreach (var g in grupos)
            {
                if (g.Count() > 2) return true;
            }
            return false;
        }

        private bool ValidarCedula(string cedula)
        {
            if (!Regex.IsMatch(cedula, @"^\d{3}-\d{6}-\d{4}[A-Z]$"))
            {
                MostrarError(errCedula, "Formato incorrecto.");
                MarcarError(txtCedula);
                return false;
            }

            string codigoMun = cedula.Substring(0, 3);
            if (!codigosMunicipios.Contains(codigoMun))
            {
                MostrarError(errCedula, "Municipio inválido.");
                MarcarError(txtCedula);
                return false;
            }

            string sDia = cedula.Substring(4, 2);
            string sMes = cedula.Substring(6, 2);
            string sAnio = cedula.Substring(8, 2);
            if (!EsFechaValida(sDia, sMes, sAnio))
            {
                MostrarError(errCedula, "Fecha inválida.");
                MarcarError(txtCedula);
                return false;
            }

            if (ExisteDatoEnBD("Cedula", cedula, hfIDCliente.Value))
            {
                MostrarError(errCedula, "Cédula ya existe.");
                MarcarError(txtCedula);
                return false;
            }

            return true;
        }

        private bool ValidarRUC(string ruc)
        {
            if (ruc.Length != 14)
            {
                MostrarError(errRUC, "Debe tener 14 caracteres.");
                MarcarError(txtRUC);
                return false;
            }

            if (!Regex.IsMatch(ruc, @"^\d{13}[A-Z]$"))
            {
                MostrarError(errRUC, "Formato inválido (13 num + letra).");
                MarcarError(txtRUC);
                return false;
            }

            string codigoMun = ruc.Substring(0, 3);
            if (!codigosMunicipios.Contains(codigoMun))
            {
                MostrarError(errRUC, "Municipio inválido.");
                MarcarError(txtRUC);
                return false;
            }

            string sDia = ruc.Substring(3, 2);
            string sMes = ruc.Substring(5, 2);
            string sAnio = ruc.Substring(7, 2);

            if (!EsFechaValida(sDia, sMes, sAnio))
            {
                MostrarError(errRUC, "Fecha inválida.");
                MarcarError(txtRUC);
                return false;
            }

            if (ExisteDatoEnBD("RUC", ruc, hfIDCliente.Value))
            {
                MostrarError(errRUC, "RUC ya existe.");
                MarcarError(txtRUC);
                return false;
            }

            return true;
        }

        private bool EsFechaValida(string dia, string mes, string anio)
        {
            try
            {
                int d = int.Parse(dia);
                int m = int.Parse(mes);
                int a = int.Parse(anio);
                int fullYear = (a > int.Parse(DateTime.Now.ToString("yy"))) ? 1900 + a : 2000 + a;
                DateTime dt = new DateTime(fullYear, m, d);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ExisteDatoEnBD(string campo, string valor, string idExcluir)
        {
            string query = "";
            if (campo == "Telefono" || campo == "Correo")
                query = $"SELECT COUNT(*) FROM Clientes WHERE {campo} = @Val AND ID_Cliente != @ID";
            else if (campo == "Cedula")
                query = $"SELECT COUNT(*) FROM ClienteNatural WHERE Cedula = @Val AND ID_Cliente != @ID";
            else if (campo == "RUC")
                query = $"SELECT COUNT(*) FROM ClienteJuridico WHERE RUC = @Val AND ID_Cliente != @ID";

            int count = 0;
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Val", valor);
                cmd.Parameters.AddWithValue("@ID", string.IsNullOrEmpty(idExcluir) ? "0" : idExcluir);
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return count > 0;
        }

        private bool ContarPalabras(string texto, int max)
        {
            if (string.IsNullOrWhiteSpace(texto)) return true;
            return texto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length <= max;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd;
                    string telefonoFull = "505-" + txtTelefono.Text.Trim();

                    if (string.IsNullOrEmpty(hfIDCliente.Value))
                    {
                        string sqlMaestro = "INSERT INTO Clientes (Direccion, Telefono, Correo, TipoCliente, FechaRegistro, Estado) VALUES (@Dir, @Tel, @Cor, @Tipo, NOW(), 1); SELECT LAST_INSERT_ID();";
                        cmd = new MySqlCommand(sqlMaestro, con);
                        cmd.Parameters.AddWithValue("@Dir", txtDireccion.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tel", telefonoFull);
                        cmd.Parameters.AddWithValue("@Cor", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tipo", ddlTipoCliente.SelectedValue);
                        int idGenerado = Convert.ToInt32(cmd.ExecuteScalar());

                        if (ddlTipoCliente.SelectedValue == "Natural")
                        {
                            string sqlNat = "INSERT INTO ClienteNatural (ID_Cliente, Nombre, Apellido, Cedula) VALUES (@ID, @Nom, @Ape, @Ced)";
                            cmd = new MySqlCommand(sqlNat, con);
                            cmd.Parameters.AddWithValue("@ID", idGenerado);
                            cmd.Parameters.AddWithValue("@Nom", txtNombre.Text.Trim());
                            cmd.Parameters.AddWithValue("@Ape", txtApellido.Text.Trim());
                            cmd.Parameters.AddWithValue("@Ced", txtCedula.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            string sqlJur = "INSERT INTO ClienteJuridico (ID_Cliente, NombreEmpresa, RUC, GiroNegocio, RepresentanteLegal, EsAfiliada, DescuentoCorporativo) VALUES (@ID, @Emp, @RUC, @Giro, @Rep, @Afi, @Desc)";
                            cmd = new MySqlCommand(sqlJur, con);
                            cmd.Parameters.AddWithValue("@ID", idGenerado);
                            cmd.Parameters.AddWithValue("@Emp", txtNombreEmpresa.Text.Trim());
                            cmd.Parameters.AddWithValue("@RUC", txtRUC.Text.Trim());
                            cmd.Parameters.AddWithValue("@Giro", txtGiro.Text.Trim());
                            cmd.Parameters.AddWithValue("@Rep", txtRepresentante.Text.Trim());
                            cmd.Parameters.AddWithValue("@Afi", chkEsAfiliada.Checked);
                            cmd.Parameters.AddWithValue("@Desc", string.IsNullOrEmpty(txtDescuento.Text) ? 0 : decimal.Parse(txtDescuento.Text));
                            cmd.ExecuteNonQuery();
                        }
                        MostrarMensaje("Cliente registrado correctamente.", "success");
                    }
                    else
                    {
                        string sqlMaestro = "UPDATE Clientes SET Direccion=@Dir, Telefono=@Tel, Correo=@Cor WHERE ID_Cliente=@ID";
                        cmd = new MySqlCommand(sqlMaestro, con);
                        cmd.Parameters.AddWithValue("@Dir", txtDireccion.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tel", telefonoFull);
                        cmd.Parameters.AddWithValue("@Cor", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID", hfIDCliente.Value);
                        cmd.ExecuteNonQuery();

                        if (ddlTipoCliente.SelectedValue == "Natural")
                        {
                            string sqlNat = "UPDATE ClienteNatural SET Nombre=@Nom, Apellido=@Ape, Cedula=@Ced WHERE ID_Cliente=@ID";
                            cmd = new MySqlCommand(sqlNat, con);
                            cmd.Parameters.AddWithValue("@Nom", txtNombre.Text.Trim());
                            cmd.Parameters.AddWithValue("@Ape", txtApellido.Text.Trim());
                            cmd.Parameters.AddWithValue("@Ced", txtCedula.Text.Trim());
                            cmd.Parameters.AddWithValue("@ID", hfIDCliente.Value);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            string sqlJur = "UPDATE ClienteJuridico SET NombreEmpresa=@Emp, RUC=@RUC, GiroNegocio=@Giro, RepresentanteLegal=@Rep, EsAfiliada=@Afi, DescuentoCorporativo=@Desc WHERE ID_Cliente=@ID";
                            cmd = new MySqlCommand(sqlJur, con);
                            cmd.Parameters.AddWithValue("@Emp", txtNombreEmpresa.Text.Trim());
                            cmd.Parameters.AddWithValue("@RUC", txtRUC.Text.Trim());
                            cmd.Parameters.AddWithValue("@Giro", txtGiro.Text.Trim());
                            cmd.Parameters.AddWithValue("@Rep", txtRepresentante.Text.Trim());
                            cmd.Parameters.AddWithValue("@Afi", chkEsAfiliada.Checked);
                            cmd.Parameters.AddWithValue("@Desc", string.IsNullOrEmpty(txtDescuento.Text) ? 0 : decimal.Parse(txtDescuento.Text));
                            cmd.Parameters.AddWithValue("@ID", hfIDCliente.Value);
                            cmd.ExecuteNonQuery();
                        }
                        MostrarMensaje("Cliente actualizado correctamente.", "success");
                    }

                    CargarDatos();
                    PanelListado.Visible = true;
                    PanelMantenimiento.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error en base de datos: " + ex.Message, "error");
            }
        }

        protected void gvClientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Editar")
            {
                CargarParaEditar(id);
            }
            else if (e.CommandName == "DarBaja")
            {
                CambiarEstado(id, 0);
            }
            else if (e.CommandName == "Reactivar")
            {
                CambiarEstado(id, 1);
            }
        }

        private void CargarParaEditar(int id)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                string sql = @"SELECT c.*, cn.Nombre, cn.Apellido, cn.Cedula, 
                             cj.NombreEmpresa, cj.RUC, cj.GiroNegocio, cj.RepresentanteLegal, cj.EsAfiliada, cj.DescuentoCorporativo
                             FROM Clientes c 
                             LEFT JOIN ClienteNatural cn ON c.ID_Cliente=cn.ID_Cliente
                             LEFT JOIN ClienteJuridico cj ON c.ID_Cliente=cj.ID_Cliente
                             WHERE c.ID_Cliente=@ID";
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ID", id);
                MySqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    hfIDCliente.Value = id.ToString();
                    ddlTipoCliente.SelectedValue = r["TipoCliente"].ToString();
                    ddlTipoCliente.Enabled = false;
                    ddlTipoCliente_SelectedIndexChanged(null, null);

                    txtDireccion.Text = r["Direccion"].ToString();
                    txtTelefono.Text = r["Telefono"].ToString().Replace("505-", "");
                    txtCorreo.Text = r["Correo"].ToString();
                    txtFechaRegistro.Text = Convert.ToDateTime(r["FechaRegistro"]).ToString("yyyy-MM-dd");

                    if (ddlTipoCliente.SelectedValue == "Natural")
                    {
                        txtNombre.Text = r["Nombre"].ToString();
                        txtApellido.Text = r["Apellido"].ToString();
                        txtCedula.Text = r["Cedula"].ToString();
                    }
                    else
                    {
                        txtNombreEmpresa.Text = r["NombreEmpresa"].ToString();
                        txtRUC.Text = r["RUC"].ToString();
                        txtGiro.Text = r["GiroNegocio"].ToString();
                        txtRepresentante.Text = r["RepresentanteLegal"].ToString();
                        chkEsAfiliada.Checked = Convert.ToBoolean(r["EsAfiliada"]);
                        txtDescuento.Text = r["DescuentoCorporativo"].ToString();
                    }

                    PanelListado.Visible = false;
                    PanelMantenimiento.Visible = true;
                    lblTituloAccion.Text = "Editar Cliente";
                    LimpiarErrores();
                }
            }
        }

        private void CambiarEstado(int id, int estado)
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE Clientes SET Estado=@Est WHERE ID_Cliente=@ID", con);
                cmd.Parameters.AddWithValue("@Est", estado);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
            CargarDatos();
            string accion = estado == 1 ? "reactivado" : "dado de baja";
            MostrarMensaje($"Cliente {accion} correctamente.", "success");
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            PanelListado.Visible = false;
            PanelMantenimiento.Visible = true;
            hfIDCliente.Value = "";
            LimpiarFormulario();
            lblTituloAccion.Text = "Registrar Nuevo Cliente";
            ddlTipoCliente.Enabled = true;
            txtFechaRegistro.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            PanelListado.Visible = true;
            PanelMantenimiento.Visible = false;
            LimpiarErrores();
        }

        protected void btnBuscar_Click(object sender, EventArgs e) => CargarDatos();

        protected void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = "";
            ddlFiltroTipo.SelectedIndex = 0;
            ddlFiltroEstado.SelectedIndex = 0;
            txtFiltroFechaInicio.Text = "";
            txtFiltroFechaFin.Text = "";
            ddlPageSize.SelectedValue = "All";
            gvClientes.AllowPaging = false;
            CargarDatos();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPageSize.SelectedValue == "All") gvClientes.AllowPaging = false;
            else { gvClientes.AllowPaging = true; gvClientes.PageSize = int.Parse(ddlPageSize.SelectedValue); }
            CargarDatos();
        }

        protected void gvClientes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvClientes.PageIndex = e.NewPageIndex;
            CargarDatos();
        }

        protected void ddlTipoCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlNatural.Visible = ddlTipoCliente.SelectedValue == "Natural";
            pnlJuridico.Visible = ddlTipoCliente.SelectedValue == "Juridico";
        }

        private void LimpiarFormulario()
        {
            txtNombre.Text = ""; txtApellido.Text = ""; txtCedula.Text = "";
            txtNombreEmpresa.Text = ""; txtRUC.Text = ""; txtGiro.Text = ""; txtRepresentante.Text = ""; txtDescuento.Text = "";
            txtDireccion.Text = ""; txtTelefono.Text = ""; txtCorreo.Text = "";
            ddlTipoCliente.SelectedIndex = 0;
            pnlNatural.Visible = false; pnlJuridico.Visible = false;
            LimpiarErrores();
            QuitarMarcas();
        }

        private void MostrarMensaje(string texto, string icono)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Toast", $"Swal.fire('Sistema', '{texto}', '{icono}');", true);
        }

        private void MostrarError(Label lbl, string msg)
        {
            lbl.Text = $"<i class='fa-solid fa-circle-exclamation'></i> {msg}";
            lbl.Visible = true;
        }

        private void MarcarError(TextBox txt)
        {
            txt.CssClass += " is-invalid";
        }

        private void LimpiarErrores()
        {
            errNombre.Visible = false; errApellido.Visible = false; errCedula.Visible = false;
            errEmpresa.Visible = false; errRUC.Visible = false; errGiro.Visible = false; errRepresentante.Visible = false;
            errDireccion.Visible = false; errTelefono.Visible = false; errCorreo.Visible = false;
            QuitarMarcas();
        }

        private void QuitarMarcas()
        {
            txtNombre.CssClass = txtNombre.CssClass.Replace(" is-invalid", "");
            txtApellido.CssClass = txtApellido.CssClass.Replace(" is-invalid", "");
            txtCedula.CssClass = txtCedula.CssClass.Replace(" is-invalid", "");
            txtDireccion.CssClass = txtDireccion.CssClass.Replace(" is-invalid", "");
            txtTelefono.CssClass = txtTelefono.CssClass.Replace(" is-invalid", "");
            txtCorreo.CssClass = txtCorreo.CssClass.Replace(" is-invalid", "");
            txtNombreEmpresa.CssClass = txtNombreEmpresa.CssClass.Replace(" is-invalid", "");
            txtRUC.CssClass = txtRUC.CssClass.Replace(" is-invalid", "");
            txtGiro.CssClass = txtGiro.CssClass.Replace(" is-invalid", "");
        }
    }
}