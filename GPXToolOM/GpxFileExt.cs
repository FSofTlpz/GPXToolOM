using FSofTUtils.Xamarin.DependencyTools;
using System;
using System.Collections.Generic;
using System.IO;

namespace GPXToolOM {

   public class GpxFileExt : GpxFile {

      /*    Oruxmaps
       *    - exportiert jeden Track in eine eigene Datei (Name aus Zeitpunkt abgeleitet)
       *    - ein Track kann mehrere Segmente enthalten
       *    - ein Track kann seine Punkte zusätzlich noch als Waypoints enthalten
       *    
       *    Garmin
       *    - eine GPX-Datei kann mehrere Tracks und Waypoints (und Routen) enthalten
       */

      #region Hilfsklassen

      class WaypointData {
         public double Latitude { get; }
         public double Longitude { get; }

         public WaypointData(double latitude, double longitude) {
            Latitude = latitude;
            Longitude = longitude;
         }

         public static bool operator ==(WaypointData w1, WaypointData w2) {
            return w1.Latitude == w2.Latitude && w1.Longitude == w2.Longitude;
         }

         public static bool operator !=(WaypointData w1, WaypointData w2) {
            return !(w1 == w2);
         }

         public override bool Equals(object obj) {
            return obj != null &&
                  (obj as WaypointData) != null &&
                  (obj as WaypointData).Latitude == (obj as WaypointData).Latitude && (obj as WaypointData).Longitude == (obj as WaypointData).Longitude;
         }

         public override int GetHashCode() {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
         }

         public override string ToString() {
            return string.Format("Lat {0}, Lon {1}", Latitude, Longitude);
         }
      }

      class SplitPoint : IComparable {

         public int Track { get; }
         public int Segment { get; }
         public int PointIdx { get; }

         public SplitPoint(int track, int segment, int ptidx) {
            Track = track;
            Segment = segment;
            PointIdx = ptidx;
         }

         int IComparable.CompareTo(object obj) {
            // A null value means that this object is greater.
            if (obj == null || (obj as SplitPoint) == null)
               return 1;
            else {
               SplitPoint compare = obj as SplitPoint;
               if (Track < compare.Track)
                  return -1;
               if (Track > compare.Track)
                  return 1;

               if (Segment < compare.Segment)
                  return -1;
               if (Segment > compare.Segment)
                  return 1;

               if (PointIdx < compare.PointIdx)
                  return -1;
               if (PointIdx > compare.PointIdx)
                  return 1;

               return 0;
            }
         }

         public override string ToString() {
            return string.Format("Track {0}, Segment {1}, Punkt {2}", Track, Segment, PointIdx);
         }
      }

      #endregion


#if ANDROID
      protected StorageHelper sh;
#endif


      public GpxFileExt(
#if ANDROID         
                        StorageHelper sh,
#endif
                        string filename = null,
                        string gpxcreator = "GpxFile",
                        string gpxversion = "1.1") :
         base(filename, gpxcreator, gpxversion) {
         this.sh = sh;
      }

      void Read() {
         Stream xmlstream = null;
         Stream xsdstream = null;

#if ANDROID
         xmlstream = sh.OpenFile(Filename, "rw");
#else
#endif

         base.Read(xmlstream, xsdstream);
      }

      void Save(string filename = null,
                bool formatted = true) {
         Stream xmlstream = null;
         Stream xsdstream = null;

#if ANDROID
         xmlstream = sh.OpenFile(Filename, "rw");
#else
#endif

         base.Save(filename, formatted, xmlstream, xsdstream);
      }

      bool FileExists(string filename) {
#if ANDROID
         return sh.FileExists(filename);
#else
         return File.Exists(filename);
#endif
      }

      void FileDelete(string filename) {
#if ANDROID
         sh.DeleteFile(filename);
#else
         File.Delete(filename);
#endif
      }


      #region Funktionen für Split

