using System;

namespace Cognex.Command
{
    using LogModule;
    using System.Xml;

    /*
     * Cognex Command, �տ� EV�� �ٴ´�.
     */
    public abstract class CognexEVCommand
    {
        private String _Command;
        public String Command
        {
            get { return "EV " + _Command; }
        }
        protected String WafID { get; set; }
        protected String Config { get; set; }
        public String CommandFormatFrame { get; set; }
        public String Status { get; set; }
        public CognexEVCommand()
        {
            try
            {
                WafID = "A4";
                Config = "0";

                GenerateCommandFormatFrame(WafID, Config);

                _Command = CommandFormatFrame;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public bool GenerateCommandFormatFrame(String waferID, String config)
        {
            if (String.IsNullOrEmpty(WafID))
                return false;

            if (String.IsNullOrEmpty(Config))
                return false;

            CommandFormatFrame = $"{this.GetType().Name}({waferID},{config})";

            return true;
        }
        private String AppendCommandFormatArg(int argCount)
        {
            if (String.IsNullOrEmpty(CommandFormatFrame))
                return String.Empty;

            //==> Remove last )

            String commandFormat = CommandFormatFrame.Remove(CommandFormatFrame.Length - 1);
            try
            {

                //==> Add Argument
                for (int i = 0; i < argCount; i++)
                    commandFormat = commandFormat + ",{" + i + "}";

                //==> Close )
                commandFormat = commandFormat + ")";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return commandFormat;
        }
        public bool SetCommandFormatArg(params object[] args)
        {
            /*
             * _Command�� ���ڸ� �����δ�.
             */

            try
            {
                if (String.IsNullOrEmpty(CommandFormatFrame))
                {
                    return false;
                }

                String commandFormat = AppendCommandFormatArg(args.Length);
                if (String.IsNullOrEmpty(commandFormat))
                {
                    return false;
                }

                _Command = String.Format(commandFormat, args);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return true;
        }
        public virtual bool ParseResponse(String response)
        {
            /*
             * ���� Command�� ���� ����(response)�� �ް� �Ľ��� �Ѵ�.
             * response�� XML ����� ���ڿ��̴�
             */

            bool result = false;

            try
            {
                if (String.IsNullOrEmpty(response))
                    return false;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                using (XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc))
                {
                    Status = GetNextNodeValue(xmlReader, nameof(Status));
                }
                do
                {
                    if (Status == "1")
                    {
                        result = true;
                        break;
                    }
                    if (Status == "0")//==> Unrecognized command.
                        break;
                    if (Status == "-2")//==> The command could not be executed.
                        break;
                    if (Status == "6")//==>User does not have Full Access to execute the command.
                        break;
                } while (false);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }
        protected String GetNextNodeValue(XmlNodeReader xmlReader, String nodeName)
        {
            /*
             * XML���� ���� ��� ���� ������
             * 
             * EX)
             * <Field></Field>
             * <String></String>
             * 
             * Field �� ���� ���� String�̴�.
             */
            try
            {
                xmlReader.ReadToFollowing(nodeName);
                xmlReader.Read();
                return xmlReader.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected String GetNextNodeAttribute(XmlNodeReader xmlReader, String nodeName, String attribute)
        {
            /*
             * XML���� ���� Attribute�� ���� �´�.
             * <Field id="A" value="10"></Field>
             * 
             * 'id' Attribute�� ���� 'Attribute'�� value �̴�.
             */
            try
            {
                xmlReader.ReadToFollowing(nodeName);
                return xmlReader.GetAttribute(attribute);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
