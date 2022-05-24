using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FSofTUtils;

namespace GPXToolOM {
   class OM_Data {

      /*
      Alle Zeiten sind in Millisekunden seit dem 1.1.1970 (Unix-Zeit) angegeben.
       */

      public class Trackpoint {

         public readonly static string Tablename = "trackpoints";

         /// <summary>
         /// ID
         /// </summary>
         public long id = 0;
         /// <summary>
         /// geogr. Breite
         /// </summary>
         public double lat = 0;
         /// <summary>
         /// geogr. Länge
         /// </summary>
         public double lon = 0;
         /// <summary>
         /// Höhe (unbekannt 0)
         /// </summary>
         public double alt = 0;
         /// <summary>
         /// Zeitpunkt der Erzeugung
         /// </summary>
         public long time = 0;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public int track = int.MinValue;
         /// <summary>
         /// Segment-ID
         /// </summary>
         public int seg = 0;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string sen = null;


         public Trackpoint() { }

         public Trackpoint(Trackpoint obj) {
            id = obj.id;
            lat = obj.lat;
            lon = obj.lon;
            alt = obj.alt;
            time = obj.time;
            track = obj.track;
            seg = obj.seg;
            sen = obj.sen;
         }

         public Trackpoint(long id,
                           double lat,
                           double lon,
                           double alt,
                           long time,
                           int segment) : this() {
            this.id = id;
            this.lat = lat;
            this.lon = lon;
            this.alt = alt;
            this.time = time;
            this.seg = segment;
         }

      }

      public class Segment {

         public readonly static string Tablename = "segments";

         /// <summary>
         /// ID
         /// </summary>
         public long id = 0;
         /// <summary>
         /// Name (i.A. "Segment: 1")
         /// </summary>
         public string name = "";
         /// <summary>
         /// Beschreibung (i.A. null)
         /// </summary>
         public string descr = "";
         /// <summary>
         /// Zeitpunkt für 1. Punkt
         /// </summary>
         public long starttime = 0;
         /// <summary>
         /// Zeitpunkt für letzten Punkt
         /// </summary>
         public long endtime = 0;
         /// <summary>
         /// Dauer des Anstieges ?
         /// </summary>
         public long timeup = 0;
         /// <summary>
         /// Dauer des Abstieges ?
         /// </summary>
         public long timedown = 0;
         /// <summary>
         /// max. Höhe
         /// </summary>
         public double maxalt = 0;
         /// <summary>
         /// min. Höhe
         /// </summary>
         public double minalt = 0;
         /// <summary>
         /// Durchschnittsgeschwindigkeit
         /// </summary>
         public double avgspeed = 0;
         /// <summary>
         /// Gesamtanstieg ?
         /// </summary>
         public double upalt = 0;
         /// <summary>
         /// Gesamtabstieg ?
         /// </summary>
         public double downalt = 0;
         /// <summary>
         /// Länge
         /// </summary>
         public double dist = 0;
         /// <summary>
         /// Zeit "in Bewegung" ?
         /// </summary>
         public long timemov = 0;
         /// <summary>
         /// Track-ID
         /// </summary>
         public int track = 0;
         /// <summary>
         /// max. Geschwindigkeit
         /// </summary>
         public double maxspeed = 0;
         /// <summary>
         /// Farbe (ARGB)
         /// </summary>
         public int color = 0;
         /// <summary>
         /// ? (i.A. 12)
         /// </summary>
         public double stroke = 12;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public int fill = int.MinValue;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public int fillColor = int.MinValue;


         public Segment() { }

         public Segment(Segment obj) {
            id = obj.id;
            name = obj.name;
            descr = obj.descr;
            starttime = obj.starttime;
            endtime = obj.endtime;
            timeup = obj.timeup;
            timedown = obj.timedown;
            maxalt = obj.maxalt;
            minalt = obj.minalt;
            avgspeed = obj.avgspeed;
            upalt = obj.upalt;
            downalt = obj.downalt;
            dist = obj.dist;
            timemov = obj.timemov;
            track = obj.track;
            maxspeed = obj.maxspeed;
            color = obj.color;
            stroke = obj.stroke;
            fill = obj.fill;
            fillColor = obj.fillColor;
         }

         public Segment(long id, string name, int track) : this() {
            this.id = id;
            this.name = name;
            this.track = track;
         }

         /// <summary>
         /// liefert die Liste der zugehörigen Trackpoints
         /// </summary>
         /// <param name="alltrackpoints"></param>
         /// <returns></returns>
         public List<Trackpoint> GetTrackpointList(List<Trackpoint> alltrackpoints) {
            List<Trackpoint> lst = new List<Trackpoint>();
            for (int p = 0; p < alltrackpoints.Count; p++)
               if (alltrackpoints[p].seg == id)
                  lst.Add(new Trackpoint(alltrackpoints[p]));
            return lst;
         }

      }

