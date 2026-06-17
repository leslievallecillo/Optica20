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
    </style>
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
                    <asp:DropDownList ID="ddlTipoEtiqueta" runat="server" CssClass="form-control-std">
                        <asp:ListItem Value="Precio">Precio</asp:ListItem>
                        <asp:ListItem Value="Etiqueta">Etiqueta de Texto</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group form-full">
                    <label>Título / Modelo</label>
                    <asp:TextBox ID="txtTitulo" runat="server" CssClass="form-control-std" placeholder="Ej: Modelo Elegante"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Valor (Precio o Etiqueta)</label>
                    <asp:TextBox ID="txtPrecio" runat="server" CssClass="form-control-std" placeholder="Ej: 1250 o ¡Aprovecha!"></asp:TextBox>
                </div>
                <div class="form-group form-full">
                    <label>Descripción Corta</label>
                    <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control-std" placeholder="Ej: Diseño sofisticado"></asp:TextBox>
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
</asp:Content>