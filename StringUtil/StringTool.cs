using System;
using System.Collections.Generic;

namespace StringUtil
{
    public static class StringTool
    {
        public static String GetMarkup(String context, char openMarkChar, char closeMarkChar)
        {
            String content = String.Empty;
            try
            {

                int openMarkCharIdx = context.IndexOf(openMarkChar);
                if (openMarkCharIdx == -1)
                    return content;

                int closeMarkCharIdx = context.IndexOf(closeMarkChar, openMarkCharIdx);
                if (closeMarkCharIdx == -1)
                    return content;

                int idxStrContentLen = closeMarkCharIdx - openMarkCharIdx;

                if (idxStrContentLen < 1)
                    return content;

                content = context.Substring(openMarkCharIdx, idxStrContentLen + 1);

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return content;
        }
        public static String GetMarkupBack(String context, char openMarkChar, char closeMarkChar)
        {
            String content = String.Empty;
            try
            {

                int openMarkCharIdx = context.LastIndexOf(openMarkChar);
                if (openMarkCharIdx == -1)
                    return content;

                int closeMarkCharIdx = context.IndexOf(closeMarkChar, openMarkCharIdx);
                if (closeMarkCharIdx == -1)
                    return content;

                int idxStrContentLen = closeMarkCharIdx - openMarkCharIdx;

                if (idxStrContentLen < 1)
                    return content;

                content = context.Substring(openMarkCharIdx, idxStrContentLen + 1);

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return content;
        }
        public static List<String> GetAllMarkup(String context, char openMarkChar, char closeMarkChar)
        {
            List<String> markupList = new List<String>();
            try
            {

                while (true)
                {
                    String content = String.Empty;

                    int openMarkCharIdx = context.IndexOf(openMarkChar);
                    if (openMarkCharIdx == -1)
                        break;

                    int closeMarkCharIdx = context.IndexOf(closeMarkChar, openMarkCharIdx);
                    if (closeMarkCharIdx == -1)
                        break;

                    int idxStrContentLen = closeMarkCharIdx - openMarkCharIdx;

                    if (idxStrContentLen < 1)
                        break;

                    content = context.Substring(openMarkCharIdx, idxStrContentLen + 1);
                    markupList.Add(content);

                    context = context.Substring(openMarkCharIdx + idxStrContentLen + 1);
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return markupList;
        }
        public static String GetMarkupContent(String markup)
        {
            if (markup.Length < "(*)".Length)
                return String.Empty;

            String content = markup.Remove(0, 1);
            try
            {
                content = content.Remove(content.Length - 1, 1);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return content;
        }

        /// <summary>
        ///  ���� ���ڿ��� Path ������ �� ���� Sub list�� parsing�Ͽ� �ٽ� �� full path�� ����� ��ȯ.
        /// </summary>
        /// <param name="path_list"> ��ȯ������ </param>
        /// <param name="target_path"> parsing ���</param>
        /// <param name="div"> ������ </param>
        /// <param name="first_searchPos"> parsing ������ ��ġ</param>
        /// ex) first_searchPos_Idx: 6 , target str: ftp://192.168.9.251/Test_1/Test_2/Test_3 
        /// ret item 1: ftp://192.168.9.251/Test_1
        /// ret item 2: ftp://192.168.9.251/Test_1/Test_2
        /// ret item 3: ftp://192.168.9.251/Test_1/Test_2/Test_3
        public static void ParsingForPathTypeToSubPath(ref List<string> path_list, string target_path, string div, int first_searchPos_Idx)
        {
            int nBasePos = first_searchPos_Idx;
            nBasePos = target_path.IndexOf(div, nBasePos);
            if (-1 != nBasePos)
            {
                while (true)
                {
                    int nTempPos = nBasePos;
                    int nNextFindPosBase = nTempPos + 1;
                    if (nTempPos == target_path.Length - 1) /// ������ ���ڿ� �������� ���
                        break;

                    nBasePos = target_path.IndexOf(div, nNextFindPosBase);
                    if (-1 != nBasePos)
                    {
                        string strCheckPath = target_path.Substring(0, nBasePos);
                        path_list.Add(strCheckPath);
                    }
                    else
                    {
                        if (nTempPos <= target_path.Length - 1) //< ������ ���ڿ��� ���� �ִ� ���                        
                            path_list.Add(target_path);

                        break;
                    }
                }
            }
        }

    }
}
