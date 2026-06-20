using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text; // Necesario para StringBuilder
using System.Security.Cryptography; // Necesario para SHA256
using MySql.Data.MySqlClient;
using Optica.Clases;

namespace Optica.Formularios
{
    public partial class RecuperarContra : System.Web.UI.Page
    {
        // =================================================================
        // CONFIGURACIÓN DE TU CORREO (Zelaya)
        // =================================================================
        string correoEmisor = "zelayaomeany6@gmail.com";

        // ¡OJO! AQUÍ PEGAS LA CLAVE DE 16 LETRAS QUE TE DA GOOGLE
        string claveAplicacion = "gptmcunrlsoafdka";
        // =================================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
        }

        // --- PASO 1: ENVIAR CÓDIGO ---
        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            string input = txtUsuario.Text.Trim();
            if (string.IsNullOrEmpty(input)) { MostrarMensaje("Ingresa un dato válido.", true); return; }

            int idUsuario = 0;
            string correoDestino = "";
            string nombreUser = "";

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    // Buscar usuario por Login o por Email
                    string sql = @"SELECT u.ID_Usuario, u.Correo, u.Nombres 
                                   FROM usuario u
                                   JOIN iniciosesion i ON u.ID_Usuario = i.ID_Usuario
                                   WHERE i.NombreUsuario = @Dato OR u.Correo = @Dato";

                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Dato", input);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                idUsuario = Convert.ToInt32(r["ID_Usuario"]);
                                correoDestino = r["Correo"].ToString();
                                nombreUser = r["Nombres"].ToString();
                            }
                        }
                    }

                    if (idUsuario > 0 && !string.IsNullOrEmpty(correoDestino))
                    {
                        // Generar código
                        string codigo = new Random().Next(100000, 999999).ToString();

                        // Guardar en BD
                        string sqlToken = "INSERT INTO passwordresettokens (ID_Usuario, Token, ExpiryDate, Used) VALUES (@uid, @tok, DATE_ADD(NOW(), INTERVAL 15 MINUTE), 0)";
                        using (MySqlCommand cmd = new MySqlCommand(sqlToken, con))
                        {
                            cmd.Parameters.AddWithValue("@uid", idUsuario);
                            cmd.Parameters.AddWithValue("@tok", codigo);
                            cmd.ExecuteNonQuery();
                        }

                        // Enviar correo
                        if (EnviarEmailGmail(correoDestino, nombreUser, codigo))
                        {
                            Session["Reset_UID"] = idUsuario; // Guardar ID temporal
                            pnlSolicitar.Visible = false;
                            pnlCambiar.Visible = true;
                            MostrarMensaje("Código enviado a: " + OcultarCorreo(correoDestino), false);
                        }
                        else
                        {
                            MostrarMensaje("Error enviando el correo. Verifica la clave de aplicación.", true);
                        }
                    }
                    else
                    {
                        MostrarMensaje("Usuario no encontrado.", true);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error: " + ex.Message, true);
                }
            }
        }

        // --- PASO 2: CAMBIAR CONTRASEÑA ---
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (Session["Reset_UID"] == null) Response.Redirect("RecuperarContra.aspx");

            string codigo = txtCodigo.Text.Trim();
            string nuevaClave = txtNuevaClave.Text;
            int uid = Convert.ToInt32(Session["Reset_UID"]);

            if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(nuevaClave))
            {
                MostrarMensaje("Completa todos los campos.", true);
                return;
            }

            using (MySqlConnection con = new MySqlConnection(Conexion.CadenaConexion))
            {
                try
                {
                    con.Open();
                    // Verificar Token
                    string sqlCheck = "SELECT COUNT(*) FROM passwordresettokens WHERE ID_Usuario=@uid AND Token=@tok AND Used=0 AND ExpiryDate > NOW()";
                    int valido = 0;
                    using (MySqlCommand cmd = new MySqlCommand(sqlCheck, con))
                    {
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.Parameters.AddWithValue("@tok", codigo);
                        valido = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (valido > 0)
                    {
                        // 1. Encriptar Nueva Clave (SHA256 para coincidir con el Login)
                        string hashNuevaClave = GenerarHashSHA256(nuevaClave);

                        // 2. Actualizar BD
                        string sqlUpd = "UPDATE iniciosesion SET Clave=@pass WHERE ID_Usuario=@uid";
                        using (MySqlCommand cmd = new MySqlCommand(sqlUpd, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", hashNuevaClave);
                            cmd.Parameters.AddWithValue("@uid", uid);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Quemar Token
                        string sqlBurn = "UPDATE passwordresettokens SET Used=1 WHERE ID_Usuario=@uid AND Token=@tok";
                        using (MySqlCommand cmd = new MySqlCommand(sqlBurn, con))
                        {
                            cmd.Parameters.AddWithValue("@uid", uid);
                            cmd.Parameters.AddWithValue("@tok", codigo);
                            cmd.ExecuteNonQuery();
                        }

                        MostrarMensaje("¡Éxito! Redirigiendo al Login...", false);
                        Response.AddHeader("REFRESH", "2;URL=Login.aspx");
                    }
                    else
                    {
                        MostrarMensaje("Código incorrecto o expirado.", true);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error: " + ex.Message, true);
                }
            }
        }

        // --- FUNCIONES AUXILIARES ---

        private bool EnviarEmailGmail(string destino, string nombre, string codigo)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(correoEmisor, "Soporte Óptica 20/20");
                mail.To.Add(destino);
                mail.Subject = "Código de Recuperación";
                mail.Body = $@"
                    <div style='background:#f4f4f4; padding:20px; font-family:sans-serif;'>
                        <div style='background:#fff; padding:30px; border-radius:10px; border-top: 5px solid #D4AF37; max-width:400px; margin:auto;'>
                            <h2 style='color:#333; text-align:center;'>Recuperar Contraseña</h2>
                            <p>Hola <strong>{nombre}</strong>,</p>
                            <p>Usa el siguiente código para acceder a tu cuenta:</p>
                            <div style='background:#eee; padding:15px; text-align:center; font-size:24px; font-weight:bold; letter-spacing:5px; color:#D4AF37;'>
                                {codigo}
                            </div>
                            <p style='font-size:12px; color:#999; text-align:center; margin-top:20px;'>Este código expira en 15 minutos.</p>
                        </div>
                    </div>";
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(correoEmisor, claveAplicacion);
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SMTP Error: " + ex.Message);
                return false;
            }
        }

        // Genera el mismo Hash que usas en el Login
        private string GenerarHashSHA256(string texto)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private void MostrarMensaje(string msg, bool error)
        {
            lblMensaje.Text = msg;
            lblMensaje.CssClass = "alert-custom " + (error ? "alert-error" : "alert-success");
            lblMensaje.Visible = true;
        }

        private string OcultarCorreo(string email)
        {
            if (!email.Contains("@")) return email;
            var partes = email.Split('@');
            return partes[0].Substring(0, Math.Min(3, partes[0].Length)) + "***@" + partes[1];
        }
    }
}