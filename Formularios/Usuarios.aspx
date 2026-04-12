<%@ Page Title="Gestión de Usuarios" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Usuarios.aspx.cs" Inherits="Optica.AdministrarAccesos.Usuarios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        /* === ESTILOS GENERALES (Copiados de tu diseño Clientes.aspx) === */
        :root {
            --color-primary: #0056b3;
            --color-secondary: #6c757d;
            --color-success: #28a745;
            --color-danger: #dc3545;
            --color-border: #dee2e6;
            --color-bg: #f4f6f9;
            --color-text: #333;
            --color-info: #17a2b8;
        }

        body {
            background-color: var(--color-bg);
            color: var(--color-text);
            font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
        }

        .container-fluid {
            padding: 20px;
            max-width: 1600px;
            margin: 0 auto;
        }

        /* === PANELES Y CARDS === */
        .panel-card {
            background: #fff;
            border: 1px solid rgba(0,0,0,0.1);
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
            margin-bottom: 20px;
            overflow: hidden;
        }

        .panel-header {
            background-color: #fff;
            padding: 15px 20px;
            border-bottom: 1px solid var(--color-border);
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .panel-header h3 {
            margin: 0;
            font-size: 1.25rem;
            color: var(--color-primary);
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .panel-body {
            padding: 20px;
        }

        /* === FILTROS === */
        .toolbar {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: flex-end;
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 6px;
            border: 1px solid var(--color-border);
            margin-bottom: 20px;
        }

        .form-group-filter {
            display: flex;
            flex-direction: column;
            gap: 5px;
        }

        .form-group-filter label {
            font-size: 0.85rem;
            font-weight: 600;
            color: var(--color-secondary);
        }

        /* === GRID FORMULARIO === */
        .form-grid-layout {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 25px;
        }

        .form-full-width {
            grid-column: 1 / -1;
        }

        /* === CONTROLES DE FORMULARIO === */
        .form-control-std {
            width: 100%;
            padding: 8px 12px;
            border: 1px solid #ced4da;
            border-radius: 4px;
            font-size: 0.95rem;
            transition: border-color 0.15s;
            box-sizing: border-box;
            height: 38px;
        }

        .form-control-std:focus {
            border-color: var(--color-primary);
            outline: 0;
            box-shadow: 0 0 0 0.2rem rgba(0, 86, 179, 0.25);
        }

        .input-group-std {
            display: flex;
            width: 100%;
        }
        .input-group-addon {
            padding: 6px 12px;
            background-color: #e9ecef;
            border: 1px solid #ced4da;
            border-right: none;
            border-radius: 4px 0 0 4px;
            display: flex;
            align-items: center;
            color: #495057;
            font-weight: 500;
        }
        .input-group-std input {
            border-top-left-radius: 0;
            border-bottom-left-radius: 0;
        }

        /* === TEXTOS Y ETIQUETAS === */
        .lbl-std {
            display: block;
            margin-bottom: 5px;
            font-weight: 600;
            color: #495057;
            font-size: 0.9rem;
        }
        
        .required-asterisk { 
            color: var(--color-danger); 
            font-weight: bold;
            margin-left: 3px;
        }

        .help-text {
            display: block;
            font-size: 0.75rem;
            color: #6c757d;
            margin-top: 4px;
            font-style: italic;
            line-height: 1.2;
        }

        .form-section-title {
            grid-column: 1 / -1;
            font-size: 1rem;
            color: var(--color-primary);
            border-bottom: 2px solid #e9ecef;
            padding-bottom: 5px;
            margin-top: 10px;
            margin-bottom: 10px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        /* === TABLA === */
        .table-controls {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 10px;
        }

        .table-scroll-container {
            width: 100%;
            max-height: 600px; 
            overflow-y: auto;
            border: 1px solid var(--color-border);
            border-radius: 4px;
        }
        
        .table-std {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.9rem;
        }

        .table-std thead th {
            background-color: #f8f9fa;
            color: #495057;
            font-weight: 700;
            padding: 12px 15px;
            text-align: left;
            border-bottom: 2px solid var(--color-border);
            position: sticky;
            top: 0;
            z-index: 1;
            white-space: nowrap;
        }

        .table-std tbody td {
            padding: 10px 15px;
            border-bottom: 1px solid var(--color-border);
            vertical-align: middle;
        }

        .table-std tbody tr:hover {
            background-color: #eef2f7;
        }

        /* === BOTONES === */
        .btn-std {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 5px;
            padding: 8px 16px;
            border-radius: 4px;
            border: none;
            cursor: pointer;
            font-weight: 500;
            font-size: 0.9rem;
            transition: all 0.2s;
            text-decoration: none;
        }
        .btn-primary { background-color: var(--color-primary); color: white; }
        .btn-success { background-color: var(--color-success); color: white; }
        .btn-danger { background-color: var(--color-danger); color: white; }
        .btn-secondary { background-color: var(--color-secondary); color: white; }
        .btn-sm { padding: 5px 10px; font-size: 0.8rem; }
        
        .badge-status {
            padding: 4px 8px;
            border-radius: 12px;
            font-size: 0.75rem;
            font-weight: 700;
            text-transform: uppercase;
        }
        .badge-active { background-color: #d4edda; color: #155724; }
        .badge-inactive { background-color: #f8d7da; color: #721c24; }
    </style>

    <script type="text/javascript">
        function confirmarDesactivar(sender) {
            Swal.fire({
                title: '¿Desactivar Usuario?',
                text: "El usuario no podrá iniciar sesión en el sistema.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, desactivar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    eval(sender.href);
                }
            });
            return false;
        }

        function confirmarReactivar(sender) {
            Swal.fire({
                title: '¿Reactivar Usuario?',
                text: "El usuario tendrá acceso nuevamente.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, reactivar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    eval(sender.href);
                }
            });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">
        
        <div style="margin-bottom: 20px;">
            <h2 style="font-weight: 300; color: var(--color-text);">Gestión de <b style="font-weight: 700;">Usuarios del Sistema</b></h2>
        </div>

        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3><i class="fa-solid fa-users-gear"></i> Listado de Usuarios</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="Nuevo Usuario" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    
                    <div class="toolbar">
                        <div class="form-group-filter" style="flex-grow: 1; min-width: 250px;">
                            <label>Búsqueda Rápida</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-std" placeholder="Nombre, Apellido, Usuario o Correo..."></asp:TextBox>
                        </div>
                        <div class="form-group-filter" style="width: 180px;">
                            <label>Rol</label>
                            <asp:DropDownList ID="ddlFiltroRol" runat="server" CssClass="form-control-std">
                                <asp:ListItem Text="Todos" Value=""></asp:ListItem>
                                <asp:ListItem Text="Administrador" Value="Administrador"></asp:ListItem>
                                <asp:ListItem Text="Vendedor" Value="Vendedor"></asp:ListItem>
                                <asp:ListItem Text="Optometrista" Value="Optometrista"></asp:ListItem>
                                <asp:ListItem Text="Gerente" Value="Administrador Financiero"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group-filter" style="width: 150px;">
                            <label>Estado</label>
                            <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control-std">
                                <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                                <asp:ListItem Text="Activo" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Inactivo" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group-filter">
                            <label>&nbsp;</label> 
                            <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                        </div>
                    </div>

                    <div class="table-controls">
                        <div>
                            <label style="font-size: 0.9rem; font-weight: bold; margin-right: 5px;">Mostrar:</label>
                            <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" 
                                CssClass="form-control-std" Style="width: 100px; display: inline-block; height: 30px; padding: 2px 5px;">
                                <asp:ListItem Value="10">10</asp:ListItem>
                                <asp:ListItem Value="20">20</asp:ListItem>
                                <asp:ListItem Value="All">Todos</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="table-scroll-container">
                        <asp:GridView ID="gvUsuarios" runat="server" CssClass="table-std" AutoGenerateColumns="False" 
                            OnRowCommand="gvUsuarios_RowCommand" DataKeyNames="ID_Usuario" 
                            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvUsuarios_PageIndexChanging"
                            GridLines="None" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:BoundField DataField="ID_Usuario" HeaderText="ID" ItemStyle-Width="50px" />
                                <asp:BoundField DataField="Nombres" HeaderText="Nombres" />
                                <asp:BoundField DataField="Apellidos" HeaderText="Apellidos" />
                                <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario (Login)" />
                                <asp:BoundField DataField="Rol" HeaderText="Rol" />
                                <asp:BoundField DataField="Correo" HeaderText="Correo" />
                                <asp:TemplateField HeaderText="Estado" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                            <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="180px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Usuario") %>' 
                                            CssClass="btn-std btn-sm btn-primary" ToolTip="Editar Usuario">
                                            <i class="fa-solid fa-pen-to-square"></i>
                                        </asp:LinkButton>

                                        <asp:LinkButton ID="btnBaja" runat="server" CommandName="Desactivar" CommandArgument='<%# Eval("ID_Usuario") %>' 
                                            CssClass="btn-std btn-sm btn-danger" ToolTip="Desactivar Acceso" 
                                            OnClientClick="return confirmarDesactivar(this);" 
                                            Visible='<%# Convert.ToBoolean(Eval("Estado")) %>'>
                                            <i class="fa-solid fa-ban"></i>
                                        </asp:LinkButton>

                                        <asp:LinkButton ID="btnReactivar" runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("ID_Usuario") %>' 
                                            CssClass="btn-std btn-sm btn-success" ToolTip="Reactivar Acceso" 
                                            OnClientClick="return confirmarReactivar(this);" 
                                            Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>'>
                                            <i class="fa-solid fa-check"></i>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pagination" HorizontalAlign="Center" BackColor="#f8f9fa" />
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelMantenimiento" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 1000px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Registrar Usuario"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    <asp:HiddenField ID="hfIDUsuario" runat="server" />

                    <div class="form-grid-layout">
                        <div class="form-section-title">1. Información Personal</div>
                        
                        <div>
                            <label class="lbl-std">Nombres <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control-std"></asp:TextBox>
                        </div>
                        <div>
                            <label class="lbl-std">Apellidos <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control-std"></asp:TextBox>
                        </div>
                        <div>
                            <label class="lbl-std">Correo Electrónico <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control-std" TextMode="Email"></asp:TextBox>
                        </div>
                        <div>
                            <label class="lbl-std">Teléfono <span class="required-asterisk">*</span></label>
                            <div class="input-group-std">
                                <span class="input-group-addon">505</span>
                                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control-std" MaxLength="8" placeholder="88888888"></asp:TextBox>
                            </div>
                            <small class="help-text">Debe iniciar con 2, 5, 7 u 8.</small>
                        </div>
                    </div>

                    <div class="form-grid-layout" style="margin-top: 20px;">
                        <div class="form-section-title">2. Credenciales de Acceso</div>
                        
                         <div>
                            <label class="lbl-std">Rol en el Sistema <span class="required-asterisk">*</span></label>
                            <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-control-std">
                                <asp:ListItem Text="-- Seleccionar --" Value=""></asp:ListItem>
                                <asp:ListItem Text="Administrador" Value="Administrador"></asp:ListItem>
                                <asp:ListItem Text="Vendedor" Value="Vendedor"></asp:ListItem>
                                <asp:ListItem Text="Optometrista" Value="Optometrista"></asp:ListItem>
                                <asp:ListItem Text="Administrador Financiero" Value="Administrador Financiero"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="lbl-std">Nombre de Usuario (Login) <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control-std"></asp:TextBox>
                             <small class="help-text">Único para iniciar sesión.</small>
                        </div>
                        
                        <div>
                            <label class="lbl-std">Contraseña <asp:Label ID="lblPassReq" runat="server" CssClass="required-asterisk" Text="*"></asp:Label></label>
                            <asp:TextBox ID="txtClave" runat="server" CssClass="form-control-std" TextMode="Password"></asp:TextBox>
                            <small class="help-text" id="helpPass" runat="server">Mínimo 6 caracteres.</small>
                        </div>
                         <div>
                            <label class="lbl-std">Confirmar Contraseña</label>
                            <asp:TextBox ID="txtClaveConfirm" runat="server" CssClass="form-control-std" TextMode="Password"></asp:TextBox>
                        </div>
                    </div>

                    <div style="margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: right; display: flex; justify-content: flex-end; gap: 10px;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar Usuario" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>