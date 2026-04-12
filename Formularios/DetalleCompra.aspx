<%@ Page Title="Detalle de Compra" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="DetalleCompra.aspx.cs" Inherits="Optica.Compras.DetalleCompra" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Mismos estilos que GestionCompras.aspx */
        :root { --color-primary: #0056b3; --color-bg: #f4f6f9; --color-border: #dee2e6; }
        .panel-card { background: #fff; padding: 20px; border-radius: 8px; border: 1px solid rgba(0,0,0,0.1); margin-bottom: 20px; }
        .panel-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #eee; padding-bottom: 15px; margin-bottom: 20px; }
        .panel-header h3 { margin: 0; color: var(--color-primary); font-size: 1.25rem; }
        .filters-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 15px; background: #f8f9fa; padding: 20px; border-radius: 6px; margin-bottom: 20px; align-items: end; }
        .lbl-std { display: block; font-size: 0.85rem; font-weight: bold; color: #666; margin-bottom: 5px; }
        .form-control-std { width: 100%; padding: 8px; border: 1px solid #ced4da; border-radius: 4px; box-sizing: border-box; height: 38px; }
        .btn-std { padding: 8px 15px; border: none; border-radius: 4px; cursor: pointer; color: white; font-weight: 500; text-decoration: none; display: inline-flex; align-items: center; gap: 5px; }
        .btn-primary { background: #0056b3; } .btn-success { background: #28a745; } .btn-danger { background: #dc3545; } .btn-secondary { background: #6c757d; }
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-std th { background: #f8f9fa; padding: 12px; text-align: left; border-bottom: 2px solid #ddd; }
        .table-std td { padding: 10px; border-bottom: 1px solid #eee; }
        .input-form-grid { display: grid; grid-template-columns: 1fr; gap: 15px; max-width: 500px; margin: 0 auto; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px; max-width: 1400px; margin: 0 auto;">
        
        <div style="margin-bottom: 20px;">
            <asp:LinkButton ID="btnVolver" runat="server" PostBackUrl="/Formularios/GestionCompras.aspx" CssClass="btn-std btn-secondary">
                <i class="fa-solid fa-arrow-left"></i> Volver al Listado
            </asp:LinkButton>
        </div>

        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3><i class="fa-solid fa-list-check"></i> Gestión de Detalle Compra</h3>
                    <span style="font-size: 0.9rem; color: #666;">Compra #<asp:Label ID="lblNumCompra" runat="server" Font-Bold="true"></asp:Label></span>
                    <asp:Button ID="btnNuevo" runat="server" Text="Nuevo registro" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>

                <div class="filters-grid">
                    <div>
                        <label class="lbl-std">Búsqueda de Detalle</label>
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-std" placeholder="Producto..."></asp:TextBox>
                    </div>
                    <div>
                        <label class="lbl-std">Estado</label>
                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-control-std">
                            <asp:ListItem Text="Todos" Value="-1" />
                            <asp:ListItem Text="Activo" Value="1" />
                            <asp:ListItem Text="Anulado" Value="0" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="lbl-std">Ordenar por</label>
                        <asp:DropDownList ID="ddlOrden" runat="server" CssClass="form-control-std">
                            <asp:ListItem Text="Producto A-Z" Value="ASC" />
                            <asp:ListItem Text="Producto Z-A" Value="DESC" />
                        </asp:DropDownList>
                    </div>
                     <div>
                        <asp:Button ID="btnBuscar" runat="server" Text="Buscar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" Width="100%" />
                    </div>
                </div>

                <asp:GridView ID="gvDetalles" runat="server" CssClass="table-std" AutoGenerateColumns="False"
                    OnRowCommand="gvDetalles_RowCommand" EmptyDataText="No hay productos en esta compra.">
                    <Columns>
                        <asp:BoundField DataField="Producto" HeaderText="Producto" />
                        <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
                        <asp:BoundField DataField="PrecioUnitario" HeaderText="Precio Unitario" DataFormatString="{0:C}" />
                        <asp:BoundField DataField="Iva" HeaderText="Iva" DataFormatString="{0:C}" />
                        <asp:BoundField DataField="PrecioTotal" HeaderText="Precio Total" DataFormatString="{0:C}" />
                         <asp:BoundField DataField="PrecioVenta" HeaderText="P. Venta" DataFormatString="{0:C}" />
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <span style='color:<%# Convert.ToBoolean(Eval("Estado")) ? "green" : "red" %>'>
                                    <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Anulado" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnBaja" runat="server" CommandName="DarBaja" CommandArgument='<%# Eval("ID_DetalleCompra") %>' 
                                    CssClass="btn-std btn-danger btn-sm" OnClientClick="return confirm('¿Eliminar producto?');" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>'>
                                    <i class="fa-solid fa-trash"></i> Dar baja
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelRegistro" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 600px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><i class="fa-solid fa-plus-circle"></i> Registrar Detalle de Compra</h3>
                </div>
                <div style="padding: 20px;">
                    <div class="input-form-grid">
                        <div>
                            <label class="lbl-std">Compra</label>
                            <asp:TextBox ID="txtCompraID" runat="server" CssClass="form-control-std" ReadOnly="true" BackColor="#eee"></asp:TextBox>
                        </div>
                        
                        <div>
                            <label class="lbl-std">Producto</label>
                            <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-control-std"></asp:DropDownList>
                        </div>

                        <div>
                            <label class="lbl-std">Cantidad</label>
                            <asp:TextBox ID="txtCantidad" runat="server" TextMode="Number" CssClass="form-control-std"></asp:TextBox>
                        </div>

                        <div>
                            <label class="lbl-std">Precio Unitario</label>
                            <asp:TextBox ID="txtPrecioUnitario" runat="server" TextMode="Number" step="0.01" CssClass="form-control-std"></asp:TextBox>
                        </div>

                        <div>
                            <label class="lbl-std">Iva</label>
                            <asp:TextBox ID="txtIva" runat="server" TextMode="Number" step="0.01" CssClass="form-control-std" Text="0.00"></asp:TextBox>
                        </div>
                        
                         <div>
                            <label class="lbl-std">Precio venta (Nuevo Precio)</label>
                            <asp:TextBox ID="txtPrecioVenta" runat="server" TextMode="Number" step="0.01" CssClass="form-control-std"></asp:TextBox>
                        </div>

                        <div>
                            <label class="lbl-std">Fecha registro</label>
                            <asp:TextBox ID="txtFechaRegistro" runat="server" CssClass="form-control-std" ReadOnly="true" BackColor="#eee"></asp:TextBox>
                        </div>

                        <div style="display: flex; gap: 10px; justify-content: center; margin-top: 20px;">
                            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" Width="120px" />
                            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" Width="120px" />
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>