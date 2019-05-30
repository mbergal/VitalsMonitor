using System;
using Monitor.Services;

namespace Monitor.Windows.SelectWindow
{
    public class Effects
    {
        private readonly WindowService windowService;
        private ScreenshotService screenshotService;

        public Effects(WindowService windowService, ScreenshotService screenshotService)
        {
            this.windowService = windowService;
            this.screenshotService = screenshotService;
        }

        public void LoadWindows(Action<Action<Model>> dispatch)
        {
            dispatch(model => model.LoadingStarted());
            var windows = windowService.ListWindows();
            dispatch(model => model.LoadingFinished(windows));
        }

        public void LoadScreenshot(Action<Action<SelectWindow.Model>> dispatch,
            WindowService.Window window)
        {
            dispatch(model => model.LoadingScreehshotStarted());
            var screenshot = this.screenshotService.TakeScreenshot(window.WindowHandle);
            dispatch(model => model.LoadingScreehshotFinished(screenshot));
        }
    }
}