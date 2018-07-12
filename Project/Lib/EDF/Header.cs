using System;

namespace SharpLib.EuropeanDataFormat
{
    public class Field
    {
        public string Name { get; set; }
        public int AsciiLength { get; set; }

        public Field() { }

        public Field(string name, int asciiLength) {
            Name = name;
            AsciiLength = asciiLength;
        }
    }

    public class HeaderItems
    {
        //Fixed length header items

        public static Field Version { get; } = new Field("Version", 8);
        public static Field PatientID { get; } = new Field("PatientID", 80);
        public static Field RecordID { get; private set; } = new Field("RecordID", 80);
        public static Field StartDate { get; private set; } = new Field("StartDate", 8);
        public static Field StartTime { get; private set; } = new Field("StartTime", 8);
        public static Field NumberOfBytesInHeader { get; private set; } = new Field("NumberOfBytesInHeader", 8);
        public static Field Reserved { get; private set; }  = new Field("Reserved", 44);
        public static Field NumberOfDataRecords { get; private set; } = new Field("NumberOfDataRecords", 8);
        public static Field DurationOfDataRecord { get; private set; } = new Field("DurationOfDataRecord", 8);
        public static Field NumberOfSignals { get; private set; } = new Field("NumberOfSignals", 4);

        //Variable length header items

        public static Field Label { get; private set; } = new Field("Labels", 16);
        public static Field TransducerType { get; private set; } = new Field("TransducerType", 80);
        public static Field PhysicalDimension { get; private set; } = new Field("PhysicalDimension", 8);
        public static Field PhysicalMinimum { get; private set; } = new Field("PhysicalMinimum", 8);
        public static Field PhysicalMaximum { get; private set; } = new Field("PhysicalMaximum", 8);
        public static Field DigitalMinimum { get; private set; } = new Field("DigitalMinimum", 8);
        public static Field DigitalMaximum { get; private set; } = new Field("DigitalMaximum", 8);
        public static Field Prefiltering { get; private set; } = new Field("Prefiltering", 80);
        public static Field NumberOfSamplesInDataRecord { get; private set; } = new Field("NumberOfSamplesInDataRecord", 8);
        public static Field SignalsReserved { get; private set; } = new Field("SignalsReserved", 32);
    }

    public abstract class HeaderItem
    {
        public HeaderItem(Field info) {
            Name = info.Name;
            AsciiLength = info.AsciiLength;
        }
        public string Name { get; set; }
        public int AsciiLength { get; set; }
        public abstract string ToAscii();
    }

    public class FixedLengthString : HeaderItem
    {
        public string Value { get; set; }
        public FixedLengthString(Field info) : base(info) { }

        public override string ToAscii() {
            string asciiString = "";
            if (Value != null)
                asciiString = Value.PadRight(AsciiLength, ' ');
            else
                asciiString = asciiString.PadRight(AsciiLength, ' ');

            return asciiString;
        }
    }

    public class FixedLengthInt : HeaderItem
    {
        public int Value { get; set; }
        public FixedLengthInt(Field info) : base(info) { }

        public override string ToAscii()
        {
            string asciiString = "";
            if (Value != null)
                asciiString = Value.ToString().PadRight(AsciiLength, ' ');
            else
                asciiString = asciiString.PadRight(AsciiLength, ' ');

            return asciiString;
        }
    }

    public class FixedLengthDouble : HeaderItem
    {
        public double Value { get; set; }
        public FixedLengthDouble(Field info) : base(info) { }

        public override string ToAscii()
        {
            string asciiString = "";
            if (Value != null)
            {
                asciiString = Value.ToString();
                if (asciiString.Length >= AsciiLength)
                    asciiString = asciiString.Substring(0, AsciiLength);
                else
                    asciiString = Value.ToString().PadRight(AsciiLength, ' ');
            }
                
            else
                asciiString = asciiString.PadRight(AsciiLength, ' ');

            return asciiString;
        }
    }

    public class VariableLengthString : HeaderItem
    {
        public string[] Value { get; set; }
        public VariableLengthString(Field info) : base(info) { }

        public override string ToAscii() {
            string ascii = "";
            foreach (var strVal in Value)
            {
                string temp = strVal.ToString();
                if (strVal.Length > AsciiLength)
                    temp = temp.Substring(0, AsciiLength);
                ascii += temp;
            }
            return ascii;
        }
    }

    public class VariableLengthInt : HeaderItem
    {
        public int[] Value { get; set; }
        public VariableLengthInt(Field info) : base(info) { }

        public override string ToAscii() {
            string ascii = "";
            foreach (var intVal in Value)
            {
                string temp = intVal.ToString();
                if (temp.Length > AsciiLength)
                    temp = temp.Substring(0, AsciiLength);
                ascii += temp;
            }
            return ascii;
        }
    }

    public class VariableLengthDouble : HeaderItem
    {
        public double[] Value { get; set; }
        public VariableLengthDouble(Field info) : base(info) { }

