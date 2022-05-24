using System;
using System.Collections.Generic;

namespace FSofTUtils {

   /// <summary>
   /// stark vereinfachter Zugriff auf SQLite3, der auf SQLite-net von Frank A. Krueger basiert (https://github.com/praeclarum/sqlite-net)
   /// </summary>
   public class MySimpleSQLite3 : IDisposable {

      public enum OpenFlags {
         /// <summary>
         /// The database is opened in read-only mode. If the database does not already exist, an error is returned.
         /// </summary>
         ReadOnly = 1,
         /// <summary>
         /// The database is opened for reading and writing if possible, or reading only if the file is write protected by the operating system. 
         /// In either case the database must already exist, otherwise an error is returned.
         /// </summary>
         ReadWrite = 2,
         /// <summary>
         /// zusammen mit <see cref="ReadWrite"/>: The database is opened for reading and writing, and is created if it does not already exist.
         /// </summary>
         Create = 4,

         //Uri = 0x40,
         //Memory = 0x80,

         /// <summary>
         /// zusätzlich: ... the database connection opens in the multi-thread threading mode as long as the single-thread mode has not been set at compile-time or start-time
         /// </summary>
         NoMutex = 0x8000,
         /// <summary>
         /// zusätzlich: ... the database connection opens in the serialized threading mode unless single-thread was previously selected at compile-time or start-time
         /// </summary>
         FullMutex = 0x10000,
         /// <summary>
         /// zusätzlich: ... the database connection to be eligible to use shared cache mode, regardless of whether or not shared cache is enabled
         /// </summary>
         SharedCache = 0x20000,
         /// <summary>
         /// zusätzlich: ... the database connection to not participate in shared cache mode even if it is enabled.
         /// </summary>
         PrivateCache = 0x40000,

         ProtectionComplete = 0x00100000,
         ProtectionCompleteUnlessOpen = 0x00200000,
         ProtectionCompleteUntilFirstUserAuthentication = 0x00300000,
         ProtectionNone = 0x00400000
      }

      public class TableInfo {

         public class ColumnInfo {
            /// <summary>
            /// Column index
            /// </summary>
            public int Idx { get; protected set; }
            /// <summary>
            /// Column name
            /// </summary>
            public string Name { get; protected set; }
            /// <summary>
            /// Column type, as given (CREATE TABLE ...)
            /// </summary>
            public string Type { get; protected set; }
            /// <summary>
            /// Has a NOT NULL constraint
            /// </summary>
            public bool NotNull { get; protected set; }
            /// <summary>
            /// DEFAULT value (oder null)
            /// </summary>
            public string DefaultValue { get; protected set; }
            /// <summary>
            /// Is part of the PRIMARY KEY
            /// </summary>
            public int PartOfPrimaryKey { get; protected set; }

            public ColumnInfo(int Idx,
                                      string Name,
                                      string Type,
                                      bool NotNull,
                                      string DefaultValue,
                                      int PartOfPrimaryKey) {
               this.Idx = Idx;
               this.Name = Name;
               this.Type = Type;
               this.NotNull = NotNull;
               this.DefaultValue = DefaultValue;
               this.PartOfPrimaryKey = PartOfPrimaryKey;
            }

            public override string ToString() {
               return string.Format("Idx={0}, {1},{2}{3}{4}, CREATE TABLE: {5}",
                                       Idx,
                                       Name,
                                       NotNull ? " not null" : "",
                                       DefaultValue != null ? " Def=" + DefaultValue : "",
                                       PartOfPrimaryKey > 0 ? " PrimKey=" + PartOfPrimaryKey.ToString() : "",
                                       Type);
            }

         }

         /// <summary>
         /// Tablename
         /// </summary>
         public string Name { get; protected set; }
         /// <summary>
         /// CREATE-Statement
         /// </summary>
         public string Sql { get; protected set; }
         /// <summary>
         /// Spalteninfos
         /// </summary>
         public List<ColumnInfo> ColumnInfos { get; protected set; }


