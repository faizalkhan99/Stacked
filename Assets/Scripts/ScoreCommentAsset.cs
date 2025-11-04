using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScoreCommentAsset", menuName = "Stack Clone/Score Comment Asset")]
public class ScoreCommentAsset : ScriptableObject
{
    [Header("List of Comments")]
    [Tooltip("A random comment will be chosen from this list.")]
    [TextArea(2, 5)] // Makes the text fields in the Inspector larger
    public List<string> comments;
}
