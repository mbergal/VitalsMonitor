using System;
using System.Drawing;
using System.Threading;
using Accord.Video;

namespace Monitor.Services
{
    /// <summary>Screen capture video source.</summary>
    /// <remarks><para>The video source constantly captures the desktop screen.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // get entire desktop area size
    /// Rectangle screenArea = Rectangle.Empty;
    /// foreach ( System.Windows.Forms.Screen screen in
    ///           System.Windows.Forms.Screen.AllScreens )
    /// {
    ///     screenArea = Rectangle.Union( screenArea, screen.Bounds );
    /// }
    /// 
    /// // create screen capture video source
    /// ScreenCaptureStream stream = new ScreenCaptureStream( screenArea );
    /// 
    /// // set NewFrame event handler
    /// stream.NewFrame += new NewFrameEventHandler( video_NewFrame );
    /// 
    /// // start the video source
    /// stream.Start( );
    /// 
    /// // ...
    /// // signal to stop
    /// stream.SignalToStop( );
    /// // ...
    /// 
    /// private void video_NewFrame( object sender, NewFrameEventArgs eventArgs )
    /// {
    ///     // get new frame
    ///     Bitmap bitmap = eventArgs.Frame;
    ///     // process the frame
    /// }
    /// </code>
    /// </remarks>
    public class WindowCaptureStream : IVideoSource
    {
        private int frameInterval = 100;
        private int framesReceived;
        private Thread thread;
        private ManualResetEvent stopEvent;
        private IntPtr windowHandle;
        private ScreenshotService screenshotService;

        /// <summary>New frame event.</summary>
        /// <remarks><para>Notifies clients about new available frame from video source.</para>
        /// 
        /// <para><note>Since video source may have multiple clients, each client is responsible for
        /// making a copy (cloning) of the passed video frame, because the video source disposes its
        /// own original copy after notifying of clients.</note></para>
        /// </remarks>
        public event NewFrameEventHandler NewFrame;

        /// <summary>Video source error event.</summary>
        /// <remarks>This event is used to notify clients about any type of errors occurred in
        /// video source object, for example internal exceptions.</remarks>
        public event VideoSourceErrorEventHandler VideoSourceError;

        /// <summary>Video playing finished event.</summary>
        /// <remarks><para>This event is used to notify clients that the video playing has finished.</para>
        /// </remarks>
        public event PlayingFinishedEventHandler PlayingFinished;

        /// <summary>Video source.</summary>
        public virtual string Source
        {
            get { return "Window Capture"; }
        }


        /// <summary>Time interval between making screen shots, ms.</summary>
        /// <remarks><para>The property specifies time interval in milliseconds between consequent screen captures.
        /// Expected frame rate of the stream should be approximately 1000/FrameInteval.</para>
        /// 
        /// <para>If the property is set to 0, then the stream will capture screen as fast as the system allows.</para>
        /// 
        /// <para>Default value is set to <b>100</b>.</para>
        /// </remarks>
        public int FrameInterval
        {
            get { return this.frameInterval; }
            set { this.frameInterval = System.Math.Max(0, value); }
        }

        /// <summary>Received frames count.</summary>
        /// <remarks>Number of frames the video source provided from the moment of the last
        /// access to the property.
        /// </remarks>
        public int FramesReceived
        {
            get
            {
                int framesReceived = this.framesReceived;
                this.framesReceived = 0;
                return framesReceived;
            }
        }

        /// <summary>Received bytes count.</summary>
        /// <remarks><para><note>The property is not implemented for this video source and always returns 0.</note></para>
        /// </remarks>
        public long BytesReceived
        {
            get { return 0; }
        }