         public TableInfo(string Name, string Sql, List<ColumnInfo> cols) {
            this.Name = Name;
            this.Sql = Sql;
            ColumnInfos = cols;
         }

         public override string ToString() {
            return "Table " + Name + ": " + Sql;
         }

      }

      /// <summary>
      /// Infos für alle Tabellen der DB
      /// </summary>
      public List<TableInfo> TableInfos { get; private set; }


      static readonly SQLitePCL.sqlite3 NullHandle = default(SQLitePCL.sqlite3);

      public string DatabasePath { get; protected set; }
      public int LibVersionNumber { get; protected set; }
      public SQLitePCL.sqlite3 Handle { get; private set; }

      bool _open;


      static MySimpleSQLite3() {
         SQLitePCL.Batteries_V2.Init();
      }

      public MySimpleSQLite3(string dbfilepath, OpenFlags openflags = OpenFlags.Create | OpenFlags.ReadWrite) {
         Handle = NullHandle;
         _open = false;

         DatabasePath = dbfilepath;
         LibVersionNumber = SQLite.SQLite3.LibVersionNumber();
         TableInfos = new List<TableInfo>();

         SQLite.SQLite3.Result r = SQLite.SQLite3.Open(DatabasePath, out SQLitePCL.sqlite3 handle, (int)openflags, null);
         if (r != SQLite.SQLite3.Result.OK)
            throw new Exception(string.Format("Could not open database file: {0} ({1})", DatabasePath, r.ToString()));

         Handle = handle;
         _open = true;
      }

      public void RebuildTableInfos() {
         TableInfos = new List<TableInfo>();
         List<string> colnames = new List<string>();

         List<List<object>> tabledata = Query("select * from sqlite_master where type='table'", colnames);
         int name_idx = ColumnIndex(colnames, "name");
         int sql_idx = ColumnIndex(colnames, "sql");
         if (name_idx >= 0 &&
             sql_idx >= 0) {
            for (int row = 1; row < tabledata.Count; row++) {
               List<TableInfo.ColumnInfo> cols = new List<TableInfo.ColumnInfo>();
               string tablename = tabledata[row][name_idx] as string;

               List<List<object>> coldata = Query(string.Format("pragma table_info('{0}')", tablename), colnames);
               int idx_idx = ColumnIndex(colnames, "cid");
               int nam_idx = ColumnIndex(colnames, "name");
               int typ_idx = ColumnIndex(colnames, "type");
               int non_idx = ColumnIndex(colnames, "notnull");
               int def_idx = ColumnIndex(colnames, "dflt_value");
               int ppk_idx = ColumnIndex(colnames, "pk");
               if (idx_idx >= 0 &&
                   nam_idx >= 0 &&
                   typ_idx >= 0 &&
                   non_idx >= 0 &&
                   def_idx >= 0 &&
                   ppk_idx >= 0) {

                  for (int r = 1; r < coldata.Count; r++)
                     cols.Add(new TableInfo.ColumnInfo(Convert.ToInt32(coldata[r][idx_idx]),
                                                     coldata[r][nam_idx] as string,
                                                     coldata[r][typ_idx] as string,
                                                     Convert.ToInt32(coldata[r][non_idx]) != 0,
                                                     coldata[r][def_idx] as string,
                                                     Convert.ToInt32(coldata[r][ppk_idx])));

                  TableInfos.Add(new TableInfo(tablename,
                                                      tabledata[row][sql_idx] as string,
                                                      cols));
               }
            }
         }
      }

      /*
       Das Ergebnis-Array enthält unabhängig vom Spaltentyp Werte im Format int, double, string, byte[] oder einen null-Wert.
       Eine sinnvolle Umwandlung in den Spaltentyp muss noch erfolgen.
       */

