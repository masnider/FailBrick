# FailBrick
FailBrick is an unreliable Service Fabric service that can be configured to fail in common ways

To get it started, grab the solution and tell Visual Studio to package it. Then run the following commands (you'll need to modify them to point to the actual application package, and note that they expect a local cluster today).

``` posh

Test-ServiceFabricApplicationPackage "{packagePath}"

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "{packagePath}" -ImageStoreConnectionString "file:C:\SfDevCluster\Data\ImageStoreShare"

Connect-ServiceFabricCluster

Register-ServiceFabricApplicationType -ApplicationPathInImageStore "Debug"

New-ServiceFabricApplication -ApplicationName fabric:/FailBrickApplication -ApplicationTypeName FailBrickApplication -ApplicationTypeVersion 1.0.0 -ApplicationParameter @{FailureMode="None"}

New-ServiceFabricService -ApplicationName fabric:/FailBrickApplication -MinReplicaSetSize 3 -PartitionSchemeSingleton -ServiceName fabric:/FailBrickApplication/FailBrick -ServiceTypeName FailBrickserviceType -Stateful -TargetReplicaSetSize 3 -HasPersistedState -PlacementConstraint "(NodeType == NodeType3 || NodeType == NodeType4 || NodeType == NodeType2)"

```

Today FailBrick supports the following failure modes, configured via Application parameter. 

``` csharp

    public enum CrashMode
    {
        None,
        CrashBeforeRegistration,
        CrashInRegistration,
        RegisterWrongType,
        RegisterTypeSlow,
        CrashInReplicaOpen,
        CrashInListenerCreation,
        CrashInReplicaRunAsync,
        CrashInReplicaDemote,
        CrashInReplicaClose,
        CrashInReplicaConstruction
    }
  ```

This parameter is upgradable and FailBrick will by default pick up the new setting and act on it immediately. So for exaple if you first deploy FailBrick configured to CrashBeforeRegistration, you can then do an Application Parameter upgrade to change it to None, and then upgrade the application (unmonitored auto), at which point the service will spring back to life!

## Purpose
FailBrick is designed as a simple service to:

 - Show common failure points and what they will behave like
 - As an aid for training/teaching about certain types of failures and how to get out of them
 - Help construct tests or guidelines that require specific sequences of failure events
