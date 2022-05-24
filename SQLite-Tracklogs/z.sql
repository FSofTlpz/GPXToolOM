-- https://www.tutorialspoint.com/sqlite/index.htm

CREATE TABLE IF NOT EXISTS `tracks_wpts` (
	`trk`	integer,
	`wpt`	integer,
	PRIMARY KEY(`trk`,`wpt`)
);

CREATE TABLE folders (
   _id INTEGER CONSTRAINT PK_folders_id PRIMARY KEY ASC AUTOINCREMENT,   4       5
   folname VARCHAR (50),                                                 '---'   'wp1'
   foltype VARCHAR (3),                                                  NULL    NULL
   folactive BOOLEAN DEFAULT (1)                                         1       1
);

CREATE TABLE IF NOT EXISTS `pois` (
	`_id`	integer PRIMARY KEY AUTOINCREMENT,   2413
	`poiname`	text,                          'Bliem'
	`poidescr`	text,                          ''
	`poilat`	real,                             47.1574377122855
	`poilon`	real,                             11.8405869623442
	`poialt`	real,                             643.0
	`poitime`	integer,                       1532099824456    <- local: Jul 20 2018 17:17:04
	`poitipo`	integer,                       1
	`poiuri`	text,                             NULL
	`poitrack`	integer,                       -1
	`poicache`	text,                          NULL
	`poifounddate`	integer,                    NULL
	`poinotes`	text,                          NULL
	`poiurl`	text,                             NULL
	`poiurlname`	text,                       NULL
	`poifolder`	text,                          'wp1'            <-- folders
	`poiciudad`	text,                          NULL
	`poipais`	text,                          NULL
	`poiidserver`	integer DEFAULT -1,         -1
	`poiuser`	text,                          NULL
	`poiuserid`	integer DEFAULT -1,            -1
	`poima_z`	integer DEFAULT 25,            25
	`poimi_z`	integer DEFAULT 0              0
);

CREATE TABLE IF NOT EXISTS `tracks` (
	`_id`	integer PRIMARY KEY AUTOINCREMENT,     233                     234                     235
	`trackname`	text,                            '2019-03-30 10:54'      '2019-03-30 13:07'      '2019-03-30 15:32'
	`trackdescr`	text,                         ''                      ''                      ''
	`trackfechaini`	integer,                   1553939660450           1553947675905           1553956357076           <- local time: Mar 30 2019 10:54:20 (erster Point)
	`trackestado`	integer,                      0                       0                       0
	`tracktipo`	integer,                         0                       0                       0
	`trackdir`	text,                            NULL                    NULL                    NULL
	`trackfolder`	text,                         NULL                    NULL                    NULL
	`tracklat`	real DEFAULT 999,                51.86212047             51.86711691             51.86052576
	`tracklon`	real DEFAULT 999,                13.96103297             13.97314967             13.93809447
	`trackciudad`	text,                         NULL                    NULL                    NULL
	`trackpais`	text,                            NULL                    NULL                    NULL
	`trackidserver`	integer DEFAULT -1,        -1                      -1                      -1
	`trackdificultad`	integer,                   0                       0                       0
	`trackuser`	text,                            NULL                    NULL                    NULL
	`trackuserid`	integer DEFAULT -1,           -1                      -1                      -1
	`trackibpmod`	text,                         NULL                    NULL                    NULL
	`trackibp`	integer DEFAULT -1,              -1                      -1                      -1
	`trackibpref`	text,                         NULL                    NULL                    NULL
	`trackcoef`	real DEFAULT 0.049,              0.0489999987185001      0.0489999987185001      0.0489999987185001
	`trackstrav`	integer DEFAULT -1            -1                      -1                      -1
);
 
