using System;
using System.Runtime.InteropServices;

namespace SDMapViewerCom
{
    /// <summary>
    /// Main COM interface for the SDMapViewerCom Control.
    /// Interactive map viewer with Leaflet.js - zoom, pan, and marker support.
    /// Supports geocoding for address-based initialization and marker placement.
    /// </summary>
    [ComVisible(true)]
    [Guid("B4036605-098C-49A7-B689-2863E41016B9")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ISDMapViewerComControl
    {
        #region Properties

        /// <summary>
        /// Gets whether WebView2 is fully initialized and ready.
        /// </summary>
        [DispId(1)]
        bool GetIsReady();

        /// <summary>
        /// Gets the last error message.
        /// </summary>
        [DispId(2)]
        string GetLastError();

        /// <summary>
        /// Gets the current map center latitude.
        /// </summary>
        [DispId(3)]
        double GetLatitude();

        /// <summary>
        /// Gets the current map center longitude.
        /// </summary>
        [DispId(4)]
        double GetLongitude();

        /// <summary>
        /// Gets the current zoom level.
        /// </summary>
        [DispId(5)]
        int GetZoom();

        /// <summary>
        /// Gets the current map type (normal, satellite, terrain).
        /// </summary>
        [DispId(6)]
        string GetMapType();

        /// <summary>
        /// Gets whether the scale control is visible.
        /// </summary>
        [DispId(7)]
        bool GetScaleVisible();

        /// <summary>
        /// Gets whether measurement mode is active.
        /// </summary>
        [DispId(8)]
        bool GetIsMeasuring();

        #endregion

        #region Map Initialization Methods

        /// <summary>
        /// Sets the map center by coordinates.
        /// </summary>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        [DispId(10)]
        void SetCenter(double latitude, double longitude);

        /// <summary>
        /// Sets the map center by full address (uses geocoding).
        /// </summary>
        /// <param name="address">Street address</param>
        /// <param name="city">City/Locality name</param>
        /// <param name="province">Province/State name</param>
        /// <param name="country">Country name</param>
        [DispId(11)]
        void SetCenterByAddress(string address, string city, string province, string country);

        /// <summary>
        /// Sets the map center by city/locality (uses geocoding).
        /// </summary>
        /// <param name="city">City/Locality name</param>
        /// <param name="province">Province/State name</param>
        /// <param name="country">Country name</param>
        [DispId(12)]
        void SetCenterByCity(string city, string province, string country);

        /// <summary>
        /// Sets the map zoom level.
        /// </summary>
        /// <param name="zoom">Zoom level (1-18)</param>
        [DispId(13)]
        void SetZoom(int zoom);

        /// <summary>
        /// Sets the map center and zoom by coordinates.
        /// </summary>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="zoom">Zoom level (1-18)</param>
        [DispId(14)]
        void SetView(double latitude, double longitude, int zoom);

        /// <summary>
        /// Sets the map center and zoom by full address.
        /// </summary>
        /// <param name="address">Street address</param>
        /// <param name="city">City/Locality name</param>
        /// <param name="province">Province/State name</param>
        /// <param name="country">Country name</param>
        /// <param name="zoom">Zoom level (1-18)</param>
        [DispId(15)]
        void SetViewByAddress(string address, string city, string province, string country, int zoom);

        /// <summary>
        /// Sets the map center and zoom by city/locality.
        /// </summary>
        /// <param name="city">City/Locality name</param>
        /// <param name="province">Province/State name</param>
        /// <param name="country">Country name</param>
        /// <param name="zoom">Zoom level (1-18)</param>
        [DispId(16)]
        void SetViewByCity(string city, string province, string country, int zoom);

        #endregion

        #region Map Navigation Methods

        /// <summary>
        /// Pans the map by the specified offset in pixels.
        /// </summary>
        /// <param name="deltaX">Horizontal offset in pixels</param>
        /// <param name="deltaY">Vertical offset in pixels</param>
        [DispId(20)]
        void PanBy(int deltaX, int deltaY);

        /// <summary>
        /// Zooms in one level.
        /// </summary>
        [DispId(21)]
        void ZoomIn();

        /// <summary>
        /// Zooms out one level.
        /// </summary>
        [DispId(22)]
        void ZoomOut();

        /// <summary>
        /// Fits the map to show all markers.
        /// </summary>
        [DispId(23)]
        void FitBounds();

        #endregion

        #region Marker Methods - By Coordinates

        /// <summary>
        /// Adds a marker at the specified coordinates.
        /// </summary>
        /// <param name="id">Unique marker ID</param>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="title">Marker tooltip text</param>
        /// <param name="description">Popup content (can include HTML)</param>
        /// <param name="color">Marker color (red, blue, green, orange, yellow, violet, grey, black)</param>
        /// <returns>True if marker was added successfully</returns>
        [DispId(30)]
        bool AddMarker(string id, double latitude, double longitude, string title, string description, string color);

        /// <summary>
        /// Adds a simple marker at the specified coordinates (default blue color).
        /// </summary>
        /// <param name="id">Unique marker ID</param>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="title">Marker tooltip text</param>
        /// <returns>True if marker was added successfully</returns>
        [DispId(31)]
        bool AddMarkerSimple(string id, double latitude, double longitude, string title);

        #endregion

        #region Marker Methods - By Address

        /// <summary>
        /// Adds a marker at the specified full address (uses geocoding).
        /// </summary>
        /// <param name="id">Unique marker ID</param>
        /// <param name="address">Street address</param>
        /// <param name="city">City/Locality name</param>
        /// <param name="province">Province/State name</param>
        /// <param name="country">Country name</param>
        /// <param name="title">Marker tooltip text</param>
        /// <param name="description">Popup content (can include HTML)</param>
        /// <param name="color">Marker color (red, blue, green, orange, yellow, violet, grey, black)</param>
        /// <returns>True if geocoding request was sent</returns>
        [DispId(35)]
        bool AddMarkerByAddress(string id, string address, string city, string province, string country, string title, string description, string color);

        /// <summary>
        /// Adds a marker at the specified city/locality (uses geocoding).
        /// </summary>
        /// <param name="id">Unique marker ID</param>
        /// <param name="city">City/Locality name</param>
        /// <param name="province">Province/State name</param>
        /// <param name="country">Country name</param>
        /// <param name="title">Marker tooltip text</param>
        /// <param name="description">Popup content (can include HTML)</param>
        /// <param name="color">Marker color (red, blue, green, orange, yellow, violet, grey, black)</param>
        /// <returns>True if geocoding request was sent</returns>
        [DispId(36)]
        bool AddMarkerByCity(string id, string city, string province, string country, string title, string description, string color);

        #endregion

        #region Marker Management Methods

        /// <summary>
        /// Removes a marker from the map.
        /// </summary>
        /// <param name="id">Marker ID to remove</param>
        /// <returns>True if marker was removed</returns>
        [DispId(40)]
        bool RemoveMarker(string id);

        /// <summary>
        /// Removes all markers from the map.
        /// </summary>
        [DispId(41)]
        void ClearMarkers();

        /// <summary>
        /// Updates a marker's position by coordinates.
        /// </summary>
        /// <param name="id">Marker ID</param>
        /// <param name="latitude">New latitude</param>
        /// <param name="longitude">New longitude</param>
        /// <returns>True if marker was updated</returns>
        [DispId(42)]
        bool UpdateMarkerPosition(string id, double latitude, double longitude);

        /// <summary>
        /// Updates a marker's color.
        /// </summary>
        /// <param name="id">Marker ID</param>
        /// <param name="color">New color (red, blue, green, orange, yellow, violet, grey, black)</param>
        /// <returns>True if marker was updated</returns>
        [DispId(43)]
        bool UpdateMarkerColor(string id, string color);

        /// <summary>
        /// Updates a marker's description/popup content.
        /// </summary>
        /// <param name="id">Marker ID</param>
        /// <param name="description">New popup content</param>
        /// <returns>True if marker was updated</returns>
        [DispId(44)]
        bool UpdateMarkerDescription(string id, string description);

        /// <summary>
        /// Shows the popup for a marker.
        /// </summary>
        /// <param name="id">Marker ID</param>
        [DispId(45)]
        void ShowMarkerPopup(string id);

        /// <summary>
        /// Centers the map on a specific marker.
        /// </summary>
        /// <param name="id">Marker ID</param>
        [DispId(46)]
        void CenterOnMarker(string id);

        /// <summary>
        /// Gets the number of markers on the map.
        /// </summary>
        /// <returns>Number of markers</returns>
        [DispId(47)]
        int GetMarkerCount();

        #endregion

        #region Tile Layer Methods

        /// <summary>
        /// Sets the tile layer URL template.
        /// </summary>
        /// <param name="urlTemplate">URL template with {x}, {y}, {z} placeholders</param>
        /// <param name="attribution">Attribution text for the tiles</param>
        [DispId(50)]
        void SetTileLayer(string urlTemplate, string attribution);

        /// <summary>
        /// Sets to use OpenStreetMap tiles (default).
        /// </summary>
        [DispId(51)]
        void UseOpenStreetMap();

        #endregion

        #region Utility Methods

        /// <summary>
        /// Refreshes the map display.
        /// </summary>
        [DispId(60)]
        void RefreshMap();

        /// <summary>
        /// Invalidates the map size (call after container resize).
        /// </summary>
        [DispId(61)]
        void InvalidateSize();

        /// <summary>
        /// Displays control name and version information.
        /// </summary>
        [DispId(62)]
        void About();

        /// <summary>
        /// Executes custom JavaScript on the map.
        /// </summary>
        /// <param name="script">JavaScript code to execute</param>
        [DispId(63)]
        void ExecuteScript(string script);

        #endregion

        #region Map Type Methods

        /// <summary>
        /// Sets the map type.
        /// </summary>
        /// <param name="mapType">Map type: "normal", "satellite", or "terrain"</param>
        [DispId(70)]
        void SetMapType(string mapType);

        #endregion

        #region Scale Control Methods

        /// <summary>
        /// Shows or hides the scale control.
        /// </summary>
        /// <param name="show">True to show, false to hide</param>
        [DispId(75)]
        void ShowScale(bool show);

        /// <summary>
        /// Sets the position of the scale control.
        /// </summary>
        /// <param name="position">Position: "topleft", "topright", "bottomleft", "bottomright"</param>
        [DispId(76)]
        void SetScalePosition(string position);

        #endregion

        #region Measurement Methods

        /// <summary>
        /// Enables or disables measurement mode.
        /// When enabled, clicking on the map adds measurement points.
        /// </summary>
        /// <param name="enable">True to enable, false to disable</param>
        [DispId(80)]
        void EnableMeasure(bool enable);

        /// <summary>
        /// Clears all measurement points and lines from the map.
        /// </summary>
        [DispId(81)]
        void ClearMeasurements();

        #endregion
    }
}
