using System.IO;
using System.Runtime.InteropServices;

namespace SharpLib.EuropeanDataFormat
{
    public class File
    {
        public Header Header { get; set; }
        public Signal[] Signals { get; set; }

        public File() { }
        public File(string edfFilePath) {
            Open(edfFilePath);
        }

        public File(byte[] edfBytes){
            Open(edfBytes);
        }

        public void ReadBase64(string edfBase64)
        {
            byte[] edfBytes = System.Convert.FromBase64String(edfBase64);
            Open(edfBytes);
        }

        public void Open(string edfFilePath)
        {
            using (var reader = new Reader(System.IO.File.Open(edfFilePath, FileMode.Open, FileAccess.Read)))
            {
                Header = reader.ReadHeader();
                Signals = reader.ReadSignals();
            }
        }

        public void Open(byte[] edfBytes)
        {
            using (var r = new Reader(edfBytes))
            {
                Header = r.ReadHeader();
                Signals = r.ReadSignals();
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
