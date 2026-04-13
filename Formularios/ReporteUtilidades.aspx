<%@ Page Title="Reporte de Utilidades" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ReporteUtilidades.aspx.cs" Inherits="Optica.Reportes.ReporteUtilidades" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root { --color-primary: #0056b3; --color-danger: #d32f2f; --bg-light: #f8f9fa; --color-border: #ced4da; }
        
        .panel-report { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
        .report-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 15px; }
        
        .filter-bar { background: #f8f9fa; padding: 15px; border-radius: 6px; display: flex; gap: 15px; align-items: flex-end; flex-wrap: wrap; margin-bottom: 20px; }
        .form-grp { display: flex; flex-direction: column; gap: 5px; }
        .input-std { padding: 8px; border: 1px solid #ccc; border-radius: 4px; }
        
        .btn-print { background: #dc3545; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer; }
        .btn-search { background: #0056b3; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer; }

        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        .table-rep { width: 100%; border-collapse: collapse; font-size: 0.9rem; white-space: nowrap; }
        .table-rep th { background: #0056b3; color: white; padding: 12px; text-align: left; }
        .table-rep td { padding: 10px; border-bottom: 1px solid #eee; }

        .utilidad-pos { color: #28a745; font-weight: bold; }
        .utilidad-neg { color: #dc3545; font-weight: bold; }

        @media (max-width: 768px) {
            .panel-report { padding: 15px; }
            .report-header { flex-direction: column; align-items: flex-start; gap: 15px; }
            .btn-print { width: 100%; text-align: center; }
            .filter-bar { flex-direction: column; align-items: stretch; }
            .form-grp { width: 100%; }
            .input-std { width: 100%; box-sizing: border-box; }
            .btn-search { width: 100%; text-align: center; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="padding: 20px; max-width: 1400px; margin: 0 auto; box-sizing: border-box;">
        <div class="panel-report">
            <div class="report-header">
                <h2 style="margin:0; color:#333;"><i class="fa-solid fa-chart-pie"></i> Reporte de Utilidades</h2>
                <asp:Button ID="btnPdf" runat="server" Text="Exportar PDF" CssClass="btn-print" OnClick="btnPdf_Click" />
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
                <asp:GridView ID="gvUtilidades" runat="server" CssClass="table-rep" AutoGenerateColumns="False" EmptyDataText="No hay datos para mostrar.">
                    <Columns>
                        <asp:BoundField DataField="FacturaNo" HeaderText="Factura Nº" ItemStyle-Font-Bold="true" />
                        <asp:BoundField DataField="Cliente" HeaderText="Cliente" />
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
                        <asp:BoundField DataField="TotalCosto" HeaderText="Total Costo" DataFormatString="{0:N2}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                        <asp:BoundField DataField="TotalVenta" HeaderText="Total Venta" DataFormatString="{0:N2}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                        <asp:TemplateField HeaderText="Total Utilidad" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <span class='<%# Convert.ToDecimal(Eval("TotalUtilidad")) >= 0 ? "utilidad-pos" : "utilidad-neg" %>'>
                                    <%# Eval("TotalUtilidad", "{0:N2}") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>