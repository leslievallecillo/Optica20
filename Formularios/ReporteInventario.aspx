<%@ Page Title="Reporte de Inventario" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ReporteInventario.aspx.cs" Inherits="Optica.Reportes.ReporteInventario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root { --color-primary: #0056b3; --color-danger: #d32f2f; --bg-light: #f8f9fa; --color-border: #ced4da; }
        .panel-report { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
        .report-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 15px; }
        .toolbar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; background-color: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid var(--color-border); margin-bottom: 20px; }
        .lbl-formal { font-weight: 700; font-size: 0.9rem; color: #2c3e50; display: block; margin-bottom: 5px; }
        .form-control-formal { width: 100%; padding: 8px 12px; font-size: 1rem; border: 1px solid var(--color-border); border-radius: 4px; box-sizing: border-box; }
        .table-rep { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-rep th { background: #17a2b8; color: white; padding: 12px; text-align: left; }
        .table-rep td { padding: 10px; border-bottom: 1px solid #eee; vertical-align: middle; }
        .stock-bajo { color: #dc3545; font-weight: bold; }
        .stock-ok { color: #28a745; font-weight: bold; }
        .btn-std { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; color: white; display: inline-flex; align-items: center; gap: 5px; font-size: 0.9rem; }
        .btn-primary { background-color: var(--color-primary); }
        .btn-danger { background-color: var(--color-danger); }
        .img-prod { width: 40px; height: 40px; object-fit: cover; border-radius: 4px; border: 1px solid #ccc; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px; max-width: 1400px; margin: 0 auto;">
        <div class="panel-report">
            <div class="report-header">
                <h2 style="margin:0; color:#17a2b8;"><i class="fa-solid fa-boxes-stacked"></i> Inventario Actual</h2>
                <asp:Button ID="btnPdf" runat="server" Text="Exportar PDF" CssClass="btn-std btn-danger" OnClick="btnPdf_Click" />
            </div>

            <div class="toolbar">
                <div style="flex-grow: 1; min-width: 200px;">
                    <label class="lbl-formal">Buscar</label>
                    <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-formal" placeholder="Código, Nombre..."></asp:TextBox>
                </div>
                <div style="width: 200px;">
                    <label class="lbl-formal">Categoría</label>
                    <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-control-formal" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoria_SelectedIndexChanged"></asp:DropDownList>
                </div>
                <div style="width: 150px;">
                    <label class="lbl-formal">Filtro Stock</label>
                    <asp:DropDownList ID="ddlFiltroStock" runat="server" CssClass="form-control-formal" AutoPostBack="true" OnSelectedIndexChanged="ddlFiltroStock_SelectedIndexChanged">
                        <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                        <asp:ListItem Text="Bajo Stock (<5)" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Con Existencia" Value="2"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div>
                    <label class="lbl-formal">&nbsp;</label>
                    <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                </div>
            </div>

            <div style="margin-bottom:10px; text-align:right;">
                Mostrar <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
                    <asp:ListItem>10</asp:ListItem><asp:ListItem>20</asp:ListItem><asp:ListItem>50</asp:ListItem><asp:ListItem Value="All">Todos</asp:ListItem>
                </asp:DropDownList>
            </div>

            <asp:GridView ID="gvInventario" runat="server" CssClass="table-rep" AutoGenerateColumns="False" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvInventario_PageIndexChanging" EmptyDataText="No se encontraron productos activos.">
                <Columns>
                    <asp:TemplateField HeaderText="Img" ItemStyle-Width="50px">
                        <ItemTemplate>
                            <asp:Image ID="img" runat="server" CssClass="img-prod" ImageUrl='<%# string.IsNullOrEmpty(Eval("RutaImagen").ToString()) ? "~/Imagenes2/default.png" : Eval("RutaImagen") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Codigo" HeaderText="Código" />
                    <asp:BoundField DataField="Categoria" HeaderText="Categoría" />
                    <asp:TemplateField HeaderText="Producto">
                        <ItemTemplate>
                            <strong><%# Eval("Descripcion") %></strong><br />
                            <small style="color:#666;"><%# Eval("Marca") %> <%# Eval("Modelo") %></small>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Stock">
                        <ItemTemplate>
                            <span class='<%# Convert.ToInt32(Eval("Stock")) < 5 ? "stock-bajo" : "stock-ok" %>'>
                                <%# Eval("Stock") %>
                            </span>
                            <%# Convert.ToInt32(Eval("Stock")) < 5 ? "<i class='fa-solid fa-triangle-exclamation' style='color:#dc3545; margin-left:5px;'></i>" : "" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="UltimoCosto" HeaderText="Último Costo" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="PrecioVenta" HeaderText="Precio Venta" DataFormatString="{0:C}" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>