      List<List<object>> Query(SQLitePCL.sqlite3_stmt stmt, IList<RecommendedType> types) {
         List<List<object>> data = new List<List<object>>();
         try {
            int colcount = SQLite.SQLite3.ColumnCount(stmt);
            if (types != null &&
                types.Count < colcount)
               throw new Exception(string.Format("to less type-definitions ({0} < {1})", types.Count, colcount));

            while (SQLite.SQLite3.Step(stmt) == SQLite.SQLite3.Result.Row) {
               List<object> dataset = new List<object>();
               for (int i = 0; i < colcount; i++) {
                  if (types != null)

                     dataset.Add(GetFieldData(stmt, i, types[i]));

                  else {

                     SQLite.SQLite3.ColType type = SQLite.SQLite3.ColumnType(stmt, i); // der akt. Datentyp dieses Feldes
                     switch (type) {
                        case SQLite.SQLite3.ColType.Text:
                           dataset.Add(SQLite.SQLite3.ColumnString(stmt, i));
                           break;

                        case SQLite.SQLite3.ColType.Integer:
                           dataset.Add(SQLite.SQLite3.ColumnInt64(stmt, i));
                           //dataset.Add(SQLite.SQLite3.ColumnInt(stmt, i));
                           break;

                        case SQLite.SQLite3.ColType.Float:
                           dataset.Add(SQLite.SQLite3.ColumnDouble(stmt, i));
                           break;

                        case SQLite.SQLite3.ColType.Blob:
                           dataset.Add(SQLite.SQLite3.ColumnByteArray(stmt, i));
                           break;

                        case SQLite.SQLite3.ColType.Null:
                           dataset.Add(null);
                           break;
                     }

                  }
               }
               data.Add(dataset);
            }
         } finally {
            SQLite.SQLite3.Finalize(stmt);
         }
         return data;
      }

      /// <summary>
      /// liefert ein Objekt aus den internen Daten entsprechend des gewünschten Types (falls möglich)
      /// </summary>
      /// <param name="stmt"></param>
      /// <param name="colidx"></param>
      /// <param name="desttype"></param>
      /// <returns></returns>
      object GetFieldData(SQLitePCL.sqlite3_stmt stmt, int colidx, RecommendedType desttype) {
         /*
            SQLite Storage Classes (SQLite.SQLite3.ColType)
            Each value stored in an SQLite database has one of the following storage classes (von SQLite3.ColumnType() geliefert):
            1 	 NULL     The value is a NULL value.
            2 	 INTEGER  The value is a signed integer, stored in 1, 2, 3, 4, 6, or 8 bytes depending on the magnitude of the value.
            3 	 REAL     The value is a floating point value, stored as an 8-byte IEEE floating point number.
            4 	 TEXT     The value is a text string, stored using the database encoding (UTF-8, UTF-16BE or UTF-16LE)
            5 	 BLOB     The value is a blob of data, stored exactly as it was input.
          */
         SQLite.SQLite3.ColType coltype = SQLite.SQLite3.ColumnType(stmt, colidx); // der akt. Datentyp dieses Feldes
         switch (desttype) {
            case RecommendedType.String:
               /* falls intern kein Text, dann interne Umwandlung:
                     BLOB 	         Add a zero terminator if needed 
                     INTEGER 	      ASCII rendering of the integer
                     FLOAT 	      ASCII rendering of the float
                     NULL 	         Result is a NULL pointer
                */
               return SQLite.SQLite3.ColumnString(stmt, colidx);

            case RecommendedType.Int:
               /* falls intern kein Text, dann interne Umwandlung:
                     NULL 	         Result is 0
                     FLOAT 	      CAST to INTEGER
                     TEXT 	         CAST to INTEGER
                     BLOB 	         CAST to INTEGER
                */
               return SQLite.SQLite3.ColumnInt(stmt, colidx);

            case RecommendedType.Long:
               /* falls intern kein Text, dann interne Umwandlung:
                     NULL 	         Result is 0
                     FLOAT 	      CAST to INTEGER
                     TEXT 	         CAST to INTEGER
                     BLOB 	         CAST to INTEGER
                */
               return SQLite.SQLite3.ColumnInt64(stmt, colidx);

            case RecommendedType.Double:
               /* falls intern kein Text, dann interne Umwandlung:
                     NULL 	         Result is 0.0
                     INTEGER 	      Convert from integer to float
                     TEXT 	         CAST to REAL
                     BLOB 	         CAST to REAL
                */
               return SQLite.SQLite3.ColumnDouble(stmt, colidx);

            case RecommendedType.Decimal:
               /* falls intern kein Text, dann interne Umwandlung:
                     NULL 	         Result is 0.0
                     INTEGER 	      Convert from integer to float
                     TEXT 	         CAST to REAL
                     BLOB 	         CAST to REAL
                */
               return Convert.ToDecimal(SQLite.SQLite3.ColumnDouble(stmt, colidx));

            case RecommendedType.Bool:
               /* falls intern kein Text, dann interne Umwandlung:
                     NULL 	         Result is 0
                     FLOAT 	      CAST to INTEGER
                     TEXT 	         CAST to INTEGER
                     BLOB 	         CAST to INTEGER
                */
               return SQLite.SQLite3.ColumnInt(stmt, colidx) != 0;

            case RecommendedType.Bytes:
               /* falls intern kein Text, dann interne Umwandlung:
                     NULL 	         Result is a NULL pointer
                     INTEGER 	      Same as INTEGER->TEXT
                     FLOAT 	      CAST to BLOB
                     TEXT 	         No change
                */
               return SQLite.SQLite3.ColumnByteArray(stmt, colidx);

            case RecommendedType.DateTime:
               /* falls intern kein Text, dann interne Umwandlung:
                     NULL 	         Result is 0
                     FLOAT 	      CAST to INTEGER
                     TEXT 	         CAST to INTEGER
                     BLOB 	         CAST to INTEGER
                */
               switch (coltype) {
                  case SQLite.SQLite3.ColType.Integer:
                     return new DateTime(SQLite.SQLite3.ColumnInt64(stmt, colidx));

                  case SQLite.SQLite3.ColType.Text:
                     string text = SQLite.SQLite3.ColumnString(stmt, colidx);
                     DateTime resultDate;
                     if (!DateTime.TryParseExact(text,
                                                 "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff",
                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                 System.Globalization.DateTimeStyles.None,
                                                 out resultDate)) {
                        resultDate = DateTime.Parse(text);
                     }
                     return resultDate;

                  default:
                     return null;
               }

            default:
               return null;
         }
      }

