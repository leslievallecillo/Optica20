<%@ Page Title="Registrar Compra" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="RegistrarCompra.aspx.cs" Inherits="Optica.Compras.RegistrarCompra" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root {
            --bg-app: #f8fafc; --bg-card: #ffffff; --text-main: #1e293b; --text-muted: #64748b;
            --border-input: #cbd5e1; --accent: #3b82f6; --accent-hover: #2563eb;
            --success-bg: #ecfdf5; --success-text: #059669; --error: #ef4444;
        }

        .empresa-wrapper { font-family: 'Inter', sans-serif; background-color: var(--bg-app); padding: 25px 40px; box-sizing: border-box; color: var(--text-main); min-height: 100vh; }
        .page-header { margin-bottom: 20px; border-bottom: 1px solid #e2e8f0; padding-bottom: 10px; display: flex; justify-content: space-between; align-items: center; }
        .page-title { font-size: 1.4rem; font-weight: 700; color: var(--text-main); display: flex; align-items: center; gap: 10px; }
        .ui-card { background: var(--bg-card); border-radius: 12px; box-shadow: 0 2px 5px rgba(0, 0, 0, 0.04); padding: 30px; border: 1px solid #f1f5f9; margin-bottom: 20px; }
        
        .ui-input { width: 100%; padding: 0 15px; height: 42px; font-size: 0.95rem; color: var(--text-main); border: 1px solid var(--border-input); border-radius: 8px; font-family: 'Inter', sans-serif; box-sizing: border-box; transition: border-color 0.3s; }
        .ui-input:focus { outline: none; border-color: var(--accent); box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15); }
        .ui-input[readonly] { background-color: #f1f5f9; color: #64748b; cursor: not-allowed; font-weight: 600; }
        .field-label { font-weight: 500; color: var(--text-muted); font-size: 0.9rem; margin-bottom: 5px; display: block; }
        
        .btn-primary-action { background-color: var(--accent); color: #ffffff; font-weight: 600; padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; transition: background-color 0.2s; font-size: 0.95rem; display: inline-flex; align-items: center; gap: 8px; text-decoration: none; }
        .btn-primary-action:hover { background-color: var(--accent-hover); }
        .btn-danger-action { background-color: var(--error); color: #ffffff; font-weight: 600; padding: 8px 15px; border: none; border-radius: 6px; cursor: pointer; font-size: 0.85rem; }
        .btn-success-action { background-color: #10b981; color: #ffffff; font-weight: 600; padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-size: 0.95rem; }
        .btn-secondary-action { background-color: #e2e8f0; color: #475569; font-weight: 600; padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; font-size: 0.95rem; }
        .btn-info-action { background-color: #0ea5e9; color: white; border: none; border-radius: 8px; padding: 0 15px; cursor: pointer; font-size: 1rem; transition: 0.2s; }
        .btn-info-action:hover { background-color: #0284c7; }

        /* Modales */
        .modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 1000; display: none; justify-content: center; align-items: center; backdrop-filter: blur(2px); }
        .modal-overlay-top { z-index: 1050; }
        .modal-box { background: var(--bg-card); width: 95%; max-width: 800px; border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.1); overflow: hidden; display: flex; flex-direction: column; max-height: 90vh; }
        .modal-header { padding: 20px 30px; border-bottom: 1px solid #f1f5f9; display: flex; justify-content: space-between; align-items: center; background: var(--accent); color: white; }
        .modal-title { font-size: 1.2rem; font-weight: 700; }
        .modal-body { padding: 30px; overflow-y: auto; }
        .close-modal { background: none; border: none; font-size: 1.8rem; cursor: pointer; color: white; line-height: 1; }

        /* Layouts */
        .grid-3-col { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 15px; }
        .grid-2-col { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 15px; }
        .input-group-row { margin-bottom: 15px; }
        
        /* Tabla */
        .table-clean { width: 100%; border-collapse: collapse; }
        .table-clean th { text-align: left; padding: 12px 15px; color: var(--text-muted); font-weight: 600; border-bottom: 1px solid #e2e8f0; font-size: 0.9rem; }
        .table-clean td { padding: 12px 15px; border-bottom: 1px solid #f1f5f9; vertical-align: middle; font-size: 0.95rem; }
        .total-row { font-size: 1.5rem; font-weight: 700; text-align: right; padding-top: 20px; color: var(--accent); }

        /* Imágenes y Autocomplete */
        .preview-small { width: 60px; height: 60px; border-radius: 8px; object-fit: cover; border: 1px solid #e2e8f0; cursor: zoom-in; transition: transform 0.2s ease; }
        .preview-small:hover { transform: scale(1.05); }
        .upload-box { border: 1px dashed var(--border-input); background-color: #f8fafc; padding: 12px 15px; border-radius: 10px; display: flex; align-items: center; gap: 15px; }
        .swal-image-zoom { max-width: 600px !important; max-height: 80vh !important; object-fit: contain; border-radius: 8px; }
        
        .ui-autocomplete { z-index: 2000 !important; max-height: 250px; overflow-y: auto; overflow-x: hidden; font-family: 'Inter', sans-serif; font-size: 0.9rem; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.1); border: 1px solid #e2e8f0; }
        .ui-menu-item { padding: 10px; border-bottom: 1px solid #f1f5f9; cursor: pointer; }
        .ui-menu-item:hover { background-color: #f8fafc; }
    </style>

    <script type="text/javascript">
        // 1. Evitar submit con Enter
        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                var target = e.target;
                if (target.tagName === "TEXTAREA" || (target.tagName === "INPUT" && target.type === "submit")) return true;
                if (!document.getElementById('modalProd').contains(target) && !document.getElementById('modalNuevoProducto').contains(target)) {
                    e.preventDefault();
                }
            }
        });

        function saltoModal(e, siguienteID) {
            if (e.key === "Enter") {
                e.preventDefault();
                if (siguienteID === 'CLICK') {
                    var btn = document.getElementById('<%= btnModalAdd.ClientID %>');
                    if (btn) btn.click();
                } else {
                    var next = document.getElementById(siguienteID);
                    if (next) { next.focus(); if (next.type !== "button") next.select(); }
                }
            }
        }

        // 2. Zoom de imágenes y previsualización
        function verImagenFull(srcPath) {
            if (srcPath.includes("default-product.png") && srcPath.indexOf('data:image') === -1) return;
            Swal.fire({
                imageUrl: srcPath, imageAlt: 'Detalle del producto', showConfirmButton: false, showCloseButton: true,
                width: 'auto', padding: '1em', background: '#ffffff', backdrop: 'rgba(0,0,0,0.7)', customClass: { image: 'swal-image-zoom' }
            });
        }

        function previewImageNew(input) {
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) { document.getElementById('<%= imgNuevoProdPreview.ClientID %>').src = e.target.result; }
                reader.readAsDataURL(input.files[0]);
            }
        }

        // 3. Cálculos Dinámicos y Bidireccionales (Margen vs Precio)
        function calcularAutomatica() {
            var costo = parseFloat(document.getElementById('<%= txtPrecioUnitario.ClientID %>').value) || 0;
            var margenPct = parseFloat(document.getElementById('<%= txtMargen.ClientID %>').value) || 0;

            var iva = costo * 0.15;
            var costoTotal = costo + iva;

            var margenMult = 1 + (margenPct / 100);
            var precioSugerido = costoTotal * margenMult;

            document.getElementById('<%= txtIva.ClientID %>').value = iva.toFixed(2);
            document.getElementById('<%= txtPrecioVenta.ClientID %>').value = precioSugerido.toFixed(2);

            // Actualiza el texto de ayuda
            document.getElementById('lblMargenHelp').innerText = 'Margen del ' + margenPct + '% sugerido. Se actualizará en Inventario.';
        }

        function calcularMargenInverso() {
            var costo = parseFloat(document.getElementById('<%= txtPrecioUnitario.ClientID %>').value) || 0;
            var precioVenta = parseFloat(document.getElementById('<%= txtPrecioVenta.ClientID %>').value) || 0;

            var iva = costo * 0.15;
            var costoTotal = costo + iva;

            var margenReal = 0;
            if (costoTotal > 0) {
                margenReal = ((precioVenta / costoTotal) - 1) * 100;
                document.getElementById('<%= txtMargen.ClientID %>').value = margenReal.toFixed(2);
            } else {
                document.getElementById('<%= txtMargen.ClientID %>').value = "0.00";
            }

            // Actualiza el texto de ayuda con el margen exacto
            document.getElementById('lblMargenHelp').innerText = 'Margen real del ' + margenReal.toFixed(2) + '%. Se actualizará en Inventario.';
        }

        // 4. Autocomplete y AJAX
        function initAutocomplete() {
            $("#<%= txtBusquedaProducto.ClientID %>").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: "RegistrarCompra.aspx/BuscarProductos",
                        data: JSON.stringify({ prefijo: request.term }),
                        dataType: "json", type: "POST", contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            response($.map(data.d, function (item) {
                                return { label: item.label, val: item.val, desc: item.desc }
                            }))
                        }
                    });
                },
                select: function (e, i) {
                    $("#<%= hfIdProductoSeleccionado.ClientID %>").val(i.item.val);
                    $("#<%= hfNombreProductoSeleccionado.ClientID %>").val(i.item.desc);
                    buscarPreciosHistoricos(i.item.val);
                    setTimeout(function() { document.getElementById('<%= txtCantidad.ClientID %>').focus(); }, 100);
                },
                minLength: 2
            });
        }

        function buscarPreciosHistoricos(idProd) {
            $.ajax({
                url: "RegistrarCompra.aspx/ObtenerDetallesProducto",
                data: JSON.stringify({ idProducto: parseInt(idProd) }),
                dataType: "json", type: "POST", contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data.d) {
                        if(data.d.costo > 0) {
                            $("#<%= txtPrecioUnitario.ClientID %>").val(data.d.costo.toFixed(2));
                            calcularAutomatica();
                        }
                        if(data.d.precioVenta > 0) {
                            $("#<%= txtPrecioVenta.ClientID %>").val(data.d.precioVenta.toFixed(2));
                            calcularMargenInverso(); 
                        }
                    }
                }
            });
        }

        $(document).ready(function () {
            initAutocomplete();
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function () { initAutocomplete(); });
        });

        // 5. Manejo de Modales
        function openModal() { 
            document.getElementById('modalProd').style.display = 'flex'; 
            setTimeout(function() { document.getElementById('<%= txtBusquedaProducto.ClientID %>').focus(); }, 100);
        }
        function closeModal() { document.getElementById('modalProd').style.display = 'none'; return false; }

        function abrirModalNuevoProducto() {
            document.getElementById('modalNuevoProducto').style.display = 'flex';
            setTimeout(function() { document.getElementById('<%= txtCodNuevo.ClientID %>').focus(); }, 100);
            return false;
        }
        function cerrarModalNuevoProducto() { document.getElementById('modalNuevoProducto').style.display = 'none'; return false; }

        // 6. Confirmación Final
        var guardando = false;
        function confirmarGuardar(sender) {
            if (guardando) return true;
            Swal.fire({
                title: '¿Finalizar Compra?',
                text: "Se sumará el stock y se actualizarán los Precios Oficiales en el Inventario.",
                icon: 'question',
                showCancelButton: true, confirmButtonColor: '#10b981', cancelButtonColor: '#64748b',
                confirmButtonText: 'Sí, guardar', cancelButtonText: 'Revisar'
            }).then((result) => {
                if (result.isConfirmed) { guardando = true; sender.click(); }
            });
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="sm1" runat="server" EnablePageMethods="true"></asp:ScriptManager>

    <div class="empresa-wrapper">
        
        <div class="page-header">
            <div class="page-title"><i class="fa-solid fa-cart-flatbed" style="color: #64748b;"></i> Registrar Compra</div>
            
            <div style="display:flex; align-items:center; gap:20px; font-size:0.95rem; color:var(--text-muted);">
                <div><i class="fa-regular fa-user"></i> <asp:Label ID="lblUsuario" runat="server" Font-Bold="true"></asp:Label></div>
                <div><i class="fa-regular fa-calendar"></i> <asp:Label ID="lblFecha" runat="server" Font-Bold="true"></asp:Label></div>
                <asp:Button ID="btnVolver" runat="server" Text="Volver al Historial" CssClass="btn-secondary-action" OnClick="btnVolver_Click" CausesValidation="false" />
            </div>
        </div>

        <div class="ui-card">
            <h4 style="margin-top:0; margin-bottom: 15px; color: var(--text-main); border-bottom: 1px solid #f1f5f9; padding-bottom: 10px;">Datos de Facturación</h4>
            <div class="grid-3-col">
                <div class="input-group-row">
                    <span class="field-label">Nº Compra (Auto)</span>
                    <asp:TextBox ID="txtNumeroCompra" runat="server" CssClass="ui-input" ReadOnly="true"></asp:TextBox>
                </div>
                <div class="input-group-row">
                    <span class="field-label">Proveedor <span style="color:red">*</span></span>
                    <asp:DropDownList ID="ddlProveedor" runat="server" CssClass="ui-input"></asp:DropDownList>
                </div>
                <div class="input-group-row">
                    <span class="field-label">Nº Factura Física</span>
                    <asp:TextBox ID="txtRefFactura" runat="server" CssClass="ui-input" placeholder="Opcional"></asp:TextBox>
                </div>
            </div>
        </div>

        <div class="ui-card">
            <asp:UpdatePanel ID="upDetalle" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:15px;">
                        <h4 style="margin:0; color: var(--text-main);">Detalle de Productos</h4>
                        <asp:Button ID="btnAddItem" runat="server" Text="+ Agregar Producto" CssClass="btn-primary-action" OnClick="btnAddItem_Click" OnClientClick="openModal(); return false;" />
                    </div>

                    <asp:GridView ID="gvDetalle" runat="server" CssClass="table-clean" AutoGenerateColumns="False" OnRowCommand="gvDetalle_RowCommand" EmptyDataText="No hay productos en el carrito.">
                        <Columns>
                            <asp:BoundField DataField="Producto" HeaderText="PRODUCTO" />
                            <asp:BoundField DataField="Cantidad" HeaderText="CANT." ItemStyle-Font-Bold="true" />
                            <asp:BoundField DataField="PrecioUnitario" HeaderText="COSTO" DataFormatString="C$ {0:N2}" />
                            <asp:BoundField DataField="Iva" HeaderText="IVA" DataFormatString="C$ {0:N2}" />
                            <asp:BoundField DataField="PrecioTotal" HeaderText="SUBTOTAL" DataFormatString="C$ {0:N2}" ItemStyle-Font-Bold="true" />
                            <asp:BoundField DataField="PrecioVenta" HeaderText="P. VENTA (Oficial)" DataFormatString="C$ {0:N2}" ItemStyle-ForeColor="#059669" ItemStyle-Font-Bold="true" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnDel" runat="server" CommandName="Borrar" CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn-danger-action" ToolTip="Eliminar"><i class="fa-solid fa-trash"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <div class="total-row">Total Compra: <asp:Label ID="lblTotal" runat="server" Text="C$ 0.00"></asp:Label></div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <div style="margin-top:30px; text-align:right; padding-top:20px; border-top:1px solid #f1f5f9;">
                <asp:Button ID="btnGuardar" runat="server" Text="Finalizar Compra" CssClass="btn-success-action" OnClick="btnGuardar_Click" OnClientClick="return confirmarGuardar(this);" />
            </div>
        </div>
    </div>

    <div id="modalProd" class="modal-overlay">
        <div class="modal-box">
            <div class="modal-header">
                <span class="modal-title">Agregar Producto al Carrito</span>
                <button type="button" onclick="return closeModal()" class="close-modal">&times;</button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel ID="upModal" runat="server">
                    <ContentTemplate>
                        <div class="input-group-row">
                            <span class="field-label">Buscar Producto (Código o Nombre) <span style="color:red">*</span></span>
                            <div style="display: flex; gap: 10px;">
                                <asp:TextBox ID="txtBusquedaProducto" runat="server" CssClass="ui-input" autocomplete="off" placeholder="Escriba para buscar..."></asp:TextBox>
                                <button type="button" class="btn-info-action" onclick="return abrirModalNuevoProducto()" title="Crear Producto Nuevo"><i class="fa-solid fa-plus"></i></button>
                            </div>
                            <asp:HiddenField ID="hfIdProductoSeleccionado" runat="server" />
                            <asp:HiddenField ID="hfNombreProductoSeleccionado" runat="server" />
                        </div>

                        <div class="grid-2-col">
                            <div class="input-group-row">
                                <span class="field-label">Cantidad <span style="color:red">*</span></span>
                                <asp:TextBox ID="txtCantidad" runat="server" TextMode="Number" CssClass="ui-input" onkeydown="saltoModal(event, 'ContentPlaceHolder1_txtPrecioUnitario')" autocomplete="off"></asp:TextBox>
                            </div>
                            <div class="input-group-row">
                                <span class="field-label">Costo (Sin IVA) <span style="color:red">*</span></span>
                                <asp:TextBox ID="txtPrecioUnitario" runat="server" TextMode="Number" step="0.01" CssClass="ui-input" oninput="calcularAutomatica()" onkeydown="saltoModal(event, 'ContentPlaceHolder1_txtMargen')" autocomplete="off"></asp:TextBox>
                            </div>
                        </div>

                        <div style="display: grid; grid-template-columns: 1fr 1fr 1.5fr; gap: 15px; margin-bottom: 15px;">
                            <div class="input-group-row" style="margin-bottom:0;">
                                <span class="field-label">IVA (15%)</span>
                                <asp:TextBox ID="txtIva" runat="server" CssClass="ui-input" ReadOnly="true" Text="0.00"></asp:TextBox>
                            </div>
                            <div class="input-group-row" style="margin-bottom:0;">
                                <span class="field-label">% Margen</span>
                                <asp:TextBox ID="txtMargen" runat="server" TextMode="Number" step="0.01" CssClass="ui-input" Text="30" oninput="calcularAutomatica()" onkeydown="saltoModal(event, 'ContentPlaceHolder1_txtPrecioVenta')" autocomplete="off"></asp:TextBox>
                            </div>
                            <div class="input-group-row" style="margin-bottom:0;">
                                <span class="field-label">P. Venta Oficial <span style="color:red">*</span></span>
                                <asp:TextBox ID="txtPrecioVenta" runat="server" TextMode="Number" step="0.01" CssClass="ui-input" oninput="calcularMargenInverso()" onkeydown="saltoModal(event, 'CLICK')" autocomplete="off"></asp:TextBox>
                                <small id="lblMargenHelp" style="color:var(--success-text); font-size:0.75rem; font-weight:600;">Margen del 30% sugerido. Se actualizará en Inventario.</small>
                            </div>
                        </div>

                        <asp:Label ID="lblErrorModal" runat="server" ForeColor="#ef4444" Visible="false" style="display:block; margin-bottom:15px; font-weight:600; text-align:center;"></asp:Label>
                        <asp:Button ID="btnModalAdd" runat="server" Text="Agregar a la Lista" CssClass="btn-primary-action" Width="100%" style="justify-content:center;" OnClick="btnModalAdd_Click" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <div id="modalNuevoProducto" class="modal-overlay modal-overlay-top">
        <div class="modal-box">
            <div class="modal-header">
                <span class="modal-title">Crear Producto Rápido</span>
                <button type="button" onclick="return cerrarModalNuevoProducto()" class="close-modal">&times;</button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel ID="upNuevoProd" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="grid-2-col">
                            <div class="input-group-row"><span class="field-label">Código <span style="color:red">*</span></span><asp:TextBox ID="txtCodNuevo" runat="server" CssClass="ui-input" autocomplete="off"></asp:TextBox></div>
                            <div class="input-group-row"><span class="field-label">Categoría <span style="color:red">*</span></span><asp:DropDownList ID="ddlCatNueva" runat="server" CssClass="ui-input"></asp:DropDownList></div>
                        </div>

                        <div class="input-group-row"><span class="field-label">Descripción <span style="color:red">*</span></span><asp:TextBox ID="txtDescNueva" runat="server" CssClass="ui-input" autocomplete="off"></asp:TextBox></div>

                        <div class="grid-2-col">
                            <div class="input-group-row"><span class="field-label">Marca</span><asp:TextBox ID="txtMarcaNueva" runat="server" CssClass="ui-input" autocomplete="off"></asp:TextBox></div>
                            <div class="input-group-row"><span class="field-label">Modelo</span><asp:TextBox ID="txtModeloNuevo" runat="server" CssClass="ui-input" autocomplete="off"></asp:TextBox></div>
                        </div>
                        <div class="grid-2-col">
                            <div class="input-group-row"><span class="field-label">Tipo Aro</span><asp:TextBox ID="txtAroNuevo" runat="server" CssClass="ui-input" autocomplete="off"></asp:TextBox></div>
                            <div class="input-group-row"><span class="field-label">Color</span><asp:TextBox ID="txtColorNuevo" runat="server" CssClass="ui-input" autocomplete="off"></asp:TextBox></div>
                        </div>

                        <div class="input-group-row">
                            <span class="field-label">Imagen del Producto</span>
                            <div class="upload-box">
                                <asp:Image ID="imgNuevoProdPreview" runat="server" CssClass="preview-small" title="Click para ampliar" onclick="verImagenFull(this.src)" ImageUrl="~/Images/default-product.png" />
                                <div style="flex-grow: 1;"><asp:FileUpload ID="fuImagenNueva" runat="server" CssClass="ui-input" style="padding-top: 9px;" onchange="previewImageNew(this);" /></div>
                            </div>
                        </div>

                        <asp:Label ID="lblErrorNuevoProd" runat="server" ForeColor="#ef4444" Visible="false" style="display:block; margin-bottom:15px; font-weight:600; text-align:center;"></asp:Label>
                        <asp:Button ID="btnGuardarNuevoProd" runat="server" Text="Guardar y Seleccionar" CssClass="btn-success-action" Width="100%" style="justify-content:center;" OnClick="btnGuardarNuevoProd_Click" />
                    </ContentTemplate>
                    <Triggers><asp:PostBackTrigger ControlID="btnGuardarNuevoProd" /></Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>