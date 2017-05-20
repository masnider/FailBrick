using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;

namespace FailBrick
{
    internal static class Program
    {

        private static CrashMode runningCrashMode;
        private static ConfigHandler configHandler;

        private static void Main()
        {
            try
            {
                configHandler = new ConfigHandler(FabricRuntime.GetActivationContext(), "FailureModes");

                var crashmode = configHandler["Mode"];

                Enum.TryParse<CrashMode>(crashmode, out runningCrashMode);

                if (runningCrashMode == CrashMode.CrashBeforeRegistration)
                {
                    throw new Exception("crashing before service type registration");
                }

                if (runningCrashMode == CrashMode.RegisterTypeSlow)
                {
                    ServiceEventSource.Current.Message("Registering the right service type, but slowly");
                    Thread.Sleep(TimeSpan.FromMinutes(10));
                }

                if (runningCrashMode == CrashMode.RegisterWrongType)
                {
                    ServiceRuntime.RegisterServiceAsync("SomeWrongServiceType", createUnreliableServiceType);

                    Thread.Sleep(Timeout.Infinite);
                }

                ServiceRuntime.RegisterServiceAsync("FailBrickserviceType", createUnreliableServiceType);

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(FailBrick).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static FailBrick createUnreliableServiceType(StatefulServiceContext context)
        {
            if (runningCrashMode == CrashMode.CrashInRegistration)
            {
                throw new Exception("crashing inside service registration");
            }

            return new FailBrick(context, configHandler);
        }
    }
}