      /// <summary>
      /// alle Segmente der akt. GPX-Datei als eigene GPX-Datei speichern (ohne Übernahme von Waypoints und Routen)
      /// </summary>
      /// <param name="destbasefilename">Basisname für die Zieldateien</param>
      /// <param name="removeorgfiles">wenn true, werden die Originaledateien gelöscht</param>
      /// <param name="overwritedestfiles">wenn true, werden schon vorhandene Zieldateien überschrieben</param>
      /// <param name="creator">Name des erzeugenden Progs. in den neuen GPX-Dateien</param>
      /// <returns>Anzahl der neuen Dateien</returns>
      public int SplitTracks(string destbasefilename,
                             bool removeorgfiles,
                             bool overwritedestfiles,
                             string creator) {
         Read();
         if (TrackCount() == 0)
            throw new Exception("Kein Track in '" + Filename + "' vorhanden.");

         int segments = 0;
         for (int t = 0; t < TrackCount(); t++)
            segments += TrackSegmentCount(t);
         if (segments <= 1)
            throw new Exception("Nur 1 Track mit 1 Segment vorhanden.");

         List<GpxFileExt> gpxlst = Split(destbasefilename, creator);
         foreach (var gpx in gpxlst) {
            if (!overwritedestfiles &&
                FileExists(gpx.Filename))
               throw new Exception("Die Datei '" + gpx.Filename + "' darf nicht überschrieben werden.");
            gpx.Save();
         }

         return gpxlst.Count;
      }

      /// <summary>
      /// alle Segmente der akt. GPX-Datei werden als eigenes <see cref="GpxFileExt"/> geliefert
      /// </summary>
      /// <param name="creator">Name des erzeugenden Progs. in den neuen GPX-Dateien</param>
      /// <returns></returns>
      public List<GpxFileExt> SplitTracks(string creator) {
         if (TrackCount() == 0)
            throw new Exception("Kein Track in '" + Filename + "' vorhanden.");

         int segments = 0;
         for (int t = 0; t < TrackCount(); t++)
            segments += TrackSegmentCount(t);
         if (segments <= 1)
            throw new Exception("Nur 1 Track mit 1 Segment vorhanden.");

         return Split("", creator);
      }

      /// <summary>
      /// alle Tracks der akt. GPX-Datei werden an den jeweils zu den Trennpunkten nächstliegenden Punkten getrennt und
      /// als eigene GPX-Datei speichern (ohne Übernahme von Waypoints und Routen)
      /// </summary>
      /// <param name="splitpointfilename">GPX-Datei mit den "Trennpunkten"</param>
      /// <param name="destbasefilename">Basisname für die Zieldateien</param>
      /// <param name="removeorgfiles">wenn true, werden die Originaledateien gelöscht</param>
      /// <param name="overwritedestfiles">wenn true, werden schon vorhandene Zieldateien überschrieben</param>
      /// <param name="creator">Name des erzeugenden Progs. in den neuen GPX-Dateien</param>
      /// <returns>Anzahl der neuen Dateien</returns>
      public int SplitTracks(string splitpointfilename,
                             string destbasefilename,
                             bool removeorgfiles,
                             bool overwritedestfiles,
                             string creator) {
         Read();
         if (TrackCount() == 0)
            throw new Exception("Kein Track in '" + Filename + "' vorhanden.");

#if ANDROID
         GpxFileExt gpxwp = new GpxFileExt(sh, splitpointfilename);
#else
         GpxFileExt gpxwp = new GpxFileExt(splitpointfilename);
#endif
         gpxwp.Read();
         if (gpxwp.WaypointCount() == 0)
            throw new Exception("Keine \"Trennpunkte\" in '" + splitpointfilename + "' vorhanden.");

         List<SplitPoint> splitpoints = GetSplitPoints(gpxwp);

         int files = 0;

         // Trennpunkte je Track einsammeln, Track auftrennen und das Ergebnis speichern
         List<SplitPoint> splitpoints4track = new List<SplitPoint>();
         int trackno = -1;
         for (int i = 0; i < splitpoints.Count; i++) {
            if (trackno != splitpoints[i].Track) {
               trackno = splitpoints[i].Track;
               if (splitpoints4track.Count > 0) {
                  List<GpxFileExt> gpxFiles = Split(splitpoints4track,
                                                    GetTrackname(splitpoints[0].Track),
                                                    destbasefilename,
                                                    creator);
                  foreach (var gpx in gpxFiles) {
                     if (!overwritedestfiles &&
                          FileExists(gpx.Filename))
                        throw new Exception("Die Datei '" + gpx.Filename + "' darf nicht überschrieben werden.");
                     gpx.Save();
                  }
                  files += gpxFiles.Count;
                  splitpoints4track.Clear();
               }
            }
            splitpoints4track.Add(splitpoints[i]);
         }
         if (splitpoints4track.Count > 0) {
            List<GpxFileExt> gpxFiles = Split(splitpoints4track,
                                              GetTrackname(splitpoints[0].Track),
                                              destbasefilename,
                                              creator);
            foreach (var gpx in gpxFiles) {
               if (!overwritedestfiles &&
                    FileExists(gpx.Filename))
                  throw new Exception("Die Datei '" + gpx.Filename + "' darf nicht überschrieben werden.");
               gpx.Save();
            }
            files += gpxFiles.Count;
         }

         if (removeorgfiles) {
            FileDelete(Filename);
            FileDelete(splitpointfilename);
         }

         return files;
      }

