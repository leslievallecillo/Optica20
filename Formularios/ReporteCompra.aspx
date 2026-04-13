<%@ Page Title="Reporte de Compras" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ReporteCompra.aspx.cs" Inherits="Optica.Reportes.ReporteCompra" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root { --color-primary: #0056b3; --color-danger: #d32f2f; --bg-light: #f8f9fa; --color-border: #ced4da; }
        
        .panel-report { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
        .report-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 15px; }
        
        .summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }
        .card-sum { background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%); padding: 20px; border-radius: 8px; border-left: 5px solid #0056b3; }
        .card-sum h4 { margin: 0; color: #666; font-size: 0.9rem; text-transform: uppercase; }
        .card-sum .val { font-size: 1.8rem; font-weight: bold; color: #333; margin-top: 5px; }

        .filter-bar { background: #f8f9fa; padding: 15px; border-radius: 6px; display: flex; gap: 15px; align-items: flex-end; flex-wrap: wrap; margin-bottom: 20px; }
        .form-grp { display: flex; flex-direction: column; gap: 5px; }
        .input-std { padding: 8px; border: 1px solid #ccc; border-radius: 4px; }
        
        .btn-print { background: #dc3545; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer; }
        .btn-search { background: #0056b3; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer; }

        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .table-rep { width: 100%; border-collapse: collapse; font-size: 0.9rem; white-space: nowrap; }
        .table-rep th { background: #0056b3; color: white; padding: 12px; text-align: left; }
        .table-rep td { padding: 10px; border-bottom: 1px solid #eee; }

        @media (max-width: 768px) {
            .panel-report { padding: 15px; }
            .report-header { flex-direction: column; align-items: flex-start; gap: 15px; }
            .btn-print { width: 100%; text-align: center; }
            .filter-bar { flex-direction: column; align-items: stretch; }
            .form-grp { width: 100%; }
            .btn-search { width: 100%; text-align: center; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px; max-width: 1400px; margin: 0 auto; box-sizing: border-box;">
        <div class="panel-report">
            <div class="report-header">
                <h2 style="margin:0; color:#333;"><i class="fa-solid fa-truck-ramp-box"></i> Reporte de Compras</h2>
                <asp:Button ID="btnPdf" runat="server" Text="Exportar PDF" CssClass="btn-print" OnClick="btnPdf_Click" />
            </div>

            <div class="summary-grid">
                <div class="card-sum">
                    <h4>Total Egresos</h4>
                    <div class="val"><asp:Label ID="lblTotalComprado" runat="server" Text="C$ 0.00"></asp:Label></div>
                </div>
                <div class="card-sum" style="border-left-color: #28a745;">
                    <h4>Transacciones</h4>
                    <div class="val"><asp:Label ID="lblCantCompras" runat="server" Text="0"></asp:Label></div>
                </div>
            </div>

            <div class="filter-bar">
                <div class="form-grp">
                    <label>Desde</label>
                    <asp:TextBox ID="txtF1" runat="server" TextMode="Date" CssClass="input-std"></asp:TextBox>
                </div>
                <div class="form-grp">
                    <label>Hasta</label>
                    <asp:TextBox ID="txtF2" runat="server" TextMode="Date" CssClass="input-std"></asp:TextBox>
                </div>
                <div class="form-grp">
                    <label>&nbsp;</label>
                    <asp:Button ID="btnGenerar" runat="server" Text="Generar Reporte" CssClass="btn-search" OnClick="btnGenerar_Click" />
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="gvReporte" runat="server" CssClass="table-rep" AutoGenerateColumns="False" EmptyDataText="No se encontraron registros en este rango de fechas.">
                    <Columns>
                        <asp:BoundField DataField="NumeroCompra" HeaderText="N° DOC" />
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Proveedor" HeaderText="Proveedor" />
                        <asp:BoundField DataField="Usuario" HeaderText="Usuario" />
                        <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" ItemStyle-Font-Bold="true" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>