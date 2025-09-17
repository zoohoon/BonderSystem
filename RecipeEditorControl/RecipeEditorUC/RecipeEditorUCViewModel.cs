using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipeEditorControl.RecipeEditorUC
{
    using LogModule;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class RecipeEditorUCViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Unit list

        // �ʿ信 ���� ���Ǵ� String�� �߰��� ��
        // RecipeEditorParamEditViewModel�� ���� �߰������ ��.
        public static readonly IList<String> AllowedUnitStringlist = new ReadOnlyCollection<string>
        (new List<String> 
        {
            "ms",
            "millisecond",
            "sec",
            "second",
            "min",
            "minute",
            "hour",
            "um",
            "micron",
            "percent",
            "%",
            "pixel",
            "��C"
        });

        #endregion

        #region ==> Caption
        private String _Caption;
        public String Caption
        {
            get { return _Caption; }
            set
            {
                if (value != _Caption)
                {
                    _Caption = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public IElement Elem { get; set; }
        public Object ValueBuffer { get; set; }

        private void SetCaptionAddUnit(String ElemValueStr)
        {
            String retval = String.Empty;

            try
            {
                bool isValidUnit = false;

                if (Elem.Unit != "EMPTY" && Elem.Unit != null)
                {
                    // TODO : ��ҹ��� �ϰ� Ȯ���� �� �ֵ���

                    var exist = AllowedUnitStringlist.FirstOrDefault(x => x == Elem.Unit);

                    if (exist != null)
                    {
                        if(Elem.Unit == "micron")
                        {
                            Elem.Unit = "um";
                        }

                        isValidUnit = true;
                    }
                }

                if (isValidUnit == false)
                {
                    Caption = $"{ElemValueStr}";
                }
                else
                {
                    Caption = $"{ElemValueStr}" + " " + $"{Elem.Unit}";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public RecipeEditorUCViewModel(IElement elem)
        {
            Elem = elem;

            SetCaptionAddUnit(elem.GetValue().ToString());
        }
        public bool FlushValueBuffer()
        {
            if (ValueBuffer == null)
                return false;

            //if (Elem.SetValue(ValueBuffer) == false)
            if (Elem.SetValue(ValueBuffer) == ProberErrorCode.EventCodeEnum.UNDEFINED)
                return false;

            ValueBuffer = null;

            return true;
        }

        public bool FlushMaxValueBuffer()
        {
            //bool result = true;
            if (ValueBuffer == null)
                return false;

            //if (Elem.SetValue(ValueBuffer) == false)
            Elem.UpperLimit = double.Parse(ValueBuffer.ToString());
            ValueBuffer = null;

            return true;
        }

        public bool FlushMinValueBuffer()
        {
            //bool result = true;
            if (ValueBuffer == null)
                return false;

            //if (Elem.SetValue(ValueBuffer) == false)
            Elem.LowerLimit = double.Parse(ValueBuffer.ToString());
            ValueBuffer = null;

            return true;
        }

        public void SetElemValueBuffer(Object valueBuffer)
        {
            String elemValueStr = Elem.GetValue().ToString();
            String valueBufferStr = valueBuffer.ToString();

            if (elemValueStr == valueBufferStr)
            {
                ValueBuffer = null;
            }
            else
            {
                ValueBuffer = valueBuffer;

                // Unit(String)�� ���� ���������� �����Ǿ� �ִ� ��� Caption�� Unit�� �߰��Ѵ�.
                SetCaptionAddUnit(valueBufferStr);
            }
        }

        public void SetElemLimitValueBuffer(Object valueBuffer)
        {
            String elemValueStr = Elem.UpperLimit.ToString();
            String valueBufferStr = valueBuffer.ToString();

            if (elemValueStr == valueBufferStr)
            {
                ValueBuffer = null;
            }
            else
            {
                ValueBuffer = valueBuffer;

                // Unit(String)�� ���� ���������� �����Ǿ� �ִ� ��� Caption�� Unit�� �߰��Ѵ�.
                //SetCaptionAddUnit(valueBufferStr);
            }
        }
    }
}

