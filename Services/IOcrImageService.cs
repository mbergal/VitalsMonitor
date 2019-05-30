using System;
using System.Drawing;
using LanguageExt;
using Monitor.Model;

namespace Monitor.Services
{
    public interface IOcrImageService : IDisposable
    {
        PulsometerRecordingResult OcrPulsometerData(Bitmap image);
    }
}