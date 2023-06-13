using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace TogiSoft.Program.Core.Controls
{
    /// <summary>
    /// Вспомогательный класс для создания всплывающих control'ов
    /// </summary>
    /// <remarks> Используется при работе ProgramCore.Show </remarks>
    public static class ControlBox
    {
        /// <summary>
        /// Отступы для кнопок
        /// </summary>
        private const int margin = 4;

        /// <summary>
        /// Создать форму с control'ом и показать её
        /// </summary>
        /// <param name="control"> Control </param>
        /// <param name="caption"> Заголвоок формы </param>
        /// <param name="buttons"> Кнопки по умолчанию на форме </param>
        /// <param name="defaultButton"> Выбранная кнопка </param>
        /// <param name="canResize"> Возможность изменения размера </param>
        /// <param name="focusedControl"> (?) </param>
        /// <param name="useAcceptCancel"> Использовать стандартные кнопки принятия и отмены </param>
        /// <param name="formClosingHandler"> Обработчик закрытия окна </param>
        /// <returns> Результат после закрытия формы пользователем </returns>
        public static DialogResult Show(
            Control control,
            string caption,
            MessageBoxButtons buttons,
            int defaultButton,
            bool canResize,
            Control focusedControl = null,
            bool useAcceptCancel = true,
            FormClosingEventHandler formClosingHandler = null)
        {
            int btnHeight;
            using (var btn = CreateButton("test", DialogResult.None))
            {
                btnHeight = btn.Height;
            }

            var box = new frmControlBox
            {
                Text = caption,
                FormBorderStyle = canResize ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog,
                FocusedControl = focusedControl
            };
            if (canResize)
            {
                box.MaximizeBox = true;
            }

            if (formClosingHandler != null)
            {
                box.FormClosing += formClosingHandler;
            }

            if (!control.MinimumSize.IsEmpty)
            {
                box.MinimumSize = new Size(control.MinimumSize.Width + (2 * margin), control.MinimumSize.Height + btnHeight + (3 * margin)) + (box.Size - box.ClientSize);
            }

            if (!control.MaximumSize.IsEmpty)
            {
                box.MaximumSize = new Size(control.MaximumSize.Width + (2 * margin), control.MaximumSize.Height + btnHeight + (3 * margin)) + (box.Size - box.ClientSize);
            }

            box.ClientSize = new Size(control.Width + (2 * margin), control.Height + btnHeight + (3 * margin));
            control.Location = new Point(margin, margin);
            if (control.Dock != DockStyle.Fill)
            {
                control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }

            AppendButtons(box, control, buttons, defaultButton, useAcceptCancel: useAcceptCancel);
            control.Parent = box;
            box.FormClosing += Box_FormClosing;
            return box.ShowDialog();
        }

        /// <summary>
        /// Добавить кнопки на форму
        /// </summary>
        /// <param name="box"> Форма </param>
        /// <param name="buttons"> Кнопки </param>
        /// <param name="defaultButton"> Кнопка по умолчанию </param>
        /// <param name="useAcceptCancel"> Использовать стандартные кнопки принятия и отмены </param>
        private static void AddButtons(Form box, BaseControl[] buttons, int defaultButton, bool useAcceptCancel = true)
        {
            double v;
            buttons = (from button in buttons
                       orderby (button.Tag != null && double.TryParse(button.Tag.ToString(), out v) ? double.Parse(button.Tag.ToString()) : 1 + double.Epsilon)
                       select button).ToArray();

            var left = box.ClientSize.Width - margin - (box.FormBorderStyle == FormBorderStyle.Sizable || box.FormBorderStyle == FormBorderStyle.SizableToolWindow ? 10 : 0);
            var right = box.Left + margin + (box.FormBorderStyle == FormBorderStyle.Sizable || box.FormBorderStyle == FormBorderStyle.SizableToolWindow ? 10 : 0);
            for (var i = buttons.Length - 1; i >= 0; i--)
            {
                var order = buttons[i].Tag != null && double.TryParse(buttons[i].Tag.ToString(), out v) && v < 0 ? v : 0;
                if (order < 0)
                {
                    buttons[i].Location = new Point(right, box.ClientSize.Height - buttons[i].Height - margin);
                    buttons[i].Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                    buttons[i].Parent = box;
                    right += buttons[i].Width + margin;
                }
                else
                {
                    buttons[i].Location = new Point(left - buttons[i].Width, box.ClientSize.Height - buttons[i].Height - margin);

                    buttons[i].Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    buttons[i].Parent = box;
                    left -= buttons[i].Width + margin;
                }
            }

            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].TabIndex = i + 1;
            }

            if (useAcceptCancel)
            {
                if (defaultButton >= 0 && defaultButton < buttons.Length && (buttons[defaultButton] as SimpleButton)?.DialogResult != DialogResult.None)
                {
                    box.AcceptButton = buttons[defaultButton] as SimpleButton;
                }
            }

            box.ClientSize = new Size(Math.Max(box.ClientSize.Width - left + (2 * margin), box.ClientSize.Width), box.ClientSize.Height);
        }

        /// <summary>
        /// Создать простую кнопку
        /// </summary>
        /// <param name="text"> Подпись кнопки </param>
        /// <param name="dialogResult"> Результат нажатия </param>
        /// <param name="order"> Порядковый индекс </param>
        /// <returns> Кнопка </returns>
        private static SimpleButton CreateButton(string text, DialogResult dialogResult, double order = 0)
        {
            var result = new SimpleButton { Tag = order, Text = text, DialogResult = dialogResult };
            return result;
        }

        /// <summary>
        /// Получить список кнопок с модификатором public, встречаемые на control'е
        /// </summary>
        /// <param name="control"> Control </param>
        /// <returns> Список кнопок </returns>
        private static List<BaseControl> GetControlElements(Control control)
        {
            var fields = control.GetType().GetFields();

            var buttons = new List<BaseControl>();
            foreach (var field in fields)
            {
                if (!field.IsPublic)
                {
                    continue;
                }

                var button = field.GetValue(control) as BaseControl;
                if (button != null)
                {
                    buttons.Add(button);
                }
            }

            return buttons;
        }

        /// <summary>
        /// Добавить кнопку на форму
        /// </summary>
        /// <param name="controlButtons"> Кнопки на форме </param>
        /// <param name="text"> Заголовок новой кнопки </param>
        /// <param name="dialogResult"> Результат нажатия на кнопку </param>
        /// <param name="order"> Порядковый индекс </param>
        private static void AppendButton(List<BaseControl> controlButtons, string text, DialogResult dialogResult, double order)
        {
            if (!controlButtons.Exists(btn => (btn as SimpleButton)?.DialogResult == dialogResult))
            {
                controlButtons.Add(CreateButton(text, dialogResult, order));
            }
        }

        /// <summary>
        /// Добавить кнопки на форму
        /// </summary>
        /// <param name="box"> Форма </param>
        /// <param name="control"> Control </param>
        /// <param name="buttons"> Кнопки по умолчанию </param>
        /// <param name="defaultButton"> Активная кнопка </param>
        /// <param name="useAcceptCancel"> Использовать стандартные кнопки принятия и отмены </param>
        private static void AppendButtons(Form box, Control control, MessageBoxButtons buttons, int defaultButton, bool useAcceptCancel = true)
        {
            var controlElements = GetControlElements(control);
            SimpleButton cancelButton = null;
            var oKpresent = false;
            foreach (var element in controlElements)
            {
                var button = element as SimpleButton;
                if (button == null)
                {
                    continue;
                }

                if (button.DialogResult != DialogResult.OK)
                {
                    continue;
                }

                oKpresent = true;
                break;
            }

            var dialogResult = DialogResult.Cancel;
            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    AppendButton(controlElements, "Прервать", DialogResult.Abort, 1 + double.Epsilon);
                    AppendButton(controlElements, "Повторить", DialogResult.Retry, 2 + double.Epsilon);
                    AppendButton(controlElements, "Игнорировать", DialogResult.Ignore, 3 + double.Epsilon);
                    dialogResult = DialogResult.Ignore;
                    break;
                case MessageBoxButtons.OK:
                    if (oKpresent)
                    {
                        AppendButton(controlElements, "Закрыть", DialogResult.Cancel, 1 + double.Epsilon);
                    }
                    else
                    {
                        AppendButton(controlElements, "ОК", DialogResult.OK, 1 + double.Epsilon);
                    }

                    break;
                case MessageBoxButtons.OKCancel:
                    AppendButton(controlElements, "ОК", DialogResult.OK, 1 + double.Epsilon);
                    AppendButton(controlElements, "Отменить", DialogResult.Cancel, 2 + double.Epsilon);
                    break;
                case MessageBoxButtons.RetryCancel:
                    AppendButton(controlElements, "Повторить", DialogResult.Retry, 1 + double.Epsilon);
                    AppendButton(controlElements, "Отменить", DialogResult.Cancel, 2 + double.Epsilon);
                    break;
                case MessageBoxButtons.YesNo:
                    AppendButton(controlElements, "Да", DialogResult.Yes, 1 + double.Epsilon);
                    AppendButton(controlElements, "Нет", DialogResult.No, 2 + double.Epsilon);
                    dialogResult = DialogResult.No;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    AppendButton(controlElements, "Да", DialogResult.Yes, 1 + double.Epsilon);
                    AppendButton(controlElements, "Нет", DialogResult.No, 2 + double.Epsilon);
                    AppendButton(controlElements, "Отменить", DialogResult.Cancel, 3 + double.Epsilon);
                    break;
            }

            cancelButton = controlElements.FirstOrDefault(x => (x as SimpleButton)?.DialogResult == dialogResult) as SimpleButton;
            AddButtons(box, controlElements.ToArray(), defaultButton, useAcceptCancel: useAcceptCancel);

            if (cancelButton != null)
            {
                box.CancelButton = cancelButton;
            }
        }

        /// <summary>
        /// Событие на уничтожение формы
        /// </summary>
        /// <param name="sender"> Источник </param>
        /// <param name="e"> Параметры </param>
        private static void box_Disposed(object sender, EventArgs e)
        {
            Control form;
            if (sender is SimpleButton)
            {
                form = (sender as SimpleButton).Parent;
            }
            else
            {
                form = sender as frmControlBox;
            }

            if (form != null)
            {
                form.Disposed -= box_Disposed;
                form.Dispose();
            }
        }

        /// <summary>
        /// Событие на подтверждение закрытия формы
        /// </summary>
        /// <param name="sender"> Источник </param>
        /// <param name="e"> Параметры </param>
        private static void Box_FormClosing(object sender, FormClosingEventArgs e)
        {
            var form = sender as frmControlBox;
            if (form.DialogResult == DialogResult.OK || form.DialogResult == DialogResult.Yes)
            {
                return;
            }

            var allowClose = true;
            if (allowClose)
            {
                form.FormClosing -= Box_FormClosing;
            }

            e.Cancel = !allowClose;
        }
    }
}