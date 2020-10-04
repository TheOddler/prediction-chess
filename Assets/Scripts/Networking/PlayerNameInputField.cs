using Photon.Pun;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    const string PLAYER_NAME_PREF_KEY = "PlayerName";

    void Start()
    {
        string name = PlayerPrefs.GetString(PLAYER_NAME_PREF_KEY, string.Empty);
        GetComponent<TMP_InputField>().text = name;
        PhotonNetwork.NickName = name;
    }

    public void SetPlayerName(string name)
    {
        PhotonNetwork.NickName = name;
        PlayerPrefs.SetString(PLAYER_NAME_PREF_KEY, name);
    }
}
