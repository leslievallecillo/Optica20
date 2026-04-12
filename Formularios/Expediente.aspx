<%@ Page Title="Gestión de Expedientes" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Expediente.aspx.cs" Inherits="Optica.Formularios.Expediente" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root { --color-primary: #0056b3; --color-danger: #d32f2f; --bg-light: #f8f9fa; --color-border: #ced4da; }
        .panel-card { background: #fff; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); margin-bottom: 20px; border: 1px solid #eaeaea; }
        .panel-header { padding: 15px 20px; border-bottom: 1px solid #eaeaea; display: flex; justify-content: space-between; align-items: center; background-color: var(--bg-light); border-radius: 8px 8px 0 0; }
        .panel-body { padding: 25px; }
        .toolbar { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; background-color: #f8f9fa; padding: 15px; border-radius: 6px; border: 1px solid var(--color-border); margin-bottom: 20px; }
        .lbl-formal { font-weight: 700; font-size: 0.9rem; color: #2c3e50; display: block; margin-bottom: 5px; }
        .form-control-formal { width: 100%; padding: 8px 12px; font-size: 1rem; border: 1px solid var(--color-border); border-radius: 4px; box-sizing: border-box; }
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-std th { background-color: #f1f3f5; padding: 12px; text-align: left; border-bottom: 2px solid #ddd; }
        .table-std td { padding: 10px; border-bottom: 1px solid #eee; }
        .btn-std { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; color: white; display: inline-flex; align-items: center; gap: 5px; font-size: 0.9rem; text-decoration:none; }
        .btn-primary { background-color: var(--color-primary); }
        .btn-success { background-color: #28a745; }
        .btn-secondary { background-color: #6c757d; }
        .form-grid-layout { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 20px; }
        .rx-box { background: #f8f9fa; padding: 15px; border: 1px solid #dee2e6; border-radius: 5px; margin-bottom: 15px; }
        .rx-title { font-weight: bold; color: var(--color-primary); border-bottom: 1px solid #ddd; padding-bottom: 5px; margin-bottom: 10px; }
        .badge-status { padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: bold; }
        .badge-active { background-color: #e8f5e9; color: #2e7d32; }
        .badge-inactive { background-color: #ffebee; color: #c62828; }
    </style>

    <script>
        function aplicarFormatoRx() {
            const ids = ['<%= txtOD.ClientID %>', '<%= txtOI.ClientID %>', '<%= txtADD.ClientID %>'];
            ids.forEach(id => {
                const el = document.getElementById(id);
                if (el) {
                    el.addEventListener('blur', function () {
                        let val = this.value.trim();
                        if (val === "" || val.toLowerCase().includes('x')) return;
                        let num = parseFloat(val);
                        if (!isNaN(num)) {
                            this.value = (num >= 0 ? "+" : "") + num.toFixed(2);
                        }
                    });
                }
            });
        }
        window.onload = aplicarFormatoRx;
        if (typeof (Sys) !== 'undefined') { Sys.WebForms.PageRequestManager.getInstance().add_endRequest(aplicarFormatoRx); }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid" style="padding: 20px;">
        <asp:HiddenField ID="hfIDExpediente" runat="server" />

        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3 style="margin:0; color:var(--color-primary);"><i class="fa-solid fa-clipboard-user"></i> Expedientes Clínicos</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="+ Nuevo Expediente" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    <div class="toolbar">
                        <div style="flex-grow: 1; min-width: 200px;">
                            <label class="lbl-formal">Buscar</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-formal" placeholder="Nombre, Beneficiario..."></asp:TextBox>
                        </div>
                        <div style="width: 150px;">
                            <label class="lbl-formal">Estado</label>
                            <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control-formal">
                                <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                                <asp:ListItem Text="Activo" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Inactivo" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div style="width: 160px;">
                            <label class="lbl-formal">Desde</label>
                            <asp:TextBox ID="txtFiltroDesde" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div style="width: 160px;">
                            <label class="lbl-formal">Hasta</label>
                            <asp:TextBox ID="txtFiltroHasta" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div>
                            <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                        </div>
                    </div>

                    <div style="margin-bottom:10px; display:flex; justify-content:space-between; align-items:center;">
                        <div>
                            Mostrar <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
                                <asp:ListItem>10</asp:ListItem><asp:ListItem>20</asp:ListItem><asp:ListItem Value="All">Todos</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <asp:GridView ID="gvExpedientes" runat="server" CssClass="table-std" AutoGenerateColumns="False" GridLines="None" 
                        AllowPaging="True" PageSize="10" OnPageIndexChanging="gvExpedientes_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="ID_Expediente" HeaderText="Ref" />
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="NombreCliente" HeaderText="Titular" />
                            <asp:BoundField DataField="Beneficiario" HeaderText="Paciente" />
                            <asp:BoundField DataField="Producto" HeaderText="Lente" />
                            <asp:TemplateField HeaderText="Estado" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                        <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEditar" runat="server" OnClick="btnEditar_Click" CommandArgument='<%# Eval("ID_Expediente") %>' CssClass="btn-std btn-primary"><i class="fa-solid fa-pen"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelMantenimiento" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 900px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Nuevo Expediente"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    <div class="form-grid-layout">
                        <div>
                            <label class="lbl-formal">Cliente Titular</label>
                            <asp:DropDownList ID="ddlCliente" runat="server" CssClass="form-control-formal" />
                        </div>
                        <div>
                            <label class="lbl-formal">Beneficiario (Paciente)</label>
                            <asp:TextBox ID="txtBeneficiario" runat="server" CssClass="form-control-formal" />
                        </div>
                        <div>
                            <label class="lbl-formal">Fecha Consulta</label>
                            <asp:TextBox ID="txtFecha" runat="server" TextMode="Date" CssClass="form-control-formal" />
                        </div>
                    </div>

                    <div class="rx-box">
                        <div class="rx-title">Medidas Optométricas (Rx)</div>
                        <div class="form-grid-layout" style="grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));">
                            <div><label class="lbl-formal">OD</label><asp:TextBox ID="txtOD" runat="server" CssClass="form-control-formal rx-input" placeholder="+0.00" /></div>
                            <div><label class="lbl-formal">OI</label><asp:TextBox ID="txtOI" runat="server" CssClass="form-control-formal rx-input" placeholder="+0.00" /></div>
                            <div><label class="lbl-formal">AV OD</label><asp:TextBox ID="txtOD_AV" runat="server" CssClass="form-control-formal" placeholder="20/20" /></div>
                            <div><label class="lbl-formal">AV OI</label><asp:TextBox ID="txtOI_AV" runat="server" CssClass="form-control-formal" placeholder="20/20" /></div>
                        </div>
                        <div class="form-grid-layout" style="grid-template-columns: repeat(3, 1fr); margin-top:10px;">
                            <div><label class="lbl-formal">ADD</label><asp:TextBox ID="txtADD" runat="server" CssClass="form-control-formal rx-input" placeholder="+2.00" /></div>
                            <div><label class="lbl-formal">DP</label><asp:TextBox ID="txtDP" runat="server" CssClass="form-control-formal" /></div>
                            <div><label class="lbl-formal">ALT</label><asp:TextBox ID="txtALT" runat="server" CssClass="form-control-formal" /></div>
                        </div>
                    </div>

                    <div class="form-grid-layout">
                        <div><label class="lbl-formal">Lente / Aro</label><asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-control-formal" /></div>
                        <div><label class="lbl-formal">Tratamiento</label><asp:DropDownList ID="ddlTratamiento" runat="server" CssClass="form-control-formal" /></div>
                    </div>

                    <div style="text-align:right; margin-top:20px;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CssClass="btn-std btn-secondary" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar Expediente" OnClick="btnGuardar_Click" CssClass="btn-std btn-success" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
    <a href="Login.aspx">Login.aspx</a>
</asp:Content>