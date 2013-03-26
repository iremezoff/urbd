using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Ugoria.URBD.WebControl.ViewModels;

namespace Ugoria.URBD.WebControl.Models
{
    public interface ISetting
    {
        string Key { get; }
        string Value { get; set; }
        string Description { get; }
        string Type { get; }
    }

    public class SettingViewModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public partial class Setting : ISetting
    {
        public string Key
        {
            get { return key; }
        }

        public string Value
        {
            get { return value; }
            set { @value = value; }
        }

        public string Description
        { get { return description; } }

        public string Type
        { get { return type; } }
    }

    public interface ISettingsRepository
    {
        IEnumerable<ISetting> GetSettings();
        IEnumerable<IExtDirectory> GetExtDirectories();
        void SaveSetting(SettingViewModel setting);
        void SaveExtDirectory(ExtDirectoryViewModel extDirectory);
        void UpdateServicesDataChange(DateTime dateTime);
    }

    public class SettingsRepository : ISettingsRepository
    {
        private URBD2Entities dataContext;
        private IEnumerable<Setting> cache;
        private IEnumerable<ExtDirectory> cache2;
        public SettingsRepository(URBD2Entities dataContext)
        {
            this.dataContext = dataContext;
        }

        public IEnumerable<IExtDirectory> GetExtDirectories()
        {
            return dataContext.ExtDirectory.Select(b => b).ToList();
        }

        public void SaveExtDirectory(ExtDirectoryViewModel extDirectoryVM)
        {
            if (cache2 == null)
                cache2 = GetExtDirectories().Select(u => (ExtDirectory)u);
            if (extDirectoryVM.DirId == 0)
            {
                dataContext.ExtDirectory.AddObject(new ExtDirectory
                {
                    local_path = extDirectoryVM.LocalPath,
                    ftp_path = extDirectoryVM.FtpPath
                });
                return;
            }
            ExtDirectory extDirectory = cache2.Where(u => u.DirId == extDirectoryVM.DirId).SingleOrDefault();
            if (extDirectory == null)
                return;
            else if (string.IsNullOrEmpty(extDirectoryVM.LocalPath) && string.IsNullOrEmpty(extDirectoryVM.FtpPath))
                dataContext.ExtDirectory.DeleteObject(extDirectory);
            else
            {
                extDirectory.local_path = extDirectoryVM.LocalPath;
                extDirectory.ftp_path = extDirectoryVM.FtpPath;
            }
        }

        public void UpdateServicesDataChange(DateTime dateTime)
        {
            dataContext.Service.Select(s => s).ToList().ForEach(s=>s.date_change = dateTime);
        }

        public IEnumerable<ISetting> GetSettings()
        {
            cache = dataContext.Setting.OrderBy(s => s.order).ThenBy(s => s.key).Select(s => s).ToList();
            return cache;
        }

        public void SaveSetting(SettingViewModel settingVM)
        {
            if (cache == null)
                cache = GetSettings().Select(s => (Setting)s);
            ISetting setting = cache.Where(s => s.Key == settingVM.Key).SingleOrDefault();
            if (setting == null)
                return;
            if (setting.Value.Equals(settingVM.Value) || (string.IsNullOrEmpty(settingVM.Value) && string.IsNullOrEmpty(setting.Value)))
                return;
            Func<string, bool> strategy = null;
            switch (setting.Type)
            {
                case "integer":
                    strategy = (@in) => { int val = 0; return int.TryParse(@in, out val); };
                    break;
                case "boolean":
                    strategy = (@in) => { bool val = false; return bool.TryParse(@in, out val); };
                    break;
                default:
                    strategy = (@in) => true;
                    break;
            }
            if (strategy(settingVM.Value))
                setting.Value = settingVM.Value;
        }
    }
}