using DevExpress.XtraEditors;
using System.ComponentModel;
using System.Drawing;

namespace TogiSoft.DataControls
{
    /// <summary> Базовый класс UserControl </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(XtraUserControl))]
    [Description("Стандартный компонент XtraUserControl.")]
    public class TogiUserControl : XtraUserControl
    {
        private byte updateDepth;
        protected bool IsLockUpdating { get { return updateDepth > 0; } }

        protected void BeginUpdate()
        {
            OnBeginUpdate();
        }
        protected void EndUpdate()
        {
            OnEndUpdate();
        }
        protected virtual void OnBeginUpdate()
        {
            updateDepth++;
        }
        protected virtual void OnEndUpdate()
        {
            if (updateDepth > 0) updateDepth--;
        }
    }
}