      public class Track {

         public readonly static string Tablename = "tracks";

         /// <summary>
         /// ID
         /// </summary>
         public long id = 0;
         /// <summary>
         /// Name
         /// </summary>
         public string name = "";
         /// <summary>
         /// Beschreibung (i.A. leere Zeichenkette)
         /// </summary>
         public string descr = "";
         /// <summary>
         /// Zeitpunkt der Trackerzeugung (Start)
         /// </summary>
         public long fechaini = 0;
         /// <summary>
         /// ? (i.A. 0)
         /// </summary>
         public int estado = 0;
         /// <summary>
         /// Typ (Klassifizierung, i.A. 0)
         /// </summary>
         public int tipo = 0;
         /// <summary>
         /// Dateiname eines importierten Tracks (i.A. null)
         /// </summary>
         public string dir = null;
         /// <summary>
         /// Verzeichnisname (i.A. null für '--')
         /// </summary>
         public string folder = null;
         /// <summary>
         /// geogr. Breite (Mittelpunkt ?)
         /// </summary>
         public double lat = 0;
         /// <summary>
         /// geogr. Länge (Mittelpunkt ?)
         /// </summary>
         public double lon = 0;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string ciudad = null;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string pais = null;
         /// <summary>
         /// vermutlich für OM-Server ? (i.A. -1)
         /// </summary>
         public int idserver = -1;
         /// <summary>
         /// Schwierigkeit (i.A. 0)
         /// </summary>
         public int dificultad = 0;
         /// <summary>
         /// vermutlich für OM-Server ? (i.A. null)
         /// </summary>
         public string user = null;
         /// <summary>
         /// vermutlich für OM-Server ? (i.A. -1)
         /// </summary>
         public int userid = -1;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string ibpmod = null;
         /// <summary>
         /// ? (i.A. -1)
         /// </summary>
         public int ibp = -1;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string ibpref = null;
         /// <summary>
         /// ? (i.A. 0.049)
         /// </summary>
         public double coef = 0.049; // 0.0489999987185001
                                     /// <summary>
                                     /// ? (i.A. -1)
                                     /// </summary>
         public int strav = -1;


         public Track() { }

         public Track(Track obj) {
            id = obj.id;
            name = obj.name;
            descr = obj.descr;
            fechaini = obj.fechaini;
            estado = obj.estado;
            tipo = obj.tipo;
            dir = obj.dir;
            folder = obj.folder;
            lat = obj.lat;
            lon = obj.lon;
            ciudad = obj.ciudad;
            pais = obj.pais;
            idserver = obj.idserver;
            dificultad = obj.dificultad;
            user = obj.user;
            userid = obj.userid;
            ibpmod = obj.ibpmod;
            ibp = obj.ibp;
            ibpref = obj.ibpref;
            coef = obj.coef;
            strav = obj.strav;
         }

         public Track(long id, string name, string description) {
            this.id = id;
            this.name = name;
            this.descr = description;
         }

         /// <summary>
         /// liefert die Liste der zugehörigen Segmente
         /// </summary>
         /// <param name="trackpoints"></param>
         /// <returns></returns>
         public List<Segment> GetSegmentList(List<Segment> allsegments) {
            List<Segment> lst = new List<Segment>();
            for (int s = 0; s < allsegments.Count; s++)
               if (allsegments[s].track == id)
                  lst.Add(new Segment(allsegments[s]));
            return lst;
         }

      }

      public class Waypoint {

         public readonly static string Tablename = "pois";

         /// <summary>
         /// ID
         /// </summary>
         public long id = 0;
         /// <summary>
         /// Name
         /// </summary>
         public string name = "";
         /// <summary>
         /// Beschreibung (i.A. leere Zeichenkette)
         /// </summary>
         public string descr = "";
         /// <summary>
         /// geogr. Breite
         /// </summary>
         public double lat = 0;
         /// <summary>
         /// geogr. Länge
         /// </summary>
         public double lon = 0;
         /// <summary>
         /// Höhe (unbekannt 0)
         /// </summary>
         public double alt = 0;
         /// <summary>
         /// Zeitpunkt der Erzeugung
         /// </summary>
         public long time = 0;
         /// <summary>
         /// Typ (Klassifizierung, i.A. 1)
         /// </summary>
         public int tipo = 1;
         /// <summary>
         /// i.A. null
         /// </summary>
         public string uri = null;
         /// <summary>
         /// Trackzugehörigkeit ? (i.A. -1)
         /// </summary>
         public int track = -1;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string cache = null;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public int founddate = int.MinValue;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string notes = null;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string url = null;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string urlname = null;
         /// <summary>
         /// Verzeichnisname (i.A. null für '--')
         /// </summary>
         public string folder = "--";
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string ciudad = null;
         /// <summary>
         /// ? (i.A. null)
         /// </summary>
         public string pais = null;
         /// <summary>
         /// vermutlich für OM-Server ? (i.A. -1)
         /// </summary>
         public int idserver = -1;
         /// <summary>
         /// vermutlich für OM-Server ? (i.A. null)
         /// </summary>
         public string user = null;
         /// <summary>
         /// vermutlich für OM-Server ? (i.A. -1)
         /// </summary>
         public int userid = -1;
         /// <summary>
         /// max. Zoom
         /// </summary>
         public int ma_z = 25;
         /// <summary>
         /// min. Zoom
         /// </summary>
         public int mi_z = 0;


