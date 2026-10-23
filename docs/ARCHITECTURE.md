# Decisiones de diseño

## Un módulo de mapas que se puede demostrar sin infraestructura

El proyecto anterior tenía una interfaz de mapas en MAUI/Blazor y dependencias de sistemas externos. Para el portafolio se utiliza exclusivamente la nueva demo local: interfaz, simulación y reglas creadas para este caso de estudio. No se copian componentes históricos ni su configuración. Blazor WebAssembly permite conservar el enfoque en C# y compartir una aplicación estática.

## Separación de responsabilidades

- `src/Mapa.Portfolio/Pages/Demo.razor`: composición de la experiencia, formularios y navegación por secciones.
- `Services/DemoStore.cs`: modelos, validación y reglas; no utiliza HTTP, JavaScript ni servicios de plataforma.
- `wwwroot/js/demo.js`: puentes del navegador para el mapa y los respaldos. Los textos de los marcadores se insertan como texto, no como HTML del usuario.
- `Pages/Demo.razor.css` y `wwwroot/css`: estilos adaptables de la interfaz.
- `tests/Mapa.Checks`: comprobaciones del dominio con fallo explícito y sin un framework adicional.

## Invariantes

1. Una unidad solo puede estar asignada a un incidente abierto.
2. Un incidente en sitio debe tener una unidad asignada.
3. El cierre necesita una nota y libera la unidad sin borrar la asignación histórica.
4. Una restauración se valida antes de reemplazar el estado.
5. Las coordenadas deben ser finitas y encontrarse dentro de los rangos geográficos válidos.

## Decisiones y compromisos

**Persistencia en localStorage.** Facilita una prueba sin cuenta ni servidor. No es almacenamiento confidencial, multiusuario ni duradero frente al borrado de datos del navegador. Los respaldos permiten trasladar el escenario manualmente.

**Interfaz única para una demo acotada.** El componente principal reúne varias secciones para que el recorrido sea fácil de seguir. Si creciera, se separarían el mapa, el editor, la bitácora y el catálogo en componentes y servicios coordinadores.

**Dominio sin infraestructura.** Se prueba sin abrir un navegador. No hay control de concurrencia entre pestañas; dos pestañas pueden sobrescribir el mismo guardado. En producción la asignación exclusiva debe garantizarse mediante transacciones en el servidor.

**JSON con metadatos generados.** `DemoJsonContext` declara el estado serializable en compilación para que los respaldos no dependan de reflexión al optimizar la publicación WebAssembly.

**Cartografía opcional.** Leaflet se distribuye localmente con su licencia. La cuadrícula permite inspeccionar marcadores sin teselas. OpenStreetMap aporta contexto geográfico cuando el visitante decide activarlo.

**Sin login ficticio.** Una contraseña de demostración no aportaría protección al estado cliente. La interfaz indica que es un entorno simulado, sin representar autenticación o autorización de producción.

## Cómo explicarlo en entrevista

- Mostrar el recorrido registro → asignación → llegada → cierre y describir la regla que protege cada transición.
- Mostrar que el cierre libera una unidad y que la bitácora permanece al recargar.
- Abrir las comprobaciones y explicar por qué se prueba la asignación exclusiva y la restauración inválida.
- Explicar qué cambia al pasar de un estado local a un sistema multiusuario: transacciones, autenticación, auditoría y actualización en tiempo real.
- Distinguir las partes implementadas de las siguientes iteraciones. No presentar esta demo como un sistema operativo de una institución.
