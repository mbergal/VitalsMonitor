using System;
using System.Drawing;
using Monitor.Model;
using Monitor.Services;

namespace Monitor.Windows.MainWindow
{
    public class Model : ModelWithEffect<Model>
    {
        private readonly Lazy<Effects> _effects;

        public Model(Lazy<MainWindow.Effects> effects)
        {
            this._effects = effects;
            this.EmailAddress = "misha.bergal@gmail.com";
        }

        public WindowCaptureStream Source { get; set; }

        public string RecognizedText { get; set; }
        public PulsometerData RecognizedData { get; set; }

        public bool TestEmailButtonEnabled =>
            !string.IsNullOrWhiteSpace(this.EmailAddress) || this.TestingEmailShown;

        public string EmailAddress { get; set; }

        public void SelectSourceRequested()
        {
            this.Effect = dispatch => { _effects.Value.SelectWindow(dispatch); };
        }

        public void SourceSelected(WindowService.Window window)
        {
            if (window != null)
            {
                this.Source = new WindowCaptureStream(window.WindowHandle)
                {
                    FrameInterval = 1000
                };
                this.Effect = dispatch => { this._effects.Value.OpenVideoSource(this.Source); };
            }
        }

        private static DateTime d = new DateTime();

        public void NewFrameReceived(Bitmap image)
        {
            DateTime now = DateTime.Now;

            if (now - d > TimeSpan.FromSeconds(1))
            {
                this.Effect = dispatch => { _effects.Value.OcrImage(dispatch, new Bitmap(image)); };

                d = now;
            }
        }

        public void TextRecognized(string text)
        {
            RecognizedText = text;
        }

        public void DataRecognized(PulsometerData dataItem1)
        {
            this.RecognizedData = dataItem1;
        }

        public void EmailTestRequested()
        {
            this.Effect = dispatch =>
            {
                this._effects.Value.TestEmail(dispatch, this.EmailAddress);
            };
        }

        public void EmailAddressChanged(string text)
        {
            this.EmailAddress = text;
        }

        public void EmailTestStarted()
        {
            this.TestingEmailShown = true;
        }

        public bool TestingEmailShown { get; set; }

        public void EmailTestFinished()
        {
            this.TestingEmailShown = false;
        }
    }
}