using System;
using System.Drawing;
using System.IO;
using Monitor.Model;
using NLog;

namespace Monitor.Services
{
    public class DataCollectionService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetLogger("DataCollectionService");
        private readonly IOcrImageService _ocrImageService;
        private ResultsCsvWriter _resultsCsvWriter;
        private string _resultsFilePath;

        public string ResultsFilePath
        {
            get => this._resultsFilePath;
            set
            {
                Logger.Info($"ResultFilePath: {value}");
                if (value != this.ResultsFilePath)
                {
                    bool fileExists = File.Exists(this.ResultsFilePath);

                    this._resultsCsvWriter = new ResultsCsvWriter(value);
                    if (!fileExists)
                    {
                        _resultsCsvWriter.WriteHeader();
                    }

                    this._resultsFilePath = value;
                }
            }
        }

        public DataCollectionService(IOcrImageService ocrImageService)
        {
            this._ocrImageService = ocrImageService;
        }

        public PulsometerRecordingResult GetData(Bitmap image)
        {
            Logger.Info($"GetData: image=(image)");
            var data = _ocrImageService.OcrPulsometerData(image);
            RecordData(data);
            return data;
        }


        private void RecordData(PulsometerRecordingResult pulsometerRecordingResult)
        {
            _resultsCsvWriter?.WriteDataRow(pulsometerRecordingResult);
        }

        public void Dispose()
        {
            _ocrImageService?.Dispose();
            _resultsCsvWriter.Dispose();
        }
    }
}