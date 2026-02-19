using UnityEngine;

public class DiscordLink : MonoBehaviour
{
    [SerializeField] private string inviteUrl = "https://discord.gg/X8fAFfc5Fs";

    // ReSharper disable once UnusedMember.Global
    // Assigned in Editor
    public void OpenDiscord()
    {
        Application.OpenURL(inviteUrl);
    }
}