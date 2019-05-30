using System;
using System.IO;
using CsvHelper;
using Monitor.Model;

namespace Monitor.Services
{
    public class ResultsCsvWriter : IDisposable
    {
        public class Data
        {
            public Data(DateTime timeStamp, double? spo2, double? pulse)
            {
                TimeStamp = timeStamp;
                SPO2 = spo2;
                Pulse = pulse;
            }

            public DateTime TimeStamp { get; set; }
            public double? SPO2 { get; set; }
            public double? Pulse { get; set; }
        }

        private readonly CsvWriter csvHelper;
        private readonly StreamWriter fileWriter;

        public ResultsCsvWriter(string filePath)
        {
            this.fileWriter = new StreamWriter(filePath);
            this.fileWriter.AutoFlush = true;
            this.csvHelper = new CsvHelper.CsvWriter(this.fileWriter);
        }

        public void WriteHeader()
        {
            this.csvHelper.WriteHeader<Data>();
            this.csvHelper.NextRecord();
        }

        public void WriteDataRow(PulsometerRecordingResult data)
        {
            this.csvHelper.WriteRecord(new Data(data.PulsometerData.Timestamp,
                data.PulsometerData.SPO2, data.PulsometerData.Pulse));
            this.csvHelper.NextRecord();
            this.csvHelper.Flush();
        }

        public void Dispose()
        {
            csvHelper?.Dispose();
            fileWriter?.Dispose();
        }
    }
}