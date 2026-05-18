# SDMapViewerCom

> Control COM para Clarion — Visor de mapas interactivo basado en WebView2 y Leaflet.js

[![Version](https://img.shields.io/badge/version-1.1.2-blue)](CHANGELOG.md)
[![Platform](https://img.shields.io/badge/platform-x86%20%7C%20.NET%204.7.2-lightgrey)](SDMapViewerCom.csproj)
[![COM](https://img.shields.io/badge/COM-RegFree%20%28manifest%29-green)](SDMapViewerComControl.manifest)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

---

## Descripción

**SDMapViewerCom** es un control ActiveX/COM para Clarion que embebe un mapa interactivo usando WebView2 (Chromium) y [Leaflet.js](https://leafletjs.com/). Permite mostrar mapas, agregar marcadores, geocodificar direcciones, medir distancias y cambiar tipos de mapa, todo desde código Clarion.

No requiere registro en el sistema (RegFree COM via manifest).

---

## Requisitos

| Requisito | Versión |
|---|---|
| Clarion | 11 o superior |
| .NET Framework | 4.7.2 |
| WebView2 Runtime | Incluido en el paquete de despliegue |
| Arquitectura | x86 (32-bit) |
| OS | Windows 10 / 11 |

---

## Instalación

### Desde el marketplace ClarionCOM

```
/ClarionCOM get SDMapViewerCom
```

### Manual

1. Copiar los archivos del directorio `Clarion/accessory/bin/` a `<Clarion>\accessory\bin\`
2. Copiar los archivos de `Clarion/accessory/resources/` a `<Clarion>\accessory\resources\`
3. Asegurarse de que `wwwroot/` quede en `<Clarion>\accessory\resources\wwwroot\`
4. Incluir el manifest en la aplicación Clarion (ver documentación ClarionCOM)

---

## Uso rápido en Clarion

```clarion
! Declarar la variable
MapViewer   SDMapViewerComControl

! Inicializar y mostrar
MapViewer.SetView(-34.6037, -58.3816, 13)  ! Buenos Aires, zoom 13

! Agregar un marcador
MapViewer.AddMarker('sede1', -34.6037, -58.3816, 'Sede Central', 'Dirección: ...', 'blue')

! Cambiar tipo de mapa
MapViewer.SetMapType('satellite')

! Activar medición de distancias
MapViewer.EnableMeasure(TRUE)
```

---

## Referencia de la API

### Propiedades (getters)

| Método | Retorno | Descripción |
|---|---|---|
| `GetIsReady()` | BOOL | El control está inicializado y listo |
| `GetLastError()` | STRING | Último mensaje de error |
| `GetLatitude()` | DOUBLE | Latitud del centro actual |
| `GetLongitude()` | DOUBLE | Longitud del centro actual |
| `GetZoom()` | INT | Nivel de zoom actual (1–18) |
| `GetMapType()` | STRING | Tipo de mapa actual (`normal`, `satellite`, `terrain`) |
| `GetScaleVisible()` | BOOL | Indica si la escala está visible |
| `GetIsMeasuring()` | BOOL | Indica si el modo de medición está activo |
| `GetMarkerCount()` | INT | Cantidad de marcadores en el mapa |

---

### Inicialización del mapa

| Método | Descripción |
|---|---|
| `SetCenter(lat, lng)` | Centra el mapa en las coordenadas indicadas |
| `SetCenterByAddress(address, city, province, country)` | Centra el mapa geocodificando una dirección |
| `SetCenterByCity(city, province, country)` | Centra el mapa geocodificando una ciudad |
| `SetZoom(zoom)` | Establece el nivel de zoom (1–18) |
| `SetView(lat, lng, zoom)` | Centra y hace zoom en un paso |
| `SetViewByAddress(address, city, province, country, zoom)` | `SetView` por dirección |
| `SetViewByCity(city, province, country, zoom)` | `SetView` por ciudad |

---

### Navegación

| Método | Descripción |
|---|---|
| `PanBy(deltaX, deltaY)` | Desplaza el mapa en píxeles |
| `ZoomIn()` | Aumenta el zoom un nivel |
| `ZoomOut()` | Reduce el zoom un nivel |
| `FitBounds()` | Ajusta la vista para mostrar todos los marcadores |

---

### Marcadores — Por coordenadas

| Método | Descripción |
|---|---|
| `AddMarker(id, lat, lng, title, description, color)` | Agrega marcador con color. Colores: `red`, `blue`, `green`, `orange`, `yellow`, `violet`, `grey`, `black` |
| `AddMarkerSimple(id, lat, lng, title)` | Agrega marcador azul sin descripción |

### Marcadores — Por dirección (geocoding)

| Método | Descripción |
|---|---|
| `AddMarkerByAddress(id, address, city, province, country, title, description, color)` | Agrega marcador geocodificando una dirección |
| `AddMarkerByCity(id, city, province, country, title, description, color)` | Agrega marcador geocodificando una ciudad |

### Gestión de marcadores

| Método | Descripción |
|---|---|
| `RemoveMarker(id)` | Elimina un marcador por ID |
| `ClearMarkers()` | Elimina todos los marcadores |
| `UpdateMarkerPosition(id, lat, lng)` | Mueve un marcador |
| `UpdateMarkerColor(id, color)` | Cambia el color de un marcador |
| `UpdateMarkerDescription(id, description)` | Actualiza el contenido del popup |
| `ShowMarkerPopup(id)` | Muestra el popup de un marcador |
| `CenterOnMarker(id)` | Centra el mapa en un marcador |

---

### Capas de tiles

| Método | Descripción |
|---|---|
| `SetTileLayer(urlTemplate, attribution)` | Define una capa de tiles personalizada (`{x}`, `{y}`, `{z}` en la URL) |
| `UseOpenStreetMap()` | Vuelve a OpenStreetMap (capa por defecto) |

---

### Tipo de mapa

| Método | Valores | Descripción |
|---|---|---|
| `SetMapType(mapType)` | `normal`, `satellite`, `terrain` | Cambia la capa base del mapa |

- **normal** → OpenStreetMap
- **satellite** → Esri World Imagery
- **terrain** → OpenTopoMap

---

### Control de escala

| Método | Descripción |
|---|---|
| `ShowScale(show)` | Muestra u oculta la escala gráfica (métrica e imperial) |
| `SetScalePosition(position)` | Posición: `topleft`, `topright`, `bottomleft`, `bottomright` |

---

### Medición de distancias

| Método | Descripción |
|---|---|
| `EnableMeasure(enable)` | Activa/desactiva el modo de medición. Al hacer clic se agregan puntos |
| `ClearMeasurements()` | Borra todos los puntos y líneas de medición |

El cálculo usa la fórmula de Haversine para precisión geográfica real.

---

### Utilidades

| Método | Descripción |
|---|---|
| `RefreshMap()` | Recarga el mapa |
| `InvalidateSize()` | Recalcula el tamaño del mapa (usar tras redimensionar el contenedor) |
| `About()` | Muestra un diálogo con versión e información del control |
| `ExecuteScript(script)` | Ejecuta JavaScript directamente sobre el mapa |

---

## Eventos

| Evento | Parámetros | Se dispara cuando... |
|---|---|---|
| `ControlReady()` | — | WebView2 y el mapa están listos para usar |
| `ErrorOccurred(errorMessage)` | STRING | Ocurre cualquier error interno |
| `MapMoved(latitude, longitude)` | DOUBLE, DOUBLE | El mapa se mueve (pan o SetCenter) |
| `ZoomChanged(zoom)` | INT | Cambia el nivel de zoom |
| `MarkerClicked(markerId, latitude, longitude)` | STRING, DOUBLE, DOUBLE | Se hace clic sobre un marcador |
| `MapClicked(latitude, longitude)` | DOUBLE, DOUBLE | Se hace clic en el mapa (no sobre marcador) |
| `MapDoubleClicked(latitude, longitude)` | DOUBLE, DOUBLE | Se hace doble clic en el mapa |
| `GeocodingComplete(requestId, latitude, longitude, displayName)` | STRING, DOUBLE, DOUBLE, STRING | El geocoding fue exitoso |
| `GeocodingFailed(requestId, errorMessage)` | STRING, STRING | El geocoding falló |
| `MarkerAdded(markerId, latitude, longitude)` | STRING, DOUBLE, DOUBLE | Se agregó un marcador al mapa |
| `MeasurementComplete(distanceMeters, distanceKm, distanceMiles, pointCount)` | DOUBLE, DOUBLE, DOUBLE, INT | Se actualizó la medición de distancia |
| `MapTypeChanged(mapType)` | STRING | Se cambió el tipo de mapa |

> **Nota sobre geocoding asíncrono:** `SetCenterByAddress`, `AddMarkerByAddress` y similares son operaciones asíncronas. El resultado llega a través de los eventos `GeocodingComplete` / `GeocodingFailed`. El parámetro `requestId` coincide con el `id` del marcador, o con `"center"` para operaciones de centrado.

---

## Estructura del proyecto

```
SDMapViewerCom/
├── ISDMapViewerComControl.cs        # Interfaz COM (métodos expuestos a Clarion)
├── ISDMapViewerComControlEvents.cs  # Interfaz de eventos COM
├── SDMapViewerComControl.cs         # Implementación del control
├── SDMapViewerComControl.manifest   # Manifest RegFree COM
├── SDMapViewerCom.csproj            # Proyecto .NET
├── Properties/
│   └── AssemblyInfo.cs              # Versión y metadatos del ensamblado
├── wwwroot/
│   ├── css/
│   │   └── styles.css
│   └── controls/sdmapviewercom/
│       ├── index.html               # HTML del mapa
│       └── app.js                   # Lógica Leaflet.js
└── CHANGELOG.md
```

---

## Tecnologías utilizadas

- [Leaflet.js](https://leafletjs.com/) — biblioteca de mapas interactivos
- [Microsoft WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) — motor Chromium embebido
- [Nominatim / OpenStreetMap](https://nominatim.org/) — geocoding de direcciones
- [Newtonsoft.Json](https://www.newtonsoft.com/json) — serialización JSON para comunicación JS↔C#

---

## Changelog

Ver [CHANGELOG.md](CHANGELOG.md) para el historial completo de versiones.

---

## Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

## ☕ ¿Te fue útil?

Si esta herramienta te ahorró tiempo, podés invitarme un café:

[![Donar con PayPal](https://www.paypalobjects.com/es_ES/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate/?business=informacion@sdigitales.com.ar)