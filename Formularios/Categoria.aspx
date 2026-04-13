<%@ Page Title="Gestión de Categorías" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Categoria.aspx.cs" Inherits="Optica.Formularios.Categoria" %>

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

        /* Contenedor principal responsivo */
        .container-fluid {
            padding: 20px;
            width: 100%;
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

        .panel-body { 
            padding: 25px; 
        }

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

        .toolbar > div {
            flex: 1 1 150px;
            min-width: 0;
        }

        .table-responsive {
            overflow-x: auto;
            -webkit-overflow-scrolling: touch;
            margin-bottom: 15px;
        }

        .table-std { 
            width: 100%; 
            border-collapse: collapse; 
            font-size: 0.9rem; 
            min-width: 600px;
        }
        
        .table-std thead th { 
            background-color: #f1f3f5; 
            padding: 12px; 
            text-align: left; 
            border-bottom: 2px solid #ddd; 
            white-space: nowrap;
        }
        
        .table-std tbody td { 
            padding: 10px; 
            border-bottom: 1px solid #eee; 
        }
        
        .badge-status { 
            padding: 4px 10px; 
            border-radius: 20px; 
            font-size: 0.75rem; 
            font-weight: bold; 
            text-transform: uppercase; 
            white-space: nowrap;
        }
        
        .badge-active { 
            background-color: #e8f5e9; 
            color: #2e7d32; 
        }
        
        .badge-inactive { 
            background-color: #ffebee; 
            color: #c62828; 
        }

        .btn-std { 
            padding: 8px 16px; 
            border: none; 
            border-radius: 4px; 
            cursor: pointer; 
            text-decoration: none; 
            display: inline-flex; 
            align-items: center; 
            gap: 5px; 
            font-size: 0.9rem; 
            transition: 0.2s; 
            white-space: nowrap;
        }
        
        .btn-primary { 
            background-color: var(--color-primary); 
            color: white; 
        }
        
        .btn-success { 
            background-color: #28a745; 
            color: white; 
        }
        
        .btn-danger { 
            background-color: var(--color-danger); 
            color: white; 
        }
        
        .btn-secondary { 
            background-color: #6c757d; 
            color: white; 
        }
        
        .btn-info { 
            background-color: #17a2b8; 
            color: white; 
        }
        
        .btn-sm { 
            padding: 4px 8px; 
            font-size: 0.85rem; 
        }

        .btn-std:hover {
            opacity: 0.9;
            transform: translateY(-1px);
        }

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

        .pagination-controls {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 10px;
            flex-wrap: wrap;
            gap: 10px;
        }

        .page-size-selector {
            display: flex;
            align-items: center;
            gap: 10px;
            flex-wrap: wrap;
        }

        .action-buttons {
            display: flex;
            gap: 5px;
            flex-wrap: wrap;
        }

        /* Responsive */
        @media (max-width: 992px) {
            .container-fluid {
                padding: 15px;
            }
            
            .panel-body {
                padding: 20px;
            }
            
            .toolbar > div {
                flex: 1 1 120px;
            }
        }

        @media (max-width: 768px) {
            .container-fluid {
                padding: 10px;
            }
            
            .panel-header {
                padding: 12px 15px;
                flex-direction: column;
                gap: 10px;
                text-align: center;
            }
            
            .panel-header h3 {
                font-size: 1.2rem;
            }
            
            .panel-body {
                padding: 15px;
            }
            
            .toolbar {
                flex-direction: column;
                align-items: stretch;
                gap: 10px;
            }
            
            .toolbar > div {
                width: 100%;
            }
            
            .toolbar label {
                margin-bottom: 5px;
            }
            
            .toolbar > div:last-child > div {
                flex-direction: column;
                width: 100%;
            }
            
            .toolbar .btn-std {
                width: 100%;
                justify-content: center;
            }
            
            .pagination-controls {
                flex-direction: column;
                align-items: stretch;
            }
            
            .page-size-selector {
                justify-content: space-between;
            }
            
            .action-buttons {
                justify-content: center;
            }
            
            .btn-sm {
                padding: 6px 10px;
            }
            
            .form-group-formal {
                margin-bottom: 20px;
            }
            
            .form-control-formal {
                font-size: 16px; /* Previene zoom en iOS */
            }
        }

        @media (max-width: 480px) {
            .panel-header h3 {
                font-size: 1.1rem;
            }
            
            .btn-std {
                padding: 10px 12px;
                font-size: 0.85rem;
            }
            
            .badge-status {
                padding: 3px 8px;
                font-size: 0.7rem;
            }
            
            .table-std {
                font-size: 0.85rem;
            }
            
            .table-std thead th,
            .table-std tbody td {
                padding: 8px;
            }
            
            .alert-info-custom {
                font-size: 0.85rem;
                padding: 8px;
            }
        }

        /* Mejoras para touch */
        @media (hover: none) and (pointer: coarse) {
            .btn-std,
            .form-control-formal,
            select {
                min-height: 44px;
            }
            
            .btn-sm {
                min-height: 36px;
            }
        }

        /* Ajustes para el grid en pantallas pequeñas */
        @media (max-width: 640px) {
            .table-responsive {
                margin: 0 -15px;
                padding: 0 15px;
            }
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
                    var elements = Array.from(form.querySelectorAll('input:not([type=hidden]):not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), input[type=submit]:not([disabled])'));
                    var index = elements.indexOf(target);

                    if (index > -1 && index < elements.length - 1) {
                        var nextElement = elements[index + 1];
                        if (nextElement.value === "Cancelar" || nextElement.classList.contains("btn-secondary")) {
                            if (index + 2 < elements.length) {
                                elements[index + 2].focus();
                            }
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
                title: '¿Inactivar Categoría?',
                text: "El registro pasará a estado Inactivo.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, inactivar',
                cancelButtonText: 'Cancelar'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }

        function confirmarReactivar(sender) {
            Swal.fire({
                title: '¿Reactivar?',
                text: "La categoría volverá a estar activa.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, reactivar',
                cancelButtonText: 'Cancelar'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }

        // Ajustar título de página dinámicamente
        document.addEventListener('DOMContentLoaded', function () {
            var pageTitle = document.getElementById('pageTitle');
            if (pageTitle) {
                pageTitle.textContent = 'Gestión de Categorías';
            }
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">
        
        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3 style="margin:0; color:var(--color-primary);"><i class="fa-solid fa-tags"></i> Listado de Categorías</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="+ Nueva Categoría" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    
                    <div class="toolbar">
                        <div style="min-width: 180px;">
                            <label class="lbl-formal">Búsqueda</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-formal" placeholder="Descripción..."></asp:TextBox>
                        </div>
                        <div style="min-width: 130px;">
                            <label class="lbl-formal">Estado</label>
                            <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control-formal">
                                <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                                <asp:ListItem Text="Activo" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Inactivo" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div style="min-width: 140px;">
                            <label class="lbl-formal">Desde</label>
                            <asp:TextBox ID="txtFiltroFechaInicio" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div style="min-width: 140px;">
                            <label class="lbl-formal">Hasta</label>
                            <asp:TextBox ID="txtFiltroFechaFin" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div>
                            <label class="lbl-formal">&nbsp;</label>
                            <div style="display: flex; gap: 10px; flex-wrap: wrap;">
                                <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                                <asp:Button ID="btnMostrarTodo" runat="server" Text="Mostrar Todo" CssClass="btn-std btn-info" OnClick="btnMostrarTodo_Click" />
                            </div>
                        </div>
                    </div>

                    <div class="pagination-controls">
                        <div class="page-size-selector">
                            <span>Mostrar</span>
                            <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" 
                                style="padding:5px; border-radius:4px; border:1px solid #ccc; width:auto; min-width:70px;">
                                <asp:ListItem Value="10">10</asp:ListItem>
                                <asp:ListItem Value="20">20</asp:ListItem>
                                <asp:ListItem Value="50">50</asp:ListItem>
                                <asp:ListItem Value="All">Todos</asp:ListItem>
                            </asp:DropDownList>
                            <span>registros</span>
                        </div>
                    </div>

                    <div class="table-responsive">
                        <asp:GridView ID="gvCategorias" runat="server" CssClass="table-std" AutoGenerateColumns="False" 
                            DataKeyNames="ID_Categoria" AllowPaging="True" PageSize="10" 
                            OnRowCommand="gvCategorias_RowCommand" OnPageIndexChanging="gvCategorias_PageIndexChanging" GridLines="None" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:BoundField DataField="ID_Categoria" HeaderText="ID" ItemStyle-Width="60px" />
                                <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                                <asp:BoundField DataField="FechaRegistro" HeaderText="Registro" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="120px" />
                                <asp:TemplateField HeaderText="Estado" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                            <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="130px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <div class="action-buttons">
                                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Categoria") %>' CssClass="btn-std btn-sm btn-primary" ToolTip="Editar"><i class="fa-solid fa-pen-to-square"></i></asp:LinkButton>
                                            <asp:LinkButton ID="btnBaja" runat="server" CommandName="DarBaja" CommandArgument='<%# Eval("ID_Categoria") %>' CssClass="btn-std btn-sm btn-danger" OnClientClick="return confirmarBaja(this);" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>' ToolTip="Inactivar"><i class="fa-solid fa-ban"></i></asp:LinkButton>
                                            <asp:LinkButton ID="btnReactivar" runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("ID_Categoria") %>' CssClass="btn-std btn-sm btn-success" OnClientClick="return confirmarReactivar(this);" Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>' ToolTip="Reactivar"><i class="fa-solid fa-check"></i></asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <tr>
                                    <td colspan="5" style="text-align:center; padding:30px;">
                                        <i class="fa-solid fa-tags" style="font-size:2rem; color:#ccc; margin-bottom:10px;"></i>
                                        <p>No se encontraron categorías</p>
                                    </td>
                                </tr>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>

                    <asp:Panel ID="pnlMensajeGrid" runat="server" Visible="false" CssClass="alert-info-custom">
                        <i class="fa-solid fa-circle-info"></i> <asp:Label ID="lblMensajeGrid" runat="server"></asp:Label>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelMantenimiento" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 600px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Registro"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    <asp:HiddenField ID="hfIDCategoria" runat="server" />
                    
                    <div class="form-group-formal">
                        <label class="lbl-formal">Descripción <span class="required-mark">*</span></label>
                        <div class="input-wrapper">
                            <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control-formal" MaxLength="50"></asp:TextBox>
                            <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                        </div>
                        <small class="help-text-formal">Solo letras admitidas.</small>
                        <asp:Label ID="errDescripcion" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                    </div>

                    <div class="form-group-formal">
                        <label class="lbl-formal">Fecha de Registro</label>
                        <asp:TextBox ID="txtFechaRegistro" runat="server" CssClass="form-control-formal" TextMode="Date" Enabled="false"></asp:TextBox>
                    </div>

                    <div style="text-align:right; margin-top:20px; display:flex; gap:10px; justify-content:flex-end; flex-wrap:wrap;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>