      /// <summary>
      /// führt ein Select aus
      /// </summary>
      /// <param name="query"></param>
      /// <param name="colnames">liefert die Liste der Spaltennamen wenn ungleich null</param>
      /// <param name="types">null oder eine Liste der gewünschten Datentypen</param>
      /// <returns></returns>
      public List<List<object>> Query(string query, List<string> colnames = null, IList<RecommendedType> types = null) {
         SQLitePCL.sqlite3_stmt stmt = SQLite.SQLite3.Prepare2(Handle, query);

         if (colnames != null) {
            colnames.Clear();
            int colcount = SQLite.SQLite3.ColumnCount(stmt);
            for (int i = 0; i < colcount; i++)
               colnames.Add(SQLite.SQLite3.ColumnName(stmt, i));
         }

         return Query(stmt, types);
      }

      /// <summary>
      /// liefert aus der Liste der Spaltennamen den Spaltenindex (case insensitiv)
      /// </summary>
      /// <param name="colnames"></param>
      /// <param name="colname">Spaltenname</param>
      /// <returns></returns>
      public int ColumnIndex(List<string> colnames, string colname) {
         colname = colname.ToUpper();
         if (colnames.Count > 0)
            for (int i = 0; i < colnames.Count; i++)
               if (colnames[i] != null &&
                   colnames[i] is string &&
                   (colnames[i] as string).ToUpper() == colname)
                  return i;
         return -1;
      }

      public enum RecommendedType {
         String,
         Int,
         Long,
         Double,
         Decimal,
         Bool,
         DateTime,
         Bytes,
      }

