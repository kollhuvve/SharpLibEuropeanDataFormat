using System.IO;
using System.Runtime.InteropServices;

namespace SharpLib.EuropeanDataFormat
{
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
       Guid("29757a02-8a0c-47e2-96bb-2266c993c97a")]
    public interface IEDFFile
    {
        //Expose methods for COM use
        void Open(string edfFilePath);
        void Open(byte[] edfBytes);
        void Save(string edfFilePath);
    }

    [ClassInterface(ClassInterfaceType.None),
        Guid("07504667-1e49-4535-9c2f-157ee5b280b0")]
    public class File : IEDFFile
    {
        public Header Header { get; set; } = new Header();
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
            using (var reader = new Reader(System.IO.File.Open(edfFilePath, FileMode.Open)))
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