         public Waypoint() { }

         public Waypoint(Waypoint obj) {
            id = obj.id;
            name = obj.name;
            descr = obj.descr;
            lat = obj.lat;
            lon = obj.lon;
            alt = obj.alt;
            time = obj.time;
            tipo = obj.tipo;
            uri = obj.uri;
            track = obj.track;
            cache = obj.cache;
            founddate = obj.founddate;
            notes = obj.notes;
            url = obj.url;
            urlname = obj.urlname;
            folder = obj.folder;
            ciudad = obj.ciudad;
            pais = obj.pais;
            idserver = obj.idserver;
            user = obj.user;
            userid = obj.userid;
            ma_z = obj.ma_z;
            mi_z = obj.mi_z;
         }

         public Waypoint(long id,
                         string name,
                         string descr,
                         double lat,
                         double lon,
                         double alt,
                         long time) {
            this.id = id;
            this.name = name;
            this.descr = descr;
            this.lat = lat;
            this.lon = lon;
            this.alt = alt;
            this.time = time;
         }

      }

      /// <summary>
      /// Dateiname der SQLite-DB
      /// </summary>
      readonly string dbfilename;

      MySimpleSQLite3 db = null;

      public List<Track> Tracks;
      public List<Segment> Segments;
      public List<Trackpoint> Trackpoints;
      public List<Waypoint> Waypoints;

      public int Version { get; private set; }


      public OM_Data(string dbfilename) {
         if (string.IsNullOrEmpty(dbfilename))
            throw new Exception("Keine SQLite-Datenbank angegeben.");
         this.dbfilename = dbfilename;
      }

      string ReadAsString(object obj) {
         return obj == null ? null : Convert.ToString(obj);
      }

      int ReadAsInt(object obj) {
         return obj == null ? int.MinValue : Convert.ToInt32(obj);
      }

      long ReadAsLong(object obj) {
         return obj == null ? long.MinValue : Convert.ToInt64(obj);
      }

      double ReadAsDouble(object obj) {
         return obj == null ? double.MinValue : Convert.ToDouble(obj);
      }

      string WriteValue(string val) {
         return val == null ? "null" : "'" + val + "'";
      }

      string WriteValue(int val) {
         return val == int.MinValue ? "null" : val.ToString();
      }

      string WriteValue(long val) {
         return val == long.MinValue ? "null" : val.ToString();
      }

      string WriteValue(double val) {
         return val == double.MinValue ? "null" : val.ToString(CultureInfo.InvariantCulture);
      }

      /// <summary>
      /// Ist die DB geöffnet?
      /// </summary>
      public bool IsOpen {
         get {
            return db != null;
         }
      }

      /// <summary>
      /// öffnet die DB
      /// </summary>
      public void Open() {
         if (!IsOpen) {
            db = new MySimpleSQLite3(dbfilename);

            List<List<object>> query = Query("select _id from version");
            Version = ReadAsInt(query[0][0]);
         }
      }

      /// <summary>
      /// schließt die DB
      /// </summary>
      public void Close() {
         if (IsOpen) {
            db.Close();
            db.Dispose();
            db = null;
         }
      }


      public void ReadAllData() {
         Tracks = ReadAllTracks();
         Segments = ReadAllSegments();
         Trackpoints = ReadAllTrackpoints();
         Waypoints = ReadAllWaypoints();
      }


      /// <summary>
      /// liefert den Track mit der ID
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public Track GetTrack(long id) {
         for (int i = 0; i < Tracks.Count; i++)
            if (Tracks[i].id == id)
               return Tracks[i];
         return null;
      }

      /// <summary>
      /// liefert das Segment mit der ID
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public Segment GetSegment(long id) {
         for (int i = 0; i < Segments.Count; i++)
            if (Segments[i].id == id)
               return Segments[i];
         return null;
      }

      /// <summary>
      /// liefert den Trackpoint mit der ID
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public Trackpoint GetTrackpoint(long id) {
         for (int i = 0; i < Trackpoints.Count; i++)
            if (Trackpoints[i].id == id)
               return Trackpoints[i];
         return null;
      }

      /// <summary>
      /// liefert den Waypoints mit der ID
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public Waypoint GetWaypoint(long id) {
         for (int i = 0; i < Waypoints.Count; i++)
            if (Waypoints[i].id == id)
               return Waypoints[i];
         return null;
      }


