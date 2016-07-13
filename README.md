# FailBrick
FailBrick is an unreliable Service Fabric service that can be configured to fail in common ways

To get it started, grab the solution and tell Visual Studio to package it. Then run the following commands (you'll need to modify them to point to the actual application package, and note that they expect a local cluster today).

Test-ServiceFabricApplicationPackage {packagePath}"
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath {packagePath}" -ImageStoreConnectionString "file:C:\SfDevCluster\Data\ImageStoreShare"
Connect-ServiceFabricCluster
Register-ServiceFabricApplicationType -ApplicationPathInImageStore "Debug"
New-ServiceFabricApplication -ApplicationName fabric:/FailBrickApplication -ApplicationTypeName FailBrickApplication -ApplicationTypeVersion 1.0.0 -ApplicationParameter @{FailureMode="None"}
New-ServiceFabricService -ApplicationName fabric:/FailBrickApplication -MinReplicaSetSize 3 -PartitionSchemeSingleton -ServiceName fabric:/FailBrickApplication/FailBrick -ServiceTypeName FailBrickserviceType -Stateful -TargetReplicaSetSize 3 -HasPersistedState -PlacementConstraint "(NodeType == NodeType3 || NodeType == NodeType4 || NodeType == NodeType2)"
