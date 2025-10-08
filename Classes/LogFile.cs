// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.classes.LogFile
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.IO;
using System.Text;

//- only form later versions #nullable disable
namespace TrackerDotNet.Classes
{
    public class LogFile
    {
        private StringBuilder _LogLines;
        private string _LogFileName;
        private bool _AppendFile;
        private bool _NewLine;

        public LogFile(string pLogFileName, bool pAppendFile)
        {
            this._LogLines = new StringBuilder();
            this._LogFileName = pLogFileName;
            this._AppendFile = pAppendFile;
            this._NewLine = true;
        }

        public StringBuilder LogLines
        {
            get => this._LogLines;
            set => value = this._LogLines;
        }

        public void AddToLog(string pLine)
        {
            if (this._NewLine)
            {
                this._LogLines.AppendFormat("{0:d}, ", (object)TimeZoneUtils.Now().Date);
                this._NewLine = false;
            }
            this._LogLines.Append(pLine);
        }

        public void AddFormatStringToLog(string pFormatString, object pObj1)
        {
            this.AddToLog(string.Format(pFormatString, pObj1));
        }

        public void AddFormatStringToLog(string pFormatString, object pObj1, object pObj2)
        {
            this.AddToLog(string.Format(pFormatString, pObj1, pObj2));
        }

        public void AddFormatStringToLog(string pFormatString, object pObj1, object pObj2, object pObj3)
        {
            this.AddToLog(string.Format(pFormatString, pObj1, pObj2, pObj3));
        }

        public void AddFormatStringToLog(
          string pFormatString,
          object pObj1,
          object pObj2,
          object pObj3,
          object pObj4)
        {
            this.AddToLog(string.Format(pFormatString, pObj1, pObj2, pObj3, pObj4));
        }

        public void AddFormatStringToLog(
          string pFormatString,
          object pObj1,
          object pObj2,
          object pObj3,
          object pObj4,
          object pObj5)
        {
            this.AddToLog(string.Format(pFormatString, pObj1, pObj2, pObj3, pObj4, pObj5));
        }

        public void AddLineToLog(string pLine)
        {
            if (this._NewLine)
                this._LogLines.AppendFormat("{0:d}, ", (object)TimeZoneUtils.Now().Date);
            this._LogLines.Append(pLine);
            this._LogLines.AppendLine();
            this._NewLine = true;
        }

        public void AddLineFormatStringToLog(string pFormatString, object pObj1)
        {
            this.AddLineToLog(string.Format(pFormatString, pObj1));
        }

        public void AddLineFormatStringToLog(string pFormatString, object pObj1, object pObj2)
        {
            this.AddLineToLog(string.Format(pFormatString, pObj1, pObj2));
        }

        public void AddLineFormatStringToLog(
          string pFormatString,
          object pObj1,
          object pObj2,
          object pObj3)
        {
            this.AddLineToLog(string.Format(pFormatString, pObj1, pObj2, pObj3));
        }

        public void AddLineFormatStringToLog(
          string pFormatString,
          object pObj1,
          object pObj2,
          object pObj3,
          object pObj4)
        {
            this.AddLineToLog(string.Format(pFormatString, pObj1, pObj2, pObj3, pObj4));
        }

        public void AddLineFormatStringToLog(
          string pFormatString,
          object pObj1,
          object pObj2,
          object pObj3,
          object pObj4,
          object pObj5)
        {
            this.AddLineToLog(string.Format(pFormatString, pObj1, pObj2, pObj3, pObj4, pObj5));
        }

        public string WriteLinesToLogFile()
        {
            string logFile = string.Empty;
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(this._LogFileName, this._AppendFile))
                    streamWriter.Write((object)this._LogLines);
                this._LogLines.Clear();
                this._AppendFile = true;
            }
            catch (Exception ex)
            {
                logFile = ex.Message;
            }
            return logFile;
        }
    }
}