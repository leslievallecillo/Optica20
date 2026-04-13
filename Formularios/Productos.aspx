<%@ Page Title="Gestión de Productos" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Productos.aspx.cs" Inherits="Optica.Formularios.Productos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
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
        
        .ui-input { width: 100%; padding: 0 15px; height: 42px; font-size: 0.95rem; color: var(--text-main); border: 1px solid var(--border-input); border-radius: 8px; font-family: 'Inter', sans-serif; box-sizing: border-box; }
        .ui-input:focus { outline: none; border-color: var(--accent); box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15); }
        .field-label { font-weight: 500; color: var(--text-muted); font-size: 0.9rem; margin-bottom: 5px; display: block; }
        
        .btn-primary-action { background-color: var(--accent); color: #ffffff; font-weight: 600; padding: 10px 20px; border: none; border-radius: 8px; cursor: pointer; transition: background-color 0.2s; font-size: 0.95rem; text-decoration: none; display: inline-block; }
        .btn-primary-action:hover { background-color: var(--accent-hover); }
        .btn-danger-action { background-color: var(--error); color: #ffffff; font-weight: 600; padding: 6px 12px; border: none; border-radius: 6px; cursor: pointer; font-size: 0.85rem; display: inline-block; }

        .modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 1000; display: none; justify-content: center; align-items: center; backdrop-filter: blur(2px); }
        .modal-box { background: var(--bg-card); width: 95%; max-width: 800px; border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.1); overflow: hidden; display: flex; flex-direction: column; max-height: 90vh; }
        .modal-header { padding: 20px 30px; border-bottom: 1px solid #f1f5f9; display: flex; justify-content: space-between; align-items: center; }
        .modal-title { font-size: 1.2rem; font-weight: 700; color: var(--text-main); }
        .modal-body { padding: 30px; overflow-y: auto; }
        .close-modal { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: var(--text-muted); }

        .grid-filters { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin-bottom: 20px; align-items: end; }
        .input-group-row { margin-bottom: 15px; }
        .grid-2-col { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
        
        /* Contenedor responsive para la tabla */
        .table-responsive { width: 100%; overflow-x: auto; -webkit-overflow-scrolling: touch; }
        
        .table-clean { width: 100%; border-collapse: collapse; white-space: nowrap; }
        .table-clean th { text-align: left; padding: 12px 15px; color: var(--text-muted); font-weight: 600; border-bottom: 1px solid #e2e8f0; font-size: 0.9rem; }
        .table-clean td { padding: 12px 15px; border-bottom: 1px solid #f1f5f9; vertical-align: middle; font-size: 0.95rem; }
        .status-pill { background-color: var(--success-bg); color: var(--success-text); font-weight: 600; font-size: 0.75rem; padding: 4px 12px; border-radius: 50px; }
        .status-inactive { background-color: #f1f5f9; color: var(--text-muted); }
        
        .action-buttons-container { display: flex; gap: 8px; }

        .img-thumb { width: 40px; height: 40px; object-fit: cover; border-radius: 6px; border: 1px solid #e2e8f0; cursor: zoom-in; transition: transform 0.2s ease; }
        .img-thumb:hover { transform: scale(1.1); box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
        .preview-small { width: 60px; height: 60px; border-radius: 8px; object-fit: cover; border: 1px solid #e2e8f0; cursor: zoom-in; transition: transform 0.2s ease; }
        .preview-small:hover { transform: scale(1.05); }
        
        .upload-box { border: 1px dashed var(--border-input); background-color: #f8fafc; padding: 12px 15px; border-radius: 10px; display: flex; align-items: center; gap: 15px; }
        
        .swal-image-zoom { max-width: 600px !important; max-height: 80vh !important; object-fit: contain; border-radius: 8px; }

        /* --- MEDIA QUERIES RESPONSIVE --- */
        @media (max-width: 768px) {
            .empresa-wrapper { padding: 15px; }
            .page-header { flex-direction: column; align-items: flex-start; gap: 15px; }
            .btn-primary-action { width: 100%; text-align: center; }
            .ui-card { padding: 15px; }
            .grid-2-col { grid-template-columns: 1fr; gap: 15px; }
            .modal-header { padding: 15px 20px; }
            .modal-body { padding: 15px 20px; }
            .upload-box { flex-direction: column; align-items: flex-start; text-align: center; }
            .preview-small { align-self: center; }
        }
    </style>

    <script type="text/javascript">
        function openModal() { document.getElementById('modalProd').style.display = 'flex'; }
        function closeModal() { document.getElementById('modalProd').style.display = 'none'; return false; }

        function previewImage(input) {
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) { document.getElementById('<%= imgProductoPreview.ClientID %>').src = e.target.result; }
                reader.readAsDataURL(input.files[0]);
            }
        }

        function verImagenFull(srcPath) {
            if (srcPath.includes("default-product.png") && srcPath.indexOf('data:image') === -1) {
                return;
            }

            Swal.fire({
                imageUrl: srcPath,
                imageAlt: 'Detalle del producto',
                showConfirmButton: false,
                showCloseButton: true,
                width: 'auto',
                padding: '1em',
                background: '#ffffff',
                backdrop: 'rgba(0,0,0,0.7)',
                customClass: {
                    image: 'swal-image-zoom'
                }
            });
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="sm" runat="server"></asp:ScriptManager>

    <div class="empresa-wrapper">
        <div class="page-header">
            <div class="page-title">
                <i class="fas fa-boxes" style="color: #64748b;"></i> Gestión de Productos
            </div>
            <asp:Button ID="btnNuevo" runat="server" Text="+ Nuevo Producto" CssClass="btn-primary-action" OnClick="btnNuevo_Click" />
        </div>

        <div class="ui-card">
            <div class="grid-filters">
                <div>
                    <span class="field-label">Buscar</span>
                    <asp:TextBox ID="txtBuscar" runat="server" CssClass="ui-input" placeholder="Código, nombre..."></asp:TextBox>
                </div>
                <div>
                    <span class="field-label">Categoría</span>
                    <asp:DropDownList ID="ddlFiltroCat" runat="server" CssClass="ui-input"></asp:DropDownList>
                </div>
                <div>
                    <span class="field-label">Estado</span>
                    <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="ui-input">
                        <asp:ListItem Value="-1">Todos</asp:ListItem>
                        <asp:ListItem Value="1" Selected="True">Activo</asp:ListItem>
                        <asp:ListItem Value="0">Inactivo</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div style="display:none;">
                     <asp:TextBox ID="txtFecIni" runat="server"></asp:TextBox>
                     <asp:TextBox ID="txtFecFin" runat="server"></asp:TextBox>
                </div>
                <div>
                    <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-primary-action" OnClick="btnBuscar_Click" />
                </div>
            </div>

            <asp:UpdatePanel ID="upGrid" runat="server">
                <ContentTemplate>
                    <div class="table-responsive">
                        <asp:GridView ID="gvProductos" runat="server" CssClass="table-clean" AutoGenerateColumns="False"
                            OnRowCommand="gvProductos_RowCommand" DataKeyNames="ID_Producto" AllowPaging="True" PageSize="8"
                            OnPageIndexChanging="gvProductos_PageIndexChanging" GridLines="None" EmptyDataText="No se encontraron productos.">
                            <Columns>
                                <asp:BoundField DataField="Codigo" HeaderText="CÓDIGO" ItemStyle-Font-Bold="true" />
                                
                                <asp:TemplateField HeaderText="IMAGEN">
                                    <ItemTemplate>
                                        <asp:Image ID="imgProd" runat="server" CssClass="img-thumb" title="Click para ampliar" onclick="verImagenFull(this.src)"
                                            ImageUrl='<%# string.IsNullOrEmpty(Eval("RutaImagen").ToString()) ? "~/Images/default-product.png" : Eval("RutaImagen") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="Categoria" HeaderText="CATEGORÍA" />
                                <asp:BoundField DataField="Descripcion" HeaderText="DESCRIPCIÓN" />
                                <asp:BoundField DataField="Marca" HeaderText="MARCA" NullDisplayText="-" />
                                <asp:BoundField DataField="Precio" HeaderText="PRECIO" DataFormatString="C$ {0:N2}" ItemStyle-Font-Bold="true" ItemStyle-ForeColor="#059669" ItemStyle-Wrap="false" />
                                <asp:BoundField DataField="Stock" HeaderText="STOCK" ItemStyle-Font-Bold="true" ItemStyle-ForeColor="#3b82f6" />
                                <asp:TemplateField HeaderText="ESTADO">
                                    <ItemTemplate>
                                        <span class='status-pill <%# Convert.ToBoolean(Eval("Estado")) ? "" : "status-inactive" %>'>
                                            <%# Convert.ToBoolean(Eval("Estado")) ? "Activo" : "Inactivo" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="ACCIONES">
                                    <ItemTemplate>
                                        <div class="action-buttons-container">
                                            <asp:LinkButton ID="btnEdit" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Producto") %>' CssClass="btn-primary-action" style="padding: 6px 12px; font-size:0.8rem;"><i class="fas fa-edit"></i></asp:LinkButton>
                                            <asp:LinkButton ID="btnDel" runat="server" CommandName="DarBaja" CommandArgument='<%# Eval("ID_Producto") %>' CssClass="btn-danger-action" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>' OnClientClick="return confirm('¿Desactivar?');"><i class="fas fa-ban"></i></asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div id="modalProd" class="modal-overlay">
        <div class="modal-box">
            <div class="modal-header">
                <span class="modal-title">Detalles del Producto</span>
                <button onclick="return closeModal()" class="close-modal">&times;</button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel ID="upModal" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:HiddenField ID="hfIdProducto" runat="server" />

                        <div class="grid-2-col">
                            <div class="input-group-row">
                                <span class="field-label">Categoría</span>
                                <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="ui-input" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoria_SelectedIndexChanged"></asp:DropDownList>
                            </div>
                            <div class="input-group-row">
                                <span class="field-label">Código</span>
                                <asp:TextBox ID="txtCodigo" runat="server" CssClass="ui-input"></asp:TextBox>
                            </div>
                        </div>

                        <div class="input-group-row">
                            <span class="field-label">Descripción</span>
                            <asp:TextBox ID="txtDescripcion" runat="server" CssClass="ui-input"></asp:TextBox>
                        </div>

                        <asp:Panel ID="pnlDetallesOptica" runat="server" Visible="false" style="background:#f8fafc; padding:15px; border-radius:8px; margin-bottom:15px; border:1px solid #e2e8f0;">
                            <div class="grid-2-col">
                                <div><span class="field-label">Marca</span><asp:TextBox ID="txtMarca" runat="server" CssClass="ui-input"></asp:TextBox></div>
                                <div><span class="field-label">Modelo</span><asp:TextBox ID="txtModelo" runat="server" CssClass="ui-input"></asp:TextBox></div>
                                <div><span class="field-label">Tipo Aro</span><asp:TextBox ID="txtTipoAro" runat="server" CssClass="ui-input"></asp:TextBox></div>
                                <div><span class="field-label">Color</span><asp:TextBox ID="txtColor" runat="server" CssClass="ui-input"></asp:TextBox></div>
                            </div>
                        </asp:Panel>

                        <div class="grid-2-col">
                            <div class="input-group-row">
                                <span class="field-label">Precio Venta (C$)</span>
                                <asp:TextBox ID="txtPrecio" runat="server" TextMode="Number" step="0.01" CssClass="ui-input" placeholder="0.00"></asp:TextBox>
                            </div>
                            <div class="input-group-row">
                                <span class="field-label">Stock</span>
                                <asp:TextBox ID="txtStock" runat="server" TextMode="Number" CssClass="ui-input"></asp:TextBox>
                            </div>
                        </div>
                        
                        <div class="input-group-row">
                            <span class="field-label">Fecha Registro</span>
                            <asp:TextBox ID="txtFechaReg" runat="server" CssClass="ui-input" ReadOnly="true" style="background-color:#f1f5f9; color:#64748b;"></asp:TextBox>
                        </div>

                        <div class="input-group-row">
                            <span class="field-label">Imagen del Producto</span>
                            <div class="upload-box">
                                <asp:Image ID="imgProductoPreview" runat="server" CssClass="preview-small" title="Click para ampliar" onclick="verImagenFull(this.src)" ImageUrl="~/Images/default-product.png" />
                                <div style="flex-grow: 1; width: 100%;">
                                    <asp:FileUpload ID="fuImagen" runat="server" CssClass="ui-input" style="padding-top: 9px;" onchange="previewImage(this);" />
                                </div>
                            </div>
                        </div>

                        <div style="text-align: right; margin-top: 20px;">
                            <asp:Button ID="btnGuardar" runat="server" Text="Guardar Producto" CssClass="btn-primary-action" OnClick="btnGuardar_Click" />
                        </div>
                    </ContentTemplate>
                    <Triggers>
                         <asp:PostBackTrigger ControlID="btnGuardar" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>