using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSiteReplicator.Data
{
    internal class LockFile : IDisposable
    {
        private Stream _stream;
        private string _path;

        public static bool TryGetLockFile(string path, out LockFile file)
        {
            bool success = false;
            file = null;

            try
            {
                Stream stream = 
                    FileHelper.FileSystem.File.Open(
                        path,
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.Read);

                success = true;
                file = new LockFile(path, stream);
            }
            catch (IOException)
            {
            }

            return success;
        }

        private LockFile(string path, Stream stream)
        {
            _path = path;
            _stream = stream;
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
            }

            try
            {
                FileHelper.FileSystem.File.Delete(_path);
            }
            catch
            {
            }
        }
    }
}
