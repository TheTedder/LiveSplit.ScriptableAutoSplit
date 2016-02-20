﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LiveSplit.ASL
{
    // Created from the ASL script and shared with the GUI to synchronize setting state.
    public class ASLSetting
    {
        public string Id { get; }
        public string Label { get; }
        public bool Value { get; set; }
        public string Parent { get; }

        public ASLSetting(string id, bool default_value, string label, string parent)
        {
            Id = id;
            Value = default_value;
            Label = label;
            Parent = parent;
        }

        public override string ToString()
        {
            return Label;
        }
    }

    public class ASLSettings
    {
        // Dict for easy access per key
        public Dictionary<string, ASLSetting> Settings { get; set; }
        // List for preserved insertion order (Dict provides that as well, but not guaranteed)
        public List<ASLSetting> OrderedSettings { get; }

        public Dictionary<string, ASLSetting> BasicSettings { get; }

        public ASLSettingsBuilder Builder;
        public ASLSettingsReader Reader;
        
        public ASLSettings()
        {
            Settings = new Dictionary<string, ASLSetting>();
            OrderedSettings = new List<ASLSetting>();
            BasicSettings = new Dictionary<string, ASLSetting>();
            Builder = new ASLSettingsBuilder(this);
            Reader = new ASLSettingsReader(this);
        }

        public void AddSetting(string name, bool default_value, string description, string parent)
        {
            if (description == null)
                description = name;
            if (parent != null && !Settings.ContainsKey(parent))
                throw new ArgumentException($"Parent for setting '{name}' is not a setting: {parent}");

            var setting = new ASLSetting(name, default_value, description, parent);
            Settings.Add(name, setting);
            OrderedSettings.Add(setting);
        }

        public bool GetSettingValue(string name)
        {
            // Don't cause error if setting doesn't exist, but still inform script
            // author since that usually shouldn't happen.
            if (Settings.ContainsKey(name))
                return GetSettingValueRecursive(Settings[name]);

            Trace.WriteLine("[ASL] Custom Setting Key doesn't exist: " + name);

            return false;
        }

        public void AddBasicSetting(string name)
        {
            BasicSettings.Add(name, new ASLSetting(name, true, "", null));
        }

        public bool GetBasicSettingValue(string name)
        {
            if (BasicSettings.ContainsKey(name))
                return BasicSettings[name].Value;

            return false;
        }

        public bool IsBasicSettingPresent(string name)
        {
            return BasicSettings.ContainsKey(name);
        }


        /// <summary>
        /// Returns true only if this setting and all it's parent settings are true.
        /// </summary>
        private bool GetSettingValueRecursive(ASLSetting setting)
        {
            if (!setting.Value)
                return false;

            if (setting.Parent == null)
                return setting.Value;

            return GetSettingValueRecursive(Settings[setting.Parent]);
        }
    }

    /// <summary>
    /// Interface for adding settings via the ASL Script.
    /// </summary>
    public class ASLSettingsBuilder
    {
        public string CurrentDefaultParent { get; set; }
        private ASLSettings _s;

        public ASLSettingsBuilder(ASLSettings s)
        {
            _s = s;
        }

        public void Add(string id, bool default_value = true, string description = null, string parent = null)
        {
            if (parent == null)
                parent = CurrentDefaultParent;

            _s.AddSetting(id, default_value, description, parent);
        }
    }

    /// <summary>
    /// Interface for reading settings via the ASL Script.
    /// </summary>
    public class ASLSettingsReader
    {
        private ASLSettings _s;

        public ASLSettingsReader(ASLSettings s)
        {
            _s = s;
        }

        public dynamic this[string id]
        {
            get { return _s.GetSettingValue(id); }
        }

        public bool StartEnabled
        {
            get { return _s.GetBasicSettingValue("start"); }
        }

        public bool ResetEnabled
        {
            get { return _s.GetBasicSettingValue("reset"); }
        }

        public bool SplitEnabled
        {
            get { return _s.GetBasicSettingValue("split"); }
        }
    }
}
