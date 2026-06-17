<%@ Page Title="Gestión de Empresas" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Empresa.aspx.cs" Inherits="Optica.Formularios.Empresa" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root {
            --bg-app: #f8fafc;
            --bg-card: #ffffff;
            --text-main: #1e293b;
            --text-muted: #64748b;
            --border-input: #cbd5e1;
            --accent: #3b82f6;
            --accent-hover: #2563eb;
            --success-bg: #ecfdf5;
            --success-text: #059669;
            --error: #ef4444;
        }

        .empresa-wrapper {
            font-family: 'Inter', sans-serif;
            background-color: var(--bg-app);
            padding: 25px 40px;
            box-sizing: border-box;
            color: var(--text-main);
            height: auto;
        }

        .page-header {
            margin-bottom: 20px;
            border-bottom: 1px solid #e2e8f0;
            padding-bottom: 10px;
        }

        .page-title {
            font-size: 1.4rem;
            font-weight: 700;
            color: var(--text-main);
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .content-grid {
            display: grid;
            grid-template-columns: 300px 1fr;
            gap: 25px;
            align-items: start;
        }

        .ui-card {
            background: var(--bg-card);
            border-radius: 12px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.04);
            padding: 30px; 
            border: 1px solid #f1f5f9;
        }

        .profile-container {
            text-align: center;
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        .logo-box {
            background-color: #fff;
            padding: 8px;
            border-radius: 20px;
            box-shadow: 0 5px 15px rgba(0,0,0,0.05);
            margin-bottom: 15px;
            border: 1px solid #f8fafc;
        }

        .img-profile {
            width: 170px;
            height: 170px;
            object-fit: contain;
            border-radius: 12px;
            display: block;
        }

        .company-label {
            font-size: 1.2rem;
            font-weight: 700;
            color: var(--accent);
            margin-bottom: 5px;
        }

        .email-label {
            color: var(--text-muted);
            font-size: 0.9rem;
            margin-bottom: 20px;
            background: #f1f5f9;
            padding: 5px 15px;
            border-radius: 20px;
            word-break: break-all;
        }

        .status-pill {
            background-color: var(--success-bg);
            color: var(--success-text);
            font-weight: 600;
            font-size: 0.8rem;
            padding: 6px 20px;
            border-radius: 50px;
        }

        .form-title-tab {
            font-size: 1.1rem;
            font-weight: 600;
            color: var(--text-main);
            border-bottom: 2px solid var(--accent);
            display: inline-block;
            padding-bottom: 8px;
            margin-bottom: 25px;
        }

        .input-group-row {
            display: flex;
            align-items: flex-start; 
            margin-bottom: 20px;
        }

        .field-label {
            width: 120px;
            text-align: right;
            padding-right: 20px;
            font-weight: 500;
            color: var(--text-muted);
            font-size: 0.95rem;
            flex-shrink: 0;
            padding-top: 10px; 
        }

        .field-input-container {
            flex-grow: 1;
        }

        .ui-input {
            width: 100%;
            padding: 0 15px;
            height: 42px;
            font-size: 0.95rem;
            color: var(--text-main);
            border: 1px solid var(--border-input);
            border-radius: 8px;
            font-family: 'Inter', sans-serif;
            box-sizing: border-box;
        }

        .ui-input:focus {
            outline: none;
            border-color: var(--accent);
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
        }

        .ui-input-group {
            display: flex;
            align-items: center;
            border: 1px solid var(--border-input);
            border-radius: 8px;
            overflow: hidden;
            height: 42px;
        }
        
        .ui-input-group:focus-within {
            border-color: var(--accent);
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
        }

        .group-addon {
            background-color: #f1f5f9;
            color: var(--text-muted);
            padding: 0 15px;
            height: 100%;
            display: flex;
            align-items: center;
            font-weight: 500;
            border-right: 1px solid var(--border-input);
            font-size: 0.9rem;
        }

        .ui-input-group .ui-input {
            border: none;
            border-radius: 0;
            height: 100%;
            padding: 0 15px;
        }

        .upload-box {
            border: 1px dashed var(--border-input);
            background-color: #f8fafc;
            padding: 12px 15px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            gap: 15px;
        }

        .preview-small {
            width: 45px;
            height: 45px;
            border-radius: 8px;
            object-fit: cover;
            border: 1px solid #e2e8f0;
        }

        .btn-primary-action {
            background-color: var(--accent);
            color: #ffffff;
            font-weight: 600;
            padding: 12px 28px;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: background-color 0.2s;
            font-size: 0.95rem;
            float: right;
        }

        .btn-primary-action:hover { background-color: var(--accent-hover); }

        .error-msg {
            color: var(--error);
            font-size: 0.8rem;
            margin-top: 4px;
            display: flex;
            align-items: center;
            font-weight: 600;
        }
        .error-msg i { margin-right: 5px; }

        @media (max-width: 992px) {
            .content-grid { grid-template-columns: 1fr; }
            .img-profile { width: 140px; height: 140px; }
            .empresa-wrapper { padding: 15px; }
            .input-group-row { flex-direction: column; align-items: flex-start; }
            .field-label { width: 100%; text-align: left; padding-right: 0; padding-top: 0; margin-bottom: 5px; }
            .field-input-container { width: 100%; }
            .upload-box { flex-direction: column; align-items: flex-start; }
            .btn-primary-action { float: none; width: 100%; text-align: center; }
        }
    </style>

    <script type="text/javascript">
        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                var target = e.target;
                if ((target.tagName === "INPUT" && target.type === "submit") || target.type === "file") {
                    return true;
                }
                e.preventDefault();
                var form = target.form;
                if (form) {
                    var elements = Array.from(form.querySelectorAll('input:not([type=hidden]):not([disabled]):not([readonly]), select:not([disabled]), input[type=submit]:not([disabled])'));
                    var index = elements.indexOf(target);
                    if (index > -1 && index < elements.length - 1) {
                        elements[index + 1].focus();
                    }
                }
            }
        });

        var guardando = false;
        function confirmarGuardar(sender) {
            if (guardando) return true;
            Swal.fire({
                title: '¿Desea guardar los cambios?',
                text: "Se subirá la imagen y se actualizará la información de la empresa.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#3b82f6',
                cancelButtonColor: '#64748b',
                confirmButtonText: 'Sí, guardar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    guardando = true;

                    Swal.fire({
                        title: 'Subiendo imagen...',
                        text: 'Por favor, espere un momento.',
                        allowOutsideClick: false,
                        showConfirmButton: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    sender.click();
                }
            });
            return false;
        }

        function previewImage(input) {
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    document.getElementById('<%= imgLogoPreview.ClientID %>').src = e.target.result;
                    document.getElementById('<%= imgLogoCard.ClientID %>').src = e.target.result;
                }
                reader.readAsDataURL(input.files[0]);
            }
        }

        function showSuccess(message) {
            Swal.fire({
                icon: 'success', title: '¡Listo!', text: message,
                confirmButtonColor: '#3b82f6', background: '#fff'
            });
        }

        function showError(message) {
            Swal.fire({
                icon: 'error', title: 'Atención', text: message,
                confirmButtonColor: '#ef4444', background: '#fff'
            });
        }

        function validateForm() {
            var nombre = document.getElementById('<%= txtNombre.ClientID %>').value.trim();
            var ruc = document.getElementById('<%= txtRUC.ClientID %>').value.trim();
            var correo = document.getElementById('<%= txtCorreo.ClientID %>').value.trim();
            var telefono = document.getElementById('<%= txtTelefono.ClientID %>').value.trim();

            if (!nombre) { showError("Ingrese el nombre de la empresa."); return false; }

            if (ruc) {
                if (ruc.length !== 14) { showError("El RUC debe tener 14 caracteres."); return false; }
                if (!/^\d{13}[A-Za-z]$/.test(ruc)) { showError("El RUC debe ser 13 números seguidos de 1 letra."); return false; }
            } else {
                showError("El RUC es obligatorio."); return false;
            }

            if (correo && !/^[\w\.-]+@(gmail\.com|hotmail\.com)$/i.test(correo)) {
                showError("Correo no permitido (use gmail o hotmail)."); return false;
            }
            if (telefono && !/^[2578][0-9]{7}$/.test(telefono)) {
                showError("Teléfono inválido."); return false;
            }
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="empresa-wrapper">
        <div class="page-header">
            <div class="page-title">
                <i class="fas fa-sliders-h" style="color: #64748b;"></i> Configuración de Empresa
            </div>
        </div>

        <div class="content-grid">
            <div class="ui-card profile-container">
                <div class="logo-box">
                    <asp:Image ID="imgLogoCard" runat="server" CssClass="img-profile" ImageUrl="~/Images/default-logo.png" />
                </div>
                <asp:Label ID="lblNombreCard" runat="server" CssClass="company-label" Text="Optica 20/20"></asp:Label>
                <div class="email-label">
                    <i class="far fa-envelope"></i>&nbsp;
                    <asp:Label ID="lblCorreoCard" runat="server" Text="..."></asp:Label>
                </div>
                <span class="status-pill">Sistema Activo</span>
            </div>

            <div class="ui-card">
                <div class="form-title-tab">Detalles Generales</div>

                <div class="input-group-row">
                    <label class="field-label">Nombre</label>
                    <div class="field-input-container">
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="ui-input" Text="Optica 20/20" ReadOnly="true" Enabled="false" style="background-color: #f1f5f9; color: #64748b; font-weight:bold;"></asp:TextBox>
                    </div>
                </div>

                <div class="input-group-row">
                    <label class="field-label">RUC</label>
                    <div class="field-input-container">
                        <div class="ui-input-group">
                            <span class="group-addon"><i class="far fa-id-card"></i></span>
                            <asp:TextBox ID="txtRUC" runat="server" CssClass="ui-input" MaxLength="14" 
                                placeholder="0011505200001T" 
                                oninput="this.value = this.value.toUpperCase().replace(/[^0-9A-Z]/g, '');"></asp:TextBox>
                        </div>
                        <asp:Label ID="errRUC" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                    </div>
                </div>

                <div class="input-group-row">
                    <label class="field-label">Correo</label>
                    <div class="field-input-container">
                        <asp:TextBox ID="txtCorreo" runat="server" CssClass="ui-input" placeholder="contacto@ejemplo.com" TextMode="Email"></asp:TextBox>
                        <asp:Label ID="errCorreo" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                    </div>
                </div>

                <div class="input-group-row">
                    <label class="field-label">Teléfono</label>
                    <div class="field-input-container">
                        <div class="ui-input-group">
                            <span class="group-addon">505</span>
                            <asp:TextBox ID="txtTelefono" runat="server" CssClass="ui-input" MaxLength="8" placeholder="88888888" oninput="this.value = this.value.replace(/[^0-9]/g, '');"></asp:TextBox>
                        </div>
                        <asp:Label ID="errTelefono" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                    </div>
                </div>

                <div class="input-group-row" style="margin-top: 25px;">
                    <label class="field-label">Logo</label>
                    <div class="field-input-container">
                        <div class="upload-box">
                            <asp:Image ID="imgLogoPreview" runat="server" CssClass="preview-small" ImageUrl="~/Images/default-logo.png" />
                            <div style="flex-grow: 1; width: 100%;">
                                <asp:FileUpload ID="fuLogo" runat="server" CssClass="ui-input" style="padding-top: 9px;" onchange="previewImage(this);" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="input-group-row" style="margin-top: 30px; margin-bottom: 0;">
                    <div style="width: 100%;">
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar Cambios" 
                            CssClass="btn-primary-action" OnClick="btnGuardar_Click" 
                            OnClientClick="return validateForm() && confirmarGuardar(this);" />
                    </div>
                </div>

            </div>
        </div>
    </div>
</asp:Content>