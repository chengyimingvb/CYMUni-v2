using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Writers;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace CYM
{
    public partial class FileUtil:BaseFileUtil
    {


        #region 压缩
        // 字符串会转换成utf8存储
        public static byte[] GZCompressToBytes(string content)
        {
            return GZCompressToBytes(Encoding.UTF8.GetBytes(content));
        }

        // 假定字符串存储是utf8格式
        public static string GZDecompressToString(byte[] data)
        {
            return Encoding.UTF8.GetString(GZDecompressToBytes(data));
        }

        public static byte[] GZCompressToBytes(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return GZCompressToBytes(ms);
            }
        }

        public static byte[] GZCompressToBytes(Stream inStream)
        {
            var archive = GZipArchive.Create();
            archive.AddEntry("content", inStream, false);
            MemoryStream ms = new MemoryStream();
            archive.SaveTo(ms, new WriterOptions(SharpCompress.Common.CompressionType.Deflate));
            return ms.ToArray();
        }

        public static byte[] GZDecompressToBytes(byte[] data)
        {
            using (MemoryStream ms = GZDecompressToMemoryStream(data))
            {
                return ms.ToArray();
            }
        }

        static MemoryStream GZDecompressToMemoryStream(byte[] data)
        {
            using (MemoryStream inMs = new MemoryStream(data))
            {
                var archive = GZipArchive.Open(inMs);
                var entry = archive.Entries.First();
                MemoryStream ms = new MemoryStream();
                entry.WriteTo(ms);
                ms.Position = 0;
                return ms;
            }
        }
        #endregion

        #region hash
        public static string Hash(string input)
        {
            return Hash(Encoding.UTF8.GetBytes(input));
        }

        static string HashToString(byte[] hash)
        {
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static string Hash(byte[] input)
        {
            var hash = (new SHA1Managed()).ComputeHash(input);
            return HashToString(hash);
        }

        public static string HashFile(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            string hash = HashStream(fs);
            fs.Close();
            return hash;
        }

        internal static string HashStream(Stream s)
        {
            return HashToString((new SHA1Managed()).ComputeHash(s));
        }
        #endregion
    }
}