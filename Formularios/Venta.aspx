<%@ Page Title="Punto de Venta" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Venta.aspx.cs" Inherits="Optica.Formularios.Venta" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root { --c-primary: #0056b3; --c-success: #198754; --c-danger: #dc3545; --c-bg: #f4f6f9; }
        .pos-card { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
        .form-control-pos { width: 100%; padding: 10px 12px; border: 1px solid #ced4da; border-radius: 6px; font-size: 0.95rem; transition: all 0.2s; box-sizing: border-box; }
        .form-control-pos:focus { border-color: var(--c-primary); outline: none; box-shadow: 0 0 0 3px rgba(0, 86, 179, 0.15); }
        .form-control-pos[readonly] { background-color: #e9ecef; color: #495057; font-weight: bold; cursor: not-allowed; }
        .is-invalid { border-color: var(--c-danger) !important; background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 12 12' width='12' height='12' fill='none' stroke='%23dc3545'%3e%3ccircle cx='6' cy='6' r='4.5'/%3e%3cpath stroke-linejoin='round' d='M5.8 3.6h.4L6 6.5z'/%3e%3ccircle cx='6' cy='8.2' r='.6' fill='%23dc3545' stroke='none'/%3e%3c/svg%3e"); background-repeat: no-repeat; background-position: right calc(0.375em + 0.1875rem) center; background-size: calc(0.75em + 0.375rem) calc(0.75em + 0.375rem); }
        .error-msg { width: 100%; margin-top: 0.25rem; font-size: 0.875em; color: var(--c-danger); font-weight: 600; display: flex; align-items: center; gap: 5px; }
        .error-msg i { font-size: 1em; }
        .pos-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #eee; padding-bottom: 15px; margin-bottom: 20px; }
        .detail-section { background: #fcfcfc; border: 1px solid #e0e0e0; border-radius: 6px; padding: 20px; margin-top: 15px; border-left: 5px solid #ccc; }
        .active-prod { border-left-color: var(--c-success); }
        .active-lente { border-left-color: var(--c-primary); }
        .pay-box { background-color: #eef2f7; padding: 20px; border-radius: 8px; margin-top: 25px; border: 1px solid #dce4ec; }
        .total-display { font-size: 2rem; font-weight: 800; color: var(--c-primary); text-align: right; border:none; background:transparent; width: 100%; }
        .input-money { font-size: 1.2rem; font-weight: bold; text-align: right; color: #333; }
        .btn-pos { padding: 12px 25px; border: none; border-radius: 5px; font-weight: 600; cursor: pointer; color: white; display: inline-flex; align-items: center; gap: 8px; }
        .btn-save { background-color: var(--c-success); } .btn-save:hover { background-color: #157347; }
        .btn-cancel { background-color: var(--c-danger); }
    </style>

    <script>
        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                const target = e.target;
                if (target.tagName === "TEXTAREA") return;

                if (target.type === "submit" || target.type === "button") {
                    return;
                }

                e.preventDefault();

                const selector = "input:not([disabled]):not([readonly]):not([type=hidden]), select:not([disabled]), .btn-save";
                const controls = Array.from(document.querySelectorAll(selector));
                const index = controls.indexOf(target);

                if (index > -1 && index < controls.length - 1) {
                    controls[index + 1].focus();
                    if (controls[index + 1].tagName === "INPUT") {
                        controls[index + 1].select();
                    }
                }
            }
        });

        function mostrarAlertaImpresion(idVenta) {
            Swal.fire({
                title: 'Operación Exitosa',
                text: 'Venta guardada correctamente. ¿Desea imprimir la factura?',
                icon: 'success',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="fa-solid fa-print"></i> Sí, imprimir',
                cancelButtonText: 'No, salir'
            }).then((result) => {
                if (result.isConfirmed) {
                    document.getElementById('<%= hfIdImprimir.ClientID %>').value = idVenta;
                    document.getElementById('<%= btnImprimirOculto.ClientID %>').click();
                    setTimeout(function () { window.location.href = 'HistorialVentas.aspx'; }, 2000);
                } else {
                    window.location.href = 'HistorialVentas.aspx';
                }
            });
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="sm1" runat="server"></asp:ScriptManager>

    <div class="pos-card">
        <asp:UpdatePanel ID="upVenta" runat="server">
            <Triggers>
                <asp:PostBackTrigger ControlID="btnImprimirOculto" />
            </Triggers>
            <ContentTemplate>
                <asp:HiddenField ID="hfIdVenta" runat="server" />
                <asp:HiddenField ID="hfIdImprimir" runat="server" />
                <asp:Button ID="btnImprimirOculto" runat="server" OnClick="btnImprimirOculto_Click" style="display:none;" />

                <div class="pos-header">
                    <h2 style="margin:0; color:#333;"><i class="fa-solid fa-cash-register"></i> <asp:Label ID="lblTitulo" runat="server" Text="Nueva Venta"></asp:Label></h2>
                    <div style="text-align:right;">
                        <small style="font-weight:bold; color:#555;">Factura #</small><br />
                        <asp:Label ID="lblNoFactura" runat="server" Text="---" style="font-size:1.3rem; color:#dc3545; font-weight:bold;"></asp:Label>
                    </div>
                </div>

                <div style="display:grid; grid-template-columns:repeat(auto-fit, minmax(200px, 1fr)); gap:15px;">
                    <div>
                        <label style="font-weight:600;">Cliente <span style="color:red">*</span></label>
                        <asp:DropDownList ID="ddlCliente" runat="server" CssClass="form-control-pos" AutoPostBack="true" OnSelectedIndexChanged="ddlCliente_SelectedIndexChanged"></asp:DropDownList>
                        <asp:Label ID="errCliente" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                    </div>
                    <div>
                        <label style="font-weight:600;">Tipo Documento <span style="color:red">*</span></label>
                        <asp:DropDownList ID="ddlTipoDoc" runat="server" CssClass="form-control-pos"></asp:DropDownList>
                        <asp:Label ID="errTipoDoc" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                    </div>
                    <div>
                        <label style="font-weight:600;">Usuario</label>
                        <asp:TextBox ID="txtUsuarioForm" runat="server" CssClass="form-control-pos" ReadOnly="true"></asp:TextBox>
                    </div>
                    <div>
                        <label style="font-weight:600;">Fecha</label>
                        <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control-pos" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>

                <div style="margin:20px 0; padding-bottom:10px; border-bottom:1px solid #eee;">
                    <label style="margin-right:20px; cursor:pointer; font-weight:bold; color:#0056b3;">
                        <asp:CheckBox ID="chkProducto" runat="server" AutoPostBack="true" OnCheckedChanged="Recalcular_Event" /> Incluir Producto
                    </label>
                    <label style="cursor:pointer; font-weight:bold; color:#0056b3;">
                        <asp:CheckBox ID="chkLente" runat="server" AutoPostBack="true" OnCheckedChanged="Recalcular_Event" /> Incluir Lente
                    </label>
                    <asp:Label ID="errSeccion" runat="server" CssClass="error-msg" Visible="false" style="margin-top:5px;"></asp:Label>
                </div>

                <asp:Panel ID="pnlProducto" runat="server" Visible="false" CssClass="detail-section active-prod">
                    <h4><i class="fa-solid fa-box"></i> Detalle Producto</h4>
                    <div style="display:grid; grid-template-columns: 2fr 1fr 1fr; gap:15px;">
                        <div>
                            <label>Producto</label>
                            <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-control-pos" AutoPostBack="true" OnSelectedIndexChanged="ddlProducto_SelectedIndexChanged"></asp:DropDownList>
                            <asp:Label ID="errProducto" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                        </div>
                        <div>
                            <label>Cantidad</label>
                            <asp:TextBox ID="txtCantProd" runat="server" CssClass="form-control-pos" TextMode="Number" Text="1" min="1" AutoPostBack="true" OnTextChanged="Recalcular_Event"></asp:TextBox>
                            <asp:Label ID="errCantProd" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                        </div>
                        <div>
                            <label>Precio Unit. (C$)</label>
                            <asp:TextBox ID="txtPrecioProd" runat="server" CssClass="form-control-pos" ReadOnly="true" placeholder="0.00"></asp:TextBox>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlLente" runat="server" Visible="false" CssClass="detail-section active-lente">
                    <h4><i class="fa-solid fa-glasses"></i> Detalle Lente</h4>
                    <div style="display:grid; grid-template-columns: 2fr 1fr; gap:15px;">
                        <div>
                            <label>Expediente Visual</label>
                            <asp:DropDownList ID="ddlExpediente" runat="server" CssClass="form-control-pos" AutoPostBack="true" OnSelectedIndexChanged="ddlExpediente_SelectedIndexChanged"></asp:DropDownList>
                            <asp:Label ID="errExpediente" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                        </div>
                        <div>
                            <label>Subtotal (C$)</label>
                            <asp:TextBox ID="txtSubtotalLente" runat="server" CssClass="form-control-pos" ReadOnly="true" placeholder="0.00"></asp:TextBox>
                        </div>
                    </div>
                </asp:Panel>

                <div class="pay-box">
                    <div style="text-align:right;">
                        <small style="color:#555; font-weight:bold;">TOTAL A FACTURAR</small>
                        <asp:TextBox ID="txtTotalVenta" runat="server" CssClass="total-display" ReadOnly="true" Text="0.00"></asp:TextBox>
                        <asp:Label ID="errTotal" runat="server" CssClass="error-msg" Visible="false" style="justify-content:flex-end;"></asp:Label>
                    </div>
                    <hr />
                    
                    <div style="display:grid; grid-template-columns:repeat(auto-fit, minmax(180px, 1fr)); gap:20px; margin-top:20px;">
                        <div>
                            <label style="font-weight:700; color:#0056b3;">El Adelanto (C$)</label>
                            <asp:TextBox ID="txtAdelanto" runat="server" CssClass="form-control-pos input-money" ReadOnly="true" Text="0.00"></asp:TextBox>
                        </div>
                        <div>
                            <label style="font-weight:700; color:#333;">A PAGAR HOY (C$)</label>
                            <asp:TextBox ID="txtMontoPagar" runat="server" CssClass="form-control-pos input-money" ReadOnly="true"></asp:TextBox>
                            <asp:Label ID="lblInfoPago" runat="server" style="font-size:0.8rem; color:#666;"></asp:Label>
                        </div>
                        <div>
                            <label style="font-weight:700;">CLIENTE ENTREGA (C$)</label>
                            <asp:TextBox ID="txtPagaCon" runat="server" CssClass="form-control-pos input-money" AutoPostBack="true" OnTextChanged="Recalcular_Event" placeholder="0.00"></asp:TextBox>
                            <asp:Label ID="errPagaCon" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                        </div>
                        <div>
                            <label style="font-weight:700; color:#198754;">SU VUELTO</label>
                            <asp:TextBox ID="txtVuelto" runat="server" CssClass="form-control-pos input-money" ReadOnly="true" style="background:#d1e7dd; color:#0f5132;" Text="0.00"></asp:TextBox>
                        </div>
                    </div>

                    <div style="display:grid; grid-template-columns: 1fr 2fr; gap:20px; margin-top:20px;">
                        <div>
                            <label>Forma de Pago <span style="color:red">*</span></label>
                            <asp:DropDownList ID="ddlTipoPago" runat="server" CssClass="form-control-pos"></asp:DropDownList>
                            <asp:Label ID="errTipoPago" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                        </div>
                        <div>
                            <label>Nota</label>
                            <asp:TextBox ID="txtDescPago" runat="server" CssClass="form-control-pos"></asp:TextBox>
                            <div style="color:red; font-size:0.85rem; font-weight:600; margin-top:4px;">solo permitido letras</div>
                            <asp:Label ID="errNota" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                        </div>
                    </div>
                </div>

                <asp:Panel ID="pnlSaldoLente" runat="server" CssClass="pay-box" Visible="false" style="background-color: #fff3cd; border-color: #ffe69c; margin-top: 15px;">
                    <h5 style="color:#856404; font-weight:bold; margin-top:0; margin-bottom: 15px;"><i class="fa-solid fa-scale-balanced"></i> Saldo pendiente</h5>
                    <div style="display:grid; grid-template-columns:repeat(auto-fit, minmax(180px, 1fr)); gap:20px;">
                        <div>
                            <label style="font-weight:700;">Estado de Pago</label>
                            <asp:DropDownList ID="ddlEstadoPagoVenta" runat="server" CssClass="form-control-pos" AutoPostBack="true" OnSelectedIndexChanged="Recalcular_Event">
                                <asp:ListItem Value="Pendiente" Text="Pendiente"></asp:ListItem>
                                <asp:ListItem Value="Cancelado" Text="Cancelado"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label style="font-weight:700;">Saldo a Pagar (C$)</label>
                            <asp:TextBox ID="txtTotalSaldo" runat="server" CssClass="form-control-pos input-money" ReadOnly="true" Text="0.00"></asp:TextBox>
                        </div>
                        <div>
                            <label style="font-weight:700;">Recibe para Saldo (C$)</label>
                            <asp:TextBox ID="txtMontoRecibidoSaldo" runat="server" CssClass="form-control-pos input-money" AutoPostBack="true" OnTextChanged="Recalcular_Event" placeholder="0.00"></asp:TextBox>
                            <asp:Label ID="errMontoRecibidoSaldo" runat="server" CssClass="error-msg" Visible="false"></asp:Label>
                        </div>
                        <div>
                            <label style="font-weight:700; color:#198754;">Vuelto Saldo (C$)</label>
                            <asp:TextBox ID="txtCambioSaldo" runat="server" CssClass="form-control-pos input-money" ReadOnly="true" style="background:#d1e7dd; color:#0f5132;" Text="0.00"></asp:TextBox>
                        </div>
                    </div>
                </asp:Panel>

                <div style="margin-top:30px; text-align:right; display:flex; justify-content:flex-end; gap:10px;">
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-pos btn-cancel" OnClick="btnCancelar_Click" CausesValidation="false" />
                    <asp:Button ID="btnFinalizar" runat="server" Text="Confirmar Venta" CssClass="btn-pos btn-save" OnClick="btnFinalizar_Click" />
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>