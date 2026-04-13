<%@ Page Title="Gestión de Clientes" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Clientes.aspx.cs" Inherits="Optica.Formularios.Clientes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root {
            --color-primary: #0056b3;
            --color-danger: #d32f2f;
            --color-border: #ced4da;
            --bg-light: #f8f9fa;
        }

        .form-group-formal {
            margin-bottom: 25px;
            position: relative;
        }

        .lbl-formal {
            font-weight: 700;
            font-size: 0.9rem;
            color: #2c3e50;
            display: block;
            margin-bottom: 5px;
        }

        .required-mark {
            color: var(--color-danger);
            margin-left: 3px;
        }

        .input-wrapper {
            position: relative;
            width: 100%;
        }

        .form-control-formal {
            width: 100%;
            padding: 10px 35px 10px 12px;
            font-size: 1rem;
            border: 1px solid var(--color-border);
            border-radius: 4px;
            transition: all 0.3s;
            background-color: #fff;
            box-sizing: border-box;
        }

        .form-control-formal:focus {
            border-color: var(--color-primary);
            box-shadow: 0 0 0 3px rgba(0, 86, 179, 0.1);
            outline: none;
        }

        .form-control-formal.is-invalid {
            border-color: var(--color-danger);
            background-color: #fff8f8;
        }

        .input-error-icon {
            position: absolute;
            right: 12px;
            top: 50%;
            transform: translateY(-50%);
            color: var(--color-danger);
            font-size: 1.1rem;
            display: none;
            pointer-events: none;
        }

        .is-invalid + .input-error-icon {
            display: block;
        }

        .error-message-formal {
            color: var(--color-danger);
            font-size: 0.8rem;
            margin-top: 5px;
            display: block;
            font-weight: 600;
        }

        .help-text-formal {
            font-size: 0.75rem;
            color: #636c72;
            margin-top: 4px;
            display: block;
        }

        .panel-card {
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.05);
            margin-bottom: 20px;
            border: 1px solid #eaeaea;
        }

        .panel-header {
            padding: 15px 20px;
            border-bottom: 1px solid #eaeaea;
            display: flex;
            justify-content: space-between;
            align-items: center;
            background-color: var(--bg-light);
            border-radius: 8px 8px 0 0;
        }

        .panel-body { padding: 25px; }

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

        .form-grid-layout {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }

        .form-full-width { grid-column: 1 / -1; }

        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; white-space: nowrap; }
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

        .alert-info-custom {
            background-color: #e3f2fd;
            color: #0d47a1;
            padding: 10px;
            border-radius: 4px;
            margin-top: 10px;
            font-size: 0.9rem;
            text-align: center;
            border: 1px solid #bbdefb;
        }

        @media (max-width: 768px) {
            .container-fluid { padding: 10px !important; }
            .panel-header { flex-direction: column; align-items: flex-start; gap: 15px; }
            .btn-std { width: 100%; justify-content: center; }
            .toolbar { flex-direction: column; align-items: stretch; }
            .toolbar > div { width: 100% !important; min-width: auto !important; }
            .toolbar > div:last-child > div { flex-direction: column; }
            .toolbar > div:last-child > div > .btn-std { margin-bottom: 5px; }
            .form-grid-layout { grid-template-columns: 1fr; }
            .panel-body > div[style*="text-align:right"] { display: flex; flex-direction: column; gap: 10px; }
        }
    </style>

    <script type="text/javascript">
        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                var target = e.target;
                if (target.tagName === "TEXTAREA" || (target.tagName === "INPUT" && target.type === "submit")) {
                    return true;
                }
                e.preventDefault();
                var form = target.form;
                if (form) {
                    var elements = Array.from(form.querySelectorAll('input:not([type=hidden]):not([disabled]), select:not([disabled]), textarea:not([disabled]), input[type=submit]:not([disabled])'));
                    var index = elements.indexOf(target);
                    if (index > -1 && index < elements.length - 1) {
                        var nextElement = elements[index + 1];
                        if (nextElement.value === "Cancelar" || nextElement.classList.contains("btn-secondary")) {
                            if (index + 2 < elements.length) { elements[index + 2].focus(); }
                        } else {
                            nextElement.focus();
                        }
                    }
                }
            }
        });

        var guardando = false;
        function confirmarGuardar(sender) {
            if (guardando) return true;
            Swal.fire({
                title: '¿Desea guardar este registro?',
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
                title: '¿Confirmar Baja?',
                text: "El registro pasará a estado Inactivo.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, dar baja',
                cancelButtonText: 'Cancelar'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }
        function confirmarReactivar(sender) {
            Swal.fire({
                title: '¿Reactivar?',
                text: "El cliente volverá a estar activo.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, reactivar',
                cancelButtonText: 'Cancelar'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">
        
        <div style="margin-bottom: 20px;">
            <h2 style="font-weight: 300; color: var(--color-text);">Gestión de <b style="font-weight: 700;">Clientes</b></h2>
        </div>

        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3 style="margin:0; color:var(--color-primary);"><i class="fa-solid fa-users"></i> Gestión de Clientes</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="+ Nuevo Cliente" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    
                    <div class="toolbar">
                        <div style="flex-grow: 1; min-width: 200px;">
                            <label class="lbl-formal">Búsqueda</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-formal" placeholder="Nombre, Cédula, RUC..."></asp:TextBox>
                        </div>
                        <div style="width: 150px;">
                            <label class="lbl-formal">Tipo</label>
                            <asp:DropDownList ID="ddlFiltroTipo" runat="server" CssClass="form-control-formal">
                                <asp:ListItem Text="Todos" Value=""></asp:ListItem>
                                <asp:ListItem Text="Natural" Value="Natural"></asp:ListItem>
                                <asp:ListItem Text="Juridico" Value="Juridico"></asp:ListItem>
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
                        <div style="width: 160px;">
                            <label class="lbl-formal">Desde</label>
                            <asp:TextBox ID="txtFiltroFechaInicio" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div style="width: 160px;">
                            <label class="lbl-formal">Hasta</label>
                            <asp:TextBox ID="txtFiltroFechaFin" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div>
                            <label class="lbl-formal">&nbsp;</label>
                            <div style="display: flex; gap: 10px;">
                                <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                                <asp:Button ID="btnMostrarTodo" runat="server" Text="Mostrar Todo" CssClass="btn-std btn-info" OnClick="btnMostrarTodo_Click" />
                            </div>
                        </div>
                    </div>

                    <div style="display:flex; justify-content:space-between; margin-bottom:10px; align-items:center;">
                        <div>
                            Mostrar 
                            <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" 
                                style="padding:5px; border-radius:4px; border:1px solid #ccc;">
                                <asp:ListItem Value="10">10</asp:ListItem>
                                <asp:ListItem Value="20">20</asp:ListItem>
                                <asp:ListItem Value="50">50</asp:ListItem>
                                <asp:ListItem Value="All">Todos</asp:ListItem>
                            </asp:DropDownList>
                            registros
                        </div>
                    </div>

                    <div class="table-responsive">
                        <asp:GridView ID="gvClientes" runat="server" CssClass="table-std" AutoGenerateColumns="False" 
                            DataKeyNames="ID_Cliente" AllowPaging="True" PageSize="10" 
                            OnRowCommand="gvClientes_RowCommand" OnPageIndexChanging="gvClientes_PageIndexChanging" GridLines="None" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:BoundField DataField="ID_Cliente" HeaderText="ID" ItemStyle-Width="50px" />
                                <asp:BoundField DataField="NombreCompleto" HeaderText="Cliente / Razón Social" HtmlEncode="false" />
                                <asp:BoundField DataField="TipoCliente" HeaderText="Tipo" />
                                <asp:BoundField DataField="Identificacion" HeaderText="Cédula / RUC" />
                                <asp:BoundField DataField="Telefono" HeaderText="Teléfono" />
                                <asp:BoundField DataField="Correo" HeaderText="Email" />
                                <asp:BoundField DataField="FechaRegistro" HeaderText="Registro" DataFormatString="{0:dd/MM/yyyy}" />
                                <asp:TemplateField HeaderText="Estado">
                                    <ItemTemplate>
                                        <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                            <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Acciones">
                                    <ItemTemplate>
                                        <div style="display: flex; gap: 5px; justify-content: center;">
                                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Cliente") %>' CssClass="btn-std btn-primary" style="padding:4px 8px;"><i class="fa-solid fa-pen"></i></asp:LinkButton>
                                            <asp:LinkButton ID="btnBaja" runat="server" CommandName="DarBaja" CommandArgument='<%# Eval("ID_Cliente") %>' CssClass="btn-std btn-danger" style="padding:4px 8px;" OnClientClick="return confirmarBaja(this);" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>'><i class="fa-solid fa-ban"></i></asp:LinkButton>
                                            <asp:LinkButton ID="btnReactivar" runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("ID_Cliente") %>' CssClass="btn-std btn-success" style="padding:4px 8px;" OnClientClick="return confirmarReactivar(this);" Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>'><i class="fa-solid fa-check"></i></asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <asp:Panel ID="pnlMensajeGrid" runat="server" Visible="false" CssClass="alert-info-custom">
                        <i class="fa-solid fa-circle-info"></i> <asp:Label ID="lblMensajeGrid" runat="server"></asp:Label>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelMantenimiento" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 900px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Registro"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    <asp:HiddenField ID="hfIDCliente" runat="server" />

                    <div class="form-group-formal">
                        <label class="lbl-formal">Tipo de Cliente <span class="required-mark">*</span></label>
                        <div class="input-wrapper">
                            <asp:DropDownList ID="ddlTipoCliente" runat="server" CssClass="form-control-formal" AutoPostBack="true" OnSelectedIndexChanged="ddlTipoCliente_SelectedIndexChanged">
                                <asp:ListItem Text="-- Seleccione --" Value=""></asp:ListItem>
                                <asp:ListItem Text="Persona Natural" Value="Natural"></asp:ListItem>
                                <asp:ListItem Text="Persona Jurídica (Empresa)" Value="Juridico"></asp:ListItem>
                            </asp:DropDownList>
                            <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                        </div>
                        <small class="help-text-formal">Seleccione categoría.</small>
                    </div>

                    <asp:Panel ID="pnlNatural" runat="server" Visible="false" CssClass="form-grid-layout">
                        <div class="form-group-formal">
                            <label class="lbl-formal">Nombres <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control-formal" MaxLength="50"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Solo letras admitidas.</small>
                            <asp:Label ID="errNombre" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Apellidos <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control-formal" MaxLength="50"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Solo letras admitidas.</small>
                            <asp:Label ID="errApellido" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal form-full-width">
                            <label class="lbl-formal">Cédula de Identidad <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtCedula" runat="server" CssClass="form-control-formal" MaxLength="16" placeholder="001-010190-0000A"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Formato: 000-000000-0000L.</small>
                            <asp:Label ID="errCedula" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlJuridico" runat="server" Visible="false" CssClass="form-grid-layout">
                        <div class="form-group-formal form-full-width">
                            <label class="lbl-formal">Razón Social <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtNombreEmpresa" runat="server" CssClass="form-control-formal" MaxLength="100"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Nombre comercial.</small>
                            <asp:Label ID="errEmpresa" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Número RUC <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtRUC" runat="server" CssClass="form-control-formal" MaxLength="14" 
                                    placeholder="0011505200001T" 
                                    oninput="this.value = this.value.toUpperCase().replace(/[^0-9A-Z]/g, '');"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">13 números y 1 letra.</small>
                            <asp:Label ID="errRUC" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Giro de Negocio <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtGiro" runat="server" CssClass="form-control-formal" MaxLength="100"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Breve descripción.</small>
                            <asp:Label ID="errGiro" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal form-full-width">
                            <label class="lbl-formal">Representante Legal <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtRepresentante" runat="server" CssClass="form-control-formal" MaxLength="100"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Solo letras.</small>
                            <asp:Label ID="errRepresentante" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                         <div class="form-group-formal">
                             <label class="lbl-formal">Descuento Corporativo (%)</label>
                             <div class="input-wrapper">
                                <asp:TextBox ID="txtDescuento" runat="server" CssClass="form-control-formal" TextMode="Number" Step="0.01" MaxLength="5"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                             </div>
                             <small class="help-text-formal">Opcional.</small>
                         </div>
                         <div style="padding-top:30px;">
                             <asp:CheckBox ID="chkEsAfiliada" runat="server" Text=" Es Afiliada" Font-Bold="true" />
                         </div>
                    </asp:Panel>

                    <div style="margin-top:20px; border-top:1px solid #eee; padding-top:20px;" class="form-grid-layout">
                        <div class="form-group-formal form-full-width">
                            <label class="lbl-formal">Dirección Domiciliar <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control-formal" TextMode="MultiLine" Rows="2" MaxLength="200"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Solo letras y espacios.</small>
                            <asp:Label ID="errDireccion" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Teléfono <span class="required-mark">*</span></label>
                            <div class="input-wrapper" style="display:flex;">
                                <span style="padding:10px; background:#eee; border:1px solid #ced4da; border-right:none; border-radius:4px 0 0 4px;">505</span>
                                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control-formal" MaxLength="8" style="border-radius:0 4px 4px 0;"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon" style="right:40px;"></i>
                            </div>
                            <small class="help-text-formal">8 dígitos.</small>
                            <asp:Label ID="errTelefono" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Correo Electrónico <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control-formal" TextMode="Email" MaxLength="100"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Formato válido.</small>
                            <asp:Label ID="errCorreo" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>
                        <div class="form-group-formal">
                            <label class="lbl-formal">Fecha Registro</label>
                            <asp:TextBox ID="txtFechaRegistro" runat="server" CssClass="form-control-formal" TextMode="Date" Enabled="false"></asp:TextBox>
                        </div>
                    </div>

                    <div style="text-align:right; margin-top:20px; display: flex; gap: 10px; justify-content: flex-end; flex-wrap: wrap;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar Datos" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>