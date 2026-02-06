namespace MCPForUnity.Editor.Constants
{
    /// <summary>
    /// Centralized list of EditorPrefs keys used by the MCP for Unity package.
    /// Keeping them in one place avoids typos and simplifies migrations.
    /// </summary>
    internal static class EditorPrefKeys
    {
        public const string UseHttpTransport = "MCPForUnity.UseHttpTransport";
        public const string HttpTransportScope = "MCPForUnity.HttpTransportScope"; // "local" | "remote"
        public const string LastLocalHttpServerPid = "MCPForUnity.LocalHttpServer.LastPid";
        public const string LastLocalHttpServerPort = "MCPForUnity.LocalHttpServer.LastPort";
        public const string LastLocalHttpServerStartedUtc = "MCPForUnity.LocalHttpServer.LastStartedUtc";
        public const string LastLocalHttpServerPidArgsHash = "MCPForUnity.LocalHttpServer.LastPidArgsHash";
        public const string LastLocalHttpServerPidFilePath = "MCPForUnity.LocalHttpServer.LastPidFilePath";
        public const string LastLocalHttpServerInstanceToken = "MCPForUnity.LocalHttpServer.LastInstanceToken";
        public const string DebugLogs = "MCPForUnity.DebugLogs";
        public const string ValidationLevel = "MCPForUnity.ValidationLevel";
        public const string UnitySocketPort = "MCPForUnity.UnitySocketPort";
        public const string ResumeHttpAfterReload = "MCPForUnity.ResumeHttpAfterReload";
        public const string ResumeStdioAfterReload = "MCPForUnity.ResumeStdioAfterReload";

        public const string UvxPathOverride = "MCPForUnity.UvxPath";
        public const string ClaudeCliPathOverride = "MCPForUnity.ClaudeCliPath";

        public const string HttpBaseUrl = "MCPForUnity.HttpUrl";
        public const string HttpRemoteBaseUrl = "MCPForUnity.HttpRemoteUrl";
        public const string SessionId = "MCPForUnity.SessionId";
        public const string WebSocketUrlOverride = "MCPForUnity.WebSocketUrl";
        public const string GitUrlOverride = "MCPForUnity.GitUrlOverride";
        public const string DevModeForceServerRefresh = "MCPForUnity.DevModeForceServerRefresh";
        public const string UseBetaServer = "MCPForUnity.UseBetaServer";
        public const string ProjectScopedToolsLocalHttp = "MCPForUnity.ProjectScopedTools.LocalHttp";

        public const string PackageDeploySourcePath = "MCPForUnity.PackageDeploy.SourcePath";
        public const string PackageDeployLastBackupPath = "MCPForUnity.PackageDeploy.LastBackupPath";
        public const string PackageDeployLastTargetPath = "MCPForUnity.PackageDeploy.LastTargetPath";
        public const string PackageDeployLastSourcePath = "MCPForUnity.PackageDeploy.LastSourcePath";

        public const string ServerSrc = "MCPForUnity.ServerSrc";
        public const string UseEmbeddedServer = "MCPForUnity.UseEmbeddedServer";
        public const string LockCursorConfig = "MCPForUnity.LockCursorConfig";
        public const string AutoRegisterEnabled = "MCPForUnity.AutoRegisterEnabled";
        public const string ToolEnabledPrefix = "MCPForUnity.ToolEnabled.";
        public const string ToolFoldoutStatePrefix = "MCPForUnity.ToolFoldout.";
        public const string ResourceEnabledPrefix = "MCPForUnity.ResourceEnabled.";
        public const string ResourceFoldoutStatePrefix = "MCPForUnity.ResourceFoldout.";
        public const string EditorWindowActivePanel = "MCPForUnity.EditorWindow.ActivePanel";
        public const string LastSelectedClientId = "MCPForUnity.LastSelectedClientId";

        public const string SetupCompleted = "MCPForUnity.SetupCompleted";
        public const string SetupDismissed = "MCPForUnity.SetupDismissed";

        public const string CustomToolRegistrationEnabled = "MCPForUnity.CustomToolRegistrationEnabled";

        public const string LastUpdateCheck = "MCPForUnity.LastUpdateCheck";
        public const string LatestKnownVersion = "MCPForUnity.LatestKnownVersion";
        public const string LastAssetStoreUpdateCheck = "MCPForUnity.LastAssetStoreUpdateCheck";
        public const string LatestKnownAssetStoreVersion = "MCPForUnity.LatestKnownAssetStoreVersion";
        public const string LastStdIoUpgradeVersion = "MCPForUnity.LastStdIoUpgradeVersion";

        public const string TelemetryDisabled = "MCPForUnity.TelemetryDisabled";
        public const string CustomerUuid = "MCPForUnity.CustomerUUID";

        public const string ApiKey = "MCPForUnity.ApiKey";
    }
}
