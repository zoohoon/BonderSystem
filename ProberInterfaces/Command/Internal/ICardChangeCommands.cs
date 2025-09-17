namespace ProberInterfaces.Command.Internal
{
    #region Stage Command 
    public interface IRUNCARDCHANGECOMMAND : IProbeCommand
    {
    }
    public interface IStagecardChangeStart : IProbeCommand
    {
    }
    public interface IAbortcardChange : IProbeCommand
    {
    }


    #endregion

    #region Loader Command
    /// <summary>
    /// PGV�� ����� CC ������(ActiveCCInfo)�� SequnceId
    /// ActiveCCInfo�� �Ѳ����� ������ ���� �ʰ� id�� �����ؼ� ActiveCCInfoList���� ã�Ƽ� ����ϱ� ����. 
    /// // TODO: ���� Start���Ŀ� ������ �������¿��� AllocateActiveCCInfo�� �� �Ҹ��ٰ��ϸ� TRANSFERED ������ TRANSFER_READY�� �;���. 
    /// </summary>
    public class RequestCCJobInfo : ProbeCommandParameter
    {
        public string allocSeqId { get; set; }
    }

    public class TransferObjectHolderInfo : ProbeCommandParameter
    {
        public object source { get; set; }
        public object target { get; set; }
    }

    public interface ITransferObject : IProbeCommand
    {
    }

    public interface IStartCardChangeSequence : IProbeCommand
    {
    }
    public interface IAbortCardChangeSequence : IProbeCommand
    {
    }
   

    #endregion
}
