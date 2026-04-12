<%@ Page Title="Usuarios Activos" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AccesoUsuarios.aspx.cs" Inherits="Optica.AdministrarAccesos.AccesoUsuarios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Tus estilos CSS permanecen igual */
        .dashboard-cards {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }

        .card {
            background: white;
            border-radius: 10px;
            padding: 25px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            text-align: center;
            border-left: 4px solid #667eea;
        }

        .card-online {
            border-left-color: #28a745;
        }

        .card-offline {
            border-left-color: #6c757d;
        }

        .card-total {
            border-left-color: #ffc107;
        }

        .card-number {
            font-size: 2.5em;
            font-weight: bold;
            margin: 10px 0;
        }

        .card-label {
            color: #666;
            font-size: 0.9em;
        }

        .users-grid {
            background: white;
            border-radius: 10px;
            padding: 25px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }

        .grid-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            flex-wrap: wrap;
            gap: 15px;
        }

        .grid-title {
            font-size: 1.5em;
            font-weight: bold;
            color: #333;
        }

        .filters {
            display: flex;
            gap: 10px;
            align-items: center;
            flex-wrap: wrap;
        }

        .filter-select {
            padding: 8px 12px;
            border: 1px solid #ddd;
            border-radius: 5px;
            background: white;
        }

        .refresh-btn {
            background: #667eea;
            color: white;
            border: none;
            border-radius: 5px;
            padding: 8px 15px;
            cursor: pointer;
            display: flex;
            align-items: center;
            gap: 5px;
        }

        .refresh-btn:hover {
            background: #5a6fd8;
        }

        .user-table {
            width: 100%;
            border-collapse: collapse;
        }

        .user-table th {
            background: #f8f9fa;
            padding: 12px 15px;
            text-align: left;
            font-weight: 600;
            color: #333;
            border-bottom: 2px solid #e9ecef;
        }

        .user-table td {
            padding: 12px 15px;
            border-bottom: 1px solid #e9ecef;
        }

        .user-table tr:hover {
            background: #f8f9fa;
        }

        .status-badge {
            display: inline-block;
            padding: 4px 8px;
            border-radius: 12px;
            font-size: 0.8em;
            font-weight: bold;
        }

        .status-online {
            background: #d4edda;
            color: #155724;
        }

        .status-offline {
            background: #f8d7da;
            color: #721c24;
        }

        .user-avatar {
            width: 35px;
            height: 35px;
            background: #667eea;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-weight: bold;
        }

        .last-connection {
            font-size: 0.85em;
            color: #666;
        }

        .no-data {
            text-align: center;
            padding: 40px;
            color: #666;
        }

        .action-btn {
            background: none;
            border: none;
            color: #667eea;
            cursor: pointer;
            padding: 5px;
        }

        .action-btn:hover {
            color: #5a6fd8;
        }

        .btn-danger {
            background: #dc3545;
            color: white;
            border: none;
            border-radius: 5px;
            padding: 5px 10px;
            cursor: pointer;
            font-size: 0.8em;
        }

        .btn-danger:hover {
            background: #c82333;
        }

        @media (max-width: 768px) {
            .dashboard-cards {
                grid-template-columns: 1fr;
            }
            
            .grid-header {
                flex-direction: column;
                align-items: stretch;
            }
            
            .filters {
                justify-content: center;
            }
            
            .user-table {
                font-size: 0.9em;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-cards">
        <div class="card card-online">
            <i class="fas fa-user-check" style="font-size: 2em; color: #28a745;"></i>
            <div class="card-number" id="onlineCount" runat="server">0</div>
            <div class="card-label">Usuarios En Línea</div>
        </div>
        
        <div class="card card-offline">
            <i class="fas fa-user-clock" style="font-size: 2em; color: #6c757d;"></i>
            <div class="card-number" id="offlineCount" runat="server">0</div>
            <div class="card-label">Usuarios Desconectados</div>
        </div>
        
        <div class="card card-total">
            <i class="fas fa-users" style="font-size: 2em; color: #ffc107;"></i>
            <div class="card-number" id="totalUsers" runat="server">0</div>
            <div class="card-label">Total de Usuarios</div>
        </div>
    </div>

    <div class="users-grid">
        <div class="grid-header">
            <div class="grid-title">
                <i class="fas fa-list"></i> Lista de Usuarios
            </div>
            <div class="filters">
                <asp:DropDownList ID="ddlEstado" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstado_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos los estados</asp:ListItem>
                    <asp:ListItem Value="1">En línea</asp:ListItem>
                    <asp:ListItem Value="0">Desconectados</asp:ListItem>
                </asp:DropDownList>
                
                <asp:DropDownList ID="ddlRol" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlRol_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos los roles</asp:ListItem>
                </asp:DropDownList>
                
                <asp:Button ID="btnRefresh" runat="server" CssClass="refresh-btn" Text="Actualizar" OnClick="btnRefresh_Click" />
            </div>
        </div>

        <div style="overflow-x: auto;">
            <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="False" 
                CssClass="user-table" EmptyDataText="No se encontraron usuarios" 
                OnRowDataBound="gvUsuarios_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="Usuario">
                        <ItemTemplate>
                            <div style="display: flex; align-items: center; gap: 10px;">
                                <div class="user-avatar">
                                    <asp:Literal ID="ltIniciales" runat="server" />
                                </div>
                                <div>
                                    <div style="font-weight: bold;">
                                        <%# Eval("Nombres") %> <%# Eval("Apellidos") %>
                                    </div>
                                    <div style="font-size: 0.85em; color: #666;">
                                        <%# Eval("Correo") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:BoundField DataField="Rol" HeaderText="Rol" SortExpression="Rol" />
                    
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate>
                            <span class='status-badge <%# Convert.ToBoolean(Eval("EnLinea")) ? "status-online" : "status-offline" %>'>
                                <i class='fas fa-circle'></i>
                                <%# Convert.ToBoolean(Eval("EnLinea")) ? "En línea" : "Desconectado" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Última Conexión">
                        <ItemTemplate>
                            <div class="last-connection">
                                <asp:Literal ID="ltUltimaConexion" runat="server" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Sesiones Activas">
                        <ItemTemplate>
                            <span style="font-weight: bold; color: #667eea;">
                                <asp:Literal ID="ltSesionesActivas" runat="server" />
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnForzarCierre" runat="server" CssClass="btn-danger" 
                                CommandArgument='<%# Eval("ID_Usuario") %>' 
                                OnClick="btnForzarCierre_Click"
                                OnClientClick='<%# "return confirm(\"¿Está seguro de forzar el cierre de sesión de " + Eval("Nombres") + " " + Eval("Apellidos") + "?\");" %>'
                                Visible='<%# Convert.ToBoolean(Eval("EnLinea")) %>'
                                title="Forzar cierre de sesión">
                                <i class="fas fa-sign-out-alt"></i> Cerrar Sesión
                            </asp:LinkButton>
                            <asp:Label ID="spanOffline" runat="server" Text="-" Visible='<%# !Convert.ToBoolean(Eval("EnLinea")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        // Auto-refresh cada 30 segundos
        setInterval(function() {
            __doPostBack('<%= btnRefresh.UniqueID %>', '');
        }, 100000);
    </script>
</asp:Content>