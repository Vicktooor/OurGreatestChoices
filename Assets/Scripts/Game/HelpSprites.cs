using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HelpSprites", menuName = "Pnj Help Sprites")]
public class HelpSprites : ScriptableObject
{
    public List<PNJHelp> sprites;
    public Color ecoColor;
    public Color gouvColor;
    public Color ngoColor;
}