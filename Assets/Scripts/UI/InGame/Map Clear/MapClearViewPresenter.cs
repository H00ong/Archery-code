using UnityEngine;

public class MapClearViewPresenter
{
    public MapClearViewPresenter(UI_MapClearView view, UnityEngine.Events.UnityAction onLobby)
    {
        view.Init(onLobby);
    }

    public MapClearViewPresenter(UI_MapClearView view, UnityEngine.Events.UnityAction onLobby, int gold)
    {
        view.Init(onLobby);
        view.SetGold(gold);
    }
}