        /// <summary>State of the video source.</summary>
        /// <remarks>Current state of video source object - running or not.</remarks>
        public bool IsRunning
        {
            get
            {
                if (this.thread != null)
                {
                    if (!this.thread.Join(0))
                        return true;
                    this.Free();
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AForge.Video.ScreenCaptureStream" /> class.
        /// </summary>
        /// <param name="region">Screen's rectangle to capture (the rectangle may cover multiple displays).</param>
        public WindowCaptureStream(IntPtr windowHandle)
        {
            this.windowHandle = windowHandle;
            this.screenshotService = new ScreenshotService();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AForge.Video.ScreenCaptureStream" /> class.
        /// </summary>
        /// <param name="region">Screen's rectangle to capture (the rectangle may cover multiple displays).</param>
        /// <param name="frameInterval">Time interval between making screen shots, ms.</param>
        public WindowCaptureStream(IntPtr windowHandle, int frameInterval)
        {
            this.windowHandle = windowHandle;
            this.FrameInterval = frameInterval;
        }

        /// <summary>Start video source.</summary>
        /// <remarks>Starts video source and return execution to caller. Video source
        /// object creates background thread and notifies about new frames with the
        /// help of <see cref="E:AForge.Video.ScreenCaptureStream.NewFrame" /> event.</remarks>
        /// <exception cref="T:System.ArgumentException">Video source is not specified.</exception>
        public void Start()
        {
            if (this.IsRunning)
                return;
            this.framesReceived = 0;
            this.stopEvent = new ManualResetEvent(false);
            this.thread = new Thread(new ThreadStart(this.WorkerThread));
            this.thread.Name = this.Source;
            this.thread.Start();
        }

        /// <summary>Signal video source to stop its work.</summary>
        /// <remarks>Signals video source to stop its background thread, stop to
        /// provide new frames and free resources.</remarks>
        public void SignalToStop()
        {
            if (this.thread == null)
                return;
            this.stopEvent.Set();
        }

        /// <summary>Wait for video source has stopped.</summary>
        /// <remarks>Waits for source stopping after it was signalled to stop using
        /// <see cref="M:AForge.Video.ScreenCaptureStream.SignalToStop" /> method.</remarks>
        public void WaitForStop()
        {
            if (this.thread == null)
                return;
            this.thread.Join();
            this.Free();
        }

        /// <summary>Stop video source.</summary>
        /// <remarks><para>Stops video source aborting its thread.</para>
        /// 
        /// <para><note>Since the method aborts background thread, its usage is highly not preferred
        /// and should be done only if there are no other options. The correct way of stopping camera
        /// is <see cref="M:AForge.Video.ScreenCaptureStream.SignalToStop">signaling it stop</see> and then
        /// <see cref="M:AForge.Video.ScreenCaptureStream.WaitForStop">waiting</see> for background thread's completion.</note></para>
        /// </remarks>
        public void Stop()
        {
            if (!this.IsRunning)
                return;
            this.stopEvent.Set();
            this.thread.Abort();
            this.WaitForStop();
        }

        /// <summary>Free resource.</summary>
        private void Free()
        {
            this.thread = (Thread) null;
            this.stopEvent.Close();
            this.stopEvent = (ManualResetEvent) null;
        }

        private void WorkerThread()
        {
            while (!this.stopEvent.WaitOne(0, false))
            {
                DateTime now = DateTime.Now;
                try
                {
                    var frame = screenshotService.TakeScreenshot(windowHandle);
                    ++this.framesReceived;
                    if (this.NewFrame != null)
                        this.NewFrame((object) this, new NewFrameEventArgs(new Bitmap(frame)));
                    if (this.frameInterval > 0)
                    {
                        int millisecondsTimeout =
                            this.frameInterval - (int) DateTime.Now.Subtract(now).TotalMilliseconds;
                        if (millisecondsTimeout > 0)
                        {
                            if (this.stopEvent.WaitOne(millisecondsTimeout, false))
                                break;
                        }
                    }
                }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                catch (ThreadAbortException ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (this.VideoSourceError != null)
                        this.VideoSourceError((object) this,
                            new VideoSourceErrorEventArgs(ex.Message));
                    Thread.Sleep(250);
                }

                if (this.stopEvent.WaitOne(0, false))
                    break;
            }

            if (this.PlayingFinished == null)
                return;
            this.PlayingFinished((object) this, ReasonToFinishPlaying.StoppedByUser);
        }
    }
}