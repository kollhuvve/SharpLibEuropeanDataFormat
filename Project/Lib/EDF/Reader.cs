using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpLib.EuropeanDataFormat
{
    class Reader : BinaryReader
    {
        public Reader(FileStream fs) : base(fs) { }
        public Reader(byte[] edfBytes) : base(new MemoryStream(edfBytes)) { }

        public Header ReadHeader()
        {
            Header h = new Header();

            this.BaseStream.Seek(0, SeekOrigin.Begin);

            //------ Fixed length header part --------
            h.Version.Value                 = ReadAscii(HeaderItems.Version);
            h.PatientID.Value               = ReadAscii(HeaderItems.PatientID);
            h.RecordID.Value                = ReadAscii(HeaderItems.RecordID);
            h.StartDate.Value               = ReadAscii(HeaderItems.StartDate);
            h.StartTime.Value               = ReadAscii(HeaderItems.StartTime);
            h.NumberOfBytesInHeader.Value   = ReadInt16(HeaderItems.NumberOfBytesInHeader);
            h.Reserved.Value                = ReadAscii(HeaderItems.Reserved);
            h.NumberOfDataRecords.Value     = ReadInt16(HeaderItems.NumberOfDataRecords);
            h.DurationOfDataRecord.Value    = ReadInt16(HeaderItems.DurationOfDataRecord);
            h.NumberOfSignals.Value         = ReadInt16(HeaderItems.NumberOfSignals);

            //------ Variable length header part --------
            int ns = h.NumberOfSignals.Value;
            h.Labels.Value                      = ReadMultipleAscii(HeaderItems.Label, ns);
            h.TransducerType.Value              = ReadMultipleAscii(HeaderItems.TransducerType, ns);
            h.PhysicalDimension.Value           = ReadMultipleAscii(HeaderItems.PhysicalDimension, ns);
            h.PhysicalMinimum.Value             = ReadMultipleDouble(HeaderItems.PhysicalMinimum, ns);
            h.PhysicalMaximum.Value             = ReadMultipleDouble(HeaderItems.PhysicalMaximum, ns);
            h.DigitalMinimum.Value              = ReadMultipleInt(HeaderItems.DigitalMinimum, ns);
            h.DigitalMaximum.Value              = ReadMultipleInt(HeaderItems.DigitalMaximum, ns);
            h.Prefiltering.Value                = ReadMultipleAscii(HeaderItems.Prefiltering, ns);
            h.SampleCountPerRecord.Value = ReadMultipleInt(HeaderItems.NumberOfSamplesInDataRecord, ns);
            h.SignalsReserved.Value             = ReadMultipleAscii(HeaderItems.SignalsReserved, ns);

            return h;
        }

        public Signal[] ReadSignals()
        {
            Header header = ReadHeader();
            Signal[] signals = new Signal[header.NumberOfSignals.Value];

            for (int i = 0; i < signals.Length; i++)
            {
                signals[i] = new Signal();
                // Just copy data from the header, ugly architecture really...
                signals[i].Label.Value = header.Labels.Value[i];
                signals[i].TransducerType.Value = header.TransducerType.Value[i];
                signals[i].PhysicalDimension.Value = header.PhysicalDimension.Value[i];
                signals[i].PhysicalMinimum.Value = header.PhysicalMinimum.Value[i];
                signals[i].PhysicalMaximum.Value = header.PhysicalMaximum.Value[i];
                signals[i].DigitalMinimum.Value = header.DigitalMinimum.Value[i];
                signals[i].DigitalMaximum.Value = header.DigitalMaximum.Value[i];
                signals[i].Prefiltering.Value = header.Prefiltering.Value[i];
                signals[i].Reserved.Value = header.SignalsReserved.Value[i];
                signals[i].SampleCountPerRecord.Value = header.SampleCountPerRecord.Value[i];
            }

            //Read the signal sample values
            //int readPosition = header.NumberOfBytesInHeader.Value;

            // For each record
            for (int j = 0; j < header.NumberOfDataRecords.Value; j++)
            {
                // For each signal
                for (int i = 0; i < signals.Length; i++)
                {
                    // Read that signal samples
                    ReadNextSignalSamples(signals[i].Samples, signals[i].SampleCountPerRecord.Value);                    
                }
            }

            return signals;
        }
        
        /// <summary>
        /// Read n next samples
        /// </summary>
        /// <param name="aSamples"></param>
        /// <param name="aSampleCount"></param>
        private void ReadNextSignalSamples(ICollection<short> aSamples, int aSampleCount)
        {
            for (int i=0;i<aSampleCount;i++)
            {
                //TODO: simplify that?
                byte[] intBytes = this.ReadBytes(sizeof(short));
                short intVal = BitConverter.ToInt16(intBytes, 0);
                aSamples.Add(intVal);
            }

        }



        private short[] ReadSignalSamples(int startPosition, int numberOfSamples)
        {
            var samples = new List<short>();
            int countBytesRead = 0;

            this.BaseStream.Seek(startPosition, SeekOrigin.Begin);

            while (countBytesRead < numberOfSamples * 2) //2 bytes per integer
            {
                byte[] intBytes = this.ReadBytes(2);
                short intVal = BitConverter.ToInt16(intBytes, 0);
                samples.Add(intVal);
                countBytesRead += intBytes.Length;
            }

            return samples.ToArray();
        }

        private Int16 ReadInt16(Field itemInfo)
        {
            string strInt = ReadAscii(itemInfo).Trim();
            Int16 intResult = -1;
            try { intResult = Convert.ToInt16(strInt); }
            catch (Exception ex) { Console.WriteLine("Error, could not convert string to integer. " + ex.Message); }
            return intResult;
        }

        private string ReadAscii(Field itemInfo)
        {
            byte[] bytes = this.ReadBytes(itemInfo.AsciiLength);
            return AsciiString(bytes);
        }

        private string[] ReadMultipleAscii(Field itemInfo, int numberOfParts)
        {
            var parts = new List<string>();

            for (int i = 0; i < numberOfParts; i++) {
                byte[] bytes = this.ReadBytes(itemInfo.AsciiLength);
                parts.Add(AsciiString(bytes));
            }
            
            return parts.ToArray();
        }

        private int[] ReadMultipleInt(Field itemInfo, int numberOfParts)
        {
            var parts = new List<int>();

            for (int i = 0; i < numberOfParts; i++)
            {
                byte[] bytes = this.ReadBytes(itemInfo.AsciiLength);
                string ascii = AsciiString(bytes);
                parts.Add(Convert.ToInt32(ascii));
            }

            return parts.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="numberOfParts"></param>
        /// <returns></returns>
        private double[] ReadMultipleDouble(Field itemInfo, int numberOfParts)
        {
            var parts = new List<double>();

            for (int i = 0; i < numberOfParts; i++)
            {
                byte[] bytes = this.ReadBytes(itemInfo.AsciiLength);
                string ascii = AsciiString(bytes);
                // Use invariant culure as we have a '.' as decimal separator
                parts.Add(double.Parse(ascii, CultureInfo.InvariantCulture));
            }

            return parts.ToArray();
        }

        private static string AsciiString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