      #region Waypoint-Bearbeitung

      public List<Waypoint> ReadAllWaypoints() {
         List<Waypoint> lst = new List<Waypoint>();

         StringBuilder sb = new StringBuilder();
         sb.Append("select _id");
         sb.Append(",poiname");
         sb.Append(",poidescr");
         sb.Append(",poilat");
         sb.Append(",poilon");
         sb.Append(",poialt");
         sb.Append(",poitime");
         sb.Append(",poitipo");
         sb.Append(",poiuri");
         sb.Append(",poitrack");
         sb.Append(",poicache");
         sb.Append(",poifounddate");
         sb.Append(",poinotes");
         sb.Append(",poiurl");
         sb.Append(",poiurlname");
         sb.Append(",poifolder");
         sb.Append(",poiciudad");
         sb.Append(",poipais");
         sb.Append(",poiidserver");
         sb.Append(",poiuser");
         sb.Append(",poiuserid");
         if (Version >= 17) {
            sb.Append(",poima_z");
            sb.Append(",poimi_z");
         }
         sb.Append(" from " + Waypoint.Tablename + " order by _id");
         List<List<object>> dat = Query(sb.ToString());

         for (int row = 0; row < dat.Count; row++) {
            lst.Add(new Waypoint(ReadAsLong(dat[row][0]),
                                 ReadAsString(dat[row][1]),
                                 ReadAsString(dat[row][2]),
                                 ReadAsDouble(dat[row][3]),
                                 ReadAsDouble(dat[row][4]),
                                 ReadAsDouble(dat[row][5]),
                                 ReadAsLong(dat[row][6])) {
               tipo = ReadAsInt(dat[row][7]),
               uri = ReadAsString(dat[row][8]),
               track = ReadAsInt(dat[row][9]),
               cache = ReadAsString(dat[row][10]),
               founddate = ReadAsInt(dat[row][11]),
               notes = ReadAsString(dat[row][12]),
               url = ReadAsString(dat[row][13]),
               urlname = ReadAsString(dat[row][14]),
               folder = ReadAsString(dat[row][15]),
               ciudad = ReadAsString(dat[row][16]),
               pais = ReadAsString(dat[row][17]),
               idserver = ReadAsInt(dat[row][18]),
               user = ReadAsString(dat[row][19]),
               userid = ReadAsInt(dat[row][20]),
               ma_z = Version >= 17 ? ReadAsInt(dat[row][21]) : 0,
               mi_z = Version >= 17 ? ReadAsInt(dat[row][22]) : 0,
            });
         }
         return lst;
      }

      public int UpdateWaypoint(Waypoint wp) {
         StringBuilder sb = new StringBuilder();
         sb.Append("update " + Waypoint.Tablename + " set");
         sb.Append(" poiname=" + WriteValue(wp.name));
         sb.Append(",poidescr=" + WriteValue(wp.descr));
         sb.Append(",poilat=" + WriteValue(wp.lat));
         sb.Append(",poilon=" + WriteValue(wp.lon));
         sb.Append(",poialt=" + WriteValue(wp.alt));
         sb.Append(",poitime=" + WriteValue(wp.time));
         sb.Append(",poitipo=" + WriteValue(wp.tipo));
         sb.Append(",poiuri=" + WriteValue(wp.uri));
         sb.Append(",poitrack=" + WriteValue(wp.track));
         sb.Append(",poicache=" + WriteValue(wp.cache));
         sb.Append(",poifounddate=" + WriteValue(wp.founddate));
         sb.Append(",poinotes=" + WriteValue(wp.notes));
         sb.Append(",poiurl=" + WriteValue(wp.url));
         sb.Append(",poiurlname=" + WriteValue(wp.urlname));
         sb.Append(",poifolder=" + WriteValue(wp.folder));
         sb.Append(",poiciudad=" + WriteValue(wp.ciudad));
         sb.Append(",poipais=" + WriteValue(wp.pais));
         sb.Append(",poiidserver=" + WriteValue(wp.idserver));
         sb.Append(",poiuser=" + WriteValue(wp.user));
         sb.Append(",poiuserid=" + WriteValue(wp.userid));
         if (Version >= 17) {
            sb.Append(",poima_z=" + WriteValue(wp.ma_z));
            sb.Append(",poimi_z=" + WriteValue(wp.mi_z));
         }
         sb.Append("where _id=" + wp.id.ToString());

         return Execute(sb.ToString());
      }

