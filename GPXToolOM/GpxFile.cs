using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FSofTUtils;

namespace GPXToolOM {

   /// <summary>
   /// Klasse zur Verwaltung einer GPX-Datei
   /// </summary>
   public class GpxFile {

      public class GpxPoint {
         /// <summary>
         /// ungültiger Zahlenwert
         /// </summary>
         public const double NOTVALID = double.MinValue;       // double.NaN ist leider nicht brauchbar, da nur über die Funktion double.isNaN() ein Vergleich erfolgen kann
         /// <summary>
         /// ungültiger Datumswert
         /// </summary>
         public static DateTime NOTVALID_TIME { get { return DateTime.MinValue; } }

         /// <summary>
         /// Latitude (Breite, y), 0°...360°
         /// </summary>
         public double Lat;
         /// <summary>
         /// Longitude (Länge, x), -180°...180°
         /// </summary>
         public double Lon;
         /// <summary>
         /// Höhe
         /// </summary>
         public double Elevation;
         /// <summary>
         /// Zeitpunkt
         /// </summary>
         public DateTime Time;

         public GpxPoint() {
            Lat = Lon = Elevation = NOTVALID;
            Time = NOTVALID_TIME;
         }

         public GpxPoint(double lon, double lat, double ele = GpxPoint.NOTVALID)
            : this() {
            Lon = lon;
            Lat = lat;
            Elevation = ele;
         }

         public GpxPoint(double lon, double lat, double ele, DateTime time)
            : this() {
            Lon = lon;
            Lat = lat;
            Elevation = ele;
            Time = time;
         }

         public GpxPoint(GpxPoint p) {
            Lat = p.Lat;
            Lon = p.Lon;
            Elevation = p.Elevation;
            Time = p.Time;
         }

         /// <summary>
         /// liefert den XML-Text für den Punkt
         /// </summary>
         /// <returns></returns>
         public string AsXml() {
            string latlon = "<trkpt lat=\"" + Lat.ToString(CultureInfo.InvariantCulture) + "\" lon=\"" + Lon.ToString(CultureInfo.InvariantCulture) + "\"";

            if (Elevation == GpxPoint.NOTVALID &&
                Time == GpxPoint.NOTVALID_TIME)
               return latlon + " />";

            latlon += ">";
            if (Elevation == GpxPoint.NOTVALID)
               return latlon + "<time>" + GpxFile.DateTimeString(Time) + "</time></trkpt>";

            latlon += "<ele>" + Elevation.ToString(CultureInfo.InvariantCulture) + "</ele>";
            if (Time == GpxPoint.NOTVALID_TIME)
               return latlon + "</trkpt>";

            return latlon + "<time>" + GpxFile.DateTimeString(Time) + "</time></trkpt>";
         }

         public override string ToString() {
            return string.Format("Lat={0}, Lon={1}, Elevation={2}, Time={3}", Lat, Lon, Elevation, Time);
         }

      }

      class XPaths {

         #region Objekt-Anzahl

         /// <summary>
         /// Anzahl der Waypoints
         /// </summary>
         /// <returns></returns>
         public static string Count_Waypoint() {
            return "/x:gpx/x:wpt";
         }
         /// <summary>
         /// Anzahl der Routen
         /// </summary>
         /// <returns></returns>
         public static string Count_Route() {
            return "/x:gpx/x:rte";
         }
         /// <summary>
         /// Anzahl der Tracks
         /// </summary>
         /// <returns></returns>
         public static string Count_Track() {
            return "/x:gpx/x:trk";
         }
         /// <summary>
         /// Anzahl der Links im Track
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string Count_TrackLink(int track) {
            return Track(track) + "/x:link";
         }
         /// <summary>
         /// Anzahl der Segmente im Track
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string Count_TrackSegment(int track) {
            return Track(track) + "/x:trkseg";
         }
         /// <summary>
         /// Anzahl der Punkte im Segmente eines Tracks
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="segment">Segmentnummer (0...)</param>
         /// <returns></returns>
         public static string Count_TrackSegmentPoint(int track, int segment) {
            return TrackSegment(track, segment) + "/x:trkpt";
         }

         #endregion

         #region GPX-Ebene

         /// <summary>
         /// Metadaten der Datei
         /// </summary>
         /// <returns></returns>
         public static string Metadata() {
            return "/x:gpx/x:metadata";
         }
         /// <summary>
         /// Waypoint
         /// </summary>
         /// <param name="waypoint">Waypointnummer (0...)</param>
         /// <returns></returns>
         public static string Waypoint(int waypoint) {
            return "/x:gpx/x:wpt[" + (++waypoint).ToString() + "]";
         }
         /// <summary>
         /// Route
         /// </summary>
         /// <param name="route">Routennummer (0...)</param>
         /// <returns></returns>
         public static string Route(int route) {
            return "/x:gpx/x:rte[" + (++route).ToString() + "]";
         }
         /// <summary>
         /// Track
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string Track(int track) {
            return "/x:gpx/x:trk[" + (++track).ToString() + "]";
         }

