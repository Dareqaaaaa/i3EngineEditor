/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 12 jul 2019
 */

using i3EngineEditor.Packet;
using i3EngineEditor.Tools;
using System;
using System.Windows.Forms;
using System.Xml;

namespace i3EngineEditor.Managers
{
    public class XmlManager
    {
        public bool Create(i3Editor form, ObjectsManager om, string path)
        {
            try
            {
                Objects obj = om.registryRoot();
                if (obj != null)
                {
                    form.setState(1);
                    int SIZEITEMS = obj.State.Items.Values.Count, SIZEINDEX = 1;
                    foreach (Objects key in obj.State.Items.Values)
                    {
                        using (XmlTextWriter xr = new XmlTextWriter($"{path}\\{key.State.Name}.xml", Settings.gI().Encoding))
                        {
                            xr.Formatting = Formatting.Indented;
                            xr.WriteStartDocument();
                            xr.WriteStartElement(obj.State.Name);
                            foreach (TreeNode node in form.tnodes.Nodes)
                                Load(form, xr, om, node.Nodes, key.State.Name);
                            xr.WriteEndElement();
                            xr.Flush();
                        }
                        if (form.updateBar(SIZEINDEX++, SIZEITEMS))
                            continue;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                form.setState(2);
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
            return false;
        }
        public void Load(i3Editor form, XmlTextWriter xr, ObjectsManager om, TreeNodeCollection tnc, string name)
        {
            foreach (TreeNode node in tnc)
            {
                if (node.Nodes.Count > 0)
                {
                    if (name.Length == 0 || node.Text.Equals(name))
                    {
                        xr.WriteStartElement(node.Text);
                        Load(form, xr, om, node.Nodes, "");
                        xr.WriteEndElement();
                    }
                }
                else
                {
                    if (om.objectKeys.TryGetValue(ulong.Parse(node.ImageKey), out Objects obj))
                    {
                        xr.WriteStartElement(node.Text);
                        if (obj.State.ItemType == 9)
                            xr.WriteAttributeString("Type", $"{obj.State.ValueType}");
                        else
                            xr.WriteAttributeString("TypeUnique", $"{(ValueEnum)obj.State.ItemType}");
                        if (obj.State.ItemsNations.Count > 0)
                            xr.WriteAttributeString("Value", obj.State.ItemsNations[obj.State.ItemType == 9 ? form.comboBox1.SelectedIndex : 0].ToString());
                        xr.WriteEndElement();
                    }
                }
            }
        }
    }
}