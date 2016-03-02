using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Infrastructures.Configurations;

namespace CMA.MICAPS.Providers
{
     public sealed class MetaData
    {
        private Dictionary<string, Configuration> _metas = new Dictionary<string, Configuration>();

        public string GetValue(string domain, string key)
        {
            domain = domain ?? "default";
            Configuration config = null;
            if (_metas.TryGetValue(domain, out config))
            {
                return config.GetString(key);
            }
            return null;
        }

        public void SetValue(string domain, string key, string value)
        {
            domain = domain ?? "default";
            Configuration config = null;
            if (_metas.TryGetValue(domain, out config))
            {
                config.SetString(key, value);
            }
            else
            {
                config = new Configuration();
                _metas[domain] = config;
                config.SetString(key, value);
            }
        }

        public string[] GetDomainValues(string name)
        {
            name = name ?? "default";
            Configuration domain = this.GetDomain(name);
            if (domain == null)
                return null;

            List<string> values = new List<string>();

            foreach (string key in domain.Keys)
            {
                string value = string.Format("{0}:{1}", key, domain.GetString(key));
                values.Add(value);
            }
            return values.ToArray();
        }

        internal void AddDomain(string domain, Configuration configs)
        {
            _metas[domain] = configs;
        }

        internal Configuration GetDomain(string domain)
        {
            domain = domain ?? "default";
            Configuration config = null;
            if (_metas.TryGetValue(domain, out config))
            {
                return config;
            }
            return null;
        }
    }
}
