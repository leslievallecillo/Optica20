<%@ Page Title="Reporte de Compras" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ReporteCompra.aspx.cs" Inherits="Optica.Reportes.ReporteCompra" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root { --color-primary: #0056b3; --color-danger: #d32f2f; --bg-light: #f8f9fa; --color-border: #ced4da; }
        .panel-report { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
        .report-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 15px; }
        
        /* Estilos del Toolbar (Filtros) */
        .toolbar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; background-color: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid var(--color-border); margin-bottom: 20px; }
        .lbl-formal { font-weight: 700; font-size: 0.9rem; color: #2c3e50; display: block; margin-bottom: 5px; }
        .form-control-formal { width: 100%; padding: 8px 12px; font-size: 1rem; border: 1px solid var(--color-border); border-radius: 4px; box-sizing: border-box; }
        
        /* Estilos de Tabla */
        .table-rep { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-rep th { background: #17a2b8; color: white; padding: 12px; text-align: left; }
        .table-rep td { padding: 10px; border-bottom: 1px solid #eee; vertical-align: middle; }
        
        /* Botones */
        .btn-std { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; color: white; display: inline-flex; align-items: center; gap: 5px; font-size: 0.9rem; }
        .btn-primary { background-color: var(--color-primary); }
        .btn-danger { background-color: var(--color-danger); }

        /* Tarjetas de Resumen (Adaptadas al nuevo diseño) */
        .summary-container { display: flex; gap: 20px; margin-bottom: 20px; }
        .card-sum { background: #fff; border: 1px solid #eee; border-left: 4px solid #17a2b8; padding: 15px; border-radius: 4px; flex: 1; box-shadow: 0 2px 5px rgba(0,0,0,0.03); }
        .card-sum h4 { margin: 0 0 5px 0; font-size: 0.85rem; color: #666; text-transform: uppercase; }
        .card-sum .val { font-size: 1.5rem; font-weight: bold; color: #333; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px; max-width: 1400px; margin: 0 auto;">
        <div class="panel-report">
            
            <div class="report-header">
                <h2 style="margin:0; color:#17a2b8;"><i class="fa-solid fa-truck-ramp-box"></i> Reporte de Compras</h2>
                <asp:Button ID="btnPdf" runat="server" Text="Exportar PDF" CssClass="btn-std btn-danger" OnClick="btnPdf_Click" />
            </div>

            <div class="summary-container">
                <div class="card-sum">
                    <h4>Total Egresos</h4>
                    <div class="val"><asp:Label ID="lblTotalComprado" runat="server" Text="C$ 0.00"></asp:Label></div>
                </div>
                <div class="card-sum" style="border-left-color: #28a745;">
                    <h4>Transacciones</h4>
                    <div class="val"><asp:Label ID="lblCantCompras" runat="server" Text="0"></asp:Label></div>
                </div>
            </div>

            <div class="toolbar">
                <div style="flex-grow: 1; min-width: 200px;">
                    <label class="lbl-formal">Desde</label>
                    <asp:TextBox ID="txtF1" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                </div>
                <div style="flex-grow: 1; min-width: 200px;">
                    <label class="lbl-formal">Hasta</label>
                    <asp:TextBox ID="txtF2" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                </div>
                <div>
                    <label class="lbl-formal">&nbsp;</label>
                    <asp:Button ID="btnGenerar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnGenerar_Click" />
                </div>
            </div>

            <asp:GridView ID="gvReporte" runat="server" CssClass="table-rep" AutoGenerateColumns="False" EmptyDataText="No se encontraron registros en este rango de fechas.">
                <Columns>
                    <asp:BoundField DataField="NumeroCompra" HeaderText="N° DOC" ItemStyle-Font-Bold="true" />
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:BoundField DataField="Proveedor" HeaderText="Proveedor" />
                    <asp:BoundField DataField="Usuario" HeaderText="Usuario" />
                    <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" ItemStyle-Font-Bold="true" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                </Columns>
            </asp:GridView>

        </div>
    </div>
</asp:Content>