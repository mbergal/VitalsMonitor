using System;
using System.Drawing;
using System.Linq;
using Accord.Video;
using Monitor.Services;

namespace Monitor.Windows.SelectWindow
{
    public class Model : ModelWithEffect<Model>
    {
        private readonly Lazy<Effects> _effects;

        public Model(Lazy<SelectWindow.Effects> effects)
        {
            this._effects = effects;
            this.Effect = dispatch => effects.Value.LoadWindows(dispatch);
        }

        public void LoadingStarted()
        {
            this.Loading = false;
        }

        public void LoadingFinished(WindowService.Window[] windows)
        {
            this.Loading = false;
            this.Windows = windows;
        }


        public void SelectionChanged(int listBox1SelectedIndex)
        {
            this.SelectedWindow = this.Windows[listBox1SelectedIndex];
            this.Effect = dispatch =>
                this._effects.Value.LoadScreenshot(dispatch, this.SelectedWindow);
            this.VideoSource = new WindowCaptureStream(this.SelectedWindow.WindowHandle)
            {
                FrameInterval = 1000
            };
        }

        public bool Loading { get; set; }

        public bool LoadingLabelVisible => Loading;
        public bool ListBoxVisible => !Loading;
        public WindowService.Window[] Windows { get; set; }

        public string[] WindowTitles
        {
            get
            {
                return Windows != null
                    ? Windows.Select(x => x.WindowTitle).ToArray()
                    : new string[] { };
            }
        }

        public WindowService.Window SelectedWindow { get; set; }

        public void LoadingScreehshotStarted()
        {
            this.LoadingScreenshot = true;
        }

        public void LoadingScreehshotFinished(Image screenshot)
        {
            this.LoadingScreenshot = false;
            this.Screenshot = screenshot;
        }

        public Image Screenshot { get; set; }

        public bool LoadingScreenshot { get; set; }
        public IVideoSource VideoSource { get; set; }
    }
}