using System;

namespace FMSC.GeoSpatial.NMEA.Sentences.Base
{
    [Serializable]
    public abstract class NmeaSentence
    {
        public TalkerID TalkerID { get; protected set; }
        public abstract SentenceID SentenceID { get; }
        public String RawNmea { get; protected set; }
        public virtual bool IsValid { get; protected set; }

        public virtual bool IsMultiString { get; } = false;


        public NmeaSentence() { }

        public NmeaSentence(String nmea)
        {
            this.TalkerID = NmeaIDTools.ParseTalkerID(nmea);

            if (nmea == null)
                throw new ArgumentNullException(nameof(nmea));

            if (IsMultiString && nmea.Contains("\n"))
            {
                String[] strs = nmea.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (String str in strs)
                {
                    Parse(str);
                }
            }
            else
            {
                this.RawNmea = nmea;
                Parse(nmea);
            }
        }


        public virtual bool Parse(String nmea)
        {
            return IsValid = IsNmeaValid(SentenceID, null, nmea);
        }


        public static bool validateChecksum(String nmea)
        {
            if (nmea.Length > 10 && nmea.StartsWith("$") && nmea.Contains("*"))
            {
                String calcString = nmea.Substring(1);
                int ast = calcString.IndexOf("*");
                String checkSumStr = calcString.Substring(ast + 1, 2);
                calcString = calcString.Substring(0, ast);

                int checksum = 0;

                for (int i = 0; i < calcString.Length; i++)
                {
                    checksum ^= (byte)calcString[i];
                }

                int ics = int.Parse(checkSumStr, System.Globalization.NumberStyles.HexNumber);

                return ics == checksum;

                //String hex = checksum.ToString("X2");
                //if (hex.Length < 2)
                //{
                //    hex = "0" + hex;
                //}
                //return hex.Equals(checkSumStr, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public static bool IsNmeaValid(SentenceID sentenceID, TalkerID? talkerID, String nmea)
        {
            bool valid = false;

            if (validateChecksum(nmea))
            {
                valid = sentenceID == NmeaIDTools.ParseSentenceID(nmea);

                if (talkerID != null)
                {
                    valid &= talkerID == NmeaIDTools.ParseTalkerID(nmea);
                }
            }

            return valid;
        }
    }
}
