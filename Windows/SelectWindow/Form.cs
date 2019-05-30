using System;
using System.Linq;
using Monitor.Services;

namespace Monitor.Windows.SelectWindow
{
    public partial class Form : System.Windows.Forms.Form, IForm<Model>
    {
        private Mediator<Form, Model, Effects> mediator;
        private string[] lastListboxItems;

        public Form(ScreenshotService screenshotService, WindowService windowService)
            : base()
        {
            this.Model = new Model(new Lazy<Effects>(() => this.Effects))
            {
                Loading = true
            };
            this.Effects = new SelectWindow.Effects(windowService, screenshotService);

            InitializeComponent();

            this.listBox1.SelectedIndexChanged += (sender, args) =>
            {
                this.Model.SelectionChanged(this.listBox1.SelectedIndex);
                mediator.EndTick();
            };

            this.mediator = new Mediator<Form, Model, Effects>(this, Model, Effects);

            mediator.EndTick();
        }

        public Effects Effects { get; set; }

        public Model Model { get; set; }


        public WindowService.Window Result { get; private set; }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Result = this.Model.SelectedWindow;
            this.Close();
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        public void SyncUI(Model model)
        {
            this.loadingLabel.Visible = this.Model.LoadingLabelVisible;
            this.listBox1.Visible = this.Model.ListBoxVisible;
            if (this.lastListboxItems == null ||
                !lastListboxItems.SequenceEqual(this.Model.WindowTitles))
            {
                this.lastListboxItems = this.Model.WindowTitles;
                this.listBox1.Items.Clear();
                this.listBox1.Items.AddRange(this.Model.WindowTitles);
            }

            if (this.Model.VideoSource != null)
            {
                this.videoSourcePlayer1.VideoSource = this.Model.VideoSource;
                this.videoSourcePlayer1.Start();
            }
        }

        private void Label1_Click(object sender, EventArgs e)
        {
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
        }
    }
}