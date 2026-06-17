using System;
using System.Data;
using System.Web.UI;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class CatalogoCliente : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCatalogos();
            }
        }

        private void CargarCatalogos()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();

                    string sqlDestacados = "SELECT Titulo, Descripcion, Precio, UrlImagen FROM CatalogoImagenes WHERE Seccion = 'Destacados' ORDER BY ID_Imagen DESC";
                    MySqlDataAdapter daDestacados = new MySqlDataAdapter(sqlDestacados, con);
                    DataTable dtDestacados = new DataTable();
                    daDestacados.Fill(dtDestacados);
                    rptDestacados.DataSource = dtDestacados;
                    rptDestacados.DataBind();

                    string sqlPromociones = "SELECT Titulo, Descripcion, Precio, UrlImagen FROM CatalogoImagenes WHERE Seccion = 'Promociones' ORDER BY ID_Imagen DESC";
                    MySqlDataAdapter daPromociones = new MySqlDataAdapter(sqlPromociones, con);
                    DataTable dtPromociones = new DataTable();
                    daPromociones.Fill(dtPromociones);
                    rptPromociones.DataSource = dtPromociones;
                    rptPromociones.DataBind();

                    string sqlOtros = "SELECT Titulo, Descripcion, Precio, UrlImagen FROM CatalogoImagenes WHERE Seccion = 'Otros' ORDER BY ID_Imagen DESC";
                    MySqlDataAdapter daOtros = new MySqlDataAdapter(sqlOtros, con);
                    DataTable dtOtros = new DataTable();
                    daOtros.Fill(dtOtros);
                    rptOtros.DataSource = dtOtros;
                    rptOtros.DataBind();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al cargar el catálogo: " + ex.Message);
                }
            }
        }
    }
}