         #endregion

         #region Track-Ebene

         /// <summary>
         /// Track-Segment
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="segment">Segmentnummer (0...)</param>
         /// <returns></returns>
         public static string TrackSegment(int track, int segment) {
            return Track(track) + "/x:trkseg[" + (++segment).ToString() + "]";
         }
         /// <summary>
         /// Trackname
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string TrackName(int track) {
            return Track(track) + "/x:name";
         }
         /// <summary>
         /// Trackkommentar
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string TrackComment(int track) {
            return Track(track) + "/x:cmt";
         }
         /// <summary>
         /// Trackbeschreibung
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string TrackDescription(int track) {
            return Track(track) + "/x:desc";
         }
         /// <summary>
         /// Trackquelle
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string TrackSource(int track) {
            return Track(track) + "/x:src";
         }
         /// <summary>
         /// Tracklink
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="link">Linknummer (0..)</param>
         /// <returns></returns>
         public static string TrackLink(int track, int link) {
            return Track(track) + "/x:link[" + (++link).ToString() + "]";
         }
         /// <summary>
         /// Tracknummer
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string TrackNumber(int track) {
            return Track(track) + "/x:number";
         }
         /// <summary>
         /// Tracktyp
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string TrackType(int track) {
            return Track(track) + "/x:type";
         }
         /// <summary>
         /// Trackerweiterung
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <returns></returns>
         public static string TrackExtensions(int track) {
            return Track(track) + "/x:extensions";
         }

         #endregion

         #region Segment-Ebene eines Tracks

         /// <summary>
         /// Punkt eines Track-Segments
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="segment">Segmentnummer (0...)</param>
         /// <param name="point">Punktnummer (0...)</param>
         /// <returns></returns>
         public static string TrackSegmentPoint(int track, int segment, int point) {
            return TrackSegment(track, segment) + "/x:trkpt[" + (++point).ToString() + "]";
         }

         /// <summary>
         /// XPath für alle Daten aller Punkte eines Track-Segmentes
         /// </summary>
         /// <param name="track"></param>
         /// <param name="segment"></param>
         /// <returns></returns>
         public static string TrackSegmentPoints(int track, int segment) {
            return TrackSegment(track, segment) + "/x:trkpt/@*|" +
                   TrackSegment(track, segment) + "/x:trkpt/*";
         }

         #endregion

         #region Point-Ebene eines Track-Segments

         /// <summary>
         /// Latitude eines Track-Segment-Punktes
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="segment">Segmentnummer (0...)</param>
         /// <param name="point">Punktnummer (0...)</param>
         /// <returns></returns>
         public static string TrackSegmentPointLatitude(int track, int segment, int point) {
            return TrackSegmentPoint(track, segment, point) + "/@lat";
         }
         /// <summary>
         /// Longitude eines Track-Segment-Punktes
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="segment">Segmentnummer (0...)</param>
         /// <param name="point">Punktnummer (0...)</param>
         /// <returns></returns>
         public static string TrackSegmentPointLongitude(int track, int segment, int point) {
            return TrackSegmentPoint(track, segment, point) + "/@lon";
         }
         /// <summary>
         /// Höhe eines Track-Segment-Punktes
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="segment">Segmentnummer (0...)</param>
         /// <param name="point">Punktnummer (0...)</param>
         /// <returns></returns>
         public static string TrackSegmentPointElevation(int track, int segment, int point) {
            return TrackSegmentPoint(track, segment, point) + "/x:ele";
         }
         /// <summary>
         /// Zeit eines Track-Segment-Punktes
         /// </summary>
         /// <param name="track">Tracknummer (0...)</param>
         /// <param name="segment">Segmentnummer (0...)</param>
         /// <param name="point">Punktnummer (0...)</param>
         /// <returns></returns>
         public static string TrackSegmentPointTime(int track, int segment, int point) {
            return TrackSegmentPoint(track, segment, point) + "/x:time";
         }

         #endregion

         #region Waypoint-Ebene

         /// <summary>
         /// Waypointname
         /// </summary>
         /// <param name="track">Waypointnummer (0...)</param>
         /// <returns></returns>
         public static string WaypointName(int wp) {
            return Waypoint(wp) + "/x:name";
         }
         /// <summary>
         /// Latitude eines Waypoint
         /// </summary>
         /// <param name="wp">Punktnummer (0...)</param>
         /// <returns></returns>
         public static string WaypointLatitude(int wp) {
            return Waypoint(wp) + "/@lat";
         }
         /// <summary>
         /// Longitude eines Waypoint
         /// </summary>
         /// <param name="wp">Punktnummer (0...)</param>
         /// <returns></returns>
         public static string WaypointLongitude(int wp) {
            return Waypoint(wp) + "/@lon";
         }

