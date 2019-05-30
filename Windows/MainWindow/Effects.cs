using System;
using System.Drawing;
using Monitor.Model;
using Monitor.Services;

namespace Monitor.Windows.MainWindow
{
    public class Effects
    {
        private readonly MainForm mainForm;
        private readonly ISendAlertService _sendAlertService;
        private readonly DataCollectionService dataCollectionService;
        private AlertService _alertService;

        public Effects(MainForm mainForm,
            ISendAlertService sendAlertService,
            AlertService alertService,
            DataCollectionService dataCollectionService)
        {
            this.mainForm = mainForm;
            this.dataCollectionService = dataCollectionService;
            this._alertService = alertService;
            this._sendAlertService = sendAlertService;
        }

        public void SelectWindow(Action<Action<Model>> dispatch)
        {
            var window = mainForm.Invoke(() => mainForm.SelectWindow());
            dispatch(model => model.SourceSelected(window));
        }

        public void OpenVideoSource(WindowCaptureStream source)
        {
            mainForm.Invoke((Action) (() => mainForm.OpenVideoSource(source)));
        }


        public void OcrImage(Action<Action<Model>> dispatch, Bitmap image)
        {
            lock (dataCollectionService)
            {
                var data = dataCollectionService.GetData(image);
                Alert alert = this._alertService.SetData(data.PulsometerData);
                if (alert != null)
                {
//                    this._sendAlertService.SendAlert("?", "???");
                }

                dispatch(model =>
                {
                    model.TextRecognized(data.RecognizedText);
                    model.DataRecognized(data.PulsometerData);
                });
            }
        }

        public void TestEmail(Action<Action<Model>> dispatch, string emailAddress)
        {
            dispatch(model => model.EmailTestStarted());
            this._sendAlertService.SendAlert("Test Email", "Test Description");
            dispatch(model => model.EmailTestFinished());
        }
    }
}