<%@ Page Title="Todos los Reclamos" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Reclamos.aspx.cs" Inherits="Optica.Formularios.Reclamos" %>

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
            --color-warning: #ffc107;
        }
        body { background-color: var(--color-bg); color: var(--color-text); font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; }
        .container-fluid { padding: 20px; max-width: 1600px; margin: 0 auto; box-sizing: border-box; }

        .panel-card { background: #fff; border: 1px solid rgba(0,0,0,0.1); border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); margin-bottom: 20px; overflow: hidden; }
        .panel-header { background-color: #fff; padding: 15px 20px; border-bottom: 1px solid var(--color-border); display: flex; justify-content: space-between; align-items: center; }
        .panel-header h3 { margin: 0; font-size: 1.25rem; color: var(--color-primary); font-weight: 700; display: flex; align-items: center; gap: 10px; }
        .panel-body { padding: 20px; }
        
        .toolbar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; background-color: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid var(--color-border); margin-bottom: 20px; }
        .form-group-filter { display: flex; flex-direction: column; gap: 5px; }
        .form-group-filter label { font-size: 0.85rem; font-weight: 600; color: var(--color-secondary); }
        
        .form-grid-layout { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 25px; }
        .form-full-width { grid-column: 1 / -1; }
        
        .form-control-std { width: 100%; padding: 8px 12px; border: 1px solid #ced4da; border-radius: 4px; font-size: 0.95rem; height: 38px; box-sizing: border-box; transition: border-color 0.15s; }
        textarea.form-control-std { height: auto; }
        .form-control-std:focus { border-color: var(--color-primary); outline: 0; box-shadow: 0 0 0 0.2rem rgba(0, 86, 179, 0.25); }
        
        .lbl-std { display: block; margin-bottom: 5px; font-weight: 600; color: #495057; font-size: 0.9rem; }
        .required-asterisk { color: var(--color-danger); font-weight: bold; margin-left: 3px; }
        .help-text { display: block; font-size: 0.75rem; color: #6c757d; margin-top: 4px; font-style: italic; line-height: 1.2; }
        .error-text { display: block; font-size: 0.8rem; color: var(--color-danger); font-weight: 700; margin-top: 3px; background-color: #fff5f5; padding: 2px 5px; border-radius: 3px; border-left: 3px solid var(--color-danger); }
        
        .table-scroll-container { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; white-space: nowrap; }
        .table-std thead th { background-color: #f8f9fa; color: #495057; font-weight: 700; padding: 12px 15px; text-align: left; border-bottom: 2px solid var(--color-border); position: sticky; top: 0; z-index: 1; }
        .table-std tbody td { padding: 10px 15px; border-bottom: 1px solid var(--color-border); vertical-align: middle; }
        .table-std tbody tr:hover { background-color: #eef2f7; }
        
        .btn-std { display: inline-flex; align-items: center; justify-content: center; gap: 5px; padding: 8px 16px; border-radius: 4px; border: none; cursor: pointer; font-weight: 500; font-size: 0.9rem; transition: all 0.2s; text-decoration: none; color: white; }
        .btn-primary { background-color: var(--color-primary); }
        .btn-success { background-color: var(--color-success); }
        .btn-danger { background-color: var(--color-danger); }
        .btn-secondary { background-color: var(--color-secondary); }
        .btn-info { background-color: var(--color-info); }
        .btn-warning { background-color: var(--color-warning); color: #333; }
        .btn-sm { padding: 4px 8px; font-size: 0.8rem; }

        .badge { padding: 5px 10px; border-radius: 12px; font-size: 0.75rem; font-weight: bold; text-transform: uppercase; }
        .bg-activa { background-color: #d4edda; color: #155724; }
        .bg-vencida { background-color: #fff3cd; color: #856404; }
        .bg-proceso { background-color: #cce5ff; color: #004085; }
        .bg-anulada { background-color: #f8d7da; color: #721c24; }

        .alert-info-custom { background-color: #e3f2fd; color: #0d47a1; padding: 10px; border-radius: 4px; margin-top: 10px; font-size: 0.9rem; text-align: center; border: 1px solid #bbdefb; }

        .info-box { background: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid #e9ecef; margin-bottom: 20px; }
        .info-box strong { color: var(--color-primary); }

        @media (max-width: 768px) {
            .container-fluid { padding: 10px; }
            .panel-header { flex-direction: column; align-items: flex-start; gap: 15px; }
            .panel-header > div { display: flex; flex-direction: column; width: 100%; gap: 10px; }
            .btn-std { width: 100%; justify-content: center; }
            .toolbar { flex-direction: column; align-items: stretch; }
            .form-group-filter { width: 100% !important; min-width: auto !important; }
            .form-group-filter > div { display: flex; flex-direction: column; gap: 5px; }
            .form-grid-layout { grid-template-columns: 1fr; }
            .panel-body > div[style*="text-align: right"] { display: flex; flex-direction: column; gap: 10px; }
        }
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
                title: '¿Guardar Resolución?',
                text: "Verifique que los datos de la solución sean correctos.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, guardar',
                cancelButtonText: 'Cancelar'
            }).then((result) => { if (result.isConfirmed) { guardando = true; sender.click(); } });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">
        
        <div style="margin-bottom: 20px;">
            <h2 style="font-weight: 300; color: var(--color-text);">Control General de <b style="font-weight: 700;">Reclamos</b></h2>
        </div>

        <asp:Panel ID="pnlListadoReclamos" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3><i class="fa-solid fa-triangle-exclamation"></i> Listado Completo de Reclamos</h3>
                </div>

                <div class="panel-body">
                    <div class="toolbar">
                        <div class="form-group-filter" style="flex-grow: 1; min-width: 200px;">
                            <label>Buscar</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-std" placeholder="Motivo, Responsable, Factura..."></asp:TextBox>
                        </div>
                        <div class="form-group-filter" style="width: 150px;">
                             <label>Estado</label>
                             <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control-std">
                                 <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                                 <asp:ListItem Text="Pendiente" Value="Pendiente"></asp:ListItem>
                                 <asp:ListItem Text="En Proceso" Value="En Proceso"></asp:ListItem>
                                 <asp:ListItem Text="Resuelto" Value="Resuelto"></asp:ListItem>
                                 <asp:ListItem Text="Rechazado" Value="Rechazado"></asp:ListItem>
                             </asp:DropDownList>
                        </div>
                        <div class="form-group-filter" style="width: 160px;">
                            <label>Desde (Fecha Reclamo)</label>
                            <asp:TextBox ID="txtFiltroFechaInicio" runat="server" TextMode="Date" CssClass="form-control-std"></asp:TextBox>
                        </div>
                        <div class="form-group-filter" style="width: 160px;">
                            <label>Hasta</label>
                            <asp:TextBox ID="txtFiltroFechaFin" runat="server" TextMode="Date" CssClass="form-control-std"></asp:TextBox>
                        </div>
                        <div class="form-group-filter">
                            <label>&nbsp;</label>
                            <div style="display:flex; gap:5px;">
                                <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnFiltrar_Click" />
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

                    <div class="table-scroll-container">
                        <asp:GridView ID="gvReclamos" runat="server" CssClass="table-std" AutoGenerateColumns="False"
                            DataKeyNames="ID_Reclamo" AllowPaging="True" PageSize="10" 
                            OnRowCommand="gvReclamos_RowCommand" OnPageIndexChanging="gvReclamos_PageIndexChanging"
                            GridLines="None" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:BoundField DataField="ID_Reclamo" HeaderText="ID" ItemStyle-Width="50px" />
                                <asp:BoundField DataField="RefGarantia" HeaderText="Referencia Garantía" />
                                <asp:BoundField DataField="FechaReclamo" HeaderText="Fecha Reclamo" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="100px" />
                                <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
                                <asp:BoundField DataField="Responsable" HeaderText="Responsable" />
                                <asp:BoundField DataField="SolucionAplicada" HeaderText="Solución" />
                                <asp:BoundField DataField="FechaSolucion" HeaderText="Fecha Solución" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="100px" />
                                <asp:TemplateField HeaderText="Estado" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='badge <%# ObtenerClaseEstadoReclamo(Eval("EstadoReclamo").ToString()) %>'>
                                            <%# Eval("EstadoReclamo") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Acciones" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="100px">
                                    <ItemTemplate>
                                        <div style="display: flex; gap: 5px; justify-content: center;">
                                            <asp:LinkButton ID="btnResolverR" runat="server" CommandName="ResolverR" CommandArgument='<%# Eval("ID_Reclamo") %>' CssClass="btn-std btn-sm btn-primary" ToolTip="Ver/Resolver Reclamo"><i class="fa-solid fa-clipboard-check"></i> Resolver</asp:LinkButton>
                                        </div>
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

        <asp:Panel ID="pnlMantReclamo" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 900px; margin: 0 auto; border-left: 5px solid #ffc107;">
                <div class="panel-header">
                    <h3>Resolución de Reclamo</h3>
                </div>
                <asp:HiddenField ID="hfIDReclamo" runat="server" />
                
                <div class="panel-body">
                    
                    <div class="info-box">
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
                            <div>
                                <strong>Referencia:</strong> <asp:Label ID="lblGarantiaRef" runat="server"></asp:Label><br/>
                                <strong>Fecha de Reclamo:</strong> <asp:Label ID="lblFechaReclamo" runat="server"></asp:Label>
                            </div>
                            <div>
                                <strong>Responsable Inicial:</strong> <asp:Label ID="lblResponsable" runat="server"></asp:Label>
                            </div>
                            <div style="grid-column: 1 / -1;">
                                <strong>Motivo del Reclamo:</strong> <asp:Label ID="lblMotivo" runat="server"></asp:Label>
                            </div>
                        </div>
                    </div>

                    <div class="form-grid-layout">
                        <div class="form-full-width">
                            <label class="lbl-std">Solución Aplicada</label>
                            <asp:TextBox ID="txtSolucion" runat="server" CssClass="form-control-std" TextMode="MultiLine" Rows="3"></asp:TextBox>
                            <small class="help-text">Describa las acciones tomadas. Obligatorio si el estado es 'Resuelto'.</small>
                            <asp:Label ID="errSolucion" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>

                        <div>
                            <label class="lbl-std">Estado del Reclamo <span class="required-asterisk">*</span></label>
                            <asp:DropDownList ID="ddlEstadoReclamo" runat="server" CssClass="form-control-std">
                                <asp:ListItem Text="Pendiente" Value="Pendiente"></asp:ListItem>
                                <asp:ListItem Text="En Proceso" Value="En Proceso"></asp:ListItem>
                                <asp:ListItem Text="Resuelto" Value="Resuelto"></asp:ListItem>
                                <asp:ListItem Text="Rechazado" Value="Rechazado"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div>
                            <label class="lbl-std">Fecha Solución</label>
                            <asp:TextBox ID="txtFechaSolucion" runat="server" TextMode="Date" CssClass="form-control-std"></asp:TextBox>
                            <small class="help-text">Máximo 1 semana de plazo desde el reclamo.</small>
                            <asp:Label ID="errFechaSolucion" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>
                    </div>

                    <div style="text-align: right; margin-top: 20px; border-top: 1px solid #eee; padding-top: 20px;">
                        <asp:Button ID="btnCancelarR" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelarR_Click" CausesValidation="false" />
                        <asp:Button ID="btnGuardarR" runat="server" Text="Guardar Resolución" CssClass="btn-std btn-success" OnClick="btnGuardarR_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>