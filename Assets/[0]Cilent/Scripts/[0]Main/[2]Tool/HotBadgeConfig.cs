using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HotBadgeConfig", menuName = "Game/HotBadgeConfig")]
public class HotBadgeConfig : ScriptableObject
{
    public List<string> badges;
}