-- off. viele berechnete Werte
CREATE TABLE IF NOT EXISTS `segments` (
	`_id`	integer PRIMARY KEY AUTOINCREMENT,     234                  235                  236
	`segname`	text,                            'Segment: 1'         'Segment: 1'         'Segment: 1'
	`segdescr`	text,                            NULL                 NULL                 NULL
	`segfechaini`	integer,                      1553939658000        1553947673000        1553956354000              <- local time: Mar 30 2019 10:54:18 (erster Point)
	`segfechafin`	integer,                      1553946015000        1553951510000        1553958146000              <- local time: Mar 30 2019 12:40:15 (letzter Point)
	`segtimeup`	integer,                         3658000              1576000              646000                        01:00:58
	`segtimedown`	integer,                      2656000              2178000              1106000                       00:44:16
	`segmaxalt`	real,                            79.9845588594821     63.7615191437705     82.2086157647185
	`segminalt`	real,                            31.1901154328947     39.7386294159479     46.5120000001058
	`segavgspeed`	real,                         1.02808292318519     1.15608711275993     1.12471498981485
	`segupalt`	real,                            171.148868560791     76.8933029174805     40.4672355651856
	`segdownalt`	real,                         -175.341564178467    -81.2172470092773    -51.7708396911621
	`segdist`	real,                            6535.52314268827     4435.90625165987     2015.48926174822
	`segtimemov`	integer,                      1317000              394000               446000
	`segtrack`	integer,                         233                  234                  235                           <- Verbindung zu `tracks`
	`segmaxspeed`	real,                         1.75649811645002     2.06946752569989     1.82946879295577
	`segcolor`	integer,                         -16776961            -16776961            -16776961                     <- FF0000FF, FF39B12D -> ARGB
	`segstroke`	real,                            12.0                 12.0                 12.0
	`segfill`	integer,                         NULL                 NULL                 NULL
	`segfillColor`	integer                       NULL                 NULL                 NULL
);



CREATE TABLE IF NOT EXISTS `trackpoints` (
	`_id`	integer PRIMARY KEY AUTOINCREMENT,     97076                97077                97078                97079             
	`trkptlat`	real,                            51.85974665          51.85982212          51.85980519          51.85973716       
	`trkptlon`	real,                            13.95194347          13.95222505          13.95253702          13.95282368       
	`trkptalt`	real,                            62.3661346435547     68.8772583007813     71.1028747558594     67.2340774536133  
	`trkpttime`	integer,                         1553956981000        1553956994000        1553957008000        1553957021000     
	`trkpttrack`	integer,                      NULL                 NULL                 NULL                 NULL              
	`trkptseg`	integer,                         236                  236                  236                  236                     <- Verbindung zu `segments`
	`trkptsen`	blob                             NULL                 NULL                 NULL                 NULL              
);



/* 
   Ein Punkt kann nur zu 1 Segment zugeordnet werden.
   Ein Segment kann nur zu 1 Track zugeordnet werden.
   -> Eine Mehrfachverwendung vorhander Objekte ist nicht möglich. Deshalb ist bei allen Operationen die Erzeugung von Kopien nötig!

   splitten eines Tracks -> splitten eines Segments

   neue Tracks erzeugen
   für jeden neuen Track ein neues Segment erzeugen und deren segtrack auf die entsprechende Track-ID setzen
         alle Segment-Daten auf null 
         außer segcolor und segstroke (nur übernommen) und
         segname neu setzen (spielt aber vermutlich keine große Rolle)
   Punkte der alten Segmente (in der korrekten Reihenfolge) kopieren und trkptseg auf die neuen Segment-ID's setzen
   
   
   concat

   neuen Track erzeugen
   neues Segment erzeugen und segtrack auf die ID des neuen Tracks setzen
   Punkte der alten Segmente (in der korrekten Reihenfolge) kopieren und trkptseg auf die neue Segment-ID setzen
   
*/

-- alle Tracks mit ihren Segmenten und Punkten
select 
	t.trackname,
	t._id as tid,
	s.segname,
	s._id as sid,
	tp._id as tpid,
	tp.trkptlat,
	tp.trkptlon
from tracks t
join segments s on s.segtrack=t._id
join trackpoints tp on tp.trkptseg=s._id
order by t._id, s._id, tp._id