         #endregion

         #region Route-Ebene

         /// <summary>
         /// Routename
         /// </summary>
         /// <param name="track">Routenummer (0...)</param>
         /// <returns></returns>
         public static string RouteName(int r) {
            return Route(r) + "/x:name";
         }

         #endregion

      }


      SimpleXmlDocument2 gpx;


      /// <summary>
      /// Dateiname
      /// </summary>
      public string Filename { get { return gpx.XmlFilename; } }


      public GpxFile(string filename,
                     string gpxcreator = "GpxFile",
                     string gpxversion = "1.1") {
         gpx = new SimpleXmlDocument2(filename, "gpx");

         gpx.CreateInternData();
         gpx.Validating = false;
         gpx.XsdFilename = null;

         string ext = "";
         if (!string.IsNullOrEmpty(gpxversion))
            ext += "  version=\"" + gpxversion + "\"";
         if (!string.IsNullOrEmpty(gpxcreator))
            ext += "  creator=\"" + gpxcreator + "\"";

         gpx.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                     "<gpx xmlns=\"http://www.topografix.com/GPX/1/1\"" + ext + ">" + // xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">"+
                     "</gpx>");
         gpx.AddNamespace("x");
      }

      /// <summary>
      /// liest die Datei ein
      /// </summary>
      /// <param name="xmlstream">Stream der XML-Daten (kann null sein)</param>
      /// <param name="xsdstream">Stream der XSD-Daten für die Validierung (kann null sein)</param>
      public void Read(Stream xmlstream = null, Stream xsdstream = null) {
         if (gpx.LoadData(xmlstream, xsdstream))
            gpx.AddNamespace("x");
      }

      /// <summary>
      /// speichert die Datei
      /// </summary>
      /// <param name="filename">Name der XML-Datei; ohne Name wird <see cref="XmlFilename"/> verwendet</param>
      /// <param name="formatted">wenn true, wird der Text formatiert ausgegeben, sonst "1-zeilig"</param>
      /// <param name="xmlstream">wenn ungleich null, wird der Stream für die Ausgabe verwendet</param>
      /// <param name="xsdstream">wenn ungleich null, wird der Stream für die Validierung verwendet, sonst <see cref="XsdFilename"/></param>
      public void Save(string filename = null,
                       bool formatted = true,
                       Stream xmlstream = null,
                       Stream xsdstream = null) {
         gpx.SaveData(filename, formatted, xmlstream, xsdstream);
      }

      /// <summary>
      /// liefert die Anzahl der Waypoints
      /// </summary>
      /// <returns></returns>
      public int WaypointCount() {
         int count;
         try {
            count = gpx.NodeCount(XPaths.Count_Waypoint());
         } catch {
            count = 0;
         }
         return count;
      }

      /// <summary>
      /// liefert die Anzahl der Routen
      /// </summary>
      /// <returns></returns>
      public int RouteCount() {
         int count;
         try {
            count = gpx.NodeCount(XPaths.Count_Route());
         } catch {
            count = 0;
         }
         return count;
      }

      /// <summary>
      /// liefert die Anzahl der Tracks
      /// </summary>
      /// <returns></returns>
      public int TrackCount() {
         int count;
         try {
            count = gpx.NodeCount(XPaths.Count_Track());
         } catch {
            count = 0;
         }
         return count;
      }

      /// <summary>
      /// liefert die Anzahl der Segmente im Track
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <returns></returns>
      public int TrackSegmentCount(int track) {
         int count;
         try {
            count = gpx.NodeCount(XPaths.Count_TrackSegment(track));
         } catch {
            count = 0;
         }
         return count;
      }

      /// <summary>
      /// liefert die Anzahl der Links im Track
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <returns></returns>
      public int TrackLinkCount(int track) {
         int count;
         try {
            count = gpx.NodeCount(XPaths.Count_TrackLink(track));
         } catch {
            count = 0;
         }
         return count;
      }


      /// <summary>
      /// liefert die Anzahl der Punkte im Segment des Tracks
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <returns></returns>
      public int TrackSegmentPointCount(int track, int segment) {
         int count;
         try {
            count = gpx.NodeCount(XPaths.Count_TrackSegmentPoint(track, segment));
         } catch {
            count = 0;
         }
         return count;
      }

      /// <summary>
      /// liefert den Namen des Tracks
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <returns></returns>
      public string GetTrackname(int track) {
         return gpx.ReadValue(XPaths.TrackName(track), "");
      }

