// SDMapViewerCom Control JavaScript
// Interactive map viewer using Leaflet.js with geocoding support

(function() {
    'use strict';

    const CONFIG = {
        controlName: 'SDMapViewerCom',
        debug: false,
        defaultLat: -34.6037,
        defaultLng: -58.3816,
        defaultZoom: 13,
        geocodeUrl: 'https://nominatim.openstreetmap.org/search'
    };

    let map = null;
    let tileLayer = null;
    let markers = {};
    let markerIcons = {};
    let currentMapType = 'normal';
    let scaleControl = null;
    let scalePosition = 'bottomleft';

    // Measurement state
    let isMeasuring = false;
    let measurePoints = [];
    let measureMarkers = [];
    let measureLine = null;
    let measureLabels = [];

    // Tile layer definitions
    const TILE_LAYERS = {
        normal: {
            url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        },
        satellite: {
            url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
            attribution: '&copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community',
            maxZoom: 19
        },
        terrain: {
            url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
            attribution: '&copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
            maxZoom: 17
        }
    };

    // Marker color definitions using Leaflet's default icon with color overlays
    const MARKER_COLORS = {
        blue: '#2A81CB',
        red: '#CB2B3E',
        green: '#2AAD27',
        orange: '#CB8427',
        yellow: '#CAC428',
        violet: '#9C2BCB',
        grey: '#7B7B7B',
        black: '#3D3D3D'
    };

    /**
     * Send message to C# host
     */
    function sendToCSharp(type, data) {
        try {
            const message = JSON.stringify({ type: type, ...data });
            if (typeof chrome !== 'undefined' && chrome.webview) {
                chrome.webview.postMessage(message);
            }
            if (CONFIG.debug) console.log('Sent to C#:', message);
        } catch (error) {
            console.error('Error sending message to C#:', error);
        }
    }

    /**
     * Create a colored marker icon
     */
    function getMarkerIcon(color) {
        const colorCode = MARKER_COLORS[color.toLowerCase()] || MARKER_COLORS.blue;

        if (!markerIcons[colorCode]) {
            markerIcons[colorCode] = L.divIcon({
                className: 'custom-marker',
                html: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 36" width="25" height="41">
                    <path fill="${colorCode}" stroke="#000" stroke-width="1" d="M12 0C5.4 0 0 5.4 0 12c0 7.2 12 24 12 24s12-16.8 12-24c0-6.6-5.4-12-12-12z"/>
                    <circle fill="#fff" cx="12" cy="12" r="5"/>
                </svg>`,
                iconSize: [25, 41],
                iconAnchor: [12, 41],
                popupAnchor: [1, -34]
            });
        }
        return markerIcons[colorCode];
    }

    /**
     * Geocode an address using Nominatim
     */
    async function geocode(query, requestId) {
        try {
            const url = `${CONFIG.geocodeUrl}?format=json&q=${encodeURIComponent(query)}&limit=1`;
            const response = await fetch(url, {
                headers: { 'User-Agent': 'SDMapViewerCom/1.0' }
            });

            if (!response.ok) {
                throw new Error(`HTTP error: ${response.status}`);
            }

            const data = await response.json();

            if (data && data.length > 0) {
                const result = data[0];
                return {
                    lat: parseFloat(result.lat),
                    lng: parseFloat(result.lon),
                    displayName: result.display_name
                };
            } else {
                throw new Error('Address not found');
            }
        } catch (error) {
            console.error('Geocoding error:', error);
            sendToCSharp('geocodingfailed', { requestId: requestId, error: error.message });
            return null;
        }
    }

    /**
     * Initialize the map
     */
    function initializeMap() {
        try {
            if (CONFIG.debug) console.log('Initializing ' + CONFIG.controlName);

            map = L.map('map', {
                center: [CONFIG.defaultLat, CONFIG.defaultLng],
                zoom: CONFIG.defaultZoom,
                zoomControl: true
            });

            tileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
                maxZoom: 19
            }).addTo(map);

            // Wire up map events
            map.on('moveend', function() {
                var center = map.getCenter();
                sendToCSharp('moveend', { lat: center.lat, lng: center.lng });
            });

            map.on('zoomend', function() {
                sendToCSharp('zoomend', { zoom: map.getZoom() });
            });

            map.on('click', function(e) {
                // Don't send mapclick when in measurement mode
                if (!isMeasuring) {
                    sendToCSharp('mapclick', { lat: e.latlng.lat, lng: e.latlng.lng });
                }
            });

            map.on('dblclick', function(e) {
                sendToCSharp('mapdblclick', { lat: e.latlng.lat, lng: e.latlng.lng });
            });

            sendToCSharp('ready', { controlName: CONFIG.controlName });
        } catch (error) {
            console.error('Error initializing map:', error);
            sendToCSharp('error', { error: error.message });
        }
    }

    // ============================================
    // Map Control Functions
    // ============================================

    window.setCenter = function(lat, lng) {
        if (map) map.setView([lat, lng], map.getZoom());
    };

    window.setCenterByAddress = async function(query) {
        const result = await geocode(query, 'center');
        if (result) {
            map.setView([result.lat, result.lng], map.getZoom());
            sendToCSharp('geocodingcomplete', {
                requestId: 'center',
                lat: result.lat,
                lng: result.lng,
                displayName: result.displayName
            });
        }
    };

    window.setZoom = function(zoom) {
        if (map) map.setZoom(zoom);
    };

    window.setView = function(lat, lng, zoom) {
        if (map) map.setView([lat, lng], zoom);
    };

    window.setViewByAddress = async function(query, zoom) {
        const result = await geocode(query, 'center');
        if (result) {
            map.setView([result.lat, result.lng], zoom);
            sendToCSharp('geocodingcomplete', {
                requestId: 'center',
                lat: result.lat,
                lng: result.lng,
                displayName: result.displayName
            });
        }
    };

    window.panBy = function(deltaX, deltaY) {
        if (map) map.panBy([deltaX, deltaY]);
    };

    window.zoomIn = function() {
        if (map) map.zoomIn();
    };

    window.zoomOut = function() {
        if (map) map.zoomOut();
    };

    window.fitBounds = function() {
        if (map && Object.keys(markers).length > 0) {
            var group = L.featureGroup(Object.values(markers).map(m => m.marker));
            map.fitBounds(group.getBounds().pad(0.1));
        }
    };

    // ============================================
    // Marker Functions
    // ============================================

    window.addMarker = function(id, lat, lng, title, description, color) {
        if (!map) return;

        // Remove existing marker with same id
        if (markers[id]) {
            map.removeLayer(markers[id].marker);
        }

        const icon = getMarkerIcon(color || 'blue');
        const marker = L.marker([lat, lng], {
            title: title,
            icon: icon
        }).addTo(map);

        if (description) {
            marker.bindPopup(description);
        }

        marker.on('click', function() {
            sendToCSharp('markerclick', { markerId: id, lat: lat, lng: lng });
        });

        markers[id] = {
            marker: marker,
            lat: lat,
            lng: lng,
            title: title,
            description: description,
            color: color || 'blue'
        };

        sendToCSharp('markeradded', {
            markerId: id,
            lat: lat,
            lng: lng,
            markerCount: Object.keys(markers).length
        });
    };

    window.addMarkerByAddress = async function(id, query, title, description, color) {
        const result = await geocode(query, id);
        if (result) {
            window.addMarker(id, result.lat, result.lng, title, description, color);
            sendToCSharp('geocodingcomplete', {
                requestId: id,
                lat: result.lat,
                lng: result.lng,
                displayName: result.displayName
            });
        }
    };

    window.removeMarker = function(id) {
        if (markers[id]) {
            map.removeLayer(markers[id].marker);
            delete markers[id];
            sendToCSharp('markerremoved', {
                markerId: id,
                markerCount: Object.keys(markers).length
            });
        }
    };

    window.clearMarkers = function() {
        for (var id in markers) {
            map.removeLayer(markers[id].marker);
        }
        markers = {};
        sendToCSharp('markerscleared', { markerCount: 0 });
    };

    window.updateMarkerPosition = function(id, lat, lng) {
        if (markers[id]) {
            markers[id].marker.setLatLng([lat, lng]);
            markers[id].lat = lat;
            markers[id].lng = lng;
        }
    };

    window.updateMarkerColor = function(id, color) {
        if (markers[id]) {
            const icon = getMarkerIcon(color);
            markers[id].marker.setIcon(icon);
            markers[id].color = color;
        }
    };

    window.updateMarkerDescription = function(id, description) {
        if (markers[id]) {
            markers[id].marker.unbindPopup();
            if (description) {
                markers[id].marker.bindPopup(description);
            }
            markers[id].description = description;
        }
    };

    window.showMarkerPopup = function(id) {
        if (markers[id]) {
            markers[id].marker.openPopup();
        }
    };

    window.centerOnMarker = function(id) {
        if (markers[id]) {
            map.setView([markers[id].lat, markers[id].lng], map.getZoom());
        }
    };

    // ============================================
    // Tile Layer Functions
    // ============================================

    window.setTileLayer = function(urlTemplate, attribution) {
        if (map) {
            if (tileLayer) map.removeLayer(tileLayer);
            tileLayer = L.tileLayer(urlTemplate, {
                attribution: attribution,
                maxZoom: 19
            }).addTo(map);
        }
    };

    // ============================================
    // Map Type Functions
    // ============================================

    window.setMapType = function(mapType) {
        if (!map) return;

        const type = (mapType || 'normal').toLowerCase();
        const layerConfig = TILE_LAYERS[type] || TILE_LAYERS.normal;

        if (tileLayer) map.removeLayer(tileLayer);

        tileLayer = L.tileLayer(layerConfig.url, {
            attribution: layerConfig.attribution,
            maxZoom: layerConfig.maxZoom
        }).addTo(map);

        currentMapType = type;
        sendToCSharp('maptypechanged', { mapType: type });
    };

    // ============================================
    // Scale Control Functions
    // ============================================

    window.showScale = function(show) {
        if (!map) return;

        if (show) {
            if (!scaleControl) {
                scaleControl = L.control.scale({
                    position: scalePosition,
                    metric: true,
                    imperial: true,
                    maxWidth: 200
                }).addTo(map);
            }
        } else {
            if (scaleControl) {
                map.removeControl(scaleControl);
                scaleControl = null;
            }
        }
    };

    window.setScalePosition = function(position) {
        if (!map) return;

        const validPositions = ['topleft', 'topright', 'bottomleft', 'bottomright'];
        const pos = validPositions.includes(position) ? position : 'bottomleft';
        scalePosition = pos;

        // If scale is visible, recreate it at new position
        if (scaleControl) {
            map.removeControl(scaleControl);
            scaleControl = L.control.scale({
                position: pos,
                metric: true,
                imperial: true,
                maxWidth: 200
            }).addTo(map);
        }
    };

    // ============================================
    // Measurement Functions
    // ============================================

    /**
     * Calculate distance between two points in meters using Haversine formula
     */
    function haversineDistance(lat1, lng1, lat2, lng2) {
        const R = 6371000; // Earth's radius in meters
        const dLat = (lat2 - lat1) * Math.PI / 180;
        const dLng = (lng2 - lng1) * Math.PI / 180;
        const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                  Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
                  Math.sin(dLng / 2) * Math.sin(dLng / 2);
        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        return R * c;
    }

    /**
     * Calculate total distance of all measurement points
     */
    function calculateTotalDistance() {
        let totalMeters = 0;
        for (let i = 1; i < measurePoints.length; i++) {
            totalMeters += haversineDistance(
                measurePoints[i - 1].lat, measurePoints[i - 1].lng,
                measurePoints[i].lat, measurePoints[i].lng
            );
        }
        return {
            meters: totalMeters,
            km: totalMeters / 1000,
            miles: totalMeters / 1609.344
        };
    }

    /**
     * Format distance for display
     */
    function formatDistance(meters) {
        if (meters < 1000) {
            return meters.toFixed(0) + ' m';
        } else {
            return (meters / 1000).toFixed(2) + ' km';
        }
    }

    /**
     * Update the measurement line and labels
     */
    function updateMeasurementDisplay() {
        if (!map) return;

        // Remove existing line
        if (measureLine) {
            map.removeLayer(measureLine);
            measureLine = null;
        }

        // Remove existing labels
        measureLabels.forEach(label => map.removeLayer(label));
        measureLabels = [];

        if (measurePoints.length < 2) return;

        // Create polyline
        const latlngs = measurePoints.map(p => [p.lat, p.lng]);
        measureLine = L.polyline(latlngs, {
            color: '#FF4444',
            weight: 3,
            opacity: 0.8,
            dashArray: '10, 10'
        }).addTo(map);

        // Add distance labels for each segment
        for (let i = 1; i < measurePoints.length; i++) {
            const segmentDist = haversineDistance(
                measurePoints[i - 1].lat, measurePoints[i - 1].lng,
                measurePoints[i].lat, measurePoints[i].lng
            );
            const midLat = (measurePoints[i - 1].lat + measurePoints[i].lat) / 2;
            const midLng = (measurePoints[i - 1].lng + measurePoints[i].lng) / 2;

            const label = L.marker([midLat, midLng], {
                icon: L.divIcon({
                    className: 'measure-label',
                    html: `<div style="background: white; padding: 2px 6px; border-radius: 3px; border: 1px solid #666; font-size: 12px; white-space: nowrap;">${formatDistance(segmentDist)}</div>`,
                    iconSize: null,
                    iconAnchor: [30, 10]
                })
            }).addTo(map);
            measureLabels.push(label);
        }

        // Send measurement update
        const dist = calculateTotalDistance();
        sendToCSharp('measurementcomplete', {
            distanceMeters: dist.meters,
            distanceKm: dist.km,
            distanceMiles: dist.miles,
            pointCount: measurePoints.length
        });

        // Update UI display
        if (window.updateMeasureDisplay) {
            window.updateMeasureDisplay(dist.meters);
        }
    }

    /**
     * Add a measurement point
     */
    function addMeasurePoint(lat, lng) {
        measurePoints.push({ lat: lat, lng: lng });

        // Add marker for the point
        const pointNumber = measurePoints.length;
        const marker = L.marker([lat, lng], {
            icon: L.divIcon({
                className: 'measure-point',
                html: `<div style="background: #FF4444; color: white; width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 12px; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);">${pointNumber}</div>`,
                iconSize: [24, 24],
                iconAnchor: [12, 12]
            })
        }).addTo(map);
        measureMarkers.push(marker);

        updateMeasurementDisplay();
    }

    /**
     * Measurement click handler
     */
    function onMeasureClick(e) {
        if (!isMeasuring) return;
        addMeasurePoint(e.latlng.lat, e.latlng.lng);
    }

    window.enableMeasure = function(enable) {
        if (!map) return;

        isMeasuring = enable;

        if (enable) {
            map.getContainer().style.cursor = 'crosshair';
            map.on('click', onMeasureClick);
            sendToCSharp('measurementstarted', {});
        } else {
            map.getContainer().style.cursor = '';
            map.off('click', onMeasureClick);
            sendToCSharp('measurementstopped', {});
        }
    };

    window.clearMeasurements = function() {
        if (!map) return;

        // Remove markers
        measureMarkers.forEach(marker => map.removeLayer(marker));
        measureMarkers = [];

        // Remove line
        if (measureLine) {
            map.removeLayer(measureLine);
            measureLine = null;
        }

        // Remove labels
        measureLabels.forEach(label => map.removeLayer(label));
        measureLabels = [];

        // Clear points
        measurePoints = [];
    };

    // ============================================
    // Utility Functions
    // ============================================

    window.invalidateSize = function() {
        if (map) map.invalidateSize();
    };

    // ============================================
    // UI Control Handlers
    // ============================================

    function initializeUIControls() {
        // Map type buttons
        const mapTypeButtons = document.querySelectorAll('[data-maptype]');
        mapTypeButtons.forEach(btn => {
            btn.addEventListener('click', function() {
                const mapType = this.getAttribute('data-maptype');
                window.setMapType(mapType);

                // Update active state
                mapTypeButtons.forEach(b => b.classList.remove('active'));
                this.classList.add('active');
            });
        });

        // Measure button
        const measureBtn = document.getElementById('measureBtn');
        const measureDisplay = document.getElementById('measureDisplay');
        const measureTotal = document.getElementById('measureTotal');
        const clearMeasureBtn = document.getElementById('clearMeasure');

        if (measureBtn) {
            measureBtn.addEventListener('click', function() {
                const isCurrentlyMeasuring = this.classList.contains('measuring');

                if (isCurrentlyMeasuring) {
                    // Stop measuring
                    window.enableMeasure(false);
                    this.classList.remove('measuring');
                    this.querySelector('.measure-text').textContent = 'Medir';
                } else {
                    // Start measuring
                    window.enableMeasure(true);
                    this.classList.add('measuring');
                    this.querySelector('.measure-text').textContent = 'Detener';
                    measureDisplay.classList.add('visible');
                }
            });
        }

        if (clearMeasureBtn) {
            clearMeasureBtn.addEventListener('click', function() {
                window.clearMeasurements();
                measureTotal.textContent = '0 m';
            });
        }

        // Update measure display when measurement changes
        window.updateMeasureDisplay = function(meters) {
            if (measureTotal) {
                if (meters < 1000) {
                    measureTotal.textContent = meters.toFixed(0) + ' m';
                } else {
                    measureTotal.textContent = (meters / 1000).toFixed(2) + ' km';
                }
                measureDisplay.classList.add('visible');
            }
        };

        // Show scale by default
        window.showScale(true);
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeMap();
        initializeUIControls();
    });

})();
