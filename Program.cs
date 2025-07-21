using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        // 1. Uçuş seçimi
        // Console.WriteLine("Hangi uçuşu dinlemek istersiniz? [1, 2, 3, 4]");
        //  string secim = Console.ReadLine();
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    
        string binPath = "C:\\Users\\LENOVO\\OneDrive - Yildiz Technical University\\Desktop\\deneme_6hour.bin";
        string baseFolder = desktopPath;
       
        // Zaman ölçüm
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // İstek oluştur
        var request = new
        {
            binPath,
            outputFolder = baseFolder,
            numChannels = 2,
            sampleRate = 44100,
            bitsPerSample = 16
        };

        string json = JsonConvert.SerializeObject(request);

        // ZMQ gönder
        using (var client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");
            client.SendFrame(json);
            string reply = client.ReceiveFrameString();
            stopwatch.Stop();
            Console.WriteLine("\nZMQ Cevabı: \n" + reply);
            Console.WriteLine($"\nToplam süre: {stopwatch.Elapsed.TotalSeconds:F2} saniye");
        }

        Console.WriteLine("\nTüm parçalar oluşturuldu.");
    }
}
