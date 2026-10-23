window.demoStorage = {
    load: () => localStorage.getItem('mapa-portfolio-v1'),
    save: (json) => localStorage.setItem('mapa-portfolio-v1', json),
    export: (json) => {
        const blob = new Blob([json], { type: 'application/json;charset=utf-8' });
        const url = URL.createObjectURL(blob),
            a = document.createElement('a');
        a.href = url;
        a.download = `mapa-demo-${new Date().toISOString().slice(0, 10)}.json`;
        a.click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    },
};
window.demoMap = (() => {
    let map,
        markers,
        tile,
        callback,
        tileFailed = false;
    const text = (value) => {
        const e = document.createElement('span');
        e.textContent = value;
        return e;
    };
    const icon = (shape, color, selected) =>
        L.divIcon({
            className: '',
            iconSize: [28, 28],
            iconAnchor: [14, 14],
            html: `<div style="height:26px;width:26px;display:grid;place-items:center;background:${color};color:#071321;border:2px solid ${selected ? '#fff' : '#ffffff66'};border-radius:${shape === '●' ? '50%' : '6px'};box-shadow:0 2px 10px #0008;font-size:16px;font-weight:bold">${shape}</div>`,
        });
    return {
        init(element, dotnet) {
            this.dispose();
            callback = dotnet;
            map = L.map(element, { attributionControl: true, zoomControl: true }).setView(
                [22.1565, -100.9855],
                13,
            );
            const Grid = L.GridLayer.extend({
                createTile(coords) {
                    const c = document.createElement('canvas');
                    c.width = c.height = 256;
                    const ctx = c.getContext('2d');
                    ctx.fillStyle = '#14263a';
                    ctx.fillRect(0, 0, 256, 256);
                    ctx.strokeStyle = '#29435a';
                    ctx.strokeRect(0, 0, 256, 256);
                    ctx.beginPath();
                    ctx.moveTo(128, 0);
                    ctx.lineTo(128, 256);
                    ctx.moveTo(0, 128);
                    ctx.lineTo(256, 128);
                    ctx.stroke();
                    ctx.fillStyle = '#7590a9';
                    ctx.font = '11px Segoe UI';
                    ctx.fillText('CUADRÍCULA DE SIMULACIÓN', 12, 24);
                    return c;
                },
            });
            new Grid({ zIndex: 0 }).addTo(map);
            markers = L.layerGroup().addTo(map);
            setTimeout(() => map?.invalidateSize(), 100);
        },
        render(state, units, cameras, closed, base, selected) {
            if (!map) return;
            if (base && !tile) {
                tileFailed = false;
                tile = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    zIndex: 1,
                    maxZoom: 19,
                    attribution:
                        '© <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a>',
                }).addTo(map);
                tile.on('tileerror', () => {
                    if (!tileFailed && map) {
                        tileFailed = true;
                        L.popup()
                            .setLatLng(map.getCenter())
                            .setContent(
                                text(
                                    'Mapa base no disponible. La cuadrícula y los registros locales siguen funcionando.',
                                ),
                            )
                            .openOn(map);
                    }
                });
            } else if (!base && tile) {
                map.removeLayer(tile);
                tile = null;
            }
            markers.clearLayers();
            for (const i of state.incidents.filter((i) => closed || i.status !== 'Cerrado')) {
                const color =
                    i.status === 'Cerrado'
                        ? '#91a5bc'
                        : i.priority === 'Alta'
                          ? '#ff927f'
                          : i.priority === 'Media'
                            ? '#f2c96d'
                            : '#78dac7';
                const marker = L.marker([i.lat, i.lng], {
                    icon: icon('●', color, i.id === selected),
                    title: `SIM-${i.id}: ${i.title}`,
                }).addTo(markers);
                marker.bindTooltip(text(`SIM-${i.id} · ${i.title}`));
                marker.on('click', () => callback?.invokeMethodAsync('SelectIncident', i.id));
                if (i.unitId && i.status !== 'Cerrado') {
                    const u = state.units.find((u) => u.id === i.unitId);
                    if (u)
                        L.polyline(
                            [
                                [u.lat, u.lng],
                                [i.lat, i.lng],
                            ],
                            { color: '#65d9c7', weight: 2, dashArray: '5 6' },
                        )
                            .bindTooltip(text('Asignación simulada · sin ruta de navegación'))
                            .addTo(markers);
                }
            }
            if (units)
                for (const u of state.units)
                    L.marker([u.lat, u.lng], { icon: icon('◆', '#70bbff', false), title: u.id })
                        .bindPopup(text(`${u.id} · ${u.specialty} · posición ficticia`))
                        .addTo(markers);
            if (cameras)
                for (const c of state.cameras)
                    L.marker([c.lat, c.lng], {
                        icon: icon('■', c.online ? '#b4a0ff' : '#75859b', false),
                        title: c.id,
                    })
                        .bindTooltip(text(c.id + ' · simulación'))
                        .on('click', () => callback?.invokeMethodAsync('SelectCamera', c.id))
                        .addTo(markers);
            map.invalidateSize();
        },
        center() {
            map?.setView([22.1565, -100.9855], 13);
        },
        dispose() {
            if (map) map.remove();
            map = null;
            markers = null;
            tile = null;
            callback = null;
        },
    };
})();
window.demoUi = { scrollToTop: () => document.querySelector('main')?.scrollTo(0, 0) };