      /// <summary>
      /// alle Tracks der akt. GPX-Datei werden an den jeweils zu den Trennpunkten nächstliegenden Punkten getrennt und
      /// als Liste von <see cref="GpxFileExt"/> geliefert (ohne Übernahme von Waypoints und Routen)
      /// </summary>
      /// <param name="gpxwp"><see cref="GpxFileExt"/> mit den "Trennpunkten"</param>
      /// <param name="destbasefilename">Basisname für die Zieldateien</param>
      /// <param name="creator">Name des erzeugenden Progs. in den neuen GPX-Dateien</param>
      /// <returns></returns>
      public List<GpxFileExt> SplitTracks(GpxFileExt gpxwp,
                                          string destbasefilename,
                                          string creator) {
         List<GpxFileExt> gpxlst = new List<GpxFileExt>();

         if (TrackCount() == 0)
            throw new Exception("Kein Track in '" + Filename + "' vorhanden.");

         if (gpxwp.WaypointCount() == 0)
            throw new Exception("Keine \"Trennpunkte\" vorhanden.");

         List<SplitPoint> splitpoints = GetSplitPoints(gpxwp);

         // Trennpunkte je Track einsammeln, Track auftrennen und das Ergebnis speichern
         List<SplitPoint> splitpoints4track = new List<SplitPoint>();
         int trackno = -1;
         for (int i = 0; i < splitpoints.Count; i++) {
            if (trackno != splitpoints[i].Track) {
               trackno = splitpoints[i].Track;
               if (splitpoints4track.Count > 0) {
                  List<GpxFileExt> gpxFiles = Split(splitpoints4track,
                                                    GetTrackname(splitpoints[0].Track),
                                                    destbasefilename,
                                                    creator);
                  gpxlst.AddRange(gpxFiles);

                  splitpoints4track.Clear();
               }
            }
            splitpoints4track.Add(splitpoints[i]);
         }
         if (splitpoints4track.Count > 0) {
            List<GpxFileExt> gpxFiles = Split(splitpoints4track,
                                              GetTrackname(splitpoints[0].Track),
                                              destbasefilename,
                                              creator);
            gpxlst.AddRange(gpxFiles);
         }

         return gpxlst;
      }

