using Microsoft.Web.Deployment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AzureSiteReplicator.Data
{
    public class LogFile : IDisposable
    {
        private const int MaxBufferSize = 1024;
        private string _filePath;
        private string _siteName;
        private StringBuilder _buffer;
        private StreamWriter _writer;
        private Object _lockObj = new Object();

        public LogFile(string siteName, bool readOnly)
        {
            _siteName = siteName;
            _filePath = Path.Combine(
                Environment.Instance.SiteReplicatorPath,
                siteName);

            _filePath = Path.Combine(_filePath, "deploy.log");

            if (!readOnly)
            {
                _buffer = new StringBuilder(MaxBufferSize);
                _writer = new StreamWriter(File.Open(
                    _filePath,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.Read));
            }
        }

        public void Log(TraceLevel traceLevel, string mesg, params object[] args)
        {
            string line = 
                string.Format(
                    "{0}\t{1}\t{2}",
                    DateTime.UtcNow.ToString("yyyy/MM/dd-HH:mm:ss:ff"),
                    traceLevel,
                    mesg,
                    args);

            lock (_lockObj)
            {
                if (_buffer.Length + line.Length > MaxBufferSize)
                {
                    Save();
                }

                _buffer.AppendLine(line);
            }
        }

        public void LogEventHandler(object sender, DeploymentTraceEventArgs traceEvent)
        {
            Log(traceEvent.EventLevel, traceEvent.Message);
        }

        public void Save()
        {
            lock (_lockObj)
            {
                if (_buffer.Length > 0)
                {
                    _writer.Write(_buffer);
                    _writer.Flush();
                    _buffer.Length = 0;
                }
            }
        }

        public string FilePath
        {
            get{return _filePath;}
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                Save();
                _writer.Close();
                _writer = null;
            }
        }
    }
}