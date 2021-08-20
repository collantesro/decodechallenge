using System;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Decoder
{
    class Program
    {
        private static char[] hexMap = "0123456789ABCDEF".ToCharArray();
        private static StringBuilder sb = new();

        private const string decoderBaseFile = "./DecodeChallengeBase.png";
        private const string decoderEncodedFile = "./DecodeChallengeEncoded.png";

        static void Main(string[] args)
        {
            using (Image<Rgb24> basefile = Image.Load<Rgb24>(decoderBaseFile))
            using (Image<Rgb24> encoded = Image.Load<Rgb24>(decoderEncodedFile))
            {
                byte[] combined = new byte[Math.Max(basefile.Width * basefile.Height, encoded.Width * encoded.Height)];
                int comboIdx = 0;
                if (basefile.TryGetSinglePixelSpan(out Span<Rgb24> spanBase))
                {
                    if (encoded.TryGetSinglePixelSpan(out Span<Rgb24> spanEncoded))
                    {
                        for (int i = 0; i < Math.Min(spanBase.Length, spanEncoded.Length); i++)
                        {
                            combined[comboIdx++] = (byte)(spanBase[i].R ^ spanEncoded[i].R);
                        }

                        int lastIdx = GetLastNonzeroIndex(combined);
                        var stats = GetStats(combined, lastIdx);
                        Console.WriteLine(BytesToHex(combined.AsSpan(0, lastIdx + 1)));
                        Console.WriteLine($"Stats: Lowest={stats.lowest}, Highest={stats.highest}");
                        sb.Clear();
                        if (stats.lowest >= 32 && stats.highest <= 126)
                        {
                            foreach (byte b in combined.AsSpan(0, lastIdx + 1))
                            {
                                sb.Append((char)b);
                            }
                            Console.WriteLine(sb.ToString());
                        }
                    }
                }
            }
        }

        static string BytesToHex(Span<byte> bytes)
        {
            sb.Clear();
            int chars = 0;
            foreach (byte b in bytes)
            {
                sb.Append(hexMap[b >> 4]).Append(hexMap[b & 0x0f]).Append(' ');
                chars += 2;
                if (chars % 32 == 0)
                    sb.AppendLine();
            }

            sb.Append(chars / 2).Append(" bytes");
            return sb.ToString();
        }

        static int GetLastNonzeroIndex(byte[] bytes)
        {
            int idx = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                    idx = i;
            }
            return idx;
        }

        public static (byte lowest, byte highest) GetStats(Span<byte> bytes, int stopIdx)
        {
            byte lowest = byte.MaxValue;
            byte highest = byte.MinValue;
            for (int i = 0; i <= stopIdx; i++)
            {
                byte b = bytes[i];
                lowest = lowest <= b ? lowest : b;
                highest = highest >= b ? highest : b;
            }
            return (lowest, highest);
        }
    }
}