      /// <summary>
      /// setzt den Tracknamen
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="name"></param>
      public void SetTrackname(int track, string name) {
         // Laut Definition GPX 1.1: <name> ist (wenn vorhanden) immer das 1 Element unterhalb von <trk>
         string path = XPaths.TrackName(track);
         if (gpx.ExistXPath(path)) {
            if (!string.IsNullOrEmpty(name))
               gpx.Change(path, name);
            else
               gpx.Remove(path);
         } else {
            if (!string.IsNullOrEmpty(name)) {
               gpx.InsertXmlText(XPaths.Track(track),
                                  string.Format("<name>{0}</name>", name),
                                  SimpleXmlDocument2.InsertPosition.PrependChild);
            }
         }
      }

      /// <summary>
      /// liest einen einzelnen Punkt ein
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      /// <returns></returns>
      public GpxPoint GetTrackSegmentPoint(int track, int segment, int point) {
         GpxPoint pt = new GpxPoint {
            Lon = gpx.ReadValue(XPaths.TrackSegmentPointLongitude(track, segment, point), GpxPoint.NOTVALID),
            Lat = gpx.ReadValue(XPaths.TrackSegmentPointLatitude(track, segment, point), GpxPoint.NOTVALID),
            Elevation = gpx.ReadValue(XPaths.TrackSegmentPointElevation(track, segment, point), GpxPoint.NOTVALID)
         };
         string tmp = gpx.ReadValue(XPaths.TrackSegmentPointTime(track, segment, point), DateTimeString(GpxPoint.NOTVALID_TIME));
         try {
            pt.Time = DateTime.Parse(tmp, null, DateTimeStyles.RoundtripKind);
         } catch { }
         return pt;
      }

      /// <summary>
      /// liefert alle Punkte eines Segmentes
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <returns></returns>
      public List<GpxPoint> GetTrackSegmentPointList(int track, int segment) {
         //int pointcount = TrackSegmentPointCount(track, segment);
         //List<GpxPoint> lst = new List<GpxPoint>(pointcount);
         //for (int i = 0; i < pointcount; i++)
         //   lst.Add(GetTrackSegmentPoint(track, segment, i));
         //return lst;

         SimpleXmlDocument2.XPathResult[] data = gpx.ReadValues(XPaths.TrackSegmentPoints(track, segment));
         List<GpxPoint> lst = new List<GpxPoint>();
         for (int i = 0; i < data.Length; i++) {
            GpxPoint pt = new GpxPoint(GpxPoint.NOTVALID, GpxPoint.NOTVALID, GpxPoint.NOTVALID, GpxPoint.NOTVALID_TIME);

            for (; i < data.Length; i++) {
               if (data[i].Localname == "lon")
                  if (pt.Lon != GpxPoint.NOTVALID) // schon gesetzt
                     break;
                  else
                     pt.Lon = Convert.ToDouble(data[i].Content, CultureInfo.GetCultureInfo("en-US"));

               else if (data[i].Localname == "lat")
                  if (pt.Lat != GpxPoint.NOTVALID) // schon gesetzt
                     break;
                  else
                     pt.Lat = Convert.ToDouble(data[i].Content, CultureInfo.GetCultureInfo("en-US"));

               else if (data[i].Localname == "ele")
                  if (pt.Elevation != GpxPoint.NOTVALID) // schon gesetzt
                     break;
                  else
                     pt.Elevation = Convert.ToDouble(data[i].Content, CultureInfo.GetCultureInfo("en-US"));

               else if (data[i].Localname == "time")
                  if (pt.Time != GpxPoint.NOTVALID_TIME) // schon gesetzt
                     break;
                  else
                     pt.Time = Convert.ToDateTime(data[i].Content);
            }
            i--;
            lst.Add(pt);
         }
         return lst;
      }

      /// <summary>
      /// liefert alle Punktlisten eines Tracks
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      public List<List<GpxPoint>> GetTrackPointLists(int track) {
         List<List<GpxPoint>> trackpt = new List<List<GpxPoint>>();
         for (int s = 0; s < TrackSegmentCount(track); s++)
            trackpt.Add(GetTrackSegmentPointList(track, s));
         return trackpt;
      }

      /// <summary>
      /// liefert den vollständigen Waypoint als XML-Text
      /// </summary>
      /// <param name="i"></param>
      /// <returns></returns>
      public string GetWaypointXml(int i) {
         return RemoveXmlns(gpx.GetXmlText(XPaths.Waypoint(i)));
      }

      /// <summary>
      /// liefert die vollständige Route als XML-Text
      /// </summary>
      /// <param name="i"></param>
      /// <returns></returns>
      public string GetRouteXml(int i) {
         return RemoveXmlns(gpx.GetXmlText(XPaths.Route(i)));
      }

