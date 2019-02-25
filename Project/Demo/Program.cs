using System;
using System.Linq;
using EDF = SharpLib.EuropeanDataFormat;
using System.IO;
using System.Collections.Generic;

namespace EuropeanDataFormatDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Example1_Create_And_Save_EDF();

            //if(args.Length >= 1)
            //Example2_Read_EDF_From_Base64(args[0]);

            // Read and dump some data

            if (args.Count() == 0)
            {
                Console.WriteLine("Error: Expecting path to folder as argument!");
                // Exit code?
                return;
            }

            string filepath = args[0];
            DirectoryInfo d = new DirectoryInfo(filepath);

            foreach (FileInfo fileInfo in d.GetFiles("*.edf"))
            {
                Console.WriteLine("======== EDF: " + fileInfo.Name + "========");

                DateTime mark = DateTime.Now;
                //

                EDF.File edf = null;
                int signalIndex = -1;

                if (args.Count()==1)
                {
                    // No signal index specified, read the whole file
                    edf = new EDF.File(fileInfo.FullName);
                }
                else
                {
                    // Signal specified, just read that one signal 
                    using (edf = new EDF.File())
                    {
                        edf.Open(fileInfo.FullName);
                        // Read specified signals
                        for (int i=1; i<args.Count(); i++)
                        {
                            Console.WriteLine("Reading signal: " + args[i]);
                            bool success = edf.ReadSignal(args[i]);
                            if (!success)
                            {
                                Console.WriteLine("ERROR: Signal " + args[i] + " not found");
                            }
                        }
                    }
                }
                

                // Print loading time
                TimeSpan runTime = DateTime.Now - mark;
                Console.WriteLine("Load time: " + runTime.TotalSeconds.ToString("0.00") + " s");

                // Print headers
                Console.WriteLine(edf.Header.ToString());

                // Print signals intro
                if (signalIndex<0)
                {
                    // Print all signal
                    foreach (EDF.Signal s in edf.Signals)
                    {
                        Console.WriteLine(s.ToString());
                    }      
                }
                else
                {
                    // Print the signal we loaded
                    Console.WriteLine(edf.Signals[signalIndex].ToString());
                }


                Console.WriteLine("=========================================\n");


                //Console.WriteLine("Scaled sample test: " + edf.Signals[31].ScaledSample(0));

                //TODO: Test saving files, I guess we would need to fix recording writing function
            }

            


            Console.ReadLine();

        }

        private static void Example1_Create_And_Save_EDF()
        {

            //Crreate an empty EDF file
            var edfFile = new EDF.File();

            //Create a signal object
            var ecgSig = new EDF.Signal();
            ecgSig.Label.Value = "ECG";
            ecgSig.SampleCountPerRecord.Value = 10;
            ecgSig.PhysicalDimension.Value = "mV";
            ecgSig.DigitalMinimum.Value = -2048;
            ecgSig.DigitalMaximum.Value = 2047;
            ecgSig.PhysicalMinimum.Value = -10.2325;
            ecgSig.PhysicalMaximum.Value = 10.2325;
            ecgSig.TransducerType.Value = "UNKNOWN";
            ecgSig.Prefiltering.Value = "UNKNOWN";
            ecgSig.Reserved.Value = "RESERVED";
            ecgSig.Samples = new List<short> { 100, 50, 23, 75, 12, 88, 73, 12, 34, 83 };

            //Set the signal
            edfFile.Signals = new EDF.Signal[1] { ecgSig };

            //Create the header object
            var h = new EDF.Header();
            h.RecordDurationInSeconds.Value = 1;
            h.Version.Value = "0";
            h.PatientID.Value = "TEST PATIENT ID";
            h.RecordID.Value = "TEST RECORD ID";
            h.RecordingStartDate.Value = "11.11.16"; //dd.mm.yy
            h.RecordingStartTime.Value = "12.12.12"; //hh.mm.ss
            h.Reserved.Value = "RESERVED";
            h.RecordCount.Value = 1;
            h.SignalCount.Value = (short)edfFile.Signals.Length;
            h.Signals.Reserveds.Value = Enumerable.Repeat("RESERVED".PadRight(32, ' '),
                                                h.SignalCount.Value).ToArray();

            //Set the header
            edfFile.Header = h;

            //Print some info
            Console.Write(
                "\nPatient ID:\t\t" + edfFile.Header.PatientID.Value +
                "\nNumber of signals:\t" + edfFile.Header.SignalCount.Value +
                "\nStart date:\t\t" + edfFile.Header.RecordingStartDate.Value +
                "\nSignal label:\t\t" + edfFile.Signals[0].Label.Value +
                "\nSignal samples:\t\t"
                    + String.Join(",", edfFile.Signals[0].Samples.Skip(0).Take(10).ToArray())
                    + "\n\n"
             );

            //Save the file
            string fileName = @"C:\temp\example.edf";
            edfFile.Save(fileName);

            //Read the file
            var f = new EDF.File(fileName);

            Console.ReadLine();
        }

        private static void Example2_Read_EDF_From_Base64(string edfBase64FilePath)
        {
            var edfBase64 = File.ReadAllText(edfBase64FilePath);
            var edfFile = new EDF.File();
            edfFile.ReadBase64(edfBase64);
            edfFile.Save(@"C:\temp\edf_bytes.edf");
        }
    }
}
