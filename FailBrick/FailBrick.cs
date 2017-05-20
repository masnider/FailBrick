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
        private readonly ConfigHandler ConfigHandler;
        private CrashMode FailureMode;

        public FailBrick(StatefulServiceContext context, ConfigHandler handler)
            : base(context)
        {
            this.ConfigHandler = handler;
            this.ConfigHandler.Changed += ConfigHandler_Changed;

            if (this.FailureMode == CrashMode.CrashInReplicaConstruction)
            {
                throw new Exception("crash in replica construction");
            }
        }

        private void ConfigHandler_Changed(object sender, EventArgs e)
        {
            Enum.TryParse<CrashMode>(this.ConfigHandler["Mode"], out this.FailureMode);
        }

        protected override Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
        {
            if (this.FailureMode == CrashMode.CrashInReplicaOpen)
            {
                throw new Exception("crash in replica open");
            }

            return Task.FromResult<bool>(true);
        }


        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            if (this.FailureMode == CrashMode.CrashInListenerCreation)
            {
                throw new Exception("crash in listenerCreation");
            }

            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            if (this.FailureMode == CrashMode.CrashInReplicaRunAsync)
            {
                throw new Exception("crash in RunAsync");
            }

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

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

            if (this.FailureMode == CrashMode.CrashInReplicaDemote)
            {
                throw new Exception("crash when being demoted");
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            if (this.FailureMode == CrashMode.CrashInReplicaClose)
            {
                throw new Exception("crash in replica open");
            }

            return Task.FromResult<bool>(true);
        }
    }
}
