﻿using Dopamine.Data.Entities;
using Dopamine.Services.Entities;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Extensions
{
    public static class ContainerProviderExtensions
    {
        public static TrackViewModel ResolveTrackViewModel(this IContainerProvider container, TrackV track)
        {
            var getTrackViewModel = container.Resolve<Func<TrackV, TrackViewModel>>();
            return getTrackViewModel(track);
        }

        public async static Task<IList<TrackViewModel>> ResolveTrackViewModelsAsync(this IContainerProvider container, IList<TrackV> tracks)
        {
            IList<TrackViewModel> trackViewModels = null;
            await Task.Run(() => { trackViewModels = tracks.Select(t => container.ResolveTrackViewModel(t)).ToList(); });

            return trackViewModels;
        }
    }
}
