using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Ugoria.URBD.Shared.Configuration
{
    // разнести по разным сборкам
    [Serializable]
    public class Configuration : IConfiguration
    {
        private Hashtable settings;

        public object GetParameter(string key)
        {
            return settings[key];
        }

        public Configuration(Hashtable settings)
        {
            this.settings = settings;
        }
    }
}
