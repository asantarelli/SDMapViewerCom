using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;

namespace SDMapViewerCom
{
    /// <summary>
    /// Host object for JavaScript to C# communication.
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class SDMapViewerComHostObject
    {
        private readonly SDMapViewerComControl _control;

        public SDMapViewerComHostObject(SDMapViewerComControl control)
        {
            _control = control;
        }

        public void SendMessage(string message)
        {
            try
            {
                _control.HandleHostObjectMessage(message);
            }
            catch (Exception ex)
            {
                _control.SetError($"SendMessage error: {ex.Message}");
            }
        }

        public string GetData()
        {
            try
            {
                return _control.GetControlData();
            }
            catch (Exception ex)
            {
                _control.SetError($"GetData error: {ex.Message}");
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// SDMapViewerCom - Interactive map viewer WebView2 COM Control for Clarion.
    /// Uses Leaflet.js with geocoding support for address-based operations.
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("A0C856EF-B15E-4707-BE41-D180257FA383")]
    [ComSourceInterfaces(typeof(ISDMapViewerComControlEvents))]
    [ProgId("SDMapViewerCom.SDMapViewerComControl")]
    public partial class SDMapViewerComControl : UserControl, ISDMapViewerComControl
    {
        #region Fields

        private WebView2 _webView;
        private SDMapViewerComHostObject _hostObject;
        private bool _isReady;
        private string _lastError;
        private bool _isInitialized;
        private double _latitude = -34.6037;
        private double _longitude = -58.3816;
        private int _zoom = 13;
        private int _markerCount = 0;
        private string _mapType = "normal";
        private bool _scaleVisible = false;
        private bool _isMeasuring = false;

        #endregion

        #region COM Event Delegates

        public delegate void ControlReadyDelegate();
        public delegate void ErrorOccurredDelegate(string errorMessage);
        public delegate void MapMovedDelegate(double latitude, double longitude);
        public delegate void ZoomChangedDelegate(int zoom);
        public delegate void MarkerClickedDelegate(string markerId, double latitude, double longitude);
        public delegate void MapClickedDelegate(double latitude, double longitude);
        public delegate void MapDoubleClickedDelegate(double latitude, double longitude);
        public delegate void GeocodingCompleteDelegate(string requestId, double latitude, double longitude, string displayName);
        public delegate void GeocodingFailedDelegate(string requestId, string errorMessage);
        public delegate void MarkerAddedDelegate(string markerId, double latitude, double longitude);
        public delegate void MeasurementCompleteDelegate(double distanceMeters, double distanceKm, double distanceMiles, int pointCount);
        public delegate void MapTypeChangedDelegate(string mapType);

        #endregion

        #region COM Events

        public event ControlReadyDelegate ControlReady;
        public event ErrorOccurredDelegate ErrorOccurred;
        public event MapMovedDelegate MapMoved;
        public event ZoomChangedDelegate ZoomChanged;
        public event MarkerClickedDelegate MarkerClicked;
        public event MapClickedDelegate MapClicked;
        public event MapDoubleClickedDelegate MapDoubleClicked;
        public event GeocodingCompleteDelegate GeocodingComplete;
        public event GeocodingFailedDelegate GeocodingFailed;
        public event MarkerAddedDelegate MarkerAdded;
        public event MeasurementCompleteDelegate MeasurementComplete;
        public event MapTypeChangedDelegate MapTypeChanged;

        #endregion

        #region Constructor

        public SDMapViewerComControl()
        {
            _isReady = false;
            _lastError = string.Empty;
            _isInitialized = false;
            Size = new Size(800, 600);
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode && !_isInitialized)
            {
                BackColor = Color.White;
                InitializeWebView2Async();
            }
        }

        #endregion

        #region WebView2 Initialization

        private async void InitializeWebView2Async()
        {
            try
            {
                if (_isInitialized) return;
                _isInitialized = true;

                _webView = new WebView2 { Dock = DockStyle.Fill };
                var env = await CoreWebView2Environment.CreateAsync(null, null, null);
                await _webView.EnsureCoreWebView2Async(env);

                _webView.CoreWebView2.Settings.IsScriptEnabled = true;
                _webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                _webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;

                _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "localapp.clarioncontrols",
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot"),
                    CoreWebView2HostResourceAccessKind.Allow);

                _hostObject = new SDMapViewerComHostObject(this);
                _webView.CoreWebView2.AddHostObjectToScript("SDMapViewerComHost", _hostObject);
                _webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                Controls.Add(_webView);
                _webView.CoreWebView2.Navigate("https://localapp.clarioncontrols/controls/sdmapviewercom/index.html");

                _isReady = true;
                RaiseControlReady();
            }
            catch (Exception ex)
            {
                _lastError = $"WebView2 initialization failed: {ex.Message}";
                RaiseErrorOccurred(_lastError);
            }
        }

        #endregion

        #region WebView2 Event Handlers

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                HandleWebMessage(message);
            }
            catch (Exception ex)
            {
                SetError($"WebMessage error: {ex.Message}");
            }
        }

        #endregion

        #region Message Handling

        private class WebMessage
        {
            public string type { get; set; }
            public string markerId { get; set; }
            public string requestId { get; set; }
            public double lat { get; set; }
            public double lng { get; set; }
            public int zoom { get; set; }
            public string error { get; set; }
            public string displayName { get; set; }
            public int markerCount { get; set; }
            public string mapType { get; set; }
            public double distanceMeters { get; set; }
            public double distanceKm { get; set; }
            public double distanceMiles { get; set; }
            public int pointCount { get; set; }
        }

        internal void HandleWebMessage(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;
                var msg = JsonConvert.DeserializeObject<WebMessage>(message);
                if (msg == null) return;

                switch (msg.type ?? string.Empty)
                {
                    case "ready":
                        break;
                    case "moveend":
                        _latitude = msg.lat;
                        _longitude = msg.lng;
                        RaiseMapMoved(msg.lat, msg.lng);
                        break;
                    case "zoomend":
                        _zoom = msg.zoom;
                        RaiseZoomChanged(msg.zoom);
                        break;
                    case "markerclick":
                        RaiseMarkerClicked(msg.markerId, msg.lat, msg.lng);
                        break;
                    case "mapclick":
                        RaiseMapClicked(msg.lat, msg.lng);
                        break;
                    case "mapdblclick":
                        RaiseMapDoubleClicked(msg.lat, msg.lng);
                        break;
                    case "geocodingcomplete":
                        RaiseGeocodingComplete(msg.requestId, msg.lat, msg.lng, msg.displayName);
                        break;
                    case "geocodingfailed":
                        RaiseGeocodingFailed(msg.requestId, msg.error);
                        break;
                    case "markeradded":
                        _markerCount = msg.markerCount;
                        RaiseMarkerAdded(msg.markerId, msg.lat, msg.lng);
                        break;
                    case "markerremoved":
                        _markerCount = msg.markerCount;
                        break;
                    case "markerscleared":
                        _markerCount = 0;
                        break;
                    case "maptypechanged":
                        _mapType = msg.mapType ?? "normal";
                        RaiseMapTypeChanged(_mapType);
                        break;
                    case "measurementcomplete":
                        RaiseMeasurementComplete(msg.distanceMeters, msg.distanceKm, msg.distanceMiles, msg.pointCount);
                        break;
                    case "measurementstarted":
                        _isMeasuring = true;
                        break;
                    case "measurementstopped":
                        _isMeasuring = false;
                        break;
                    case "error":
                        SetError(msg.error ?? "Unknown error");
                        break;
                }
            }
            catch (Exception ex)
            {
                SetError($"Message parse error: {ex.Message}");
            }
        }

        internal void HandleHostObjectMessage(string message) => HandleWebMessage(message);

        internal string GetControlData()
        {
            return JsonConvert.SerializeObject(new
            {
                isReady = _isReady,
                latitude = _latitude,
                longitude = _longitude,
                zoom = _zoom,
                markerCount = _markerCount
            });
        }

        #endregion

        #region ISDMapViewerComControl Properties

        [ComVisible(true)]
        public bool GetIsReady() => _isReady;

        [ComVisible(true)]
        public string GetLastError() => _lastError ?? string.Empty;

        [ComVisible(true)]
        public double GetLatitude() => _latitude;

        [ComVisible(true)]
        public double GetLongitude() => _longitude;

        [ComVisible(true)]
        public int GetZoom() => _zoom;

        [ComVisible(true)]
        public string GetMapType() => _mapType;

        [ComVisible(true)]
        public bool GetScaleVisible() => _scaleVisible;

        [ComVisible(true)]
        public bool GetIsMeasuring() => _isMeasuring;

        #endregion

        #region Map Initialization Methods

        [ComVisible(true)]
        public void SetCenter(double latitude, double longitude)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript($"setCenter({Fmt(latitude)}, {Fmt(longitude)})");
            }
            catch (Exception ex) { SetError($"SetCenter error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void SetCenterByAddress(string address, string city, string province, string country)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                var query = BuildAddressQuery(address, city, province, country);
                ExecuteMapScript($"setCenterByAddress({JsonConvert.SerializeObject(query)})");
            }
            catch (Exception ex) { SetError($"SetCenterByAddress error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void SetCenterByCity(string city, string province, string country)
        {
            SetCenterByAddress("", city, province, country);
        }

        [ComVisible(true)]
        public void SetZoom(int zoom)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript($"setZoom({zoom})");
            }
            catch (Exception ex) { SetError($"SetZoom error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void SetView(double latitude, double longitude, int zoom)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript($"setView({Fmt(latitude)}, {Fmt(longitude)}, {zoom})");
            }
            catch (Exception ex) { SetError($"SetView error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void SetViewByAddress(string address, string city, string province, string country, int zoom)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                var query = BuildAddressQuery(address, city, province, country);
                ExecuteMapScript($"setViewByAddress({JsonConvert.SerializeObject(query)}, {zoom})");
            }
            catch (Exception ex) { SetError($"SetViewByAddress error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void SetViewByCity(string city, string province, string country, int zoom)
        {
            SetViewByAddress("", city, province, country, zoom);
        }

        #endregion

        #region Map Navigation Methods

        [ComVisible(true)]
        public void PanBy(int deltaX, int deltaY)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript($"panBy({deltaX}, {deltaY})");
            }
            catch (Exception ex) { SetError($"PanBy error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void ZoomIn()
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript("zoomIn()");
            }
            catch (Exception ex) { SetError($"ZoomIn error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void ZoomOut()
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript("zoomOut()");
            }
            catch (Exception ex) { SetError($"ZoomOut error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void FitBounds()
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript("fitBounds()");
            }
            catch (Exception ex) { SetError($"FitBounds error: {ex.Message}"); }
        }

        #endregion

        #region Marker Methods - By Coordinates

        [ComVisible(true)]
        public bool AddMarker(string id, double latitude, double longitude, string title, string description, string color)
        {
            try
            {
                if (!_isReady || _webView == null) return false;
                ExecuteMapScript($"addMarker({Js(id)}, {Fmt(latitude)}, {Fmt(longitude)}, {Js(title)}, {Js(description)}, {Js(color)})");
                return true;
            }
            catch (Exception ex) { SetError($"AddMarker error: {ex.Message}"); return false; }
        }

        [ComVisible(true)]
        public bool AddMarkerSimple(string id, double latitude, double longitude, string title)
        {
            return AddMarker(id, latitude, longitude, title, "", "blue");
        }

        #endregion

        #region Marker Methods - By Address

        [ComVisible(true)]
        public bool AddMarkerByAddress(string id, string address, string city, string province, string country, string title, string description, string color)
        {
            try
            {
                if (!_isReady || _webView == null) return false;
                var query = BuildAddressQuery(address, city, province, country);
                ExecuteMapScript($"addMarkerByAddress({Js(id)}, {Js(query)}, {Js(title)}, {Js(description)}, {Js(color)})");
                return true;
            }
            catch (Exception ex) { SetError($"AddMarkerByAddress error: {ex.Message}"); return false; }
        }

        [ComVisible(true)]
        public bool AddMarkerByCity(string id, string city, string province, string country, string title, string description, string color)
        {
            return AddMarkerByAddress(id, "", city, province, country, title, description, color);
        }

        #endregion

        #region Marker Management Methods

        [ComVisible(true)]
        public bool RemoveMarker(string id)
        {
            try
            {
                if (!_isReady || _webView == null) return false;
                ExecuteMapScript($"removeMarker({Js(id)})");
                return true;
            }
            catch (Exception ex) { SetError($"RemoveMarker error: {ex.Message}"); return false; }
        }

        [ComVisible(true)]
        public void ClearMarkers()
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript("clearMarkers()");
            }
            catch (Exception ex) { SetError($"ClearMarkers error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public bool UpdateMarkerPosition(string id, double latitude, double longitude)
        {
            try
            {
                if (!_isReady || _webView == null) return false;
                ExecuteMapScript($"updateMarkerPosition({Js(id)}, {Fmt(latitude)}, {Fmt(longitude)})");
                return true;
            }
            catch (Exception ex) { SetError($"UpdateMarkerPosition error: {ex.Message}"); return false; }
        }

        [ComVisible(true)]
        public bool UpdateMarkerColor(string id, string color)
        {
            try
            {
                if (!_isReady || _webView == null) return false;
                ExecuteMapScript($"updateMarkerColor({Js(id)}, {Js(color)})");
                return true;
            }
            catch (Exception ex) { SetError($"UpdateMarkerColor error: {ex.Message}"); return false; }
        }

        [ComVisible(true)]
        public bool UpdateMarkerDescription(string id, string description)
        {
            try
            {
                if (!_isReady || _webView == null) return false;
                ExecuteMapScript($"updateMarkerDescription({Js(id)}, {Js(description)})");
                return true;
            }
            catch (Exception ex) { SetError($"UpdateMarkerDescription error: {ex.Message}"); return false; }
        }

        [ComVisible(true)]
        public void ShowMarkerPopup(string id)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript($"showMarkerPopup({Js(id)})");
            }
            catch (Exception ex) { SetError($"ShowMarkerPopup error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void CenterOnMarker(string id)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript($"centerOnMarker({Js(id)})");
            }
            catch (Exception ex) { SetError($"CenterOnMarker error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public int GetMarkerCount() => _markerCount;

        #endregion

        #region Tile Layer Methods

        [ComVisible(true)]
        public void SetTileLayer(string urlTemplate, string attribution)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript($"setTileLayer({Js(urlTemplate)}, {Js(attribution)})");
            }
            catch (Exception ex) { SetError($"SetTileLayer error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void UseOpenStreetMap()
        {
            SetTileLayer(
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                "&copy; <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors"
            );
        }

        #endregion

        #region Utility Methods

        [ComVisible(true)]
        public void RefreshMap()
        {
            try
            {
                if (!_isReady || _webView == null) return;
                _webView.CoreWebView2.Reload();
            }
            catch (Exception ex) { SetError($"RefreshMap error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void InvalidateSize()
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript("invalidateSize()");
            }
            catch (Exception ex) { SetError($"InvalidateSize error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void About()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var name = assembly.GetName().Name;
                var version = assembly.GetName().Version;
                MessageBox.Show(
                    $"{name}\nVersion: {version.Major}.{version.Minor}.{version.Build}\n\nInteractive map viewer using Leaflet.js\nGeocoding: Nominatim (OpenStreetMap)\nWebView2 Runtime: {CoreWebView2Environment.GetAvailableBrowserVersionString()}",
                    "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        }

        [ComVisible(true)]
        public void ExecuteScript(string script)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                _webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex) { SetError($"ExecuteScript error: {ex.Message}"); }
        }

        #endregion

        #region Map Type Methods

        [ComVisible(true)]
        public void SetMapType(string mapType)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                var validType = (mapType ?? "normal").ToLower();
                if (validType != "normal" && validType != "satellite" && validType != "terrain")
                    validType = "normal";
                ExecuteMapScript($"setMapType({Js(validType)})");
            }
            catch (Exception ex) { SetError($"SetMapType error: {ex.Message}"); }
        }

        #endregion

        #region Scale Control Methods

        [ComVisible(true)]
        public void ShowScale(bool show)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                _scaleVisible = show;
                ExecuteMapScript($"showScale({(show ? "true" : "false")})");
            }
            catch (Exception ex) { SetError($"ShowScale error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void SetScalePosition(string position)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                var validPos = (position ?? "bottomleft").ToLower();
                if (validPos != "topleft" && validPos != "topright" && validPos != "bottomleft" && validPos != "bottomright")
                    validPos = "bottomleft";
                ExecuteMapScript($"setScalePosition({Js(validPos)})");
            }
            catch (Exception ex) { SetError($"SetScalePosition error: {ex.Message}"); }
        }

        #endregion

        #region Measurement Methods

        [ComVisible(true)]
        public void EnableMeasure(bool enable)
        {
            try
            {
                if (!_isReady || _webView == null) return;
                _isMeasuring = enable;
                ExecuteMapScript($"enableMeasure({(enable ? "true" : "false")})");
            }
            catch (Exception ex) { SetError($"EnableMeasure error: {ex.Message}"); }
        }

        [ComVisible(true)]
        public void ClearMeasurements()
        {
            try
            {
                if (!_isReady || _webView == null) return;
                ExecuteMapScript("clearMeasurements()");
            }
            catch (Exception ex) { SetError($"ClearMeasurements error: {ex.Message}"); }
        }

        #endregion

        #region Helper Methods

        private string BuildAddressQuery(string address, string city, string province, string country)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(address)) parts.Add(address.Trim());
            if (!string.IsNullOrWhiteSpace(city)) parts.Add(city.Trim());
            if (!string.IsNullOrWhiteSpace(province)) parts.Add(province.Trim());
            if (!string.IsNullOrWhiteSpace(country)) parts.Add(country.Trim());
            return string.Join(", ", parts);
        }

        private string Fmt(double value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        private string Js(string value) => JsonConvert.SerializeObject(value ?? "");

        private async void ExecuteMapScript(string functionCall)
        {
            try
            {
                if (_webView?.CoreWebView2 != null)
                    await _webView.CoreWebView2.ExecuteScriptAsync(functionCall);
            }
            catch { }
        }

        internal void SetError(string error)
        {
            _lastError = error ?? string.Empty;
            RaiseErrorOccurred(_lastError);
        }

        #endregion

        #region Event Raising Methods

        private void RaiseControlReady() { if (ControlReady != null) try { ControlReady(); } catch { } }
        private void RaiseErrorOccurred(string msg) { if (ErrorOccurred != null) try { ErrorOccurred(msg ?? ""); } catch { } }
        private void RaiseMapMoved(double lat, double lng) { if (MapMoved != null) try { MapMoved(lat, lng); } catch { } }
        private void RaiseZoomChanged(int zoom) { if (ZoomChanged != null) try { ZoomChanged(zoom); } catch { } }
        private void RaiseMarkerClicked(string id, double lat, double lng) { if (MarkerClicked != null) try { MarkerClicked(id ?? "", lat, lng); } catch { } }
        private void RaiseMapClicked(double lat, double lng) { if (MapClicked != null) try { MapClicked(lat, lng); } catch { } }
        private void RaiseMapDoubleClicked(double lat, double lng) { if (MapDoubleClicked != null) try { MapDoubleClicked(lat, lng); } catch { } }
        private void RaiseGeocodingComplete(string id, double lat, double lng, string name) { if (GeocodingComplete != null) try { GeocodingComplete(id ?? "", lat, lng, name ?? ""); } catch { } }
        private void RaiseGeocodingFailed(string id, string error) { if (GeocodingFailed != null) try { GeocodingFailed(id ?? "", error ?? ""); } catch { } }
        private void RaiseMarkerAdded(string id, double lat, double lng) { if (MarkerAdded != null) try { MarkerAdded(id ?? "", lat, lng); } catch { } }
        private void RaiseMeasurementComplete(double meters, double km, double miles, int points) { if (MeasurementComplete != null) try { MeasurementComplete(meters, km, miles, points); } catch { } }
        private void RaiseMapTypeChanged(string mapType) { if (MapTypeChanged != null) try { MapTypeChanged(mapType ?? "normal"); } catch { } }

        #endregion

        #region Cleanup

        protected override void Dispose(bool disposing)
        {
            if (disposing && _webView != null)
            {
                _webView.Dispose();
                _webView = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
