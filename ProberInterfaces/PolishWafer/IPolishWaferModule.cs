using LogModule;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ProberInterfaces.PolishWafer
{
    [ServiceContract]
    public interface IPolishWaferModule : IStateModule, IHasDevParameterizable, ITemplateStateModule, IManualOPReadyAble
    {
        IParam PolishWaferParameter { get; set; }

        [OperationContract]
        IParam GetPolishWaferIParam();

        [OperationContract]
        void SetPolishWaferIParam(byte[] param);
        [OperationContract]
        bool PWIntervalhasLotstart(int index = -1);
        [OperationContract]
        EventCodeEnum DoCentering(IPolishWaferCleaningParameter param);
        [OperationContract]
        EventCodeEnum DoFocusing(IPolishWaferCleaningParameter param);
        [OperationContract]
        EventCodeEnum DoCleaning(IPolishWaferCleaningParameter param);
        //[OperationContract]
        //EventCodeEnum SelectIntervalWafer();
        //[OperationContract]
        //EventCodeEnum UnLoadPolishWafer();
        //[OperationContract]
        //EventCodeEnum LoadPolishWafer(string definetype);
        bool IsReadyPolishWafer();
        [OperationContract]
        EventCodeEnum PolishWaferValidate(bool isExist);

        [OperationContract]
        byte[] GetPolishWaferParam();

        [OperationContract]
        bool IsServiceAvailable();

        void SetDevParam(byte[] param);

        //EventCodeEnum DoPolishWaferCleaning(byte[] param);

        Task<EventCodeEnum> DoManualPolishWaferCleaning(byte[] param);
        Task<EventCodeEnum> ManualPolishWaferFocusing(byte[] param);


        IPolishWaferCleaningParameter ManualCleaningParam { get; set; }
        PolishWafertCleaningInfo ManualCleaningInfo { get; set; }
        bool NeedAngleUpdate { get; set; }

        void InitTriggeredData();

        bool IsRequested(ref string RequestedWaferName);

        bool LotStartFlag { get; set; }
        bool LotEndFlag { get; set; }

        int RestoredMarkedWaferCountLastPolishWaferCleaning { get; }
        int RestoredMarkedTouchDownCountLastPolishWaferCleaning { get; }
        bool IsExecuteOfScheduler();
        List<IPolishWaferIntervalParameter> GetPolishWaferIntervalParameters();

        IPolishWaferControlItems PolishWaferControlItems { get; set; }

        IPolishWaferIntervalParameter GetCurrentIntervalParam();
        IPolishWaferCleaningParameter GetCurrentCleaningParam();

        PolishWaferProcessingInfo ProcessingInfo { get; set; }

    }

    public enum PolishWaferModuleStateEnum
    {
        IDLE = 0,
        RUNNING,
        REQUESTING,
        WAITLOADWAFER,
        //WAITUNLOADWAFER, 
        CLEANING_WAFER_LOADED,
        //CLEANING_WAFER_UNLOADED,
        SUSPENDED,
        DONE,
        ERROR,
        PAUSED,
        ABORT,
    }

    public interface IPolishWaferControlItems
    {
        bool POLISHWAFER_CENTERING_ERROR { get; set; }
        bool POLISHWAFER_FOCUSING_ERROR { get; set; }
        bool POLISHWAFER_CLEAING_ERROR { get; set; }
        bool POLISHWAFER_CLEANING_MARGIN_EXCEEDED { get; set; }
    }

    public class PolishWafertIntervalInfo
    {
        public PolishWafertIntervalInfo(string code)
        {
            this.HashCode = code;
        }

        public string HashCode { get; set; }

        private bool _Triggered;
        public bool Triggered
        {
            get { return _Triggered; }
            set
            {
                if (value != _Triggered)
                {
                    _Triggered = value;
                }
            }
        }

        private int _TriggeredInterval = 0;
        public int TriggeredInterval
        {
            get { return _TriggeredInterval; }
            set
            {
                if (value != _TriggeredInterval)
                {
                    _TriggeredInterval = value;
                }
            }
        }

        private int _TriggeredTouchDown = 0;
        public int TriggeredTouchDown
        {
            get { return _TriggeredTouchDown; }
            set
            {
                if (value != _TriggeredTouchDown)
                {
                    _TriggeredTouchDown = value;
                }
            }
        }

        private List<PolishWafertCleaningInfo> _CleaningInfos = new List<PolishWafertCleaningInfo>();
        public List<PolishWafertCleaningInfo> CleaningInfos
        {
            get { return _CleaningInfos; }
            set
            {
                if (value != _CleaningInfos)
                {
                    _CleaningInfos = value;
                }
            }
        }
    }

    public class PolishWafertCleaningInfo
    {
        public PolishWafertCleaningInfo(string code)
        {
            this.HashCode = code;
        }

        public string HashCode { get; set; }

        // PinAlign Before Polish Wafer Cleaning ����� �ߵ��Ǿ��ٴ� ���� �÷���
        // Polishwafer ����� IDLE���� RUN���� ó�� �Ѿ �� �ʱ�ȭ �Ѵ�.
        private bool _PinAlignBeforeCleaningProcessed = new bool();
        public bool PinAlignBeforeCleaningProcessed
        {
            get { return _PinAlignBeforeCleaningProcessed; }
            set
            {
                if (value != _PinAlignBeforeCleaningProcessed)
                {
                    _PinAlignBeforeCleaningProcessed = value;
                }
            }
        }

        // PinAlign After Polish Wafer Cleaning ����� �ߵ��Ǿ��ٴ� ���� �÷���
        // Polishwafer ����� IDLE���� RUN���� ó�� �Ѿ �� �ʱ�ȭ �Ѵ�.
        private bool _PinAlignAfterCleaningProcessed = new bool();
        public bool PinAlignAfterCleaningProcessed
        {
            get { return _PinAlignAfterCleaningProcessed; }
            set
            {
                if (value != _PinAlignAfterCleaningProcessed)
                {
                    _PinAlignAfterCleaningProcessed = value;
                }
            }
        }

        // Polish Wafer �� ����Ǿ��ٴ� ���� �÷���
        // Pin Align After Cleaning�� ��� �տ��� Ŭ������ ����ǰ� �ڱ� ���� �� ������, SKIP �ǰ� �� ������ ������ �Ұ����ϴ�.
        // �׷��⿡ ������ Ŭ������ ����Ǿ����� �ƴ����� �տ��� �˷� �־�� �Ѵ�.
        // �� �÷��״� Ŭ������ ����ϴ� ���μ��� ��⿡�� ���ְ�, Pin Align After Cleaning ��⿡�� ���ش�.
        private bool _PolishWaferCleaningProcessed = new bool();
        public bool PolishWaferCleaningProcessed
        {
            get { return _PolishWaferCleaningProcessed; }
            set
            {
                if (value != _PolishWaferCleaningProcessed)
                {
                    _PolishWaferCleaningProcessed = value;
                }
            }
        }


        // Polish Wafer �� �� ���� �Ǿ�� ���� �˸��� �÷���
        // Ŭ������ ���� �Ǳ� ���̰� wafer�� chuck �� �ε� �Ǿ� ���� ��� error -> pause -> idle ���� �ٷ� Ŭ���� ���� state �� PolishWaferLoadedState �� ���� �ϱ� ����
        // Pin Align After Cleaning�� ��� �տ��� Ŭ������ ����Ǿ� PolishWaferCleaningProcessed �� true �̰�, PolishWaferCleaningRetry �� false ������.
        // Module.DoCleaningProcessing(); �� �Ҹ����� ���� ���� ���� �̹� ���� �ߴٰ� ���� �Ұ��̴�.
        // �� �÷��״� Ŭ������ ����ϴ� ���μ��� ��⿡�� ���ְ� Ŭ������ �Ϸ� �Ǹ� ���ش�.
        private bool _PolishWaferCleaningRetry = new bool();
        public bool PolishWaferCleaningRetry
        {
            get { return _PolishWaferCleaningRetry; }
            set
            {
                if (value != _PolishWaferCleaningRetry)
                {
                    _PolishWaferCleaningRetry = value;
                }
            }
        }
    }

    /// <summary>
    /// PolishWafer ���� �� ���Ǵ� �Ķ���Ϳ� �׿� ���� �� �÷��׵��� �����ϱ� ���� Ŭ����
    /// </summary>
    public class PolishWaferProcessingInfo
    {
        private List<PolishWafertIntervalInfo> _IntervalInfos = new List<PolishWafertIntervalInfo>();
        public List<PolishWafertIntervalInfo> IntervalInfos
        {
            get { return _IntervalInfos; }
            set
            {
                if (value != _IntervalInfos)
                {
                    _IntervalInfos = value;
                }
            }
        }
        public PolishWaferProcessingInfo()
        {
            Init();
        }

        public string CurrentIntervalHashCode { get; set; }
        public string CurrentCleaningHashCode { get; set; }

        public bool isValid { get; set; }

        public void Init()
        {
            try
            {
                IntervalInfos.Clear();

                this.CurrentIntervalHashCode = string.Empty;
                this.CurrentCleaningHashCode = string.Empty;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ResetAllCleaningProcessedParam(string targetcode)
        {
            try
            {
                var info = GetIntervalInfo(targetcode);

                if (info != null)
                {
                    foreach (var item in info.CleaningInfos)
                    {
                        item.PinAlignBeforeCleaningProcessed = false;
                        item.PolishWaferCleaningProcessed = false;
                        item.PinAlignAfterCleaningProcessed = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private PolishWafertIntervalInfo GetIntervalInfo(string targetcode)
        {
            PolishWafertIntervalInfo retval = null;

            try
            {
                if (IntervalInfos != null)
                {
                    retval = IntervalInfos.Find(x => x.HashCode == targetcode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PolishWafertIntervalInfo GetCurrentIntervalInfo()
        {
            PolishWafertIntervalInfo retval = null;

            try
            {
                if (IntervalInfos != null)
                {
                    retval = IntervalInfos.Find(x => x.HashCode == this.CurrentIntervalHashCode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PolishWafertCleaningInfo GetCurrentCleaningInfo()
        {
            PolishWafertCleaningInfo retval = null;

            try
            {
                var intervalinfo = GetCurrentIntervalInfo();

                if (intervalinfo != null)
                {
                    retval = intervalinfo.CleaningInfos.Find(x => x.HashCode == this.CurrentCleaningHashCode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool GetIntervalTrigger(string targetcode)
        {
            bool retval = false;
            try
            {
                var info = GetIntervalInfo(targetcode);

                if (info != null)
                {
                    retval = info.Triggered;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetIntervalTrigger(string targetcode, bool triggered)
        {
            try
            {
                if (IntervalInfos != null)
                {
                    var info = IntervalInfos.Find(x => x.HashCode == targetcode);

                    if (info != null)
                    {
                        info.Triggered = triggered;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetIntervalTrigger(IPolishWaferIntervalParameter intervalParam, bool triggered)
        {
            try
            {
                if (IntervalInfos != null)
                {
                    var info = IntervalInfos.Find(x => x.HashCode == intervalParam.HashCode);

                    if (info != null)
                    {
                        info.Triggered = triggered;
                    }
                    else
                    {
                        PolishWafertIntervalInfo intervalinfo = new PolishWafertIntervalInfo(intervalParam.HashCode);
                        intervalinfo.Triggered = triggered;

                        if (intervalinfo.CleaningInfos == null)
                        {
                            intervalinfo.CleaningInfos = new List<PolishWafertCleaningInfo>();
                        }

                        foreach (var item in intervalParam.CleaningParameters)
                        {
                            PolishWafertCleaningInfo cleaningInfo = new PolishWafertCleaningInfo(item.HashCode);

                            intervalinfo.CleaningInfos.Add(cleaningInfo);
                        }

                        IntervalInfos.Add(intervalinfo);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCurrentPolishWaferCleaningRetry(bool param)
        {
            try
            {
                var cleaninginfo = GetCurrentCleaningInfo();

                if (cleaninginfo != null)
                {
                    cleaninginfo.PolishWaferCleaningRetry = param;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public bool GetCurrentPolishWaferCleaningRetry()
        {
            bool retval = false;

            try
            {
                var cleaninginfo = GetCurrentCleaningInfo();

                if (cleaninginfo != null)
                {
                    retval = cleaninginfo.PolishWaferCleaningRetry;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsRemainingCleaningParam()
        {
            bool retval = false;

            try
            {
                var intervalinfo = GetCurrentIntervalInfo();

                if (intervalinfo != null)
                {
                    // �̹� ����� Ŭ������ ���, PolishWaferCleaningProcessed = true
                    // ����, Ŭ������ ���� ������� ���� ù �����͸� ����ϸ� �ȴ�.
                    var RemainCleaingInfo = intervalinfo.CleaningInfos.Find(x => x.PolishWaferCleaningProcessed == false);

                    if (RemainCleaingInfo != null)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsRemainingCleaningParam(string intervalHashCode, ref string cleaningHashCode)
        {
            bool retval = false;

            try
            {
                if (IntervalInfos != null)
                {
                    var intervalinfo = IntervalInfos.Find(x => x.HashCode == intervalHashCode);

                    if (intervalinfo != null)
                    {
                        // �̹� ����� Ŭ������ ���, PolishWaferCleaningProcessed = true
                        // ����, Ŭ������ ���� ������� ���� ù �����͸� ����ϸ� �ȴ�.
                        var RemainCleaingInfo = intervalinfo.CleaningInfos.Find(x => x.PolishWaferCleaningProcessed == false);

                        if (RemainCleaingInfo != null)
                        {
                            retval = true;
                            cleaningHashCode = RemainCleaingInfo.HashCode;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetCurrentData(string intervalHashcode, string cleaningHashcode)
        {
            this.CurrentIntervalHashCode = intervalHashcode;
            this.CurrentCleaningHashCode = cleaningHashcode;
        }
    }
}
