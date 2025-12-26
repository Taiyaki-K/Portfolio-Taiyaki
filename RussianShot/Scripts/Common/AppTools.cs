public static class AppTools
{
    public static void ExitGame()
    {
#if UNITY_EDITOR
        // エディタ上なら再生を停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド後はアプリ終了
        // Application.Quit();
#endif
    }
}