        public override string ToAscii() {
            string ascii = "";
            foreach (var doubleVal in Value)
            {
                string temp = doubleVal.ToString();
                if (temp.Length > AsciiLength)
                    temp = temp.Substring(0, AsciiLength);
                ascii += temp;
            }
            return ascii;
        }
    }

    public class Header
    {
        public FixedLengthString Version { get; private set; } = new FixedLengthString(HeaderItems.Version);
        public FixedLengthString PatientID { get; private set; } = new FixedLengthString(HeaderItems.PatientID);
        public FixedLengthString RecordID { get; private set; } = new FixedLengthString(HeaderItems.RecordID);
        public FixedLengthString StartDate { get; private set; } = new FixedLengthString(HeaderItems.StartDate);
        public FixedLengthString StartTime { get; private set; } = new FixedLengthString(HeaderItems.StartTime);
        public FixedLengthInt NumberOfBytesInHeader { get; private set; } = new FixedLengthInt(HeaderItems.NumberOfBytesInHeader);
        public FixedLengthString Reserved { get; private set; } = new FixedLengthString(HeaderItems.Reserved);
        public FixedLengthInt NumberOfDataRecords { get; private set; } = new FixedLengthInt(HeaderItems.NumberOfDataRecords);
        public FixedLengthInt DurationOfDataRecord { get; private set; } = new FixedLengthInt(HeaderItems.DurationOfDataRecord);
        public FixedLengthInt NumberOfSignals { get; private set; } = new FixedLengthInt(HeaderItems.NumberOfSignals);

        public VariableLengthString Labels { get; private set; } = new VariableLengthString(HeaderItems.Label);
        public VariableLengthString TransducerType { get; private set; } = new VariableLengthString(HeaderItems.TransducerType);
        public VariableLengthString PhysicalDimension { get; private set; } = new VariableLengthString(HeaderItems.PhysicalDimension);
        public VariableLengthDouble PhysicalMinimum { get; private set; } = new VariableLengthDouble(HeaderItems.PhysicalMinimum);
        public VariableLengthDouble PhysicalMaximum { get; private set; } = new VariableLengthDouble(HeaderItems.PhysicalMaximum);
        public VariableLengthInt DigitalMinimum { get; private set; } = new VariableLengthInt(HeaderItems.DigitalMinimum);
        public VariableLengthInt DigitalMaximum { get; private set; } = new VariableLengthInt(HeaderItems.DigitalMaximum);
        public VariableLengthString Prefiltering { get; private set; } = new VariableLengthString(HeaderItems.Prefiltering);
        public VariableLengthInt SampleCountPerRecord { get; private set; } = new VariableLengthInt(HeaderItems.NumberOfSamplesInDataRecord);
        public VariableLengthString SignalsReserved { get; private set; } = new VariableLengthString(HeaderItems.SignalsReserved);

        public Header() { }

        public override string ToString()
        {
            string strOutput = "";

            strOutput += "\n---------- EDF File Header ---------\n";
            strOutput += "8b\tVersion [" + Version.Value + "]\n";
            strOutput += "80b\tPatient ID [" + PatientID.Value + "]\n";
            strOutput += "80b\tRecording ID [" + RecordID.Value + "]\n";
            strOutput += "8b\tStart Date [" + StartDate.Value + "]\n";
            strOutput += "8b\tStart Time [" + StartTime.Value + "\n]";
            strOutput += "8b\tNumber of bytes in header [" + NumberOfBytesInHeader.Value + "]\n";
            strOutput += "44b\tReserved [" + Reserved.Value + "]\n";
            strOutput += "8b\tNumber of data records [" + NumberOfDataRecords.Value + "]\n";
            strOutput += "8b\tDuration of data record [" + DurationOfDataRecord.Value + "]\n";
            strOutput += "4b\tNumber of signals [" + NumberOfSignals.Value + "]\n";

            for (int i=0;i<NumberOfSignals.Value;i++)
            {
                strOutput += "---------Signal Header---------\n";
                strOutput += "\tLabel [" + Labels.Value[i] + "]\n";
                strOutput += "\tTransducer type [" + TransducerType.Value[i] + "]\n";
                strOutput += "\tPhysical dimension [" + PhysicalDimension.Value[i] + "]\n";
                strOutput += "\tPhysical minimum [" + PhysicalMinimum.Value[i] + "]\n";
                strOutput += "\tPhysical maximum [" + PhysicalMaximum.Value[i] + "]\n";
                strOutput += "\tDigital minimum [" + DigitalMinimum.Value[i] + "]\n";
                strOutput += "\tDigital maximum [" + DigitalMaximum.Value[i] + "]\n";
                strOutput += "\tPrefiltering [" + Prefiltering.Value[i] + "]\n";
                strOutput += "\tNumber of samples in data record [" + SampleCountPerRecord.Value[i] + "]\n";
                strOutput += "\tSignals reserved [" + SignalsReserved.Value[i] + "]\n";
            }


            strOutput += "\n-----------------------------------\n";

            //Console.WriteLine();

            return strOutput;
        }
    }
}
