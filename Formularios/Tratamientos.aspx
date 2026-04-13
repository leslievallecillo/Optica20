<%@ Page Title="Gestión de Tratamientos" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Tratamientos.aspx.cs" Inherits="Optica.Formularios.Tratamientos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root { --color-primary: #0056b3; --color-danger: #d32f2f; --color-text: #333; --color-border: #ced4da; --bg-light: #f8f9fa; }
        
        .panel-card { background: #fff; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); margin-bottom: 20px; border: 1px solid #eaeaea; }
        .panel-header { padding: 15px 20px; border-bottom: 1px solid #eaeaea; display: flex; justify-content: space-between; align-items: center; background-color: var(--bg-light); border-radius: 8px 8px 0 0; }
        .panel-body { padding: 25px; }
        
        .toolbar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; background-color: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid var(--color-border); margin-bottom: 20px; }
        
        .form-grid-layout { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }
        .form-group-formal { margin-bottom: 12px; }
        .form-full-width { grid-column: 1 / -1; }
        
        .lbl-formal { font-weight: 700; font-size: 0.9rem; color: #2c3e50; display: block; margin-bottom: 5px; }
        .form-control-formal { width: 100%; padding: 10px 12px; font-size: 1rem; border: 1px solid var(--color-border); border-radius: 4px; box-sizing: border-box; transition: all 0.3s; }
        .form-control-formal:focus { border-color: var(--color-primary); box-shadow: 0 0 0 3px rgba(0, 86, 179, 0.1); outline: none; }
        
        .error-message-formal { color: var(--color-danger); font-size: 0.85rem; margin-top: 3px; display: flex; align-items: center; font-weight: 600; }
        .error-message-formal i { margin-right: 5px; }
        .help-text-formal { font-size: 0.75rem; color: #636c72; margin-top: 3px; font-style: italic; display: block; line-height: 1.2; }
        
        .req { color: #dc3545; font-weight: bold; margin-left: 3px; }

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
        
        .alert-info-custom { background-color: #e3f2fd; color: #0d47a1; padding: 10px; border-radius: 4px; margin-top: 10px; font-size: 0.9rem; text-align: center; border: 1px solid #bbdefb; }

        @media (max-width: 768px) {
            .container-fluid { padding: 10px !important; }
            .panel-header { flex-direction: column; align-items: flex-start; gap: 15px; }
            .toolbar { flex-direction: column; align-items: stretch; }
            .toolbar > div { width: 100% !important; min-width: auto !important; }
            .toolbar .btn-std { width: 100%; justify-content: center; }
            .toolbar > div > div { flex-direction: column; }
        }
    </style>

    <script type="text/javascript">
        function noDobleEspacio(input) {
            if (input.value.startsWith(' ')) input.value = input.value.trimStart();
            input.value = input.value.replace(/  +/g, ' ');
        }

        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                var target = e.target;

                var formPanel = document.getElementById('<%= PanelMantenimiento.ClientID %>');
                if (!formPanel || !formPanel.contains(target)) return;

                if (target.tagName === "TEXTAREA" || (target.tagName === "INPUT" && target.type === "submit")) return true;

                e.preventDefault();

                var elements = Array.from(formPanel.querySelectorAll('input:not([type=hidden]):not([disabled]), select:not([disabled])'));
                var index = elements.indexOf(target);

                if (index > -1) {
                    if (index === elements.length - 1 || target.id.includes("btnGuardar")) {
                        var btn = document.getElementById('<%= btnGuardar.ClientID %>');
                        if (btn) btn.click();
                    } else {
                        elements[index + 1].focus();
                    }
                }
            }
        });

        var guardando = false;
        function confirmarGuardar(sender) {
            if (guardando) return true;
            Swal.fire({
                title: '¿Guardar Tratamiento?', text: "Verifique la información.", icon: 'question',
                showCancelButton: true, confirmButtonColor: '#28a745', cancelButtonColor: '#6c757d', confirmButtonText: 'Sí, guardar'
            }).then((result) => { if (result.isConfirmed) { guardando = true; sender.click(); } });
            return false;
        }

        function confirmarEstado(sender, accion) {
            Swal.fire({
                title: '¿' + accion + '?', text: "Cambiará el estado del registro.", icon: 'warning',
                showCancelButton: true, confirmButtonColor: accion === 'Desactivar' ? '#d33' : '#28a745', confirmButtonText: 'Sí, proceder'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="sm" runat="server"></asp:ScriptManager>

    <div class="container-fluid" style="padding: 20px; box-sizing: border-box;">
        
        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3 style="margin:0; color:var(--color-primary);"><i class="fa-solid fa-wand-magic-sparkles"></i> Catálogo de Tratamientos</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="+ Nuevo Tratamiento" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    
                    <div class="toolbar">
                        <div style="flex-grow: 1; min-width: 200px;">
                            <label class="lbl-formal">Buscar</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-formal" placeholder="Nombre del tratamiento..."></asp:TextBox>
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
                        <asp:GridView ID="gvTratamientos" runat="server" CssClass="table-std" AutoGenerateColumns="False" 
                            DataKeyNames="ID_Tratamiento" AllowPaging="True" PageSize="10" 
                            OnRowCommand="gvTratamientos_RowCommand" OnPageIndexChanging="gvTratamientos_PageIndexChanging" 
                            GridLines="None" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:BoundField DataField="ID_Tratamiento" HeaderText="ID" ItemStyle-Width="50px" />
                                <asp:BoundField DataField="Nombre" HeaderText="Tratamiento" />
                                <asp:BoundField DataField="PrecioAdicional" HeaderText="Precio Adicional" DataFormatString="{0:C}" ItemStyle-Width="150px" />
                                <asp:BoundField DataField="FechaRegistro" HeaderText="Registro" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="120px" />
                                
                                <asp:TemplateField HeaderText="Estado" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                            <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="160px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <div style="display: flex; gap: 5px; justify-content: center;">
                                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Tratamiento") %>' CssClass="btn-std btn-primary" style="padding:4px 8px;"><i class="fa-solid fa-pen-to-square"></i></asp:LinkButton>
                                            
                                            <asp:LinkButton ID="btnDesactivar" runat="server" CommandName="Desactivar" CommandArgument='<%# Eval("ID_Tratamiento") %>' 
                                                CssClass="btn-std btn-danger" style="padding:4px 8px;" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>' 
                                                OnClientClick="return confirmarEstado(this, 'Desactivar');">
                                                <i class="fa-solid fa-ban"></i>
                                            </asp:LinkButton>
                                            
                                            <asp:LinkButton ID="btnReactivar" runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("ID_Tratamiento") %>' 
                                                CssClass="btn-std btn-success" style="padding:4px 8px;" Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>' 
                                                OnClientClick="return confirmarEstado(this, 'Reactivar');">
                                                <i class="fa-solid fa-check"></i>
                                            </asp:LinkButton>
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
            <div class="panel-card" style="max-width: 700px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Registro"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    
                    <asp:HiddenField ID="hfIDTratamiento" runat="server" />

                    <div style="width: 0px; height: 0px; overflow: hidden; position: absolute;">
                        <input type="text" />
                    </div>

                    <div class="form-grid-layout" style="grid-template-columns: 1fr;">
                        
                        <div class="form-group-formal">
                            <label class="lbl-formal">Nombre del Tratamiento <span class="req">*</span></label>
                            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control-formal" autocomplete="off" MaxLength="50" oninput="noDobleEspacio(this)"></asp:TextBox>
                            <small class="help-text-formal">Ej: Antirreflejo, UV 400. Mínimo 2 caracteres por palabra.</small>
                            <asp:Label ID="errNombre" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                        <div class="form-group-formal">
                            <label class="lbl-formal">Precio Adicional (C$) <span class="req">*</span></label>
                            <asp:TextBox ID="txtPrecio" runat="server" CssClass="form-control-formal" TextMode="Number" step="0.01" min="0" placeholder="0.00"></asp:TextBox>
                            <small class="help-text-formal">Valor que se sumará al costo base. No puede ser negativo.</small>
                            <asp:Label ID="errPrecio" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                    </div>

                    <div style="text-align:right; margin-top:20px; border-top:1px solid #eee; padding-top:20px; display: flex; justify-content: flex-end; gap: 10px; flex-wrap: wrap;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" CausesValidation="false" formnovalidate="formnovalidate" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar Datos" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>