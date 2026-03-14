using UnityEngine;

public class MapClearViewPresenter
{
    public MapClearViewPresenter(UI_MapClearView view, UnityEngine.Events.UnityAction onLobby)
    {
        view.Init(onLobby);
    }
}
