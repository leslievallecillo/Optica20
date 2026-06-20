<%@ Page Title="Catálogo de Clientes" Language="C#" AutoEventWireup="true" CodeBehind="CatalogoCliente.aspx.cs" Inherits="Optica.Formularios.CatalogoCliente" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Catálogo de Clientes - Óptica 20/20</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=yes" />
    <meta name="description" content="Catálogo de productos ópticos - Óptica 20/20" />
    
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;600;700&display=swap');

        :root {
            --color-acento: #D7C782; 
            --color-acento-hover: #E1D5A6;
            --color-oscuro: #2f3542;
            --color-texto: #57606f;
            --color-fondo: #f1f2f6;
            --color-blanco: #ffffff;
            --radio-borde: 16px;
            --sombra-moderna: 0 10px 20px rgba(0,0,0,0.08), 0 6px 6px rgba(0,0,0,0.1);
            --sombra-suave: 0 4px 12px rgba(0,0,0,0.05);
            --transicion: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
            --fuente-principal: 'Poppins', sans-serif;
        }

        * { margin: 0; padding: 0; box-sizing: border-box; }

        body { font-family: var(--fuente-principal); color: var(--color-texto); background-color: var(--color-fondo); line-height: 1.6; overflow-x: hidden; }
        .contenedor { max-width: 1200px; margin: 0 auto; padding: 0 20px; }
        .encabezado { background: rgba(255, 255, 255, 0.9); backdrop-filter: blur(10px); -webkit-backdrop-filter: blur(10px); box-shadow: var(--sombra-suave); position: sticky; top: 0; z-index: 1000; transition: var(--transicion); border-bottom: 1px solid rgba(0,0,0,0.05); }
        .contenedor-encabezado { display: flex; justify-content: space-between; align-items: center; padding: 12px 0; }
        .logo { width: 60px; height: 60px; border-radius: 12px; overflow: hidden; display: flex; align-items: center; justify-content: center; background: var(--color-acento); box-shadow: 0 4px 10px rgba(215, 199, 130, 0.5); flex-shrink: 0; }
        .logo img { width: 100%; height: 100%; object-fit: cover; }
        .navegacion-principal { display: flex; gap: 5px; align-items: center; flex-wrap: wrap; }
        .boton-navegacion { background: transparent; border: none; color: var(--color-oscuro); padding: 10px 18px; cursor: pointer; font-size: 0.9rem; font-weight: 600; transition: var(--transicion); border-radius: 50px; position: relative; white-space: nowrap; }
        .boton-navegacion:hover { background-color: rgba(47, 53, 66, 0.05); color: var(--color-oscuro); transform: translateY(-2px); }
        .boton-iniciar-sesion { background: var(--color-oscuro); color: var(--color-blanco); padding: 10px 25px; box-shadow: 0 4px 15px rgba(47, 53, 66, 0.3); }
        .boton-iniciar-sesion:hover { background: var(--color-acento); color: var(--color-oscuro); box-shadow: 0 6px 20px rgba(215, 199, 130, 0.6); }
        .menu-toggle { display: none; background: transparent; border: none; font-size: 1.8rem; cursor: pointer; color: var(--color-oscuro); padding: 10px; transition: var(--transicion); }
        .menu-toggle:hover { color: var(--color-acento); }

        .banner-bienvenida { background: linear-gradient(135deg, rgba(47, 53, 66, 0.4), rgba(47, 53, 66, 0.2)), url('https://images.unsplash.com/photo-1574258495973-f010dfbb5371?q=80&w=1920&auto=format&fit=crop'); background-size: cover; background-position: center;  color: var(--color-blanco); text-align: center; padding: 120px 0;  margin-bottom: 60px; border-radius: 0 0 40px 40px; box-shadow: var(--sombra-moderna); position: relative; }
        .banner-bienvenida h1 { font-size: 3.5rem; margin-bottom: 20px; font-weight: 800; letter-spacing: -1px; animation: fadeInUp 0.8s ease-out; text-shadow: 0 2px 10px rgba(0,0,0,0.6); }
        .banner-bienvenida p { font-size: 1.4rem; font-weight: 600; opacity: 0.95; animation: fadeInUp 0.8s ease-out 0.2s backwards; text-shadow: 0 2px 5px rgba(0,0,0,0.6); }
        @keyframes fadeInUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }

        .seccion-productos { margin-bottom: 60px; padding: 20px 0; }
        .titulo-seccion { text-align: center; margin-bottom: 50px; color: var(--color-oscuro); font-size: 2.2rem; font-weight: 700; position: relative; display: inline-block; width: 100%; }
        .titulo-seccion::after { content: ''; position: absolute; bottom: -15px; left: 50%; transform: translateX(-50%); width: 60px; height: 6px; background: var(--color-acento); border-radius: 10px; }

        .contenedor-carrusel { position: relative; padding: 0 20px; }
        .carrusel-wrapper { display: flex; align-items: center; position: relative; }
        .carrusel { display: flex; gap: 30px; padding: 40px 20px; width: 100%; overflow-x: auto; scroll-behavior: smooth; scrollbar-width: none; scroll-padding: 0 20px; -webkit-overflow-scrolling: touch; }
        .carrusel::-webkit-scrollbar { display: none; }
        .item-carrusel { flex: 0 0 300px; min-width: 300px; background-color: var(--color-blanco); border-radius: var(--radio-borde); box-shadow: var(--sombra-suave); overflow: hidden; transition: var(--transicion); border: 1px solid rgba(0,0,0,0.02); cursor: pointer; display: flex; flex-direction: column; }
        .item-carrusel:hover { transform: translateY(-10px); box-shadow: var(--sombra-moderna); }
        .imagen-producto { width: 100%; height: 240px; object-fit: cover; border-bottom: 1px solid rgba(0,0,0,0.03); }
        .info-producto { padding: 25px; text-align: left; display: flex; flex-direction: column; flex-grow: 1; }
        .info-producto h3 { color: var(--color-oscuro); font-size: 1.2rem; font-weight: 700; margin-bottom: 8px; }
        .info-producto p { font-size: 0.95rem; color: var(--color-texto); margin-bottom: 15px; }
        .precio-producto { display: inline-block; color: var(--color-oscuro); background: var(--color-acento); font-weight: 700; font-size: 1.1rem; padding: 6px 16px; border-radius: 50px; box-shadow: 0 4px 10px rgba(215, 199, 130, 0.4); margin-top: auto; align-self: flex-start; }

        .boton-carrusel { position: absolute; top: 50%; transform: translateY(-50%); background: var(--color-blanco); color: var(--color-oscuro); border: none; width: 50px; height: 50px; border-radius: 50%; font-size: 1.5rem; box-shadow: var(--sombra-moderna); z-index: 10; cursor: pointer; transition: var(--transicion); display: flex !important; align-items: center; justify-content: center; }
        .boton-carrusel:hover { background: var(--color-acento); color: var(--color-oscuro); transform: translateY(-50%) scale(1.1); }
        .boton-anterior { left: -10px; }
        .boton-siguiente { right: -10px; }

        .contenedor-barra-carrusel { display: flex; justify-content: center; padding: 10px 0; }
        .barra-carrusel { width: 150px; height: 6px; background: #dfe4ea; border-radius: 10px; overflow: hidden; }
        .indicador-barra-carrusel { height: 100%; background: var(--color-oscuro); width: 0%; transition: width 0.2s ease; border-radius: 10px; }

        .seccion-servicios { background: var(--color-blanco); padding: 80px 0; position: relative; }
        .contenedor-servicios { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 30px; }
        .item-servicio { padding: 40px; background: var(--color-fondo); border-radius: var(--radio-borde); transition: var(--transicion); border: 1px solid transparent; }
        .item-servicio:hover { background: var(--color-blanco); box-shadow: var(--sombra-moderna); border-color: var(--color-acento); transform: translateY(-5px); }
        .item-servicio h3 { color: var(--color-oscuro); font-size: 1.5rem; margin-bottom: 15px; position: relative; display: inline-block; }
        .item-servicio h3::after { content: ''; display: block; width: 40px; height: 4px; background: var(--color-acento); margin-top: 8px; border-radius: 2px; }

        .seccion-nosotros { background: var(--color-oscuro); color: var(--color-blanco); padding: 80px 0; border-radius: 40px; margin: 40px 20px; }
        .titulo-seccion-nosotros { text-align: center; margin-bottom: 50px; font-size: 2.2rem; font-weight: 700; color: var(--color-acento); }
        .contenedor-nosotros { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 40px; padding: 0 20px; }
        .item-nosotros { text-align: center; padding: 30px; background: rgba(255, 255, 255, 0.05); border-radius: var(--radio-borde); backdrop-filter: blur(5px); border: 1px solid rgba(255, 255, 255, 0.1); transition: var(--transicion); }
        .item-nosotros:hover { background: rgba(255, 255, 255, 0.1); transform: translateY(-5px); }
        .item-nosotros h3 { color: var(--color-acento); font-size: 1.5rem; margin-bottom: 15px; }
        .item-nosotros p { color: rgba(255, 255, 255, 0.8); }

        .seccion-contacto { background: var(--color-blanco); padding: 60px 0; border-top: 1px solid #eee; }
        .contenedor-contacto { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 40px; }
        .info-contacto h3 { color: var(--color-oscuro); margin-bottom: 20px; font-size: 1.3rem; font-weight: 700; }
        .lista-contacto { list-style: none; }
        .lista-contacto li { margin-bottom: 12px; display: flex; align-items: center; color: var(--color-texto); }
        .lista-contacto li::before { content: ''; display: inline-block; width: 8px; height: 8px; background: var(--color-acento); border-radius: 50%; margin-right: 10px; flex-shrink: 0; }
        .boton-red-social { display: inline-flex; align-items: center; background: var(--color-fondo); color: var(--color-oscuro); padding: 10px 20px; margin: 0 10px 10px 0; border-radius: 50px; text-decoration: none; font-weight: 600; transition: var(--transicion); }
        .boton-red-social:hover { background: var(--color-oscuro); color: var(--color-acento); transform: translateY(-3px); box-shadow: var(--sombra-suave); }

        .boton-whatsapp { position: fixed; bottom: 30px; right: 30px; width: 65px; height: 65px; background: linear-gradient(135deg, #25D366 0%, #128C7E 100%); border-radius: 50%; display: flex; align-items: center; justify-content: center; z-index: 2000; cursor: pointer; box-shadow: 0 10px 25px rgba(37, 211, 102, 0.4); transition: var(--transicion); border: 2px solid rgba(255, 255, 255, 0.2); }
        .boton-whatsapp:hover { transform: scale(1.1); box-shadow: 0 15px 30px rgba(37, 211, 102, 0.6); }
        .boton-whatsapp::before, .boton-whatsapp::after { content: ''; position: absolute; width: 100%; height: 100%; background: #25D366; border-radius: 50%; opacity: 0.5; z-index: -1; animation: pulse-onda 2s infinite; }
        .boton-whatsapp::after { animation-delay: 0.5s; }
        @keyframes pulse-onda { 0% { transform: scale(1); opacity: 0.6; } 100% { transform: scale(1.6); opacity: 0; } }

        .modal-imagen { display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(47, 53, 66, 0.95); z-index: 3000; justify-content: center; align-items: center; backdrop-filter: blur(8px); padding: 20px; }
        .modal-contenido { max-width: 90%; max-height: 90%; position: relative; border-radius: var(--radio-borde); box-shadow: 0 25px 50px rgba(0,0,0,0.3); animation: zoomIn 0.3s ease; }
        @keyframes zoomIn { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }
        .modal-imagen-ampliada { display: block; max-width: 100%; max-height: 85vh; border-radius: var(--radio-borde); object-fit: contain; }
        .cerrar-modal { position: absolute; top: -50px; right: 0; background: transparent; border: none; color: var(--color-blanco); font-size: 2.5rem; cursor: pointer; transition: var(--transicion); width: 40px; height: 40px; display: flex; align-items: center; justify-content: center; }
        .cerrar-modal:hover { color: var(--color-acento); transform: rotate(90deg); }
        .controles-modal { position: absolute; top: 50%; width: 100%; display: flex; justify-content: space-between; transform: translateY(-50%); padding: 0 20px; pointer-events: none; }
        .boton-modal { background: rgba(255, 255, 255, 0.2); color: var(--color-blanco); border: 1px solid rgba(255, 255, 255, 0.3); width: 55px; height: 55px; border-radius: 50%; cursor: pointer; font-size: 1.5rem; display: flex; align-items: center; justify-content: center; pointer-events: auto; transition: var(--transicion); backdrop-filter: blur(4px); }
        .boton-modal:hover { background: var(--color-acento); border-color: var(--color-acento); color: var(--color-oscuro); }

        @media (max-width: 992px) {
            .banner-bienvenida h1 { font-size: 2.8rem; }
            .banner-bienvenida p { font-size: 1.2rem; }
            .titulo-seccion { font-size: 2rem; }
            .item-carrusel { flex: 0 0 280px; min-width: 280px; }
            .contenedor-servicios { grid-template-columns: repeat(2, 1fr); }
        }

        @media (max-width: 768px) {
            .contenedor { padding: 0 15px; }
            .contenedor-encabezado { flex-wrap: wrap; justify-content: center; gap: 10px; }
            .logo { width: 50px; height: 50px; }
            .navegacion-principal { justify-content: center; width: 100%; }
            .boton-navegacion { padding: 8px 12px; font-size: 0.8rem; }
            .boton-iniciar-sesion { padding: 8px 20px; }
            .banner-bienvenida { padding: 80px 0; margin-bottom: 40px; border-radius: 0 0 20px 20px; }
            .banner-bienvenida h1 { font-size: 2rem; padding: 0 15px; }
            .banner-bienvenida p { font-size: 1rem; padding: 0 15px; }
            .seccion-productos { margin-bottom: 40px; padding: 10px 0; }
            .titulo-seccion { font-size: 1.8rem; margin-bottom: 35px; }
            .contenedor-carrusel { padding: 0 10px; }
            .carrusel { gap: 20px; padding: 30px 10px; }
            .item-carrusel { flex: 0 0 250px; min-width: 250px; }
            .imagen-producto { height: 200px; }
            .info-producto { padding: 20px; }
            .info-producto h3 { font-size: 1.1rem; }
            .precio-producto { font-size: 1rem; padding: 5px 14px; }
            .boton-carrusel { width: 40px; height: 40px; font-size: 1.2rem; }
            .boton-anterior { left: -5px; }
            .boton-siguiente { right: -5px; }
            .seccion-servicios { padding: 50px 0; }
            .contenedor-servicios { grid-template-columns: 1fr; gap: 20px; }
            .item-servicio { padding: 30px; }
            .item-servicio h3 { font-size: 1.3rem; }
            .seccion-nosotros { padding: 50px 0; margin: 30px 10px; border-radius: 20px; }
            .titulo-seccion-nosotros { font-size: 1.8rem; margin-bottom: 35px; }
            .contenedor-nosotros { grid-template-columns: 1fr; gap: 25px; }
            .item-nosotros { padding: 25px; }
            .item-nosotros h3 { font-size: 1.3rem; }
            .seccion-contacto { padding: 40px 0; }
            .contenedor-contacto { grid-template-columns: 1fr; gap: 30px; }
            .info-contacto { text-align: center; }
            .lista-contacto li { justify-content: center; }
            .boton-red-social { margin: 5px; }
            .boton-whatsapp { width: 55px; height: 55px; bottom: 20px; right: 20px; }
            .boton-whatsapp svg { width: 30px; height: 30px; }
            .modal-contenido { max-width: 95%; }
            .cerrar-modal { top: -40px; font-size: 2rem; }
            .boton-modal { width: 45px; height: 45px; font-size: 1.2rem; }
        }

        @media (max-width: 480px) {
            .contenedor-encabezado { flex-direction: column; gap: 15px; }
            .navegacion-principal { gap: 3px; }
            .boton-navegacion { padding: 6px 10px; font-size: 0.75rem; }
            .banner-bienvenida { padding: 60px 0; }
            .banner-bienvenida h1 { font-size: 1.6rem; }
            .banner-bienvenida p { font-size: 0.9rem; }
            .titulo-seccion { font-size: 1.5rem; }
            .item-carrusel { flex: 0 0 220px; min-width: 220px; }
            .imagen-producto { height: 180px; }
            .info-producto { padding: 15px; }
            .info-producto h3 { font-size: 1rem; }
            .info-producto p { font-size: 0.85rem; }
            .precio-producto { font-size: 0.9rem; padding: 4px 12px; }
            .item-servicio { padding: 20px; }
            .item-nosotros { padding: 20px; }
            .boton-whatsapp { width: 50px; height: 50px; bottom: 15px; right: 15px; }
        }

        @media (max-height: 500px) and (orientation: landscape) {
            .banner-bienvenida { padding: 40px 0; }
            .modal-imagen-ampliada { max-height: 70vh; }
        }

        @media (hover: none) and (pointer: coarse) {
            .boton-navegacion, .boton-carrusel, .boton-modal, .boton-whatsapp, .boton-red-social { min-height: 44px; min-width: 44px; }
            .item-carrusel:hover { transform: none; }
            .item-servicio:hover { transform: none; }
            .item-nosotros:hover { transform: none; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <header class="encabezado">
            <div class="contenedor contenedor-encabezado">
                <div class="logo">
                    <img src="https://i.postimg.cc/c1QY31mY/FB-IMG-1781671016736.jpg" alt="Logo Óptica 20/20" />
                </div>
                <nav class="navegacion-principal">
                    <button type="button" class="boton-navegacion" data-destino="inicio">Inicio</button>
                    <button type="button" class="boton-navegacion" data-destino="productos">Productos</button>
                    <button type="button" class="boton-navegacion" data-destino="promociones">Promociones</button>
                    <button type="button" class="boton-navegacion" data-destino="servicios">Servicios</button>
                    <button type="button" class="boton-navegacion" data-destino="nosotros">Nosotros</button>
                    <button type="button" class="boton-navegacion" data-destino="contacto">Contacto</button>
                    <button type="button" class="boton-navegacion boton-iniciar-sesion" onclick="redirigirLogin()">Iniciar Sesión</button>
                </nav>
            </div>
        </header>

        <section id="inicio" class="banner-bienvenida">
            <div class="contenedor">
                <h1>Bienvenidos a la Óptica 20/20</h1>
                <p>Tu visión es nuestra prioridad</p>
            </div>
        </section>

        <section id="productos" class="seccion-productos">
            <div class="contenedor">
                <h2 class="titulo-seccion">Los más destacados</h2>
                <div class="contenedor-carrusel">
                    <div class="carrusel-wrapper">
                        <button type="button" class="boton-carrusel boton-anterior" aria-label="Productos anteriores">‹</button>
                        <div class="carrusel">
                            <asp:Repeater ID="rptDestacados" runat="server">
                                <ItemTemplate>
                                    <div class="item-carrusel">
                                        <img src='<%# Eval("UrlImagen") %>' alt='<%# Eval("Titulo") %>' class="imagen-producto" />
                                        <div class="info-producto">
                                            <h3><%# Eval("Titulo") %></h3>
                                            <p><%# Eval("Descripcion") %></p>
                                            <p class="precio-producto"><%# Eval("Precio") %></p>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        <button type="button" class="boton-carrusel boton-siguiente" aria-label="Productos siguientes">›</button>
                    </div>
                    <div class="contenedor-barra-carrusel">
                        <div class="barra-carrusel">
                            <div class="indicador-barra-carrusel"></div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <section id="promociones" class="seccion-productos">
            <div class="contenedor">
                <h2 class="titulo-seccion">Promociones Especiales</h2>
                <div class="contenedor-carrusel">
                    <div class="carrusel-wrapper">
                        <button type="button" class="boton-carrusel boton-anterior" aria-label="Promociones anteriores">‹</button>
                        <div class="carrusel">
                            <asp:Repeater ID="rptPromociones" runat="server">
                                <ItemTemplate>
                                    <div class="item-carrusel">
                                        <img src='<%# Eval("UrlImagen") %>' alt='<%# Eval("Titulo") %>' class="imagen-producto" />
                                        <div class="info-producto">
                                            <h3><%# Eval("Titulo") %></h3>
                                            <p><%# Eval("Descripcion") %></p>
                                            <p class="precio-producto"><%# Eval("Precio") %></p>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        <button type="button" class="boton-carrusel boton-siguiente" aria-label="Promociones siguientes">›</button>
                    </div>
                    <div class="contenedor-barra-carrusel">
                        <div class="barra-carrusel">
                            <div class="indicador-barra-carrusel"></div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <section id="otros-productos" class="seccion-productos">
            <div class="contenedor">
                <h2 class="titulo-seccion">Otros Productos</h2>
                <div class="contenedor-carrusel">
                    <div class="carrusel-wrapper">
                        <button type="button" class="boton-carrusel boton-anterior" aria-label="Productos anteriores">‹</button>
                        <div class="carrusel">
                            <asp:Repeater ID="rptOtros" runat="server">
                                <ItemTemplate>
                                    <div class="item-carrusel">
                                        <img src='<%# Eval("UrlImagen") %>' alt='<%# Eval("Titulo") %>' class="imagen-producto" />
                                        <div class="info-producto">
                                            <h3><%# Eval("Titulo") %></h3>
                                            <p><%# Eval("Descripcion") %></p>
                                            <p class="precio-producto"><%# Eval("Precio") %></p>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        <button type="button" class="boton-carrusel boton-siguiente" aria-label="Productos siguientes">›</button>
                    </div>
                    <div class="contenedor-barra-carrusel">
                        <div class="barra-carrusel">
                            <div class="indicador-barra-carrusel"></div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <section id="servicios" class="seccion-servicios">
            <div class="contenedor">
                <h2 class="titulo-seccion">Nuestros Servicios</h2>
                <div class="contenedor-servicios">
                    <div class="item-servicio">
                        <h3>Exámenes de Cortesía</h3>
                        <p>Evaluaciones visuales completas sin costo adicional con cualquier compra. Realizadas por profesionales certificados.</p>
                    </div>
                    <div class="item-servicio">
                        <h3>Garantías Extendidas</h3>
                        <p>Todos nuestros productos incluyen garantía contra defectos de fabricación.</p>
                    </div>
                    <div class="item-servicio">
                        <h3>Reparaciones Especializadas</h3>
                        <p>Servicio técnico para reparación de armazones, cambio de cristales, ajustes y mantenimiento preventivo de tus lentes.</p>
                    </div>
                </div>
            </div>
        </section>

        <section id="nosotros" class="seccion-nosotros">
            <div class="contenedor">
                <h2 class="titulo-seccion-nosotros">Sobre Nosotros</h2>
                <div class="contenedor-nosotros">
                    <div class="item-nosotros">
                        <h3>Misión</h3>
                        <p>Mejorar la calidad de vida mediante soluciones visuales integrales y atención personalizada.</p>
                    </div>
                    <div class="item-nosotros">
                        <h3>Visión</h3>
                        <p>Ser líderes en innovación óptica y servicio excepcional en nuestra comunidad.</p>
                    </div>
                    <div class="item-nosotros">
                        <h3>Compromiso</h3>
                        <p>Calidad garantizada en cada producto y atención dedicada a cada cliente.</p>
                    </div>
                </div>
            </div>
        </section>

        <section id="contacto" class="seccion-contacto">
            <div class="contenedor contenedor-contacto">
                <div class="info-contacto">
                    <h3>Contacto</h3>
                    <ul class="lista-contacto">
                        <li>Teléfono: (505) 2233-4455</li>
                        <li>WhatsApp: (505) 8877-6655</li>
                        <li>Email: info@optica2020.com</li>
                    </ul>
                </div>
                <div class="info-contacto">
                    <h3>Dirección</h3>
                    <p>Av. Principal #123<br />Colonia Centro<br />Managua, Nicaragua</p>
                </div>
                <div class="info-contacto">
                    <h3>Horarios</h3>
                    <p>Lunes a Viernes: 8:00 AM - 6:00 PM<br />Sábados: 8:00 AM - 2:00 PM<br />Domingos: Cerrado</p>
                </div>
                <div class="info-contacto">
                    <h3>Redes Sociales</h3>
                    <a href="https://facebook.com/optica2020" class="boton-red-social" target="_blank" rel="noopener noreferrer">Facebook</a>
                    <a href="https://instagram.com/optica2020" class="boton-red-social" target="_blank" rel="noopener noreferrer">Instagram</a>
                    <a href="https://twitter.com/optica2020" class="boton-red-social" target="_blank" rel="noopener noreferrer">Twitter</a>
                </div>
            </div>
        </section>

        <div id="modal-imagen" class="modal-imagen">
            <div class="modal-contenido">
                <button type="button" class="cerrar-modal" aria-label="Cerrar modal">&times;</button>
                <img id="modal-imagen-ampliada" class="modal-imagen-ampliada" alt="Imagen ampliada" />
                <div class="controles-modal">
                    <button type="button" class="boton-modal boton-modal-anterior" aria-label="Imagen anterior">‹</button>
                    <button type="button" class="boton-modal boton-modal-siguiente" aria-label="Imagen siguiente">›</button>
                </div>
            </div>
        </div>

        <div class="boton-whatsapp">
            <svg width="36" height="36" viewBox="0 0 24 24" fill="white">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893c0-3.18-1.24-6.179-3.495-8.428"/>
            </svg>
        </div>
    </form>
    
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const botonesNavegacion = document.querySelectorAll('.boton-navegacion');
            botonesNavegacion.forEach(boton => {
                boton.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    if (!this.classList.contains('boton-iniciar-sesion')) {
                        const destinoId = this.getAttribute('data-destino');
                        const destinoElemento = document.getElementById(destinoId);

                        if (destinoElemento) {
                            const headerHeight = document.querySelector('.encabezado').offsetHeight;
                            const destinoPosicion = destinoElemento.offsetTop - headerHeight - 20;

                            window.scrollTo({
                                top: destinoPosicion,
                                behavior: 'smooth'
                            });
                        }
                    }
                });
            });

            const botonesCarrusel = document.querySelectorAll('.boton-carrusel');
            botonesCarrusel.forEach(boton => {
                boton.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                });
            });

            inicializarTodasLasFuncionalidades();
        });

        // ✅ SOLO ESTO SE CAMBIÓ: Ruta absoluta a Login.aspx
        function redirigirLogin() {
            window.location.href = '/Formularios/Login.aspx';
        }

        const carruseles = document.querySelectorAll('.carrusel');
        const modal = document.getElementById('modal-imagen');
        const modalImagen = document.getElementById('modal-imagen-ampliada');
        const cerrarModal = document.querySelector('.cerrar-modal');
        const botonModalAnterior = document.querySelector('.boton-modal-anterior');
        const botonModalSiguiente = document.querySelector('.boton-modal-siguiente');

        let imagenesProductos = [];
        let imagenesPromociones = [];
        let imagenesOtrosProductos = [];

        let imagenesActuales = [];
        let indiceImagenActual = 0;
        let seccionActual = '';

        function inicializarCarruseles() {
            const botonesAnterior = document.querySelectorAll('.boton-anterior');
            const botonesSiguiente = document.querySelectorAll('.boton-siguiente');

            botonesAnterior.forEach(boton => {
                boton.style.display = 'flex';
                boton.style.visibility = 'visible';
            });
            botonesSiguiente.forEach(boton => {
                boton.style.display = 'flex';
                boton.style.visibility = 'visible';
            });

            carruseles.forEach((carrusel) => {
                inicializarCarruselConBarra(carrusel);
            });
        }

        function inicializarCarruselConBarra(carrusel) {
            const contenedorCarrusel = carrusel.closest('.contenedor-carrusel');
            const barraCarrusel = contenedorCarrusel.querySelector('.barra-carrusel');
            const indicadorBarra = contenedorCarrusel.querySelector('.indicador-barra-carrusel');
            const botonAnterior = contenedorCarrusel.querySelector('.boton-anterior');
            const botonSiguiente = contenedorCarrusel.querySelector('.boton-siguiente');

            const primerItem = carrusel.querySelector('.item-carrusel');
            if (!primerItem) return;

            function obtenerDimensiones() {
                const anchoItem = primerItem.offsetWidth;
                const estiloComputado = window.getComputedStyle(carrusel);
                const gap = parseInt(estiloComputado.columnGap) || parseInt(estiloComputado.gap) || 30;
                const anchoTotalItem = anchoItem + gap;

                const cantidadItems = carrusel.children.length;
                const anchoTotalCarrusel = cantidadItems * anchoTotalItem;
                const anchoVisible = carrusel.parentElement.clientWidth;
                const maxDesplazamiento = Math.max(0, anchoTotalCarrusel - anchoVisible);

                return { anchoTotalItem, maxDesplazamiento, anchoVisible };
            }

            let { anchoTotalItem, maxDesplazamiento, anchoVisible } = obtenerDimensiones();
            let desplazamientoActual = 0;
            let isScrolling = false;
            let scrollTimeout;

            function actualizarBarra() {
                if (maxDesplazamiento > 0 && desplazamientoActual <= maxDesplazamiento) {
                    const porcentaje = (desplazamientoActual / maxDesplazamiento) * 100;
                    indicadorBarra.style.width = `${Math.min(100, Math.max(0, porcentaje))}%`;
                } else {
                    indicadorBarra.style.width = maxDesplazamiento > 0 ? '0%' : '100%';
                }
            }

            function actualizarVisibilidadBotones() {
                if (botonAnterior) {
                    botonAnterior.style.opacity = desplazamientoActual <= 5 ? '0.5' : '1';
                    botonAnterior.style.pointerEvents = desplazamientoActual <= 5 ? 'none' : 'auto';
                }
                if (botonSiguiente) {
                    botonSiguiente.style.opacity = desplazamientoActual >= maxDesplazamiento - 5 ? '0.5' : '1';
                    botonSiguiente.style.pointerEvents = desplazamientoActual >= maxDesplazamiento - 5 ? 'none' : 'auto';
                }
            }

            function desplazarCarrusel(nuevaPosicion) {
                if (isScrolling) return;

                isScrolling = true;
                desplazamientoActual = Math.max(0, Math.min(nuevaPosicion, maxDesplazamiento));

                carrusel.scrollTo({
                    left: desplazamientoActual,
                    behavior: 'smooth'
                });

                actualizarBarra();
                actualizarVisibilidadBotones();

                clearTimeout(scrollTimeout);
                scrollTimeout = setTimeout(() => {
                    isScrolling = false;
                }, 400);
            }

            function desplazarUnItem(direccion) {
                const desplazamiento = direccion * anchoTotalItem;
                desplazarCarrusel(desplazamientoActual + desplazamiento);
            }

            if (botonAnterior) {
                botonAnterior.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    desplazarUnItem(-1);
                });
            }

            if (botonSiguiente) {
                botonSiguiente.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    desplazarUnItem(1);
                });
            }

            carrusel.addEventListener('scroll', () => {
                if (!isScrolling) {
                    desplazamientoActual = carrusel.scrollLeft;
                    actualizarBarra();
                    actualizarVisibilidadBotones();
                }
            });

            carrusel.addEventListener('wheel', (evento) => {
                if (evento.deltaY !== 0) {
                    evento.preventDefault();
                    const scrollAmount = evento.deltaY * 0.8;
                    desplazarCarrusel(desplazamientoActual + scrollAmount);
                }
            }, { passive: false });

            let startX, scrollLeft, isDown = false;

            carrusel.addEventListener('mousedown', (e) => {
                isDown = true;
                carrusel.style.cursor = 'grabbing';
                startX = e.pageX - carrusel.offsetLeft;
                scrollLeft = carrusel.scrollLeft;
                isScrolling = false;
            });

            carrusel.addEventListener('mouseleave', () => {
                isDown = false;
                carrusel.style.cursor = 'grab';
            });

            carrusel.addEventListener('mouseup', () => {
                isDown = false;
                carrusel.style.cursor = 'grab';
            });

            carrusel.addEventListener('mousemove', (e) => {
                if (!isDown) return;
                e.preventDefault();
                const x = e.pageX - carrusel.offsetLeft;
                const walk = (x - startX) * 1.5;
                carrusel.scrollLeft = scrollLeft - walk;
                desplazamientoActual = carrusel.scrollLeft;
                actualizarBarra();
                actualizarVisibilidadBotones();
            });

            carrusel.addEventListener('touchstart', (e) => {
                isDown = true;
                startX = e.touches[0].pageX - carrusel.offsetLeft;
                scrollLeft = carrusel.scrollLeft;
                isScrolling = false;
            }, { passive: true });

            carrusel.addEventListener('touchend', () => {
                isDown = false;
            });

            carrusel.addEventListener('touchmove', (e) => {
                if (!isDown) return;
                const x = e.touches[0].pageX - carrusel.offsetLeft;
                const walk = (x - startX) * 1.2;
                carrusel.scrollLeft = scrollLeft - walk;
                desplazamientoActual = carrusel.scrollLeft;
                actualizarBarra();
                actualizarVisibilidadBotones();
            }, { passive: true });

            let resizeTimeout;
            window.addEventListener('resize', () => {
                clearTimeout(resizeTimeout);
                resizeTimeout = setTimeout(() => {
                    const nuevasDimensiones = obtenerDimensiones();
                    anchoTotalItem = nuevasDimensiones.anchoTotalItem;
                    maxDesplazamiento = nuevasDimensiones.maxDesplazamiento;
                    anchoVisible = nuevasDimensiones.anchoVisible;

                    if (desplazamientoActual > maxDesplazamiento) {
                        desplazarCarrusel(maxDesplazamiento);
                    }

                    actualizarBarra();
                    actualizarVisibilidadBotones();
                }, 150);
            });

            actualizarBarra();
            actualizarVisibilidadBotones();
        }

        function inicializarBotonWhatsApp() {
            const botonWhatsApp = document.querySelector('.boton-whatsapp');

            if (!botonWhatsApp) return;

            botonWhatsApp.addEventListener('click', () => {
                const numeroTelefono = '50588776655';
                const mensaje = 'Hola, me gustaría agendar una cita en la óptica 20/20 y obtener más información sobre sus productos.';
                const urlWhatsApp = `https://wa.me/${numeroTelefono}?text=${encodeURIComponent(mensaje)}`;
                window.open(urlWhatsApp, '_blank', 'noopener,noreferrer');
            });
        }

        function inicializarRedesSociales() {
            const botonesRedes = document.querySelectorAll('.boton-red-social');

            if (botonesRedes.length === 0) return;

            const urlsRedes = {
                'Facebook': 'https://www.facebook.com/profile.php?id=61581543142157',
                'Instagram': 'https://instagram.com/optica2020',
                'Twitter': 'https://twitter.com/optica2020'
            };

            botonesRedes.forEach(boton => {
                const redSocial = boton.textContent.trim();
                if (urlsRedes[redSocial]) {
                    boton.href = urlsRedes[redSocial];
                    boton.target = '_blank';
                    boton.rel = 'noopener noreferrer';
                }
            });
        }

        function inicializarNavegacionSuave() {
            const enlacesNavegacion = document.querySelectorAll('.boton-navegacion');

            if (enlacesNavegacion.length === 0) return;

            enlacesNavegacion.forEach(enlace => {
                enlace.addEventListener('click', (evento) => {
                    evento.preventDefault();

                    const destinoId = enlace.getAttribute('data-destino');
                    const destinoElemento = document.getElementById(destinoId);

                    if (destinoElemento) {
                        const headerHeight = document.querySelector('.encabezado').offsetHeight;
                        const destinoPosicion = destinoElemento.offsetTop - headerHeight - 20;

                        window.scrollTo({
                            top: destinoPosicion,
                            behavior: 'smooth'
                        });
                    }
                });
            });
        }

        function inicializarModalImagen() {
            const itemsProductos = document.querySelectorAll('#productos .item-carrusel');
            const itemsPromociones = document.querySelectorAll('#promociones .item-carrusel');
            const itemsOtrosProductos = document.querySelectorAll('#otros-productos .item-carrusel');

            imagenesProductos = [];
            imagenesPromociones = [];
            imagenesOtrosProductos = [];

            itemsProductos.forEach((item, indice) => {
                const imagen = item.querySelector('.imagen-producto');
                if (imagen) {
                    imagen.addEventListener('click', () => abrirModal('productos', indice));
                    imagenesProductos.push(imagen.src);
                }
            });

            itemsPromociones.forEach((item, indice) => {
                const imagen = item.querySelector('.imagen-producto');
                if (imagen) {
                    imagen.addEventListener('click', () => abrirModal('promociones', indice));
                    imagenesPromociones.push(imagen.src);
                }
            });

            itemsOtrosProductos.forEach((item, indice) => {
                const imagen = item.querySelector('.imagen-producto');
                if (imagen) {
                    imagen.addEventListener('click', () => abrirModal('otros-productos', indice));
                    imagenesOtrosProductos.push(imagen.src);
                }
            });

            if (cerrarModal) {
                cerrarModal.addEventListener('click', cerrarModalImagen);
            }

            if (modal) {
                modal.addEventListener('click', (evento) => {
                    if (evento.target === modal) {
                        cerrarModalImagen();
                    }
                });
            }

            if (botonModalAnterior) {
                botonModalAnterior.addEventListener('click', imagenAnterior);
            }

            if (botonModalSiguiente) {
                botonModalSiguiente.addEventListener('click', imagenSiguiente);
            }

            document.addEventListener('keydown', (evento) => {
                if (modal && modal.style.display === 'flex') {
                    if (evento.key === 'Escape') {
                        cerrarModalImagen();
                    } else if (evento.key === 'ArrowLeft') {
                        imagenAnterior();
                    } else if (evento.key === 'ArrowRight') {
                        imagenSiguiente();
                    }
                }
            });
        }

        function abrirModal(seccion, indice) {
            seccionActual = seccion;

            switch (seccion) {
                case 'productos': imagenesActuales = imagenesProductos; break;
                case 'promociones': imagenesActuales = imagenesPromociones; break;
                case 'otros-productos': imagenesActuales = imagenesOtrosProductos; break;
                default: return;
            }

            if (indice < 0 || indice >= imagenesActuales.length) return;

            indiceImagenActual = indice;

            if (modalImagen) {
                modalImagen.src = imagenesActuales[indiceImagenActual];
                modalImagen.alt = `Imagen ampliada - ${seccion} ${indice + 1}`;
            }

            if (modal) {
                modal.style.display = 'flex';
                document.body.style.overflow = 'hidden';
            }
        }

        function cerrarModalImagen() {
            if (modal) {
                modal.style.display = 'none';
                document.body.style.overflow = 'auto';
            }
        }

        function imagenAnterior() {
            if (imagenesActuales.length === 0) return;

            indiceImagenActual = (indiceImagenActual - 1 + imagenesActuales.length) % imagenesActuales.length;
            if (modalImagen) modalImagen.src = imagenesActuales[indiceImagenActual];
        }

        function imagenSiguiente() {
            if (imagenesActuales.length === 0) return;

            indiceImagenActual = (indiceImagenActual + 1) % imagenesActuales.length;
            if (modalImagen) modalImagen.src = imagenesActuales[indiceImagenActual];
        }

        function inicializarAnimacionesScroll() {
            const elementosAnimados = document.querySelectorAll('.item-nosotros, .item-servicio, .titulo-seccion');

            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                    }
                });
            }, { threshold: 0.1 });

            elementosAnimados.forEach(elemento => {
                elemento.style.opacity = '0';
                elemento.style.transform = 'translateY(30px)';
                elemento.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
                observer.observe(elemento);
            });
        }

        function inicializarTodasLasFuncionalidades() {
            try {
                inicializarCarruseles();
                inicializarBotonWhatsApp();
                inicializarRedesSociales();
                inicializarNavegacionSuave();
                inicializarModalImagen();
                inicializarAnimacionesScroll();
            } catch (error) {
            }
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', inicializarTodasLasFuncionalidades);
        } else {
            inicializarTodasLasFuncionalidades();
        }
    </script>
</body>
</html>