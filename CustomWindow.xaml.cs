﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using WindowsDisplayAPI.DisplayConfig;
using System.Configuration;
using Newtonsoft.Json;

namespace Scalizer
{
    /// <summary>
    /// This is a business logic that retrieves the display information,
    /// parses it, and stores them in the JSON objects.
    /// 
    /// SCALIZER: ONE AND ONLY CUSTOM SCALING SOFTWARE FOR WINDOWS 10 AND 11
    /// An Open-source Project by John Seong. Served under the Apache License.
    /// </summary>
    
    internal class DisplayConfig
    {
        public int? displayIndex { get; set; }
        public String? displayName { get; set; }
        public String? dpiSetting { get; set; }
    }

    static class Extensions
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    public partial class CustomWindow : Window
    {
        private List<string>? displayInfoList;

        private List<string> filePaths = new List<string>();

        public CustomWindow()
        {
            InitializeComponent();

            Retrieve_Display_Info();

            monitorName.ItemsSource = displayInfoList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button? b = sender as Button;

            if (b!.Name == "backButton") { Delete_Display_Config(); Change_Window(sender, e); }
            if (b!.Name == "saveButton") { Save_Display_Config(); Change_Window(sender, e); }
        }
        private void Change_Window(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            Visibility = Visibility.Hidden;

            mainWindow.Show();
        }

        public void Retrieve_Display_Info()
        {
            List<string> displays = new List<string>();

            foreach (PathInfo pi in PathInfo.GetActivePaths())
            {
                if (!pi.TargetsInfo[0].DisplayTarget.IsAvailable) continue;

                string currentValue = String.Format("{0}",
                        string.IsNullOrEmpty(pi.TargetsInfo[0].DisplayTarget.FriendlyName) ? "Generic PnP Monitor" : pi.TargetsInfo[0].DisplayTarget.FriendlyName);

                displays.Add(currentValue);
            }
            
            displayInfoList = displays.Clone();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Save_Display_Config();
        }

        private void Save_Display_Config()
        {
            DisplayConfig displayConfig = new DisplayConfig
            {
                displayIndex = displayInfoList?.IndexOf(monitorName.Text.Trim()) + 1,
                displayName = monitorName.Text.Trim(),
                dpiSetting = dpiValue.Text.Trim()
            };

            // A guard clause that makes sure that the profile name has been entered...
            if (profileName.Text.Trim() == "") return;

            string path = String.Format(@"{0}@{1}.json", profileName.Text.Trim().Replace(" ", "_"), monitorName.Text);

            filePaths.Add(path);

            File.WriteAllText(path, JsonConvert.SerializeObject(displayConfig));
        }

        private void Delete_Display_Config()
        {
            // A Null-checking Guard Clause...
            if (filePaths == null) return;

            foreach (string path in filePaths)
            {
                File.Delete(path);
            }
        }

        // Only allows numbers to be entered in the DPI textbox...
        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if ((e.Text) == null || !(e.Text).All(char.IsDigit))
            {
                e.Handled = true;
            }
        }
    }
}