      /// <summary>
      /// Waypoints als Liste der Trennpunkte ermitteln (langsam)
      /// </summary>
      /// <param name="gpxwp"></param>
      /// <returns></returns>
      List<SplitPoint> GetSplitPoints(GpxFileExt gpxwp) {
         List<SplitPoint> splitpoints = new List<SplitPoint>();
         for (int i = 0; i < gpxwp.WaypointCount(); i++) {
            GpxPoint splitpoint = new GpxPoint(gpxwp.GetWaypointLongitude(i), gpxwp.GetWaypointLatitude(i));
            double distance = double.MaxValue;
            int track = -1;
            int segment = -1;
            int ptidx = -1;

            for (int t = 0; t < TrackCount(); t++) {
               for (int s = 0; s < TrackSegmentCount(t); s++) {
                  List<GpxPoint> pt = GetTrackSegmentPointList(t, s);
                  for (int p = 0; p < pt.Count; p++) {
                     double psdist = PointSquareDistance(splitpoint, pt[p]);
                     if (psdist < distance) {
                        track = t;
                        segment = s;
                        ptidx = p;
                        distance = psdist;
                     }
                     //if (GetLowestPointSquareDistance(splitpoint, pt[p], ref distance)) {
                     //   track = t;
                     //   segment = s;
                     //   ptidx = p;
                     //}
                  }
                  System.Diagnostics.Debug.WriteLine(string.Format(">>> split Track {0}, Segment {1}, Points {2}: idx={3}", t, s, pt.Count, ptidx));
               }
            }
            if (track >= 0)
               splitpoints.Add(new SplitPoint(track, segment, ptidx));
         }
         splitpoints.Sort(); // Punkte der Reihe nach sortieren

         return splitpoints;
      }

      /// <summary>
      /// Alle Tracks werden an den "Trennpunkten" geteilt. Jeder neue Track wird als eigenes <see cref="GpxFileExt"/> geliefert.
      /// </summary>
      /// <param name="splitpoints4track">Liste der "Trennpunkte"</param>
      /// <param name="orgtrackname"></param>
      /// <param name="destbasefilename"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      List<GpxFileExt> Split(List<SplitPoint> splitpoints4track, string orgtrackname, string destbasefilename, string creator) {
         List<GpxFileExt> lst = new List<GpxFileExt>();

         int count = 0;
         if (splitpoints4track != null) {
            List<List<List<GpxPoint>>> tracks = SplitTrack(splitpoints4track); // nur Tracks mit min. 1 Segement und jedes Segment mit min. 1 Punkt
            for (int i = 0; i < tracks.Count; i++) {
               if (tracks[i].Count > 0) {
                  string trackname = orgtrackname + ", [" + (i + 1).ToString() + "]";
                  string filename = destbasefilename + "_" + (i + 1).ToString() + ".gpx";
#if ANDROID
                  GpxFileExt newgpx = new GpxFileExt(sh, filename, creator);
#else
                  GpxFileExt newgpx = new GpxFileExt(filename, creator);
#endif
                  newgpx.InsertTrack(0, tracks[i]);
                  newgpx.SetTrackname(0, trackname);
                  lst.Add(newgpx);
                  count++;
               }
            }
         }

         return lst;
      }

      /// <summary>
      /// Jedes Segment der vorhandenen Tracks wird als eigenes <see cref="GpxFileExt"/> geliefert.
      /// </summary>
      /// <param name="destbasefilename"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      List<GpxFileExt> Split(string destbasefilename, string creator) {
         List<GpxFileExt> lst = new List<GpxFileExt>();

