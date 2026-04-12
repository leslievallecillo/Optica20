<%@ Page Title="Gestión de Citas" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Cita.aspx.cs" Inherits="Optica.Formularios.Cita" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        :root {
            --color-primary: #0056b3;
            --color-danger: #d32f2f;
            --color-border: #ced4da;
            --bg-light: #f8f9fa;
        }

        .form-group-formal {
            margin-bottom: 25px;
            position: relative;
        }

        .lbl-formal {
            font-weight: 700;
            font-size: 0.9rem;
            color: #2c3e50;
            display: block;
            margin-bottom: 5px;
        }

        .required-mark {
            color: var(--color-danger);
            margin-left: 3px;
        }

        .input-wrapper {
            position: relative;
            width: 100%;
        }

        .form-control-formal {
            width: 100%;
            padding: 10px 35px 10px 12px;
            font-size: 1rem;
            border: 1px solid var(--color-border);
            border-radius: 4px;
            transition: all 0.3s;
            background-color: #fff;
            box-sizing: border-box;
        }

        .form-control-formal:focus {
            border-color: var(--color-primary);
            box-shadow: 0 0 0 3px rgba(0, 86, 179, 0.1);
            outline: none;
        }

        .form-control-formal.is-invalid {
            border-color: var(--color-danger);
            background-color: #fff8f8;
        }

        .input-error-icon {
            position: absolute;
            right: 12px;
            top: 50%;
            transform: translateY(-50%);
            color: var(--color-danger);
            font-size: 1.1rem;
            display: none;
            pointer-events: none;
        }

        .is-invalid + .input-error-icon {
            display: block;
        }

        .error-message-formal {
            color: var(--color-danger);
            font-size: 0.8rem;
            margin-top: 5px;
            display: block;
            font-weight: 600;
        }

        .help-text-formal {
            font-size: 0.75rem;
            color: #636c72;
            margin-top: 4px;
            display: block;
        }

        .input-group-formal {
            display: flex;
            width: 100%;
            position: relative;
        }
        .input-group-formal .form-control-formal {
            border-radius: 4px 0 0 4px;
        }
        .input-group-formal .combo-append {
            width: 80px;
            border-radius: 0 4px 4px 0;
            border-left: 0;
            background-color: #f1f3f5;
        }

        /* Estilos para los checkboxes de motivos */
        .motivos-container {
            border: 1px solid var(--color-border);
            border-radius: 8px;
            padding: 15px;
            background-color: #fafafa;
            max-height: 300px;
            overflow-y: auto;
        }

        .motivos-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
            gap: 10px;
        }

        .motivo-item {
            display: flex;
            align-items: center;
            gap: 8px;
            cursor: pointer;
            padding: 6px 10px;
            border-radius: 6px;
            transition: all 0.2s ease;
            background-color: #fff;
            border: 1px solid #e9ecef;
        }

        .motivo-item:hover {
            background-color: #e8f0fe;
            border-color: var(--color-primary);
            transform: translateX(2px);
        }

        .motivo-item input[type="checkbox"] {
            width: 18px;
            height: 18px;
            cursor: pointer;
            accent-color: var(--color-primary);
            margin: 0;
        }

        .motivo-item label {
            margin: 0;
            cursor: pointer;
            font-weight: normal;
            font-size: 0.9rem;
            color: #495057;
            flex: 1;
        }

        .motivo-item label:hover {
            color: var(--color-primary);
        }

        .motivo-seleccionado {
            background-color: #e3f2fd !important;
            border-left: 3px solid var(--color-primary);
            border-color: var(--color-primary);
        }

        .motivo-otros {
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px dashed var(--color-border);
        }

        .motivo-otros-checkbox {
            margin-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .motivo-otros-checkbox input[type="checkbox"] {
            width: 18px;
            height: 18px;
            cursor: pointer;
            accent-color: var(--color-primary);
        }

        .motivo-otros-checkbox label {
            margin: 0;
            cursor: pointer;
            font-weight: 600;
            color: var(--color-primary);
        }

        .otros-texto {
            margin-left: 28px;
            display: none;
        }

        .otros-texto textarea {
            width: 100%;
            padding: 8px 12px;
            border: 1px solid var(--color-border);
            border-radius: 4px;
            font-family: inherit;
            resize: vertical;
            font-size: 0.9rem;
        }

        .otros-texto textarea:focus {
            border-color: var(--color-primary);
            outline: none;
            box-shadow: 0 0 0 2px rgba(0, 86, 179, 0.1);
        }

        .panel-card {
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.05);
            margin-bottom: 20px;
            border: 1px solid #eaeaea;
        }

        .panel-header {
            padding: 15px 20px;
            border-bottom: 1px solid #eaeaea;
            display: flex;
            justify-content: space-between;
            align-items: center;
            background-color: var(--bg-light);
            border-radius: 8px 8px 0 0;
        }

        .panel-body { padding: 25px; }

        .toolbar {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: flex-end;
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 6px;
            border: 1px solid var(--color-border);
            margin-bottom: 20px;
        }

        .form-grid-layout {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }
        .form-full-width { grid-column: 1 / -1; }

        .table-std { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .table-std thead th { background-color: #f1f3f5; padding: 12px; text-align: left; border-bottom: 2px solid #ddd; }
        .table-std tbody td { padding: 10px; border-bottom: 1px solid #eee; }
        
        .badge-status { padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: bold; text-transform: uppercase; }
        .badge-active { background-color: #e8f5e9; color: #2e7d32; }
        .badge-inactive { background-color: #ffebee; color: #c62828; }

        .btn-std { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; gap: 5px; font-size: 0.9rem; transition: 0.2s; }
        .btn-primary { background-color: var(--color-primary); color: white; }
        .btn-success { background-color: #28a745; color: white; }
        .btn-danger { background-color: var(--color-danger); color: white; }
        .btn-secondary { background-color: #6c757d; color: white; }
        .btn-info { background-color: #17a2b8; color: white; }
        .btn-sm { padding: 4px 8px; font-size: 0.85rem; }

        .alert-info-custom {
            background-color: #e3f2fd;
            color: #0d47a1;
            padding: 10px;
            border-radius: 4px;
            margin-top: 10px;
            font-size: 0.9rem;
            text-align: center;
            border: 1px solid #bbdefb;
        }

        /* Scrollbar personalizado */
        .motivos-container::-webkit-scrollbar {
            width: 6px;
        }
        .motivos-container::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 3px;
        }
        .motivos-container::-webkit-scrollbar-thumb {
            background: #c1c1c1;
            border-radius: 3px;
        }
        .motivos-container::-webkit-scrollbar-thumb:hover {
            background: #a8a8a8;
        }
    </style>

    <script type="text/javascript">
        document.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                var target = e.target;
                if (target.tagName === "TEXTAREA" || (target.tagName === "INPUT" && target.type === "submit")) {
                    return true;
                }
                e.preventDefault();
                var form = target.form;
                if (form) {
                    var elements = Array.from(form.querySelectorAll('input:not([type=hidden]):not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled])'));
                    var index = elements.indexOf(target);
                    if (index > -1 && index < elements.length - 1) {
                        var nextElement = elements[index + 1];
                        if (nextElement.value === "Cancelar" || nextElement.classList.contains("btn-secondary")) {
                            if (index + 2 < elements.length) { elements[index + 2].focus(); }
                        } else {
                            nextElement.focus();
                        }
                    }
                }
            }
        });

        var guardando = false;
        function confirmarGuardar(sender) {
            if (guardando) return true;
            Swal.fire({
                title: '¿Desea guardar?',
                text: "Verifique los datos.",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Sí, guardar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    guardando = true;
                    sender.click();
                }
            });
            return false;
        }

        function confirmarCancelacion(sender) {
            Swal.fire({
                title: '¿Cancelar Cita?',
                text: "Pasará a estado Inactivo.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, cancelar',
                cancelButtonText: 'Volver'
            }).then((result) => { if (result.isConfirmed) { eval(sender.href); } });
            return false;
        }

        // Script para manejar los checkboxes de motivos
        document.addEventListener('DOMContentLoaded', function () {
            var chkOtros = document.getElementById('<%= chkOtros.ClientID %>');
            var otrosTexto = document.querySelector('.otros-texto');

            if (chkOtros && otrosTexto) {
                function toggleOtrosTexto() {
                    otrosTexto.style.display = chkOtros.checked ? 'block' : 'none';
                    if (!chkOtros.checked) {
                        var txtOtros = document.getElementById('<%= txtOtrosMotivo.ClientID %>');
                        if (txtOtros) txtOtros.value = '';
                    }
                }

                chkOtros.addEventListener('change', toggleOtrosTexto);
                toggleOtrosTexto();
            }

            // Estilo visual para items seleccionados
            var checkboxes = document.querySelectorAll('.motivo-item input[type="checkbox"]');
            checkboxes.forEach(function (chk) {
                chk.addEventListener('change', function () {
                    var item = this.closest('.motivo-item');
                    if (this.checked) {
                        item.classList.add('motivo-seleccionado');
                    } else {
                        item.classList.remove('motivo-seleccionado');
                    }
                });

                // Inicializar estado
                if (chk.checked) {
                    var item = chk.closest('.motivo-item');
                    if (item) item.classList.add('motivo-seleccionado');
                }
            });
        });

        function obtenerMotivosTexto() {
            var motivos = [];

            var motivosMap = {
                '<%= chkExamenVisual.ClientID %>': 'Examen Visual',
                '<%= chkGraduacion.ClientID %>': 'Actualización de Graduación',
                '<%= chkSeleccionMontura.ClientID %>': 'Selección de Montura',
                '<%= chkAjusteLentes.ClientID %>': 'Ajuste de Lentes',
                '<%= chkReparacion.ClientID %>': 'Reparación de Gafas',
                '<%= chkEntregaLentes.ClientID %>': 'Entrega de Lentes Terminados',
                '<%= chkGarantia.ClientID %>': 'Gestión de Garantía',
                '<%= chkAsesoria.ClientID %>': 'Asesoría en Lentes de Contacto',
                '<%= chkSeguro.ClientID %>': 'Gestión de Seguro Médico',
                '<%= chkLimpieza.ClientID %>': 'Limpieza y Mantenimiento',
                '<%= chkSegundaOpinion.ClientID %>': 'Segunda Opinión',
                '<%= chkControl.ClientID %>': 'Control de Rutina',
                '<%= chkDolorOcular.ClientID %>': 'Dolor Ocular / Molestias',
                '<%= chkReceta.ClientID %>': 'Actualización de Receta',
                '<%= chkProteccion.ClientID %>': 'Protección Visual'
            };
            
            for (var id in motivosMap) {
                var chk = document.getElementById(id);
                if (chk && chk.checked) {
                    motivos.push(motivosMap[id]);
                }
            }
            
            var chkOtros = document.getElementById('<%= chkOtros.ClientID %>');
            var txtOtros = document.getElementById('<%= txtOtrosMotivo.ClientID %>');

            if (chkOtros && chkOtros.checked && txtOtros && txtOtros.value.trim() !== '') {
                motivos.push('Otros: ' + txtOtros.value.trim());
            }

            return motivos.join(', ');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid" style="padding: 20px;">
        
        <asp:Panel ID="PanelListado" runat="server">
            <div class="panel-card">
                <div class="panel-header">
                    <h3 style="margin:0; color:var(--color-primary);"><i class="fa-regular fa-calendar-check"></i> Listado de Citas</h3>
                    <asp:Button ID="btnNuevo" runat="server" Text="+ Nueva Cita" CssClass="btn-std btn-success" OnClick="btnNuevo_Click" />
                </div>
                <div class="panel-body">
                    
                    <div class="toolbar">
                        <div style="flex-grow: 1; min-width: 200px;">
                            <label class="lbl-formal">Búsqueda</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control-formal" placeholder="Motivo, Cliente..."></asp:TextBox>
                        </div>
                        <div style="width: 150px;">
                            <label class="lbl-formal">Estado</label>
                            <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control-formal">
                                <asp:ListItem Text="Todos" Value="-1"></asp:ListItem>
                                <asp:ListItem Text="Activa" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Cancelada" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div style="width: 160px;">
                            <label class="lbl-formal">Desde</label>
                            <asp:TextBox ID="txtFiltroFechaInicio" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div style="width: 160px;">
                            <label class="lbl-formal">Hasta</label>
                            <asp:TextBox ID="txtFiltroFechaFin" runat="server" TextMode="Date" CssClass="form-control-formal"></asp:TextBox>
                        </div>
                        <div>
                            <label class="lbl-formal">&nbsp;</label>
                            <div style="display: flex; gap: 10px;">
                                <asp:Button ID="btnBuscar" runat="server" Text="Filtrar" CssClass="btn-std btn-primary" OnClick="btnBuscar_Click" />
                                <asp:Button ID="btnMostrarTodo" runat="server" Text="Mostrar Todo" CssClass="btn-std btn-info" OnClick="btnMostrarTodo_Click" />
                            </div>
                        </div>
                    </div>

                    <div style="display:flex; justify-content:space-between; margin-bottom:10px; align-items:center;">
                        <div>
                            Mostrar 
                            <asp:DropDownList ID="ddlPageSize" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" 
                                style="padding:5px; border-radius:4px; border:1px solid #ccc;">
                                <asp:ListItem Value="10">10</asp:ListItem>
                                <asp:ListItem Value="20">20</asp:ListItem>
                                <asp:ListItem Value="50">50</asp:ListItem>
                                <asp:ListItem Value="All">Todos</asp:ListItem>
                            </asp:DropDownList>
                            registros
                        </div>
                    </div>

                    <asp:GridView ID="gvCitas" runat="server" CssClass="table-std" AutoGenerateColumns="False" 
                        DataKeyNames="ID_Cita" AllowPaging="True" PageSize="10" 
                        OnRowCommand="gvCitas_RowCommand" OnPageIndexChanging="gvCitas_PageIndexChanging" GridLines="None" ShowHeaderWhenEmpty="true">
                        <Columns>
                            <asp:BoundField DataField="ID_Cita" HeaderText="ID" ItemStyle-Width="50px" />
                            <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" />
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha Cita" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="120px" />
                            <asp:BoundField DataField="Hora" HeaderText="Hora" ItemStyle-Width="100px" />
                            <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
                            <asp:BoundField DataField="NombreUsuario" HeaderText="Atendido Por" />
                            <asp:BoundField DataField="FechaRegistro" HeaderText="Registro" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="100px" />
                            <asp:TemplateField HeaderText="Estado" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <span class='badge-status <%# Convert.ToBoolean(Eval("Estado")) ? "badge-active" : "badge-inactive" %>'>
                                        <%# Convert.ToBoolean(Eval("Estado")) ? "Activa" : "Cancelada" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("ID_Cita") %>' CssClass="btn-std btn-sm btn-primary"><i class="fa-solid fa-pen-to-square"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnBaja" runat="server" CommandName="DarBaja" CommandArgument='<%# Eval("ID_Cita") %>' CssClass="btn-std btn-sm btn-danger" OnClientClick="return confirmarCancelacion(this);" Visible='<%# Convert.ToBoolean(Eval("Estado")) %>'><i class="fa-solid fa-ban"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <asp:Panel ID="pnlMensajeGrid" runat="server" Visible="false" CssClass="alert-info-custom">
                        <i class="fa-solid fa-circle-info"></i> <asp:Label ID="lblMensajeGrid" runat="server"></asp:Label>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PanelMantenimiento" runat="server" Visible="false">
            <div class="panel-card" style="max-width: 950px; margin: 0 auto;">
                <div class="panel-header">
                    <h3><asp:Label ID="lblTituloAccion" runat="server" Text="Programar Cita"></asp:Label></h3>
                </div>
                <div class="panel-body">
                    <asp:HiddenField ID="hfIDCita" runat="server" />

                    <div class="form-grid-layout">
                        
                        <div class="form-group-formal form-full-width">
                            <label class="lbl-formal">Cliente <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:DropDownList ID="ddlCliente" runat="server" CssClass="form-control-formal"></asp:DropDownList>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Seleccione el cliente.</small>
                            <asp:Label ID="errCliente" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                        <div class="form-group-formal form-full-width">
                            <label class="lbl-formal">Usuario (Atiende) <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-control-formal"></asp:DropDownList>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Usuario responsable.</small>
                            <asp:Label ID="errUsuario" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                        <div class="form-group-formal">
                            <label class="lbl-formal">Fecha de Cita <span class="required-mark">*</span></label>
                            <div class="input-wrapper">
                                <asp:TextBox ID="txtFechaCita" runat="server" CssClass="form-control-formal" TextMode="Date"></asp:TextBox>
                                <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            </div>
                            <small class="help-text-formal">Máx. 7 días futuro.</small>
                            <asp:Label ID="errFechaCita" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                        <div class="form-group-formal">
                            <label class="lbl-formal">Hora <span class="required-mark">*</span></label>
                            <div class="input-group-formal">
                                <div class="input-wrapper" style="flex-grow:1;">
                                    <asp:TextBox ID="txtHoraNum" runat="server" CssClass="form-control-formal" placeholder="00:00" MaxLength="5"></asp:TextBox>
                                    <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                                </div>
                                <asp:DropDownList ID="ddlAmPm" runat="server" CssClass="form-control-formal combo-append">
                                    <asp:ListItem Text="AM" Value="AM"></asp:ListItem>
                                    <asp:ListItem Text="PM" Value="PM"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <small class="help-text-formal">08:00 AM - 06:00 PM.</small>
                            <asp:Label ID="errHora" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                        <div class="form-group-formal form-full-width">
                            <label class="lbl-formal">Motivo(s) de la Cita <span class="required-mark">*</span></label>
                            
                            <div class="motivos-container">
                                <div class="motivos-grid">
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkExamenVisual" runat="server" />
                                        <label for="chkExamenVisual">Examen Visual</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkGraduacion" runat="server" />
                                        <label for="chkGraduacion">Actualización de Graduación</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkSeleccionMontura" runat="server" />
                                        <label for="chkSeleccionMontura">Selección de Montura</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkAjusteLentes" runat="server" />
                                        <label for="chkAjusteLentes">Ajuste de Lentes</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkReparacion" runat="server" />
                                        <label for="chkReparacion">Reparación de Gafas</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkEntregaLentes" runat="server" />
                                        <label for="chkEntregaLentes">Entrega de Lentes Terminados</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkGarantia" runat="server" />
                                        <label for="chkGarantia">Gestión de Garantía</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkAsesoria" runat="server" />
                                        <label for="chkAsesoria">Asesoría en Lentes de Contacto</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkSeguro" runat="server" />
                                        <label for="chkSeguro">Gestión de Seguro Médico</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkLimpieza" runat="server" />
                                        <label for="chkLimpieza">Limpieza y Mantenimiento</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkSegundaOpinion" runat="server" />
                                        <label for="chkSegundaOpinion">Segunda Opinión</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkControl" runat="server" />
                                        <label for="chkControl">Control de Rutina</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkDolorOcular" runat="server" />
                                        <label for="chkDolorOcular">Dolor Ocular / Molestias</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkReceta" runat="server" />
                                        <label for="chkReceta">Actualización de Receta</label>
                                    </div>
                                    <div class="motivo-item">
                                        <asp:CheckBox ID="chkProteccion" runat="server" />
                                        <label for="chkProteccion">Protección Visual</label>
                                    </div>
                                </div>
                                
                                <div class="motivo-otros">
                                    <div class="motivo-otros-checkbox">
                                        <asp:CheckBox ID="chkOtros" runat="server" />
                                        <label for="chkOtros">Otros (Especificar)</label>
                                    </div>
                                    <div class="otros-texto">
                                        <asp:TextBox ID="txtOtrosMotivo" runat="server" TextMode="MultiLine" Rows="2" 
                                            placeholder="Describa el motivo de la cita..." CssClass="form-control-formal"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                            
                            <asp:HiddenField ID="hfMotivoTexto" runat="server" />
                            <i class="fa-solid fa-circle-exclamation input-error-icon"></i>
                            <small class="help-text-formal">Seleccione uno o más motivos. Si selecciona "Otros", especifique el motivo.</small>
                            <asp:Label ID="errMotivo" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                        <div class="form-group-formal">
                            <label class="lbl-formal">Fecha de Registro</label>
                            <asp:TextBox ID="txtFechaRegistro" runat="server" CssClass="form-control-formal" TextMode="Date"></asp:TextBox>
                            <small class="help-text-formal">Automático.</small>
                            <asp:Label ID="errFechaRegistro" runat="server" CssClass="error-message-formal" Visible="false"></asp:Label>
                        </div>

                    </div>

                    <div style="text-align:right; margin-top:20px; border-top: 1px solid #eee; padding-top: 20px;">
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-std btn-secondary" OnClick="btnCancelar_Click" />
                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn-std btn-success" OnClick="btnGuardar_Click" OnClientClick="return confirmarGuardar(this);" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>