<%@ Page Title="Gestión de Usuarios" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionUsuarios.aspx.cs" Inherits="Optica.Formularios.GestionUsuarios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        /* === ESTILOS (Se mantienen igual) === */
        :root { --color-primary: #0056b3; --color-danger: #d32f2f; --color-text: #333; --color-border: #ced4da; --bg-light: #f8f9fa; }
        .form-group-formal { margin-bottom: 20px; }
        .lbl-formal { font-weight: 700; font-size: 0.9rem; color: #2c3e50; display: block; margin-bottom: 5px; }
        .form-control-formal { width: 100%; padding: 10px 12px; font-size: 1rem; border: 1px solid var(--color-border); border-radius: 4px; transition: all 0.3s; background-color: #fff; box-sizing: border-box; }
        .form-control-formal:focus { border-color: var(--color-primary); box-shadow: 0 0 0 3px rgba(0, 86, 179, 0.1); outline: none; }
        .error-message-formal { color: var(--color-danger); font-size: 0.85rem; margin-top: 4px; display: flex; align-items: center; font-weight: 600; }
        .error-message-formal i { margin-right: 5px; }
        .help-text-formal { font-size: 0.75rem; color: #636c72; margin-top: 4px; display: block; }
        .panel-card { background: #fff; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); margin-bottom: 20px; border: 1px solid #eaeaea; }
        .panel-header { padding: 15px 20px; border-bottom: 1px solid #eaeaea; display: flex; justify-content: space-between; align-items: center; background-color: var(--bg-light); border-radius: 8px 8px 0 0; }
        .panel-body { padding: 25px; }
        .toolbar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; background-color: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid var(--color-border); margin-bottom: 20px; }
        .form-grid-layout { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }
        .form-full-width { grid-column: 1 / -1; }
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-std thead th { background-color: #f1f3f5; padding: 12px; text-align: left; border-bottom: 2px solid #ddd; }
        .table-std tbody td { padding: 10px; border-bottom: 1px solid #eee; }
        .badge-status { padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: bold; text-transform: uppercase; }
        .badge-active { background-color: #e8f5e9; color: #2e7d32; }
        .badge-inactive { background-color: #ffebee; color: #c62828; }
        .btn-std { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; gap: 5px; font-size: 0.9rem; transition: 0.2s; }
        .btn-primary { background-color: var(--color-primary); color: white; }
        .btn-success { background-color: #28a745; color: white; }
        .btn-danger { background-color: var(--color-danger); color: white; }
        .btn-secondary { background-color: #6c757d; color: white; }
        .btn-info { background-color: #17a2b8; color: white; }
        .section-divider { margin: 25px 0; border-bottom: 2px solid #f1f1f1; padding-bottom: 10px; font-size: 1.1rem; color: var(--color-primary); font-weight: bold; }
    </style>

    <script type="text/javascript">
        // === LÓGICA ENTER PARA NAVEGAR Y GUARDAR ===
        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                var target = e.target;

                // Si es un textarea o un botón submit, dejar que actúe normal (o click)
                if (target.tagName === "TEXTAREA" || (target.tagName === "INPUT" && target.type === "submit")) {
                    return true;
                }

                e.preventDefault(); // Prevenir el submit por defecto del form

                // Obtener todos los elementos focusables del formulario actual visible
                var formElements = Array.from(document.querySelectorAll('input:not([type=hidden]):not([disabled]), select:not([disabled]), button:not([disabled]), input[type=submit]'));
                var index = formElements.indexOf(target);

                if (index > -1 && index < formElements.length - 1) {
                    var nextElement = formElements[index + 1];

                    // Si el siguiente elemento es el botón cancelar, saltarlo e ir al Guardar
                    if (nextElement.value === "Cancelar" || nextElement.innerText === "Cancelar") {
                        if (index + 2 < formElements.length) {
                            formElements[index + 2].focus();
                        }
                    } else {
                        nextElement.focus(); // Mover foco al siguiente
                    }
                } else {
                    // Si es el último elemento o estamos en el botón Guardar, hacer click
                    // Buscamos el botón guardar explícitamente
                    var btnGuardar = document.getElementById('<%= btnGuardar.ClientID %>');
                    if (btnGuardar) btnGuardar.click();
                }
            }
        });

        // === VALIDACIONES DE FORMATO (Mantenemos las anteriores) ===
        function soloLetras(input) {
            let val = input.value.replace(/[^a-zA-ZñÑáéíóúÁÉÍÓÚ\s]/g, '');
            val = val.replace(/(.)\1{2,}/g, '$1$1'); // Anti-spam caracteres
            val = val.replace(/\s\s+/g, ' '); // Anti-doble espacio
            val = val.toLowerCase().replace(/(?:^|\s)\S/g, function (a) { return a.toUpperCase(); });
            input.value = val;
        }

        function soloNumeros(input) { input.value = input.value.replace(/[^0-9]/g, ''); }
        function soloUsuario(input) { input.value = input.value.replace(/[^a-zA-Z0-9]/g, '').toLowerCase(); }

        var guardando = false;
        function confirmarGuardar(sender) {
            if (guardando) return true;
            Swal.fire({
                title: '¿Guardar Usuario?',
                text: "Verifique que los datos sean correctos.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, guardar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    guardando = true;
                    sender.click();
                }
            });
            return false;
        }

        function confirmarBaja(sender) {
            Swal.fire({
                title: '¿Bloquear?',
                text: "El usuario perderá el acceso.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, bloquear'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid" style="padding: 20px;">
        
        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3 style="margin:0; color:var(--color-primary);"><i class="fa-solid fa-users-gear"></i> Gestión de Usuarios</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="+ Nuevo Usuario" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    
                    <div class="toolbar">
                        <div style="flex-grow: 1; min-width: 200px;">
                            <label class="lbl-formal">Búsqueda</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-formal" placeholder="Nombre, Usuario..."></asp:TextBox>
                        </div>
                         <div style="width: 150px;">
                            <label class="lbl-formal">Rol</label>
                            <asp:DropDownList ID="ddlFiltroRol" runat="server" CssClass="form-control-formal">
                                <asp:ListItem Text="Todos" Value=""></asp:ListItem>
                                <asp:ListItem Text="Administrador" Value="Administrador"></asp:ListItem>
                                <asp:ListItem Text="Administrador Financiero" Value="Administrador Financiero"></asp:ListItem>
                                <asp:ListItem Text="Vendedor" Value="Vendedor"></asp:ListItem>
                                <asp:ListItem Text="Optometrista" Value="Optometrista"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                         <div style="width: 150px;">
                            <label class="lbl-formal">Estado</label>
                            <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control-formal">
                                <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                                <asp:ListItem Text="Activo" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Inactivo" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="lbl-formal">&nbsp;</label>
                            <div style="display:flex; gap:10px;">
                                <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                                <asp:Button ID="btnMostrarTodo" runat="server" Text="Mostrar Todo" CssClass="btn-std btn-info" OnClick="btnMostrarTodo_Click" />
                            </div>
                        </div>
                    </div>

                    <asp:GridView ID="gvUsuarios" runat="server" CssClass="table-std" AutoGenerateColumns="False" 
                        DataKeyNames="ID_Usuario" AllowPaging="True" PageSize="10" 
                        OnRowCommand="gvUsuarios_RowCommand" OnPageIndexChanging="gvUsuarios_PageIndexChanging" GridLines="None" ShowHeaderWhenEmpty="true">
                        <Columns>
                            <asp:BoundField DataField="ID_Usuario" HeaderText="ID" ItemStyle-Width="50px" />
                            <asp:BoundField DataField="NombreCompleto" HeaderText="Nombre Completo" />
                            <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" />
                            <asp:BoundField DataField="Rol" HeaderText="Rol" />
                            <asp:BoundField DataField="Correo" HeaderText="Correo" />
                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate>
                                    <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                        <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Usuario") %>' CssClass="btn-std btn-primary" style="padding:4px 8px;"><i class="fa-solid fa-pen"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnBaja" runat="server" CommandName="DarBaja" CommandArgument='<%# Eval("ID_Usuario") %>' CssClass="btn-std btn-danger" style="padding:4px 8px;" OnClientClick="return confirmarBaja(this);" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>'><i class="fa-solid fa-ban"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnReactivar" runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("ID_Usuario") %>' CssClass="btn-std btn-success" style="padding:4px 8px;" OnClientClick="return confirmarBaja(this);" Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>'><i class="fa-solid fa-check"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelMantenimiento" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 900px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Registro"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    <asp:HiddenField ID="hfIDUsuario" runat="server" />

                    <div class="section-divider"><i class="fa-regular fa-id-card"></i> Información Personal</div>

                    <div class="form-grid-layout">
                        <div class="form-group-formal">
                            <label class="lbl-formal">Nombres <span style="color:red">*</span></label>
                            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control-formal" MaxLength="40" oninput="soloLetras(this)" autocomplete="off"></asp:TextBox>
                            <small class="help-text-formal">Máximo 3 palabras (sin apellidos aquí).</small>
                            <asp:Label ID="errNombre" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Apellidos <span style="color:red">*</span></label>
                            <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control-formal" MaxLength="40" oninput="soloLetras(this)" autocomplete="off"></asp:TextBox>
                            <small class="help-text-formal">Máximo 3 palabras.</small>
                            <asp:Label ID="errApellido" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Correo Electrónico <span style="color:red">*</span></label>
                            <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control-formal" TextMode="Email" MaxLength="80" autocomplete="off"></asp:TextBox>
                            <asp:Label ID="errCorreo" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Teléfono <span style="color:red">*</span></label>
                            <div style="display:flex;">
                                <span style="padding:10px; background:#eee; border:1px solid #ced4da; border-right:none; border-radius:4px 0 0 4px; font-weight:bold; color:#555;">505</span>
                                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control-formal" MaxLength="8" style="border-radius:0 4px 4px 0;" oninput="soloNumeros(this)" autocomplete="off" placeholder="88888888"></asp:TextBox>
                            </div>
                            <small class="help-text-formal">8 dígitos. Inicia con 2, 5, 7 u 8.</small>
                            <asp:Label ID="errTelefono" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                    </div>

                    <div class="section-divider"><i class="fa-solid fa-shield-halved"></i> Credenciales</div>

                    <div class="form-grid-layout">
                        <div class="form-group-formal">
                            <label class="lbl-formal">Rol <span style="color:red">*</span></label>
                            <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-control-formal">
                                <asp:ListItem Text="-- Seleccione --" Value=""></asp:ListItem>
                                <asp:ListItem Text="Administrador" Value="Administrador"></asp:ListItem>
                                <asp:ListItem Text="Vendedor" Value="Vendedor"></asp:ListItem>
                                <asp:ListItem Text="Optometrista" Value="Optometrista"></asp:ListItem>
                                <asp:ListItem Text="Administrador Financiero" Value="Administrador Financiero"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:Label ID="errRol" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Usuario <span style="color:red">*</span></label>
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control-formal" MaxLength="20" oninput="soloUsuario(this)" autocomplete="off"></asp:TextBox>
                            <asp:Label ID="errUsuario" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Contraseña</label>
                            <asp:TextBox ID="txtClave" runat="server" CssClass="form-control-formal" TextMode="Password" autocomplete="new-password"></asp:TextBox>
                            <small class="help-text-formal" id="helpClave" runat="server">Mínimo 6 caracteres.</small>
                            <asp:Label ID="errClave" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Confirmar Contraseña</label>
                            <asp:TextBox ID="txtClaveConfirm" runat="server" CssClass="form-control-formal" TextMode="Password" autocomplete="new-password"></asp:TextBox>
                            <asp:Label ID="errClaveConfirm" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                    </div>

                    <div style="text-align:right; margin-top:30px; border-top:1px solid #eee; padding-top:20px;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" formnovalidate="formnovalidate" CausesValidation="false" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar Usuario" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>