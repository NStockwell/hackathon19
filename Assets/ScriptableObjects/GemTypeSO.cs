using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Data", menuName = "Gem/Create", order = 1)]
public class GemTypeSO : ScriptableObject {
    public GemType GemType = GemType.INVALID;
    public GemColor GemColor = GemColor.INVALID;    
    public Sprite Sprite;
}