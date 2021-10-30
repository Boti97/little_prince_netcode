public static class SceneLoadData
{
    public enum JoinMode
    {
        Host,
        Client
    }

    public static JoinMode chosenJoinMode;
    public static string ReasonForSceneLoad { get; set; }
}