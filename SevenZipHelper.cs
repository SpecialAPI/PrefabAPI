using System;
using System.IO;
using SevenZip.Compression.LZMA;


namespace PrefabAPI
{
    public static class SevenZipHelper
    {


        static int dictionary = 1 << 23;

        // static Int32 posStateBits = 2;
        // static  Int32 litContextBits = 3; // for normal files
        // UInt32 litContextBits = 0; // for 32-bit data
        // static  Int32 litPosBits = 0;
        // UInt32 litPosBits = 2; // for 32-bit data
        // static   Int32 algorithm = 2;
        // static    Int32 numFastBytes = 128;

        static bool eos = false;





        static SevenZip.CoderPropID[] propIDs =
                   {
                    SevenZip.CoderPropID.DictionarySize,
                    SevenZip.CoderPropID.PosStateBits,
                    SevenZip.CoderPropID.LitContextBits,
                    SevenZip.CoderPropID.LitPosBits,
                    SevenZip.CoderPropID.Algorithm,
                    SevenZip.CoderPropID.NumFastBytes,
                    SevenZip.CoderPropID.MatchFinder,
                    SevenZip.CoderPropID.EndMarker
                };

        // these are the default properties, keeping it simple for now:
        static object[] properties =
                   {
                    (Int32)(dictionary),
                    (Int32)(2),
                    (Int32)(3),
                    (Int32)(0),
                    (Int32)(2),
                    (Int32)(128),
                    "bt4",
                    eos
                };

        public static byte[] Compress(byte[] inputBytes)
        {
            MemoryStream inStream = new MemoryStream(inputBytes);
            MemoryStream outStream = new MemoryStream();
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            encoder.Code(inStream, outStream, -1, -1, null);
            return outStream.ToArray();
        }
    }
}
