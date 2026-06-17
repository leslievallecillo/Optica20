using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Web.UI;
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class Empresa : System.Web.UI.Page
    {
        private const int EMPRESA_DEFAULT_ID = 1;
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
                CargarDatosEmpresa();
            }
        }

        private void CargarDatosEmpresa()
        {
            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    string query = "SELECT * FROM Empresa WHERE ID_Empresa = @ID LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ID", EMPRESA_DEFAULT_ID);

                    con.Open();
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            txtNombre.Text = "Optica 20/20";
                            txtRUC.Text = dr["RUC"].ToString();
                            txtCorreo.Text = dr["Correo"].ToString();

                            string telefonoFull = dr["Telefono"].ToString();
                            if (telefonoFull.StartsWith("505") && telefonoFull.Length > 3)
                                txtTelefono.Text = telefonoFull.Substring(3);
                            else
                                txtTelefono.Text = telefonoFull;

                            lblNombreCard.Text = "Optica 20/20";
                            lblCorreoCard.Text = dr["Correo"].ToString();

                            string logoUrl = dr["LogoRuta"] != DBNull.Value ? dr["LogoRuta"].ToString() : "";
                            if (!string.IsNullOrEmpty(logoUrl))
                            {
                                imgLogoCard.ImageUrl = logoUrl;
                                imgLogoPreview.ImageUrl = logoUrl;
                            }
                        }
                        else
                        {
                            txtNombre.Text = "Optica 20/20";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar datos: " + ex.Message, "error");
                }
            }
        }

        private bool ValidarFormulario()
        {
            bool esValido = true;
            LimpiarErrores();

            if (!ValidarRUC(txtRUC.Text.Trim())) esValido = false;

            string tel = txtTelefono.Text.Trim();
            if (!Regex.IsMatch(tel, @"^[2578]\d{7}$"))
            {
                MostrarError(errTelefono, "Debe tener 8 dígitos (Inicia 2, 5, 7 u 8).");
                esValido = false;
            }

            string correo = txtCorreo.Text.Trim();
            if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarError(errCorreo, "Formato inválido (ejemplo@dominio.com).");
                esValido = false;
            }

            return esValido;
        }

        private bool ValidarRUC(string ruc)
        {
            if (ruc.Length != 14)
            {
                MostrarError(errRUC, "Debe tener exactamente 14 caracteres.");
                return false;
            }

            if (!Regex.IsMatch(ruc, @"^\d{13}[A-Z]$"))
            {
                MostrarError(errRUC, "Formato incorrecto (13 números seguidos de una letra).");
                return false;
            }

            string codigoMun = ruc.Substring(0, 3);
            if (!codigosMunicipios.Contains(codigoMun))
            {
                MostrarError(errRUC, "El RUC debe iniciar con un código de municipio válido.");
                return false;
            }

            string sDia = ruc.Substring(3, 2);
            string sMes = ruc.Substring(5, 2);
            string sAnio = ruc.Substring(7, 2);

            if (!int.TryParse(sDia, out _) || !int.TryParse(sMes, out _) || !int.TryParse(sAnio, out _))
            {
                MostrarError(errRUC, "Formato de fecha en RUC inválido.");
                return false;
            }

            if (!EsFechaValida(sDia, sMes, sAnio))
            {
                MostrarError(errRUC, "La fecha contenida en el RUC es inválida.");
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
            catch { return false; }
        }

        private void LimpiarErrores()
        {
            errRUC.Visible = false;
            errTelefono.Visible = false;
            errCorreo.Visible = false;
        }

        private void MostrarError(System.Web.UI.WebControls.Label lbl, string msg)
        {
            lbl.Text = $"<i class='fas fa-exclamation-circle'></i> {msg}";
            lbl.Visible = true;
        }

        private void MostrarMensaje(string texto, string icono)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "Toast", $"Swal.fire('Empresa', '{texto}', '{icono}');", true);
        }

        private string SubirImagenAHosting(byte[] archivoBytes)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Authorization", "Client-ID 546c25a59c58ad7");
                    System.Collections.Specialized.NameValueCollection req = new System.Collections.Specialized.NameValueCollection();
                    req.Add("image", Convert.ToBase64String(archivoBytes));
                    byte[] response = wc.UploadValues("https://api.imgur.com/3/image", "POST", req);
                    string json = Encoding.UTF8.GetString(response);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic data = js.Deserialize<dynamic>(json);
                    return data["data"]["link"];
                }
            }
            catch { return null; }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            string nombreFijo = "Optica 20/20";
            string telefonoFinal = "505" + txtTelefono.Text.Trim();
            string nuevaUrlLogo = null;

            if (fuLogo.HasFile)
            {
                string ext = Path.GetExtension(fuLogo.FileName).ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                {
                    if (fuLogo.PostedFile.ContentLength <= 5 * 1024 * 1024)
                    {
                        nuevaUrlLogo = SubirImagenAHosting(fuLogo.FileBytes);

                        if (string.IsNullOrEmpty(nuevaUrlLogo))
                        {
                            MostrarMensaje("Ocurrió un error al subir la imagen al servidor remoto.", "error");
                            return;
                        }
                    }
                    else
                    {
                        MostrarMensaje("La imagen no debe superar los 5MB.", "warning");
                        return;
                    }
                }
                else
                {
                    MostrarMensaje("Formato de imagen inválido. Solo se admiten .png, .jpg y .jpeg", "warning");
                    return;
                }
            }

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Empresa WHERE ID_Empresa = @ID";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@ID", EMPRESA_DEFAULT_ID);
                    int existe = Convert.ToInt32(checkCmd.ExecuteScalar());

                    string query;
                    if (existe > 0)
                    {
                        query = @"UPDATE Empresa SET 
                                  Nombre = @Nombre, 
                                  RUC = @RUC, 
                                  Telefono = @Telefono, 
                                  Correo = @Correo";
                        if (!string.IsNullOrEmpty(nuevaUrlLogo))
                        {
                            query += ", LogoRuta = @LogoRuta";
                        }
                        query += " WHERE ID_Empresa = @ID";
                    }
                    else
                    {
                        query = @"INSERT INTO Empresa (ID_Empresa, Nombre, RUC, Telefono, Correo, Estado, FechaRegistro, LogoRuta) 
                                  VALUES (@ID, @Nombre, @RUC, @Telefono, @Correo, 1, CURRENT_DATE, @LogoRuta)";
                    }

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ID", EMPRESA_DEFAULT_ID);
                    cmd.Parameters.AddWithValue("@Nombre", nombreFijo);
                    cmd.Parameters.AddWithValue("@RUC", txtRUC.Text.Trim());
                    cmd.Parameters.AddWithValue("@Telefono", telefonoFinal);
                    cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text.Trim());

                    if (!string.IsNullOrEmpty(nuevaUrlLogo) || existe == 0)
                    {
                        cmd.Parameters.AddWithValue("@LogoRuta", nuevaUrlLogo ?? "https://i.postimg.cc/3R3WcxSb/565834681-122104982919051438-2227136986678388589-n.jpg");
                    }

                    cmd.ExecuteNonQuery();

                    CargarDatosEmpresa();
                    MostrarMensaje("Información y logo actualizados correctamente en la nube.", "success");
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al guardar: " + ex.Message, "error");
                }
            }
        }
    }
}