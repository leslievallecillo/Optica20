<%@ Page Title="Mi Perfil" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Perfil.aspx.cs" Inherits="Optica.Formularios.Perfil" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        .profile-container { display: flex; flex-wrap: wrap; gap: 20px; max-width: 1000px; margin: 20px auto; }
        .profile-sidebar { flex: 1; min-width: 250px; background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); text-align: center; }
        .profile-main { flex: 2; min-width: 300px; background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
        
        .avatar-current { width: 150px; height: 150px; border-radius: 50%; object-fit: cover; border: 4px solid #f4f6f9; box-shadow: 0 4px 8px rgba(0,0,0,0.1); margin-bottom: 15px; }
        .role-badge { display: inline-block; padding: 5px 15px; background: #0056b3; color: white; border-radius: 20px; font-size: 0.85rem; font-weight: 600; text-transform: uppercase; margin-bottom: 20px; }
        
        .form-group { margin-bottom: 20px; text-align: left; }
        .form-group label { display: block; font-weight: 600; color: #495057; margin-bottom: 8px; font-size: 0.9rem; }
        .form-control-std { width: 100%; padding: 10px; border: 1px solid #ced4da; border-radius: 5px; box-sizing: border-box; font-family: inherit; transition: 0.3s; }
        .form-control-std:focus { border-color: #0056b3; outline: none; box-shadow: 0 0 5px rgba(0,86,179,0.2); }
        
        /* Nuevo estilo para los inputs bloqueados */
        .form-control-std[readonly] { background-color: #e9ecef; cursor: not-allowed; color: #6c757d; }

        .avatar-selector { display: flex; gap: 15px; flex-wrap: wrap; margin-top: 10px; }
        .avatar-option { width: 65px; height: 65px; border-radius: 50%; cursor: pointer; border: 3px solid transparent; transition: 0.3s; object-fit: cover; }
        .avatar-option:hover { transform: scale(1.1); }
        .avatar-option.selected { border-color: #28a745; transform: scale(1.1); box-shadow: 0 4px 8px rgba(40,167,69,0.3); }

        .btn-std { padding: 10px 20px; border: none; border-radius: 5px; cursor: pointer; font-weight: 600; transition: 0.3s; display: inline-flex; align-items: center; gap: 8px; color: white; }
        .btn-success { background: #28a745; }
        .btn-success:hover { background: #218838; }
        
        .required-asterisk { color: #dc3545; font-weight: bold; margin-left: 3px; }
    </style>
    <script>
        function selectAvatar(imgUrl, element) {
            document.getElementById('<%= hfAvatarSeleccionado.ClientID %>').value = imgUrl;

            // Cambio en tiempo real de la imagen principal
            document.getElementById('<%= imgAvatarActual.ClientID %>').src = imgUrl;
            
            var avatars = document.querySelectorAll('.avatar-option');
            avatars.forEach(a => a.classList.remove('selected'));
            
            element.classList.add('selected');
        }

        document.addEventListener('DOMContentLoaded', function() {
            var currentAvatar = document.getElementById('<%= hfAvatarSeleccionado.ClientID %>').value;
            if (currentAvatar) {
                var avatars = document.querySelectorAll('.avatar-option');
                avatars.forEach(function (img) {
                    if (img.src === currentAvatar) {
                        img.classList.add('selected');
                    }
                });
            }
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="profile-container">
        <div class="profile-sidebar">
            <asp:Image ID="imgAvatarActual" runat="server" CssClass="avatar-current" />
            <h3 style="margin: 10px 0; color: #333;">Mi Perfil</h3>
            <asp:Label ID="lblRol" runat="server" CssClass="role-badge" Text="Rol"></asp:Label>
            <p style="color: #6c757d; font-size: 0.9rem;">Gestione su información personal y personalice su avatar en el sistema.</p>
        </div>

        <div class="profile-main">
            <h2 style="margin-top: 0; color: #0056b3; border-bottom: 2px solid #f4f6f9; padding-bottom: 10px;">
                <i class="fa-solid fa-user-pen"></i> Editar Información
            </h2>
            
            <div class="form-group">
                <label>Nombres <span class="required-asterisk">*</span></label>
                <asp:TextBox ID="txtNombres" runat="server" CssClass="form-control-std" ReadOnly="true"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Apellidos <span class="required-asterisk">*</span></label>
                <asp:TextBox ID="txtApellidos" runat="server" CssClass="form-control-std" ReadOnly="true"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Correo Electrónico <span class="required-asterisk">*</span></label>
                <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control-std" TextMode="Email" ReadOnly="true"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Seleccionar Avatar Predeterminado</label>
                <asp:HiddenField ID="hfAvatarSeleccionado" runat="server" />
                
                <div class="avatar-selector">
                    <img src="https://avataaars.io/?avatarStyle=Circle&topType=LongHairStraight&accessoriesType=Blank&hairColor=BrownDark&facialHairType=Blank&clotheType=BlazerShirt&eyeType=Default&eyebrowType=Default&mouthType=Default&skinColor=Light" class="avatar-option" onclick="selectAvatar(this.src, this)" alt="Avatar 1" />
                    <img src="https://avataaars.io/?avatarStyle=Circle&topType=NoHair&accessoriesType=Blank&hairColor=BrownDark&facialHairType=Blank&clotheType=BlazerShirt&eyeType=Default&eyebrowType=Default&mouthType=Default&skinColor=Light" class="avatar-option" onclick="selectAvatar(this.src, this)" alt="Avatar 2" />
                    <img src="https://avataaars.io/?avatarStyle=Circle&topType=LongHairBob&accessoriesType=Blank&hairColor=BrownDark&facialHairType=Blank&clotheType=BlazerShirt&eyeType=Default&eyebrowType=Default&mouthType=Default&skinColor=Light" class="avatar-option" onclick="selectAvatar(this.src, this)" alt="Avatar 3" />
                    <img src="https://avataaars.io/?avatarStyle=Circle&topType=NoHair&accessoriesType=Prescription02&facialHairType=Blank&clotheType=BlazerShirt&eyeType=Default&eyebrowType=Default&mouthType=Eating&skinColor=Black" class="avatar-option" onclick="selectAvatar(this.src, this)" alt="Avatar 4" />
                    <img src="https://avataaars.io/?avatarStyle=Circle&topType=ShortHairShortFlat&accessoriesType=Blank&hairColor=Black&facialHairType=Blank&clotheType=Hoodie&clotheColor=Blue03&eyeType=Default&eyebrowType=Default&mouthType=Smile&skinColor=Tanned" class="avatar-option" onclick="selectAvatar(this.src, this)" alt="Avatar 5" />
                    <img src="https://avataaars.io/?avatarStyle=Circle&topType=LongHairCurly&accessoriesType=Kurt&hairColor=Blonde&facialHairType=Blank&clotheType=ShirtVNeck&clotheColor=Pink&eyeType=Happy&eyebrowType=Default&mouthType=Smile&skinColor=Light" class="avatar-option" onclick="selectAvatar(this.src, this)" alt="Avatar 6" />
                </div>
            </div>

            <div style="text-align: right; margin-top: 30px;">
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar Cambios" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" />
            </div>
        </div>
    </div>
</asp:Content>