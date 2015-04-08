namespace LibVdsModbus
{
    using System;

    public class PlcFrame
    {
        private readonly ushort[] registers;

        public PlcFrame(ushort[] data)
        {
            if (data == null || data.Length < 12)
            {
                throw new ArgumentException();
            }

            this.registers = data;
        }

        public bool Verbindungsaufbau_UE
        {
            get
            {
                return Convert.ToBoolean(this.registers[0]);
            }
        }

        public bool Uebertragungsart
        {
            get
            {
                return Convert.ToBoolean(this.registers[1]);
            }
        }

        public ushort Zyklus_Testmeldung
        {
            get
            {
                return this.registers[2];
            }
        }


        public bool Allg_Meldung
        {
            get
            {
                return Convert.ToBoolean(this.registers[3]);
            }
        }

        public bool Stoerung_Batterie
        {
            get
            {
                return Convert.ToBoolean(this.registers[4]);
            }
        }

        public bool Erdschluss
        {
            get
            {
                return Convert.ToBoolean(this.registers[5]);
            }
        }

        public bool Systemstoerung
        {
            get
            {
                return Convert.ToBoolean(this.registers[7]);
            }
        }

        public bool Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB
        {
            get
            {
                return Convert.ToBoolean(this.registers[8]);
            }
        }

        public bool Firmenspez_Meldung_Stellung_LS
        {
            get
            {
                return Convert.ToBoolean(this.registers[6]);
            }
        }

        public double Spannung
        {
            get
            {
                return Convert.ToDouble(this.registers[9]);
            }
        }

        public double Strom
        {
            get
            {
                return Convert.ToDouble(this.registers[10]);
            }
        }

        public double Leistung
        {
            get
            {
                return Convert.ToDouble(this.registers[11]);
            }
        }

        public ushort Live_Signal
        {
            get
            {
                return this.registers[12];
            }
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Verbindungsaufbau UE: {0}, "
                    + "Übertragungsart: {1}, "
                    + "Zyklus Testmeldung: {2}, "
                    + "Allg. Meldung: {3}, "
                    + "Stoerung Batt: {4}, "
                    + "Erdschluss: {5}, "
                    + "Systemstoerung: {6}, "
                    + "FMBLSAUSVNB: {7}, "
                    + "FMBMSLS: {8}, "
                    + "Spannung: {9}, "
                    + "Strom: {10}, "
                    + "Leistung: {11}, "
                    + "LiveSig: {12}.",
                    this.Verbindungsaufbau_UE,
                    this.Uebertragungsart,
                    this.Zyklus_Testmeldung,
                    this.Allg_Meldung,
                    this.Stoerung_Batterie,
                    this.Erdschluss,
                    this.Systemstoerung,
                    this.Firmenspez_Meldung_Befehl_LS_AUS_VOM_NB,
                    this.Firmenspez_Meldung_Stellung_LS,
                    this.Spannung,
                    this.Strom,
                    this.Leistung,
                    this.Live_Signal);
        }
    }
}
