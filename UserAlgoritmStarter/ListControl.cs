using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserAlgoritmStarter
{
    /// <summary>
    /// Control содержащий label и ComboBox для выбора значений
    /// </summary>
    public partial class ListControl : UserControl
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        public ListControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализация с заданием значений для элементов Label и ComboBox
        /// </summary>
        /// <param name="name"> Текст для label </param>
        /// <param name="elements"> Элементы для ComboBox </param>
        public ListControl(string name, JsonElement elements)
        {
            InitializeComponent();
            this.labelControl1.Text = name;
            foreach (var item in elements.EnumerateArray())
            {
                this.comboBoxEdit1.Properties.Items.Add(item);
            }

            var itemsCount = comboBoxEdit1.Properties.Items.Count - 1;
            if (itemsCount != 0)
            {
                this.comboBoxEdit1.EditValue = comboBoxEdit1.Properties.Items[0];
            }
        }

        /// <summary>
        /// Метод для получения текста labelControl
        /// </summary>
        /// <returns> Возвращает labelControl1.Text </returns>
        public string GetLabelText()
        {
            return labelControl1.Text;
        }

        /// <summary>
        /// Метод для получения выбранного в ComboBox элемента
        /// </summary>
        /// <returns> Возвращает comboBoxEdit1.Text </returns>
        public string GetComboText()
        {
            return comboBoxEdit1.Text;
        }
    }
}