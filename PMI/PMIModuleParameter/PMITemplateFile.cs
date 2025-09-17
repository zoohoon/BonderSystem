using System;
using System.Collections.Generic;
using System.Linq;

namespace PMIModuleParameter
{
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberInterfaces.Template;
    using System.IO;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using LogModule;
    using Newtonsoft.Json;

    [Serializable]
    public class PMITemplateFile : INotifyPropertyChanged, ITemplateFileParam, IDeviceParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        [ParamIgnore]
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




        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "PMIModule";
        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "PMITemplateFile.json";

        private ITemplateParameter _Param = new TemplateParameter();
        public ITemplateParameter Param
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PMITemplateFile()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = SetDefaultParam();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                Param.BasePath = @"PMIModule\Template\";

                Param.TemplateInfos.Add(new TemplateInfo("Standard", Param.BasePath + "StandardTemplate.json"));

                Param.SeletedTemplate = Param.TemplateInfos[0];

                RetVal = SetDefaultTemplates();

                //Param.BasePath = this.FileManager().GetDeviceParamFullPath(@"WaferAlignParam\Template\", "");
                ////Param.BasePath = @"C:\ProberSystem\Default\Parameters\DeviceParam\DEFAULTDEVNAME\WaferAlignParam\Template\";
                //Param.TemplateInfos.Add(new TemplateInfo("Standard", Param.BasePath + "StandardTemplate.xml"));
                //Param.TemplateInfos.Add(new TemplateInfo("Development", Param.BasePath + "DevelopmentTemplate.xml"));
                //Param.SeletedTemplate = Param.TemplateInfos[1];

                //RetVal = SetDefaultTemplates();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public string GetFilePath()
        {
            return FilePath;
        }

        public string GetFileName()
        {
            return FileName;
        }