      /// <summary>
      /// ermittelt den C#-Datentyp zur Spaltedef. (aus CREATE TABLE ...)
      /// </summary>
      /// <param name="columncreatetype"></param>
      /// <returns></returns>
      public RecommendedType RecommendedDataType(string columncreatetype) {
         columncreatetype = columncreatetype.ToUpper();
         /*
            SQLite Affinity and Type Names
            Following table lists down various data type names which can be used while creating SQLite3 tables with the corresponding applied affinity.
            TEXT                       INTEGER              NUMERIC           REAL               NONE
            ----------------------------------------------------------------------------------------------------------                                                                                     
            CHARACTER(20)              INT                  NUMERIC           REAL               BLOB
            VARCHAR(255)               INTEGER              DECIMAL(10,5)     DOUBLE             no datatype specified
            VARYING CHARACTER(255)     TINYINT              BOOLEAN           DOUBLE PRECISION   
            NCHAR(55)                  SMALLINT             DATE              FLOAT              
            NATIVE CHARACTER(70)       MEDIUMINT            DATETIME                             
            NVARCHAR(100)              BIGINT                                                    
            TEXT                       UNSIGNED BIG INT                                          
            CLOB                       INT2                                                      
                                       INT8                                                      
          */

         if (columncreatetype.IndexOf("CHAR") >= 0 ||
             columncreatetype == "TEXT" ||
             columncreatetype == "CLOB")
            return RecommendedType.String;

         if (columncreatetype.IndexOf("BIGINT") >= 0 ||
             columncreatetype.IndexOf("BIG INT") >= 0)
            return RecommendedType.Long;

         if (columncreatetype.IndexOf("INT") >= 0)
            return RecommendedType.Int;

         if (columncreatetype.IndexOf("BOOL") >= 0)
            return RecommendedType.Bool;

         if (columncreatetype.IndexOf("DATE") >= 0)
            return RecommendedType.DateTime;

         if (columncreatetype.IndexOf("NUMERIC") >= 0 ||
             columncreatetype.IndexOf("DECIMAL") >= 0)
            return RecommendedType.Decimal;

         return RecommendedType.Bytes;
      }

      int Execute(SQLitePCL.sqlite3_stmt stmt) {
         SQLite.SQLite3.Result result = SQLite.SQLite3.Step(stmt);
         SQLite.SQLite3.Finalize(stmt);
         switch (result) {
            case SQLite.SQLite3.Result.Done:
               return SQLite.SQLite3.Changes(Handle);

            case SQLite.SQLite3.Result.Error:
               throw new Exception(SQLite.SQLite3.GetErrmsg(Handle));

            default:
               throw new Exception(result.ToString());
         }
      }

      /// <summary>
      /// führt einen Nicht-Select-Befehl aus und löst im Fehlerfall eine Exception aus
      /// </summary>
      /// <param name="sqlcmd"></param>
      /// <returns></returns>
      public int Execute(string sqlcmd) {
         SQLitePCL.sqlite3_stmt stmt = SQLite.SQLite3.Prepare2(Handle, sqlcmd);
         return Execute(stmt);
      }

      /// <summary>
      /// The maximum number of bytes in the text of an SQL statement.
      /// (z.Z. keine Möglichkeit der direkten Abfrage oder des Setzens mit  sqlite3_limit(db, SQLITE_LIMIT_SQL_LENGTH, size) )
      /// </summary>
      public int SQLITE_MAX_SQL_LENGTH {
         get {
            return 1000000; // defaults; max. 1073741824
         }
      }


      #region Dispose

      ~MySimpleSQLite3() {
         Dispose(false);
      }

      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      public void Close() {
         Dispose(true);
      }

      protected virtual void Dispose(bool disposing) {
         if (_open &&
             Handle != NullHandle) {
            try {
               if (disposing) {


               }
               SQLite.SQLite3.Result result = LibVersionNumber >= 3007014 ?
                                                                        SQLite.SQLite3.Close2(Handle) :
                                                                        SQLite.SQLite3.Close(Handle);
               if (disposing &&
                   result != SQLite.SQLite3.Result.OK)
                  throw new Exception(SQLite.SQLite3.GetErrmsg(Handle));

            } finally {
               Handle = NullHandle;
               _open = false;
            }
         }
      }

      #endregion

   }

}
