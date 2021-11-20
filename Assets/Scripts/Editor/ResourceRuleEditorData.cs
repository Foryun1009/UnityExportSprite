using System.Collections.Generic;
using UnityEngine;

namespace Tools.NGUIAtlasToSprite
{
    public class ResourceRuleEditorData : ScriptableObject
    {
        public List<ResourceRule> rules = new List<ResourceRule>();
    }

    [System.Serializable]
    public class ResourceRule
    {
        public string atlasName = string.Empty;
        public UIAtlas atlas = null;
        public Texture2D texture = null;
    }
}