        public EventCodeEnum SetDefaultTemplates()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (this is IDeviceParameterizable)
                {
                    retVal = CreateStandardDllList(
                        this.FileManager().GetDeviceParamFullPath(Param.TemplateInfos[0].Path.Value));
                }
                else if (this is ISystemParameterizable)
                {
                    retVal = CreateStandardDllList(
                    this.FileManager().GetSystemParamFullPath(Param.TemplateInfos[0].Path.Value, null));
                }


            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        private EventCodeEnum CreateStandardDllList(string path)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                if (!File.Exists(path))
                {


                    TemplateFileParam file = new TemplateFileParam();
                    ModuleInfo moduleinfo;
                    CategoryInfo categoryinfo;

                    file.SubRoutineModule = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"PMIModuleSubRutineStandard.dll",
                                     @"PMIModuleSubRutineStandard",
                                     1000,
                                     true));

                    moduleinfo = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"PMIProcesser.dll",
                                     @"NormalPMIProcessModule",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);

                    moduleinfo = new ModuleInfo(
                            new ModuleDllInfo(
                                     @"PMIScheduler.dll",
                                     @"PMIScheduleModule",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);

                    #region ==> Manual PMI TEST
                    moduleinfo = new ModuleInfo(
                             new ModuleDllInfo(
                                     @"PMIProcesser.dll",
                                     @"ManualPMIProcessModule",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);
                    #endregion

                    #region ==> Pad Template
                    categoryinfo = new CategoryInfo("Pad Template");
                    categoryinfo.Categories = new ObservableCollection<CategoryModuleBase>();

                    categoryinfo.Categories.Add(new ModuleInfo(
                        new ModuleDllInfo(
                                 @"PMISetup.dll",
                                 @"PadTemplateSetupModule",
                                 1000,
                                 true)));

                    categoryinfo.Categories.Add(new ModuleInfo(
                        new ModuleDllInfo(
                                 @"PMISetup.dll",
                                 @"PadJudgingWindowSetupModule",
                                 1000,
                                 true)));

                    categoryinfo.Categories.Add(new ModuleInfo(
                        new ModuleDllInfo(
                                 @"PMISetup.dll",
                                 @"ProbeMarkSizeSetupModule",
                                 1000,
                                 true)));

                    file.AddTemplateModules(categoryinfo);
                    #endregion

                    #region ==> Pad Registration
                    categoryinfo = CreatePadRegistrationCategory();
                    file.AddTemplateModules(categoryinfo);
                    #endregion

                    #region ==> PMI Map Setup
                    //categoryinfo = new CategoryInfo("PMI Map Setup");
                    //categoryinfo.Categories = new ObservableCollection<CategoryModuleBase>();

                    //categoryinfo.Categories.Add(new ModuleInfo(
                    //        new ModuleDllInfo(
                    //                 @"PMISetup.dll",
                    //                 @"NormalPMIMapSetupModule",
                    //                 1000,
                    //                 true)));

                    moduleinfo = new ModuleInfo(
                             new ModuleDllInfo(
                                     @"PMISetup.dll",
                                     @"NormalPMIMapSetupModule",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);
                    #endregion

                    #region ==> Parmeter Setup
                    moduleinfo = new ModuleInfo(
                             new ModuleDllInfo(
                                     @"PMISetup.dll",
                                     @"PMIParameterSettingModule",
                                     1000,
                                     true));
                    file.AddTemplateModules(moduleinfo);
                    #endregion


                    //=======Control==========

                    //PMI                    
                    file.AddControlTemplate(new ControlTemplateParameter() {
                        ControlPNPButtons = new ObservableCollection<ControlPNPButton>() { CreatePNPButton() }
                    });


                    retVal = Extensions_IParam.SaveParameter(null, file, null, path);
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        /// <summary>
        /// PNPButton ��� ����
        /// </summary>
        /// <returns></returns>
        private static ControlPNPButton CreatePNPButton()
        {
            var newPNPButton = new ControlPNPButton()
            {
                CuiBtnGUID = PMIModuleKeyWordInfo.CUIButtonGUID_PMI,
                ViewGuid = PMIModuleKeyWordInfo.ScreenGUID_UcPnpControl,
                StepGUID = new List<Guid>() {
                    PMIModuleKeyWordInfo.ScreenGUID_PadTemplateSetup,
                    PMIModuleKeyWordInfo.ScreenGUID_PadJudgingwindowSetup,
                    PMIModuleKeyWordInfo.ScreenGUID_ProbeMarkSizeSetup,
                    PMIModuleKeyWordInfo.ScreenGUID_PadAutoRegistration,
                    PMIModuleKeyWordInfo.ScreenGUID_PadManualRegistration,
                    PMIModuleKeyWordInfo.ScreenGUID_PadPositionAdjustment,
                    PMIModuleKeyWordInfo.ScreenGUID_PadTableSetup,
                    PMIModuleKeyWordInfo.ScreenGUID_NormalPMIMapSetup,
                    PMIModuleKeyWordInfo.ScreenGUID_PMIParameterSetup,
                    PMIModuleKeyWordInfo.ScreenGUID_ManualPMITest,
                    PMIModuleKeyWordInfo.ScreenGUID_PatternInPadRegistration
                }
            };
            return newPNPButton;
        }

        /// <summary>
        /// �е� ��� ī�װ� ����
        /// </summary>
        /// <returns></returns>
        public static CategoryInfo CreatePadRegistrationCategory()
        {
            CategoryInfo categoryinfo = new CategoryInfo(PMIModuleKeyWordInfo.CategoryPadReg)
            {
                Categories = new ObservableCollection<CategoryModuleBase>()
                {   
                    //CreatePMIModule(PMIModuleKeyWordInfo.ClassAutoReg),// Not Ready
                    CreatePMIModule(PMIModuleKeyWordInfo.ClassManualReg),
                    CreatePMIModule(PMIModuleKeyWordInfo.ClassPosAdjust),
                    CreatePMIModule(PMIModuleKeyWordInfo.ClassSetup),
                    CreatePMIModule(PMIModuleKeyWordInfo.ClassPatternReg)
                }
            };
            return categoryinfo;
        }

        /// <summary>
        ///  ���� ī�װ��� ��� �߰� (�����ϸ� �߰����� ����)
        /// </summary>
        /// <param name="category"> ī�װ�</param>
        /// <param name="className">�߰��� ��� �̸�</param>
        public static void CategoryModuleInjection(ref CategoryInfo category, string className)
        {
            if(category.Categories?.Where(x=> ModuleConatinClassName(x, className)).FirstOrDefault() == null)
            {
                category.Categories.Add(CreatePMIModule(className));
            }
        }

        /// <summary>
        /// ��ư�� ���� �߰�. (�����ϸ� �߰����� ����)
        /// </summary>
        /// <param name="button">��ư</param>
        /// <param name="screenGuid">�߰��� ����</param>
        public static void PNPButtonInjection(ref ControlPNPButton button, Guid screenGuid)
        {
            if(!button.StepGUID.Contains(screenGuid))
            {
                button.StepGUID.Add(screenGuid);
            }
        }

        /// <summary>
        /// ��⿡ Ư�� Ŭ���� �̸��� ���ԵǾ� �ִ��� Ȯ��
        /// </summary>
        /// <param name="module">Ȯ�� �Ϸ��� ���</param>
        /// <param name="className">ã�� Ŭ���� �̸�</param>
        /// <returns></returns>
        private static bool ModuleConatinClassName(CategoryModuleBase module, string className)
        {
            if (string.IsNullOrWhiteSpace(className)) //ã������ Ŭ���� �̸� ����.( ����)
            {
                return false;
            }

            if(module is ModuleInfo i) 
            {
                var findClass = i?.DllInfo?.ClassName?.Where(x => string.Compare(x, className) == 0);
                if(findClass==null || !findClass.Any())
                {
                    return false;
                }

                return true;
            }
            return false;
        }


        #region  �е� ��� ���
        /// <summary>
        /// �߰��� ��� ���� ���� Wrapper,  PMISetup.dll �������� ����.
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static ModuleInfo CreatePMIModule(string className)
        {
            return new ModuleInfo(
                new ModuleDllInfo ( 
                    PMIModuleKeyWordInfo.Module,
                    className,
                    PMIModuleKeyWordInfo.VerionNumber,
                    PMIModuleKeyWordInfo.VerionCompatibility
                    )
                );
        }

        
        #endregion
    }

    /// <summary>
    /// PMI ��⿡�� ���Ǵ� Ű���� ����.
    /// </summary>
    public static class PMIModuleKeyWordInfo
    {
        public const int VerionNumber = 1000;
        public const bool VerionCompatibility = true; //���� ���� ȣȯ�� ���

        public const string Module = @"PMISetup.dll";
        public const string ClassAutoReg = @"PadAutoRegistrationModule";
        public const string ClassManualReg = @"PadManualRegistrationModule";
        public const string ClassPosAdjust = @"PadPositionAdjustModule";
        public const string ClassSetup = @"PadTableSetupModule";
        public const string ClassPatternReg = @"PatternInPadRegistrationModule";

        public const string CategoryPadReg = @"Pad Registration";
               
        public static readonly Guid CUIButtonGUID_PMI = new Guid("103482a3-ad4c-4e9f-88fe-a4df5877924b");
               
        public static readonly Guid ScreenGUID_UcPnpControl = new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0");
               
        public static readonly Guid ScreenGUID_PadTemplateSetup = new Guid("EAE0AD9C-EB31-72B1-BC97-BA31F2161F38");
        public static readonly Guid ScreenGUID_PadJudgingwindowSetup = new Guid("0FE97FE5-5774-9D69-732E-9DFD9C3C93BF");
        public static readonly Guid ScreenGUID_ProbeMarkSizeSetup = new Guid("96F03BEF-D3EE-05DB-464E-2A9F5C059085");
        public static readonly Guid ScreenGUID_PadAutoRegistration = new Guid("467070CE-2C29-6610-9005-728C4DA1658D");
        public static readonly Guid ScreenGUID_PadManualRegistration = new Guid("9777B59C-FCCB-168F-FE6C-0D378163B7FD");
        public static readonly Guid ScreenGUID_PadPositionAdjustment = new Guid("FEFE762A-4993-1370-127A-F7817B69A098");
        public static readonly Guid ScreenGUID_PadTableSetup = new Guid("EF7414A4-1663-7B72-F039-8E0853B32DD1");
        public static readonly Guid ScreenGUID_NormalPMIMapSetup = new Guid("EAF50F0A-D36D-86BB-AB6B-633A98474483");
        public static readonly Guid ScreenGUID_PMIParameterSetup = new Guid("61EFD80F-0029-BEE1-125F-2F1112A42C4D");
        public static readonly Guid ScreenGUID_ManualPMITest = new Guid("08C25DD4-C7BB-35CE-DBE5-44571E2C2068");
        public static readonly Guid ScreenGUID_PatternInPadRegistration = new Guid("C50ADE93-3B82-432E-807B-87C4E28E776A");

        /// <summary>
        /// ����� �е� ���ø��� ���۷��� �̹��� ���
        /// </summary>
        /// <param name="fileManager">���� �Ŵ��� ex) this.FileManager()</param>
        /// <param name="selectedIndex">�е� ���ø� �ε���</param>
        /// <returns></returns>
        public static string PadTemplateReferencePath(IFileManager fileManager, int selectedIndex)
        {
            if (fileManager == null)
                return null;

            string Save_Extension = ".bmp";
            return System.IO.Path.Combine(
                fileManager.FileManagerParam.DeviceParamRootDirectory,
                fileManager.FileManagerParam.DeviceName,
                "PMIModule",
                $"Reference_Image{selectedIndex}{Save_Extension}");
        }
    }
}
