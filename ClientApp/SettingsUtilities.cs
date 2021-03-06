﻿using Microsoft.Win32;

namespace SuperMarketPlanner
{
    public class SettingsUtilities
    {
        /// <summary>
        /// Saves a setting to the Registry
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SaveSetting(string name, object value)
        {
            var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\JscoSuperMarketPlanner");

            if (value != null)
            {
                key.SetValue(name, value);
            }
        }

        /// <summary>
        /// Loads a setting from the Registry
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object LoadSetting(string name)
        {
            // Get the value stored in the Registry  
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\JscoSuperMarketPlanner");

            if (key == null)
            {
                return null;
            }
            else
            {
                return key.GetValue(name);
            }
        }
    }
}
