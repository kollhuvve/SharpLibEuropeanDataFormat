using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace SharpLib.EuropeanDataFormat.EDF
{
    class Reader : BinaryReader
    {
        public Reader(FileStream fileStream) : base(fileStream) { }
        public Reader(Stream stream) : base(stream) { }
        public Reader(byte[] edfBytes) : base(new MemoryStream(edfBytes)) { }

        public Header ReadHeader()
        {
            var header = new Header();

            this.BaseStream.Seek(0, SeekOrigin.Begin);

            // Fixed size header
            header.Version.Value = ReadAscii(HeaderItems.Version);
            header.PatientID.Value = ReadAscii(HeaderItems.PatientID);
            header.RecordID.Value = ReadAscii(HeaderItems.RecordID);
            header.RecordingStartDate.Value = ReadAscii(HeaderItems.RecordingStartDate);
            header.RecordingStartTime.Value = ReadAscii(HeaderItems.RecordingStartTime);
            header.SizeInBytes.Value = ReadUInt16(HeaderItems.SizeInBytes);
            header.Reserved.Value = ReadAscii(HeaderItems.Reserved);
            header.RecordCount.Value = ReadUInt16(HeaderItems.NumberOfDataRecords);
            header.RecordDurationInSeconds.Value = ReadDouble(HeaderItems.RecordDurationInSeconds);
            header.SignalCount.Value = ReadInt16(HeaderItems.SignalCount);

            // Variable size header
            // Contains signal headers
            var signalCount = header.SignalCount.Value;
            header.Signals.Labels.Value = ReadMultipleAscii(HeaderItems.Label, signalCount);
            header.Signals.TransducerTypes.Value = ReadMultipleAscii(HeaderItems.TransducerType, signalCount);
            header.Signals.PhysicalDimensions.Value = ReadMultipleAscii(HeaderItems.PhysicalDimension, signalCount);
            header.Signals.PhysicalMinimums.Value = ReadMultipleDouble(HeaderItems.PhysicalMinimum, signalCount);
            header.Signals.PhysicalMaximums.Value = ReadMultipleDouble(HeaderItems.PhysicalMaximum, signalCount);
            header.Signals.DigitalMinimums.Value = ReadMultipleInt(HeaderItems.DigitalMinimum, signalCount);
            header.Signals.DigitalMaximums.Value = ReadMultipleInt(HeaderItems.DigitalMaximum, signalCount);
            header.Signals.Prefilterings.Value = ReadMultipleAscii(HeaderItems.Prefiltering, signalCount);
            header.Signals.SampleCountPerRecords.Value = ReadMultipleInt(HeaderItems.NumberOfSamplesInDataRecord, signalCount);
            header.Signals.Reserveds.Value = ReadMultipleAscii(HeaderItems.SignalsReserved, signalCount);

            header.ParseRecordingStartTime();

            return header;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public Signal[] AllocateSignals(Header header)
        {
            var signals = new Signal[header.SignalCount.Value];

            for (var i = 0; i < signals.Length; i++)
            {
                signals[i] = new Signal();
                // Just copy data from the header, ugly architecture really...
                signals[i].Index = i;
                signals[i].Label.Value = header.Signals.Labels.Value[i];
                signals[i].TransducerType.Value = header.Signals.TransducerTypes.Value[i];
                signals[i].PhysicalDimension.Value = header.Signals.PhysicalDimensions.Value[i];
                signals[i].PhysicalMinimum.Value = header.Signals.PhysicalMinimums.Value[i];
                signals[i].PhysicalMaximum.Value = header.Signals.PhysicalMaximums.Value[i];
                signals[i].DigitalMinimum.Value = header.Signals.DigitalMinimums.Value[i];
                signals[i].DigitalMaximum.Value = header.Signals.DigitalMaximums.Value[i];
                signals[i].Prefiltering.Value = header.Signals.Prefilterings.Value[i];
                signals[i].Reserved.Value = header.Signals.Reserveds.Value[i];
                signals[i].SampleCountPerRecord.Value = header.Signals.SampleCountPerRecords.Value[i];
            }

            return signals;
        }

        /// <summary>
        /// Read the requested signal for our file
        /// </summary>
        /// <param name="header"></param>
        /// <param name="signal"></param>
        public void ReadSignal(Header header, Signal signal)
        {
            // Make sure we start just after our header
            this.BaseStream.Seek(header.SizeInBytes.Value, SeekOrigin.Begin);

            signal.Samples.Clear();
            // Compute capacity thus pre-allocating memory to avoid resizing
            signal.Samples.Capacity = header.RecordCount.Value * signal.SampleCountPerRecord.Value;
            var capacity = signal.Samples.Capacity;
            // For each record
            for (var j = 0; j < header.RecordCount.Value; j++)
            {
                // For each signal
                for (var i = 0; i < header.SignalCount.Value; i++)
                {
                    // Read that signal samples
                    if (i == signal.Index)
                    {
                        ReadNextSignalSamples(signal.Samples, signal.SampleCountPerRecord.Value);
                    }
                    else
                    {
                        // Just skip it
                        SkipSignalSamples(header.Signals.SampleCountPerRecords.Value[i]);
                    }
                }
            }

            if (capacity != signal.Samples.Capacity)
            {
                // We should never get there
                Debug.WriteLine("ERROR: signal array was resized");
            }

        }

        /// <summary>
        /// Read all signal sample value from our file.
        /// </summary>
        /// <returns></returns>
        public Signal[] ReadSignals(Header aHeader)
        {            
            var signals = AllocateSignals(aHeader);
            // For each record
            for (var j = 0; j < aHeader.RecordCount.Value; j++)
            {
                // For each signal
                for (var i = 0; i < signals.Length; i++)
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
        /// <param name="samples"></param>
        /// <param name="sampleCount"></param>
        private void ReadNextSignalSamples(ICollection<short> samples, int sampleCount)
        {
            // Single file read operation per record
            var intBytes = this.ReadBytes(sizeof(short) * sampleCount);
            for (var i = 0; i < sampleCount; i++)
            {
                // Fetch our sample short from our record buffer
                var intVal = BitConverter.ToInt16(intBytes, i* sizeof(short));
                // TODO: use a static array for better performance? I guess it's not needed since we prealloc using Capacity.
                samples.Add(intVal);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aSampleCount"></param>
        private void SkipSignalSamples(int aSampleCount)
        {
            BaseStream.Seek(aSampleCount * sizeof(short), SeekOrigin.Current);
        }



        /// <summary>
        /// TODO: Is this still being used?
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="numberOfSamples"></param>
        /// <returns></returns>
        private short[] ReadSignalSamples(int startPosition, int numberOfSamples)
        {
            var samples = new List<short>();
            var countBytesRead = 0;

            this.BaseStream.Seek(startPosition, SeekOrigin.Begin);

            while (countBytesRead < numberOfSamples * 2) //2 bytes per integer
            {
                var intBytes = this.ReadBytes(2);
                var intVal = BitConverter.ToInt16(intBytes, 0);
                samples.Add(intVal);
                countBytesRead += intBytes.Length;
            }

            return samples.ToArray();
        }

        private double ReadDouble(Field itemInfo)
        {
            var strInt = ReadAscii(itemInfo).Trim();
            double doubleResult = -1;
            try
            {
                doubleResult = Convert.ToDouble(strInt, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new Exception($"Field {itemInfo.Name} Error, could not convert string to double. " + ex.Message);
            }
            return doubleResult;
        }

        private UInt16 ReadUInt16(Field itemInfo)
        {
            var strInt = ReadAscii(itemInfo).Trim();
            UInt16 intResult = 0;
            try
            {
                intResult = Convert.ToUInt16(strInt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Field {itemInfo.Name} Error, could not convert string to integer. " + ex.Message);
            }
            return intResult;
        }

        private Int16 ReadInt16(Field itemInfo)
        {
            var strInt = ReadAscii(itemInfo).Trim();
            Int16 intResult = -1;
            try
            {
                intResult = Convert.ToInt16(strInt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Field {itemInfo.Name} Error, could not convert string to integer. " + ex.Message);
            }
            return intResult;
        }

        private string ReadAscii(Field itemInfo)
        {
            var bytes = this.ReadBytes(itemInfo.AsciiLength);
            return AsciiString(bytes).Trim();
        }

        private string[] ReadMultipleAscii(Field itemInfo, int numberOfParts)
        {
            var parts = new List<string>();

            for (var i = 0; i < numberOfParts; i++) {
                var bytes = this.ReadBytes(itemInfo.AsciiLength);
                parts.Add(AsciiString(bytes).Trim());
            }
            
            return parts.ToArray();
        }

        private int[] ReadMultipleInt(Field itemInfo, int numberOfParts)
        {
            var parts = new List<int>();

            for (var i = 0; i < numberOfParts; i++)
            {
                var bytes = this.ReadBytes(itemInfo.AsciiLength);
                var ascii = AsciiString(bytes);
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

            for (var i = 0; i < numberOfParts; i++)
            {
                var bytes = this.ReadBytes(itemInfo.AsciiLength);
                var ascii = AsciiString(bytes);
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
