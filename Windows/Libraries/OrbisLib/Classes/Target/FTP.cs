using Limilabs.FTP.Client;
using OrbisSuite.Common;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace OrbisSuite
{
    public class FTP
    {
        private Target Target;

        public FTP(Target Target)
        {
            this.Target = Target;
        }

        public void DownloadFile(string LocalFilePath, string RemoteFilePath)
        {
            using (Ftp ftp = new Ftp())
            {
                ftp.Connect($"ftp://{Target.Info.IPAddress}:{Config.FTPPort}");
                ftp.Login("anonymous", "anonymous");

                ftp.Download(RemoteFilePath, LocalFilePath);

                ftp.Close();
            }
        }

        public void SendFile(string LocalFilePath, string RemoteFilePath)
        {
            using (Ftp ftp = new Ftp())
            {
                ftp.Connect($"ftp://{Target.Info.IPAddress}:{Config.FTPPort}");
                ftp.Login("anonymous", "anonymous");

                ftp.Upload(RemoteFilePath, LocalFilePath);

                ftp.Close();
            }
        }

        public List<FtpItem> GetDir(string Dir)
        {
            var ftpItems = new List<FtpItem>();
            using (Ftp ftp = new Ftp())
            {
                ftp.Connect($"ftp://{Target.Info.IPAddress}:{Config.FTPPort}");
                ftp.Login("anonymous", "anonymous");

                ftp.ChangeFolder(Dir);
                ftpItems = ftp.GetList();

                ftp.Close();
            }

            return ftpItems;
        }
    }
}
