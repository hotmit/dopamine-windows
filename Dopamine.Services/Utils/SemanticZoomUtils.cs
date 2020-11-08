﻿using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Base;
using Dopamine.Core.Utils;
using Dopamine.Services.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dopamine.Services.Utils
{
    public sealed class SemanticZoomUtils
    {
        public static async Task<ObservableCollection<ISemanticZoomSelector>> UpdateSemanticZoomSelectors(ICollectionView semanticZoomables)
        {

            //=== ALEX (Support all characters)
            return new ObservableCollection<ISemanticZoomSelector>(semanticZoomables.OfType<ISemanticZoomable>().Select(x => x.Header).Distinct().Select(x => new SemanticZoomSelectorViewModel() { Header = x, CanZoom = true }).ToList());
            //=== END ALEX

            // Get all the possible semantic zoom selectors
            var zoomSelectors = new ObservableCollection<ISemanticZoomSelector>();

            await Task.Run(() =>
            {
                foreach (string item in Defaults.SemanticZoomItems)
                {
                    zoomSelectors.Add(new SemanticZoomSelectorViewModel
                    {
                        Header = item,
                        CanZoom = false
                    });
                }
            });

            // Set the availability of the semantic zoom selectors
            await Task.Run(() =>
            {
                try
                {

                    foreach (ISemanticZoomable zoomable in semanticZoomables)
                    {
                        dynamic selector = zoomSelectors.Select((s) => s).Where((s) => s.Header.ToLower() == zoomable.Header.ToLower()).FirstOrDefault();
                        if (selector != null) selector.CanZoom = true;
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while setting the availability of the semantic zoom selectors.", ex.Message);
                }
            });

            return zoomSelectors;
        }

        public static async Task SemanticScrollAsync(ListBox box, string header)
        {
            if (box == null) return;

            // Find the Object in the ListBox Items
            Object itemobject = null;

            await Task.Run(() => {
                try
                {
                    for (int i = 0; i <= box.Items.Count - 1; i++)
                    {
                        if (((ISemanticZoomable)box.Items[i]).Header.ToLower().Equals(header.ToLower()))
                        {
                            itemobject = box.Items[i];
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });

            if (itemobject == null) return;

            try
            {
                ScrollUtils.ScrollToListBoxItem(box, itemobject, false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetGroupHeader(string originalString, bool removePrefix = false)
        {
            if (string.IsNullOrEmpty(originalString))
                return string.Empty;
            //=== ALEX: Make it group also international characters
            string firstLetter = FormatUtils.GetSortableString(originalString, removePrefix).Substring(0, 1);
            if ("`1234567890-=[];'\\,./~!@#$%^&*()_+{}:\"|<>? ".Contains(firstLetter))
                return "#";

            return firstLetter;
            /*
            foreach (string semanticZoomItem in Defaults.SemanticZoomItems)
            {
                // CompareOptions.IgnoreNonSpace compares by ignoring accents on letters (important for special characters)
                if (string.Compare(firstLetter, semanticZoomItem, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0)
                {
                    return semanticZoomItem.ToLower();
                }
            }

            return "#";
            */
        }
    }
}