      #region Track-Segment-Punkt-Bearbeitung

      /// <summary>
      /// ändert die Daten des Punktes
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      /// <param name="pt">Punkt mit neuen Daten</param>
      public void ChangeTrackSegmentPoint(int track, int segment, int point, GpxPoint pt) {
         ChangeTrackSegmentPointLatitude(track, segment, point, pt.Lat);
         ChangeTrackSegmentPointLongitude(track, segment, point, pt.Lon);
         ChangeTrackSegmentPointElevation(track, segment, point, pt.Elevation);
         ChangeTrackSegmentPointTime(track, segment, point, pt.Time);
      }
      /// <summary>
      /// ändert die Latitude des Punktes
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      /// <param name="lat"></param>
      public void ChangeTrackSegmentPointLatitude(int track, int segment, int point, double lat) {
         gpx.Change(XPaths.TrackSegmentPointLatitude(track, segment, point), lat.ToString(CultureInfo.InvariantCulture));
      }
      /// <summary>
      /// ändert die Longitude des Punktes
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      /// <param name="lon"></param>
      public void ChangeTrackSegmentPointLongitude(int track, int segment, int point, double lon) {
         gpx.Change(XPaths.TrackSegmentPointLongitude(track, segment, point), lon.ToString(CultureInfo.InvariantCulture));
      }
      /// <summary>
      /// ändert die Höhe des Punktes (oder löscht sie)
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      /// <param name="ele"></param>
      public void ChangeTrackSegmentPointElevation(int track, int segment, int point, double ele = GpxPoint.NOTVALID) {
         if (ele != GpxPoint.NOTVALID)
            if (gpx.ExistXPath(XPaths.TrackSegmentPointElevation(track, segment, point)))
               gpx.Change(XPaths.TrackSegmentPointElevation(track, segment, point), ele.ToString(CultureInfo.InvariantCulture));
            else
               gpx.InsertXmlText(XPaths.TrackSegmentPoint(track, segment, point), "<ele>" + ele.ToString(CultureInfo.InvariantCulture) + "</ele>", SimpleXmlDocument2.InsertPosition.PrependChild);
         else
            if (gpx.ExistXPath(XPaths.TrackSegmentPointElevation(track, segment, point)))
            gpx.Remove(XPaths.TrackSegmentPointElevation(track, segment, point));
      }
      /// <summary>
      /// ändert die Zeit des Punktes (oder löscht sie)
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      /// <param name="time"></param>
      public void ChangeTrackSegmentPointTime(int track, int segment, int point, DateTime time) {
         if (time != GpxPoint.NOTVALID_TIME)
            if (gpx.ExistXPath(XPaths.TrackSegmentPointTime(track, segment, point)))
               gpx.Change(XPaths.TrackSegmentPointTime(track, segment, point), DateTimeString(time));
            else
               if (gpx.ExistXPath(XPaths.TrackSegmentPointElevation(track, segment, point)))
               gpx.InsertXmlText(XPaths.TrackSegmentPointElevation(track, segment, point), "<time>" + DateTimeString(time) + "</time>", SimpleXmlDocument2.InsertPosition.After);
            else
               gpx.InsertXmlText(XPaths.TrackSegmentPoint(track, segment, point), "<time>" + DateTimeString(time) + "</time>", SimpleXmlDocument2.InsertPosition.PrependChild);
         else
            if (gpx.ExistXPath(XPaths.TrackSegmentPointTime(track, segment, point)))
            gpx.Remove(XPaths.TrackSegmentPointTime(track, segment, point));
      }

      /// <summary>
      /// fügt einen Punkt an der Position ein, oder hängt in an
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      /// <param name="pt">Punkt mit neuen Daten</param>
      public void InsertTrackSegmentPoint(int track, int segment, int point, GpxPoint pt) {
         string pointxml = pt.AsXml();
         int ptcount = TrackSegmentPointCount(track, segment);
         point = Math.Max(0, Math.Min(point, ptcount));        // 0 ... ptcount
         if (ptcount == 0)
            gpx.InsertXmlText(XPaths.TrackSegment(track, segment),
                               pointxml,
                               SimpleXmlDocument2.InsertPosition.AppendChild);
         else
            if (point < ptcount)
            gpx.InsertXmlText(XPaths.TrackSegmentPoint(track, segment, point),
                               pointxml,
                               SimpleXmlDocument2.InsertPosition.Before);
         else
            gpx.InsertXmlText(XPaths.TrackSegmentPoint(track, segment, point - 1),
                               pointxml,
                               SimpleXmlDocument2.InsertPosition.After);
      }

