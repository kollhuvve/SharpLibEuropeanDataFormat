﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SharpLib.EuropeanDataFormat.EDF;

namespace SharpLib.EuropeanDataFormat.EDF
{
    class Writer : BinaryWriter
    {
        public Writer(FileStream fs) : base(fs) { }

        public void WriteEDF(EDFFile edf, string edfFilePath)
        {
            edf.Header.SizeInBytes.Value = CalcNumOfBytesInHeader(edf);

            //----------------- Fixed length header items -----------------
            WriteItem(edf.Header.Version);
            WriteItem(edf.Header.PatientID);
            WriteItem(edf.Header.RecordID);
            WriteItem(edf.Header.RecordingStartDate);
            WriteItem(edf.Header.RecordingStartTime);
            WriteItem(edf.Header.SizeInBytes);
            WriteItem(edf.Header.Reserved);
            WriteItem(edf.Header.RecordCount);
            WriteItem(edf.Header.RecordDurationInSeconds);
            WriteItem(edf.Header.SignalCount);

            //----------------- Variable length header items -----------------
            WriteItem(edf.Signals.Select(s => s.Label));
            WriteItem(edf.Signals.Select(s => s.TransducerType));
            WriteItem(edf.Signals.Select(s => s.PhysicalDimension));
            WriteItem(edf.Signals.Select(s => s.PhysicalMinimum));
            WriteItem(edf.Signals.Select(s => s.PhysicalMaximum));
            WriteItem(edf.Signals.Select(s => s.DigitalMinimum));
            WriteItem(edf.Signals.Select(s => s.DigitalMaximum));
            WriteItem(edf.Signals.Select(s => s.Prefiltering));
            WriteItem(edf.Signals.Select(s => s.SampleCountPerRecord));
            WriteItem(edf.Signals.Select(s => s.Reserved));

            Console.WriteLine("Writer position after header: " + BaseStream.Position);
            Console.WriteLine("Writing signals.");
            //TODO: Missing for each record loop, see fixed reading code
            foreach (var sig in edf.Signals) WriteSignal(sig);

            Close();
            Console.WriteLine("File size: " + System.IO.File.ReadAllBytes(edfFilePath).Length);
        }

        private int CalcNumOfBytesInHeader(EDFFile edf)
        {
            int totalFixedLength = 256;
            int ns = edf.Signals.Length;
            int totalVariableLength = ns * 16 + (ns * 80) * 2 + (ns * 8) * 6 + (ns * 32);
            return totalFixedLength + totalVariableLength;
        }

        public void WriteItem(HeaderItem headerItem)
        {
            string strItem = headerItem.ToAscii();
            if (strItem == null) strItem = "";
            byte[] itemBytes = AsciiToBytes(strItem);
            this.Write(itemBytes);
            Console.WriteLine(headerItem.Name + " [" + strItem + "] \n\n-- ** BYTES LENGTH: " + itemBytes.Length 
                + "> Position after write item: " + this.BaseStream.Position + "\n");
        }

        public void WriteItem(IEnumerable<HeaderItem> headerItems)
        {
            string joinedItems = StrJoin(headerItems);
            if (joinedItems == null) joinedItems = "";
            byte[] itemBytes = AsciiToBytes(joinedItems);
            this.Write(itemBytes);
            Console.WriteLine("[" + joinedItems + "] \n\n-- ** BYTES LENGTH: " + itemBytes.Length 
                + " Position after write item: " + this.BaseStream.Position + "\n");
        }

        private string StrJoin(IEnumerable<HeaderItem> list)
        {
            string joinedString = "";

            foreach (var item in list)
            {
                joinedString += item.ToAscii();
            }

            return joinedString;
        }

        private static byte[] AsciiToBytes(string strItem)
        {
            return Encoding.ASCII.GetBytes(strItem);
        }

        private static byte[] AsciiToIntBytes(string strItem, int length)
        {
            string strInt = "";
            string str = strItem.Substring(0, length);
            double val = Convert.ToDouble(str);
            strInt += val.ToString("0").PadRight(length, ' ');
            return Encoding.ASCII.GetBytes(strInt);
        }

        public void WriteSignal(Signal signal)
        {
            Console.WriteLine("Write position before signal: " + this.BaseStream.Position);
            for (int i = 0; i < signal.SampleCountPerRecord.Value; i++)
            {
                this.Write(BitConverter.GetBytes(signal.Samples[i]));
            }
            Console.WriteLine("Write position after signal: " + this.BaseStream.Position);
        }
    }
}
