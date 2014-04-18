using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Web;

namespace AzureSiteReplicator
{
    public class FileHelper
    {
        private static IFileSystem _fileSystem = new FileSystem();
        public static IFileSystem FileSystem
        {
            get
            {
                return _fileSystem;
            }
            set
            {
                _fileSystem = value;
            }
        }

        public static string GetProfileNameFromFileName(string path)
        {
            return Path.GetFileName(path).Split('.').First();
        }
    }
}