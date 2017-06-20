namespace FailBrick
{
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
        CrashInReplicaConstruction,
        SlowCancellationShutdown,
        HangCancellationShutdown
    }
}
