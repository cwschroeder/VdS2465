namespace LibVds.Proto
{
    public enum VdSType
    {
        Prioritaet = 0x01,
        Meldung_Zustandsaenderung__Steuerung_mit_Quittungsanforderung = 0x02, // Muss
        Quittungsruecksendung = 0x03, // Muss
        Meldung_Zustandsaenderung__Steuerung_ohne_Quittungsanforderung = 0x04,

        // herstellerspezifische Daten

        Abfrage = 0x0F, // Muss
        Fehler = 0x11, // Muss
        Status = 0x20, // Muss
        Blockstatus = 0x24,
        Sammelstatus = 0x25,
        Mess_Zaehl_Stellwerte = 0x30,
        GpsKoordinaten = 0x35,
        Testmeldung = 0x40, // Muss
        Testmeldungsquittung = 0x41, // Muss
        Sicherheitscode =0x42,
        Information_zur_Meldungsweiterleitung=0x44,
        Durchschalten_des_Verbindungswegs = 0x48,
        Datum_Uhrzeit = 0x50,
        HerstellerId = 0x51,
        Kommunkationsadresse_S2 = 0x52,
        Kommunkationsadresse_S3 = 0x53,
        Ascii_Zeichenfolge = 0x54,
        Aktuell_unterstuetzte_Satztypen = 0x55, // Muss
        Identifikations_Nummer = 0x56, // Muss
        Netzstatus = 0x60,
        Transportdienstkennung = 0x61,
        Quelladresse_aus_dem_Netz = 0x62,
        Datenblock_transparent = 0x70,
        Letzter_Datenblock_transparent = 0x71,
        Datenblock_gueltig = 0x72,
        Telegrammzaehler = 0x73,
        Container = 0x74,

        // herstellerspez. Daten

        Verbindung_wird_nicht_mehr_benoetigt = 0xFF // Muss
    }
}