using NP.Utilities;
using System.ComponentModel;

namespace NP.DataContextSample
{
    public class TestVM : VMBase
    {

        #region TheStr Property
        private string _str;
        public string TheStr
        {
            get
            {
                return this._str;
            }
            set
            {
                if (this._str == value)
                {
                    return;
                }

                this._str = value;
                this.OnPropertyChanged(nameof(TheStr));
            }
        }
        #endregion TheStr Property

    }
}
