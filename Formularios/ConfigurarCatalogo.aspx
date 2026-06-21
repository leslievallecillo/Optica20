<%@ Page Title="Configurar Catálogo" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ConfigurarCatalogo.aspx.cs" Inherits="Optica.Formularios.ConfigurarCatalogo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        .panel-card { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); margin-bottom: 20px; }
        .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 15px; margin-bottom: 20px; }
        .form-full { grid-column: 1 / -1; }
        .form-group label { display: block; font-weight: 600; color: #495057; margin-bottom: 5px; font-size: 0.9rem; }
        .form-control-std { width: 100%; padding: 8px 12px; border: 1px solid #ced4da; border-radius: 4px; box-sizing: border-box; }
        .btn-std { padding: 8px 15px; border: none; border-radius: 4px; color: white; cursor: pointer; font-weight: 500; }
        .btn-success { background: #28a745; }
        .btn-primary { background: #0056b3; }
        .btn-danger { background: #dc3545; }
        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; margin-top: 15px; }
        .table-std th { background: #f8f9fa; padding: 10px; text-align: left; border-bottom: 2px solid #dee2e6; }
        .table-std td { padding: 10px; border-bottom: 1px solid #dee2e6; vertical-align: middle; }
        .img-preview { width: 60px; height: 60px; object-fit: cover; border-radius: 4px; border: 1px solid #ddd; }
        /* Estilos para mensajes de error en los campos */
        .field-error { border-color: #dc3545 !important; }
        .error-message { color: #dc3545; font-size: 0.8rem; margin-top: 3px; display: block; }
    </style>

    <script type="text/javascript">
        // Evitar que Enter dispare el botón guardar y que solo navegue entre campos
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                var target = event.target;
                if (target.tagName === 'INPUT' || target.tagName === 'SELECT' || target.tagName === 'TEXTAREA') {
                    event.preventDefault();
                    var form = target.form;
                    if (form) {
                        var inputs = form.querySelectorAll('input, select, textarea');
                        var currentIndex = Array.from(inputs).indexOf(target);
                        if (currentIndex < inputs.length - 1) {
                            inputs[currentIndex + 1].focus();
                        }
                    }
                }
            }
        });

        function validarCampo(input, tipo) {
            var valor = input.value;
            var errorSpan = document.getElementById(input.id + '_error');
            if (!errorSpan) {
                errorSpan = document.createElement('span');
                errorSpan.id = input.id + '_error';
                errorSpan.className = 'error-message';
                input.parentNode.appendChild(errorSpan);
            }

            // Limpiar error previo
            input.classList.remove('field-error');
            errorSpan.textContent = '';

            if (tipo === 'titulo') {
                // No permitir números
                if (/\d/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'El título no puede contener números.';
                    return false;
                }
                // No permitir más de 1 espacio seguido
                if (/\s{2,}/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permite más de un espacio seguido por palabra.';
                    return false;
                }
                // No permitir 3 o más letras repetidas
                if (/([a-zA-ZáéíóúÁÉÍÓÚñÑ])\1{2,}/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permite repetir la misma letra 3 o más veces seguidas.';
                    return false;
                }
                // Solo permitir letras, espacios y ! ¡ ? ¿
                if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s!¡?¿]*$/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permiten números ni símbolos especiales, a excepción de ! ¡ ? ¿';
                    return false;
                }
            }

            if (tipo === 'etiqueta') {
                // No permitir 3 o más letras repetidas
                if (/([a-zA-ZáéíóúÁÉÍÓÚñÑ])\1{2,}/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permite repetir la misma letra 3 o más veces seguidas.';
                    return false;
                }
                // No permitir más de 1 espacio seguido
                if (/\s{2,}/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permite más de un espacio seguido por palabra.';
                    return false;
                }
                // Solo permitir letras, números, espacios y ! ¡ ? ¿
                if (!/^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s!¡?¿]*$/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'Solo se permiten letras, números, espacios y ! ¡ ? ¿';
                    return false;
                }
            }

            if (tipo === 'descripcion') {
                // Límite de 100 caracteres
                if (valor.length > 100) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'La descripción no puede exceder los 100 caracteres.';
                    return false;
                }
                // No permitir números
                if (/\d/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'La descripción no puede contener números.';
                    return false;
                }
                // No permitir más de 1 espacio seguido
                if (/\s{2,}/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permite más de un espacio seguido por palabra.';
                    return false;
                }
                // No permitir 3 o más letras repetidas
                if (/([a-zA-ZáéíóúÁÉÍÓÚñÑ])\1{2,}/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permite repetir la misma letra 3 o más veces seguidas.';
                    return false;
                }
                // Solo permitir letras, espacios y ! ¡ ? ¿
                if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s!¡?¿]*$/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'No se permiten números ni símbolos especiales, a excepción de ! ¡ ? ¿';
                    return false;
                }
            }

            if (tipo === 'precio') {
                // Solo permitir números
                if (!/^\d*$/.test(valor)) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'Solo se permiten números.';
                    return false;
                }
                // Validar que no exceda 4000
                if (valor && parseInt(valor) > 4000) {
                    input.classList.add('field-error');
                    errorSpan.textContent = 'El precio no puede exceder 4000 C$.';
                    return false;
                }
            }

            return true;
        }

        function validarTitulo(input) { validarCampo(input, 'titulo'); }
        function validarDescripcion(input) { validarCampo(input, 'descripcion'); }
        function validarPrecio(input) { validarCampo(input, 'precio'); }
        function validarEtiqueta(input) { validarCampo(input, 'etiqueta'); }

        function validarTipoEtiqueta() {
            var tipo = document.getElementById('<%= ddlTipoEtiqueta.ClientID %>').value;
            var inputPrecio = document.getElementById('<%= txtPrecio.ClientID %>');
            var errorSpan = document.getElementById(inputPrecio.id + '_error');
            if (!errorSpan) {
                errorSpan = document.createElement('span');
                errorSpan.id = inputPrecio.id + '_error';
                errorSpan.className = 'error-message';
                inputPrecio.parentNode.appendChild(errorSpan);
            }

            inputPrecio.classList.remove('field-error');
            errorSpan.textContent = '';
            inputPrecio.value = '';

            if (tipo === 'Precio') {
                inputPrecio.placeholder = 'Ej: 1250';
                inputPrecio.oninput = function () { validarPrecio(this); };
                // Remover validación de etiqueta
                inputPrecio.onkeypress = function (e) {
                    var charCode = e.which ? e.which : e.keyCode;
                    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
                        e.preventDefault();
                        inputPrecio.classList.add('field-error');
                        errorSpan.textContent = 'Solo se permiten números.';
                    } else {
                        inputPrecio.classList.remove('field-error');
                        errorSpan.textContent = '';
                    }
                };
            } else {
                inputPrecio.placeholder = 'Ej: ¡Aprovecha!';
                inputPrecio.oninput = function () { validarEtiqueta(this); };
                // Remover restricción de solo números
                inputPrecio.onkeypress = null;
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="max-width: 1200px; margin: 0 auto;">
        <h2 style="color: #333; margin-bottom: 20px;"><i class="fa-solid fa-images"></i> Configurar Catálogo Web</h2>

        <div class="panel-card">
            <asp:HiddenField ID="hfIDImagen" runat="server" />
            <div class="form-grid">
                <div class="form-group">
                    <label>Sección del Catálogo</label>
                    <asp:DropDownList ID="ddlSeccion" runat="server" CssClass="form-control-std">
                        <asp:ListItem Value="Destacados">Los más destacados</asp:ListItem>
                        <asp:ListItem Value="Promociones">Promociones Especiales</asp:ListItem>
                        <asp:ListItem Value="Otros">Otros Productos</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <label>Tipo de Etiqueta</label>
                    <asp:DropDownList ID="ddlTipoEtiqueta" runat="server" CssClass="form-control-std" onchange="validarTipoEtiqueta();">
                        <asp:ListItem Value="Precio">Precio</asp:ListItem>
                        <asp:ListItem Value="Etiqueta">Etiqueta de Texto</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group form-full">
                    <label>Título / Modelo</label>
                    <asp:TextBox ID="txtTitulo" runat="server" CssClass="form-control-std" placeholder="Ej: Modelo Elegante" oninput="validarTitulo(this);"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Valor (Precio o Etiqueta)</label>
                    <asp:TextBox ID="txtPrecio" runat="server" CssClass="form-control-std" placeholder="Ej: 1250 o ¡Aprovecha!" oninput="validarTipoEtiqueta();"></asp:TextBox>
                </div>
                <div class="form-group form-full">
                    <label>Descripción Corta</label>
                    <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control-std" placeholder="Ej: Diseño sofisticado" oninput="validarDescripcion(this);"></asp:TextBox>
                </div>
                <div class="form-group form-full">
                    <label>URL de la Imagen Externa</label>
                    <asp:TextBox ID="txtUrlImagen" runat="server" CssClass="form-control-std" placeholder="https://... (.png, .jpg, .jpeg)"></asp:TextBox>
                </div>
            </div>
            <div style="text-align: right;">
                <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar" CssClass="btn-std btn-primary" OnClick="btnLimpiar_Click" CausesValidation="false" />
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar en Catálogo" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" />
            </div>
        </div>

        <div class="panel-card" style="overflow-x: auto;">
            <h3 style="margin-top: 0;">Imágenes Actuales en Catálogo</h3>
            <asp:GridView ID="gvCatalogo" runat="server" CssClass="table-std" AutoGenerateColumns="False" OnRowCommand="gvCatalogo_RowCommand" GridLines="None">
                <Columns>
                    <asp:BoundField DataField="Seccion" HeaderText="Sección" />
                    <asp:TemplateField HeaderText="Imagen">
                        <ItemTemplate>
                            <img src='<%# Eval("UrlImagen") %>' class="img-preview" alt="Img" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Titulo" HeaderText="Título" />
                    <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                    <asp:BoundField DataField="Precio" HeaderText="Precio/Etiqueta" />
                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Imagen") %>' CssClass="btn-std btn-primary" style="padding: 5px 10px;"><i class="fa-solid fa-pen"></i></asp:LinkButton>
                            <asp:LinkButton ID="btnEliminar" runat="server" CommandName="Eliminar" CommandArgument='<%# Eval("ID_Imagen") %>' CssClass="btn-std btn-danger" style="padding: 5px 10px;" OnClientClick="return confirm('¿Seguro que desea eliminar esta imagen del catálogo?');"><i class="fa-solid fa-trash"></i></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
    <script type="text/javascript">
     
        document.addEventListener('DOMContentLoaded', function () {
            validarTipoEtiqueta();
        });
    </script>
</asp:Content>