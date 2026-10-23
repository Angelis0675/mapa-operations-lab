# Verificación de esta entrega

- Publicación Release de Blazor WebAssembly completada y cargada desde un servidor de archivos estáticos.
- Registro, asignación, llegada, cierre y restauración del estado al recargar comprobados en el navegador.
- Comprobaciones del dominio ejecutadas correctamente con `tests/Mapa.Checks`.
- Interfaz comprobada a 390 px y en escritorio; sin desbordamiento horizontal en la vista móvil inicial.
- Ruta de proyecto (`/mapa-operations-lab/`) probada con la misma transformación de `base href` que utiliza el workflow.
- Transformación del workflow comprobada para repositorios de proyecto y repositorios `usuario.github.io`.
- Búsqueda local sin coincidencias para las claves y endpoints históricos conocidos. No se incluyen módulos de integración antiguos ni archivos de su historial.
- Avisos de Leaflet conservados.

Estas comprobaciones no equivalen a una auditoría de seguridad, cobertura total ni a un despliegue ya ejecutado en una cuenta de GitHub. El workflow remoto se ejecutará cuando se suba el repositorio y se habilite Pages.

Referencia para el modelo de publicación estática: [documentación de Microsoft sobre Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/webassembly/?view=aspnetcore-8.0).
