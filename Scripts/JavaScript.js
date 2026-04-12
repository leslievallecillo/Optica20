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

function redirigirLogin() {
    window.location.href = 'Login.aspx';
}

const carruseles = document.querySelectorAll('.carrusel');
const modal = document.getElementById('modal-imagen');
const modalImagen = document.getElementById('modal-imagen-ampliada');
const cerrarModal = document.querySelector('.cerrar-modal');
const botonModalAnterior = document.querySelector('.boton-modal-anterior');
const botonModalSiguiente = document.querySelector('.boton-modal-siguiente');

const imagenesProductos = [];
const imagenesPromociones = [];
const imagenesOtrosProductos = [];

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

    const anchoItem = primerItem.offsetWidth;
    const margen = 25;
    const anchoTotalItem = anchoItem + margen;

    const cantidadItems = carrusel.children.length;
    const anchoTotalCarrusel = cantidadItems * anchoTotalItem;
    const anchoVisible = carrusel.parentElement.clientWidth;

    const maxDesplazamiento = Math.max(0, anchoTotalCarrusel - anchoVisible);

    let desplazamientoActual = 0;

    function actualizarBarra() {
        if (maxDesplazamiento > 0 && desplazamientoActual <= maxDesplazamiento) {
            const porcentaje = (desplazamientoActual / maxDesplazamiento) * 100;
            indicadorBarra.style.width = `${Math.min(100, Math.max(0, porcentaje))}%`;
        } else {
            indicadorBarra.style.width = '100%';
        }
    }

    function actualizarVisibilidadBotones() {
        if (botonAnterior) {
            botonAnterior.style.display = desplazamientoActual <= 10 ? 'none' : 'flex';
        }
        if (botonSiguiente) {
            botonSiguiente.style.display = desplazamientoActual >= maxDesplazamiento - 10 ? 'none' : 'flex';
        }
    }

    function desplazarCarrusel(nuevaPosicion) {
        desplazamientoActual = Math.max(0, Math.min(nuevaPosicion, maxDesplazamiento));
        carrusel.scrollTo({
            left: desplazamientoActual,
            behavior: 'smooth'
        });
        actualizarBarra();
        setTimeout(actualizarVisibilidadBotones, 300);
    }

    function desplazarPorItems(direccion) {
        const itemsPorView = Math.max(1, Math.floor(anchoVisible / anchoTotalItem));
        const desplazamiento = direccion * itemsPorView * anchoTotalItem;
        desplazarCarrusel(desplazamientoActual + desplazamiento);
    }

    if (botonAnterior) {
        botonAnterior.addEventListener('click', () => {
            desplazarPorItems(-1);
        });
    }

    if (botonSiguiente) {
        botonSiguiente.addEventListener('click', () => {
            desplazarPorItems(1);
        });
    }

    carrusel.addEventListener('scroll', () => {
        desplazamientoActual = carrusel.scrollLeft;
        actualizarBarra();
        actualizarVisibilidadBotones();
    });

    carrusel.addEventListener('wheel', (evento) => {
        evento.preventDefault();
        const scrollAmount = evento.deltaY * 0.8;
        carrusel.scrollLeft += scrollAmount;
        desplazamientoActual = carrusel.scrollLeft;
        actualizarBarra();
        actualizarVisibilidadBotones();
    });

    let startX, scrollLeft, isDown = false;

    carrusel.addEventListener('mousedown', (e) => {
        isDown = true;
        carrusel.classList.add('active');
        startX = e.pageX - carrusel.offsetLeft;
        scrollLeft = carrusel.scrollLeft;
    });

    carrusel.addEventListener('mouseleave', () => {
        isDown = false;
        carrusel.classList.remove('active');
    });

    carrusel.addEventListener('mouseup', () => {
        isDown = false;
        carrusel.classList.remove('active');
    });

    carrusel.addEventListener('mousemove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - carrusel.offsetLeft;
        const walk = (x - startX) * 2;
        carrusel.scrollLeft = scrollLeft - walk;
        desplazamientoActual = carrusel.scrollLeft;
        actualizarBarra();
        actualizarVisibilidadBotones();
    });

    carrusel.addEventListener('touchstart', (e) => {
        isDown = true;
        startX = e.touches[0].pageX - carrusel.offsetLeft;
        scrollLeft = carrusel.scrollLeft;
    });

    carrusel.addEventListener('touchend', () => {
        isDown = false;
    });

    carrusel.addEventListener('touchmove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.touches[0].pageX - carrusel.offsetLeft;
        const walk = (x - startX) * 2;
        carrusel.scrollLeft = scrollLeft - walk;
        desplazamientoActual = carrusel.scrollLeft;
        actualizarBarra();
        actualizarVisibilidadBotones();
    });

    window.addEventListener('resize', () => {
        setTimeout(() => {
            actualizarVisibilidadBotones();
            actualizarBarra();
        }, 100);
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

    botonWhatsApp.addEventListener('mouseenter', () => {
        botonWhatsApp.style.animation = 'pulso-whatsapp 0.5s infinite';
    });

    botonWhatsApp.addEventListener('mouseleave', () => {
        botonWhatsApp.style.animation = 'pulso-whatsapp 2s infinite';
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
        console.log('Todas las funcionalidades inicializadas correctamente');
    } catch (error) {
        console.error('Error al inicializar funcionalidades:', error);
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', inicializarTodasLasFuncionalidades);
} else {
    inicializarTodasLasFuncionalidades();
}