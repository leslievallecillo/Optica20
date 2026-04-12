<%@ Page Title="Kardex de Producto" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Kardex.aspx.cs" Inherits="Optica.Reportes.Kardex" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <style>
        .panel-kardex { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
        .kardex-header { display:flex; gap:15px; align-items:center; background:#f1f1f1; padding:20px; border-radius:8px; margin-bottom:20px; }
        .input-std { padding:8px; width:100%; border:1px solid #ccc; border-radius:4px; }
        .btn-search { background:#333; color:white; padding:8px 20px; border:none; border-radius:4px; cursor:pointer; }
        
        .table-kardex { width:100%; border-collapse:collapse; font-size:0.9rem; }
        .table-kardex th { background:#343a40; color:white; padding:10px; text-align:left; }
        .table-kardex td { padding:10px; border-bottom:1px solid #eee; }
        
        .mov-entrada { color: #28a745; font-weight:bold; }
        .mov-salida { color: #dc3545; font-weight:bold; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px; max-width: 1200px; margin: 0 auto;">
        <div class="panel-kardex">
            <h2 style="margin-top:0;"><i class="fa-solid fa-right-left"></i> Kardex de Producto</h2>
            
            <div class="kardex-header">
                <div style="flex-grow:1;">
                    <label style="font-weight:bold;">Seleccione Producto:</label>
                    <asp:DropDownList ID="ddlProducto" runat="server" CssClass="input-std" AutoPostBack="true" OnSelectedIndexChanged="ddlProducto_SelectedIndexChanged"></asp:DropDownList>
                </div>
                <div style="min-width:150px;">
                    <label style="font-weight:bold;">Stock Actual:</label>
                    <asp:TextBox ID="txtStockActual" runat="server" CssClass="input-std" ReadOnly="true" style="font-weight:bold; background:#e9ecef; text-align:center;"></asp:TextBox>
                </div>
            </div>

            <asp:GridView ID="gvKardex" runat="server" CssClass="table-kardex" AutoGenerateColumns="False" EmptyDataText="Seleccione un producto para ver sus movimientos.">
                <Columns>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                    <asp:BoundField DataField="Tipo" HeaderText="Movimiento" />
                    <asp:BoundField DataField="Documento" HeaderText="Ref. Documento" />
                    <asp:TemplateField HeaderText="Cantidad">
                        <ItemTemplate>
                            <span class='<%# Eval("Tipo").ToString()=="ENTRADA (COMPRA)" ? "mov-entrada" : "mov-salida" %>'>
                                <%# Eval("Tipo").ToString()=="ENTRADA (COMPRA)" ? "+" : "-" %><%# Eval("Cantidad") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>