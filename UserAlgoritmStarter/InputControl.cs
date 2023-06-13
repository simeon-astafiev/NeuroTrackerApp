using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TogiSoft.DataControls;

namespace UserAlgoritmStarter
{
    /// <summary>
    /// Control содержащий label и SpinEdit для введения значений
    /// </summary>
    public partial class InputControl : TogiUserControl
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        public InputControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализация с заданием значений для элементов Label и SpinEdit
        /// </summary>
        /// <param name="name"> Текст для Label </param>
        /// <param name="num"> Число для SpinEdit </param>
        public InputControl(string name, decimal num)
        {
            InitializeComponent();
            this.labelControl1.Text = name;
            this.spinEdit1.Value = num;
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
        /// Метод для получения числа из SpinEdit
        /// </summary>
        /// <returns> Возвращает spinEdit1.Value </returns>
        public decimal GetSpinValue()
        {
            return spinEdit1.Value;
        }
    }
}