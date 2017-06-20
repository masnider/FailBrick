using System;
using System.Collections.ObjectModel;
using System.Fabric;
using System.Fabric.Description;

namespace FailBrick
{
    class ConfigHandler
    {
        private readonly string SectionName;
        private KeyedCollection<string, ConfigurationProperty> Settings;
        public event EventHandler Changed;

        public ConfigHandler(CodePackageActivationContext activationcontext, string sectionName)
        {
            this.SectionName = sectionName;

            if (activationcontext.GetConfigurationPackageNames().Contains("Config"))
            {
                ConfigurationPackage configPackage = activationcontext.GetConfigurationPackageObject("Config");

                this.UpdateConfigSettings(configPackage.Settings);

                activationcontext.ConfigurationPackageModifiedEvent
                    += this.CodePackageActivationContext_ConfigurationPackageModifiedEvent;
            }
        }


        private void UpdateConfigSettings(ConfigurationSettings configSettings)
        {
            try
            {
                this.Settings = configSettings.Sections[this.SectionName].Parameters;
                this.OnChanged(EventArgs.Empty);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            this.UpdateConfigSettings(e.NewPackage.Settings);
        }

        public string this[string name]
        {
            get
            {
                return this.Settings[name].Value;
            }

        }

        protected virtual void OnChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }


    }
}
