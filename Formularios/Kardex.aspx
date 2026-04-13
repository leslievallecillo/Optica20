<%@ Page Title="Kardex de Producto" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Kardex.aspx.cs" Inherits="Optica.Reportes.Kardex" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
<style>
        /* Panel principal: Fondo blanco y sombra suave igual a Ventas */
        .panel-kardex { 
            background: #ffffff; 
            padding: 25px; 
            border-radius: 8px; 
            box-shadow: 0 4px 10px rgba(0,0,0,0.05); 
        }
        
        /* Contenedor de filtros: Gris muy claro (#f8f9fa) como en la imagen */
        .kardex-header { 
            display: flex; 
            gap: 15px; 
            align-items: center; 
            background: #f8f9fa; 
            padding: 20px; 
            border-radius: 8px; 
            margin-bottom: 20px; 
        }
        
        /* Inputs estandarizados con el borde sutil de la imagen */
        .input-std { 
            padding: 8px 12px; 
            width: 100%; 
            border: 1px solid #ced4da; 
            border-radius: 4px; 
            background-color: #ffffff;
            color: #495057;
        }
        
        /* Tabla estandarizada. Usa el azul del botón "Generar Reporte" para la cabecera */
        .table-kardex { 
            width: 100%; 
            border-collapse: collapse; 
            font-size: 0.9rem; 
            border: 1px solid #dee2e6; /* Borde exterior que rodea el "No hay ventas" de la imagen */
        }
        .table-kardex th { 
            background: #0056b3; 
            color: white; 
            padding: 12px; 
            text-align: left; 
        }
        .table-kardex td { 
            padding: 10px; 
            border-bottom: 1px solid #eee; 
            color: #333;
        }
        
        /* Colores para las cantidades (Intactos) */
        .mov-entrada { color: #28a745; font-weight: bold; }
        .mov-salida { color: #dc3545; font-weight: bold; }
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