      public int InsertWaypoint(Waypoint wp) {
         StringBuilder sb = new StringBuilder();
         sb.Append("insert into " + Waypoint.Tablename);
         sb.Append("(poiname");
         sb.Append(",poidescr");
         sb.Append(",poilat");
         sb.Append(",poilon");
         sb.Append(",poialt");
         sb.Append(",poitime");
         sb.Append(",poitipo");
         sb.Append(",poiuri");
         sb.Append(",poitrack");
         sb.Append(",poicache");
         sb.Append(",poifounddate");
         sb.Append(",poinotes");
         sb.Append(",poiurl");
         sb.Append(",poiurlname");
         sb.Append(",poifolder");
         sb.Append(",poiciudad");
         sb.Append(",poipais");
         sb.Append(",poiidserver");
         sb.Append(",poiuser");
         sb.Append(",poiuserid");
         if (Version >= 17) {
            sb.Append(",poima_z");
            sb.Append(",poimi_z");
         }
         sb.Append(") values (");
         sb.Append(WriteValue(wp.name));
         sb.Append("," + WriteValue(wp.descr));
         sb.Append("," + WriteValue(wp.lat));
         sb.Append("," + WriteValue(wp.lon));
         sb.Append("," + WriteValue(wp.alt));
         sb.Append("," + WriteValue(wp.time));
         sb.Append("," + WriteValue(wp.tipo));
         sb.Append("," + WriteValue(wp.uri));
         sb.Append("," + WriteValue(wp.track));
         sb.Append("," + WriteValue(wp.cache));
         sb.Append("," + WriteValue(wp.founddate));
         sb.Append("," + WriteValue(wp.notes));
         sb.Append("," + WriteValue(wp.url));
         sb.Append("," + WriteValue(wp.urlname));
         sb.Append("," + WriteValue(wp.folder));
         sb.Append("," + WriteValue(wp.ciudad));
         sb.Append("," + WriteValue(wp.pais));
         sb.Append("," + WriteValue(wp.idserver));
         sb.Append("," + WriteValue(wp.user));
         sb.Append("," + WriteValue(wp.userid));
         if (Version >= 17) {
            sb.Append("," + WriteValue(wp.ma_z));
            sb.Append("," + WriteValue(wp.mi_z));
         }
         sb.Append(")");

         return Execute(sb.ToString());
      }

      public int DeleteWaypoint(Waypoint wp) {
         return DeleteTrackpoint(wp.id);
      }

      public int DeleteWaypoint(long id) {
         StringBuilder sb = new StringBuilder();
         sb.Append("delete from " + Waypoint.Tablename);
         sb.Append(" where _id=" + id.ToString());

         return Execute(sb.ToString());
      }

      #endregion

      #region Track-Bearbeitung

