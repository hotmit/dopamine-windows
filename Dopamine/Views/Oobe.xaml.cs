﻿using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Services.Indexing;
using System.ComponentModel;
using System.Windows;

namespace Dopamine.Views
{
    public partial class Oobe : Windows10BorderlessWindow
    {
        private IIndexingService indexingService;

        public Oobe(IIndexingService indexingService)
        {
            InitializeComponent();

            this.indexingService = indexingService;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Prevent the OOBE window from appearing the next time the application is started
            SettingsClient.Set<bool>("General", "ShowOobe", false);

            // We're closing the OOBE window, tell the IndexingService to start checking the collection.
            this.indexingService.RefreshCollectionAsync(false, false);
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
