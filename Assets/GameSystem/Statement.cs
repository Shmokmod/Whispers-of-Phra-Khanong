using UnityEngine;

[System.Serializable]
public class Statement
{
    public string id;
    public string speakerName;
    [TextArea(3, 6)]
    public string text;
    public Sprite portrait;
    public string locationFound;

    // State
    public bool isVerified;
    public bool isContradicted;

    public string GetFullDescription()
    {
        return $"<b>{speakerName}</b>\n\n{text}\n\n<i>เจอที่: {locationFound}</i>";
    }
}