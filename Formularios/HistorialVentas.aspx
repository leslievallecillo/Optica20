<%@ Page Title="Historial de Ventas" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="HistorialVentas.aspx.cs" Inherits="Optica.Formularios.HistorialVentas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        .main-card { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
        .filter-bar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; margin-bottom: 20px; padding: 15px; background: #f8f9fa; border-radius: 8px; }
        .filter-group { display: flex; flex-direction: column; gap: 5px; }
        .filter-group label { font-size: 0.85rem; font-weight: 600; color: #555; }
        .form-control-sm { padding: 8px; border: 1px solid #ddd; border-radius: 4px; font-size: 0.9rem; }
        .table-elegant { width: 100%; border-collapse: collapse; margin-top: 10px; }
        .table-elegant th { text-align: left; padding: 15px; color: #444; font-weight: 700; border-bottom: 2px solid #eee; font-size: 0.95rem; }
        .table-elegant td { padding: 15px; vertical-align: middle; border-bottom: 1px solid #f0f0f0; color: #333; font-size: 0.9rem; }
        .table-elegant tr:hover { background-color: #fafafa; }
        .table-elegant tr:last-child td { border-bottom: none; }
        .btn-filter { background-color: #0056b3; color: white; border: none; padding: 9px 20px; border-radius: 4px; cursor: pointer; font-weight: 600; }
        .btn-filter:hover { background-color: #004494; }
        .badge { padding: 5px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
        .bg-active { background-color: #e6f4ea; color: #1e7e34; }
        .bg-inactive { background-color: #feeced; color: #dc3545; }
        .bg-pendiente { background-color: #fff3cd; color: #856404; }
        .action-btn { width: 32px; height: 32px; border-radius: 4px; display: inline-flex; justify-content: center; align-items: center; text-decoration: none; transition: 0.2s; border: none; cursor: pointer; margin-right: 5px; }
        .btn-edit { background-color: #0056b3; color: white; }
        .btn-edit:hover { background-color: #003d80; }
        .btn-anular { background-color: #dc3545; color: white; }
        .btn-anular:hover { background-color: #b02a37; }
        .btn-reactivar { background-color: #198754; color: white; }
        .btn-reactivar:hover { background-color: #146c43; }
        .btn-print { background-color: #17a2b8; color: white; } 
        .btn-print:hover { background-color: #138496; }
        .msg-count { background-color: #fff3cd; color: #856404; padding: 10px; border-radius: 4px; border: 1px solid #ffeeba; margin-top: 10px; font-size: 0.9rem; display: flex; align-items: center; gap: 8px; }
    </style>

    <script>
        function confirmarAccion(btn, e, mensaje, icono) {
            if (btn.getAttribute("data-confirmed") === "true") {
                return true;
            } else {
                e.preventDefault();
                Swal.fire({
                    title: '¿Estás seguro?',
                    text: mensaje,
                    icon: icono,
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Sí, confirmar',
                    cancelButtonText: 'Cancelar'
                }).then((result) => {
                    if (result.isConfirmed) {
                        btn.setAttribute("data-confirmed", "true");
                        btn.click();
                    }
                });
                return false;
            }
        }

        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                const target = e.target;
                if (target.closest('.filter-bar')) {
                    e.preventDefault();
                    const btn = document.getElementById('<%= btnFiltrar.ClientID %>');
                    if (btn) btn.click();
                }
            }
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="main-card">
        <h2 style="color:#333; margin-bottom:20px;">Historial de Ventas</h2>

        <div class="filter-bar">
            <div class="filter-group">
                <label>Buscar (Fac/Cliente)</label>
                <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-sm" placeholder="Escriba aquí..."></asp:TextBox>
            </div>
            <div class="filter-group">
                <label>Estado</label>
                <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-control-sm">
                    <asp:ListItem Text="Todos" Value="-1" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Activos" Value="1"></asp:ListItem>
                    <asp:ListItem Text="Inactivos" Value="0"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label>Desde</label>
                <asp:TextBox ID="txtFIni" runat="server" TextMode="Date" CssClass="form-control-sm"></asp:TextBox>
            </div>
            <div class="filter-group">
                <label>Hasta</label>
                <asp:TextBox ID="txtFFin" runat="server" TextMode="Date" CssClass="form-control-sm"></asp:TextBox>
            </div>
            <div class="filter-group">
                <label>Mostrar</label>
                <asp:DropDownList ID="ddlLimite" runat="server" CssClass="form-control-sm">
                    <asp:ListItem Text="Todos" Value="0" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="10 registros" Value="10"></asp:ListItem>
                    <asp:ListItem Text="20 registros" Value="20"></asp:ListItem>
                    <asp:ListItem Text="30 registros" Value="30"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn-filter" OnClick="btnFiltrar_Click" />
            </div>
        </div>

        <asp:GridView ID="gvVentas" runat="server" AutoGenerateColumns="false" CssClass="table-elegant" GridLines="None" DataKeyNames="ID_Venta" OnRowCommand="gvVentas_RowCommand">
            <Columns>
                <asp:BoundField DataField="NumeroDocumento" HeaderText="Factura" ItemStyle-Font-Bold="true" />
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Cliente" HeaderText="Cliente" />
                <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="C$ {0:N2}" />
                <asp:BoundField DataField="TipoDoc" HeaderText="Tipo Doc." />
                
                <asp:TemplateField HeaderText="Pago Lente">
                    <ItemTemplate>
                        <span class='badge <%# Convert.ToString(Eval("EstadoPagoVenta")) == "Pendiente" ? "bg-pendiente" : "bg-active" %>' style='<%# string.IsNullOrEmpty(Convert.ToString(Eval("EstadoPagoVenta"))) ? "display:none;" : "" %>'>
                            <%# Convert.ToString(Eval("EstadoPagoVenta")).ToUpper() %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Estado">
                    <ItemTemplate>
                        <span class='badge <%# Convert.ToBoolean(Eval("Estado")) ? "bg-active" : "bg-inactive" %>'>
                            <%# Convert.ToBoolean(Eval("Estado")) ? "ACTIVO" : "INACTIVO" %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="160px">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnPrint" runat="server" CommandName="Imprimir" CommandArgument='<%# Eval("ID_Venta") %>' CssClass="action-btn btn-print" ToolTip="Generar Reporte PDF">
                            <i class="fa-solid fa-print"></i>
                        </asp:LinkButton>

                        <asp:LinkButton ID="btnMod" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Venta") %>' CssClass="action-btn btn-edit" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>' ToolTip="Modificar">
                            <i class="fa-solid fa-pen"></i>
                        </asp:LinkButton>

                        <asp:LinkButton ID="btnBaja" runat="server" CommandName="CambiarEstado" CommandArgument='<%# Eval("ID_Venta") %>' CssClass="action-btn btn-anular" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>' OnClientClick="return confirmarAccion(this, event, '¿Desea dar de baja esta venta? (No será eliminada)', 'warning');" ToolTip="Anular">
                            <i class="fa-solid fa-ban"></i>
                        </asp:LinkButton>

                        <asp:LinkButton ID="btnAlta" runat="server" CommandName="CambiarEstado" CommandArgument='<%# Eval("ID_Venta") %>' CssClass="action-btn btn-reactivar" Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>' OnClientClick="return confirmarAccion(this, event, '¿Desea reactivar esta venta?', 'question');" ToolTip="Reactivar">
                            <i class="fa-solid fa-rotate-left"></i>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:Panel ID="pnlAviso" runat="server" Visible="false" CssClass="msg-count">
            <i class="fa-solid fa-circle-info"></i>
            <asp:Label ID="lblAviso" runat="server"></asp:Label>
        </asp:Panel>
    </div>
</asp:Content>