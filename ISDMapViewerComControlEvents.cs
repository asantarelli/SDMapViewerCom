using System;
using System.Runtime.InteropServices;

namespace SDMapViewerCom
{
    /// <summary>
    /// COM event interface for the SDMapViewerCom Control.
    /// This defines all events that can be fired to Clarion.
    /// </summary>
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [Guid("1E6E5925-1AF0-4D7E-B63B-937D7103B03B")]
    public interface ISDMapViewerComControlEvents
    {
        /// <summary>
        /// Fired when WebView2 and map are fully initialized and ready for use.
        /// </summary>
        [DispId(1)]
        void ControlReady();

        /// <summary>
        /// Fired when an error occurs.
        /// </summary>
        /// <param name="errorMessage">Description of the error</param>
        [DispId(2)]
        void ErrorOccurred(string errorMessage);

        /// <summary>
        /// Fired when the map center changes (pan or setCenter).
        /// </summary>
        /// <param name="latitude">New center latitude</param>
        /// <param name="longitude">New center longitude</param>
        [DispId(3)]
        void MapMoved(double latitude, double longitude);

        /// <summary>
        /// Fired when the zoom level changes.
        /// </summary>
        /// <param name="zoom">New zoom level</param>
        [DispId(4)]
        void ZoomChanged(int zoom);

        /// <summary>
        /// Fired when a marker is clicked.
        /// </summary>
        /// <param name="markerId">ID of the clicked marker</param>
        /// <param name="latitude">Marker latitude</param>
        /// <param name="longitude">Marker longitude</param>
        [DispId(5)]
        void MarkerClicked(string markerId, double latitude, double longitude);

        /// <summary>
        /// Fired when the map is clicked (not on a marker).
        /// </summary>
        /// <param name="latitude">Click latitude</param>
        /// <param name="longitude">Click longitude</param>
        [DispId(6)]
        void MapClicked(double latitude, double longitude);

        /// <summary>
        /// Fired when the map is double-clicked.
        /// </summary>
        /// <param name="latitude">Click latitude</param>
        /// <param name="longitude">Click longitude</param>
        [DispId(7)]
        void MapDoubleClicked(double latitude, double longitude);

        /// <summary>
        /// Fired when geocoding completes successfully.
        /// </summary>
        /// <param name="requestId">ID of the geocoding request (marker ID or "center")</param>
        /// <param name="latitude">Resolved latitude</param>
        /// <param name="longitude">Resolved longitude</param>
        /// <param name="displayName">Full display name returned by geocoder</param>
        [DispId(8)]
        void GeocodingComplete(string requestId, double latitude, double longitude, string displayName);

        /// <summary>
        /// Fired when geocoding fails.
        /// </summary>
        /// <param name="requestId">ID of the geocoding request</param>
        /// <param name="errorMessage">Error description</param>
        [DispId(9)]
        void GeocodingFailed(string requestId, string errorMessage);

        /// <summary>
        /// Fired when a marker is added to the map.
        /// </summary>
        /// <param name="markerId">ID of the added marker</param>
        /// <param name="latitude">Marker latitude</param>
        /// <param name="longitude">Marker longitude</param>
        [DispId(10)]
        void MarkerAdded(string markerId, double latitude, double longitude);

        /// <summary>
        /// Fired when a distance measurement is completed.
        /// </summary>
        /// <param name="distanceMeters">Total distance in meters</param>
        /// <param name="distanceKm">Total distance in kilometers</param>
        /// <param name="distanceMiles">Total distance in miles</param>
        /// <param name="pointCount">Number of measurement points</param>
        [DispId(11)]
        void MeasurementComplete(double distanceMeters, double distanceKm, double distanceMiles, int pointCount);

        /// <summary>
        /// Fired when the map type changes.
        /// </summary>
        /// <param name="mapType">New map type (normal, satellite, terrain)</param>
        [DispId(12)]
        void MapTypeChanged(string mapType);
    }
}
