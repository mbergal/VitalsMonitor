using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Accord.Controls;
using Accord.Video;
using Monitor.Services;
using Form = Monitor.Windows.SelectWindow.Form;

namespace Monitor.Windows.MainWindow
{
    public partial class MainForm : System.Windows.Forms.Form, IForm<Model>
    {
        private Stopwatch stopWatch = null;

        public MainForm()
        {
            InitializeComponent();

            var dataCollectionService = new DataCollectionService(new OcrImageService());
            var alertService = new AlertService();

            var resultsFile =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Vitals Monitor", "results.csv");
            if (!Directory.Exists(Path.GetDirectoryName(resultsFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(resultsFile));
            }

            dataCollectionService.ResultsFilePath = resultsFile;
            this.Model = new Model(new Lazy<Effects>(() => this.Effects));
            this.Effects = new Effects(
                this,
                new PagerDutyService("<?>"),
                new AlertService(),
                dataCollectionService
            );
            this.Mediator = new Mediator<MainForm, Model, Effects>(this, this.Model, this.Effects);
            this.emailTextBox.TextChanged += EmailTextBox_TextChanged;
        }

        public Effects Effects { get; set; }

        public Model Model { get; set; }


        private void EmailTextBox_TextChanged(object sender, EventArgs e)
        {
            Model.EmailAddressChanged(this.emailTextBox.Text);
            Mediator.EndTick();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentVideoSource(videoSourcePlayer);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void OpenVideoSource(IVideoSource source)
        {
            OpenVideoSource(this, videoSourcePlayer, source, timer, out stopWatch);
        }

        // Open video source
        public static void OpenVideoSource(Control control, VideoSourcePlayer videoSourcePlayer,
            IVideoSource source,
            Timer timer, out Stopwatch stopWatch)
        {
            // set busy cursor
            control.Cursor = Cursors.WaitCursor;

            // stop current video source
            CloseCurrentVideoSource(videoSourcePlayer);

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();

            // reset stop watch
            stopWatch = null;

            // start timer
            timer.Start();

            control.Cursor = Cursors.Default;
        }

        // Close video source if it is running
        private static void CloseCurrentVideoSource(VideoSourcePlayer videoSourcePlayer)
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
            }
        }

        private Mediator<MainForm, Model, Effects> Mediator;

        private void SelectSource_Click(object sender, EventArgs e)
        {
            Model.SelectSourceRequested();
            Mediator.EndTick();
        }

        private void TestEmailButton_Click(object sender, EventArgs e)
        {
            Model.EmailTestRequested();
            Mediator.EndTick();
        }

        // New frame received by the player
        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            Model.NewFrameReceived(new Bitmap(image));
            Mediator.EndTick();

            DateTime now = DateTime.Now;
            Graphics g = Graphics.FromImage(image);

            // paint current time
            SolidBrush brush = new SolidBrush(Color.Red);
            g.DrawString(now.ToString(), this.Font, brush, new PointF(5, 5));
            brush.Dispose();

            g.Dispose();
        }

        // On timer event - gather statistics
        private void timer_Tick(object sender, EventArgs e)
        {
            IVideoSource videoSource = videoSourcePlayer.VideoSource;

            if (videoSource != null)
            {
                // get number of frames since the last timer tick
                int framesReceived = videoSource.FramesReceived;

                if (stopWatch == null)
                {
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                }
                else
                {
                    stopWatch.Stop();

                    float fps = 1000.0f * framesReceived / stopWatch.ElapsedMilliseconds;
                    fpsLabel.Text = fps.ToString("F2") + " fps";

                    stopWatch.Reset();
                    stopWatch.Start();
                }
            }
        }


        public WindowService.Window SelectWindow()
        {
            var dialog = new Form(new ScreenshotService(), new WindowService());

            var result = dialog.ShowDialog(this) == DialogResult.OK;
            return result ? dialog.Result : null;
        }

        public void SyncUI(Model model)
        {
            this.recognizedText.Text = model.RecognizedText;
            this.testEmailButton.Enabled = model.TestEmailButtonEnabled;

            if (model.RecognizedData != null)
            {
                this.spo2Label.Text = model.RecognizedData.SPO2.ToString();
                this.pulseLabel.Text = model.RecognizedData.Pulse.ToString();
            }
            else
            {
                this.spo2Label.Text = "???";
                this.pulseLabel.Text = "???";
            }
        }
    }
}