using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Perfil : System.Web.UI.Page
    {
        private string urlPredeterminada = "https://avataaars.io/?avatarStyle=Circle&topType=NoHair&accessoriesType=Blank&facialHairType=Blank&clotheType=ShirtCrewNeck&clotheColor=Gray01&eyeType=Default&eyebrowType=Default&mouthType=Default&skinColor=Light";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarDatosPerfil();
            }
        }

        private void CargarDatosPerfil()
        {
            int idUsuario = Convert.ToInt32(Session["UsuarioID"]);
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "SELECT Nombres, Apellidos, Correo, Rol, Avatar FROM usuario WHERE ID_Usuario = @ID";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", idUsuario);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtNombres.Text = r["Nombres"].ToString();
                            txtApellidos.Text = r["Apellidos"].ToString();
                            txtCorreo.Text = r["Correo"].ToString();
                            lblRol.Text = r["Rol"].ToString();

                            string avatarObtenido = r["Avatar"] != DBNull.Value ? r["Avatar"].ToString() : "";
                            string avatarFinal = string.IsNullOrEmpty(avatarObtenido) || !avatarObtenido.StartsWith("http") ? urlPredeterminada : avatarObtenido;

                            hfAvatarSeleccionado.Value = avatarFinal;
                            imgAvatarActual.ImageUrl = avatarFinal;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarAlerta("Error al cargar perfil: " + ex.Message, "error");
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int idUsuario = Convert.ToInt32(Session["UsuarioID"]);
            string nombres = txtNombres.Text.Trim();
            string apellidos = txtApellidos.Text.Trim();
            string correo = txtCorreo.Text.Trim();
            string avatar = hfAvatarSeleccionado.Value;

            if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos) || string.IsNullOrEmpty(correo))
            {
                MostrarAlerta("Por favor, complete los campos obligatorios.", "warning");
                return;
            }

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    string sql = "UPDATE usuario SET Nombres=@Nom, Apellidos=@Ape, Correo=@Cor, Avatar=@Ava WHERE ID_Usuario=@ID";
                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Nom", nombres);
                    cmd.Parameters.AddWithValue("@Ape", apellidos);
                    cmd.Parameters.AddWithValue("@Cor", correo);
                    cmd.Parameters.AddWithValue("@Ava", avatar);
                    cmd.Parameters.AddWithValue("@ID", idUsuario);
                    cmd.ExecuteNonQuery();

                    MostrarAlerta("Perfil actualizado correctamente.", "success");
                    imgAvatarActual.ImageUrl = avatar;
                }
                catch (Exception ex)
                {
                    MostrarAlerta("Error al guardar: " + ex.Message, "error");
                }
            }
        }

        private void MostrarAlerta(string msg, string tipo)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "AlertaPerfil", $"Swal.fire('Perfil de Usuario', '{msg}', '{tipo}');", true);
        }
    }
}