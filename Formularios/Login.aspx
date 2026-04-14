<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Optica.Formularios.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Login - Óptica Visionary</title>
    
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700&display=swap" rel="stylesheet">

    <style>
        :root {
            --bg-color: #f3f4f6;
            --card-bg: #ffffff;
            --accent-color: #ffe082;
            --accent-hover: #ffca28;
            --text-color: #374151;
            --text-muted: #6b7280;
            --input-bg: #f9fafb;
            --input-border: #e5e7eb;
            --placeholder: #9ca3af;
        }

        body {
            background-color: var(--bg-color);
            font-family: 'Montserrat', sans-serif;
            height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0;
            overflow: hidden;
            box-sizing: border-box;
        }

        .contenedor-login {
            background-color: var(--card-bg);
            width: 100%;
            max-width: 400px;
            padding: 50px 40px;
            border-radius: 16px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.05);
            text-align: center;
            position: relative;
            border: 1px solid rgba(0,0,0,0.02);
            opacity: 0;
            transform: scale(0.95);
            animation: cardEntrance 0.8s cubic-bezier(0.2, 0.8, 0.2, 1) forwards;
            box-sizing: border-box;
        }

        @keyframes cardEntrance {
            to { opacity: 1; transform: scale(1); }
        }

        .logo-container {
            margin-bottom: 30px;
            opacity: 0;
            animation: fadeInDown 0.8s ease forwards 0.3s;
        }

        .logo-icon {
            font-size: 50px;
            color: var(--accent-hover);
            margin-bottom: 15px;
            display: inline-block;
            animation: floating 4s ease-in-out infinite;
        }

        @keyframes floating {
            0% { transform: translateY(0px); }
            50% { transform: translateY(-8px); }
            100% { transform: translateY(0px); }
        }

        @keyframes fadeInDown {
            from { opacity: 0; transform: translateY(-20px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .titulo-login {
            color: var(--text-color);
            font-weight: 700;
            font-size: 22px;
            letter-spacing: 0.5px;
            margin: 0;
        }
        
        .titulo-login span {
            color: var(--accent-hover);
        }

        .form-group-anim {
            opacity: 0;
            transform: translateY(10px);
            animation: fadeInUp 0.5s ease forwards;
        }

        .delay-1 { animation-delay: 0.5s; }
        .delay-2 { animation-delay: 0.7s; }
        .delay-btn { animation-delay: 0.9s; }

        @keyframes fadeInUp {
            to { opacity: 1; transform: translateY(0); }
        }

        .input-wrapper {
            position: relative;
            margin-bottom: 25px;
            text-align: left;
        }

        .input-label {
            color: var(--text-muted);
            font-size: 11px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 8px;
            display: block;
            margin-left: 2px;
        }

        .form-control {
            height: 52px;
            background-color: var(--input-bg);
            border: 2px solid var(--input-border);
            color: var(--text-color);
            border-radius: 10px;
            padding-left: 20px;
            padding-right: 50px;
            font-size: 14px;
            transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
        }

        .form-control::placeholder {
            color: var(--placeholder);
            font-size: 13px;
        }

        .form-control:focus {
            background-color: #ffffff;
            border-color: var(--accent-hover);
            box-shadow: 0 0 0 4px rgba(255, 202, 40, 0.1);
            color: var(--text-color);
        }

        .form-control:focus + .eye-icon {
            color: var(--accent-hover);
            transform: scale(1.1);
        }

        .eye-icon {
            position: absolute;
            right: 18px;
            top: 42px;
            color: #b0b0b0;
            cursor: pointer;
            font-size: 18px;
            transition: all 0.3s ease;
            z-index: 10;
        }

        .eye-icon:hover {
            color: var(--text-color);
        }

        .btn-login {
            width: 100%;
            padding: 16px;
            margin-top: 10px;
            background-color: var(--accent-color);
            color: #374151;
            border: none;
            border-radius: 10px;
            font-size: 15px;
            font-weight: 700;
            letter-spacing: 0.5px;
            cursor: pointer;
            transition: all 0.3s ease;
            opacity: 0;
            transform: translateY(10px);
        }

        .btn-login:hover {
            background-color: var(--accent-hover);
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(255, 202, 40, 0.25);
        }

        .btn-login:active {
            transform: scale(0.98);
        }

        .btn-volver {
            width: 100%;
            padding: 16px;
            margin-top: 15px;
            background-color: transparent;
            color: var(--text-color);
            border: 2px solid var(--input-border);
            border-radius: 10px;
            font-size: 15px;
            font-weight: 700;
            letter-spacing: 0.5px;
            cursor: pointer;
            transition: all 0.3s ease;
            opacity: 0;
            transform: translateY(10px);
        }

        .btn-volver:hover {
            background-color: var(--input-bg);
            border-color: var(--text-muted);
        }

        .footer-link {
            display: block;
            margin-top: 25px;
            text-align: center;
            opacity: 0;
            animation: fadeInUp 0.5s ease forwards 1.1s;
        }

        .footer-link a {
            color: var(--text-muted);
            text-decoration: none;
            font-size: 13px;
            font-weight: 500;
            transition: color 0.3s;
        }

        .footer-link a:hover {
            color: var(--accent-hover);
        }

        .alert-error {
            color: #d32f2f;
            background-color: #ffebee;
            padding: 12px 15px;
            border-radius: 6px;
            font-size: 13px;
            display: block;
            margin-top: 20px;
            border: 1px solid #ffcdd2;
            border-left: 4px solid #d32f2f;
            text-align: left;
            animation: shake 0.4s ease-in-out;
            box-sizing: border-box;
        }

        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-5px); }
            75% { transform: translateX(5px); }
        }

        @media (max-width: 480px) {
            body {
                overflow-y: auto;
                padding: 20px;
            }
            .contenedor-login {
                padding: 40px 20px;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        
        <script>
            function togglePass() {
                var x = document.getElementById('<%= txtClave.ClientID %>');
                var icon = document.getElementById('eyeIcon');

                icon.style.transform = "scale(0.8)";
                setTimeout(() => icon.style.transform = "scale(1)", 150);

                if (x.type === "password") {
                    x.type = "text";
                    icon.classList.remove('fa-eye');
                    icon.classList.add('fa-eye-slash');
                } else {
                    x.type = "password";
                    icon.classList.remove('fa-eye-slash');
                    icon.classList.add('fa-eye');
                }
            }

            function handleEnter(e, nextElementId) {
                if (e.keyCode === 13) {
                    e.preventDefault();
                    if (nextElementId === 'btnLogin') {
                        document.getElementById('<%= btnLogin.ClientID %>').click();
                    } else {
                        var nextInput = document.getElementById('<%= txtClave.ClientID %>');
                        if (nextInput) {
                            nextInput.focus();
                        }
                    }
                }
            }

            function iniciarTemporizador(segundosRestantes) {
                var btnLogin = document.getElementById('<%= btnLogin.ClientID %>');
                var lblError = document.getElementById('<%= lblError.ClientID %>');

                if (btnLogin) {
                    btnLogin.disabled = true;
                    btnLogin.style.opacity = '0.5';
                    btnLogin.style.cursor = 'not-allowed';
                    btnLogin.value = 'BLOQUEADO';
                }

                if (lblError) {
                    lblError.style.display = 'block';
                }

                function actualizarContador() {
                    if (segundosRestantes <= 0) {
                        clearInterval(intervalo);
                        if (btnLogin) {
                            btnLogin.disabled = false;
                            btnLogin.style.opacity = '1';
                            btnLogin.style.cursor = 'pointer';
                            btnLogin.value = 'INICIAR SESIÓN';
                        }
                        if (lblError) {
                            lblError.style.display = 'none';
                        }
                    } else {
                        var m = Math.floor(segundosRestantes / 60);
                        var s = Math.floor(segundosRestantes % 60);

                        var mFormat = m < 10 ? "0" + m : m;
                        var sFormat = s < 10 ? "0" + s : s;

                        if (lblError) {
                            lblError.innerHTML = '<strong><i class="fas fa-lock"></i> Sistema bloqueado</strong><br/>Vuelve a intentar en: <strong>' + mFormat + ':' + sFormat + '</strong>';
                        }
                        segundosRestantes--;
                    }
                }

                actualizarContador();
                var intervalo = setInterval(actualizarContador, 1000);
            }
        </script>

        <div class="contenedor-login">
            
            <div class="logo-container">
                <i class="fas fa-glasses logo-icon"></i>
                <h2 class="titulo-login">Óptica <span>20/20</span></h2>
            </div>

            <div class="input-wrapper form-group-anim delay-1">
                <label class="input-label">Usuario</label>
                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" placeholder="Ingresa tu usuario" onkeydown="handleEnter(event, 'txtClave')"></asp:TextBox>
            </div>

            <div class="input-wrapper form-group-anim delay-2">
                <label class="input-label">Contraseña</label>
                <asp:TextBox ID="txtClave" runat="server" CssClass="form-control" TextMode="Password" placeholder="Ingresa tu contraseña" onkeydown="handleEnter(event, 'btnLogin')"></asp:TextBox>
                <i class="fas fa-eye eye-icon" id="eyeIcon" onclick="togglePass()"></i>
            </div>

            <asp:Label ID="lblError" runat="server" CssClass="alert-error" Visible="false"></asp:Label>

            <asp:Button ID="btnLogin" runat="server" CssClass="btn-login delay-btn form-group-anim" Text="INICIAR SESIÓN" OnClick="btnLogin_Click" />
            <asp:Button ID="btnVolver" runat="server" CssClass="btn-volver delay-btn form-group-anim" Text="VOLVER AL CATÁLOGO" OnClientClick="window.location.href='CatalogoCliente.aspx'; return false;" />
            
            <div class="text-center mt-3">
                <a href="RecuperarContra.aspx" style="color: #666; text-decoration: none; font-size: 0.9rem;">
                    ¿Olvidaste tu contraseña?
                </a>
            </div>
        </div>
    </form>
</body>
</html>