using FSofTUtils.Xamarin;
using FSofTUtils.Xamarin.DependencyTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GPXToolOM {

   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class OruxmapPage : ContentPage {
      readonly object androidactivity;
      readonly StorageHelper sh;

      public enum TypeOfWork {
         nothing,
         ConcatTracks,
         SplitTracksegments,
         SplitTrack
      }

      /// <summary>
      /// vollständige Daten der Datenbank
      /// </summary>
      readonly OM_Data dbdata;

      /// <summary>
      /// Um welche Arbeit geht es?
      /// </summary>
      readonly TypeOfWork typeofwork;

      /// <summary>
      /// Noch bei der Arbeit?
      /// </summary>
      bool isBusy = false;
      
      readonly Color ButtonRun_EnabledTextColor;
      readonly Color ButtonRun_DiabledTextColor;


      public class ListViewObjectItem {

         static ulong IDCounter = 0;

         /// <summary>
         /// Track oder Waypoint
         /// </summary>
         public bool Track { get; protected set; }
         public string Text1 { get; protected set; }
         public string Text2 { get; protected set; }
         /// <summary>
         /// Sortiernummer in der Originalliste
         /// </summary>
         public ulong OrderNumber { get; protected set; }
         /// <summary>
         /// ID aus der Datenbank
         /// </summary>
         public long ObjectID { get; protected set; }

         public ListViewObjectItem(string text1, string text2, bool istrack, long objectid) {
            Text1 = text1;
            Text2 = text2;
            Track = istrack;
            OrderNumber = ++IDCounter;
            ObjectID = objectid;
         }

         public override string ToString() {
            return string.Format("{0}: ID={1}, OrderNumber={2}, Text1={3}, Text2={4}",
                                 Track ? "Track" : "Waypoint",
                                 ObjectID,
                                 OrderNumber,
                                 Text1,
                                 Text2);
         }

      }

      readonly List<ListViewObjectItem> tracklst;
      readonly List<ListViewObjectItem> waypointlst;
      readonly List<ListViewObjectItem> sampledlst;

      /// <summary>
      /// alle Tracknamen
      /// </summary>
      readonly List<string> tracknamelst;


      public OruxmapPage() {
         InitializeComponent();
      }

      public OruxmapPage(object androidactivity,
                         StorageHelper sh,
                         string sqlitefilename,
                         TypeOfWork type) : this() {
         this.androidactivity = androidactivity;
         this.sh = sh;
         typeofwork = type;
         if (!DesignMode.IsDesignModeEnabled) {
            dbdata = new OM_Data(sqlitefilename);
            ButtonRun_EnabledTextColor = Buttontext.TextColor;
            ButtonRun_DiabledTextColor = Color.LightGray;

            tracklst = new List<ListViewObjectItem>();
            waypointlst = new List<ListViewObjectItem>();
            sampledlst = new List<ListViewObjectItem>();

            tracknamelst = new List<string>();

            //////////////////////////////////////////////////
            // sinnvoll beim Testen
            //dbdata.Open();
            //try {
            //   for (int id = 238; id < 260; id++)
            //      dbdata.DeleteTrack(id, true);
            //} catch (Exception ex) {

            //}
            //dbdata.Close();
            //////////////////////////////////////////////////


         }
      }

      protected override void OnAppearing() {
         base.OnAppearing();
         if (!DesignMode.IsDesignModeEnabled) {

            switch (typeofwork) {
               case TypeOfWork.ConcatTracks:
                  Buttontext.Text = "Tracks verbinden";
                  SwitchTracksOrWaypoints.IsEnabled = false;
                  break;

               case TypeOfWork.SplitTracksegments:
                  Buttontext.Text = "Tracksegmente aufteilen";
                  SwitchTracksOrWaypoints.IsEnabled = false;
                  break;

               case TypeOfWork.SplitTrack:
                  Buttontext.Text = "Track aufteilen";
                  SwitchTracksOrWaypoints.IsEnabled = true;
                  break;

               default:
                  Buttontext.Text = "?";
                  break;
            }
            SetButtonRunStatus();

            if (sampledlst.Count == 0) // wenn schon eine Auswahl getroffen wurde, dann NICHT neu einlesen
               ReadDatabaseAndShowObjects();
         }
      }


      /// <summary>
      /// Ist die Source-Liste für Tracks oder Waypoints?
      /// </summary>
      bool SrcIsTrackList {
         get {
            return SwitchTracksOrWaypoints.IsToggled;
         }
      }

      /// <summary>
      /// näherungsweise Entfernung zwischen 2 Punkten in m
      /// </summary>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <returns></returns>
      double Distance(GpxFile.GpxPoint p1, GpxFile.GpxPoint p2) {
         return FSofTUtils.GeoHelper.Wgs84Distance(p1.Lon, p2.Lon, p1.Lat, p2.Lat, FSofTUtils.GeoHelper.Wgs84DistanceCompute.simple);
      }


      #region schreibender/lesender Zugriff auf die Datenbank

      /// <summary>
      /// liest die Datenbank neu ein und zeigt die Objekte an
      /// </summary>
      async void ReadDatabaseAndShowObjects() {
         try {

            dbdata.Open();
            dbdata.ReadAllData();
            dbdata.Close();

            tracknamelst.Clear();

            StringBuilder sb = new StringBuilder();
            tracklst.Clear();
            for (int i = 0; i < dbdata.Tracks.Count; i++) {
               OM_Data.Track track = dbdata.Tracks[i];
               tracknamelst.Add(track.name);
               sb.Clear();
               sb.Append("[" + track.name + "]");
               if (!string.IsNullOrEmpty(track.descr))
                  sb.Append(" " + track.descr);

               List<OM_Data.Segment> seglst = track.GetSegmentList(dbdata.Segments);
               int ptcount = 0;
               for (int s = 0; s < seglst.Count; s++)
                  ptcount += seglst[s].GetTrackpointList(dbdata.Trackpoints).Count;

               tracklst.Add(new ListViewObjectItem(sb.ToString(),
                                                   string.Format("{0}, Segmente: {1}, Punkte: {2}", UnixTime2LocalTimeString2(track.fechaini), seglst.Count, ptcount),
                                                   true,
                                                   track.id));
            }

            waypointlst.Clear();
            for (int i = 0; i < dbdata.Waypoints.Count; i++) {
               OM_Data.Waypoint waypoint = dbdata.Waypoints[i];
               sb.Clear();
               sb.Append("[" + waypoint.name + "]");
               if (!string.IsNullOrEmpty(waypoint.descr))
                  sb.Append(" " + waypoint.descr);
               waypointlst.Add(new ListViewObjectItem(sb.ToString(),
                                                      UnixTime2LocalTimeString2(waypoint.time),
                                                      false,
                                                      waypoint.id));
            }

            sampledlst.Clear();

            SetObjectListData();
            SetSampledListData();

         } catch (Exception ex) {
            await Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void WriteToDatabase(IList<GpxFileExt> gpx, long tracktemplateid) {
         try {
            for (int g = 0; g < gpx.Count; g++) {
               // Es werden nur Tracks in die DB geschrieben.
               for (int t = 0; t < gpx[g].TrackCount(); t++) {
                  InsertTrack(gpx[g].GetTrackPointLists(t), gpx[g].GetTrackname(t), tracktemplateid);
               }
            }
         } catch (Exception ex) {
            await Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      /// <summary>
      /// Track in die Datenbank schreiben
      /// </summary>
      /// <param name="trackpointlists"></param>
      /// <param name="name"></param>
      /// <param name="tracktemplateid"></param>
      void InsertTrack(List<List<GpxFile.GpxPoint>> trackpointlists, string name, long tracktemplateid) {
         // Track-Objekt erzeugen
         OM_Data.Track track = new OM_Data.Track(dbdata.GetTrack(tracktemplateid)) {
            name = name,
            descr = "",
            dir = null,
            id = 0
         };

         // ältesten Zeitpunkt und Schwerpunkt ermitteln
         DateTime tracktime = DateTime.MaxValue;
         int count = 0;
         double sumlat = 0;
         double sumlon = 0;
         for (int i = 0; i < trackpointlists.Count; i++) {
            for (int j = 0; j < trackpointlists[i].Count; j++) {
               GpxFile.GpxPoint pt = trackpointlists[i][j];
               if (pt.Time != GpxFile.GpxPoint.NOTVALID_TIME &&
                   pt.Time < tracktime)
                  tracktime = pt.Time;
               sumlat += pt.Lat;
               sumlon += pt.Lon;
               count++;
            }
         }

         if (tracktime < DateTime.MaxValue &&
             tracktime != GpxFile.GpxPoint.NOTVALID_TIME)
            track.fechaini = OM_Data.DateTime2UnixTime(tracktime);
         if (count > 0) {
            track.lat = sumlat / count;
            track.lon = sumlon / count;
         }

         // Track-Objekt in die DB einfügen
         dbdata.Open();

         dbdata.InsertTrack(track);
         int trackid = dbdata.GetLastInsertRowId();

         // Segmente mit ihren Punkten erzeugen und in die DB einfügen
         for (int s = 0; s < trackpointlists.Count; s++) {
            // statistische Daten ermitteln
            double dist = 0;
            DateTime starttime = GpxFile.GpxPoint.NOTVALID_TIME;
            DateTime endtime = GpxFile.GpxPoint.NOTVALID_TIME;
            double downalt = 0;
            double upalt = 0;
            double maxalt = double.MinValue;
            double minalt = double.MaxValue;

            for (int p = 0; p < trackpointlists[s].Count; p++) {
               GpxFile.GpxPoint gpxpt = trackpointlists[s][p];

               if (p == 0) {
                  starttime = gpxpt.Time;
               } else if (p > 0) {
                  GpxFile.GpxPoint oldgpxpt = trackpointlists[s][p - 1];
                  dist += Distance(gpxpt, oldgpxpt);
                  minalt = Math.Min(minalt, gpxpt.Elevation);
                  maxalt = Math.Max(maxalt, gpxpt.Elevation);
                  if (gpxpt.Elevation > oldgpxpt.Elevation)
                     upalt += gpxpt.Elevation - oldgpxpt.Elevation;
                  else
                     downalt += gpxpt.Elevation - oldgpxpt.Elevation;
                  if (p == trackpointlists[s].Count - 1) {
                     endtime = gpxpt.Time;
                  }
               }
            }
            OM_Data.Segment segment = new OM_Data.Segment(0, "", trackid) {
               dist = dist,
               downalt = downalt,
               upalt = upalt
            };
            if (starttime != GpxFile.GpxPoint.NOTVALID_TIME &&
                endtime != GpxFile.GpxPoint.NOTVALID_TIME) {
               segment.starttime = OM_Data.DateTime2UnixTime(starttime);
               segment.endtime = OM_Data.DateTime2UnixTime(endtime);
            }
            if (maxalt < double.MinValue)
               segment.maxalt = maxalt;
            if (minalt < double.MaxValue)
               segment.minalt = minalt;

            dbdata.InsertSegment(segment);
            int segmentid = dbdata.GetLastInsertRowId();

            // Punkte des Segments erzeugen und in die DB einfügen

            // Einzelnes Einfügen der Punkte dauert wesentlich länger !!!
            //for (int t = 0; t < trackpointlists[s].Count; t++) {
            //   GpxFile.GpxPoint gpxpt = trackpointlists[s][t];
            //   OM_Data.Trackpoint tp = new OM_Data.Trackpoint(0,
            //                                                  gpxpt.Lat,
            //                                                  gpxpt.Lon,
            //                                                  gpxpt.Elevation,
            //                                                  gpxpt.Time != GpxFile.GpxPoint.NOTVALID_TIME ? OM_Data.DateTime2UnixTime(gpxpt.Time) : 0,
            //                                                  segmentid);
            //   dbdata.InsertTrackpoint(tp);
            //}

            List<OM_Data.Trackpoint> tplst = new List<OM_Data.Trackpoint>();
            for (int p = 0; p < trackpointlists[s].Count; p++) {
               GpxFile.GpxPoint gpxpt = trackpointlists[s][p];
               tplst.Add(new OM_Data.Trackpoint(0,
                                                gpxpt.Lat,
                                                gpxpt.Lon,
                                                gpxpt.Elevation,
                                                gpxpt.Time != GpxFile.GpxPoint.NOTVALID_TIME ? OM_Data.DateTime2UnixTime(gpxpt.Time) : 0,
                                                segmentid));
            }
            dbdata.InsertTrackpoints(tplst);

         }
         dbdata.Close();
      }

      #endregion

      #region Umwandlungsfkt. Datenbank -> GPX

      /// <summary>
      /// liefert ein <see cref="GpxFileExt"/>-Objekt, dass den Datenbank-Track mit der ID enthält
      /// </summary>
      /// <param name="trackid"></param>
      /// <returns></returns>
      GpxFileExt GetTrackAsGpxFile(long trackid) {
         GpxFileExt gpx = null;
         if (sh != null) {
            OM_Data.Track track = dbdata.GetTrack(trackid);
            if (track != null) {
               gpx = new GpxFileExt(sh);
               List<List<GpxFile.GpxPoint>> trackpointlists = BuildGpxTrackPointlists(track.GetSegmentList(dbdata.Segments), dbdata.Trackpoints);
               gpx.InsertTrack(0, trackpointlists);
               gpx.SetTrackname(0, track.name);
            }
         }
         return gpx;
      }

      /// <summary>
      /// liefert ein <see cref="GpxFileExt"/>-Objekt, dass alle Datenbank-Waypoints mit der ID enthält
      /// </summary>
      /// <param name="waypointid"></param>
      /// <returns></returns>
      GpxFileExt GetWaypointsAsGpxFile(IList<long> waypointid) {
         GpxFileExt gpx = null;
         if (sh != null && waypointid != null) {
            int no = 0;
            if (waypointid.Count > 0)
               gpx = new GpxFileExt(sh);
            for (int w = 0; w < waypointid.Count; w++) {
               OM_Data.Waypoint wp = dbdata.GetWaypoint(waypointid[w]);
               if (wp != null) {
                  GpxFile.GpxPoint gpxwp = BuildGpxWaypoint(wp);
                  string xml = gpxwp.AsXml();
                  xml = "<wpt " + xml.Substring(7);                   // "<trkpt " -> "<wp "
                  xml = xml.Substring(0, xml.Length - 8) + "</wpt>";  // "</trkpt>" -> "</wp>"
                  gpx.InsertWaypoint(no, xml);
                  gpx.SetWaypointname(no, wp.name);
                  no++;
               }
            }
         }
         return gpx;
      }

      /// <summary>
      /// erzeugt aus dem Datenbank-Trackpoint einen GPX-Trackpoint
      /// </summary>
      /// <param name="tp"></param>
      /// <returns></returns>
      GpxFile.GpxPoint BuildGpxPoint(OM_Data.Trackpoint tp) {
         GpxFile.GpxPoint gp = new GpxFile.GpxPoint(tp.lon, tp.lat);
         if (tp.alt > 0)
            gp.Elevation = tp.alt;
         if (tp.time > 0)
            gp.Time = OM_Data.UnixTime2DateTime(tp.time);
         return gp;
      }

      /// <summary>
      /// erzeugt aus dem Datenbank-Trackpointliste einen GPX-Trackpointliste
      /// </summary>
      /// <param name="tps"></param>
      /// <returns></returns>
      List<GpxFile.GpxPoint> BuildGpxSegmentPointlist(List<OM_Data.Trackpoint> tps) {
         List<GpxFile.GpxPoint> gpxtps = new List<GpxFile.GpxPoint>();
         for (int t = 0; t < tps.Count; t++)
            gpxtps.Add(BuildGpxPoint(tps[t]));
         return gpxtps;
      }

      /// <summary>
      /// erzeugt aus dem Datenbank-Segmentliste eine Liste der GPX-Trackpointlisten je Segment
      /// </summary>
      /// <param name="segments"></param>
      /// <param name="alltrackpoints"></param>
      /// <returns></returns>
      List<List<GpxFile.GpxPoint>> BuildGpxTrackPointlists(List<OM_Data.Segment> segments, List<OM_Data.Trackpoint> alltrackpoints) {
         List<List<GpxFile.GpxPoint>> ptlist = new List<List<GpxFile.GpxPoint>>();
         for (int s = 0; s < segments.Count; s++)
            ptlist.Add(BuildGpxSegmentPointlist(segments[s].GetTrackpointList(alltrackpoints)));
         return ptlist;
      }

      /// <summary>
      /// erzeugt aus dem Datenbank-Waypoint einen GPX-Waypoint
      /// </summary>
      /// <param name="wp"></param>
      /// <returns></returns>
      GpxFile.GpxPoint BuildGpxWaypoint(OM_Data.Waypoint wp) {
         GpxFile.GpxPoint gp = new GpxFile.GpxPoint(wp.lon, wp.lat);
         if (wp.alt > 0)
            gp.Elevation = wp.alt;
         if (wp.time > 0)
            gp.Time = OM_Data.UnixTime2DateTime(wp.time);
         return gp;
      }

      #endregion

      void SetObjectListData() {
         if (SrcIsTrackList)
            SetTrackListData();
         else
            SetWaypointListData();
      }

      void SetTrackListData() {
         ListViewObjects.ItemsSource = null;
         ListViewObjects.ItemsSource = tracklst;
         LabelObjectType.Text = "Tracks: " + tracklst.Count.ToString();
      }

      void SetWaypointListData() {
         ListViewObjects.ItemsSource = null;
         ListViewObjects.ItemsSource = waypointlst;
         LabelObjectType.Text = "Waypoints: " + waypointlst.Count.ToString();
      }

      void SetSampledListData() {
         ListViewSampledObjects.ItemsSource = null;
         ListViewSampledObjects.ItemsSource = sampledlst;
         LabelSampled.Text = "Auswahl: " + sampledlst.Count.ToString();
      }

      /// <summary>
      /// <see cref="SwitchTracksOrWaypoints"/> wurde betätigt
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnSwitchTracksOrWaypoints_Toggled(object sender, ToggledEventArgs e) {
         if (SrcIsTrackList)
            SetTrackListData();
         else
            SetWaypointListData();
      }

      /// <summary>
      /// die Unix-Zeit wird als lokale Zeit "d.M.yyyy, H:mm" geliefert
      /// </summary>
      /// <param name="ms"></param>
      /// <returns></returns>
      string UnixTime2LocalTimeString1(long ms) {
         return ms == long.MinValue ?
                     "" :
                     OM_Data.UnixTime2DateTime(ms).ToLocalTime().ToString("d.M.yyyy, H:mm");
      }

      /// <summary>
      /// die Unix-Zeit wird als lokale Zeit "d.M.yyyy, H:mm:ss" geliefert
      /// </summary>
      /// <param name="ms"></param>
      /// <returns></returns>
      string UnixTime2LocalTimeString2(long ms) {
         return ms == long.MinValue ?
                     "" :
                     OM_Data.UnixTime2DateTime(ms).ToLocalTime().ToString("d.M.yyyy, H:mm:ss");
      }

      #region Tapped-Behandlung der Listen

      /*    ItemTapped fkt. im ListView NICHT, wenn ein Scrollview enthalten ist. Deshalb wird der Umweg über einen StackLayout.GestureRecognizers und die 
       *    Helper-Funktionen genommen und damit die beiden ItemTapped-Funktionen bedient.
       */

      private void OnListViewObjects_ItemTapped(object sender, ItemTappedEventArgs e) {
         if (!isBusy)
            Move(true, e.Item as ListViewObjectItem, e.ItemIndex);
      }

      private void OnListViewSampledObjects_ItemTapped(object sender, ItemTappedEventArgs e) {
         if (!isBusy)
            Move(false, e.Item as ListViewObjectItem, e.ItemIndex);
      }

      void OnListViewObjects_ItemTappedHelper(object sender, EventArgs e) {
         if (!isBusy)
            OnListView_ItemTappedHelper((sender as StackLayout).Children, true);
      }

      void OnListViewSampledObjects_ItemTappedHelper(object sender, EventArgs e) {
         if (!isBusy)
            OnListView_ItemTappedHelper((sender as StackLayout).Children, false);
      }

      void OnListView_ItemTappedHelper(IList<View> children, bool srclist) {
         if (!isBusy)
            // an Hand der Struktur das Item ermitteln: Das erste unsichtbare Child-Label enthält die Item-ID als Text.
            foreach (var child in children) {
               if (child is Label &&
                   !child.IsVisible) {
                  //child.Id.ToString() == "ItemID") {
                  ulong id = Convert.ToUInt64((child as Label).Text);
                  List<ListViewObjectItem> objects = (srclist ? ListViewObjects.ItemsSource : ListViewSampledObjects.ItemsSource) as List<ListViewObjectItem>;
                  for (int i = 0; i < objects.Count; i++) {
                     if (objects[i].OrderNumber == id)
                        if (srclist)
                           OnListViewObjects_ItemTapped(null, new ItemTappedEventArgs(null, objects[i], i));
                        else
                           OnListViewSampledObjects_ItemTapped(null, new ItemTappedEventArgs(null, objects[i], i));
                  }
               }
            }
      }

      #endregion

      /// <summary>
      /// liefert die ID-Listen der ausgewählten Tracks und Waypoints
      /// </summary>
      /// <param name="wplst"></param>
      /// <returns></returns>
      List<long> GetSelectedDataID(out List<long> wplst) {
         List<long> trlst = new List<long>();
         wplst = new List<long>();

         for (int i = 0; i < sampledlst.Count; i++) {
            if (sampledlst[i].Track) {
               trlst.Add(sampledlst[i].ObjectID);
            } else {
               wplst.Add(sampledlst[i].ObjectID);
            }
         }
         return trlst;
      }

      /// <summary>
      /// liefert einen eind. Tracknamen auf der Basis der Tracknamenliste <see cref="tracknamelst"/>
      /// </summary>
      /// <param name="basename"></param>
      /// <returns></returns>
      string GetUniqueTrackname(string basename) {
         for (int i = 1; ; i++) {
            string tmp = string.Format("{0} [{1}]", basename, i);
            if (!tracknamelst.Contains(tmp))
               return tmp;
         }
      }

      /// <summary>
      /// verschiebt ein Objekt von einer Liste in die andere
      /// </summary>
      /// <param name="fromsrc">wenn true, wird ein Objekt aus der Source-Liste entnommen</param>
      /// <param name="item">Item</param>
      /// <param name="itemindex">Item-Index in der ursprünglichen Liste</param>
      void Move(bool fromsrc, ListViewObjectItem item, int itemindex) {
         List<ListViewObjectItem> objectlst = SrcIsTrackList ? tracklst : waypointlst;

         if (fromsrc) {
            sampledlst.Add(item);
            objectlst.RemoveAt(itemindex);
         } else {
            if (item.Track == SrcIsTrackList) { // sonst die falsche Liste zur Rücknahme
               // Pos. über die ID ermitteln (erste ID, die größer ist)
               int idx = -1;
               for (int i = 0; i < objectlst.Count; i++)
                  if (objectlst[i].OrderNumber > item.OrderNumber) {
                     idx = i;
                     break;
                  }
               if (idx < 0) // keine Pos. gefunden
                  objectlst.Add(item);
               else
                  objectlst.Insert(idx, item);
               sampledlst.RemoveAt(itemindex);
            }
         }

         SetObjectListData();
         SetSampledListData();

         SetButtonRunStatus();
      }

      /// <summary>
      /// ToolbarItem für das neue Einlesen der Oruxmaps-Datenbank wurde ausgewählt
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ToolbarItem_Clicked(object sender, EventArgs e) {
         ReadDatabaseAndShowObjects();
      }


      private void ButtonRunTapped(object sender, EventArgs e) {
         List<long> trlst = GetSelectedDataID(out List<long> wplst);

         GpxFileExt gpxwp = GetWaypointsAsGpxFile(wplst);
         List<GpxFileExt> gpxtracks = new List<GpxFileExt>();
         for (int i = 0; i < trlst.Count; i++)
            gpxtracks.Add(GetTrackAsGpxFile(trlst[i]));


         // Alle Originaldaten bleiben immer erhalten!
         // Die Ergebnisse der Verarbeitung werden als zusätzliche Tracks gespeichert.

         switch (typeofwork) {
            case TypeOfWork.ConcatTracks:
               /* Alle Segmente der Tracks werden kopiert und in einem neuen Track zusammengeführt.
                * Die Trackdaten des 1. Originaltracks werden übernommen außer:
                *    id          Es wird eine neue ID verwendet.
                *    name        Es wird " Concat " und eine eindeutige Zahl angehängt.
                *    dir         null
                *    fechaini    Zeitpunkt des 1. Trackpunktes
                *    lat / lon   Mittelpunkt neu berechnet
                */
               SetBusyStatus(true);
#pragma warning disable 4014  // async-Aufruf ohne await
               ConcatTracksAsync(gpxtracks, trlst[0], GetUniqueTrackname(gpxtracks[0].GetTrackname(0)), ConcatTracksReady);
#pragma warning restore 4014
               break;

            case TypeOfWork.SplitTracksegments:
               /* Wenn im Track mehr als 1 Segement vorhanden ist, wird für jedes Segment ein neuer Track erzeugt.
                * Die Trackdaten des Originaltracks werden übernommen außer:
                *    id          Es wird eine neue ID verwendet.
                *    name        Es wird " Seg " und eine eindeutige Zahl (die Segmentnummer) angehängt.
                *    dir         null
                *    fechaini    Zeitpunkt des 1. Trackpunktes
                *    lat / lon   Mittelpunkt neu berechnet
                */
               SetBusyStatus(true);
#pragma warning disable 4014  // async-Aufruf ohne await
               SplitTracksAsync(gpxtracks, trlst, SplitTracksReady);
#pragma warning restore 4014
               break;

            case TypeOfWork.SplitTrack:
               /* Alle Segmente der Tracks werden in der Nähe der Waypoints getrennt und als neue Tracks gespeichert.
                * Die Trackdaten des jeweiligen Trackstracks werden übernommen außer:
                *    id          Es wird eine neue ID verwendet.
                *    name        Es wird " Split " und eine eindeutige Zahl angehängt.
                *    dir         null
                *    fechaini    Zeitpunkt des 1. Trackpunktes
                *    lat / lon   Mittelpunkt neu berechnet
                */
               SetBusyStatus(true);
#pragma warning disable 4014  // async-Aufruf ohne await
               Split2TracksAsync(gpxtracks, trlst, gpxwp, Split2TracksReady);
#pragma warning restore 4014
               break;
         }
      }

      #region Concat Tracks

      /// <summary>
      /// i.W. die Kapselung von <see cref="ConcatFiles"/> in eine async-Funktion
      /// <para>Nach Abschluss wird eine Callbak-Funktion aufgerufen, die auch die Fehlermeldung einer ev. aufgetretenen Exception erhält.</para>
      /// </summary>
      /// <param name="gpxfiles"></param>
      /// <param name="tracktemplateid"></param>
      /// <param name="trackname"></param>
      /// <param name="ready"></param>
      /// <returns></returns>
      private async Task ConcatTracksAsync(IList<GpxFileExt> gpxfiles, long tracktemplateid, string trackname, Action<bool, string> ready) {
         string errormsg = "";
         GpxFileExt newgpx = null;
         bool res = await Task.Run(() => {
            bool ok = true;
            try {
               newgpx = new GpxFileExt(sh);
               newgpx.ConcatFiles(gpxfiles,
                                  null,
                                  true,
                                  true,
                                  false,
                                  false,
                                  false);
               newgpx.SetTrackname(0, trackname);

               WriteToDatabase(new GpxFileExt[] { newgpx }, tracktemplateid);
            } catch (Exception ex) {
               errormsg = ex.Message;
               ok = false;
            }
            return ok;
         });
         ready(res, errormsg);
      }

      /// <summary>
      /// Callback-Funktion für die async-Funktion
      /// </summary>
      /// <param name="ok"></param>
      /// <param name="errormsg"></param>
      async void ConcatTracksReady(bool ok, string errormsg) {
         await ShowResult(ok,
                          ok ? "Verbinden" : "",
                          ok ? "Der neue Track wurde erzeugt." : errormsg);
      }

      #endregion

      #region Split Track to Segments

      /// <summary>
      /// i.W. die Kapselung von <see cref="SplitTracks"/> in eine async-Funktion
      /// <para>Nach Abschluss wird eine Callbak-Funktion aufgerufen, die auch die Fehlermeldung einer ev. aufgetretenen Exception erhält.</para>
      /// </summary>
      /// <param name="gpxfiles">alle aufzuteilenden <see cref="GpxFileExt"/></param>
      /// <param name="tracktemplateid"></param>
      /// <param name="ready"></param>
      /// <returns></returns>
      private async Task SplitTracksAsync(IList<GpxFileExt> gpxfiles, IList<long> tracktemplateid, Action<bool, string, int, int> ready) {
         string errormsg = "";
         int tracks = 0;
         int segments = 0;
         bool res = await Task.Run(() => {
            bool ok = true;
            try {
               for (int g = 0; g < gpxfiles.Count; g++, tracks++) {
                  List<GpxFileExt> newgpxfiles = gpxfiles[g].SplitTracks("");
                  for (int i = 0; i < newgpxfiles.Count; i++) {
                     string trackname = GetUniqueTrackname(gpxfiles[g].GetTrackname(0));
                     tracknamelst.Add(trackname);  // verwendeten Namen registrieren
                     newgpxfiles[i].SetTrackname(0, trackname);
                  }
                  segments += newgpxfiles.Count;
                  WriteToDatabase(newgpxfiles, tracktemplateid[g]);
               }
            } catch (Exception ex) {
               errormsg = ex.Message;
               ok = false;
            }
            return ok;
         });
         ready(res, errormsg, tracks, segments);
      }

      /// <summary>
      /// Callback-Funktion für die async-Funktion
      /// </summary>
      /// <param name="ok"></param>
      /// <param name="errormsg"></param>
      /// <param name="tracks"></param>
      /// <param name="segments"></param>
      async void SplitTracksReady(bool ok, string errormsg, int tracks, int segments) {
         await ShowResult(ok,
                          ok ? "Teilen" : "",
                          ok ? string.Format("Aus {0} Segmenten in {1} Track/s wurden Tracks erzeugt.", segments, tracks) : errormsg);
      }

      #endregion

      #region Split Tracks on Waypoints

      private async Task Split2TracksAsync(IList<GpxFileExt> gpxfiles, IList<long> tracktemplateid, GpxFileExt gpxfilewp, Action<bool, string, int, int> ready) {
         string errormsg = "";
         int tracks = 0;
         int newtracks = 0;

         bool res = await Task.Run(() => {
            bool ok = true;
            try {
               for (int g = 0; g < gpxfiles.Count; g++, tracks++) {
                  List<GpxFileExt> newgpxfiles = gpxfiles[g].SplitTracks(gpxfilewp, "", "");
                  for (int i = 0; i < newgpxfiles.Count; i++) {
                     string trackname = GetUniqueTrackname(gpxfiles[g].GetTrackname(0));
                     tracknamelst.Add(trackname);  // verwendeten Namen registrieren
                     newgpxfiles[i].SetTrackname(0, trackname);
                  }
                  newtracks += newgpxfiles.Count;
                  WriteToDatabase(newgpxfiles, tracktemplateid[g]);
               }
            } catch (Exception ex) {
               errormsg = ex.Message;
               ok = false;
            }
            return ok;
         });
         ready(res, errormsg, tracks, newtracks);

         //await Helper.MessageBox(this, "info", mymsg);
      }

      /// <summary>
      /// Callback-Funktion für die async-Funktion
      /// </summary>
      /// <param name="ok"></param>
      /// <param name="errormsg"></param>
      /// <param name="tracks"></param>
      /// <param name="newtracks"></param>
      async void Split2TracksReady(bool ok, string errormsg, int tracks, int newtracks) {
         await ShowResult(ok,
                          ok ? "Teilen" : "",
                          ok ? string.Format("{0} Track/s wurden in {1} neue Tracks geteilt.", tracks, newtracks) : errormsg);
      }

      #endregion

      /// <summary>
      /// Anzeige des Ergebnis (oder Fehlermeldung)
      /// </summary>
      /// <param name="ok"></param>
      /// <param name="caption"></param>
      /// <param name="msg"></param>
      /// <returns></returns>
      async Task ShowResult(bool ok, string caption, string msg) {
         if (ok)
            await Helper.MessageBox(this, caption, msg);
         else
            await Helper.MessageBox(this, "Fehler", msg);

         SetBusyStatus(false);
         ReadDatabaseAndShowObjects();
      }

      void SetBusyStatus(bool isbusy) {
         IsBusy = isBusy = isbusy;
         SwitchTracksOrWaypoints.IsEnabled = !isbusy && typeofwork == TypeOfWork.SplitTrack;
         SetButtonRunStatus();
      }

      /// <summary>
      /// <see cref="ButtonRun"/> aktivieren oder deaktivieren
      /// </summary>
      void SetButtonRunStatus() {
         List<long> trlst = GetSelectedDataID(out List<long> wplst);

         switch (typeofwork) {
            case TypeOfWork.ConcatTracks:
               ButtonRun.IsEnabled = !isBusy && trlst.Count > 1;
               break;

            case TypeOfWork.SplitTracksegments:
               ButtonRun.IsEnabled = !isBusy && trlst.Count > 0;
               break;

            case TypeOfWork.SplitTrack:
               ButtonRun.IsEnabled = !isBusy && trlst.Count > 0 && wplst.Count > 0;
               break;
         }
         Buttontext.TextColor = ButtonRun.IsEnabled ?
                                          ButtonRun_EnabledTextColor :
                                          ButtonRun_DiabledTextColor;

         NavigationPage.SetHasBackButton(this, !isBusy);    // den Soft-Backbutton in der Titelleiste entfernen oder sichtbar machen
      }

      /// <summary>
      /// Event that is raised when the hardware back button is pressed. This event is not raised on iOS.
      /// </summary>
      /// <returns></returns>
      protected override bool OnBackButtonPressed() {
         if (isBusy)
            return true; // Disable Backbutton

         return base.OnBackButtonPressed(); // Standard
      }

   }
}