using System;
using System.IO;
using System.Linq;
using System.Text;

/*
 * This is the https://github.com/Slion/SharpLibEuropeanDataFormat
 *
 * There has been modifications to the project.
 */
namespace SharpLib.EuropeanDataFormat.EDF
{
    public class EDFFile : IDisposable
    {
        public Header Header { get; set; }
        public Signal[] Signals { get; set; }

        private Reader _reader;

        public EDFFile() { }
        public EDFFile(string edfFilePath)
        {
            ReadAll(edfFilePath);
        }
        public EDFFile(Stream stream)
        {
            ReadAll(stream);
        }

        public EDFFile(byte[] edfBytes)
        {
            ReadAll(edfBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_reader!=null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        public string PrintSummary()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"First Record Time : {Header.FirstRecordTime.ToUTCDateTimeString()}");
            stringBuilder.AppendLine($"Patient ID : {Header.PatientID.ToAscii().Trim()}");
            stringBuilder.AppendLine($"Start Date : {Header.RecordingStartDate.ToAscii().Trim()}");
            stringBuilder.AppendLine($"Start Time : {Header.RecordingStartTime.ToAscii().Trim()}");
            stringBuilder.AppendLine($"Record Count : {Header.RecordCount.ToAscii().Trim()}");
            stringBuilder.AppendLine($"Record Duration : {Header.RecordDurationInSeconds.ToAscii().Trim()}");
            stringBuilder.AppendLine($"Signal Count : {Signals.Length}");

            var counter = 0;
            foreach (var edfFileSignal in Signals)
            {
                stringBuilder.AppendLine($"-------------------------------");
                stringBuilder.AppendLine($"Signal #{counter}");
                stringBuilder.AppendLine($"Label : {edfFileSignal.Label.ToAscii().Trim()}");
                stringBuilder.AppendLine($"Sample Count : {edfFileSignal.Samples.Count}");
                stringBuilder.AppendLine($"Samples Per Record : {edfFileSignal.SampleCountPerRecord.Value}");
                stringBuilder.AppendLine($"Digital Maximum : {edfFileSignal.DigitalMaximum.Value}");
                stringBuilder.AppendLine($"Digital Minimum : {edfFileSignal.DigitalMinimum.Value}");
                stringBuilder.AppendLine($"Physical Dimension : {edfFileSignal.PhysicalDimension.Value}");
                stringBuilder.AppendLine($"Physical Maximum : {edfFileSignal.PhysicalMaximum.Value}");
                stringBuilder.AppendLine($"Physical Minimum : {edfFileSignal.PhysicalMinimum.Value}");
                stringBuilder.AppendLine($"Transducer Type : {edfFileSignal.TransducerType.Value}");
                counter++;
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edfBase64"></param>
        public void ReadBase64(string edfBase64)
        {
            byte[] edfBytes = System.Convert.FromBase64String(edfBase64);
            ReadAll(edfBytes);
        }

        /// <summary>
        /// Open the given EDF file, read its header and allocate corresponding Signal objects.
        /// </summary>
        /// <param name="edfFilePath"></param>
        public void Open(string edfFilePath)
        {
            // Open file
            _reader = new Reader(System.IO.File.Open(edfFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
            // Read headers
            Header = _reader.ReadHeader();
            // Allocate signals
            Signals = _reader.AllocateSignals(Header);
        }

        /// <summary>
        /// Read the signal at the given index.
        /// </summary>
        /// <param name="index"></param>
        public void ReadSignal(int index)
        {
            _reader.ReadSignal(Header, Signals[index]);
        }

        /// <summary>
        /// Read the signal matching the given name.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public Signal ReadSignal(string match)
        {
            var signal = Signals.FirstOrDefault(s => s.Label.Value.Equals(match));
            if (signal == null)
            {
                return null;
            }
            
            _reader.ReadSignal(Header, signal);
            return signal;
        }



        /// <summary>
        /// Read the whole file into memory
        /// </summary>
        /// <param name="edfFilePath"></param>
        public void ReadAll(string edfFilePath)
        {
            using (var reader = new Reader(System.IO.File.Open(edfFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                Header = reader.ReadHeader();
                Signals = reader.ReadSignals(Header);
            }
        }

        /// <summary>
        /// Read a whole EDF file from a stream. 
        /// </summary>
        /// <param name="edfFilePath"></param>
        public void ReadAll(Stream stream)
        {
            using (var reader = new Reader(stream))
            {
                Header = reader.ReadHeader();
                Signals = reader.ReadSignals(Header);
            }
        }

        /// <summary>
        /// Read a whole EDF file from a memory buffer. 
        /// </summary>
        /// <param name="edfBytes"></param>
        public void ReadAll(byte[] edfBytes)
        {
            using (var r = new Reader(edfBytes))
            {
                Header = r.ReadHeader();
                Signals = r.ReadSignals(Header);
            }
        }

        public void Save(string edfFilePath)
        {
            if (Header == null) return;

            using (var writer = new Writer(System.IO.File.Open(edfFilePath, FileMode.Create)))
            {
                writer.WriteEDF(this, edfFilePath);
            }
        }
    }
}