      public List<Track> ReadAllTracks() {
         List<Track> lst = new List<Track>();
         List<List<object>> dat = Query(@"select _id, 
trackname,
trackdescr,
trackfechaini,
trackestado,
tracktipo,
trackdir,
trackfolder,
tracklat,
tracklon,
trackciudad,
trackpais,
trackidserver,
trackdificultad,
trackuser,
trackuserid,
trackibpmod,
trackibp,
trackibpref,
trackcoef,
trackstrav
from " + Track.Tablename + " order by _id");

         for (int row = 0; row < dat.Count; row++) {
            lst.Add(new Track(ReadAsLong(dat[row][0]),
                              ReadAsString(dat[row][1]),
                              ReadAsString(dat[row][2])) {
               fechaini = ReadAsLong(dat[row][3]),
               estado = ReadAsInt(dat[row][4]),
               tipo = ReadAsInt(dat[row][5]),
               dir = ReadAsString(dat[row][6]),
               folder = ReadAsString(dat[row][7]),
               lat = ReadAsDouble(dat[row][8]),
               lon = ReadAsDouble(dat[row][9]),
               ciudad = ReadAsString(dat[row][10]),
               pais = ReadAsString(dat[row][11]),
               idserver = ReadAsInt(dat[row][12]),
               dificultad = ReadAsInt(dat[row][13]),
               user = ReadAsString(dat[row][14]),
               userid = ReadAsInt(dat[row][15]),
               ibpmod = ReadAsString(dat[row][16]),
               ibp = ReadAsInt(dat[row][17]),
               ibpref = ReadAsString(dat[row][18]),
               coef = ReadAsDouble(dat[row][19]),
               strav = ReadAsInt(dat[row][20]),
            });
         }
         return lst;
      }

      public int UpdateTrack(Track t) {
         StringBuilder sb = new StringBuilder();
         sb.Append("update " + Track.Tablename + " set");
         sb.Append(" trackname=" + WriteValue(t.name));
         sb.Append(",trackdescr=" + WriteValue(t.descr));
         sb.Append(",trackfechaini=" + WriteValue(t.fechaini));
         sb.Append(",trackestado=" + WriteValue(t.estado));
         sb.Append(",tracktipo=" + WriteValue(t.tipo));
         sb.Append(",trackdir=" + WriteValue(t.dir));
         sb.Append(",trackfolder=" + WriteValue(t.folder));
         sb.Append(",tracklat=" + WriteValue(t.lat));
         sb.Append(",tracklon=" + WriteValue(t.lon));
         sb.Append(",trackciudad=" + WriteValue(t.ciudad));
         sb.Append(",trackpais=" + WriteValue(t.pais));
         sb.Append(",trackidserver=" + WriteValue(t.idserver));
         sb.Append(",trackdificultad=" + WriteValue(t.dificultad));
         sb.Append(",trackuser=" + WriteValue(t.user));
         sb.Append(",trackuserid=" + WriteValue(t.userid));
         sb.Append(",trackibpmod=" + WriteValue(t.ibpmod));
         sb.Append(",trackibp=" + WriteValue(t.ibp));
         sb.Append(",trackibpref=" + WriteValue(t.ibpref));
         sb.Append(",trackcoef=" + WriteValue(t.coef));
         sb.Append(",trackstrav=" + WriteValue(t.strav));
         sb.Append("where _id=" + t.id.ToString());

         return Execute(sb.ToString());
      }

      public int InsertTrack(Track t) {
         StringBuilder sb = new StringBuilder();
         sb.Append("insert into " + Track.Tablename + @" (
trackname,
trackdescr,
trackfechaini,
trackestado,
tracktipo,
trackdir,
trackfolder,
tracklat,
tracklon,
trackciudad,
trackpais,
trackidserver,
trackdificultad,
trackuser,
trackuserid,
trackibpmod,
trackibp,
trackibpref,
trackcoef,
trackstrav) values ");
         sb.Append("(" + WriteValue(t.name));
         sb.Append("," + WriteValue(t.descr));
         sb.Append("," + WriteValue(t.fechaini));
         sb.Append("," + WriteValue(t.estado));
         sb.Append("," + WriteValue(t.tipo));
         sb.Append("," + WriteValue(t.dir));
         sb.Append("," + WriteValue(t.folder));
         sb.Append("," + WriteValue(t.lat));
         sb.Append("," + WriteValue(t.lon));
         sb.Append("," + WriteValue(t.ciudad));
         sb.Append("," + WriteValue(t.pais));
         sb.Append("," + WriteValue(t.idserver));
         sb.Append("," + WriteValue(t.dificultad));
         sb.Append("," + WriteValue(t.user));
         sb.Append("," + WriteValue(t.userid));
         sb.Append("," + WriteValue(t.ibpmod));
         sb.Append("," + WriteValue(t.ibp));
         sb.Append("," + WriteValue(t.ibpref));
         sb.Append("," + WriteValue(t.coef));
         sb.Append("," + WriteValue(t.strav) + ")");

         return Execute(sb.ToString());
      }

      public int DeleteTrack(Track t) {
         return DeleteTrackpoint(t.id);
      }

      public int DeleteTrack(long id, bool withsegments) {
         if (withsegments) {
            Execute("delete from " + Trackpoint.Tablename + " where trkptseg in (select _id from " + Segment.Tablename + " where segtrack=" + id.ToString() + ")");
            Execute("delete from " + Segment.Tablename + " where segtrack=" + id.ToString());
         }
         return Execute("delete from " + Track.Tablename + " where _id=" + id.ToString());
      }

      #endregion

      #region Tracksegment-Bearbeitung

      public List<Segment> ReadAllSegments() {
         List<Segment> lst = new List<Segment>();

         List<List<object>> dat = Query(@"select _id, 
segname,
segdescr,
segfechaini,
segfechafin,
segtimeup,
segtimedown,
segmaxalt,
segminalt,
segavgspeed,
segupalt,
segdownalt,
segdist,
segtimemov,
segtrack,
segmaxspeed,
segcolor,
segstroke,
segfill,
segfillColor
from " + Segment.Tablename + " order by _id");

         for (int row = 0; row < dat.Count; row++) {
            lst.Add(new Segment(ReadAsLong(dat[row][0]),
                                ReadAsString(dat[row][1]),
                                ReadAsInt(dat[row][14])) {
               descr = ReadAsString(dat[row][2]),
               starttime = ReadAsLong(dat[row][3]),
               endtime = ReadAsLong(dat[row][4]),
               timeup = ReadAsLong(dat[row][5]),
               timedown = ReadAsLong(dat[row][6]),
               maxalt = ReadAsDouble(dat[row][7]),
               minalt = ReadAsDouble(dat[row][8]),
               avgspeed = ReadAsDouble(dat[row][9]),
               upalt = ReadAsDouble(dat[row][10]),
               downalt = ReadAsDouble(dat[row][11]),
               dist = ReadAsDouble(dat[row][12]),
               timemov = ReadAsLong(dat[row][13]),
               //track = DB_ReadAsDouble(dat[row][14]),
               maxspeed = ReadAsDouble(dat[row][15]),
               color = ReadAsInt(dat[row][16]),
               stroke = ReadAsDouble(dat[row][17]),
               fill = ReadAsInt(dat[row][18]),
               fillColor = ReadAsInt(dat[row][19]),
            });
         }
         return lst;
      }

      public int UpdateSegment(Segment seg) {
         StringBuilder sb = new StringBuilder();
         sb.Append("update " + Segment.Tablename + " set");
         sb.Append(",segname=" + WriteValue(seg.name));
         sb.Append(",segdescr=" + WriteValue(seg.descr));
         sb.Append(",segfechaini=" + WriteValue(seg.starttime));
         sb.Append(",segfechafin=" + WriteValue(seg.endtime));
         sb.Append(",segtimeup=" + WriteValue(seg.timeup));
         sb.Append(",segtimedown=" + WriteValue(seg.timedown));
         sb.Append(",segmaxalt=" + WriteValue(seg.maxalt));
         sb.Append(",segminalt=" + WriteValue(seg.minalt));
         sb.Append(",segavgspeed=" + WriteValue(seg.avgspeed));
         sb.Append(",segupalt=" + WriteValue(seg.upalt));
         sb.Append(",segdownalt=" + WriteValue(seg.downalt));
         sb.Append(",segdist=" + WriteValue(seg.dist));
         sb.Append(",segtimemov=" + WriteValue(seg.timemov));
         sb.Append(",segtrack=" + WriteValue(seg.track));
         sb.Append(",segmaxspeed=" + WriteValue(seg.maxspeed));
         sb.Append(",segcolor=" + WriteValue(seg.color));
         sb.Append(",segstroke=" + WriteValue(seg.stroke));
         sb.Append(",segfill=" + WriteValue(seg.fill));
         sb.Append(",segfillColor=" + WriteValue(seg.fillColor));
         sb.Append("where _id=" + seg.id.ToString());

         return Execute(sb.ToString());
      }

      public int InsertSegment(Segment seg) {
         StringBuilder sb = new StringBuilder();
         sb.Append("insert into " + Segment.Tablename + @" (
segname,
segdescr,
segfechaini,
segfechafin,
segtimeup,
segtimedown,
segmaxalt,
segminalt,
segavgspeed,
segupalt,
segdownalt,
segdist,
segtimemov,
segtrack,
segmaxspeed,
segcolor,
segstroke,
segfill,
segfillColor) values ");
         sb.Append("(" + WriteValue(seg.name));
         sb.Append("," + WriteValue(seg.descr));
         sb.Append("," + WriteValue(seg.starttime));
         sb.Append("," + WriteValue(seg.endtime));
         sb.Append("," + WriteValue(seg.timeup));
         sb.Append("," + WriteValue(seg.timedown));
         sb.Append("," + WriteValue(seg.maxalt));
         sb.Append("," + WriteValue(seg.minalt));
         sb.Append("," + WriteValue(seg.avgspeed));
         sb.Append("," + WriteValue(seg.upalt));
         sb.Append("," + WriteValue(seg.downalt));
         sb.Append("," + WriteValue(seg.dist));
         sb.Append("," + WriteValue(seg.timemov));
         sb.Append("," + WriteValue(seg.track));
         sb.Append("," + WriteValue(seg.maxspeed));
         sb.Append("," + WriteValue(seg.color));
         sb.Append("," + WriteValue(seg.stroke));
         sb.Append("," + WriteValue(seg.fill));
         sb.Append("," + WriteValue(seg.fillColor) + ")");

         return Execute(sb.ToString());
      }

      public int DeleteSegment(Segment seg) {
         return DeleteSegment(seg.id);
      }

      public int DeleteSegment(long id, bool withtrkpt = false) {
         if (withtrkpt)
            Execute("delete from " + Trackpoint.Tablename + " where trkptseg=" + id.ToString());
         return Execute("delete from " + Segment.Tablename + " where _id=" + id.ToString());
      }

      #endregion

      #region Trackpoint-Bearbeitung

      public List<Trackpoint> ReadAllTrackpoints() {
         List<Trackpoint> lst = new List<Trackpoint>();

         List<List<object>> dat = Query(@"select _id, 
trkptlat,
trkptlon,
trkptalt,
trkpttime,
trkpttrack,
trkptseg,
trkptsen
from " + Trackpoint.Tablename + " order by _id");

         for (int row = 0; row < dat.Count; row++) {
            lst.Add(new Trackpoint(ReadAsLong(dat[row][0]),
                                   ReadAsDouble(dat[row][1]),
                                   ReadAsDouble(dat[row][2]),
                                   ReadAsDouble(dat[row][3]),
                                   ReadAsLong(dat[row][4]),
                                   ReadAsInt(dat[row][6])) {
               //lat = DB_ReadAsDouble(dat[row][1]),
               //lon = DB_ReadAsDouble(dat[row][2]),
               //alt = DB_ReadAsDouble(dat[row][3]),
               //time = DB_ReadAsInt(dat[row][4]),
               track = ReadAsInt(dat[row][5]),
               //seg = DB_ReadAsInt(dat[row][6]),
               sen = ReadAsString(dat[row][7]),
            });
         }
         return lst;
      }

      public int UpdateTrackpoint(Trackpoint tp) {
         StringBuilder sb = new StringBuilder();
         sb.Append("update " + Trackpoint.Tablename + " set");
         sb.Append(" trkptlat=" + WriteValue(tp.lat));
         sb.Append(",trkptlon=" + WriteValue(tp.lon));
         sb.Append(",trkptalt=" + WriteValue(tp.alt));
         sb.Append(",trkpttime=" + WriteValue(tp.time));
         sb.Append(",trkpttrack=" + WriteValue(tp.track));
         sb.Append(",trkptseg=" + WriteValue(tp.seg));
         sb.Append(",trkptsen=" + WriteValue(tp.sen));
         sb.Append("where _id=" + tp.id.ToString());

         return Execute(sb.ToString());
      }

      public int InsertTrackpoint(Trackpoint tp) {
         StringBuilder sb = new StringBuilder();
         sb.Append("insert into " + Trackpoint.Tablename + @" (
trkptlat,
trkptlon,
trkptalt,
trkpttime,
trkpttrack,
trkptseg,
trkptsen) values ");
         sb.Append("(" + WriteValue(tp.lat));
         sb.Append("," + WriteValue(tp.lon));
         sb.Append("," + WriteValue(tp.alt));
         sb.Append("," + WriteValue(tp.time));
         sb.Append("," + WriteValue(tp.track));
         sb.Append("," + WriteValue(tp.seg));
         sb.Append("," + WriteValue(tp.sen) + ")");

         return Execute(sb.ToString());
      }

      public int InsertTrackpoints(IList<Trackpoint> tplst) {
         StringBuilder sb = new StringBuilder();
         StringBuilder sbpt = new StringBuilder();
         int i = 0;
         int ret = 0;

         // Daten mit möglichst wenig SQL-Befehlen importieren (idealerweise mit 1)
         while (i < tplst.Count) {
            sb.Clear();
            sb.Append("insert into " + Trackpoint.Tablename + @" (
trkptlat,
trkptlon,
trkptalt,
trkpttime,
trkpttrack,
trkptseg,
trkptsen) values ");
            int corecmdlen = sb.Length;
            for (; i < tplst.Count; i++) {
               Trackpoint tp = tplst[i];
               sbpt.Clear();
               sbpt.Append("(" + WriteValue(tp.lat));
               sbpt.Append("," + WriteValue(tp.lon));
               sbpt.Append("," + WriteValue(tp.alt));
               sbpt.Append("," + WriteValue(tp.time));
               sbpt.Append("," + WriteValue(tp.track));
               sbpt.Append("," + WriteValue(tp.seg));
               sbpt.Append("," + WriteValue(tp.sen) + ")");

               if (sb.Length + sbpt.Length + 1 < db.SQLITE_MAX_SQL_LENGTH) {
                  if (sb.Length > corecmdlen)
                     sb.Append(",");
                  sb.Append(sbpt);
               } else
                  break;
            }
            ret += Execute(sb.ToString());
         }
         return ret;
      }

      public int DeleteTrackpoint(Trackpoint tp) {
         return DeleteTrackpoint(tp.id);
      }

      public int DeleteTrackpoint(long id) {
         StringBuilder sb = new StringBuilder();
         sb.Append("delete from " + Trackpoint.Tablename);
         sb.Append(" where _id=" + id.ToString());

         return Execute(sb.ToString());
      }

      #endregion



      /// <summary>
      /// liefert die ID des zuletzt eingefügten Datensatzes
      /// </summary>
      /// <returns></returns>
      public int GetLastInsertRowId() {
         List<List<object>> dat = Query("select last_insert_rowid()");
         if (dat.Count > 0 && dat[0].Count > 0)
            return ReadAsInt(dat[0][0]);
         return -1;
      }

      #region Datenbankzugriffe (Query, Execute)

      List<List<object>> Query(string sql) {
         if (db == null)
            throw new Exception("database is not open");
         return db.Query(sql);
      }

      int Execute(string sql) {
         if (db == null)
            throw new Exception("database is not open");
         return db.Execute(sql);
      }

      #endregion


      private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

      /// <summary>
      /// liefert die "Unix-Zeit" (The unix timestamp is traditionally a 32 bit integer, and has a range of '1970-01-01 00:00:01' UTC to '2038-01-19 03:14:07' UTC)
      /// </summary>
      /// <param name="dt"></param>
      /// <returns></returns>
      public static long DateTime2UnixTime(DateTime dt) {
         return (long)dt.Subtract(UnixEpoch).TotalMilliseconds;
      }

      public static DateTime UnixTime2DateTime(long ms) {
         return UnixEpoch.AddMilliseconds(ms);
      }

   }

}