      /// <summary>
      /// entfernt den Punkt
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="point">Punktnummer (0...)</param>
      public void DeleteTrackSegmentPoint(int track, int segment, int point) {
         gpx.Remove(XPaths.TrackSegmentPoint(track, segment, point));
      }

      #endregion

      #region Track-Segment-Bearbeitung

      /// <summary>
      /// fügt ein Segment mit den Punkten an der Position ein
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      /// <param name="ptlst">Liste der GPX-Punkte</param>
      public void InsertSegment(int track, int segment, List<GpxPoint> ptlst) {
         int trackcount = TrackCount();
         if (trackcount == 0)
            throw new Exception("Kein Track vorhanden.");
         track = Math.Max(0, Math.Min(track, trackcount - 1));

         StringBuilder sb = new StringBuilder();
         sb.Append("<trkseg>");
         for (int i = 0; i < ptlst.Count; i++)
            sb.Append(ptlst[i].AsXml());
         sb.Append("</trkseg>");

         int segcount = TrackSegmentCount(track);
         segment = Math.Max(0, Math.Min(segment, segcount));        // 0 ... segcount
         if (segcount == 0) {
            // Sequenz unter <trk>: <name>, <cmt>, <desc>, <src>, <link> 0.., <number>, <type>, <extensions>, <trkseg> 0..
            if (gpx.ExistXPath(XPaths.TrackExtensions(track)))
               gpx.InsertXmlText(XPaths.TrackExtensions(track),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.After);
            else if (gpx.ExistXPath(XPaths.TrackNumber(track)))
               gpx.InsertXmlText(XPaths.TrackNumber(track),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.After);
            else if (TrackLinkCount(track) > 0)
               gpx.InsertXmlText(XPaths.TrackLink(track, TrackLinkCount(track) - 1),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.After);
            else if (gpx.ExistXPath(XPaths.TrackSource(track)))
               gpx.InsertXmlText(XPaths.TrackSource(track),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.After);
            else if (gpx.ExistXPath(XPaths.TrackDescription(track)))
               gpx.InsertXmlText(XPaths.TrackDescription(track),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.After);
            else if (gpx.ExistXPath(XPaths.TrackComment(track)))
               gpx.InsertXmlText(XPaths.TrackComment(track),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.After);
            else if (gpx.ExistXPath(XPaths.TrackName(track)))
               gpx.InsertXmlText(XPaths.TrackName(track),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.After);
            else
               gpx.InsertXmlText(XPaths.Track(track),
                                  sb.ToString(),
                                  SimpleXmlDocument2.InsertPosition.AppendChild);
         } else
            if (segment < segcount)
            gpx.InsertXmlText(XPaths.TrackSegment(track, segment),
                               sb.ToString(),
                               SimpleXmlDocument2.InsertPosition.Before);
         else
            gpx.InsertXmlText(XPaths.TrackSegment(track, segment - 1),
                               sb.ToString(),
                               SimpleXmlDocument2.InsertPosition.After);
      }

      /// <summary>
      /// löscht das Segment
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="segment">Segmentnummer (0...)</param>
      public void DeleteSegment(int track, int segment) {
         gpx.Remove(XPaths.TrackSegment(track, segment));
      }

      #endregion

      #region Track-Bearbeitung

      /// <summary>
      /// fügt einen Track mit den Punkten an der Position ein
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="ptlist">Punktliste je Segment</param>
      public void InsertTrack(int track, List<List<GpxPoint>> ptlist) {
         insertTrack(track, "<trk></trk>");
         for (int s = 0; s < ptlist.Count; s++)
            InsertSegment(track, s, ptlist[s]);
      }

      /// <summary>
      /// fügt einen Track als Kopie eines Tracks aus einer anderen GPX-Datei ein
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="fromgpx"></param>
      /// <param name="fromtrack">Quell-Tracknummer (0...)</param>
      public void InsertTrack(int track, GpxFile fromgpx, int fromtrack) {
         insertTrack(track, RemoveXmlns(fromgpx.gpx.GetXmlText(XPaths.Track(fromtrack))));
      }

