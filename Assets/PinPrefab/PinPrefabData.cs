using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinPrefab
{
  [CreateAssetMenu(menuName = "Project/Special/Pin Data", fileName = "PinPrefabData")]
  public class PinPrefabData : ScriptableObject
  {
    [NonSerialized] public const string DataPath = "Assets/Tools/PinPrefab/PinPrefabData.asset";

    [Header("Pinned Prefab List")]
    
    public List<string> PrefabList;

    public List<string> HistoryList;
  }
}