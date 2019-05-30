using System.Drawing;

namespace Monitor.Model
{
    public class PulsometerRecordingResult
    {
        public PulsometerRecordingResult(
            PulsometerData pulsometerData,
            string recognizedText,
            Image image)
        {
            this.PulsometerData = pulsometerData;
            this.RecognizedText = recognizedText;
            this.Image = image;
        }

        public PulsometerData PulsometerData { get; }
        public string RecognizedText { get; }
        public Image Image { get; }

        public override string ToString()
        {
            return
                $"{nameof(PulsometerData)}: {PulsometerData}, {nameof(RecognizedText)}: {RecognizedText}, {nameof(Image)}: {Image}";
        }
    }
}