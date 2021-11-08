public static class SceneLoadData
{
    public enum JoinMode
    {
        Host,
        Client
    }

    public static JoinMode chosenJoinMode;
    public static string ReasonForSceneLoad { get; set; }
    public static string Username { get; set; }
    public static string IPAddress { get; set; }
    public static int Port { get; set; }
}