      /// <summary>
      /// fügt den Track als XML-Text ein
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      /// <param name="trackxml"></param>
      void insertTrack(int track, string trackxml) {
         int trackcount = TrackCount();
         track = Math.Max(0, Math.Min(track, trackcount));
         // leeren Track erzeugen
         if (trackcount == 0) {
            // Sequenz unter <gpx>: <metadata>, <wpt> 0.., <rte> 0.., <trk> 0..
            int count = 0;
            count = RouteCount();
            if (count > 0) {
               gpx.InsertXmlText(XPaths.Route(count - 1),
                                  trackxml,
                                  SimpleXmlDocument2.InsertPosition.After);
            } else {
               count = WaypointCount();
               if (count > 0) {
                  gpx.InsertXmlText(XPaths.Waypoint(count - 1),
                                     trackxml,
                                     SimpleXmlDocument2.InsertPosition.After);
               } else {
                  if (gpx.ExistXPath(XPaths.Metadata()))
                     gpx.InsertXmlText(XPaths.Metadata(),
                                        trackxml,
                                        SimpleXmlDocument2.InsertPosition.After);
                  else
                     gpx.InsertXmlText("/x:gpx",
                                        trackxml,
                                        SimpleXmlDocument2.InsertPosition.AppendChild);
               }
            }
         } else
            if (track < trackcount)
            gpx.InsertXmlText(XPaths.Track(track),
                               trackxml,
                               SimpleXmlDocument2.InsertPosition.Before);
         else
            gpx.InsertXmlText(XPaths.Track(track - 1),
                               trackxml,
                               SimpleXmlDocument2.InsertPosition.After);
      }

      /// <summary>
      /// löscht den Track
      /// </summary>
      /// <param name="track">Tracknummer (0...)</param>
      public void DeleteTrack(int track) {
         gpx.Remove(XPaths.Track(track));
      }

      #endregion

      #region Routen-Bearbeitung

      /// <summary>
      /// fügt eine Route als Kopie einer Route aus einer anderen GPX-Datei ein
      /// </summary>
      /// <param name="route">Routennummer (0...)</param>
      /// <param name="fromgpx"></param>
      /// <param name="fromroute">Quell-Routennummer (0...)</param>
      public void InsertRoute(int route, GpxFile fromgpx, int fromroute) {
         InsertRoute(route, fromgpx.GetRouteXml(fromroute));
      }

      /// <summary>
      /// fügt die Route als XML-Text ein
      /// </summary>
      /// <param name="route">Routennummer (0...)</param>
      /// <param name="routexml"></param>
      public void InsertRoute(int route, string routexml) {
         int routecount = RouteCount();
         route = Math.Max(0, Math.Min(route, routecount));
         // leere Route erzeugen
         if (routecount == 0) {
            // Sequenz unter <gpx>: <metadata>, <wpt> 0.., <rte> 0..
            int count = 0;
            count = WaypointCount();
            if (count > 0) {
               gpx.InsertXmlText(XPaths.Waypoint(count - 1),
                                  routexml,
                                  SimpleXmlDocument2.InsertPosition.After);
            } else {
               if (gpx.ExistXPath(XPaths.Metadata()))
                  gpx.InsertXmlText(XPaths.Metadata(),
                                     routexml,
                                     SimpleXmlDocument2.InsertPosition.After);
               else
                  gpx.InsertXmlText("/x:gpx",
                                     routexml,
                                     SimpleXmlDocument2.InsertPosition.AppendChild);
            }
         } else
            if (route < routecount)
            gpx.InsertXmlText(XPaths.Route(route),
                               routexml,
                               SimpleXmlDocument2.InsertPosition.Before);
         else
            gpx.InsertXmlText(XPaths.Route(route - 1),
                               routexml,
                               SimpleXmlDocument2.InsertPosition.After);
      }

      /// <summary>
      /// löscht die Route
      /// </summary>
      /// <param name="route">Routennummer (0...)</param>
      public void DeleteRoute(int route) {
         gpx.Remove(XPaths.Route(route));
      }

      #endregion

      #region Waypoint-Bearbeitung

      /// <summary>
      /// fügt einen Waypoint als Kopie eines Waypoint aus einer anderen GPX-Datei ein
      /// </summary>
      /// <param name="waypt">Routennummer (0...)</param>
      /// <param name="fromgpx"></param>
      /// <param name="fromwaypt">Quell-Routennummer (0...)</param>
      public void InsertWaypoint(int waypt, GpxFile fromgpx, int fromwaypt) {
         InsertWaypoint(waypt, fromgpx.GetWaypointXml(fromwaypt));
      }

      /// <summary>
      /// fügt den Waypoint als XML-Text ein
      /// </summary>
      /// <param name="waypt">Waypointnummer (0...)</param>
      /// <param name="wayptxml"></param>
      public void InsertWaypoint(int waypt, string wayptxml) {
         int wayptcount = WaypointCount();
         waypt = Math.Max(0, Math.Min(waypt, wayptcount));
         // leeren Waypoint erzeugen
         if (wayptcount == 0) {
            // Sequenz unter <gpx>: <metadata>, <wpt> 0..
            if (gpx.ExistXPath(XPaths.Metadata()))
               gpx.InsertXmlText(XPaths.Metadata(),
                                  wayptxml,
                                  SimpleXmlDocument2.InsertPosition.After);
            else
               gpx.InsertXmlText("/x:gpx",
                                  wayptxml,
                                  SimpleXmlDocument2.InsertPosition.AppendChild);
         } else
            if (waypt < wayptcount)
            gpx.InsertXmlText(XPaths.Waypoint(waypt),
                               wayptxml,
                               SimpleXmlDocument2.InsertPosition.Before);
         else
            gpx.InsertXmlText(XPaths.Waypoint(waypt - 1),
                               wayptxml,
                               SimpleXmlDocument2.InsertPosition.After);
      }

