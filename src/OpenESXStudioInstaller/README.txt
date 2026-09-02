OpenESX Studio 2.0 RC5 – Windows-x64-Programm
================================================

Dieses Paket enthält die installierbare und die portable Ausgabe von OpenESX
Studio. Beide enthalten die vollständige Offline-Oberfläche 2.0 RC5 mit Sample-
Verwaltung, Pattern Studio, Effekten, Motion-Sequences, Songs und Global/MIDI.

Installation
------------

1. „OpenESX-Studio-2.0-RC5-Setup.exe“ doppelt anklicken.
2. Optional die Desktop-Verknüpfung aktiviert lassen.
3. „Installieren“ wählen.
4. OpenESX Studio anschließend über Desktop oder Startmenü öffnen.

Das Programm wird nur für das aktuelle Benutzerkonto installiert. Es benötigt
keine Administratorrechte. Über „Installierte Apps“ in Windows oder die
Startmenü-Verknüpfung „OpenESX Studio deinstallieren“ kann es wieder entfernt
werden. Eigene ESX- und WAV-Dateien werden bei der Deinstallation nicht gelöscht.

Portable Nutzung
----------------

„OpenESX-Studio-2.0-RC5-Portable.exe“ kann ohne Installation gestartet werden.
Die EXE enthält die Offline-Oberfläche vollständig und legt beim Start lediglich
eine geprüfte Laufzeitkopie unter %LOCALAPPDATA%\OpenESX Studio\Runtime an.

Programmfenster und Voraussetzungen
------------------------------------

- Windows 10 oder Windows 11, 64 Bit
- Microsoft Edge (Bestandteil aktueller Windows-Versionen)
- Keine Internetverbindung erforderlich
- Keine Administratorrechte erforderlich

Die EXE startet OpenESX Studio in einem eigenen Microsoft-Edge-App-Fenster ohne
normalen Browser-Tab. Falls Edge nicht gefunden wird, öffnet sie die Oberfläche
ersatzweise im eingestellten Standardbrowser.

Karte und ESX-Bänke
-------------------

- Der neue Bereich „Karte & Bänke“ erkennt eingesteckte Wechseldatenträger und
  zeigt Größe, freien Platz und Dateisystem.
- Vorhandene ESX-Bänke im Hauptverzeichnis lassen sich direkt im Studio öffnen.
- Die aktuelle Arbeitskopie kann unter einem neuen Namen direkt auf die Karte
  gespeichert werden. Vor dem Überschreiben wird ausdrücklich nachgefragt.
- OpenESX beachtet die von Korg dokumentierten Grenzen der ESX-SD: SD bis 2 GB,
  SDHC bis 32 GB und höchstens 256 Dateien. Größere Karten werden als nicht
  offiziell unterstützt gekennzeichnet.
- Eine größere Karte vergrößert nicht den Sample-RAM einer einzelnen ESX-Bank.
  Pro Bank bleiben 384 Sample-Slots und ungefähr 24 MB Sampledaten; zusätzlicher
  Kartenplatz wird für mehrere getrennte ESX-Bänke genutzt.
- Karten sollten für größtmögliche Kompatibilität direkt an der Korg formatiert
  werden.

Sicherheitsmodell
-----------------

- ESX- und WAV-Dateien werden nicht ins Internet übertragen.
- Das Original der geöffneten ESX-Datei bleibt unverändert.
- Änderungen werden in einer getrennten Arbeitskopie gesammelt.
- Erst „Bearbeitete ESX speichern“ erzeugt eine neue ESX-Datei.
- Backups und bitgenaue Kopien beziehen sich auf das Original.
- Pattern-Schreibvorgänge sind auf das ausgewählte Pattern begrenzt.
- Die EXE und der Installer fordern keine erhöhten Rechte an.

Windows-Sicherheitshinweis
--------------------------

Diese privat erstellte Testversion besitzt kein kommerzielles Code-Signing-
Zertifikat. Windows SmartScreen oder Smart App Control kann sie deshalb als
unbekannte Anwendung einstufen oder blockieren. Das ist kein Funktionsnachteil
des Editors, sondern betrifft ausschließlich die Vertrauensbewertung der EXE.

Eine dauerhaft ohne Warnung verteilbare EXE benötigt eine digitale Signatur mit
einem vertrauenswürdigen Windows-Code-Signing-Zertifikat. Die hier enthaltenen
Dateien wurden lokal aus dem geprüften Quellstand erzeugt und mit SHA-256 erfasst.

Mit einer echten ESX-1-Datei geprüft
-----------------------------------

- Die private Testdatei und ihre Samples werden nicht veröffentlicht.
- 253 Mono-Samples, 0 Stereo-Samples, 256 Pattern und 64 Songs
- 0 strukturelle Warnungen
- Bitgenauer unveränderter ESX-Rundlauf
- Takt 1–8 und Step-Ausrichtung 1, 5, 9 und 13 geprüft
- Global-Daten und benachbarte Pattern bleiben bei Pattern-Änderungen unverändert
- Sample-Vorschau im Pattern Studio geprüft
- Live-Vorschau-Mixer: 14 Sample-Parts einzeln sowie gemeinsam an/aus schaltbar,
  ohne die ESX-Daten zu verändern
- Portable EXE und eingebettete Offline-Oberfläche automatisch geprüft
- Kartenerkennung sowie Speichern, Öffnen und erneutes Speichern einer Bank unter
  Windows praktisch geprüft
- Das Laden einer so erzeugten Bank auf echter ESX-SD-Hardware ist noch ein
  ausdrücklich gekennzeichneter Community-Betatest

Version: 2.0 RC5
Build: Windows x64, installierbar und portabel
