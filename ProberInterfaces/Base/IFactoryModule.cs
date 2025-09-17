using ProberErrorCode;

namespace ProberInterfaces
{
    using Autofac;
    public interface IFactoryModule
    {
    }

    public interface ILoaderFactoryModule
    {
        /// <summary>
        /// InitPriority �� �����ɴϴ�.
        /// </summary>
        InitPriorityEnum InitPriority { get; }

        /// <summary>
        /// ����� �ʱ�ȭ �մϴ�.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        EventCodeEnum InitModule(IContainer container);

        /// <summary>
        /// ����� �ı��մϴ�.
        /// </summary>
        void DeInitModule();
    }

    /// <summary>
    /// InitPriority �� �����մϴ�.
    /// </summary>
    public enum InitPriorityEnum
    {
        /// <summary>
        /// LEVEL1
        /// </summary>
        LEVEL1 = 1000,
        /// <summary>
        /// LEVEL2
        /// </summary>
        LEVEL2 = 2000,
        /// <summary>
        /// LEVEL3
        /// </summary>
        LEVEL3 = 3000,
        /// <summary>
        /// LEVEL4
        /// </summary>
        LEVEL4 = 4000,
        /// <summary>
        /// LEVEL5
        /// </summary>
        LEVEL5 = 5000,
    }
}