      /// <summary>
      /// löscht den Waypoint
      /// </summary>
      /// <param name="waypt">Waypointnummer (0...)</param>
      public void DeleteWaypoint(int waypt) {
         gpx.Remove(XPaths.Waypoint(waypt));
      }

      #endregion

      /// <summary>
      /// ev. vorhandene Namespace-Angabe im 1. Knoten entfernen
      /// </summary>
      /// <param name="xml"></param>
      /// <returns></returns>
      string RemoveXmlns(string xml) {
         // xmlns="http://www.topografix.com/GPX/1/1"
         int pos1 = xml.IndexOf("xmlns");
         if (pos1 >= 0) {
            int pos2 = xml.IndexOf(">", pos1);
            xml = xml.Remove(pos1 - 1, pos2 - pos1 + 1);
         }
         return xml;
      }

      /// <summary>
      /// liefert den Namen des Waypoints
      /// </summary>
      /// <param name="wp">Waypointnummer (0...)</param>
      /// <returns></returns>
      public string GetWaypointname(int wp) {
         return gpx.ReadValue(XPaths.WaypointName(wp), "");
      }

      /// <summary>
      /// setzt den Tracknamen
      /// </summary>
      /// <param name="wp">Waypointnummer (0...)</param>
      /// <param name="name"></param>
      public void SetWaypointname(int wp, string name) {
         string path = XPaths.WaypointName(wp);
         if (gpx.ExistXPath(path)) {
            if (!string.IsNullOrEmpty(name))
               gpx.Change(path, name);
            else
               gpx.Remove(path);
         } else {
            if (!string.IsNullOrEmpty(name)) {
               gpx.InsertXmlText(XPaths.Waypoint(wp),
                                 string.Format("<name>{0}</name>", name),
                                 SimpleXmlDocument2.InsertPosition.PrependChild);
            }
         }
      }

      public double GetWaypointLatitude(int wp) {
         return Convert.ToDouble(gpx.ReadValue(XPaths.WaypointLatitude(wp), "0"), CultureInfo.InvariantCulture);
      }

      public double GetWaypointLongitude(int wp) {
         return Convert.ToDouble(gpx.ReadValue(XPaths.WaypointLongitude(wp), "0"), CultureInfo.InvariantCulture);
      }


      /// <summary>
      /// liefert den Namen der Route
      /// </summary>
      /// <param name="route">Routenummer (0...)</param>
      /// <returns></returns>
      public string GetRoutename(int route) {
         return gpx.ReadValue(XPaths.RouteName(route), "");
      }

      /// <summary>
      /// setzt den Tracknamen
      /// </summary>
      /// <param name="r">Routenummer (0...)</param>
      /// <param name="name"></param>
      public void SetRoutename(int r, string name) {
         string path = XPaths.RouteName(r);
         if (gpx.ExistXPath(path)) {
            if (!string.IsNullOrEmpty(name))
               gpx.Change(path, name);
            else
               gpx.Remove(path);
         } else {
            if (!string.IsNullOrEmpty(name)) {
               gpx.InsertXmlText(XPaths.Route(r),
                                 string.Format("<name>{0}</name>", name),
                                 SimpleXmlDocument2.InsertPosition.PrependChild);
            }
         }
      }


      /// <summary>
      /// hängt die Daten der GPX-Datei an die vorhandene Datei an (Waypoints, Routen und Tracks)
      /// </summary>
      /// <param name="fromgpx"></param>
      public void Concat(GpxFile fromgpx) {
         for (int i = 0; i < fromgpx.WaypointCount(); i++)
            InsertWaypoint(int.MaxValue, fromgpx, i);
         for (int i = 0; i < fromgpx.RouteCount(); i++)
            InsertRoute(int.MaxValue, fromgpx, i);
         for (int i = 0; i < fromgpx.TrackCount(); i++)
            InsertTrack(int.MaxValue, fromgpx, i);
      }

      /// <summary>
      /// liefert Datum und Zeit im Format für die GPX-Datei
      /// </summary>
      /// <param name="dt"></param>
      /// <returns></returns>
      static string DateTimeString(DateTime dt) {
         return dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");
      }


      public override string ToString() {
         return string.Format("{0}, {1} Tracks", Filename, TrackCount());
      }
   }
}
