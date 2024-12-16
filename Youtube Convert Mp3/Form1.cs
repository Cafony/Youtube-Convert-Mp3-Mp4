using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using System.Diagnostics;


namespace Youtube_Convert_Mp3
{
    public partial class Form1 : Form
    {
        // Set the output directory path here
        private readonly YoutubeClient youtube = new YoutubeClient();        
        private string videoUrl;
        private string _format;
        private string _outputFilePath;
        //Add ffmpeg file into the aplication instalation folder
        private string _ffmpegPath = Path.Combine(Application.StartupPath, "ffmpeg.exe");

        public Form1()
        {
            InitializeComponent();
        }



        private async void buttonConvert_Click(object sender, EventArgs e)
        {
            videoUrl = textBox1.Text;
            _format = comboBox1.Text;

            // Create a SaveFileDialog to prompt the user for a save location
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
                saveFileDialog.Title = "Save MP3 File";

                try
                {
                    // Get video information
                    var video = await youtube.Videos.GetAsync(videoUrl);
                    var title = video.Title;
                    saveFileDialog.FileName = $"{title}.{_format}"; // Get Default file name


                    if (saveFileDialog.ShowDialog() == DialogResult.OK) {

                        // Set the output file path
                        _outputFilePath = saveFileDialog.FileName;
                        //string outputFilePath = saveFileDialog.FileName;
                        labelStatus.ForeColor=Color.Blue;
                        labelStatus.Text = "Video Converting !";

                        // Convert Process
                        YoutubeConverter(videoUrl,_outputFilePath,_format,_ffmpegPath);
                        await Task.Delay(1000);

                        //Change Color Labels
                        labelStatus.ForeColor = Color.Red;
                        labelStatus.Text = "Download completed!";
                        await Task.Delay(3000);
                        textBox1.Clear();
                        labelStatus.Text = "-";
                        labelStatus.ForeColor = Color.Black;
                        
                    }
                }
                catch (Exception ex)
                {                    
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }       
        
        }

        private async void YoutubeConverter(string videoUrl, string outputFilePath,string _format, string _ffmpegPath)
        {
            //Converted
            var youtube = new YoutubeClient();
            // FFmpeg Definitions Download and convert to MP3
            await youtube.Videos.DownloadAsync(videoUrl, outputFilePath, options => options
                .SetContainer(_format) // Set the desired format                                       
                .SetFFmpegPath(_ffmpegPath)); // Set the path to ffmpeg if not in PATH

            // For using Normalization of volume
            if (checkBoxNormalize.Checked)
            {
                string normalizedOutputPath = Path.ChangeExtension(outputFilePath, null) + "_normalized." + _format;

                NormalizeVolume(outputFilePath, normalizedOutputPath, _ffmpegPath);

                // Step 3: Optionally, replace the original file with the normalized version
                File.Delete(outputFilePath);
                File.Move(normalizedOutputPath, outputFilePath);
            }

        }

        private void NormalizeVolume(string inputFilePath, string outputFilePath, string ffmpegPath)
        {
            // FFmpeg arguments for loudness normalization
            string arguments = $"-i \"{inputFilePath}\" -af loudnorm=I=-14:TP=-2:LRA=11 \"{outputFilePath}\"";

            // Create and start the FFmpeg process
            using (var ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                ffmpegProcess.Start();

                // Capture output and error (optional)
                string output = ffmpegProcess.StandardOutput.ReadToEnd();
                string error = ffmpegProcess.StandardError.ReadToEnd();

                ffmpegProcess.WaitForExit();

                // Check for errors
                if (ffmpegProcess.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg normalization error: {error}");
                }
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //Make ComboBox Inicial to mp3 format
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_outputFilePath == null)
            {
                MessageBox.Show("Please convert a file");
            }
            else 
            {
                string directoryPath = System.IO.Path.GetDirectoryName(_outputFilePath);
                System.Diagnostics.Process.Start("explorer.exe", directoryPath);
            }

        }
    }
}
