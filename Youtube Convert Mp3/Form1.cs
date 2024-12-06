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


namespace Youtube_Convert_Mp3
{
    public partial class Form1 : Form
    {
        // Set the output directory path here
        private readonly YoutubeClient youtube = new YoutubeClient();


        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string videoUrl = textBox1.Text;
            var youtube = new YoutubeClient();
            string _format = comboBox1.Text;

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

                    saveFileDialog.FileName = $"{title}.{_format}"; // Default file name


                    if (saveFileDialog.ShowDialog() == DialogResult.OK) {

                        // Set the output file path
                        string outputFilePath = saveFileDialog.FileName;
                        //string outputFilePath = saveFileDialog.FileName;
                        labelStatus.ForeColor=Color.Blue;
                        labelStatus.Text = "Video Converting !";
                                                

                            // FFmpeg Definitions Download and convert to MP3
                            await youtube.Videos.DownloadAsync(videoUrl, outputFilePath, options => options
                                .SetContainer(_format) // Set the desired format
                                .SetFFmpegPath("c:/apagar/ffmpeg.exe")); // Set the path to ffmpeg if not in PATH

                        labelStatus.ForeColor = Color.Red;
                        labelStatus.Text = "Download completed!";
                        await Task.Delay(5000);
                        textBox1.Clear();
                        labelStatus.Text = "-";
                        labelStatus.ForeColor = Color.Black;

                        // Open Folder in the end
                        if (checkBox1.Checked) {                            
                            string directoryPath = System.IO.Path.GetDirectoryName(outputFilePath);
                            // Open the directory in File Explorer
                            System.Diagnostics.Process.Start("explorer.exe", directoryPath);
                        }                     

                        
                    }
                }
                catch (Exception ex)
                {
                    labelStatus.Text = $"Error: {ex.Message}";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

    }
}
