using UnityEngine;

public class DiscordLink : MonoBehaviour
{
    [SerializeField]
    private string inviteUrl = "https://discord.gg/X8fAFfc5Fs";

    public void OpenDiscord()
    {
        Application.OpenURL(inviteUrl);
    }
}

