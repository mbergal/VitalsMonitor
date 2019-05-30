using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Monitor.Windows.MainWindow;
using NLog;

namespace Monitor.Windows
{
    public class Mediator<F, M, E> where F : Form, IForm<M>
    {
        private static readonly Logger logger = LogManager.GetLogger("Mediator");

        public Mediator(F form, M model, E effects)
        {
            this.Form = form;
            this.Model = model;
            this.Effects = effects;
        }

        public M Model { get; set; }

        public E Effects { get; set; }

        public F Form { get; set; }

        public void EndTick()
        {
            Form.SyncUI(Model);

            if (Model is IWithEffect<M> withEffect && withEffect.Effect != null)
            {
                var effect = withEffect.Effect;
                withEffect.Effect = null;
                InvokeEffect(effect);
            }
        }


        private void InvokeEffect(Action<Action<Action<M>>> effect)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {
                try
                {
                    effect(x =>
                    {
                        while (!this.Form.IsHandleCreated)
                        {
                            Thread.Sleep(100);
                        }

                        this.Form.Invoke(x, this.Model);
                        this.Form.Invoke((MethodInvoker) this.EndTick);
                    });
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            };
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
//                throw new NotImplementedException();
            }
        }
    }
}