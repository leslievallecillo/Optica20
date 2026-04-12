<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecuperarContra.aspx.cs" Inherits="Optica.Formularios.RecuperarContra" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Recuperar Contraseña - Óptica 20/20</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&display=swap" rel="stylesheet">

    <style>
        body {
            background-color: #f0f2f5; /* Fondo gris claro como la imagen */
            font-family: 'Poppins', sans-serif;
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0;
        }

        .login-card {
            background: #ffffff;
            padding: 40px 30px;
            border-radius: 20px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.05);
            width: 100%;
            max-width: 400px;
            text-align: center;
        }

        /* Estilos del Logo */
        .brand-icon {
            color: #FFC107; /* Color amarillo */
            font-size: 50px;
            margin-bottom: 10px;
        }

        .brand-title {
            font-weight: 700;
            color: #2c3e50;
            font-size: 1.5rem;
            margin-bottom: 30px;
        }
        .brand-title span {
            color: #FFC107; /* El 20/20 en amarillo */
        }

        /* Estilos de los Inputs para que parezcan a la imagen (fondo gris, sin borde) */
        .form-label-custom {
            font-size: 0.75rem;
            font-weight: 700;
            color: #6c757d;
            text-transform: uppercase;
            letter-spacing: 1px;
            display: block;
            text-align: left;
            margin-bottom: 5px;
            margin-top: 15px;
        }

        .form-control-custom {
            background-color: #f1f3f5;
            border: 1px solid transparent;
            border-radius: 8px;
            padding: 12px 15px;
            font-size: 0.95rem;
            color: #495057;
            width: 100%;
            transition: all 0.3s;
        }

        .form-control-custom:focus {
            background-color: #ffffff;
            border-color: #FFC107;
            box-shadow: 0 0 0 4px rgba(255, 193, 7, 0.1);
            outline: none;
        }

        /* Botón Amarillo */
        .btn-custom {
            background-color: #ffe082; /* Amarillo suave */
            background: linear-gradient(to bottom, #ffe082, #ffd54f);
            color: #2c3e50;
            font-weight: 700;
            text-transform: uppercase;
            border: none;
            border-radius: 8px;
            padding: 12px;
            width: 100%;
            margin-top: 25px;
            letter-spacing: 1px;
            transition: transform 0.2s;
        }

        .btn-custom:hover {
            background: #ffca28;
            transform: translateY(-2px);
            color: #000;
        }

        /* Mensajes de Alerta */
        .alert-custom {
            border-radius: 8px;
            font-size: 0.9rem;
            padding: 10px;
            margin-bottom: 20px;
            display: block;
        }
        .alert-error { background-color: #ffebee; color: #c62828; border: 1px solid #ffcdd2; }
        .alert-success { background-color: #e8f5e9; color: #2e7d32; border: 1px solid #c8e6c9; }

        .back-link {
            display: block;
            margin-top: 20px;
            color: #6c757d;
            text-decoration: none;
            font-size: 0.9rem;
        }
        .back-link:hover {
            color: #2c3e50;
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-card">
            
            <div class="mb-3">
                <i class="fas fa-glasses brand-icon"></i>
                <div class="brand-title">Óptica <span>20/20</span></div>
            </div>

            <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="alert-custom"></asp:Label>

            <asp:Panel ID="pnlSolicitar" runat="server">
                <div class="text-start">
                    <span class="form-label-custom">USUARIO / CORREO</span>
                    <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control-custom" placeholder="Ingresa tu usuario"></asp:TextBox>
                </div>

                <asp:Button ID="btnEnviar" runat="server" CssClass="btn-custom" Text="ENVIAR CÓDIGO" OnClick="btnEnviar_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlCambiar" runat="server" Visible="false">
                <p class="small text-muted mb-3">Hemos enviado un código a tu correo.</p>

                <div class="text-start">
                    <span class="form-label-custom">CÓDIGO DE VERIFICACIÓN</span>
                    <asp:TextBox ID="txtCodigo" runat="server" CssClass="form-control-custom text-center" placeholder="000000" MaxLength="6"></asp:TextBox>
                </div>

                <div class="text-start">
                    <span class="form-label-custom">NUEVA CONTRASEÑA</span>
                    <asp:TextBox ID="txtNuevaClave" runat="server" CssClass="form-control-custom" TextMode="Password" placeholder="Ingresa tu nueva clave"></asp:TextBox>
                </div>

                <asp:Button ID="btnActualizar" runat="server" CssClass="btn-custom" Text="CAMBIAR CONTRASEÑA" OnClick="btnActualizar_Click" />
            </asp:Panel>

            <div class="mt-3">
                <a href="Login.aspx" class="back-link">¿Recordaste tu contraseña?</a>
            </div>

        </div>
    </form>
</body>
</html>