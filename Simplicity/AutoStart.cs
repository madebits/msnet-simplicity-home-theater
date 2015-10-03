using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace Simplicity
{
    class AutoStart
    {
        //const string KeyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
        //const string ValueName = Config.AppNameShort;

        const string ShellKeyName = @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon";
        const string ShellValueName = "Shell";
        const string ShellValueDefault = "explorer.exe";
                
        public enum ShellStartState { Off, On }

        /*
        public enum AutoStartState{Off, OnNormal, OnMax}
        public static AutoStartState CurrentState
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName))
                    {
                        if (key == null) return AutoStartState.Off;
                        string value = (string)key.GetValue(ValueName);
                        if (value == null) return AutoStartState.Off;
                        bool fullscreen = false;
                        if (value.EndsWith(Program.fullScreenPrefix))
                        {
                            fullscreen = true;
                            value = value.Substring(0, value.Length - Program.fullScreenPrefix.Length).Trim();
                        }
                        if (!value.Equals(Config.Default.ExePath, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return AutoStartState.Off;
                        }
                        return fullscreen ? AutoStartState.OnMax : AutoStartState.OnNormal;
                    }
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                return AutoStartState.Off;
            }
            set
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyName))
                    {
                        if (key == null) return;
                        string v = Config.Default.ExePath;
                        switch (value)
                        {
                            case AutoStartState.Off:
                                key.DeleteValue(ValueName, false);
                                break;
                            case AutoStartState.OnNormal:
                                key.SetValue(ValueName, v);
                                break;
                            case AutoStartState.OnMax:
                                key.SetValue(ValueName, v + " " + Program.fullScreenPrefix);
                                break;
                        }
                    }
                }
                catch (Exception ex) { Config.Default.Error(ex); }
            }
        }
        */

        public static bool IsAutoStart 
        {
            get { return (ShellCurrentState == ShellStartState.On); }
        }

        public static ShellStartState ShellCurrentState 
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ShellKeyName))
                    {
                        if (key == null) return ShellStartState.Off;
                        string value = (string)key.GetValue(ShellValueName);
                        if (value == null) return ShellStartState.Off;
                        if (value.Equals(Config.Default.ExePath, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return ShellStartState.On;
                        }
                    }
                }
                catch (Exception ex) { Config.Default.Error(ex); }
                return ShellStartState.Off;
            }
            set
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ShellKeyName))
                    {
                        if (key == null) return;
                        string v = Config.Default.ExePath;
                        switch (value)
                        {
                            case ShellStartState.Off:
                                key.DeleteValue(ShellValueName, false);
                                break;
                            case ShellStartState.On:
                                key.SetValue(ShellValueName, v);
                                break;
                        }
                    }
                }
                catch (Exception ex) { Config.Default.Error(ex); }
            }
        }
    }//EOC
}
