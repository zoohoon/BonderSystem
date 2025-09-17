using System;
using System.Collections.Generic;

namespace ProberInterfaces.Lamp
{
    using Newtonsoft.Json;
    using ProberInterfaces;
    using System.Xml.Serialization;

    /*
     * ������ �Ѵ� ����, Lamp Manager�� Lamp Combination�� ���� Lamp�� ��� ���� ���� �Ѵ�.
     */
    [Serializable]
    public abstract class LampCombination : IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        public String ID { get; set; }

        private Element<LampStatusEnum> _RedLampStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> RedLampStatus
        {
            get { return _RedLampStatus; }
            set { _RedLampStatus = value; }
        }
        private Element<LampStatusEnum> _YellowLampStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> YellowLampStatus
        {
            get { return _YellowLampStatus; }
            set { _YellowLampStatus = value; }
        }
        private Element<LampStatusEnum> _BlueLampStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> BlueLampStatus
        {
            get { return _BlueLampStatus; }
            set { _BlueLampStatus = value; }
        }
        private Element<LampStatusEnum> _BuzzerStatus = new Element<LampStatusEnum>();
        public Element<LampStatusEnum> BuzzerStatus
        {
            get { return _BuzzerStatus; }
            set { _BuzzerStatus = value; }
        }

        //==> ���� ������ �켱������ ������. �켱������ ���� ���� Lamp Manager�� ���� ó�� �ؾ��� ���� ������ Ų��.
        private Element<AlarmPriority> _Priority = new Element<AlarmPriority>();
        public Element<AlarmPriority> Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        //==> Serialize/Deserialize ������ �ʿ�
        public LampCombination()
        {

        }
        public LampCombination(
            LampStatusEnum redLampStatus,
            LampStatusEnum yellowLampStatus,
            LampStatusEnum blueLampStatus,
            LampStatusEnum buzzerStatus,
            AlarmPriority priority,
            String id)
        {
            RedLampStatus.Value = redLampStatus;
            YellowLampStatus.Value = yellowLampStatus;
            BlueLampStatus.Value = blueLampStatus;
            BuzzerStatus.Value = buzzerStatus;
            Priority.Value = priority;
            ID = id;
        }
    }
}
