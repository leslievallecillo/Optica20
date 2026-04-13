<%@ Page Title="Panel de Control" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Optica.Formularios.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />

    <style>
        :root {
            --bg-light: #f4f6f9;
            --card-shadow: 0 1px 1px rgba(0,0,0,0.1);
            --color-purple: #605ca8;
            --color-green: #00a65a;
            --color-orange: #f39c12;
            --color-blue: #00c0ef;
        }

        .dashboard-wrapper { padding: 15px; background-color: var(--bg-light); min-height: 100vh; box-sizing: border-box; }
        .dash-header { margin-bottom: 20px; color: #444; font-weight: 300; font-size: 24px; }
        .dash-header small { font-size: 15px; color: #777; margin-left: 5px; }

        .top-row { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 20px; }
        
        .chart-section {
            flex: 2; 
            background: #fff;
            border-top: 3px solid #d2d6de;
            box-shadow: var(--card-shadow);
            border-radius: 3px;
            padding: 10px;
            min-width: 300px;
            box-sizing: border-box;
        }

        .kpi-section {
            flex: 1; 
            display: flex;
            flex-direction: column;
            gap: 15px;
            min-width: 250px;
            box-sizing: border-box;
        }

        .small-box {
            border-radius: 2px;
            position: relative;
            display: block;
            margin-bottom: 5px;
            box-shadow: 0 1px 1px rgba(0,0,0,0.1);
            color: #fff;
            padding: 20px;
            overflow: hidden;
            box-sizing: border-box;
        }
        .small-box .inner { padding-right: 10px; }
        .small-box h3 { font-size: 28px; font-weight: bold; margin: 0 0 10px 0; white-space: nowrap; padding: 0; }
        .small-box p { font-size: 14px; font-weight: 600; text-transform: uppercase; margin: 0; }
        .small-box .small-desc { display: block; font-size: 12px; margin-top: 5px; opacity: 0.8; }
        
        .small-box .icon {
            position: absolute;
            top: 10px;
            right: 10px;
            font-size: 60px;
            color: rgba(0,0,0,0.15);
            transition: transform .3s;
        }
        .small-box:hover .icon { transform: scale(1.1); }

        .bg-purple { background-color: var(--color-purple) !important; }
        .bg-green { background-color: var(--color-green) !important; }
        .bg-orange { background-color: var(--color-orange) !important; }
        .bg-blue { background-color: var(--color-blue) !important; }

        .bottom-row { display: flex; flex-wrap: wrap; gap: 20px; }

        .box {
            flex: 1;
            background: #fff;
            border-top: 3px solid #00c0ef;
            box-shadow: var(--card-shadow);
            border-radius: 3px;
            min-width: 300px;
            box-sizing: border-box;
        }

        .box-header {
            color: #444;
            display: block;
            padding: 10px;
            position: relative;
            border-bottom: 1px solid #f4f4f4;
        }
        .box-title { display: inline-block; font-size: 18px; margin: 0; line-height: 1; font-weight: 400; }

        .box-body { padding: 0; overflow-x: auto; -webkit-overflow-scrolling: touch; }

        .table-dash { width: 100%; border-collapse: collapse; font-size: 14px; white-space: nowrap;}
        .table-dash th { background-color: #fff; border-bottom: 2px solid #f4f4f4; padding: 8px; text-align: left; color: #333; font-weight: 600; }
        .table-dash td { border-bottom: 1px solid #f4f4f4; padding: 8px; color: #555; }
        .link-inv { color: #3c8dbc; cursor: pointer; text-decoration: none; }
        
        .products-list { list-style: none; padding: 0; margin: 0; }
        .products-list .item {
            padding: 10px;
            border-bottom: 1px solid #f4f4f4;
            display: flex;
            align-items: center;
            gap: 15px;
            flex-wrap: wrap;
        }
        .products-list .product-img img { width: 50px; height: 50px; object-fit: cover; border: 1px solid #ddd; padding: 2px; }
        .products-list .product-info { flex: 1; min-width: 150px;}
        .products-list .product-title { font-weight: 600; color: #3c8dbc; font-size: 14px; text-decoration: none; display: block; margin-bottom: 5px;}
        .products-list .product-description { display: block; color: #999; font-size: 12px; }
        .label-price { float: right; padding: .2em .6em .3em; font-size: 75%; font-weight: 700; line-height: 1; color: #fff; text-align: center; white-space: nowrap; vertical-align: baseline; border-radius: .25em; background-color: #dd4b39; }

        .box-footer { padding: 10px; text-align: center; background-color: #fff; border-top: 1px solid #f4f4f4; }
        .box-footer a { color: #444; text-transform: uppercase; text-decoration: none; font-size: 13px; font-weight: 600; }

        @media (max-width: 768px) {
            .dashboard-wrapper { padding: 10px; }
            .top-row { flex-direction: column; }
            .chart-section, .kpi-section { flex: auto; width: 100%; min-width: auto; }
            .bottom-row { flex-direction: column; }
            .box { width: 100%; min-width: auto; }
            .small-box h3 { font-size: 24px; }
            .label-price { float: none; display: inline-block; margin-top: 5px; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-wrapper">
        
        <h1 class="dash-header">Panel de Control <small>Versión 6.3</small></h1>

        <div class="top-row">
            <div class="chart-section">
                <div style="margin-bottom:10px; font-weight:bold; color:#666; text-align:center;">
                    Compras & Ventas <script>document.write(new Date().getFullYear())</script>
                </div>
                <div style="height: 320px; width:100%; position: relative;">
                    <canvas id="mainChart"></canvas>
                </div>
            </div>

            <div class="kpi-section">
                
                <div class="small-box bg-purple">
                    <div class="inner">
                        <p>INVENTARIO NETO</p>
                        <h3><asp:Label ID="lblInvNeto" runat="server" Text="0.00"></asp:Label></h3>
                        <span class="small-desc">Productos en stock: <asp:Label ID="lblStock" runat="server" Text="0"></asp:Label></span>
                    </div>
                    <div class="icon"><i class="fa-solid fa-tags"></i></div>
                </div>

                <div class="small-box bg-green">
                    <div class="inner">
                        <p>VENTAS <script>document.write(new Date().getFullYear())</script></p>
                        <h3><asp:Label ID="lblVentasTotal" runat="server" Text="0.00"></asp:Label></h3>
                        <span class="small-desc">Facturas emitidas: <asp:Label ID="lblVentasCount" runat="server" Text="0"></asp:Label></span>
                    </div>
                    <div class="icon"><i class="fa-solid fa-money-bill-1-wave"></i></div>
                </div>

                <div class="small-box bg-orange">
                    <div class="inner">
                        <p>COMPRAS <script>document.write(new Date().getFullYear())</script></p>
                        <h3><asp:Label ID="lblComprasTotal" runat="server" Text="0.00"></asp:Label></h3>
                        <span class="small-desc">Compras realizadas: <asp:Label ID="lblComprasCount" runat="server" Text="0"></asp:Label></span>
                    </div>
                    <div class="icon"><i class="fa-solid fa-cart-shopping"></i></div>
                </div>

                <div class="small-box bg-blue">
                    <div class="inner">
                        <p>CLIENTES</p>
                        <h3><asp:Label ID="lblClientesTotal" runat="server" Text="0"></asp:Label></h3>
                        <span class="small-desc">Clientes registrados</span>
                    </div>
                    <div class="icon"><i class="fa-solid fa-users"></i></div>
                </div>

            </div>
        </div>

        <div class="bottom-row">
            
            <div class="box">
                <div class="box-header">
                    <h3 class="box-title">Próximas Citas</h3>
                </div>
                <div class="box-body">
                    <asp:GridView ID="gvCitasProximas" runat="server" CssClass="table-dash" AutoGenerateColumns="False" GridLines="None" ShowHeader="true">
                        <Columns>
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
                            <asp:BoundField DataField="Hora" HeaderText="Hora" />
                            <asp:BoundField DataField="Cliente" HeaderText="Cliente" />
                            <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="box-footer">
                    <a href="Cita.aspx">Ver todas las citas</a>
                </div>
            </div>

            <div class="box">
                <div class="box-header">
                    <h3 class="box-title">Productos con Menos Stock</h3>
                </div>
                <div class="box-body">
                    <ul class="products-list">
                        <asp:Repeater ID="rptProductos" runat="server">
                            <ItemTemplate>
                                <li class="item">
                                    <div class="product-img">
                                        <img src='<%# ResolveUrl(string.IsNullOrEmpty(Convert.ToString(Eval("RutaImagen"))) ? "~/Imagenes2/default.png" : Convert.ToString(Eval("RutaImagen"))) %>' alt="Img">
                                    </div>
                                    <div class="product-info">
                                        <a href="#" class="product-title">
                                            <%# Eval("Descripcion") %>
                                            <span class="label-price">Stock: <%# Eval("Stock") %></span>
                                        </a>
                                        <span class="product-description">
                                            <%# Eval("Marca") %> - <%# Eval("Modelo") %>
                                        </span>
                                    </div>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
                <div class="box-footer">
                    <a href="ReporteInventario.aspx">Ver todo el inventario</a>
                </div>
            </div>

        </div>
    </div>

    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const dataVentas = <%= JsonVentas %>;
            const dataCompras = <%= JsonCompras %>;

            const ctx = document.getElementById('mainChart').getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
                    datasets: [
                        {
                            label: 'Ventas',
                            backgroundColor: '#00a65a',
                            borderColor: '#00a65a',
                            data: dataVentas
                        },
                        {
                            label: 'Compras',
                            backgroundColor: '#f39c12',
                            borderColor: '#f39c12',
                            data: dataCompras,
                            hidden: true
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: { beginAtZero: true }
                    },
                    plugins: {
                        legend: { position: 'top' }
                    }
                }
            });
        });
    </script>
</asp:Content>