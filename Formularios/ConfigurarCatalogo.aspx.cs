using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class ConfigurarCatalogo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarGrid();
            }
        }

        private void CargarGrid()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "SELECT * FROM catalogoimagenes ORDER BY Seccion, ID_Imagen DESC";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvCatalogo.DataSource = dt;
                    gvCatalogo.DataBind();
                }
                catch (Exception ex)
                {
                    MostrarAlerta("Error al cargar datos: " + ex.Message, "error");
                }
            }
        }

        private bool ValidarTexto(string texto, out string mensajeError)
        {
            mensajeError = "";
            if (string.IsNullOrEmpty(texto)) return true;

            if (Regex.IsMatch(texto, @"\s{2,}"))
            {
                mensajeError = "No se permite más de un espacio seguido por palabra.";
                return false;
            }
            if (Regex.IsMatch(texto, @"([a-zA-ZáéíóúÁÉÍÓÚñÑ])\1{2,}"))
            {
                mensajeError = "No se permite repetir la misma letra 3 o más veces seguidas.";
                return false;
            }
            if (!Regex.IsMatch(texto, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s!¡?¿]+$"))
            {
                mensajeError = "No se permiten símbolos especiales, a excepción de ! ¡ ? ¿";
                return false;
            }
            return true;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            string seccion = ddlSeccion.SelectedValue;
            string tipoEtiqueta = ddlTipoEtiqueta.SelectedValue;
            string titulo = txtTitulo.Text.Trim();
            string descripcion = txtDescripcion.Text.Trim();
            string precioRaw = txtPrecio.Text.Trim();
            string urlImagen = txtUrlImagen.Text.Trim();

            if (string.IsNullOrEmpty(titulo) || string.IsNullOrEmpty(urlImagen) || string.IsNullOrEmpty(precioRaw))
            {
                MostrarAlerta("El Título, el Precio/Etiqueta y la URL de la imagen son obligatorios.", "warning");
                return;
            }

            string urlLower = urlImagen.ToLower();
            if (!urlLower.EndsWith(".png") && !urlLower.EndsWith(".jpeg") && !urlLower.EndsWith(".jpg"))
            {
                MostrarAlerta("La URL de la imagen debe terminar en .png, .jpeg o .jpg", "error");
                return;
            }

            string errorMsj;
            if (!ValidarTexto(titulo, out errorMsj))
            {
                MostrarAlerta("Título inválido: " + errorMsj, "error");
                return;
            }

            if (!ValidarTexto(descripcion, out errorMsj))
            {
                MostrarAlerta("Descripción inválida: " + errorMsj, "error");
                return;
            }

            string precioFinal = "";

            if (tipoEtiqueta == "Precio")
            {
                if (!decimal.TryParse(precioRaw, out decimal valorPrecio))
                {
                    MostrarAlerta("El precio debe ser un número válido.", "error");
                    return;
                }

                if (valorPrecio > 4000)
                {
                    MostrarAlerta("El precio no puede exceder de 4000 C$.", "error");
                    return;
                }

                precioFinal = valorPrecio.ToString("0.##") + " C$";
            }
            else
            {
                if (!ValidarTexto(precioRaw, out errorMsj))
                {
                    MostrarAlerta("Etiqueta inválida: " + errorMsj, "error");
                    return;
                }
                precioFinal = precioRaw;
            }

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    MySqlCommand cmd;

                    if (string.IsNullOrEmpty(hfIDImagen.Value))
                    {
                        string sql = "INSERT INTO catalogoimagenes (Seccion, Titulo, Descripcion, Precio, UrlImagen) VALUES (@Sec, @Tit, @Desc, @Pre, @Url)";
                        cmd = new MySqlCommand(sql, con);
                    }
                    else
                    {
                        string sql = "UPDATE catalogoimagenes SET Seccion=@Sec, Titulo=@Tit, Descripcion=@Desc, Precio=@Pre, UrlImagen=@Url WHERE ID_Imagen=@ID";
                        cmd = new MySqlCommand(sql, con);
                        cmd.Parameters.AddWithValue("@ID", hfIDImagen.Value);
                    }

                    cmd.Parameters.AddWithValue("@Sec", seccion);
                    cmd.Parameters.AddWithValue("@Tit", titulo);
                    cmd.Parameters.AddWithValue("@Desc", descripcion);
                    cmd.Parameters.AddWithValue("@Pre", precioFinal);
                    cmd.Parameters.AddWithValue("@Url", urlImagen);

                    cmd.ExecuteNonQuery();
                    MostrarAlerta("Elemento guardado correctamente.", "success");
                    btnLimpiar_Click(null, null);
                    CargarGrid();
                }
                catch (Exception ex)
                {
                    MostrarAlerta("Error al guardar: " + ex.Message, "error");
                }
            }
        }

        protected void gvCatalogo_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Editar")
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM catalogoimagenes WHERE ID_Imagen=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            hfIDImagen.Value = r["ID_Imagen"].ToString();
                            ddlSeccion.SelectedValue = r["Seccion"].ToString();
                            txtTitulo.Text = r["Titulo"].ToString();
                            txtDescripcion.Text = r["Descripcion"].ToString();

                            string precioBd = r["Precio"].ToString();
                            if (precioBd.EndsWith(" C$"))
                            {
                                ddlTipoEtiqueta.SelectedValue = "Precio";
                                txtPrecio.Text = precioBd.Replace(" C$", "").Trim();
                            }
                            else
                            {
                                ddlTipoEtiqueta.SelectedValue = "Etiqueta";
                                txtPrecio.Text = precioBd;
                            }

                            txtUrlImagen.Text = r["UrlImagen"].ToString();
                        }
                    }
                }
            }
            else if (e.CommandName == "Eliminar")
            {
                using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM catalogoimagenes WHERE ID_Imagen=@ID", con);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    MostrarAlerta("Elemento eliminado.", "success");
                    CargarGrid();
                }
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            hfIDImagen.Value = "";
            ddlSeccion.SelectedIndex = 0;
            ddlTipoEtiqueta.SelectedIndex = 0;
            txtTitulo.Text = "";
            txtDescripcion.Text = "";
            txtPrecio.Text = "";
            txtUrlImagen.Text = "";
        }

        private void MostrarAlerta(string msg, string tipo)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "AlertaCatalogo", $"Swal.fire('Gestor de Catálogo', '{msg}', '{tipo}');", true);
        }
    }
}