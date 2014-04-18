using AzureSiteReplicator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureSiteReplicator.Models
{
    public class PublishSettingsModel
    {
        private PublishSettings _settings;
        private string _publishUrl;
        private string _siteName;
        private string _userName;
        private string _password;

        public PublishSettingsModel(string filePath)
        {
            _settings = new PublishSettings(filePath);
        }

        public string PublishUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_publishUrl))
                {
                    _publishUrl = _settings.PublishUrlRaw;
                }

                return _publishUrl;
            }

            set
            {
                _publishUrl = value;
            }
        }

        public string SiteName
        {
            get
            {
                if (string.IsNullOrEmpty(_siteName))
                {
                    _siteName = _settings.SiteName;
                }

                return _siteName;
            }

            set
            {
                _siteName = value;
            }
        }

        public string Username
        {
            get
            {
                if (string.IsNullOrEmpty(_userName))
                {
                    _userName = _settings.Username;
                }

                return _userName;
            }

            set
            {
                _userName = value;
            }
        }

        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(_password))
                {
                    _password = _settings.Password;
                }
                return _password;
            }
            set
            {
                _password = value;
            }
        }
    }
}