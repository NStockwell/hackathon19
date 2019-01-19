using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "Gem/List", order = 1)]
public class GemsInfoScriptableObject : ScriptableObject {
    public GemTypeSO[] Info;
}