         int count = 0;
         List<List<GpxPoint>> track = new List<List<GpxPoint>>();
         for (int t = 0; t < TrackCount(); t++) {
            string orgtrackname = GetTrackname(t);
            for (int s = 0; s < TrackSegmentCount(t); s++) {
               string trackname = orgtrackname;
               if (TrackSegmentCount(t) > 1)
                  trackname += ", [" + (s + 1).ToString() + "]";
               track.Add(GetTrackSegmentPointList(t, s));

               string filename = destbasefilename + "_" + (count + 1).ToString() + ".gpx";
#if ANDROID
               GpxFileExt newgpx = new GpxFileExt(sh, filename, creator);
#else
               GpxFileExt newgpx = new GpxFileExt(filename, creator);
#endif
               newgpx.InsertTrack(0, track);
               newgpx.SetTrackname(0, trackname);
               lst.Add(newgpx);
               count++;

               track.Clear();
            }
         }

         return lst;
      }

      /// <summary>
      /// einen einzelnen Track splitten
      /// <para>Die neuen Tracks enthalten min. 1 Segment.</para>
      /// <para>Die neuen Tracks enthalten nur Segmente mit min. 1 Punkt.</para>
      /// </summary>
      /// <param name="splitpoints">alle Trennpunkte gehören zum gleichen Track</param>
      /// <returns></returns>
      List<List<List<GpxPoint>>> SplitTrack(List<SplitPoint> splitpoints) {
         if (splitpoints.Count > 0) {
            List<int> segment = new List<int>();
            List<int> ptidx = new List<int>();
            for (int i = 0; i < splitpoints.Count; i++) {
               segment.Add(splitpoints[i].Segment);
               ptidx.Add(splitpoints[i].PointIdx);
            }
            return SplitTrack(splitpoints[0].Track, segment, ptidx);
         }
         return null;
      }

      /// <summary>
      /// einen einzelnen Track splitten
      /// <para>Die neuen Tracks enthalten min. 1 Segment.</para>
      /// <para>Die neuen Tracks enthalten nur Segmente mit min. 1 Punkt.</para>
      /// </summary>
      /// <param name="track">Nummer des Originaltracks</param>
      /// <param name="splitsegment">sortierte (!) Liste der Segmente</param>
      /// <param name="splitptidx">sortierte (!) Liste der Punkte im Segment</param>
      List<List<List<GpxPoint>>> SplitTrack(int track, IList<int> splitsegment, IList<int> splitptidx) {
         if (splitsegment == null || splitptidx == null ||
             splitsegment.Count == 0 || splitsegment.Count != splitptidx.Count)
            throw new ArgumentException("Interner Fehler: Fehler bei Segment oder Punktangabe.");

         List<List<GpxPoint>> orgtrack = GetTrackPointLists(track);
         List<List<List<GpxPoint>>> newtracks = new List<List<List<GpxPoint>>>();

         int startsegment = 0;
         int startidx = 0;
         for (int i = 0; i <= splitsegment.Count; i++) {
            int endsegment = i < splitsegment.Count ? splitsegment[i] : orgtrack.Count - 1;
            int endidx = i < splitsegment.Count ? splitptidx[i] : orgtrack[orgtrack.Count - 1].Count - 1; // zum Abschluß letzter Punkt im letzten Segment

            List<List<GpxPoint>> newtrack = new List<List<GpxPoint>>();

            List<GpxPoint> tmpsegmentstart = new List<GpxPoint>(orgtrack[startsegment]); // Kopie erzeugen
            if (startsegment == endsegment) {

               if (startidx > 0)
                  tmpsegmentstart.RemoveRange(0, startidx); // ev. Anfang entfernen
               int tmpendidx = endidx - startidx;

               if (tmpendidx < tmpsegmentstart.Count - 1)
                  tmpsegmentstart.RemoveRange(tmpendidx + 1, tmpsegmentstart.Count - tmpendidx - 1);
               if (tmpsegmentstart.Count > 0)
                  newtrack.Add(tmpsegmentstart);

            } else {

               for (int s = startsegment + 1; s < endsegment; s++)
                  if (orgtrack[s].Count > 0)
                     newtrack.Add(orgtrack[s]);

               List<GpxPoint> tmpsegmentend = new List<GpxPoint>(orgtrack[endsegment]); // Kopie erzeugen
               if (endidx < tmpsegmentend.Count - 1)
                  tmpsegmentend.RemoveRange(endidx + 1, tmpsegmentend.Count - endidx - 1);
               if (tmpsegmentend.Count > 0)
                  newtrack.Add(tmpsegmentend);

            }

            startsegment = endsegment;
            startidx = endidx;

            if (newtrack.Count > 0) // min. 1 Segment
               newtracks.Add(newtrack);
         }
         return newtracks;
      }

      #endregion

      #region Funktionen für Concat

      /// <summary>
      /// verbindet alle Tracks und ihre Segmente zu einem neuen Track mit einem einzigen Segment
      /// <para>Es muss entweder eine Liste von <see cref="GpxFileExt"/> oder eine Liste von GPX-Dateinamen geliefert werden. Die jeweils andere Liste muss null sein.</para>
      /// <para>Wenn Filename ex., wird das Ergebnis zusätzlich als Datei gespeichert.</para>
      /// </summary>
      /// <param name="gpxfiles">Liste der <see cref="GpxFileExt"/> oder null</param>
      /// <param name="srcfilenames">Liste der Trackdateien oder null</param>
      /// <param name="shortestdist">wenn true, die am nächsten liegenden Anfangs-/Endpunkte verbinden</param>
      /// <param name="withwaypoints">wenn true, Waypoints auch übernehmen</param>
      /// <param name="withroutes">wenn true, Routen auch übernehmen</param>
      /// <param name="removeorgfiles">wenn true, werden die Originaledateien gelöscht</param>
      /// <param name="overwritedestfiles">wenn true, werden schon vorhandene Zieldateien überschrieben</param>
      public void ConcatFiles(IList<GpxFileExt> gpxfiles,
                              IList<string> srcfilenames,
                              bool shortestdist,
                              bool withwaypoints,
                              bool withroutes,
                              bool removeorgfiles,
                              bool overwritedestfile) {
         if (gpxfiles != null ||
             srcfilenames != null) {

            if (string.IsNullOrEmpty(Filename) &&
                FileExists(Filename) && !overwritedestfile)
               throw new Exception("Die Zieldatei existiert schon.");

            List<GpxPoint> tr = new List<GpxPoint>();
            Dictionary<string, List<WaypointData>> waypointnames = new Dictionary<string, List<WaypointData>>();
            Dictionary<string, int> routenames = new Dictionary<string, int>();

            if (gpxfiles != null) {

               foreach (var gpx in gpxfiles) {
                  ConcatGpxData(gpx, shortestdist, withwaypoints, withroutes, tr, waypointnames, routenames);
               }

            } else {

               foreach (var item in srcfilenames) {
#if ANDROID
                  GpxFileExt gpx = new GpxFileExt(sh, item);
#else
                  GpxFileExt gpx = new GpxFileExt(item);
#endif
                  gpx.Read();

                  ConcatGpxData(gpx, shortestdist, withwaypoints, withroutes, tr, waypointnames, routenames);

                  if (removeorgfiles)
                     FileDelete(gpx.Filename);
               }

            }

            for (int t = TrackCount() - 1; t >= 0; t--)
               DeleteTrack(t);

            List<List<GpxPoint>> track = new List<List<GpxPoint>> {
               tr
            };
            InsertTrack(0, track);
            if (string.IsNullOrEmpty(Filename)) {
               SetTrackname(0, Path.GetFileNameWithoutExtension(Filename));
               Save();
            }
         }
      }

      void ConcatGpxData(GpxFileExt gpx,
                         bool shortestdist,
                         bool withwaypoints,
                         bool withroutes,
                         List<GpxPoint> tr,
                         Dictionary<string, List<WaypointData>> waypointnames,
                         Dictionary<string, int> routenames) {

         gpx.ConcatTracks(shortestdist);
         AddPointList(tr, gpx.GetTrackSegmentPointList(0, 0), shortestdist);

         if (withwaypoints) {
            // Waypoints übernehmen:
            // wenn Name UND Koordinaten gleich sind -> KEINE Übernahme
            // wenn nur Name gleich ist -> Namenssuffix
            for (int w = 0; w < gpx.WaypointCount(); w++) {
               string name = gpx.GetWaypointname(w);
               WaypointData wd = new WaypointData(gpx.GetWaypointLatitude(w), gpx.GetWaypointLongitude(w));

               int pos = WaypointCount();
               if (waypointnames.ContainsKey(name)) {
                  bool coordexist = false;
                  foreach (var wditem in waypointnames[name]) {
                     if (wd == wditem) {
                        coordexist = true;
                        break;
                     }
                  }
                  if (!coordexist) {
                     waypointnames[name].Add(wd);  // neuer Punkt (gleicher Name, neue Koordinaten)
                     InsertWaypoint(pos, gpx.GetWaypointXml(w));
                     SetWaypointname(pos,
                                     name += " [" + waypointnames[name].Count.ToString() + "]");
                  }
               } else { // neuer Name
                  List<WaypointData> wplist = new List<WaypointData> {
                           wd
                        };
                  waypointnames[name] = wplist;
                  InsertWaypoint(pos, gpx.GetWaypointXml(w));
               }
            }
         }

         if (withroutes) {
            // Routen übernehmen
            // wenn Name schon ex. -> Namenssuffix
            for (int r = 0; r < gpx.RouteCount(); r++) {
               int pos = RouteCount();
               InsertRoute(pos, gpx.GetRouteXml(r));
               string name = gpx.GetRoutename(r);
               if (routenames.ContainsKey(name)) {
                  routenames[name]++;
                  SetRoutename(pos,
                               name += " [" + routenames[name].ToString() + "]");
               } else {
                  routenames[name] = 1;
               }
            }
         }

      }

      /// <summary>
      /// alle Tracks und ihre Segmente dieser Datei zu einem neuen Track mit einem einzigen Segment verbinden
      /// </summary>
      /// <param name="shortestdist"></param>
      void ConcatTracks(bool shortestdist) {
         List<GpxPoint> tr = ConcatAllTracks(shortestdist);

         for (int t = TrackCount() - 1; t >= 0; t--) // alte Tracks löschen
            DeleteTrack(t);

         List<List<GpxPoint>> track = new List<List<GpxPoint>> {
            tr
         };
         InsertTrack(0, track); // neuen Track speichern
      }

      /// <summary>
      /// erzeugt die Punktliste des "Gesamttracks" aller Tracks und ihrer Segmente
      /// </summary>
      /// <param name="shortestdist"></param>
      /// <returns></returns>
      List<GpxPoint> ConcatAllTracks(bool shortestdist) {
         List<GpxPoint> tr1 = ConcatAllSegments(0, shortestdist);
         for (int t = 1; t < TrackCount(); t++) {
            AddPointList(tr1, ConcatAllSegments(t, shortestdist), shortestdist);
         }
         return tr1;
      }

      /// <summary>
      /// erzeugt die Punktliste aller Segmente eines Tracks
      /// </summary>
      /// <param name="track"></param>
      /// <param name="shortestdist"></param>
      /// <returns></returns>
      List<GpxPoint> ConcatAllSegments(int track, bool shortestdist) {
         List<GpxPoint> seg1 = GetTrackSegmentPointList(track, 0);
         for (int s = 1; s < TrackSegmentCount(track); s++) {
            AddPointList(seg1, GetTrackSegmentPointList(track, s), shortestdist);
         }
         return seg1;
      }

      /// <summary>
      /// hängt die 2. Punktliste an die 1. Punktliste an
      /// </summary>
      /// <param name="lst1"></param>
      /// <param name="lst2"></param>
      /// <param name="shortestdist"></param>
      void AddPointList(List<GpxPoint> lst1, List<GpxPoint> lst2, bool shortestdist) {
         if (shortestdist) {
            if (lst1.Count == 0)
               lst1.AddRange(lst2);
            else {
               if (lst2.Count > 0) {
                  switch (GetShortestDistance(lst1[0], lst1[lst1.Count - 1], lst2[0], lst2[lst2.Count - 1])) {
                     case ShortestDistance.End1Start2:
                        lst1.AddRange(lst2);
                        break;

                     case ShortestDistance.Start1End2:
                        lst1.InsertRange(0, lst2);
                        break;

                     case ShortestDistance.End1End2:
                        lst2 = new List<GpxPoint>(lst2); // Kopie invertieren
                        InvertPointList(lst2);
                        lst1.AddRange(lst2);
                        break;

                     case ShortestDistance.Start1Start2:
                        InvertPointList(lst1);
                        lst1.AddRange(lst2);
                        break;
                  }
               }
            }
         } else {
            lst1.AddRange(lst2);
         }
      }

      /// <summary>
      /// Punktfolge "umkehren"
      /// </summary>
      /// <param name="pointlist"></param>
      void InvertPointList(List<GpxPoint> pointlist) {
         int lastidx = pointlist.Count - 1;
         for (int i = 0; i < pointlist.Count / 2; i++) {
            // Swap
            GpxPoint p = pointlist[i];
            pointlist[i] = pointlist[lastidx - i];
            pointlist[lastidx - i] = p;
         }
      }

      #endregion

      /// <summary>
      /// keine echte Differenz, sondern nur das Quadrat der Grad-Differenz
      /// </summary>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <returns></returns>
      double PointSquareDistance(GpxPoint p1, GpxPoint p2) {
         return (p1.Lat - p2.Lat) * (p1.Lat - p2.Lat) + (p1.Lon - p2.Lon) * (p1.Lon - p2.Lon);
      }

      /// <summary>
      /// keine echte Differenz, sondern nur das Quadrat der Grad-Differenz
      /// </summary>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <param name="dist">Bezugsdistanz</param>
      /// <returns>true, wenn die neue Distanz kleiner ist</returns>
      bool GetLowestPointSquareDistance(GpxPoint p1, GpxPoint p2, ref double dist) {
         double distance = double.MaxValue;
         if (Math.Abs(p1.Lat - p2.Lat) + Math.Abs(p1.Lon - p2.Lon) < dist) // sonst lohnt die exakte Berechnung nicht
            distance = PointSquareDistance(p1, p2);
         bool newislower = distance < dist;
         if (newislower)
            dist = distance;
         return newislower;
      }

      enum ShortestDistance {
         End1Start2,
         End1End2,
         Start1Start2,
         Start1End2
      }

      /// <summary>
      /// kürzesten Abstand ermitteln
      /// </summary>
      /// <param name="start1"></param>
      /// <param name="end1"></param>
      /// <param name="start2"></param>
      /// <param name="end2"></param>
      /// <returns></returns>
      ShortestDistance GetShortestDistance(GpxPoint start1, GpxPoint end1, GpxPoint start2, GpxPoint end2) {
         double dist_start1start2 = Math.Abs(PointSquareDistance(start1, start2));
         double dist_start1end2 = Math.Abs(PointSquareDistance(start1, end2));
         double dist_end1start2 = Math.Abs(PointSquareDistance(end1, start2));
         double dist_end1end2 = Math.Abs(PointSquareDistance(end1, end2));
         double dist_min = Math.Min(Math.Min(dist_start1start2, dist_start1end2), Math.Min(dist_end1start2, dist_end1end2));
         if (dist_min == dist_start1start2)
            return ShortestDistance.Start1Start2;
         else if (dist_min == dist_start1end2)
            return ShortestDistance.Start1End2;
         else if (dist_min == dist_end1start2)
            return ShortestDistance.End1Start2;
         return ShortestDistance.End1End2;
      }


   }
}
