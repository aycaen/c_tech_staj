using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        private Process? zmqProc;
        private const string ZMQ_EXE = "zeromq.exe";
        private string binPath = @"C:\Users\LENOVO\OneDrive - Yildiz Technical University\Desktop\converted.bin";
        private List<string> outputPaths = new List<string>();

        public Form1()
        {
            InitializeComponent();
            ConfigureUI();

            if (!File.Exists(binPath))
            {
                MessageBox.Show($"Bin dosyası bulunamadı:\n{binPath}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            YukleSureSecenekleri(binPath);
            // Seçim ve dönüştür butonlarını göster
            labelSure.Visible = true;
            comboBoxSure.Visible = true;
            btnConvert.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            zmqProc = ZmqServerManager.StartZmqServer(ZMQ_EXE);
            if (zmqProc == null)
            {
                Close();
                return;
            }
            System.Threading.Thread.Sleep(600); // Bağlantı için gecikme
        }

        private void ConfigureUI()
        {


            // labelBinPath, textBoxBinPath, btnLoadPath başta görünür
            labelSure.Visible = false;
            comboBoxSure.Visible = false;
            btnConvert.Visible = false;
            lblStatus.Visible = false;
            labelOutput.Visible = false;
            comboBoxOutput.Visible = false;

            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnConvert.BackColor = Color.SteelBlue;
            btnConvert.ForeColor = Color.White;
            btnConvert.FlatStyle = FlatStyle.Flat;
            btnConvert.FlatAppearance.BorderSize = 0;

            comboBoxSure.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOutput.DropDownStyle = ComboBoxStyle.DropDownList;
        }


        private void YukleSureSecenekleri(string path)
        {
            const int sampleRate = 44100;
            const int bitsPerSample = 8;
            const int numChannels = 1;

            long byteSize = new FileInfo(path).Length;
            int bytesPerSecond = sampleRate * numChannels * (bitsPerSample / 8);
            int totalSeconds = (int)(byteSize / bytesPerSecond);
            int totalHours = totalSeconds / 3600;

            comboBoxSure.Items.Clear();
            if (totalHours < 2)
            {
                comboBoxSure.Items.Add("Tamamı");
            }
            else
            {
                for (int h = 2; h < totalHours; h += 2)
                    comboBoxSure.Items.Add($"İlk {h} saat");
                comboBoxSure.Items.Add("Tamamı");
            }
            comboBoxSure.SelectedIndex = 0;
        }




        private void InitializeComponent()
        {
            labelSure = new Label();
            comboBoxSure = new ComboBox();
            btnConvert = new Button();
            lblStatus = new Label();
            labelOutput = new Label();
            comboBoxOutput = new ComboBox();
            SuspendLayout();
            // 
            // labelSure
            // 
            labelSure.AutoSize = true;
            labelSure.Location = new Point(162, 52);
            labelSure.Name = "labelSure";
            labelSure.Size = new Size(283, 25);
            labelSure.TabIndex = 3;
            labelSure.Text = "Hangi bölümü dinlemek istersiniz?";
            labelSure.Visible = false;
            // 
            // comboBoxSure
            // 
            comboBoxSure.FormattingEnabled = true;
            comboBoxSure.Location = new Point(162, 98);
            comboBoxSure.Name = "comboBoxSure";
            comboBoxSure.Size = new Size(182, 33);
            comboBoxSure.TabIndex = 4;
            comboBoxSure.Visible = false;
            // 
            // btnConvert
            // 
            btnConvert.Location = new Point(377, 98);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new Size(112, 34);
            btnConvert.TabIndex = 5;
            btnConvert.Text = "Dönüştür";
            btnConvert.UseVisualStyleBackColor = true;
            btnConvert.Visible = false;
            btnConvert.Click += btnConvert_Click_1;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(266, 155);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 6;
            lblStatus.Visible = false;
            // 
            // labelOutput
            // 
            labelOutput.AutoSize = true;
            labelOutput.Location = new Point(224, 227);
            labelOutput.Name = "labelOutput";
            labelOutput.Size = new Size(176, 25);
            labelOutput.TabIndex = 7;
            labelOutput.Text = "Dönüştürülen dosya:";
            labelOutput.Visible = false;
            labelOutput.Click += labelOutput_Click;
            // 
            // comboBoxOutput
            // 
            comboBoxOutput.FormattingEnabled = true;
            comboBoxOutput.Location = new Point(224, 255);
            comboBoxOutput.Name = "comboBoxOutput";
            comboBoxOutput.Size = new Size(182, 33);
            comboBoxOutput.TabIndex = 8;
            comboBoxOutput.Visible = false;
            comboBoxOutput.SelectedIndexChanged += comboBoxOutput_SelectedIndexChanged_1;
            // 
            // Form1
            // 
            ClientSize = new Size(664, 362);
            Controls.Add(comboBoxOutput);
            Controls.Add(labelOutput);
            Controls.Add(lblStatus);
            Controls.Add(btnConvert);
            Controls.Add(comboBoxSure);
            Controls.Add(labelSure);
            Name = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();

        }
        private Label labelSure;
        private ComboBox comboBoxSure;
        private Button btnConvert;
        private Label lblStatus;
        private Label labelOutput;
        private ComboBox comboBoxOutput;

        private void labelOutput_Click(object sender, EventArgs e)
        {

        }

        private async void btnConvert_Click_1(object sender, EventArgs e)
        {
            // Bölüm seçimi
            string secim = comboBoxSure.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(secim))
            {
                MessageBox.Show("Lütfen bir bölüm seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int durationSec = 0;  // 0 = tamamı
            if (secim.StartsWith("İlk", StringComparison.OrdinalIgnoreCase))
            {
                int saat = int.Parse(secim.Split(' ')[1]);
                durationSec = saat * 3600;
            }

            // Çıktı klasörü
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string baseName = Path.GetFileNameWithoutExtension(binPath);
            string outputDir = Path.Combine(desktop, baseName + "_Kayitlar");
            Directory.CreateDirectory(outputDir);

            // Hedef wav yolu
            string wavPath = Path.Combine(outputDir, secim.Replace(" ", "_") + ".wav");

            var request = new ZmqRequest
            {
                binPath = binPath,
                outputFolder = outputDir,
                numChannels = 1,
                sampleRate = 44100,
                bitsPerSample = 8,
                durationSec = durationSec,
                isFromEnd = false
            };
            string json = JsonConvert.SerializeObject(request);


            btnConvert.Enabled = false;
            lblStatus.Visible = true;
            lblStatus.Text = "Dönüştürülüyor...";

            // ZMQ çağrısı
            await Task.Run(() =>
            {
                using var client = new RequestSocket();
                client.Connect("tcp://localhost:5555");
                client.SendFrame(json);
                string reply = client.ReceiveFrameString();
                dynamic resp = JsonConvert.DeserializeObject(reply);

                Invoke((Action)(() =>
                {
                    outputPaths.Clear();
                    comboBoxOutput.Items.Clear();

                    if ((string)resp.status == "success")
                    {
                        // Sunucudan dönen tüm wav yollarını al
                        foreach (string p in resp.outputs.ToObject<List<string>>())
                        {
                            outputPaths.Add(p);
                            comboBoxOutput.Items.Add(Path.GetFileName(p));
                        }
                        comboBoxOutput.SelectedIndex = 0;
                        labelOutput.Visible = true;
                        comboBoxOutput.Visible = true;
                        lblStatus.Text = "Dönüştürme tamamlandı.";
                    }
                    else
                    {
                        lblStatus.Text = "Dönüştürme başarısız.";
                        MessageBox.Show($"Sunucu hatası: {(string)resp.status}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            });

            btnConvert.Enabled = true;
        }

        private void comboBoxOutput_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int idx = comboBoxOutput.SelectedIndex;
            if (idx < 0 || idx >= outputPaths.Count) return;

            string filePath = outputPaths[idx];
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Dosya bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Dosyanın bulunduğu klasörü al
            string folder = Path.GetDirectoryName(filePath);

            // Explorer'ı doğrudan o klasöre yönlendir
            var psi = new ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (zmqProc is { HasExited: false })
                    zmqProc.Kill();          // ani kapat (veya CloseMainWindow)
            }
            catch { /* yok say */ }

            base.OnFormClosed(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
