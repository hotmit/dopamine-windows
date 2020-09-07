﻿using Dopamine.Core.Alex; //Digimezzo.Foundation.Core.Settings
using Dopamine.Core.Base;
using Dopamine.Services.Cache;
using Dopamine.Services.Playback;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Dopamine.Services.ExternalControl
{
    public class ExternalControlService : IExternalControlService
    {
        private ServiceHost svcHost;
        private ExternalControlServer svcExternalControlInstance;
        private readonly IPlaybackService playbackService;
     
        public ExternalControlService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            if(SettingsClient.Get<bool>("Playback", "EnableExternalControl"))
            {
                this.Start();
            }   
        }
       
        public void Start()
        {
            if (this.svcExternalControlInstance == null)
            {
                this.svcExternalControlInstance = new ExternalControlServer(this.playbackService);
            }
            this.svcExternalControlInstance.Open();

            svcHost = new ServiceHost(svcExternalControlInstance, new Uri($"net.pipe://localhost/{ProductInformation.ApplicationName}"));
            svcHost.AddServiceEndpoint(typeof(IExternalControlServer), new NetNamedPipeBinding()
            {
#if DEBUG
                SendTimeout = new TimeSpan(0, 0, 8),
#else
                SendTimeout = new TimeSpan(0, 0, 1),
#endif
            }, "/ExternalControlService");

            svcHost.AddServiceEndpoint(typeof(IFftDataServer), new NetNamedPipeBinding()
            {
#if DEBUG
                SendTimeout = new TimeSpan(0, 0, 8),
#else
                SendTimeout = new TimeSpan(0, 0, 1),
#endif
            }, "/ExternalControlService/FftDataServer");

            var smb = svcHost.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            svcHost.Description.Behaviors.Add(smb);
            svcHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), "/ExternalControlService/mex");

            svcHost.Open();
        }

        public void Stop()
        {
            this.svcHost.Close();
            this.svcExternalControlInstance.Close();
        }
    }
}