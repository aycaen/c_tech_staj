using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace WinFormsApp2
{
    internal class ZmqServerManager
    {
        public static Process? StartZmqServer(string exeName)// Belirtilen bir yürütülebilir dosyayı başlatmaya çalışır.
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName); // uygulamanın çalıştığı dizini veririr ve exeName ile birleştirir.
            if (!File.Exists(exePath))
            {
                MessageBox.Show($"Sunucu bulunamadı:\n{exePath}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            try
            {
                var psi = new ProcessStartInfo // ProcessStartInfo sınıfı, yeni bir işlem başlatmak için gerekli bilgileri tutar.
                {
                    FileName = exePath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                return Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sunucu başlatılamadı:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
