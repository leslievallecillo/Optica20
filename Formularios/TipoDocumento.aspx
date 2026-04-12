<%@ Page Title="Gestión de Tipos de Documento" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="TipoDocumento.aspx.cs" Inherits="Optica.Formularios.TipoDocumento" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root {
            --color-primary: #0056b3;
            --color-secondary: #6c757d;
            --color-success: #28a745;
            --color-danger: #dc3545;
            --color-border: #ced4da;
            --color-bg: #f4f6f9;
            --color-text: #333;
            --color-info: #17a2b8;
        }
        body { background-color: var(--color-bg); color: var(--color-text); font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; }
        .container-fluid { padding: 20px; max-width: 1200px; margin: 0 auto; }
        .panel-card { background: #fff; border: 1px solid rgba(0,0,0,0.1); border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); margin-bottom: 20px; overflow: hidden; }
        .panel-header { background-color: #fff; padding: 15px 20px; border-bottom: 1px solid var(--color-border); display: flex; justify-content: space-between; align-items: center; }
        .panel-header h3 { margin: 0; font-size: 1.25rem; color: var(--color-primary); font-weight: 700; display: flex; align-items: center; gap: 10px; }
        .panel-body { padding: 20px; }
        .toolbar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; background-color: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid var(--color-border); margin-bottom: 20px; }
        .form-group-filter { display: flex; flex-direction: column; gap: 5px; }
        .form-group-filter label { font-size: 0.85rem; font-weight: 600; color: var(--color-secondary); }
        .form-control-std { width: 100%; padding: 8px 12px; border: 1px solid #ced4da; border-radius: 4px; font-size: 0.95rem; height: 38px; box-sizing: border-box; transition: border-color 0.15s; }
        .form-control-std:focus { border-color: var(--color-primary); outline: 0; box-shadow: 0 0 0 0.2rem rgba(0, 86, 179, 0.25); }
        .lbl-std { display: block; margin-bottom: 5px; font-weight: 600; color: #495057; font-size: 0.9rem; }
        .required-asterisk { color: var(--color-danger); font-weight: bold; margin-left: 3px; }
        .help-text { display: block; font-size: 0.75rem; color: #6c757d; margin-top: 4px; font-style: italic; line-height: 1.2; }
        .error-text { display: block; font-size: 0.8rem; color: var(--color-danger); font-weight: 700; margin-top: 3px; background-color: #fff5f5; padding: 2px 5px; border-radius: 3px; border-left: 3px solid var(--color-danger); }
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-std thead th { background-color: #f8f9fa; color: #495057; font-weight: 700; padding: 12px 15px; text-align: left; border-bottom: 2px solid var(--color-border); }
        .table-std tbody td { padding: 10px 15px; border-bottom: 1px solid var(--color-border); vertical-align: middle; }
        .table-std tbody tr:hover { background-color: #eef2f7; }
        .btn-std { display: inline-flex; align-items: center; justify-content: center; gap: 5px; padding: 8px 16px; border-radius: 4px; border: none; cursor: pointer; font-weight: 500; font-size: 0.9rem; transition: all 0.2s; text-decoration: none; color: white; }
        .btn-primary { background-color: var(--color-primary); }
        .btn-success { background-color: var(--color-success); }
        .btn-danger { background-color: var(--color-danger); }
        .btn-secondary { background-color: var(--color-secondary); }
        .btn-info { background-color: var(--color-info); }
        .btn-sm { padding: 5px 10px; font-size: 0.8rem; }
        .badge-status { padding: 4px 8px; border-radius: 12px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; }
        .badge-active { background-color: #d4edda; color: #155724; }
        .badge-inactive { background-color: #f8d7da; color: #721c24; }
        .table-scroll-container { width: 100%; max-height: 600px; overflow-y: auto; border: 1px solid var(--color-border); border-radius: 4px; }
        .alert-info-custom { background-color: #e3f2fd; color: #0d47a1; padding: 10px; border-radius: 4px; margin-top: 10px; font-size: 0.9rem; text-align: center; border: 1px solid #bbdefb; }
    </style>

    <script type="text/javascript">
        document.addEventListener('keydown', function (event) {
            if (event.key === "Enter") {
                const form = event.target.form;
                const index = Array.prototype.indexOf.call(form, event.target);
                if (event.target.type !== 'submit' && event.target.type !== 'button' && event.target.tagName !== 'TEXTAREA') {
                    event.preventDefault();
                    if (form.elements[index + 1] && (form.elements[index + 1].value === 'Cancelar' || form.elements[index + 1].type === 'button')) {
                        if (form.elements[index + 2]) form.elements[index + 2].focus();
                    } else if (form.elements[index + 1]) {
                        form.elements[index + 1].focus();
                    }
                }
            }
        });

        var guardando = false;
        function confirmarGuardar(sender) {
            if (guardando) return true;
            Swal.fire({
                title: '¿Guardar Registro?',
                text: "Verifique que los datos sean correctos.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, guardar',
                cancelButtonText: 'Cancelar'
            }).then((result) => { if (result.isConfirmed) { guardando = true; sender.click(); } });
            return false;
        }

        function confirmarBaja(sender) {
            Swal.fire({ title: '¿Inactivar?', text: "El tipo de documento pasará a estado inactivo.", icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', cancelButtonColor: '#6c757d', confirmButtonText: 'Sí, inactivar', cancelButtonText: 'Cancelar' }).then((result) => { if (result.isConfirmed) { eval(sender.href); } }); return false;
        }
        function confirmarReactivar(sender) {
            Swal.fire({ title: '¿Reactivar?', text: "El tipo de documento volverá a estar activo.", icon: 'question', showCancelButton: true, confirmButtonColor: '#28a745', cancelButtonColor: '#6c757d', confirmButtonText: 'Sí, reactivar', cancelButtonText: 'Cancelar' }).then((result) => { if (result.isConfirmed) { eval(sender.href); } }); return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">
        <div style="margin-bottom: 20px;">
            <h2 style="font-weight: 300; color: var(--color-text);">Gestión de <b style="font-weight: 700;">Tipos de Documento</b></h2>
        </div>

        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3><i class="fa-solid fa-folder-open"></i> Listado de Tipos de Documento</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="Nuevo Tipo Documento" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    <div class="toolbar">
                        <div class="form-group-filter" style="flex-grow: 1; min-width: 200px;">
                            <label>Buscar Descripción</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-std" placeholder="Ej: DNI, Pasaporte..."></asp:TextBox>
                        </div>
                        <div class="form-group-filter" style="width: 150px;">
                            <label>Estado</label>
                            <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control-std">
                                <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                                <asp:ListItem Text="Activo" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Inactivo" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group-filter" style="width: 160px;">
                            <label>Desde</label>
                            <asp:TextBox ID="txtFiltroFechaInicio" runat="server" TextMode="Date" CssClass="form-control-std"></asp:TextBox>
                        </div>
                        <div class="form-group-filter" style="width: 160px;">
                            <label>Hasta</label>
                            <asp:TextBox ID="txtFiltroFechaFin" runat="server" TextMode="Date" CssClass="form-control-std"></asp:TextBox>
                        </div>
                        <div class="form-group-filter">
                            <label>&nbsp;</label>
                            <div style="display:flex; gap:5px;">
                                <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                                <asp:Button ID="btnMostrarTodo" runat="server" Text="Mostrar Todo" CssClass="btn-std btn-info" OnClick="btnMostrarTodo_Click" />
                            </div>
                        </div>
                    </div>

                    <div style="display:flex; justify-content:space-between; margin-bottom:10px; align-items:center;">
                        <div>
                            Mostrar 
                            <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" 
                                CssClass="form-control-std" Style="width: 80px; display: inline-block; height: 30px; padding: 2px 5px;">
                                <asp:ListItem Value="10">10</asp:ListItem>
                                <asp:ListItem Value="20">20</asp:ListItem>
                                <asp:ListItem Value="50">50</asp:ListItem>
                                <asp:ListItem Value="All">Todos</asp:ListItem>
                            </asp:DropDownList>
                            registros
                        </div>
                    </div>

                    <div class="table-scroll-container" id="divGridContainer" runat="server">
                        <asp:GridView ID="gvTipoDocumento" runat="server" CssClass="table-std" AutoGenerateColumns="False" 
                            OnRowCommand="gvTipoDocumento_RowCommand" DataKeyNames="ID_TipoDocumento" 
                            AllowPaging="True" PageSize="10" OnPageIndexChanging="gvTipoDocumento_PageIndexChanging"
                            GridLines="None" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:BoundField DataField="ID_TipoDocumento" HeaderText="ID" ItemStyle-Width="60px" />
                                <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                                <asp:BoundField DataField="FechaRegistro" HeaderText="Registro" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="120px" />
                                <asp:TemplateField HeaderText="Estado" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                            <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_TipoDocumento") %>' CssClass="btn-std btn-sm btn-primary"><i class="fa-solid fa-pen-to-square"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnBaja" runat="server" CommandName="DarBaja" CommandArgument='<%# Eval("ID_TipoDocumento") %>' CssClass="btn-std btn-sm btn-danger" OnClientClick="return confirmarBaja(this);" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>'><i class="fa-solid fa-ban"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnReactivar" runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("ID_TipoDocumento") %>' CssClass="btn-std btn-sm btn-success" OnClientClick="return confirmarReactivar(this);" Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>'><i class="fa-solid fa-check"></i></asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pagination" HorizontalAlign="Center" BackColor="#f8f9fa" />
                        </asp:GridView>
                        <asp:Panel ID="pnlMensajeGrid" runat="server" Visible="false" CssClass="alert-info-custom">
                            <i class="fa-solid fa-circle-info"></i> <asp:Label ID="lblMensajeGrid" runat="server"></asp:Label>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelMantenimiento" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 600px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Tipo de Documento"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    <asp:HiddenField ID="hfIDTipoDocumento" runat="server" />
                    
                    <div style="display: flex; flex-direction: column; gap: 20px;">
                        <div>
                            <label class="lbl-std">Descripción <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control-std" MaxLength="50"></asp:TextBox>
                            <small class="help-text">Solo letras y un espacio por palabra.</small>
                            <asp:Label ID="errDescripcion" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>

                        <div>
                            <label class="lbl-std">Fecha de Registro</label>
                            <asp:TextBox ID="txtFechaRegistro" runat="server" CssClass="form-control-std" TextMode="Date"></asp:TextBox>
                            <small class="help-text">Automática.</small>
                        </div>
                    </div>

                    <div style="margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: right; display: flex; justify-content: flex-end; gap: 10px;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>