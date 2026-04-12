<%@ Page Title="Gestión de Compras" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionCompras.aspx.cs" Inherits="Optica.Compras.GestionCompras" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Mismos estilos limpios */
        .panel-card { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }
        .header-flex { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #eee; padding-bottom: 15px; margin-bottom: 20px; }
        .filters { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 15px; background: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 20px; align-items: end; }
        .input-std { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
        .btn { padding: 8px 15px; border: none; border-radius: 4px; color: white; cursor: pointer; }
        .btn-primary { background: #0056b3; } .btn-success { background: #28a745; } .btn-danger { background: #dc3545; } .btn-info { background: #17a2b8; }
        .table-std { width: 100%; border-collapse: collapse; } 
        .table-std th { background: #f1f1f1; padding: 10px; text-align: left; } .table-std td { padding: 10px; border-bottom: 1px solid #eee; }

        /* Badges */
        .badge-status { padding: 5px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; }
        .badge-active { background-color: #d4edda; color: #155724; }
        .badge-inactive { background-color: #f8d7da; color: #721c24; }

        /* MODAL VER DETALLE */
        .modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 1000; display: none; justify-content: center; align-items: center; }
        .modal-content { background: white; padding: 20px; width: 90%; max-width: 700px; border-radius: 8px; max-height: 80vh; overflow-y: auto; }
    </style>
    <script>
        function openDetail() { document.getElementById('modalDetail').style.display = 'flex'; }
        function closeDetail() { document.getElementById('modalDetail').style.display = 'none'; return false; }

        function confirmarAnular(sender) {
            Swal.fire({
                title: '¿Anular Compra?',
                text: "ADVERTENCIA: Los productos se restarán del inventario.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, anular'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }

        function confirmarReactivar(sender) {
            Swal.fire({
                title: '¿Reactivar Compra?',
                text: "Los productos volverán a sumarse al inventario.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, reactivar'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="sm" runat="server"></asp:ScriptManager>

    <div style="padding: 20px; max-width: 1400px; margin: 0 auto;">
        <div class="panel-card">
            <div class="header-flex">
                <h2 style="margin:0; color:#333;"><i class="fa-solid fa-list-check"></i> Historial de Compras</h2>
                <asp:Button ID="btnNuevo" runat="server" Text="+ Nueva Compra" CssClass="btn btn-success" OnClick="btnNuevo_Click" />
            </div>

            <div class="filters">
                <div style="grid-column: span 2;">
                    <label>Buscar</label> <asp:TextBox ID="txtBuscar" runat="server" CssClass="input-std" placeholder="N° Factura..."></asp:TextBox>
                </div>
                <div><label>Estado</label> <asp:DropDownList ID="ddlEstado" runat="server" CssClass="input-std"><asp:ListItem Value="-1">Todos</asp:ListItem><asp:ListItem Value="1">Activo</asp:ListItem><asp:ListItem Value="0">Anulado</asp:ListItem></asp:DropDownList></div>
                <div><label>Desde</label> <asp:TextBox ID="txtF1" runat="server" TextMode="Date" CssClass="input-std"></asp:TextBox></div>
                <div><label>Hasta</label> <asp:TextBox ID="txtF2" runat="server" TextMode="Date" CssClass="input-std"></asp:TextBox></div>
                
                <div style="display:flex; gap:5px;">
                    <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn btn-primary" OnClick="btnBuscar_Click" style="flex:1;" />
                    <asp:Button ID="btnMostrarTodo" runat="server" Text="Todo" CssClass="btn btn-info" OnClick="btnMostrarTodo_Click" style="flex:1;" />
                </div>
            </div>

            <asp:UpdatePanel ID="upGrid" runat="server">
                <ContentTemplate>
                    <asp:GridView ID="gvCompras" runat="server" CssClass="table-std" AutoGenerateColumns="False" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvCompras_PageIndexChanging" OnRowCommand="gvCompras_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="ID_Compra" HeaderText="ID" />
                            <asp:BoundField DataField="NumeroCompra" HeaderText="Factura" />
                            <asp:BoundField DataField="Proveedor" HeaderText="Proveedor" />
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            
                            <%-- AQUI ESTA LA CORRECCION: ItemStyle-ForeColor --%>
                            <asp:BoundField DataField="Total" HeaderText="Monto Total" DataFormatString="C$ {0:N2}" ItemStyle-Font-Bold="true" ItemStyle-ForeColor="#0056b3" />
                            
                            <asp:TemplateField HeaderText="Estado" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                        <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Anulado" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnVer" runat="server" CommandName="Ver" CommandArgument='<%# Eval("ID_Compra") %>' CssClass="btn btn-primary" style="padding:5px 10px;"><i class="fa fa-eye"></i> Ver</asp:LinkButton>
                                    <asp:LinkButton ID="btnAnular" runat="server" CommandName="Anular" CommandArgument='<%# Eval("ID_Compra") %>' CssClass="btn btn-danger" style="padding:5px 10px;" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>' OnClientClick="return confirmarAnular(this);"><i class="fa fa-ban"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnReactivar" runat="server" CommandName="Reactivar" CommandArgument='<%# Eval("ID_Compra") %>' CssClass="btn btn-success" style="padding:5px 10px;" Visible='<%# !Convert.ToBoolean(Eval("Estado")) %>' OnClientClick="return confirmarReactivar(this);"><i class="fa-solid fa-check"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div id="modalDetail" class="modal-overlay">
        <div class="modal-content">
            <asp:UpdatePanel ID="upModal" runat="server">
                <ContentTemplate>
                    <div style="display:flex; justify-content:space-between; margin-bottom:15px; border-bottom:1px solid #eee; padding-bottom:10px;">
                        <h3 style="margin:0; color:#0056b3;">Detalle de Compra #<asp:Label ID="lblFacturaModal" runat="server"></asp:Label></h3>
                        <button onclick="return closeDetail()" style="border:none; background:none; font-size:1.5rem; cursor:pointer;">&times;</button>
                    </div>
                    
                    <asp:GridView ID="gvDetalleView" runat="server" CssClass="table-std" AutoGenerateColumns="False" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="Producto" HeaderText="Producto" />
                            <asp:BoundField DataField="Cantidad" HeaderText="Cant." />
                            <asp:BoundField DataField="PrecioUnitario" HeaderText="Costo" DataFormatString="{0:C}" />
                            <asp:BoundField DataField="Iva" HeaderText="IVA" DataFormatString="{0:C}" />
                            <asp:BoundField DataField="PrecioTotal" HeaderText="Total" DataFormatString="{0:C}" />
                        </Columns>
                    </asp:GridView>
                    
                    <div style="text-align:right; margin-top:15px;">
                        <button onclick="return closeDetail()" class="btn btn-secondary">Cerrar</button>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</asp:Content>