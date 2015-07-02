using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GCS_Analytics_Server
{
    class Program
    {
        private static string root
        {

            get
            {
                var codebase = System.Reflection.Assembly.GetExecutingAssembly().Location;
                codebase = codebase.Substring(0, codebase.LastIndexOf('\\'));
                return codebase + "\\";
            }

        }

        static void Main(string[] args)
        {

            TcpListener server = new TcpListener(4272);
            server.Start();
            Console.WriteLine("Server Started.");


            while (true)
            {
                Console.WriteLine("Waiting for an incomming report...");

                var connection = server.AcceptTcpClient();

                Console.WriteLine("Connection incomming...");
                var stream = connection.GetStream();
                DateTime now = DateTime.Now;

                byte[] bytes = ReadToEnd(stream);
                string str = Encoding.ASCII.GetString(bytes);
                string[] parts = str.Split('\n');
                string folder = parts[0].ToUpper();

                string filename = root + folder + "\\" + now.Year + "\\" + now.ToString("MMM") + "\\" + now.ToString("dd") + "\\" + now.ToString("hh-mm-ss") + ".log";

                if (!Directory.Exists(root + folder)) Directory.CreateDirectory(root + folder);
                if (!Directory.Exists(root + folder + "\\" + now.Year)) Directory.CreateDirectory(root + folder + "\\" + now.Year);
                if (!Directory.Exists(root + folder + "\\" + now.Year + "\\" + now.ToString("MMM"))) Directory.CreateDirectory(root + folder + "\\" + now.Year + "\\" + now.ToString("MMM"));
                if (!Directory.Exists(root + folder + "\\" + now.Year + "\\" + now.ToString("MMM") + "\\" + now.ToString("dd"))) Directory.CreateDirectory(root + folder + "\\" + now.Year + "\\" + now.ToString("MMM") + "\\" + now.ToString("dd"));

                Console.WriteLine("File to write: " + filename);

                File.WriteAllBytes(filename, bytes);

            }

        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}
