using System;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace TogiSoft.Program.Core.Controls
{
    /// <summary>
    /// Форма для показа Show и ShowDialog control'ов программы
    /// </summary>
    public partial class frmControlBox : XtraForm
    {
        /// <summary>
        /// Беспараметрический конструктор
        /// </summary>
        public frmControlBox()
        {
            InitializeComponent();
            NeedDispose = false;
        }

        /// <summary>
        /// необходимо ли уничтожать окно при акрытии, по умолчанию такой необходимости нет, но если мы показываем плавающее окно, то надо прибить
        /// </summary>
        public bool NeedDispose { get; set; }

        /// <summary>Элемент формы автоматически получающий фокус </summary>
        public Control FocusedControl { get; set; }

        /// <summary>
        /// Событие на закрытие формы
        /// </summary>
        /// <param name="e"> Параметры </param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (NeedDispose)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Событие на пред обработку нажатой клавиши клавиатуры
        /// </summary>
        /// <param name="msg"> Message </param>
        /// <param name="keyData"> Клавиша </param>
        /// <returns> Обработано </returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Событие на уничтожение формы
        /// </summary>
        /// <param name="disposing"> Форма уничтожается </param>
        protected override void Dispose(bool disposing)
        {
            foreach (Control control in Controls)
            {
                control.Dispose();
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Событие на показ формы
        /// </summary>
        /// <param name="sender"> Источник </param>
        /// <param name="e"> Параметры </param>
        private void frmControlBox_Shown(object sender, EventArgs e)
        {
            if (FocusedControl == null)
            {
                var control = Controls.Cast<Control>().FirstOrDefault(c => !(c is SimpleButton));
                control?.Focus();
                return;
            }

            FocusedControl.Focus();
        }
    }
}