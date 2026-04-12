<%@ Page Title="Gestión de Garantías y Reclamos" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Garantias.aspx.cs" Inherits="Optica.Formularios.Garantias" %>

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
        .container-fluid { padding: 20px; max-width: 1600px; margin: 0 auto; }

        /* ESTILOS FORMALES */
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
        
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-std thead th { background-color: #f8f9fa; color: #495057; font-weight: 700; padding: 12px 15px; text-align: left; border-bottom: 2px solid var(--color-border); position: sticky; top: 0; z-index: 1; }
        .table-std tbody td { padding: 10px 15px; border-bottom: 1px solid var(--color-border); vertical-align: middle; }
        .table-std tbody tr:hover { background-color: #eef2f7; }
        
        .btn-std { display: inline-flex; align-items: center; justify-content: center; gap: 5px; padding: 8px 16px; border-radius: 4px; border: none; cursor: pointer; font-weight: 500; font-size: 0.9rem; transition: all 0.2s; text-decoration: none; color: white; }
        .btn-primary { background-color: var(--color-primary); }
        .btn-success { background-color: var(--color-success); }
        .btn-danger { background-color: var(--color-danger); }
        .btn-secondary { background-color: var(--color-secondary); }
        .btn-info { background-color: var(--color-info); }
        .btn-sm { padding: 4px 8px; font-size: 0.8rem; }

        .badge { padding: 4px 8px; border-radius: 10px; font-size: 0.75rem; font-weight: bold; text-transform: uppercase; }
        .bg-activa { background-color: #d4edda; color: #155724; }
        .bg-vencida { background-color: #fff3cd; color: #856404; }
        .bg-anulada { background-color: #f8d7da; color: #721c24; }

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
            Swal.fire({
                title: '¿Dar de Baja?',
                text: "El registro pasará a estado ANULADA/INACTIVO.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                confirmButtonText: 'Sí, anular',
                cancelButtonText: 'Cancelar'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">
        
        <div style="margin-bottom: 20px;">
            <h2 style="font-weight: 300; color: var(--color-text);">Gestión de <b style="font-weight: 700;">Garantías y Reclamos</b></h2>
        </div>

        <asp:Panel ID="pnlListadoGarantias" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3><i class="fa-solid fa-shield-halved"></i> Listado de Garantías</h3>
                    <asp:Button ID="btnNuevaGarantia" runat="server" Text="Nueva Garantía" CssClass="btn-std btn-success" OnClick="btnNuevaGarantia_Click" />
                </div>

                <div class="panel-body">
                    <div class="toolbar">
                        <div class="form-group-filter" style="flex-grow: 1; min-width: 200px;">
                            <label>Buscar</label>
                            <asp:TextBox ID="txtBuscarGarantia" runat="server" CssClass="form-control-std" placeholder="Cliente, Producto..."></asp:TextBox>
                        </div>
                        <div class="form-group-filter" style="width: 150px;">
                             <label>Estado</label>
                             <asp:DropDownList ID="ddlFiltroEstadoG" runat="server" CssClass="form-control-std">
                                 <asp:ListItem Text="Todas" Value="-1"></asp:ListItem>
                                 <asp:ListItem Text="Activa" Value="Activa"></asp:ListItem>
                                 <asp:ListItem Text="Vencida" Value="Vencida"></asp:ListItem>
                                 <asp:ListItem Text="Anulada" Value="Anulada"></asp:ListItem>
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
                                <asp:Button ID="btnFiltrarGarantia" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnFiltrarGarantia_Click" />
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

                    <div class="table-scroll-container" runat="server" id="divGridGarantias">
                        <asp:GridView ID="gvGarantias" runat="server" CssClass="table-std" AutoGenerateColumns="False"
                            DataKeyNames="ID_Garantia" AllowPaging="True" PageSize="10" 
                            OnRowCommand="gvGarantias_RowCommand" OnPageIndexChanging="gvGarantias_PageIndexChanging"
                            GridLines="None" ShowHeaderWhenEmpty="true">
                            <Columns>
                                <asp:BoundField DataField="ID_Garantia" HeaderText="ID" ItemStyle-Width="50px" />
                                <asp:BoundField DataField="DescripcionProducto" HeaderText="Cliente - Producto/Lente" />
                                <asp:BoundField DataField="ResponsableEntrega" HeaderText="Entregado Por" />
                                <asp:BoundField DataField="FechaRegistro" HeaderText="Registro" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="100px" />
                                <asp:BoundField DataField="FechaFin" HeaderText="Vence" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="100px" />
                                <asp:TemplateField HeaderText="Estado" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='badge <%# ObtenerClaseEstado(Eval("Estado").ToString()) %>'>
                                            <%# Eval("Estado") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Acciones" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="200px">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEditarG" runat="server" CommandName="EditarG" CommandArgument='<%# Eval("ID_Garantia") %>' CssClass="btn-std btn-sm btn-primary" ToolTip="Editar"><i class="fa-solid fa-pen"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnBajaG" runat="server" CommandName="DarBajaG" CommandArgument='<%# Eval("ID_Garantia") %>' CssClass="btn-std btn-sm btn-danger" ToolTip="Anular Garantía" OnClientClick="return confirmarBaja(this);" Visible='<%# Eval("Estado").ToString() != "Anulada" %>'><i class="fa-solid fa-ban"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnReclamos" runat="server" CommandName="VerReclamos" CommandArgument='<%# Eval("ID_Garantia") %>' CssClass="btn-std btn-sm btn-info" ToolTip="Ver Reclamos"><i class="fa-solid fa-list-check"></i> Reclamos</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pagination" HorizontalAlign="Center" BackColor="#f8f9fa" />
                        </asp:GridView>
                        <asp:Panel ID="pnlMensajeGridG" runat="server" Visible="false" CssClass="alert-info-custom">
                            <i class="fa-solid fa-circle-info"></i> <asp:Label ID="lblMensajeGridG" runat="server"></asp:Label>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlMantGarantia" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 800px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblAccionGarantia" runat="server" Text="Datos de Garantía"></asp:Label></h3>
                </div>
                <asp:HiddenField ID="hfIDGarantia" runat="server" />
                
                <div class="panel-body">
                    <div class="form-grid-layout">
                        <div class="form-full-width">
                            <label class="lbl-std">Lente Vendido (Cliente - Producto) <span class="required-asterisk">*</span></label>
                            <asp:DropDownList ID="ddlDetalleLente" runat="server" CssClass="form-control-std" AutoPostBack="true" OnSelectedIndexChanged="ddlDetalleLente_SelectedIndexChanged"></asp:DropDownList>
                            <small class="help-text">Seleccione para calcular fecha fin.</small>
                            <asp:Label ID="errLente" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>
                        
                        <div>
                            <label class="lbl-std">Entrega Asociada <span class="required-asterisk">*</span></label>
                            <asp:DropDownList ID="ddlEntrega" runat="server" CssClass="form-control-std"></asp:DropDownList>
                            <asp:Label ID="errEntrega" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>

                        <div>
                            <label class="lbl-std">Fecha Fin Garantía <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtFechaFinG" runat="server" TextMode="Date" CssClass="form-control-std" Enabled="false"></asp:TextBox>
                            <small class="help-text">Automática (6 meses desde la venta).</small>
                            <asp:Label ID="errFechaFinG" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>

                        <div>
                            <label class="lbl-std">Estado <span class="required-asterisk">*</span></label>
                            <asp:DropDownList ID="ddlEstadoG" runat="server" CssClass="form-control-std">
                                <asp:ListItem Text="Activa" Value="Activa"></asp:ListItem>
                                <asp:ListItem Text="Vencida" Value="Vencida"></asp:ListItem>
                                <asp:ListItem Text="Anulada" Value="Anulada"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        
                        <div>
                            <label class="lbl-std">Fecha Registro</label>
                            <asp:TextBox ID="txtFechaRegG" runat="server" TextMode="Date" CssClass="form-control-std" Enabled="false"></asp:TextBox>
                        </div>
                    </div>

                    <div style="text-align: right; margin-top: 20px; border-top: 1px solid #eee; padding-top: 20px;">
                        <asp:Button ID="btnCancelarG" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelarG_Click" />
                        <asp:Button ID="btnGuardarG" runat="server" Text="Guardar" CssClass="btn-std btn-success" OnClick="btnGuardarG_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlListadoReclamos" runat="server" Visible="false">
            <div class="panel-card" style="border-left: 5px solid #17a2b8;">
                <div class="panel-header">
                    <h3><i class="fa-solid fa-triangle-exclamation"></i> Reclamos de la Garantía #<asp:Label ID="lblIDGarantiaRef" runat="server"></asp:Label></h3>
                    <div style="display: flex; gap: 10px;">
                        <asp:Button ID="btnVolverG" runat="server" Text="Volver" CssClass="btn-std btn-secondary" OnClick="btnVolverG_Click" />
                        <asp:Button ID="btnNuevoReclamo" runat="server" Text="Nuevo Reclamo" CssClass="btn-std btn-info" OnClick="btnNuevoReclamo_Click" />
                    </div>
                </div>

                <asp:GridView ID="gvReclamos" runat="server" CssClass="table-std" AutoGenerateColumns="False"
                    DataKeyNames="ID_Reclamo" OnRowCommand="gvReclamos_RowCommand"
                    EmptyDataText="No hay reclamos registrados para esta garantía.">
                    <Columns>
                        <asp:BoundField DataField="FechaReclamo" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="100px" />
                        <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
                        <asp:BoundField DataField="EstadoReclamo" HeaderText="Estado" ItemStyle-Font-Bold="true" />
                        <asp:BoundField DataField="Responsable" HeaderText="Responsable" />
                        <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="120px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnEditarR" runat="server" CommandName="EditarR" CommandArgument='<%# Eval("ID_Reclamo") %>' CssClass="btn-std btn-sm btn-primary"><i class="fa-solid fa-pen"></i></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlMantReclamo" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 800px; margin: 0 auto; border: 1px solid #17a2b8;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblAccionReclamo" runat="server" Text="Datos del Reclamo"></asp:Label></h3>
                </div>
                <asp:HiddenField ID="hfIDReclamo" runat="server" />
                
                <div class="panel-body">
                    <div class="form-grid-layout">
                        <div>
                            <label class="lbl-std">Fecha Reclamo <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtFechaReclamo" runat="server" TextMode="Date" CssClass="form-control-std" Enabled="false"></asp:TextBox>
                            <small class="help-text">Fecha de hoy (Automática).</small>
                            <asp:Label ID="errFechaReclamo" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>

                        <div>
                            <label class="lbl-std">Responsable <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtResponsableR" runat="server" CssClass="form-control-std"></asp:TextBox>
                            <small class="help-text">Automático al crear.</small>
                            <asp:Label ID="errResponsableR" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>

                        <div class="form-full-width">
                            <label class="lbl-std">Motivo del Reclamo <span class="required-asterisk">*</span></label>
                            <asp:TextBox ID="txtMotivo" runat="server" CssClass="form-control-std" TextMode="MultiLine" Rows="2"></asp:TextBox>
                            <small class="help-text">Solo letras, un espacio por palabra.</small>
                            <asp:Label ID="errMotivo" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>

                        <div class="form-full-width">
                            <label class="lbl-std">Solución Aplicada</label>
                            <asp:TextBox ID="txtSolucion" runat="server" CssClass="form-control-std" TextMode="MultiLine" Rows="2"></asp:TextBox>
                            <small class="help-text">Solo letras. Requerido si es "Resuelto".</small>
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
                            <small class="help-text">Máx 1 semana.</small>
                            <asp:Label ID="errFechaSolucion" runat="server" CssClass="error-text" Visible="false"></asp:Label>
                        </div>
                    </div>

                    <div style="text-align: right; margin-top: 20px; border-top: 1px solid #eee; padding-top: 20px;">
                        <asp:Button ID="btnCancelarR" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelarR_Click" />
                        <asp:Button ID="btnGuardarR" runat="server" Text="Guardar" CssClass="btn-std btn-success" OnClick="btnGuardarR_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>