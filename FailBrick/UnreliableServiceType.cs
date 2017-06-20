using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace FailBrick
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class FailBrick : StatefulService
    {
        private readonly ConfigHandler configHandler;
        private CrashMode crashMode;

        public FailBrick(StatefulServiceContext context, ConfigHandler handler)
            : base(context)
        {
            this.configHandler = handler;
            this.configHandler.Changed += this.ConfigHandler_Changed;

            if (this.crashMode == CrashMode.CrashInReplicaConstruction)
            {
                throw new Exception("crash in replica construction");
            }
        }

        private void ConfigHandler_Changed(object sender, EventArgs e)
        {
            Enum.TryParse<CrashMode>(this.configHandler["Mode"], out this.crashMode);
        }

        protected override Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
        {
            if (this.crashMode == CrashMode.CrashInReplicaOpen)
            {
                throw new Exception("crash in replica open");
            }

            return Task.FromResult<bool>(true);
        }
        
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            if (this.crashMode == CrashMode.CrashInListenerCreation)
            {
                throw new Exception("crash in listenerCreation");
            }

            return new ServiceReplicaListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            if (this.crashMode == CrashMode.CrashInReplicaRunAsync)
            {
                throw new Exception("crash in RunAsync");
            }

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
            
            try
            {

                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var tx = this.StateManager.CreateTransaction())
                    {
                        var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                        ServiceEventSource.Current.ServiceMessage(this, "Current Counter Value: {0}",
                            result.HasValue ? result.Value.ToString() : "Value does not exist.");

                        await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                        // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                        // discarded, and nothing is saved to the secondary replicas.
                        await tx.CommitAsync();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }

            }
            catch (TaskCanceledException)
            {
                if (this.crashMode == CrashMode.SlowCancellationShutdown)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
                else if (this.crashMode == CrashMode.HangCancellationShutdown)
                {
                    await Task.Delay(TimeSpan.MaxValue);
                }
            }

            if (this.crashMode == CrashMode.CrashInReplicaDemote)
            {
                throw new Exception("crash when being demoted");
            }
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            if (this.crashMode == CrashMode.CrashInReplicaClose)
            {
                throw new Exception("crash in replica open");
            }

            return Task.FromResult<bool>(true);
        }
    }
}
