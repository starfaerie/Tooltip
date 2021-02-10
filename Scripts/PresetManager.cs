using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tooltip
{
    public static class PresetManager
    {
        public const string PROP_KEY = "TOOLTIP_TRIGGER_SAVE";

        private static Dictionary<string, PresetList> _presets = new Dictionary<string, PresetList>();

        public static PresetList Load(int saveIndex)
        {
            string listId = PROP_KEY + saveIndex;
            if (_presets.TryGetValue(listId, out PresetList preset)) return preset;
            preset = new PresetList(saveIndex);

            return preset;
        }

        public static PresetList Save(int saveIndex)
        {
            string listId = PROP_KEY + saveIndex;
            PresetList preset = new PresetList(saveIndex);
            preset.SavePreset(saveIndex);
            _presets.Add(listId,preset);

            return preset;
        }
    }

    public class PresetList
    {
        public string KEY { get; private set; }
        
        public string Header { get; private set; }
        public string Content { get; private set; }
        public int HeaderSize { get; private set; }
        public int ContentSize { get; private set; }
        public int BorderWidth { get; private set; }
        public int BorderRadius { get; private set; }
        
        public Color PanelColor { get; private set; }
        public Color HeaderColor { get; private set; }
        public Color ContentColor { get; private set; }
        public Color BorderColor { get; private set; }
        
        public int ContentFont { get; private set; }
        public int HeaderFont { get; private set; }

        public PresetList(int listId)
        {
            KEY = PresetManager.PROP_KEY + listId;
            LoadSavedSerializedProperties();
        }

        public void SavePreset(int listId)
        {
            KEY = PresetManager.PROP_KEY + listId;
            SaveSerializedProperties();
        }
        
        private void SaveSerializedProperties()
        {
            //Strings
            EditorPrefs.SetString(KEY + "Header", Header);
            EditorPrefs.SetString(KEY + "Content", Content);
            
            //Ints
            EditorPrefs.SetInt(KEY + "HeaderSize", HeaderSize);
            EditorPrefs.SetInt(KEY + "ContentSize", ContentSize);
            EditorPrefs.SetInt(KEY + "BorderWidth", BorderWidth);
            EditorPrefs.SetInt(KEY + "BorderRadius", BorderRadius);
            
            //Enums
            EditorPrefs.SetInt(KEY + "ContentFont", (int)ContentFont);
            EditorPrefs.SetInt(KEY + "HeaderFont", (int)HeaderFont);
            
            //Colors
            EditorPrefs.SetFloat(KEY + "PanelColor.r", PanelColor.r);
            EditorPrefs.SetFloat(KEY + "PanelColor.g", PanelColor.g);
            EditorPrefs.SetFloat(KEY + "PanelColor.b", PanelColor.b);
            EditorPrefs.SetFloat(KEY + "PanelColor.a", PanelColor.a);
            
            EditorPrefs.SetFloat(KEY + "BorderColor.r", BorderColor.r);
            EditorPrefs.SetFloat(KEY + "BorderColor.g", BorderColor.g);
            EditorPrefs.SetFloat(KEY + "BorderColor.b", BorderColor.b);
            EditorPrefs.SetFloat(KEY + "BorderColor.a", BorderColor.a);

            EditorPrefs.SetFloat(KEY + "ContentColor.r", ContentColor.r);
            EditorPrefs.SetFloat(KEY + "ContentColor.g", ContentColor.g);
            EditorPrefs.SetFloat(KEY + "ContentColor.b", ContentColor.b);
            EditorPrefs.SetFloat(KEY + "ContentColor.a", ContentColor.a);
            
            EditorPrefs.SetFloat(KEY + "HeaderColor.r", HeaderColor.r);
            EditorPrefs.SetFloat(KEY + "HeaderColor.g", HeaderColor.g);
            EditorPrefs.SetFloat(KEY + "HeaderColor.b", HeaderColor.b);
            EditorPrefs.SetFloat(KEY + "HeaderColor.a", HeaderColor.a);
        }

        private void LoadSavedSerializedProperties()
        {
            //Strings
            Header = EditorPrefs.GetString(KEY + "Header", "Header");
            Content = EditorPrefs.GetString(KEY + "Content", "Content");
            
            //Ints
            HeaderSize = EditorPrefs.GetInt(KEY + "HeaderSize", 32);
            ContentSize = EditorPrefs.GetInt(KEY + "ContentSize", 30);
            BorderWidth = EditorPrefs.GetInt(KEY + "BorderWidth", 3);
            BorderRadius = EditorPrefs.GetInt(KEY + "BorderRadius", 10);
            
            //Enums
            ContentFont = EditorPrefs.GetInt(KEY + "ContentFont", 0);
            HeaderFont = EditorPrefs.GetInt(KEY + "HeaderFont", 0);
            
            //Colors
            PanelColor = new Color(EditorPrefs.GetFloat(KEY + "PanelColor.r", 0.8f),
                EditorPrefs.GetFloat(KEY + "PanelColor.g", 0.8f),
                EditorPrefs.GetFloat(KEY + "PanelColor.b", 0.8f),
                EditorPrefs.GetFloat(KEY + "PanelColor.a", 1));
            BorderColor = new Color(EditorPrefs.GetFloat(KEY + "BorderColor.r", 1),
                EditorPrefs.GetFloat(KEY + "BorderColor.g", 1),
                EditorPrefs.GetFloat(KEY + "BorderColor.b", 1),
                EditorPrefs.GetFloat(KEY + "BorderColor.a", 1));
            ContentColor = new Color(EditorPrefs.GetFloat(KEY + "ContentColor.r", 0f),
                EditorPrefs.GetFloat(KEY + "ContentColor.g", 0f),
                EditorPrefs.GetFloat(KEY + "ContentColor.b", 0f),
                EditorPrefs.GetFloat(KEY + "ContentColor.a", 1));
            HeaderColor = new Color(EditorPrefs.GetFloat(KEY + "HeaderColor.r", 0f),
                EditorPrefs.GetFloat(KEY + "HeaderColor.g", 0f),
                EditorPrefs.GetFloat(KEY + "HeaderColor.b", 0f),
                EditorPrefs.GetFloat(KEY + "HeaderColor.a", 1));
        }
    }
}
