using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class FindComponentInPrefabs : EditorWindow
{

    [MenuItem("Tools/Find Component In Prefabs")]
    static void GetWindow()
    {
        GetWindow<FindComponentInPrefabs>();
    }

    Object targetFolder;
    bool showFullName;
    bool searchFullName;
    string typeSearchName = string.Empty;
    System.Type targetComponentType;
    System.Type[] componentTypes = null;
    System.Type[] filteredComponentTypes = null;
    GameObject[] gameObjects;
    Vector2 scrollPosition;
    Vector2 scrollPosition2;
    bool showTypes = true;
    bool showGameObjects = true;

    private void OnGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            var folder = EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(Object), false);
            if (check.changed)
            {
                if (folder == null)
                {
                    targetFolder = null;
                    return;
                }
                else
                {
                    var path = AssetDatabase.GetAssetPath(folder);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        targetFolder = folder;
                    }
                }
            }
        }

        showFullName = EditorGUILayout.Toggle("Show Full Name", showFullName);
        EditorGUI.BeginChangeCheck();
        searchFullName = EditorGUILayout.Toggle("Search Full Name", searchFullName);
        var searchFullNameChanged = EditorGUI.EndChangeCheck();

        if (componentTypes == null)
        {
            ListComponentTypes();
        }

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            typeSearchName = EditorGUILayout.TextField("Type Search Name", typeSearchName);
            if (check.changed || searchFullNameChanged || filteredComponentTypes == null)
            {
                if (searchFullName)
                {
                    filteredComponentTypes = componentTypes.Where(t => t.FullName.Contains(typeSearchName, System.StringComparison.OrdinalIgnoreCase)).ToArray();
                }
                else
                {
                    filteredComponentTypes = componentTypes.Where(t => t.Name.Contains(typeSearchName, System.StringComparison.OrdinalIgnoreCase)).ToArray();
                }
            }
        }

        EditorGUILayout.LabelField("Search: " + (targetComponentType == null ? "(NONE)" : showFullName ? targetComponentType.FullName : targetComponentType.Name), EditorStyles.boldLabel);

        showTypes = EditorGUILayout.Foldout(showTypes, "Component Types");
        if (showTypes)
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scroll.scrollPosition;
                foreach (var type in filteredComponentTypes)
                {
                    if (GUILayout.Button(showFullName ? type.FullName : type.Name))
                    {
                        targetComponentType = type;
                    }
                }
            }
        }

        using (new EditorGUI.DisabledScope(targetComponentType == null || targetFolder == null))
        {
            if (GUILayout.Button("Search"))
            {
                gameObjects = GetTargetPrefabs(AssetDatabase.GetAssetPath(targetFolder), targetComponentType).ToArray();
            }
        }

        showGameObjects = EditorGUILayout.Foldout(showGameObjects, "Found GameObjects");
        if (showGameObjects)
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition2))
            {
                scrollPosition2 = scroll.scrollPosition;
                if (gameObjects != null)
                {
                    foreach (var go in gameObjects)
                    {
                        EditorGUILayout.ObjectField(go, typeof(GameObject), false);
                    }
                }
            }
        }
    }

    public IEnumerable<GameObject> GetTargetPrefabs(string folder, System.Type componentType)
    {
        foreach (var e in Directory.EnumerateFiles(folder, "*.prefab"))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(e);
            var c = prefab.GetComponentInChildren(componentType);
            if (c != null)
            {
                yield return prefab;
            }
        }
    }

    void ListComponentTypes()
    {
        componentTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Component)))
            .OrderBy(type => type.Name)
            .ToArray();
    }
}
