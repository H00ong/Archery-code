using UnityEngine;

/// <summary> 씬 진입용 키 설정. 입력 처리와 안내 텍스트가 같은 값을 쓰도록 한 곳에서만 정의한다. </summary>
public static class GameKeys
{
    public const KeyCode LoadingToLobby = KeyCode.Space;
    public const KeyCode LobbyToInGame = KeyCode.Space;

    public static string GetDisplayName(KeyCode key)
    {
        return key switch
        {
            KeyCode.Space => "SPACE",
            KeyCode.Return => "ENTER",
            _ => key.ToString().ToUpperInvariant(),
        };
    }
}
