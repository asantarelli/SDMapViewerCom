# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

## [1.1.3] - 2026-05-18

### Fixed
- Corrección de bug en la implementación

## [1.1.2] - 2026-05-08

### Changed
- Actualización de Webview

## [1.1.0] - 2026-02-03

### Added
- **Map types** - Switch between different map visualizations
  - `SetMapType(type)` - Set map type: "normal", "satellite", "terrain"
  - `GetMapType()` - Get current map type
  - Normal: OpenStreetMap tiles
  - Satellite: Esri World Imagery
  - Terrain: OpenTopoMap
- **Scale control** - Visual scale indicator on the map
  - `ShowScale(show)` - Show or hide the scale control
  - `SetScalePosition(position)` - Set position: "topleft", "topright", "bottomleft", "bottomright"
  - `GetScaleVisible()` - Check if scale is visible
  - Displays both metric and imperial units
- **Distance measurement tool** - Measure distances by clicking on the map
  - `EnableMeasure(enable)` - Enable/disable measurement mode
  - `ClearMeasurements()` - Clear all measurement points and lines
  - `GetIsMeasuring()` - Check if measurement mode is active
  - Numbered point markers with connecting dashed lines
  - Distance labels for each segment
  - Uses Haversine formula for accurate geographic distance calculation
- **New events**
  - `MeasurementComplete(distanceMeters, distanceKm, distanceMiles, pointCount)` - Measurement updated
  - `MapTypeChanged(mapType)` - Map type was changed

## [1.0.1] - 2026-01-30

### Added
- **Geocoding support** using Nominatim (OpenStreetMap)
  - `SetCenterByAddress(address, city, province, country)` - Center map by full address
  - `SetCenterByCity(city, province, country)` - Center map by city/locality
  - `SetViewByAddress(...)` - Set center and zoom by address
  - `SetViewByCity(...)` - Set center and zoom by city
- **Markers by address**
  - `AddMarkerByAddress(...)` - Add marker using geocoded address
  - `AddMarkerByCity(...)` - Add marker using city/locality
- **Colored markers** with 8 color options: red, blue, green, orange, yellow, violet, grey, black
  - `AddMarker(id, lat, lng, title, description, color)` - Full marker with color
  - `AddMarkerSimple(id, lat, lng, title)` - Simple blue marker
  - `UpdateMarkerColor(id, color)` - Change marker color
  - `UpdateMarkerDescription(id, description)` - Update popup content
- **New events**
  - `GeocodingComplete(requestId, lat, lng, displayName)` - Geocoding succeeded
  - `GeocodingFailed(requestId, errorMessage)` - Geocoding failed
  - `MarkerAdded(markerId, lat, lng)` - Marker was added
- **Marker management**
  - `CenterOnMarker(id)` - Center map on a specific marker
  - `GetMarkerCount()` - Get number of markers

## [1.0.0] - 2026-01-30

### Added
- Initial release
- Interactive map viewer using Leaflet.js
- Map control: SetCenter, SetZoom, SetView, PanBy, ZoomIn, ZoomOut, FitBounds
- Basic marker support
- Tile layer configuration: SetTileLayer, UseOpenStreetMap
- Events: ControlReady, ErrorOccurred, MapMoved, ZoomChanged, MarkerClicked, MapClicked, MapDoubleClicked
- WebView2 runtime support
- Registration-free COM (manifest-based)
