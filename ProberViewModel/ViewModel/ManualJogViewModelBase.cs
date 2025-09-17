using Autofac;
using CylType;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VirtualKeyboardControl;
using LoaderControllerBase;
using SubstrateObjects;
using ProberInterfaces.PnpSetup;
using MetroDialogInterfaces;
using ProberInterfaces.State;
using ProberViewModel.Data;
//using ProberInterfaces.ThreadSync;

namespace ManualJogViewModel
{
    public class AxisObjectVM : INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    public class MotionText : IEquatable<MotionText>
    {

        public int MotionTextvalue { get; set; }

        public override string ToString()
        {
            return "Value: " + MotionTextvalue;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            MotionText objAsPart = obj as MotionText;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return MotionTextvalue;
        }
        public bool Equals(MotionText other)
        {
            if (other == null) return false;
            return (this.MotionTextvalue.Equals(other.MotionTextvalue));
        }
    }
    public class ManualJogViewModelBase : IMainScreenViewModel, IFactoryModule, INotifyPropertyChanged, ISetUpState
    {
        readonly Guid _ViewModelGUID = new Guid("A9796E36-D6D8-6EA1-349B-6E5E30A90E68");
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        public ILoaderControllerExtension LoaderController { get; set; }
        public bool Initialized { get; set; } = false;


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private StageState _StageMove;
        public StageState StageMove
        {
            get { return _StageMove; }
            set { _StageMove = value; }
        }
        private bool _StageButtonsVisibility = true;
        public bool StageButtonsVisibility
        {
            get { return _StageButtonsVisibility; }
            set
            {
                if (value != _StageButtonsVisibility)
                {
                    _StageButtonsVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<StageCamera> _StageCamList = new ObservableCollection<StageCamera>();
        public ObservableCollection<StageCamera> StageCamList
        {
            get { return _StageCamList; }
            set
            {
                if (value != _StageCamList)
                {
                    _StageCamList = value;
                    RaisePropertyChanged();
                }
            }
        }
        public WaferObject Wafer { get { return this.StageSupervisor().WaferObject as WaferObject; } }

        private enumStageCamType _SelectedCam;
        public enumStageCamType SelectedCam
        {
            get { return _SelectedCam; }
            set
            {
                if (value != _SelectedCam)
                {
                    _SelectedCam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IStage3DModel _Stage3DModel;
        public IStage3DModel Stage3DModel
        {
            get { return _Stage3DModel; }
            set
            {
                if (value != _Stage3DModel)
                {
                    _Stage3DModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region // Properties
        private ObservableCollection<IOPortDescripter<bool>> _OutputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> OutputPorts
        {
            get { return _OutputPorts; }
            set
            {
                if (value != _OutputPorts)
                {
                    _OutputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<IOPortDescripter<bool>> _InputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> InputPorts
        {
            get { return _InputPorts; }
            set
            {
                if (value != _InputPorts)
                {
                    _InputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private LockKey outPortLock = new LockKey("Manual jog VM - out port");
        private object outPortLock = new object();

        private ObservableCollection<IOPortDescripter<bool>> _FilteredOutputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> FilteredOutputPorts
        {
            get { return _FilteredOutputPorts; }
            set
            {
                if (value != _FilteredOutputPorts)
                {
                    _FilteredOutputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private LockKey inPortLock = new LockKey("Manual jog VM - in port");
        private object inPortLock = new object();

        private ObservableCollection<IOPortDescripter<bool>> _FilteredInputPorts
            = new ObservableCollection<IOPortDescripter<bool>>();
        public ObservableCollection<IOPortDescripter<bool>> FilteredInputPorts
        {
            get { return _FilteredInputPorts; }
            set
            {
                if (value != _FilteredInputPorts)
                {
                    _FilteredInputPorts = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _SearchKeyword = string.Empty;
        public string SearchKeyword
        {
            get { return _SearchKeyword; }
            set
            {
                if (value != _SearchKeyword)
                {
                    _SearchKeyword = value;
                    RaisePropertyChanged();
                    SearchMatched();
                }
            }
        }


        private int _LightValue;
        public int LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                    UpdateLight();
                }
            }
        }
        //private int _SelectedLightChannel;
        //public int SelectedLightChannel
        //{
        //    get { return _SelectedLightChannel; }
        //    set
        //    {
        //        if (value != _SelectedLightChannel)
        //        {
        //            _SelectedLightChannel = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<LightChannelType> _Lights
            = new ObservableCollection<LightChannelType>();
        public ObservableCollection<LightChannelType> Lights
        {
            get { return _Lights; }
            set
            {
                if (value != _Lights)
                {
                    _Lights = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<CameraChannelType> _CamChannels = new ObservableCollection<CameraChannelType>();
        public ObservableCollection<CameraChannelType> CamChannels
        {
            get { return _CamChannels; }
            set
            {
                if (value != _CamChannels)
                {
                    _CamChannels = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CameraChannelType _SelectedChannel;
        public CameraChannelType SelectedChannel
        {
            get { return _SelectedChannel; }
            set
            {
                if (value != _SelectedChannel)
                {
                    _SelectedChannel = value;
                    RaisePropertyChanged();
                }
            }
        }


        private LightChannelType _SelectedLight;
        public LightChannelType SelectedLight
        {
            get { return _SelectedLight; }
            set
            {
                if (value != _SelectedLight)
                {
                    _SelectedLight = value;
                    RaisePropertyChanged();
                }
            }
        }


        private RelayCommand _SearchTextChangedCommand;
        public ICommand SearchTextChangedCommand
        {
            get
            {
                if (null == _SearchTextChangedCommand) _SearchTextChangedCommand = new RelayCommand(SearchMatched);
                return _SearchTextChangedCommand;
            }
        }


        private RelayCommand<object> _ChannelChangeCommand;
        public ICommand ChannelChangeCommand
        {
            get
            {
                if (null == _ChannelChangeCommand) _ChannelChangeCommand = new RelayCommand<object>(ChangeChannel);
                return _ChannelChangeCommand;
            }
        }

        private bool _EnableTiltElement;
        public bool EnableTiltElement
        {
            get { return _EnableTiltElement; }
            set
            {
                if (value != _EnableTiltElement)
                {
                    _EnableTiltElement = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _RPosDist;
        public int RPosDist
        {
            get { return _RPosDist; }
            set
            {
                if (value != _RPosDist)
                {
                    _RPosDist = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _TTPosDist;
        public double TTPosDist
        {
            get { return _TTPosDist; }
            set
            {
                if (value != _TTPosDist)
                {
                    _TTPosDist = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private RelayCommand _OuputOffCommand;
        //public ICommand OuputOffCommand
        //{
        //    get
        //    {
        //        if (null == _OuputOffCommand) _OuputOffCommand = new RelayCommand(OuputOff);
        //        return _OuputOffCommand;
        //    }
        //}

        //private void OuputOff()
        //{
        //    throw new NotImplementedException();
        //}

        //private RelayCommand _OutputOnCommand;
        //public ICommand OutputOnCommand
        //{
        //    get
        //    {
        //        if (null == _OutputOnCommand) _OutputOnCommand = new RelayCommand(OutputOn);
        //        return _OutputOnCommand;
        //    }
        //}

        //private void OutputOn()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
        ILightAdmin light;
        public ManualJogViewModelBase()
        {
            SearchKeyword = "";
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private ObservableCollection<AxisObjectVM> _StageAxisObjectVmList
    = new ObservableCollection<AxisObjectVM>();
        public ObservableCollection<AxisObjectVM> StageAxisObjectVmList
        {
            get { return _StageAxisObjectVmList; }
            set
            {
                if (value != _StageAxisObjectVmList)
                {
                    _StageAxisObjectVmList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<AxisObjectVM> _LoaderAxisObjectVmList
            = new ObservableCollection<AxisObjectVM>();
        public ObservableCollection<AxisObjectVM> LoaderAxisObjectVmList
        {
            get { return _LoaderAxisObjectVmList; }
            set
            {
                if (value != _LoaderAxisObjectVmList)
                {
                    _LoaderAxisObjectVmList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ProbeAxisObject _axis;
        public ProbeAxisObject axis
        {
            get { return _axis; }
            set
            {
                if (value != _axis)
                {
                    _axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _RelMoveStepDist;
        public double RelMoveStepDist
        {
            get { return _RelMoveStepDist; }
            set
            {
                if (value != _RelMoveStepDist)
                {
                    _RelMoveStepDist = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ProbeAxisObject _AxisObject;
        public ProbeAxisObject AxisObject
        {
            get { return _AxisObject; }
            set
            {
                if (value != _AxisObject)
                {
                    _AxisObject = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _PosButtonVisibility = true;
        public bool PosButtonVisibility
        {
            get { return _PosButtonVisibility; }
            set
            {
                if (value != _PosButtonVisibility)
                {
                    _PosButtonVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _NegButtonVisibility = true;
        public bool NegButtonVisibility
        {
            get { return _NegButtonVisibility; }
            set
            {
                if (value != _NegButtonVisibility)
                {
                    _NegButtonVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region Move ButtonCommand

        #region Stage Move
        private AsyncCommand _XPosMoveCommand;
        public ICommand XPosMoveCommand
        {
            get
            {
                if (null == _XPosMoveCommand) _XPosMoveCommand = new AsyncCommand(XPosMoveFunc);
                return _XPosMoveCommand;
            }
        }
        private AsyncCommand _XNegMoveCommand;
        public ICommand XNegMoveCommand
        {
            get
            {
                if (null == _XNegMoveCommand) _XNegMoveCommand = new AsyncCommand(XNegMoveFunc);
                return _XNegMoveCommand;
            }
        }
        private AsyncCommand _YPosMoveCommand;
        public ICommand YPosMoveCommand
        {
            get
            {
                if (null == _YPosMoveCommand) _YPosMoveCommand = new AsyncCommand(YPosMoveFunc);
                return _YPosMoveCommand;
            }
        }
        private AsyncCommand _YNegMoveCommand;
        public ICommand YNegMoveCommand
        {
            get
            {
                if (null == _YNegMoveCommand) _YNegMoveCommand = new AsyncCommand(YNegMoveFunc);
                return _YNegMoveCommand;
            }
        }
        private AsyncCommand _ZPosMoveCommand;
        public ICommand ZPosMoveCommand
        {
            get
            {
                if (null == _ZPosMoveCommand) _ZPosMoveCommand = new AsyncCommand(ZPosMoveFunc);
                return _ZPosMoveCommand;
            }
        }
        private AsyncCommand _ZNegMoveCommand;
        public ICommand ZNegMoveCommand
        {
            get
            {
                if (null == _ZNegMoveCommand) _ZNegMoveCommand = new AsyncCommand(ZNegMoveFunc);
                return _ZNegMoveCommand;
            }
        }
        private AsyncCommand _CPosMoveCommand;
        public ICommand CPosMoveCommand
        {
            get
            {
                if (null == _CPosMoveCommand) _CPosMoveCommand = new AsyncCommand(CPosMoveFunc);
                return _CPosMoveCommand;
            }
        }
        private AsyncCommand _CNegMoveCommand;
        public ICommand CNegMoveCommand
        {
            get
            {
                if (null == _CNegMoveCommand) _CNegMoveCommand = new AsyncCommand(CNegMoveFunc);
                return _CNegMoveCommand;
            }
        }
        private AsyncCommand _TriPosMoveCommand;
        public ICommand TriPosMoveCommand
        {
            get
            {
                if (null == _TriPosMoveCommand) _TriPosMoveCommand = new AsyncCommand(TriPosMoveFunc);
                return _TriPosMoveCommand;
            }
        }
        private AsyncCommand _TriNegMoveCommand;
        public ICommand TriNegMoveCommand
        {
            get
            {
                if (null == _TriNegMoveCommand) _TriNegMoveCommand = new AsyncCommand(TriNegMoveFunc);
                return _TriNegMoveCommand;
            }
        }
        private AsyncCommand _PzPosMoveCommand;
        public ICommand PzPosMoveCommand
        {
            get
            {
                if (null == _PzPosMoveCommand) _PzPosMoveCommand = new AsyncCommand(PzPosMoveFunc);
                return _PzPosMoveCommand;
            }
        }
        private AsyncCommand _PzNegMoveCommand;
        public ICommand PzNegMoveCommand
        {
            get
            {
                if (null == _PzNegMoveCommand) _PzNegMoveCommand = new AsyncCommand(PzNegMoveFunc);
                return _PzNegMoveCommand;
            }
        }
        #endregion

        #region Loader Move
        private AsyncCommand _APosMoveCommand;
        public ICommand APosMoveCommand
        {
            get
            {
                if (null == _APosMoveCommand) _APosMoveCommand = new AsyncCommand(APosMoveFunc);
                return _APosMoveCommand;
            }
        }
        private AsyncCommand _ANegMoveCommand;
        public ICommand ANegMoveCommand
        {
            get
            {
                if (null == _ANegMoveCommand) _ANegMoveCommand = new AsyncCommand(ANegMoveFunc);
                return _ANegMoveCommand;
            }
        }
        private AsyncCommand _U1PosMoveCommand;
        public ICommand U1PosMoveCommand
        {
            get
            {
                if (null == _U1PosMoveCommand) _U1PosMoveCommand = new AsyncCommand(U1PosMoveFunc);
                return _U1PosMoveCommand;
            }
        }
        private AsyncCommand _U1NegMoveCommand;
        public ICommand U1NegMoveCommand
        {
            get
            {
                if (null == _U1NegMoveCommand) _U1NegMoveCommand = new AsyncCommand(U1NegMoveFunc);
                return _U1NegMoveCommand;
            }
        }
        private AsyncCommand _U2PosMoveCommand;
        public ICommand U2PosMoveCommand
        {
            get
            {
                if (null == _U2PosMoveCommand) _U2PosMoveCommand = new AsyncCommand(U2PosMoveFunc);
                return _U2PosMoveCommand;
            }
        }
        private AsyncCommand _U2NegMoveCommand;
        public ICommand U2NegMoveCommand
        {
            get
            {
                if (null == _U2NegMoveCommand) _U2NegMoveCommand = new AsyncCommand(U2NegMoveFunc);
                return _U2NegMoveCommand;
            }
        }
        private AsyncCommand _WPosMoveCommand;
        public ICommand WPosMoveCommand
        {
            get
            {
                if (null == _WPosMoveCommand) _WPosMoveCommand = new AsyncCommand(WPosMoveFunc);
                return _WPosMoveCommand;
            }
        }
        private AsyncCommand _WNegMoveCommand;
        public ICommand WNegMoveCommand
        {
            get
            {
                if (null == _WNegMoveCommand) _WNegMoveCommand = new AsyncCommand(WNegMoveFunc);
                return _WNegMoveCommand;
            }
        }
        private AsyncCommand _VPosMoveCommand;
        public ICommand VPosMoveCommand
        {
            get
            {
                if (null == _VPosMoveCommand) _VPosMoveCommand = new AsyncCommand(VPosMoveFunc);
                return _VPosMoveCommand;
            }
        }
        private AsyncCommand _VNegMoveCommand;
        public ICommand VNegMoveCommand
        {
            get
            {
                if (null == _VNegMoveCommand) _VNegMoveCommand = new AsyncCommand(VNegMoveFunc);
                return _VNegMoveCommand;
            }
        }
        private AsyncCommand _ScPosMoveCommand;
        public ICommand ScPosMoveCommand
        {
            get
            {
                if (null == _ScPosMoveCommand) _ScPosMoveCommand = new AsyncCommand(ScPosMoveFunc);
                return _ScPosMoveCommand;
            }
        }
        private AsyncCommand _ScNegMoveCommand;
        public ICommand ScNegMoveCommand
        {
            get
            {
                if (null == _ScNegMoveCommand) _ScNegMoveCommand = new AsyncCommand(ScNegMoveFunc);
                return _ScNegMoveCommand;
            }
        }
        #endregion
        #region TextVal
        private int _XTextVal = 0;

        public int XTextVal
        {
            get { return _XTextVal; }
            set
            {
                if (value != _XTextVal)
                {
                    _XTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> XTextBoxClickCommand
        private RelayCommand<Object> _XTextBoxClickCommand;
        public ICommand XTextBoxClickCommand
        {
            get
            {
                if (null == _XTextBoxClickCommand) _XTextBoxClickCommand = new RelayCommand<Object>(XTextBoxClickCommandFunc);
                return _XTextBoxClickCommand;
            }
        }

        private void XTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _YTextVal = 0;

        public int YTextVal
        {
            get { return _YTextVal; }
            set
            {
                if (value != _YTextVal)
                {
                    _YTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> YTextBoxClickCommand
        private RelayCommand<Object> _YTextBoxClickCommand;
        public ICommand YTextBoxClickCommand
        {
            get
            {
                if (null == _YTextBoxClickCommand) _YTextBoxClickCommand = new RelayCommand<Object>(YTextBoxClickCommandFunc);
                return _YTextBoxClickCommand;
            }
        }

        private void YTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        private int _ZTextVal = 0;

        public int ZTextVal
        {
            get { return _ZTextVal; }
            set
            {
                if (value != _ZTextVal)
                {
                    _ZTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> ZTextBoxClickCommand
        private RelayCommand<Object> _ZTextBoxClickCommand;
        public ICommand ZTextBoxClickCommand
        {
            get
            {
                if (null == _ZTextBoxClickCommand) _ZTextBoxClickCommand = new RelayCommand<Object>(ZTextBoxClickCommandFunc);
                return _ZTextBoxClickCommand;
            }
        }

        private void ZTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        private int _CTextVal = 0;

        public int CTextVal
        {
            get { return _CTextVal; }
            set
            {
                if (value != _CTextVal)
                {
                    _CTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region ==> CTextBoxClickCommand
        private RelayCommand<Object> _CTextBoxClickCommand;
        public ICommand CTextBoxClickCommand
        {
            get
            {
                if (null == _CTextBoxClickCommand) _CTextBoxClickCommand = new RelayCommand<Object>(CTextBoxClickCommandFunc);
                return _CTextBoxClickCommand;
            }
        }

        private void CTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        private int _TriTextVal = 0;

        public int TriTextVal
        {
            get { return _TriTextVal; }
            set
            {
                if (value != _TriTextVal)
                {
                    _TriTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> TRITextBoxClickCommand
        private RelayCommand<Object> _TRITextBoxClickCommand;
        public ICommand TRITextBoxClickCommand
        {
            get
            {
                if (null == _TRITextBoxClickCommand) _TRITextBoxClickCommand = new RelayCommand<Object>(TRITextBoxClickCommandFunc);
                return _TRITextBoxClickCommand;
            }
        }

        private void TRITextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        private int _PzTextVal = 0;

        public int PzTextVal
        {
            get { return _PzTextVal; }
            set
            {
                if (value != _PzTextVal)
                {
                    _PzTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> PZTextBoxClickCommand
        private RelayCommand<Object> _PZTextBoxClickCommand;
        public ICommand PZTextBoxClickCommand
        {
            get
            {
                if (null == _PZTextBoxClickCommand) _PZTextBoxClickCommand = new RelayCommand<Object>(PZTextBoxClickCommandFunc);
                return _PZTextBoxClickCommand;
            }
        }

        private void PZTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        private int _ATextVal = 0;

        public int ATextVal
        {
            get { return _ATextVal; }
            set
            {
                if (value != _ATextVal)
                {
                    _ATextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _U1TextVal = 0;

        public int U1TextVal
        {
            get { return _U1TextVal; }
            set
            {
                if (value != _U1TextVal)
                {
                    _U1TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _U2TextVal = 0;

        public int U2TextVal
        {
            get { return _U2TextVal; }
            set
            {
                if (value != _U2TextVal)
                {
                    _U2TextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _WTextVal = 0;

        public int WTextVal
        {
            get { return _WTextVal; }
            set
            {
                if (value != _WTextVal)
                {
                    _WTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _VTextVal = 0;

        public int VTextVal
        {
            get { return _VTextVal; }
            set
            {
                if (value != _VTextVal)
                {
                    _VTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _SCTextVal = 0;

        public int ScTextVal
        {
            get { return _SCTextVal; }
            set
            {
                if (value != _SCTextVal)
                {
                    _SCTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ActualVal
        private AxisObject _XAxis;

        public AxisObject XAxis
        {
            get { return _XAxis; }
            set
            {
                if (value != _XAxis)
                {
                    _XAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _YAxis;

        public AxisObject YAxis
        {
            get { return _YAxis; }
            set
            {
                if (value != _YAxis)
                {
                    _YAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _ZAxis;

        public AxisObject ZAxis
        {
            get { return _ZAxis; }
            set
            {
                if (value != _ZAxis)
                {
                    _ZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _CAxis;

        public AxisObject CAxis
        {
            get { return _CAxis; }
            set
            {
                if (value != _CAxis)
                {
                    _CAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _TriAxis;

        public AxisObject TriAxis
        {
            get { return _TriAxis; }
            set
            {
                if (value != _TriAxis)
                {
                    _TriAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _PzAxis;

        public AxisObject PzAxis
        {
            get { return _PzAxis; }
            set
            {
                if (value != _PzAxis)
                {
                    _PzAxis = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AxisObject _AAxis;

        public AxisObject AAxis
        {
            get { return _AAxis; }
            set
            {
                if (value != _AAxis)
                {
                    _AAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _U1Axis;

        public AxisObject U1Axis
        {
            get { return _U1Axis; }
            set
            {
                if (value != _U1Axis)
                {
                    _U1Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _U2Axis;

        public AxisObject U2Axis
        {
            get { return _U2Axis; }
            set
            {
                if (value != _U2Axis)
                {
                    _U2Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _WAxis;

        public AxisObject WAxis
        {
            get { return _WAxis; }
            set
            {
                if (value != _WAxis)
                {
                    _WAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _VAxis;

        public AxisObject VAxis
        {
            get { return _VAxis; }
            set
            {
                if (value != _VAxis)
                {
                    _VAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private AxisObject _SCAxis;

        public AxisObject SCAxis
        {
            get { return _SCAxis; }
            set
            {
                if (value != _SCAxis)
                {
                    _SCAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Command 
        private async Task XPosMoveFunc()
        {
            try
            {
                

                axis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = XTextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }
        private async Task XNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = XTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                
            }
        }
        private async Task YPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = YTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task YNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = YTextVal;
                    Negmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task ZPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ZTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task ZNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ZTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task CPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task CNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = CTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task TriPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.TRI);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = TriTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task TriNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.TRI);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = TriTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task PzPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = PzTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task PzNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = PzTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task APosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.A);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ATextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task ANegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.A);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ATextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task U1PosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.U1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U1TextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task U1NegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.U1);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U1TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task U2PosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.U2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U2TextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task U2NegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.U2);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = U2TextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task WPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.W);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = WTextVal;
                    Posmove();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task WNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.W);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = WTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task VPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.V);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = VTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task VNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.V);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = VTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task ScPosMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.SC);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ScTextVal;
                    Posmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }
        private async Task ScNegMoveFunc()
        {
            try
            {
                
                axis = this.MotionManager().GetAxis(EnumAxisConstants.SC);
                if (axis == null)
                {

                }
                else
                {
                    RelMoveStepDist = ScTextVal;
                    Negmove();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
            finally
            {
                
            }
        }

        #endregion
        #region Move Code
        private async Task Posmove()
        {

            try
            {
                double apos = 0;

                AxisObject = axis;
                apos = axis.Status.RawPosition.Ref;
                //this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos); //AxisObject.AxisType.Value 축 Enum //  apos : 축위치
                double pos = Math.Abs(RelMoveStepDist); // 움직일 거리의 절대값
                if (pos + apos < AxisObject.Param.PosSWLimit.Value) // 리밋체크 pos(움직일 거리)와 apos(기존 나의 위치)의 합이 리밋보다 작으면 동작
                {
                    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
                    NegButtonVisibility = false;
                    retVal = this.StageSupervisor().StageModuleState.ManualRelMove(AxisObject, pos);

                    //retVal = this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                }
                else
                {
                    //Sw limit
                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "SW Limit", EnumMessageStyle.Affirmative);

                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                PosRefresh();
            }

        }
        private async Task Negmove()
        {

            try
            {
                double apos = 0;
                AxisObject = axis;
                apos = axis.Status.RawPosition.Ref;
                //this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos);
                double pos = Math.Abs(RelMoveStepDist) * -1;
                if (pos + apos > AxisObject.Param.NegSWLimit.Value)
                {
                    PosButtonVisibility = false;
                    this.StageSupervisor().StageModuleState.ManualRelMove(AxisObject, pos);
                    //this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                }
                else
                {
                    //Sw Limit
                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "SW Limit", EnumMessageStyle.Affirmative);

                    return;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                PosRefresh();
            }

        }
        #endregion
        #region Position PosRefresh

        private async Task PosRefresh()
        {
            try
            {
                IMotionManager Motionmanager = this.MotionManager();
                //XActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.X).Status.Position.Actual,2);
                //YActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.Y).Status.Position.Actual, 2);
                //ZActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.Z).Status.Position.Actual, 2);
                //CActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.C).Status.Position.Actual, 2);
                //TriActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.TRI).Status.Position.Actual, 2);
                //PzActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.PZ).Status.Position.Actual, 2);
                //AActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.A).Status.Position.Actual, 2);
                //U1ActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.U2).Status.Position.Actual, 2);
                //U2ActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.U1).Status.Position.Actual, 2);
                //WActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.W).Status.Position.Actual, 2);
                //VActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.V).Status.Position.Actual, 2);
                //ScActualVal = Math.Round(Motionmanager.GetAxis(EnumAxisConstants.SC).Status.Position.Actual, 2);
                //double Pos = 0;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.X).AxisType.Value, ref Pos);
                //XActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Y).AxisType.Value, ref Pos);
                //YActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.Z).AxisType.Value, ref Pos);
                //ZActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.C).AxisType.Value, ref Pos);
                //CActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.TRI).AxisType.Value, ref Pos);
                //TriActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.PZ).AxisType.Value, ref Pos);
                //PzActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.A).AxisType.Value, ref Pos);
                //AActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.U1).AxisType.Value, ref Pos);
                //U1ActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.U2).AxisType.Value, ref Pos);
                //U2ActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.W).AxisType.Value, ref Pos);
                //WActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.V).AxisType.Value, ref Pos);
                //VActualVal = Pos;
                //this.MotionManager().GetActualPos(this.MotionManager().GetAxis(EnumAxisConstants.SC).AxisType.Value, ref Pos);
                //ScActualVal = Pos;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
            }

        }
        #endregion
        #endregion

        #region TextBox.Value
        private List<MotionText> _MotionTextVal;
        public List<MotionText> MotionTextVal
        {
            get { return _MotionTextVal; }
            set
            {
                if (value != _MotionTextVal)
                {
                    _MotionTextVal = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        private AsyncCommand _PosRelMoveCommand;
        public ICommand PosRelMoveCommand
        {
            get
            {
                if (null == _PosRelMoveCommand) _PosRelMoveCommand = new AsyncCommand(PosRelMove);
                return _PosRelMoveCommand;
            }
        }
        private async Task PosRelMove()
        {
            try
            {
                await Task.Run(() =>
                {
                    double apos = 0;
                    this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos); //AxisObject.AxisType.Value 축 Enum //  apos : 축위치
                    double pos = Math.Abs(RelMoveStepDist); // 움직일 거리의 절대값
                    if (pos + apos < AxisObject.Param.PosSWLimit.Value) // 리밋체크 pos(움직일 거리)와 apos(기존 나의 위치)의 합이 리밋보다 작으면 동작
                    {
                        NegButtonVisibility = false;
                        this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                    }
                    else
                    {
                        //Sw limit
                    }
                });

                NegButtonVisibility = true;
            }
            catch (Exception err)
            {
                NegButtonVisibility = true;
            }
        }

        private AsyncCommand _NegRelMoveCommand;
        public ICommand NegRelMoveCommand
        {
            get
            {
                if (null == _NegRelMoveCommand) _NegRelMoveCommand = new AsyncCommand(NegRelMove);
                return _NegRelMoveCommand;
            }
        }
        private async Task NegRelMove()
        {
            EnumMessageDialogResult ret;
            try
            {
                await Task.Run(() =>
                {
                    double apos = 0;
                    this.MotionManager().GetActualPos(AxisObject.AxisType.Value, ref apos);
                    double pos = Math.Abs(RelMoveStepDist) * -1;
                    if (pos + apos > AxisObject.Param.NegSWLimit.Value)
                    {
                        PosButtonVisibility = false;
                        this.MotionManager().RelMove(AxisObject, pos, AxisObject.Param.Speed.Value, AxisObject.Param.Acceleration.Value);
                    }
                    else
                    {
                        //Sw Limit
                    }
                });
                PosButtonVisibility = true;
            }
            catch (Exception err)
            {
                PosButtonVisibility = true;
            }
        }

        private AsyncCommand _StopMoveCommand;
        public ICommand StopMoveCommand
        {
            get
            {
                if (null == _StopMoveCommand) _StopMoveCommand = new AsyncCommand(StopMove);
                return _StopMoveCommand;
            }
        }
        private async Task StopMove()
        {
            try
            {
                await Task.Run(() =>
                {
                    this.MotionManager().Stop(AxisObject);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        #region ==> TextBoxClickCommand
        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }

        private void TextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    PropertyInfo[] propertyInfos;
                    IOPortDescripter<bool> port;
                    object propObject;
                    double axisX, axisY, axisZ, axisC, axisTRI, axisR, axisTT, axisPZ, axisCT, axisCCM, axisCCS, axisCCG, axisA, axisU1, axisU2, axisW, axisV, axisSC;

                    //StageAxes aes = this.MotionManager().StageAxes;
                    //LoaderAxes les = this.MotionManager().LoaderAxes;

                    StageAxisObjectVmList = new ObservableCollection<AxisObjectVM>();

                    LoaderController = this.LoaderController() as ILoaderControllerExtension;

                    ViewModelManager = this.ViewModelManager();

                    Stage3DModel = this.ViewModelManager().Stage3DModel;

                    ViewNUM = 0;

                    CenterView();

                    IsItDisplayed2RateMagnification = false;
                    //{
                    //    if (item.AxisType.Value == EnumAxisConstants.R || item.AxisType.Value == EnumAxisConstants.TT)
                    //    {

                    //        var axisObjVM = new AxisObjectVM();
                    //        axisObjVM.AxisObject = item;
                    //        axisObjVM.NegButtonVisibility = false;
                    //        axisObjVM.PosButtonVisibility = false;

                    //        StageAxisObjectVmList.Add(axisObjVM);
                    //    }
                    //    else
                    //    {
                    //        var axisObjVM = new AxisObjectVM();
                    //        axisObjVM.AxisObject = item;

                    //        StageAxisObjectVmList.Add(axisObjVM);
                    //    }

                    //}
                    #region Axis Getval
                    IMotionManager Motionmanager = this.MotionManager();

                    if (Motionmanager != null)
                    {
                        XAxis = Motionmanager.GetAxis(EnumAxisConstants.X);
                        YAxis = Motionmanager.GetAxis(EnumAxisConstants.Y);
                        ZAxis = Motionmanager.GetAxis(EnumAxisConstants.Z);
                        CAxis = Motionmanager.GetAxis(EnumAxisConstants.C);
                        TriAxis = Motionmanager.GetAxis(EnumAxisConstants.TRI);
                        PzAxis = Motionmanager.GetAxis(EnumAxisConstants.PZ);
                        AAxis = Motionmanager.GetAxis(EnumAxisConstants.A);
                        U1Axis = Motionmanager.GetAxis(EnumAxisConstants.U1);
                        U2Axis = Motionmanager.GetAxis(EnumAxisConstants.U2);
                        WAxis = Motionmanager.GetAxis(EnumAxisConstants.W);
                        VAxis = Motionmanager.GetAxis(EnumAxisConstants.V);
                        SCAxis = Motionmanager.GetAxis(EnumAxisConstants.SC);
                    }

                    #endregion
                    LoaderAxisObjectVmList = new ObservableCollection<AxisObjectVM>();
                    //foreach (var item in les.ProbeAxisProviders)
                    //{
                    //    var axisObjVM = new AxisObjectVM();
                    //    axisObjVM.AxisObject = item;

                    //    LoaderAxisObjectVmList.Add(axisObjVM);
                    //}

                    PosRefresh();

                    StageCamList = new ObservableCollection<StageCamera>();
                    StageCamList.Add(new StageCamera(enumStageCamType.WaferHigh));
                    StageCamList.Add(new StageCamera(enumStageCamType.WaferLow));
                    StageCamList.Add(new StageCamera(enumStageCamType.PinHigh));
                    StageCamList.Add(new StageCamera(enumStageCamType.PinLow));
                    StageCamList.Add(new StageCamera(enumStageCamType.MAP_REF));
                    StageCamList.Add(new StageCamera(enumStageCamType.UNDEFINED));

                    if (this.IOManager() != null)
                    {
                        OutputPorts.Clear();
                        InputPorts.Clear();
                        propertyInfos = this.IOManager().IO.Outputs.GetType().GetProperties();
                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.IOManager().IO.Outputs);
                                port = (IOPortDescripter<bool>)propObject;
                                OutputPorts.Add(port);
                                FilteredOutputPorts.Add(port);
                            }
                        }
                        propertyInfos = this.IOManager().IO.Inputs.GetType().GetProperties();
                        foreach (var item in propertyInfos)
                        {
                            if (item.PropertyType == typeof(IOPortDescripter<bool>))
                            {
                                port = new IOPortDescripter<bool>();
                                propObject = item.GetValue(this.IOManager().IO.Inputs);
                                port = (IOPortDescripter<bool>)propObject;
                                InputPorts.Add(port);
                                FilteredInputPorts.Add(port);
                            }
                        }
                        //port.Key
                    }

                    light = this.LightAdmin();
                    //foreach (var item in light.Lights)
                    //{
                    //    light.SetLight(item.ChannelMapIdx, (ushort)LightValue);
                    //    Lights.Add(item);
                    //}
                    for (int i = 0; i < 8; i++)
                    {
                        Lights.Add(new LightChannelType(EnumLightType.UNDEFINED, i));
                    }
                    SelectedLight = Lights[0];

                    for (int i = ((int)EnumProberCam.UNDEFINED + 1); i < ((int)EnumProberCam.CAM_LAST); i++)
                    {
                        CamChannels.Add(new CameraChannelType((EnumProberCam)i, i));
                    }
                    SelectedChannel = CamChannels[0];

                    if (this.MotionManager() != null)
                    {
                        if (this.MotionManager().GetAxis(EnumAxisConstants.R) == null || this.MotionManager().GetAxis(EnumAxisConstants.TT) == null)
                        {
                            EnableTiltElement = false;
                        }
                        else
                        {
                            EnableTiltElement = true;
                        }
                    }

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retval = EventCodeEnum.SYSTEM_ERROR;
            }

            return retval;
        }
        private void ChangeChannel(object obj)
        {
            try
            {
                var vm = this.VisionManager();
                vm.SwitchCamera(vm.GetCam(SelectedChannel.Type).Param, this);
                //vm.GetCam(SelectedChannel.Type).SwitchCamera();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void UpdateLight()
        {
            try
            {
                //light.SetLight(0, (ushort)LightValue);
                //light.SetLight(1, (ushort)LightValue);
                //light.SetLight(2, (ushort)LightValue);
                //light.SetLight(3, (ushort)LightValue);
                //light.SetLight(4, (ushort)LightValue);
                //light.SetLight(5, (ushort)LightValue);
                //light.SetLight(6, (ushort)LightValue);
                //light.SetLight(7, (ushort)LightValue);
                ushort lightValue = (ushort)LightValue;
                light.SetLight(SelectedLight.ChannelMapIdx.Value, lightValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async void SearchMatched()
        {
            try
            {
                string upper = SearchKeyword.ToUpper();
                string lower = SearchKeyword.ToLower();

                await Task.Run(() =>
                {
                    if (SearchKeyword.Length > 0)
                    {
                        var outs = OutputPorts.Where(
                            t => t.Key.Value.StartsWith(upper) ||
                            t.Key.Value.StartsWith(lower) ||
                            t.Key.Value.ToUpper().Contains(upper));
                        var filtered = new ObservableCollection<IOPortDescripter<bool>>(outs);

                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredOutputPorts.Clear();
                                foreach (var item in filtered)
                                {
                                    FilteredOutputPorts.Add(item);
                                }
                            });
                        }


                        var inputs = InputPorts.Where(
                            t => t.Key.Value.StartsWith(upper) ||
                            t.Key.Value.StartsWith(lower) ||
                            t.Key.Value.ToUpper().Contains(upper));
                        filtered = new ObservableCollection<IOPortDescripter<bool>>(inputs);

                        //using (Locker locker = new Locker(inPortLock))
                        //{
                        lock (inPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredInputPorts.Clear();
                                foreach (var item in filtered)
                                {
                                    FilteredInputPorts.Add(item);
                                }
                            });

                        }
                    }
                    else
                    {
                        //using (Locker locker = new Locker(inPortLock))
                        //{
                        lock (inPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredInputPorts.Clear();
                                foreach (var item in InputPorts)
                                {
                                    FilteredInputPorts.Add(item);
                                }
                            });
                        }

                        //using (Locker locker = new Locker(outPortLock))
                        //{
                        lock (outPortLock)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                FilteredOutputPorts.Clear();
                                foreach (var item in OutputPorts)
                                {
                                    FilteredOutputPorts.Add(item);
                                }
                            });
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private IZoomObject _ZoomObject;

        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set { _ZoomObject = value; }
        }
        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ZoomObject = Wafer;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                PosRefresh();
                //this.SysState().SetSetUpState();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Stage3DModel = null;
                    Stage3DModel = this.ViewModelManager().Stage3DModel;
                });

                CenterView();
                IsItDisplayed2RateMagnification = false;
                this.StageSupervisor().StageModuleState.ManualZDownMove();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        private RelayCommand<object> _SwitchPage;
        public ICommand SwitchPage
        {
            get
            {
                if (null == _SwitchPage) _SwitchPage = new RelayCommand<object>(PageSwitching);
                return _SwitchPage;
            }
        }
        private RelayCommand<CUI.Button> _OperatorPageSwitchCommand;
        public ICommand OperatorPageSwitchCommand
        {
            get
            {
                if (null == _OperatorPageSwitchCommand) _OperatorPageSwitchCommand = new RelayCommand<CUI.Button>(FuncOperatorPageSwitchCommand);
                return _OperatorPageSwitchCommand;
            }
        }

        private void FuncOperatorPageSwitchCommand(CUI.Button cuiparam)
        {
            try
            {
                this.ViewModelManager().ChangeFlyOutControlStatus(true);

                Guid ViewGUID = CUIServices.CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void PageSwitching(object obj)
        {
            try
            {
                this.ViewModelManager().ViewTransitionAsync(new Guid(obj.ToString()));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            try
            {
                //this.SysState().SetSetUpDoneState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.StageSupervisor().StageModuleState.ZCLEARED();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }
        #region Move
        private AsyncCommand _MoveToBackCommand;
        public ICommand MoveToBackCommand
        {
            get
            {
                if (null == _MoveToBackCommand) _MoveToBackCommand = new AsyncCommand(MoveToBack);
                return _MoveToBackCommand;
            }
        }
        private async Task MoveToBack()
        {
            try
            {
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    this.StageSupervisor().StageModuleState.ManualAbsMove(0, yaxis.Param.PosSWLimit.Value - 100, zaxis.Param.HomeOffset.Value);
                    //this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                //LoggerManager.Error($ex.Message);
                LoggerManager.Exception(err);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _MoveToCenterCommand;
        public ICommand MoveToCenterCommand
        {
            get
            {
                if (null == _MoveToCenterCommand) _MoveToCenterCommand = new AsyncCommand(MoveToCenter);
                return _MoveToCenterCommand;
            }
        }
        private async Task MoveToCenter()
        {
            try
            {
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().StageMove(0, 0, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);

            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _MoveToFrontCommand;
        public ICommand MoveToFrontCommand
        {
            get
            {
                if (null == _MoveToFrontCommand) _MoveToFrontCommand = new AsyncCommand(MoveToFront);
                return _MoveToFrontCommand;
            }
        }
        private async Task MoveToFront()
        {
            try
            {
                StageButtonsVisibility = false;

                await Task.Run(() =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);

                    this.MotionManager().StageMove(0, yaxis.Param.NegSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                PosRefresh();
            }
        }

        private AsyncCommand _MoveToLoadPosCommand;
        public ICommand MoveToLoadPosCommand
        {
            get
            {
                if (null == _MoveToLoadPosCommand) _MoveToLoadPosCommand = new AsyncCommand(MoveToLoadPos);
                return _MoveToLoadPosCommand;
            }
        }
        private async Task MoveToLoadPos()
        {
            try
            {
                StageButtonsVisibility = false;
                double zoffset = 0;
                await Task.Run(() =>
                {
                    this.StageSupervisor().StageModuleState.MoveLoadingPosition(zoffset);
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
        }
        private AsyncCommand _UnLoadFromBackCommand;
        public ICommand UnLoadFromBackCommand
        {
            get
            {
                if (null == _UnLoadFromBackCommand) _UnLoadFromBackCommand = new AsyncCommand(UnLoadFromBackCommandPos);
                return _UnLoadFromBackCommand;
            }
        }
        private async Task UnLoadFromBackCommandPos()
        {
            try
            {

                EnumMessageDialogResult ret;
                
                await Task.Run(() =>
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });

                ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Hand UnLoading", "Turn Off the Chuck Vacuum??", EnumMessageStyle.AffirmativeAndNegative);


                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    //척 베큠 off
                }
                else
                {
                    //Dialog 쏘자
                }


            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                

            }
        }

        private AsyncCommand _LoadFromBackCommand;
        public ICommand LoadFromBackCommand
        {
            get
            {
                if (null == _LoadFromBackCommand) _LoadFromBackCommand = new AsyncCommand(LoadFromBackCommandPos);
                return _LoadFromBackCommand;
            }
        }
        private async Task LoadFromBackCommandPos()
        {
            try
            {

                EnumMessageDialogResult ret;
                
                await Task.Run(() =>
                {
                    ProbeAxisObject yaxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().StageMove(0, yaxis.Param.PosSWLimit.Value, zaxis.Param.HomeOffset.Value);
                });

                ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Hand UnLoading", "Turn On the Chuck Vacuum??", EnumMessageStyle.AffirmativeAndNegative);

                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    //척 베큠 On
                }
                else
                {
                    //Dialog 쏘자
                }

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                

            }
        }
        public IViewModelManager ViewModelManager { get; set; }

        private int _ViewNUM;
        public int ViewNUM
        {
            get { return _ViewNUM; }
            set
            {
                if (value != _ViewNUM)
                {
                    _ViewNUM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsItDisplayed2RateMagnification;
        public bool IsItDisplayed2RateMagnification
        {
            get { return _IsItDisplayed2RateMagnification; }
            set
            {
                if (value != _IsItDisplayed2RateMagnification)
                {
                    _IsItDisplayed2RateMagnification = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void Viewx2() // 2x view
        {
            try
            {
                IsItDisplayed2RateMagnification = !IsItDisplayed2RateMagnification;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CWVIEW() //CW
        {
            try
            {
                ViewNUM = ((Enum.GetNames(typeof(CameraViewPoint)).Length) + (--ViewNUM)) % Enum.GetNames(typeof(CameraViewPoint)).Length;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CenterView() //FRONT
        {
            try
            {
                ViewNUM = 0;
                IsItDisplayed2RateMagnification = false;
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CCWView() // CCW
        {
            try
            {
                ViewNUM = Math.Abs(++ViewNUM % Enum.GetNames(typeof(CameraViewPoint)).Length);
                ViewModelManager.Set3DCamPosition((CameraViewPoint)ViewNUM, IsItDisplayed2RateMagnification);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _X2ViewChangeCommand;
        public RelayCommand X2ViewChangeCommand
        {
            get
            {
                if (null == _X2ViewChangeCommand) _X2ViewChangeCommand = new RelayCommand(Viewx2);
                return _X2ViewChangeCommand;
            }
        }


        private RelayCommand _CWViewChangeCommand;
        public ICommand CWViewChangeCommand
        {
            get
            {
                if (null == _CWViewChangeCommand) _CWViewChangeCommand = new RelayCommand(CWVIEW);
                return _CWViewChangeCommand;
            }
        }


        private RelayCommand _CenterViewChangeCommand;
        public ICommand CenterViewChangeCommand
        {
            get
            {
                if (null == _CenterViewChangeCommand) _CenterViewChangeCommand = new RelayCommand(CenterView);
                return _CenterViewChangeCommand;
            }
        }


        private RelayCommand _CCWViewChangeCommand;
        public ICommand CCWViewChangeCommand
        {
            get
            {
                if (null == _CCWViewChangeCommand) _CCWViewChangeCommand = new RelayCommand(CCWView);
                return _CCWViewChangeCommand;
            }
        }
        private void ChuckVacuum(string ONOFF)
        {
            try
            {
                if (ONOFF == "ON")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, true);
                    }
                }
                else if (ONOFF == "OFF")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, false);
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ChuckVacuumCheck(string ONOFF)
        {
            try
            {
                if (ONOFF == "ON")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_12, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, true);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, true);
                    }
                }
                else if (ONOFF == "OFF")
                {
                    if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH12)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_12, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH8)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_8, false);
                    }
                    else if (this.Wafer.GetPhysInfo().WaferSizeEnum == EnumWaferSize.INCH6)
                    {
                        this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIWAFERONCHUCK_6, false);
                    }
                }
                else
                {

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _ManualWaferCommand;
        public ICommand ManualWaferCommand
        {
            get
            {
                if (null == _ManualWaferCommand) _ManualWaferCommand = new AsyncCommand(ManualWaferCommandFunc);
                return _ManualWaferCommand;
            }
        }
        private async Task ManualWaferCommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;
                
                await Task.Run(async () =>
                {
                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    this.MotionManager().StageMove(0, 0, zaxis.Param.HomeOffset.Value);
                    if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Unloading", "Do you want to remove the wafer from the chuck? ", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            // Size 별로 베큠 센서 조작 VAC OFF
                            ChuckVacuum("OFF");
                            ChuckVacuumCheck("OFF");

                            this.StageSupervisor().StageModuleState.Handlerhold(10000);

                            ret2 = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Unloading", "Did you remove the wafer from the chuck? ", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret2 == EnumMessageDialogResult.AFFIRMATIVE) // OK button
                            {
                                //3PIN Down
                                ChuckVacuum("ON");
                                this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.EXIST)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "The wafer was not removed from the chuck.", EnumMessageStyle.Affirmative);
                                }
                                else if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                                {
                                    ChuckVacuum("OFF");

                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Remove wafer done.", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                //척 배큠 ON
                                ChuckVacuum("ON");

                                this.StageSupervisor().StageModuleState.Handlerrelease(10000);

                                if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "The wafer was not removed from the chuck.", EnumMessageStyle.Affirmative);
                                }
                                else if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                                {
                                    ChuckVacuum("OFF");

                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Unload wafer done.", EnumMessageStyle.Affirmative);
                                }
                            }
                        }
                        else
                        {

                        }

                    }

                    else if (LoaderController.LoaderInfo.StateMap.ChuckModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)  // Load 해야하는 상황
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Hand Loading", "Do you want to load the wafer onto Chuck ? ", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            ChuckVacuum("OFF");
                            ChuckVacuumCheck("OFF");

                            this.StageSupervisor().StageModuleState.Handlerhold(10000);

                            ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Loading", "Did you put the wafer on Three Pin? ", EnumMessageStyle.AffirmativeAndNegative);

                            if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                            {
                                ChuckVacuum("ON");
                                this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.EXIST)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Load wafer done.", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    ChuckVacuum("OFF");
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "No Wafer on chuck.", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                // 준비 ㄴㄴ
                                ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Loading", "Cancel the wafer loading? ", EnumMessageStyle.AffirmativeAndNegative);

                                if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                                {
                                    this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                    await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Cancel.", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Manual Loading", "Did you put the wafer on Three Pin? ", EnumMessageStyle.AffirmativeAndNegative);

                                    if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                                    {
                                        ChuckVacuum("ON");
                                        this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                        if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.EXIST)
                                        {
                                            await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Load wafer done.", EnumMessageStyle.Affirmative);
                                        }
                                        else
                                        {
                                            ChuckVacuum("OFF");
                                            await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "No Wafer on chuck.", EnumMessageStyle.Affirmative);
                                        }
                                    }
                                    else
                                    {
                                        this.StageSupervisor().StageModuleState.Handlerrelease(10000);
                                        await this.MetroDialogManager().ShowMessageDialog("Manual Jog", "Cancel.", EnumMessageStyle.Affirmative);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ////Motion Cancel
                        }

                    }
                });


            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                

            }
        }

        private AsyncCommand _ManualPreCommand;
        public ICommand ManualPreCommand
        {
            get
            {
                if (null == _ManualPreCommand) _ManualPreCommand = new AsyncCommand(ManualPreCommandFunc);
                return _ManualPreCommand;
            }
        }
        private async Task ManualPreCommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;
                
                await Task.Run(async () =>
                {
                    if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOSUBCHUCKAIRON, true);
                    }
                    else if (LoaderController.LoaderInfo.StateMap.PreAlignModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOSUBCHUCKAIRON, false);
                    }

                });

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                

            }
        }
        private AsyncCommand _ManualArmCommand;
        public ICommand ManualArmCommand
        {
            get
            {
                if (null == _ManualArmCommand) _ManualArmCommand = new AsyncCommand(ManualArmCommandFunc);
                return _ManualArmCommand;
            }
        }
        private async Task ManualArmCommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;
                
                await Task.Run(async () =>
                {
                    if (LoaderController.LoaderInfo.StateMap.ARMModules[0].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARMAIRON, true);
                    }
                    else if (LoaderController.LoaderInfo.StateMap.ARMModules[0].WaferStatus == EnumSubsStatus.NOT_EXIST)
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARMAIRON, false);
                    }

                });

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                

            }
        }
        private AsyncCommand _ManualArm2Command;
        public ICommand ManualArm2Command
        {
            get
            {
                if (null == _ManualArm2Command) _ManualArm2Command = new AsyncCommand(ManualArm2CommandFunc);
                return _ManualArm2Command;
            }
        }
        private async Task ManualArm2CommandFunc()
        {
            try
            {

                EnumMessageDialogResult ret;
                EnumMessageDialogResult ret2;
                EventCodeEnum leg = EventCodeEnum.UNDEFINED;
                bool isThreelegDown = false;
                bool isThreelegUp = false;
                int IOError;
                
                await Task.Run(async () =>
                {
                    if (LoaderController.LoaderInfo.StateMap.ARMModules[1].WaferStatus == EnumSubsStatus.EXIST) // Unload 해야하는 상황
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARM2AIRON, true);
                    }
                    else if (LoaderController.LoaderInfo.StateMap.ARMModules[1].WaferStatus == EnumSubsStatus.NOT_EXIST)
                    {

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOARM2AIRON, false);
                    }

                });

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
            finally
            {
                

            }
        }


        private AsyncCommand _StageMachineInitCommand;
        public ICommand StageMachineInitCommand
        {
            get
            {
                if (null == _StageMachineInitCommand) _StageMachineInitCommand = new AsyncCommand(StageInit);
                return _StageMachineInitCommand;
            }
        }
        private async Task StageInit()
        {
            try
            {
                StageButtonsVisibility = false;
                int ret = -1;
                await Task.Run(() =>
                {
                    ret = this.MotionManager().ForcedZDown();
                });


                await Task.Run(() =>
                {
                    if (ret == 0)
                    {
                        this.MotionManager().StageSystemInit();
                    }
                });

                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
        }

        private AsyncCommand _LoaderMachineInitCommand;
        public ICommand LoaderMachineInitCommand
        {
            get
            {
                if (null == _LoaderMachineInitCommand) _LoaderMachineInitCommand = new AsyncCommand(LoaderInit);
                return _LoaderMachineInitCommand;
            }
        }
        private async Task LoaderInit()
        {

            try
            {
                StageButtonsVisibility = false;


                await Task.Run(() =>
                {
                    this.LoaderController().LoaderSystemInit();
                });

                StageButtonsVisibility = true;

            }
            catch (Exception ex)
            {
                StageButtonsVisibility = true;
                LoggerManager.Error(ex.Message);

            }
        }

        private AsyncCommand _AxisZHomingCommand;
        public ICommand AxisZHomingCommand
        {
            get
            {
                if (null == _AxisZHomingCommand) _AxisZHomingCommand = new AsyncCommand(AxisZHoming);
                return _AxisZHomingCommand;
            }
        }
        private async Task AxisZHoming()
        {
            try
            {
            }
            catch (Exception ex)
            {
                StageButtonsVisibility = true;
                LoggerManager.Error(ex.Message);

            }
        }


        private AsyncCommand _CamMoveCommand;
        public ICommand CamMoveCommand
        {
            get
            {
                if (null == _CamMoveCommand) _CamMoveCommand = new AsyncCommand(CamMove);
                return _CamMoveCommand;
            }
        }
        private async Task CamMove()
        {
            //double Thickness = this.StageSupervisor().WaferObject.PhysInfoGetter.Thickness.Value;
            double Thickness = 0;
            //double pinHeight = this.StageSupervisor().ProbeCardInfo.PinDefaultHeight.Value;
            double pinHeight = -10000;
            try
            {
                StageButtonsVisibility = false;
                await Task.Run(() =>
                {
                    switch (SelectedCam)
                    {
                        case enumStageCamType.UNDEFINED:
                            break;
                        case enumStageCamType.WaferHigh:
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(0, 0, Thickness);
                            break;
                        case enumStageCamType.WaferLow:
                            this.StageSupervisor().StageModuleState.WaferLowViewMove(0, 0, Thickness);
                            break;
                        case enumStageCamType.PinHigh:
                            this.StageSupervisor().StageModuleState.PinHighViewMove(0, 0, pinHeight);
                            break;
                        case enumStageCamType.PinLow:
                            this.StageSupervisor().StageModuleState.PinLowViewMove(0, 0, pinHeight);
                            break;
                        case enumStageCamType.MAP_REF:
                            this.StageSupervisor().StageModuleState.PinLowViewMove(0, 0, pinHeight);
                            break;
                        default:
                            break;
                    }

                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
        }

        private AsyncCommand _MoveToMarkCommand;
        public ICommand MoveToMarkCommand
        {
            get
            {
                if (null == _MoveToMarkCommand) _MoveToMarkCommand = new AsyncCommand(MoveToMark);
                return _MoveToMarkCommand;
            }
        }
        private async Task MoveToMark()
        {
            try
            {
                StageButtonsVisibility = false;

                await Task.Run(() =>
                {
                    this.StageSupervisor().StageModuleState.MoveToMark();
                });
                StageButtonsVisibility = true;

            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;

                LoggerManager.Exception(err);
                //LoggerManager.Error($ex.Message);
            }
        }

        private AsyncCommand _AutoTiltCommand;
        public ICommand AutoTiltCommand
        {
            get
            {
                if (null == _AutoTiltCommand) _AutoTiltCommand = new AsyncCommand(AutoTiltFunc);
                return _AutoTiltCommand;
            }
        }
        private async Task AutoTiltFunc()
        {
        }

        private AsyncCommand _AutoTiltStopCommand;
        public ICommand AutoTiltStopCommand
        {
            get
            {
                if (null == _AutoTiltStopCommand) _AutoTiltStopCommand = new AsyncCommand(AutoTiltStopFunc);
                return _AutoTiltStopCommand;
            }
        }
        private async Task AutoTiltStopFunc()
        {
        }

        private AsyncCommand _TiltMoveCommand;
        public ICommand TiltMoveCommand
        {
            get
            {
                if (null == _TiltMoveCommand) _TiltMoveCommand = new AsyncCommand(ChuckTiltMove);
                return _TiltMoveCommand;
            }
        }
        private async Task ChuckTiltMove()
        {
            try
            {

                await Task.Run(() =>
                {
                    this.StageSupervisor().StageModuleState.ChuckTiltMove(RPosDist, TTPosDist);
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //LoggerManager.Error($err.Message);
            }
        }

        private RelayCommand<object> _SensorSetZeroCommand;
        public ICommand SensorSetZeroCommand
        {
            get
            {
                if (_SensorSetZeroCommand == null) _SensorSetZeroCommand = new RelayCommand<object>(SensorSetZero);
                return _SensorSetZeroCommand;
            }
        }

        private void SensorSetZero(object noparam)
        {
            try
            {
                this.MotionManager().SetLoadCellZero();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //LoggerManager.Error($err.Message);
            }
        }

        private RelayCommand<object> _DualLoopOnCommand;
        public ICommand DualLoopOnCommand
        {
            get
            {
                if (_DualLoopOnCommand == null) _DualLoopOnCommand = new RelayCommand<object>(DualLoopOn);
                return _DualLoopOnCommand;
            }
        }

        private void DualLoopOn(object noparam)
        {
            try
            {
                this.MotionManager().SetDualLoop(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //LoggerManager.Error($err.Message);
            }
        }
        private RelayCommand<object> _DualLoopOffCommand;
        public ICommand DualLoopOffCommand
        {
            get
            {
                if (_DualLoopOffCommand == null) _DualLoopOffCommand = new RelayCommand<object>(DualLoopOff);
                return _DualLoopOffCommand;
            }
        }

        private void DualLoopOff(object noparam)
        {
            this.MotionManager().SetDualLoop(false);
        }


        private AsyncCommand _WaferMoveMiddleCommand;
        public ICommand WaferMoveMiddleCommand
        {
            get
            {
                if (null == _WaferMoveMiddleCommand) _WaferMoveMiddleCommand = new AsyncCommand(WaferMoveMiddle);
                return _WaferMoveMiddleCommand;
            }
        }
        private async Task WaferMoveMiddle()
        {
            int ret = -1;
            try
            {
                ret = StageCylinderType.MoveWaferCam.Extend();
                if (ret != 0)
                {
                    //ERrror
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand _WaferMoveRearCommand;
        public ICommand WaferMoveRearCommand
        {
            get
            {
                if (null == _WaferMoveRearCommand) _WaferMoveRearCommand = new AsyncCommand(WaferMoveRear);
                return _WaferMoveRearCommand;
            }
        }
        private async Task WaferMoveRear()
        {
            int ret = -1;
            try
            {
                ret = StageCylinderType.MoveWaferCam.Retract();
                if (ret != 0)
                {
                    //ERrror
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand _MeasurementChuckPlainCommand;
        public ICommand MeasurementChuckPlainCommand
        {
            get
            {
                if (null == _MeasurementChuckPlainCommand) _MeasurementChuckPlainCommand = new AsyncCommand(Test);
                return _MeasurementChuckPlainCommand;
            }
        }

        private async Task Test()
        {
            try
            {
                //double centerx;
                //double centery;
                //this.MotionManager().GetActualPos(EnumAxisConstants.X, out centerx);
                //this.MotionManager().GetActualPos(EnumAxisConstants.Y, out centery);
                //double dx = 0;
                //double dy = 0;
                //double posx;
                //double posy;
                //double zpos = 0;
                //this.StageSupervisor().StageModuleState.WaferHighViewMove(0,0, zpos);

                //for (int i = 0; i <= 36; i++)
                //{
                //    dx = 150000 * Math.Cos(Math.PI * (i * 10) / 180);
                //    dy = 150000 * Math.Sin(Math.PI * (i * 10) / 180);
                //    posx = centerx + (dx * -1);
                //    posy = centery + dy;
                //    MachineCoordinate mccoord = new MachineCoordinate();
                //    WaferCoordinate wafercoord = new WaferCoordinate();

                //    mccoord.X.Value = posx;
                //    mccoord.Y.Value = posy;
                //    wafercoord = this.CoordinateManager().WaferHighChuckConvert.Convert(mccoord);
                //    this.StageSupervisor().StageModuleState.WaferHighViewMove(wafercoord.X.Value, wafercoord.Y.Value);

                //}

                ////degree = 0;
                ////for (int i = 0; i < 18; i++)
                ////{
                ////    degree = Math.PI * (degree * i) / 180;
                ////    dx = 150000 * Math.Cos(degree);
                ////    dy = 150000 * Math.Sin(degree);
                ////    posx = centerx + dx;
                ////    posy = centery + (dy * -1);

                ////    this.MotionManager().StageMove(posx, posy);
                ////    degree += 10;
                ////}

                //this.StageSupervisor().StageModuleState.WaferHighViewMove(0,0, zpos);

                double a = 0.0;
                double b = 0.0;
                double c = 0.0;
                double d = 0.0;

                double zheight = 0.0;
                double zOffset = 0.0;
                double zValue = 0.0;
                //Calc
                CatCoordinates pos1 = new CatCoordinates();
                CatCoordinates pos2 = new CatCoordinates();
                CatCoordinates pos3 = new CatCoordinates();
                List<CatCoordinates> poslist = new List<CatCoordinates>();
                pos1.X.Value = -94000;
                pos1.Y.Value = 227000;
                pos1.Z.Value = 396;

                pos2.X.Value = 75000;
                pos2.Y.Value = 231000;
                pos2.Z.Value = -386;


                pos3.X.Value = -6000;
                pos3.Y.Value = 92000;
                pos3.Z.Value = 32;

                poslist.Add(pos1);
                poslist.Add(pos2);
                poslist.Add(pos3);

                double x1 = -94000;
                double y1 = 227000;
                double x2 = 75000;
                double y2 = 231000;
                double x3 = -6000;
                double y3 = 92000;

                double Dx = 0;
                double Dy = 0;
                double Ex = 0;
                double Ey = 0;
                double Fx = 0;
                double Fy = 0;

                Dx = GetCenterPoint(x1, x2);
                Dy = GetCenterPoint(y1, y2);
                Ex = GetCenterPoint(x2, x3);
                Ey = GetCenterPoint(y2, y3);
                Fx = GetCenterPoint(x3, x1);
                Fy = GetCenterPoint(y3, y1);

                double slope1 = GetSlope(x1, y1, x2, y2);
                double slope2 = GetSlope(x2, y2, x3, y3);
                double slope3 = GetSlope(x3, y3, x1, y1);

                slope1 = GetReciprocal(slope1);
                slope2 = GetReciprocal(slope2);
                slope3 = GetReciprocal(slope3);

                double resultX = 0;
                double resultY = 0;

                resultX = ((slope1 * Dx) - (slope2 * Ex) - Dy + Ey) / (slope1 - slope2);
                resultY = ((slope1 * slope2) / (slope2 - slope1)) * (((-slope1 * Dx) / slope1) + (Dy / slope1) + ((slope2 * Ex) / slope2) - (Ey / slope2));

                double r1 = GetRadius(resultX, resultY, x1, y1);
                double r2 = GetRadius(resultX, resultY, x2, y2);
                double r3 = GetRadius(resultX, resultY, x3, y3);

                double xposition = 81656 * Math.PI * Math.Cos(90) / 180;
                double yposition = 81656 * Math.PI * Math.Sin(90) / 180;




                LoggerManager.Debug($"First Point = {poslist[0].X.Value}, {poslist[0].Y.Value}, {poslist[0].Z.Value}");
                LoggerManager.Debug($"Second Point = {poslist[1].X.Value}, {poslist[1].Y.Value}, {poslist[1].Z.Value}");
                LoggerManager.Debug($"Third Point = {poslist[2].X.Value}, {poslist[2].Y.Value}, {poslist[2].Z.Value}");

                a = poslist[0].Y.Value * (poslist[1].Z.Value - poslist[2].Z.Value) + poslist[1].Y.Value
                    * (poslist[2].Z.Value - poslist[0].Z.Value) + poslist[2].Y.Value * (poslist[0].Z.Value - poslist[1].Z.Value);

                b = poslist[0].Z.Value * (poslist[1].X.Value - poslist[2].X.Value) + poslist[1].Z.Value
                    * (poslist[2].X.Value - poslist[0].X.Value) + poslist[2].Z.Value * (poslist[0].X.Value - poslist[1].X.Value);

                c = poslist[0].X.Value * (poslist[1].Y.Value - poslist[2].Y.Value) + poslist[1].X.Value
                    * (poslist[2].Y.Value - poslist[0].Y.Value) + poslist[2].X.Value * (poslist[0].Y.Value - poslist[1].Y.Value);

                d = -poslist[0].X.Value * (poslist[1].Y.Value * poslist[2].Z.Value - poslist[2].Y.Value * poslist[1].Z.Value)
                    - poslist[1].X.Value * (poslist[2].Y.Value * poslist[0].Z.Value - poslist[0].Y.Value * poslist[2].Z.Value)
                    - poslist[2].X.Value * (poslist[0].Y.Value * poslist[1].Z.Value - poslist[1].Y.Value * poslist[0].Z.Value);

                zheight = -(a * xposition + b * yposition + d) / c;
                //zOffset = zheight - Wafer.SubsInfo.AveWaferThick;
                //zValue = zpos;//+ zOffset;
                //LoggerManager.Debug($string.Format("input zpos = {0} zOffset = {1} ReturnValue = {2}", zpos, zOffset, zValue));



                List<CatCoordinates> catlist = new List<CatCoordinates>();
                for (int i = 0; i < 359; i++)
                {
                    double xpos = 94587.7 * Math.Cos(Math.PI * i / 180);
                    double ypos = 94587.7 * Math.Sin(Math.PI * i / 180);

                    a = poslist[0].Y.Value * (poslist[1].Z.Value - poslist[2].Z.Value) + poslist[1].Y.Value
                            * (poslist[2].Z.Value - poslist[0].Z.Value) + poslist[2].Y.Value * (poslist[0].Z.Value - poslist[1].Z.Value);

                    b = poslist[0].Z.Value * (poslist[1].X.Value - poslist[2].X.Value) + poslist[1].Z.Value
                        * (poslist[2].X.Value - poslist[0].X.Value) + poslist[2].Z.Value * (poslist[0].X.Value - poslist[1].X.Value);

                    c = poslist[0].X.Value * (poslist[1].Y.Value - poslist[2].Y.Value) + poslist[1].X.Value
                        * (poslist[2].Y.Value - poslist[0].Y.Value) + poslist[2].X.Value * (poslist[0].Y.Value - poslist[1].Y.Value);

                    d = -poslist[0].X.Value * (poslist[1].Y.Value * poslist[2].Z.Value - poslist[2].Y.Value * poslist[1].Z.Value)
                        - poslist[1].X.Value * (poslist[2].Y.Value * poslist[0].Z.Value - poslist[0].Y.Value * poslist[2].Z.Value)
                        - poslist[2].X.Value * (poslist[0].Y.Value * poslist[1].Z.Value - poslist[1].Y.Value * poslist[0].Z.Value);

                    zheight = -(a * xpos + b * ypos + d) / c;

                    CatCoordinates cat = new CatCoordinates();
                    cat.X.Value = xpos * -1d;
                    cat.Y.Value = ypos * -1d;
                    cat.Z.Value = zheight;
                    catlist.Add(cat);

                    //this.MotionManager().StageMove(cat.X.Value, cat.Y.Value, -86500);
                }
                var minindex = catlist.FindIndex(item => item.Z.Value == catlist.Min(value => value.Z.Value));
                var maxindex = catlist.FindIndex(item => item.Z.Value == catlist.Max(value => value.Z.Value));
                var minzvalue = catlist.Min(item => item.Z.Value);
                var maxzvalue = catlist.Max(item => item.Z.Value);


            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public double GetSlope(double x1, double y1, double x2, double y2)
        {
            double retVal = 0.0;

            try
            {
                retVal = (y2 - y1) / (x2 - x1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public double GetReciprocal(double source)
        {
            double retVal = 0.0;

            try
            {
                retVal = (1 / source) * -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public double GetCenterPoint(double startPoint, double endPoint)
        {
            double retVal = 0.0;

            try
            {
                retVal = (startPoint + endPoint) / 2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public double GetRadius(double x1, double y1, double x2, double y2)
        {
            double retVal = 0.0;

            try
            {
                retVal = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y1 - y2), 2));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private RelayCommand<object> _LoaderDoorCloseCommand;
        public ICommand LoaderDoorCloseCommand
        {
            get
            {
                if (_LoaderDoorCloseCommand == null) _LoaderDoorCloseCommand = new RelayCommand<object>(LoaderDoorCloseCmdFunc);
                return _LoaderDoorCloseCommand;
            }
        }

        private void LoaderDoorCloseCmdFunc(object noparam)
        {

            try
            {
                var ret = this.StageSupervisor().StageModuleState.LoaderDoorClose();
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }



        }

        private RelayCommand<object> _LoaderDoorOpenCommand;
        public ICommand LoaderDoorOpenCommand
        {
            get
            {
                if (_LoaderDoorOpenCommand == null) _LoaderDoorOpenCommand = new RelayCommand<object>(LoaderDoorOpenCmdFunc);
                return _LoaderDoorOpenCommand;
            }
        }

        private void LoaderDoorOpenCmdFunc(object noparam)
        {
            try
            {
                var ret = this.StageSupervisor().StageModuleState.LoaderDoorOpen();
            }
            catch (Exception err)
            {
                StageButtonsVisibility = true;
                LoggerManager.Exception(err);
            }

        }

        private RelayCommand<object> _FrontDoorUnLockCommand;
        public ICommand FrontDoorUnLockCommand
        {
            get
            {
                if (_FrontDoorUnLockCommand == null) _FrontDoorUnLockCommand = new RelayCommand<object>(FrontDoorUnLockCmdFunc);
                return _FrontDoorUnLockCommand;
            }
        }

        private void FrontDoorUnLockCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] FrontDoorUnLockCmdFunc is not implemented.");
        }

        private RelayCommand<object> _FrontDoorLockCommand;
        public ICommand FrontDoorLockCommand
        {
            get
            {
                if (_FrontDoorLockCommand == null) _FrontDoorLockCommand = new RelayCommand<object>(FrontDoorLockCmdFunc);
                return _FrontDoorLockCommand;
            }
        }

        private void FrontDoorLockCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] FrontDoorLockCmdFunc is not implemented.");
        }

        private RelayCommand<object> _TriDNCommand;
        public ICommand TriDNCommand
        {
            get
            {
                if (_TriDNCommand == null) _TriDNCommand = new RelayCommand<object>(TriDNCmdFunc);
                return _TriDNCommand;
            }
        }

        private void TriDNCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] TriDNCmdFunc is not implemented.");
        }

        private RelayCommand<object> _TriUPCommand;
        public ICommand TriUPCommand
        {
            get
            {
                if (_TriUPCommand == null) _TriUPCommand = new RelayCommand<object>(TriUPCmdFunc);
                return _TriUPCommand;
            }
        }

        private void TriUPCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] TriUPCmdFunc is not implemented.");
        }

        private RelayCommand<object> _ChuckVacOffCommand;
        public ICommand ChuckVacOffCommand
        {
            get
            {
                if (_ChuckVacOffCommand == null) _ChuckVacOffCommand = new RelayCommand<object>(ChuckVacOffCmdFunc);
                return _ChuckVacOffCommand;
            }
        }

        private void ChuckVacOffCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] ChuckVacOffCmdFunc is not implemented.");
        }

        private RelayCommand<object> _ChuckVacOnCommand;
        public ICommand ChuckVacOnCommand
        {
            get
            {
                if (_ChuckVacOnCommand == null) _ChuckVacOnCommand = new RelayCommand<object>(ChuckVacOnCmdFunc);
                return _ChuckVacOnCommand;
            }
        }

        private void ChuckVacOnCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] ChuckVacOnCmdFunc is not implemented.");
        }

        private RelayCommand<object> _FocusingCommand;
        public ICommand FocusingCommand
        {
            get
            {
                if (_FocusingCommand == null) _FocusingCommand = new RelayCommand<object>(FocusingCmdFunc);
                return _FocusingCommand;
            }
        }

        private void FocusingCmdFunc(object noparam)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] FocusingCmdFunc is not implemented.");
        }


        #endregion
    }
}
