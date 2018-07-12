using System.Collections.Generic;
using System.Linq;

namespace LibEDF_DotNet
{
    public class EDFSignal
    {
        public FixedLengthString Label              { get; } = new FixedLengthString(HeaderItems.Label);
        public FixedLengthString TransducerType     { get; } = new FixedLengthString(HeaderItems.TransducerType);
        public FixedLengthString PhysicalDimension  { get; } = new FixedLengthString(HeaderItems.PhysicalDimension);
        public FixedLengthDouble PhysicalMinimum    { get; } = new FixedLengthDouble(HeaderItems.PhysicalMinimum);
        public FixedLengthDouble PhysicalMaximum    { get; } = new FixedLengthDouble(HeaderItems.PhysicalMaximum);
        public FixedLengthInt DigitalMinimum        { get; } = new FixedLengthInt(HeaderItems.DigitalMinimum);
        public FixedLengthInt DigitalMaximum        { get; } = new FixedLengthInt(HeaderItems.DigitalMaximum);
        public FixedLengthString Prefiltering       { get; } = new FixedLengthString(HeaderItems.Prefiltering);
        public FixedLengthInt SampleCountPerRecord  { get; } = new FixedLengthInt(HeaderItems.NumberOfSamplesInDataRecord);
        public FixedLengthString Reserved           { get; } = new FixedLengthString(HeaderItems.SignalsReserved);

        public List<short> Samples { get; set; } = new List<short> { };

        public override string ToString()
        {
            return Label.Value + " " + SampleCountPerRecord.Value.ToString() + "/" + Samples.Count().ToString() + " [" 
                + string.Join(",", Samples.Skip(0).Take(10).ToArray()) + " ...]";
        }
    }
}
