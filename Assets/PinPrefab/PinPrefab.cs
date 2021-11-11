#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Scripts.Tools.PinPrefab
{
  public enum PinTab
  {
    PinList,
    History
  }

  public static class Extensions
  {
    public static List<string> GetClone(this List<string> source)
    {
      return source.Select(item => (string) item.Clone())
        .ToList();
    }
  }

  public class PinPrefab : EditorWindow
  {
    private const string OpenIcon = "UnityEditor.SceneHierarchyWindow";
    private const string InfoIcon = "UnityEditor.InspectorWindow";
    private const string RestoreIcon = "d_winbtn_mac_max_h";
    private const string DeleteIcon = "d_winbtn_mac_close_h";

    private static PinPrefabData _data;

    private static readonly List<string> newPrefabList = new List<string>();

    /*
 * TODO: Yeni eklenen itemlera new badge'i eklenecek
 */
    private static bool _onFocus;

    private readonly string[] _tabList = {"Pinned List", "History"};

    private PinTab _currentTab;

    private Vector2 _scrollPosition = Vector2.zero;

    private static List<string> PrefabList => Data.PrefabList;
    private static List<string> HistoryList => Data.HistoryList;

    private static PinPrefabData Data
    {
      get
      {
        if (!_data) _data = AssetDatabase.LoadAssetAtPath<PinPrefabData>(PinPrefabData.DataPath);
        return _data;
      }
    }

    private void OnEnable()
    {
      titleContent.text = "Pinned Prefab List";
    }

    private void OnDisable()
    {
      Save();
    }

    private void OnDestroy()
    {
      Save();
    }

    private void OnGUI()
    {
      _currentTab = (PinTab) GUILayout.Toolbar((int) _currentTab, _tabList);

      _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

      switch (_currentTab)
      {
        case PinTab.PinList:
          OpenPins();
          break;
        case PinTab.History:
          OpenHistory();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      GUILayout.EndScrollView();


      if (GUILayout.Button("Clear List")) ClearList();
    }

    private void OnFocus()
    {
      _onFocus = true;
    }

    private void OnLostFocus()
    {
      _onFocus = false;
    }

    private void ClearList()
    {
      switch (_currentTab)
      {
        case PinTab.PinList:
          ClearPins();
          break;
        case PinTab.History:
          ClearHistory();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void OpenPins()
    {
      // ReSharper disable once ForCanBeConvertedToForeach
      if(PrefabList.Count <= 0) return;
      
      for (var index = 0; index < PrefabList.Count; index++)
      {
        var path = PrefabList[index];

        GUILayout.BeginHorizontal();

        GUILayout.Label(PrefabName(path), GUILayout.Width(position.width / 100 * 60));
        if (GUILayout.Button(EditorGUIUtility.IconContent(OpenIcon), GUILayout.Width(position.width / 100 * 10)))
          OpenPrefab(path);

        if (GUILayout.Button(EditorGUIUtility.IconContent(InfoIcon), GUILayout.Width(position.width / 100 * 10)))
          OpenInInspector(path);

        if (GUILayout.Button(EditorGUIUtility.IconContent(DeleteIcon), GUILayout.Width(position.width / 100 * 10)))
          RemovePin(path);

        GUILayout.EndHorizontal();
      }
    }

    private void OpenHistory()
    {
      // ReSharper disable once ForCanBeConvertedToForeach
      for (var index = 0; index < HistoryList.Count; index++)
      {
        var path = HistoryList[index];
        GUILayout.BeginHorizontal();
        GUILayout.Label(PrefabName(path), GUILayout.Width(position.width / 100 * 70));
        if (GUILayout.Button(EditorGUIUtility.IconContent(RestoreIcon), GUILayout.Width(position.width / 100 * 10)))
          Restore(path);

        if (GUILayout.Button(EditorGUIUtility.IconContent(DeleteIcon), GUILayout.Width(position.width / 100 * 10)))
          RemoveHistory(path);

        GUILayout.EndHorizontal();
      }
    }

    private static string PrefabName(string path)
    {
      return Regex.Replace(path.Split('/').Last().Split('.').First(), "(\\B[A-Z])", " $1");
    }

    private static void OpenInInspector(string path)
    {
      Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
    }

    private static void OpenPrefab(string path)
    {
      Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
      AssetDatabase.OpenAsset(Selection.activeObject);
    }

    private static void Save()
    {
      try
      {
        EditorUtility.SetDirty(Data);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw new NullReferenceException("Null reference exception. Data maybe null. Exception: " + e);
      }
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
    }

    private static void AddPin(string path)
    {
      if (PrefabList.Contains(path)) return;

      if (!_onFocus) newPrefabList.Add(path);

      PrefabList.Add(path);

      Save();
    }

    private static void RemovePin(string path)
    {
      if (!PrefabList.Contains(path)) return;
      AddHistory(path);
      PrefabList.Remove(path);
    }

    private static void ClearPins()
    {
      var list = PrefabList.GetClone();
      for (var index = 0; index < list.Count; index++)
      {
        var path = list[index];
        RemovePin(path);
      }
    }

    private static void AddHistory(string path)
    {
      if (HistoryList.Contains(path)) return;

      HistoryList.Add(path);
    }

    private static void RemoveHistory(string path)
    {
      if (!HistoryList.Contains(path)) return;
      HistoryList.Remove(path);
    }

    private static void Restore(string path)
    {
      if (!HistoryList.Contains(path)) return;
      RemoveHistory(path);
      AddPin(path);
    }

    private static void ClearHistory()
    {
      var list = HistoryList.GetClone();
      for (var index = 0; index < list.Count; index++)
      {
        var path = list[index];
        RemoveHistory(path);
      }
    }

    [MenuItem("Assets/Pin Prefab %g")]
    private static void Pin()
    {
      try
      {
        var selectedGameObject = Selection.activeGameObject;
        var path = AssetDatabase.GetAssetPath(selectedGameObject);
        AddPin(path);
      }
      catch (NullReferenceException e)
      {
        Debug.LogError("You can pin only prefab. Exception: " + e);
      }
    }


    [MenuItem("BrosWindow/Pinned Prefab List")]
    private static void Init()
    {
      var window = (PinPrefab) GetWindow(typeof(PinPrefab));
      window.Show();
    }
  }
}
#endif