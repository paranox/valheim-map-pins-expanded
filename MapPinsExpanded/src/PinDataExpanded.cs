using UnityEngine;

namespace Paranox.MapPinsExpanded
{
    class PinDataExpanded : Minimap.PinData
    {
        public int m_id;
        public Color m_color;

        public PinDataExpanded(string name)
        {
            ProcessNameString(name);
        }

        public PinDataExpanded(Minimap.PinData vanillaData)
        {
            ProcessNameString(vanillaData.m_name);
            m_type = vanillaData.m_type;
            m_icon = vanillaData.m_icon;
            m_pos = vanillaData.m_pos;
            m_save = vanillaData.m_save;
            m_ownerID = vanillaData.m_ownerID;
            m_checked = vanillaData.m_checked;
            m_doubleSize = vanillaData.m_doubleSize;
            m_animate = vanillaData.m_animate;
            m_worldSize = vanillaData.m_worldSize;
            m_uiElement = vanillaData.m_uiElement;
            m_checkedElement = vanillaData.m_checkedElement;
            m_iconElement = vanillaData.m_iconElement;
            m_NamePinData = vanillaData.m_NamePinData;
        }

        public void ProcessNameString(string name)
        {
            m_id = -1;
            m_color = Color.white;

            if (name.StartsWith("[") && name.EndsWith("]"))
            {
                string[] parsed = name.Substring(1, name.Length - 2).Split(',');
                m_id = parsed.Length >= 1 ? int.Parse(parsed[0]) : 0;
                m_name = parsed.Length >= 2 ? parsed[1] : "";
                if (parsed.Length >= 5)
                {
                    m_color = new Color(
                        Mathf.Clamp01(float.Parse(parsed[2])),
                        Mathf.Clamp01(float.Parse(parsed[3])),
                        Mathf.Clamp01(float.Parse(parsed[4])));
                }
                return;
            }

            m_name = name;
        }

        public string GetSaveString()
        {
            if (m_id >= 0)
                return string.Format("[{0},{1},{2},{3},{4}]", m_id, m_name, m_color.r, m_color.g, m_color.b);

            return m_name;
        }
    }
}
