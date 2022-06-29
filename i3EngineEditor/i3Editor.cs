/* TeamExploit \o/
 * 
 * Build by PISTOLA
 * 
 * 12 jul 2019
 */

using i3EngineEditor.Packet;
using i3EngineEditor.Managers;
using i3EngineEditor.Tools;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace i3EngineEditor
{
    public partial class i3Editor : Form
    {
        public byte[] fileBuffer, nationBuffer;
        public FilesExt fext = FilesExt.NULL;
        public FileInfo info;
        public List<string> clients = new List<string>();
        public Header h;
        public TableManager tm;
        public ObjectsManager om;
        public XmlManager xm;
        public long last_node = -1, last_nation = -1;
        public string nationPath;
        public string[] args;
        public i3Editor(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (this)
            {
                try
                {
                    menuStrip1.Enabled = false;
                    bool dragFile = (args != null && args.Length > 0 && sender == null && e == null);
                    browserFile.Filter = FilesExtData.getTypes(-1);
                    if (dragFile || browserFile.ShowDialog() == DialogResult.OK)
                    {
                        info = new FileInfo(browserFile.FileName);
                        if (info != null)
                        {
                            last_node = -1;
                            fileBuffer = File.ReadAllBytes(info.FullName);
                            if (fileBuffer.Length > 0)
                            {
                                fext = (FilesExt)browserFile.FilterIndex;
                                if (fext == FilesExt.NULL)
                                    fext = FilesExtData.getExtType(info.Extension.ToLower());
                                if (fext == FilesExt.SIF || fext == FilesExt.TEXT)
                                {
                                    updateBar(1, 3);
                                    if (fext == FilesExt.SIF)
                                    {
                                        for (var i = 0; i < fileBuffer.Length; i += 2048)
                                        {
                                            Utils.Unshift(fileBuffer, i, 2048, 7); //descriptografa em blocos de 2048 bytes (FIX)
                                        }
                                    }
                                    h = new Header(info)
                                    {
                                        builder = new StringBuilder()
                                    };
                                    h.LoadI3R2(this, new byte[0], fileBuffer.Length, false);
                                    updateBar(2, 3);
                                    openToolStripMenuItem.Enabled = false;
                                    closeToolStripMenuItem.Enabled = true;
                                    toolsToolStripMenuItem.Enabled = true;
                                    saveStrip.Enabled = true;
                                    dumpToolStripMenuItem.Enabled = false;
                                    tnodes.Enabled = false;
                                    tnodes.Visible = false;
                                    viewnodes.Enabled = false;
                                    viewnodes.Visible = false;
                                    textBox1.Enabled = true;
                                    textBox1.Visible = true;
                                    comboBox1.Visible = false;
                                    comboBox1.Enabled = false;
                                    formatToolStripMenuItem.Enabled = true;
                                    AllowDrop = false;
                                    textBox1.Clear();
                                    textBox1.Text = Settings.gI().Encoding.GetString(fileBuffer);
                                    updateBar(3, 3);
                                    updateBar(-1, 0);
                                    return;
                                }
                                else if (fext == FilesExt.ENCRYPT || fext == FilesExt.DECRYPT)
                                {
                                    dragFile = true;
                                    List<string> bits = new List<string>();
                                    for (int i = 1; i < 9; i++)
                                        bits.Add(i.ToString());
                                    string bit = "1";
                                    if (InfoBox.InputBox(bits, Application.ProductName, $"Set shift to {fext.ToString().ToLower()}:", ref bit) == DialogResult.OK)
                                    {
                                        if (fext == FilesExt.ENCRYPT)
                                        {
                                            for (var i = 0; i < fileBuffer.Length; i += 2048)
                                            {
                                                Utils.Shift(fileBuffer, i, 2048, Convert.ToInt32(bit)); //descriptografa em blocos de 2048 bytes (FIX)
                                            }
                                        }
                                        else
                                        {
                                            for (var i = 0; i < fileBuffer.Length; i += 2048)
                                            {
                                                Utils.Unshift(fileBuffer, i, 2048, Convert.ToInt32(bit)); //descriptografa em blocos de 2048 bytes (FIX)
                                            }
                                        }
                                        saveStrip.Enabled = true;  
                                        toolStripMenuItem3_Click(fileBuffer, null);
                                        saveStrip.Enabled = false;
                                    }
                                    bits.Clear();
                                    bits = null;
                                }
                                else if (fext == FilesExt.I3I || fext == FilesExt.I3VTEX)
                                {
                                    bool crypt = false;
                                    if (!Settings.gI().Encoding.GetString(fileBuffer, 0, 4).Equals(fext == FilesExt.I3I ? "I3IB" : "VTIH"))
                                    {
                                        for (var i = 0; i < fileBuffer.Length; i += 2048)
                                        {
                                            Utils.Unshift(fileBuffer, i, 2048, 3); //descriptografa em blocos de 2048 bytes (FIX)
                                        }
                                        crypt = true;
                                    }
                                    using (MemoryStream stream = new MemoryStream(fileBuffer))
                                    using (Read r = new Read(new BinaryReader(stream)))
                                    {
                                        if (r != null)
                                        {
                                            h = new Header(info);
                                            if ((fext == FilesExt.I3I ? h.LoadI3IB(this, r, crypt) : fext == FilesExt.I3VTEX ? h.LoadVTIH(this, r, crypt) : false) && h.ProcessImage(this, r))
                                            {
                                                openToolStripMenuItem.Enabled = false;
                                                closeToolStripMenuItem.Enabled = true;
                                                toolsToolStripMenuItem.Enabled = true;
                                                dumpToolStripMenuItem.Enabled = true;
                                                saveStrip.Enabled = true;
                                                tnodes.Enabled = false;
                                                tnodes.Visible = false;
                                                viewnodes.Enabled = false;
                                                viewnodes.Visible = false;
                                                comboBox1.Enabled = false;
                                                comboBox1.Visible = false;
                                                formatToolStripMenuItem.Enabled = false;
                                                updateBar(-1, 0);
                                                AllowDrop = true;
                                                dragSave.Visible = true;
                                                Logger.gI().Info($"Loaded {h.Format} image.", MessageBoxIcon.Information);
                                                return;
                                            }
                                            throw new Exception($"Missing information to read the file.");
                                        }
                                    }
                                }
                                else if (fext == FilesExt.I3PACK || fext == FilesExt.I3GAME || fext == FilesExt.PEF && CheckNation())
                                {
                                    bool enc = false;
                                    if ((fext == FilesExt.I3GAME || fext == FilesExt.PEF) && !Settings.gI().Encoding.GetString(fileBuffer, 0, 4).Equals("I3R2"))
                                    {
                                        for (var i = 0; i < fileBuffer.Length; i += 2048)
                                        {
                                            Utils.Unshift(fileBuffer, i, 2048, 3); //descriptografa em blocos de 2048 bytes (FIX)
                                        }
                                        enc = true;
                                    }
                                    using (MemoryStream stream = new MemoryStream(fileBuffer))
                                    using (Read r = new Read(new BinaryReader(stream)))
                                    {
                                        if (r != null)
                                        {
                                            h = new Header(info);
                                            if (h.LoadI3R2(this, r.ReadBytes(184), -1, enc))
                                            {
                                                textBox1.Clear();
                                                tm = new TableManager();
                                                if (tm.Load(this, r))
                                                {
                                                    om = new ObjectsManager();
                                                    if (om.Load(this, r))
                                                    {
                                                        if ((fext == FilesExt.I3GAME || fext == FilesExt.PEF) && LoadNodes())
                                                        {
                                                            xm = new XmlManager();
                                                            openToolStripMenuItem.Enabled = false;
                                                            closeToolStripMenuItem.Enabled = true;
                                                            toolsToolStripMenuItem.Enabled = true;
                                                            dumpToolStripMenuItem.Enabled = true;
                                                            saveStrip.Enabled = true;
                                                            tnodes.Enabled = true;
                                                            tnodes.Visible = true;
                                                            viewnodes.Enabled = true;
                                                            viewnodes.Visible = true;
                                                            AllowDrop = false;
                                                            updateBar(-1, 0);
                                                            last_nation = -1;
                                                            comboBox1.SelectedIndex = 1;
                                                            comboBox1.Enabled = true;
                                                            comboBox1.Visible = true;
                                                            last_nation = -1;
                                                            comboBox1.SelectedIndex = 0;
                                                            formatToolStripMenuItem.Enabled = false;
                                                            searchToolStripMenuItem.Enabled = true;
                                                            return;
                                                        }
                                                        else if (fext == FilesExt.I3PACK)
                                                        {
                                                        }
                                                    }
                                                }
                                                throw new Exception($"Missing information to read the file.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if ((!dragFile && fext != FilesExt.ENCRYPT && fext != FilesExt.DECRYPT) && Logger.gI().Info($"Could not open file.", MessageBoxIcon.Warning) == DialogResult.OK)
                            openToolStripMenuItem_Click(sender, e);
                        else
                        {
                            if (dragFile && fext != FilesExt.ENCRYPT && fext != FilesExt.DECRYPT)
                                Logger.gI().Info($"Could not open file.", MessageBoxIcon.Warning);
                            closeToolStripMenuItem_Click(null, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    setState(2);
                    Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                    Logger.gI().Log(ex.ToString());
                    closeToolStripMenuItem_Click(null, null);
                }
                finally
                {
                    menuStrip1.Enabled = true;
                    args = null;
                }
            }
        }
        private void DumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (menuStrip1.Enabled)
            {
                menuStrip1.Enabled = false;
                try
                {
                    if (fext == FilesExt.I3I || fext == FilesExt.I3VTEX)
                    {
                        saveFileDialog1.InitialDirectory = info.FullName;
                        saveFileDialog1.Filter = FilesExtData.getImageType(h.Format);
                        saveFileDialog1.FileName = info.Name;
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            updateBar(2, 5);
                            byte[] buffer;
                            using (MemoryStream stream = new MemoryStream())
                            using (Write writer = new Write(new BinaryWriter(stream)))
                            {
                                if (h.dds != null)
                                    writer.WriteBytes(h.dds.ProcessData);
                                else if (h.tga != null)
                                    writer.WriteBytes(h.tga.ProcessData);
                                updateBar(3, 5);
                                buffer = new byte[stream.Length];
                                Array.Copy(stream.GetBuffer(), 0, buffer, 0, stream.Length);
                                updateBar(4, 5);
                            }
                            using (FileStream stream = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                            {
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                stream.Close();
                                Utils.RemoveEvents(stream);
                                buffer = null;
                            }
                            updateBar(5, 5);
                            Logger.gI().Info($"File dump successfully.", MessageBoxIcon.Information);
                        }
                    }
                    else if (fext == FilesExt.PEF)
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            bool state = xm.Create(this, om, folderBrowserDialog1.SelectedPath);
                            if (!state) setState(2);
                            Logger.gI().Info($"{info.Name.Split('.')[0]} dump {(state ? "successfully" : "error")}.", state ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                        }
                    }
                    else Logger.gI().Info($"Unrecognized.", MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    setState(2);
                    Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                    Logger.gI().Log(ex.ToString());
                }
                finally
                {
                    menuStrip1.Enabled = true;
                    updateBar(-1, 0);
                }
            }
        }
        private void Editor_Load(object sender, EventArgs e)
        {
            try
            {
                //var value = Convert.ToInt32(Convert.ToString(0x6B, 16).Substring(1, 1), 16);// | (0 << 3));
                //MessageBox.Show("value: " + value);
                string dir = Directory.GetCurrentDirectory();
                if (File.Exists($"{dir}\\SharpDX.dll") && File.Exists($"{dir}\\SharpDX.Mathematics.dll"))
                {
                    Settings set = Settings.gI();
                    set.Load();
                    if (args != null && args.Length > 0)
                        Editor_DragDrop(args, null);
                }
                else throw new Exception();
            }
            catch (Exception ex)
            {
                Enabled = false;
                Visible = false;
                Logger.gI().Info($"There was a problem opening.", MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
                Close();
            }
        }
        public bool CheckNation()
        {
            string path = $"{Directory.GetCurrentDirectory()}\\Nation.nlf";
            if (!File.Exists(path))
            {
                if (nationBuffer == null || nationBuffer.Length == 0)
                {
                    Logger.gI().Info($"Nation.nlf was not found at the root.", MessageBoxIcon.Warning);
                    browserFile.Filter = $"Nation File|Nation.nlf";
                    if (browserFile.ShowDialog() == DialogResult.OK)
                    {
                        path = browserFile.FileName;
                        nationPath = path;
                        nationBuffer = null;
                    }
                    else return false;
                }
            }
            else
            {
                nationPath = path;
            }
            byte[] newNationBuffer = File.ReadAllBytes(nationPath);
            if (nationBuffer != null && nationBuffer.Length > 0)
            {
                if (!Utils.GetHash<MD5>(nationBuffer).Equals(Utils.GetHash<MD5>(newNationBuffer)))
                {
                    Logger.gI().Info($"The Nation.nlf has changed, the list will be updated.", MessageBoxIcon.Information);
                    clients.Clear();
                }
                else return true;
            }
            nationBuffer = newNationBuffer;
            comboBox1.Items.Clear();
            foreach (string text in File.ReadAllLines(nationPath))
            {
                if (!text.Contains(";"))
                {
                    clients.Add(text);
                    comboBox1.Items.Add(text);
                }
            }
            comboBox1.SelectedIndex = 0;
            if (clients.Count == 0)
            {
                Logger.gI().Info($"Nation.nlf is empty.", MessageBoxIcon.Error);
                return false;
            }
            newNationBuffer = null;
            WindowState = FormWindowState.Normal;
            //Show();
            BringToFront();
            Focus();
            return true;
        }
        public class InfoBox
        {
            public static DialogResult InputBox(List<string> list, string title, string promptText, ref string value)
            {
                DialogResult dialogResult = DialogResult.Cancel;
                using (Form form = new Form())
                using (Label label = new Label())
                using (ComboBox textBox = new ComboBox())
                using (Button buttonOk = new Button())
                using (Button buttonCancel = new Button())
                {
                    foreach (string item in list)
                        textBox.Items.Add(item);
                    form.Text = title;
                    label.Text = promptText;
                    textBox.Text = value;
                    textBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    buttonOk.Text = "OK";
                    buttonCancel.Text = "Cancel";
                    buttonOk.DialogResult = DialogResult.OK;
                    buttonCancel.DialogResult = DialogResult.Cancel;
                    label.SetBounds(9, 20, 372, 13);
                    textBox.SetBounds(12, 36, 372, 20);
                    buttonOk.SetBounds(228, 72, 75, 23);
                    buttonCancel.SetBounds(309, 72, 75, 23);
                    label.AutoSize = true;
                    textBox.Anchor |= AnchorStyles.Right;
                    buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    form.ClientSize = new Size(396, 107);
                    form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                    form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.AcceptButton = buttonOk;
                    form.CancelButton = buttonCancel;
                    dialogResult = form.ShowDialog();
                    value = textBox.SelectedItem.ToString();
                    form.Close();
                    Utils.RemoveEvents(form);
                }
                return dialogResult;
            }
            public static DialogResult DXTBOX(Dictionary<string, uint> list, string title, string promptText, ref uint value)
            {
                DialogResult dialogResult = DialogResult.Cancel;
                using (Form form = new Form())
                using (Label label = new Label())
                using (ComboBox textBox = new ComboBox())
                using (Button buttonOk = new Button())
                using (Button buttonCancel = new Button())
                {
                    foreach (string item in list.Keys)
                    {
                        textBox.Items.Add(item);
                        if (string.IsNullOrEmpty(textBox.Text))
                            textBox.Text = item;
                    }
                    form.Text = title;
                    label.Text = promptText;
                    textBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    buttonOk.Text = "OK";
                    buttonCancel.Text = "Cancel";
                    buttonOk.DialogResult = DialogResult.OK;
                    buttonCancel.DialogResult = DialogResult.Cancel;
                    label.SetBounds(9, 20, 372, 13);
                    textBox.SetBounds(12, 36, 372, 20);
                    buttonOk.SetBounds(228, 72, 75, 23);
                    buttonCancel.SetBounds(309, 72, 75, 23);
                    label.AutoSize = true;
                    textBox.Anchor |= AnchorStyles.Right;
                    buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    form.ClientSize = new Size(396, 107);
                    form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                    form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.AcceptButton = buttonOk;
                    form.CancelButton = buttonCancel;
                    dialogResult = form.ShowDialog();
                    list.TryGetValue(textBox.SelectedItem.ToString(), out value);
                    form.Close();
                    Utils.RemoveEvents(form);
                }
                return dialogResult;
            }
            public static DialogResult EditBox(string title, string promptText, ref string value, ref ValueEnum valueType)
            {
                DialogResult dialogResult = DialogResult.Cancel;
                using (Form form = new Form())
                using (Label label = new Label())
                using (TextBox textBox = new TextBox())
                using (Button buttonOk = new Button())
                using (ComboBox comboBox = new ComboBox())
                {
                    foreach (ValueEnum en in Enum.GetValues(typeof(ValueEnum)))
                        comboBox.Items.Add(en);
                    comboBox.Text = valueType.ToString();
                    comboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    comboBox.SetBounds(228, 72, 75, 23);
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    textBox.Text = value;
                    form.Text = title;
                    label.Text = promptText;
                    textBox.Text = value;
                    buttonOk.Text = "Save";
                    buttonOk.DialogResult = DialogResult.OK;
                    label.SetBounds(9, 20, 372, 13);
                    textBox.SetBounds(12, 36, 372, 20);
                    buttonOk.SetBounds(309, 72, 75, 23);
                    label.AutoSize = true;
                    textBox.Anchor |= AnchorStyles.Right;
                    buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    form.ClientSize = new Size(396, 107);
                    form.Controls.AddRange(new Control[] { label, textBox, buttonOk, comboBox });
                    form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.AcceptButton = buttonOk;
                    dialogResult = form.ShowDialog();
                    value = textBox.Text;
                    valueType = (ValueEnum)Enum.Parse(typeof(ValueEnum), comboBox.Text);
                    form.Close();
                    Utils.RemoveEvents(form);
                }
                return dialogResult;
            }
            public static DialogResult PutText(string title, string promptText, ref string value)
            {
                DialogResult dialogResult = DialogResult.Cancel;
                using (Form form = new Form())
                using (Label label = new Label())
                using (TextBox textBox = new TextBox() { Text = value })
                using (Button buttonOk = new Button() { Enabled = false, Anchor = AnchorStyles.Bottom | AnchorStyles.Right })
                {
                    textBox.Text = value;
                    form.Text = title;
                    label.Text = promptText;
                    textBox.TextChanged += delegate (object sender2, EventArgs e)
                    {
                        buttonOk.Enabled = !string.IsNullOrEmpty(textBox.Text);
                    };
                    buttonOk.Text = "Find it";
                    buttonOk.DialogResult = DialogResult.OK;
                    label.SetBounds(9, 20, 372, 13);
                    textBox.SetBounds(12, 36, 372, 20);
                    buttonOk.SetBounds(309, 72, 75, 23);
                    label.AutoSize = true;
                    textBox.Anchor |= AnchorStyles.Right;
                    form.ClientSize = new Size(396, 107);
                    form.Controls.AddRange(new Control[] { label, textBox, buttonOk });
                    form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.AcceptButton = buttonOk;
                    dialogResult = form.ShowDialog();
                    value = textBox.Text;
                    form.Close();
                    Utils.RemoveEvents(form);
                }
                return dialogResult;
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://raw.githubusercontent.com/eoqhenrqu/PointBlankProject/37b6d58e7f18bb8b833a37983d89ec40c55b5cfd/Release.txt?token=AMDVDUNOM2VW7G4RCNXSAA25Q3ONU");
        }
        private void Editor_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Dispose(true);
                Application.Exit();
                Application.ExitThread();
                Process.GetCurrentProcess().Close();
                Environment.Exit(0);          
            }
            catch { }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            menuStrip1.Enabled = false;
            int state = 1;
            try
            {
                int steps = 7;
                setState(state = 1);
                if (updateBar(1, steps))
                {
                    last_node = -1;
                    last_nation = -1;
                    args = null;
                    fext = FilesExt.NULL;
                    openToolStripMenuItem.Enabled = true;
                    closeToolStripMenuItem.Enabled = false;
                    toolsToolStripMenuItem.Enabled = false;
                    saveStrip.Enabled = false;
                    dumpToolStripMenuItem.Enabled = false;
                    formatToolStripMenuItem.Enabled = false;
                    searchToolStripMenuItem.Enabled = false;
                    dragSave.Visible = false;
                    comboBox1.Enabled = false;
                    comboBox1.Visible = false;
                    //comboBox1.SelectedIndex = 0;
                    tnodes.Enabled = false;
                    tnodes.Visible = false;
                    viewnodes.Enabled = false;
                    viewnodes.Visible = false;
                    AllowDrop = true;
                    textBox1.Enabled = false;
                    textBox1.Visible = false;
                    textBox1.Clear();
                    tnodes.Nodes.Clear();
                    Utils.RemoveEvents(tnodes);
                    viewnodes.Items.Clear();
                    if (updateBar(2, steps))
                    {
                        if (h != null) h.Dispose();
                        if (tm != null) tm.tableKeys.Clear();
                        if (om != null)
                        {
                            foreach (Objects obj in om.objectKeys.Values)
                            {
                                obj.Dispose();
                                Utils.RemoveEvents(obj);
                            }
                            om.objectKeys.Clear();
                            //GC.ReRegisterForFinalize(om);
                        }
                        //if (xm != null) GC.ReRegisterForFinalize(xm);
                        if (updateBar(3, steps))
                        {
                            Utils.RemoveEvents(fileBuffer);
                            Utils.RemoveEvents(info);
                            Utils.RemoveEvents(h);
                            Utils.RemoveEvents(tm);
                            Utils.RemoveEvents(om);
                            Utils.RemoveEvents(xm);
                            if (updateBar(4, steps))
                            {
                                fileBuffer = null;
                                info = null;
                                h = null;
                                tm = null;
                                om = null;
                                xm = null;
                                if (updateBar(5, steps))
                                {
                                    if (updateBar(6, steps))
                                    {
                                        Process.GetCurrentProcess().Refresh();
                                        Application.DoEvents();
                                        Refresh();
                                        updateBar(7, steps);
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                        //MemoryCache cache = MemoryCache.Default;                                       
                                        //cache.ToList().ForEach(a => cache.Remove(a.Key));
                                    }
                                    else setState(state = 3);
                                }
                                else setState(state = 3);
                            }
                            else setState(state = 3);
                        }
                        else setState(state = 3);
                    }
                    else setState(state = 3);
                }
                else setState(state = 3);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                setState(state = 2);
            }
            finally
            {
                if (sender != null)
                    Logger.gI().Info($"Closed {(state == 1 ? "successfully" : state == 2 ? "error" : "failed")}.", state == 1 ? MessageBoxIcon.Information : state == 2 ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
                menuStrip1.Enabled = true;
                setState(1); 
                updateBar(-1, 0);
                fileBuffer = null;
                info = null;
                h = null;
                tm = null;
                om = null;
                xm = null;
                GC.GetTotalMemory(true);
            }
        }
        public bool LoadNodes()
        {
            try
            {
                //TreeNode starter = tnodes.Nodes.Add("0", "I3R2", "0", "-1");
                //foreach (string key in tm.tableKeys.Keys)
                //{
                //    TreeNode node = starter.Nodes.Add("0", key, "0", "-1");
                //    if (tm.tableKeys.TryGetValue(key, out List<Table> items))
                //    {
                //        foreach (Table t in items)
                //            node.Nodes.Add("0", t.Name, "0", "-1");
                //    }
                //}
                Objects obj = om.registryRoot();
                if (obj != null)
                {
                    TreeNode node = tnodes.Nodes.Add(obj.ID.ToString(), obj.State.Name, obj.ID.ToString(), readInfos(obj));
                    foreach (Objects obj1 in obj.State.Items.Values)
                    {
                        TreeNode node2 = node.Nodes.Add(obj1.ID.ToString(), obj1.State.Name, obj1.ID.ToString(), readInfos(obj1));
                        foreach (Objects obj2 in obj1.State.Items.Values)
                        {
                            TreeNode node3 = node2.Nodes.Add(obj2.ID.ToString(), obj2.State.Name, obj2.ID.ToString(), readInfos(obj2));
                            foreach (Objects obj3 in obj2.State.Items.Values)
                            {
                                TreeNode node4 = node3.Nodes.Add(obj3.ID.ToString(), obj3.State.Name, obj3.ID.ToString(), readInfos(obj3));
                                foreach (Objects obj4 in obj3.State.Items.Values)
                                {
                                    TreeNode node5 = node4.Nodes.Add(obj4.ID.ToString(), obj4.State.Name, obj4.ID.ToString(), readInfos(obj4));
                                    foreach (Objects obj5 in obj4.State.Items.Values)
                                    {
                                        TreeNode node6 = node5.Nodes.Add(obj5.ID.ToString(), obj5.State.Name, obj5.ID.ToString(), readInfos(obj5));
                                        foreach (Objects obj6 in obj5.State.Items.Values)
                                        {
                                            TreeNode node7 = node6.Nodes.Add(obj6.ID.ToString(), obj6.State.Name, obj6.ID.ToString(), readInfos(obj6));
                                            foreach (Objects obj7 in obj6.State.Items.Values)
                                            {
                                                node7.Nodes.Add(obj7.ID.ToString(), obj7.State.Name, obj7.ID.ToString(), readInfos(obj7));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info($"Load nodes error.", MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
            return false;
        }
        private void fileInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (h != null && h.builder != null)
            {
                //StringBuilder builder = new StringBuilder();
                //builder.AppendLine(h.builder.ToString());
                //builder.AppendLine($"• RAM: {Utils.SizeSuffix(GC.GetTotalMemory(false))}");
                //Logger.gI().Info(builder.ToString(), MessageBoxIcon.Information);
                //builder = null;
                Logger.gI().Info(h.builder.ToString(), MessageBoxIcon.Information);
            }
            else
                Logger.gI().Info($"HeaderInfo is clear.", MessageBoxIcon.Warning);
        }
        public bool updateBar(int index, int count)
        {
            try
            {
                lock (progressBar1)
                {
                    int value = 0;
                    if (index == -1)
                        value = count;
                    else
                        value = (int)(((double)index / count) * 100);
                    if (value > 100) value = 100;
                    if (value < 0) value = 0;
                    if (progressBar1.Parent.InvokeRequired)
                        progressBar1.Parent.Invoke(new MethodInvoker(delegate { progressBar1.Value = value; }));
                    else
                        progressBar1.Value = value;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
                return false;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        private void Editor_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
        }
        private void Editor_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                args = (e != null ? e.Data.GetData(DataFormats.FileDrop) : sender) as string[];
                if ((fext == FilesExt.I3I || fext == FilesExt.I3VTEX) && dragSave.Visible)
                {
                    using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(args[0])))
                    using (Read r = new Read(new BinaryReader(stream)))
                    {
                        if (h.Format == ImageFormat.TGA && h.tga != null)
                        {

                        }
                        else if (h.Format == ImageFormat.DDS && h.dds != null)
                        {
                            DDS d = new DDS
                            {
                                Header = r.ReadUInt(),
                                Size = r.ReadUInt()
                            };
                            if (d.Header == 0x20534444 && d.Size == 124)
                            {
                                d.Flags = r.ReadUInt();
                                d.Height = r.ReadUInt();
                                d.Width = r.ReadUInt();
                                d.PitchOrLinearSize = r.ReadUInt();
                                d.Depth = r.ReadUInt();
                                d.MipMapCount = r.ReadUInt();
                                d.AlphaBitDepth = r.ReadUInt();
                                for (int i = 0; i < 10; i++)
                                    d.Reserved[i] = r.ReadUInt();
                                d.StructPixelFormat.Size = r.ReadUInt();
                                d.StructPixelFormat.Flags = r.ReadUInt();
                                d.StructPixelFormat.FourCC = r.ReadUInt();
                                d.StructPixelFormat.RGBBitCount = r.ReadUInt();
                                d.StructPixelFormat.RBitMask = r.ReadUInt();
                                d.StructPixelFormat.GBitMask = r.ReadUInt();
                                d.StructPixelFormat.BBitMask = r.ReadUInt();
                                d.StructPixelFormat.ABitMask = r.ReadUInt();
                                d.Caps1 = r.ReadUInt();
                                d.Caps2 = r.ReadUInt();
                                d.Caps3 = r.ReadUInt();
                                d.Caps4 = r.ReadUInt();
                                d.TextureStage = r.ReadUInt();
                                d.Data = r.ReadAllBytes();
                                Dictionary<string, uint> methods = new Dictionary<string, uint>();
                                uint BlockSize = 0;
                                switch (d.GetFormat(ref BlockSize))
                                {
                                    case DDS.PixelFormat.DXT1:
                                        {
                                            methods.Add("Method DXT1-1 (Recommended)", 0xA0000681);
                                            methods.Add("Method DXT1-2", 0x80000680);
                                            methods.Add("Method DXT1-3", 0x31545844);
                                            break;
                                        }
                                    case DDS.PixelFormat.DXT2:
                                        {
                                            methods.Add("Method DXT2-1 (Recommended)", 0xA0000601);
                                            methods.Add("Method DXT2-2", 0x32545844);
                                            break;
                                        }
                                    case DDS.PixelFormat.DXT3:
                                        {
                                            methods.Add("Method DXT3-1 (Recommended)", 0xA0000602);
                                            methods.Add("Method DXT3-2", 0x33545844);
                                            break;
                                        }
                                    case DDS.PixelFormat.DXT4:
                                        {
                                            methods.Add("Method DXT4-1 (Recommended)", 0xA0000603);
                                            methods.Add("Method DXT4-2", 0x34545844);
                                            break;
                                        }
                                    case DDS.PixelFormat.DXT5:
                                        {
                                            methods.Add("Method DXT5-1 (Recommended)", 0xA0000604);
                                            methods.Add("Method DXT5-2", 0x35545844);
                                            break;
                                        }
                                }
                                if (methods.Count > 0)
                                {
                                    InfoBox.DXTBOX(methods, Application.ProductName, "Select the method to create the image:", ref BlockSize);
                                    d.Surface = BlockSize;
                                    methods.Clear();
                                    methods = null;
                                    toolStripMenuItem3_Click(d, null);
                                }
                                else throw new Exception("No methods were found.");
                            }
                            else throw new Exception("Your DDS image is invalid.");
                        }
                        else throw new Exception("Unrecognized format.");
                    }
                }
                else
                {
                    browserFile.FileName = args[0];
                    browserFile.FilterIndex = 0;
                    openToolStripMenuItem_Click(null, null);
                }
                WindowState = FormWindowState.Normal;
                //Show();
                BringToFront();
                Focus();
            }
            catch (Exception ex)
            {
                Logger.gI().Info($"Drag file error. " + ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
        }
        public void setState(int state)
        {
            try
            {
                SendMessage(progressBar1.Handle, 1040, (IntPtr)state, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
        }
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                ListViewItem item = viewnodes.FocusedItem;
                if (e.Button == MouseButtons.Right && (item != null && item.Bounds.Contains(e.Location)) && (om.objectKeys.TryGetValue(ulong.Parse(item.Text), out Objects obj) && obj.Children != 0))
                {
                    Objects.StateObject so = obj.State;
                    string value = so.ItemsNations[so.ItemType == 9 ? comboBox1.SelectedIndex : 0].ToString();
                    string last_value = value;

                    ContextMenu m = new ContextMenu();
                    MenuItem menu = new MenuItem("Edit");
                    {
                        menu.Click += delegate (object sender2, EventArgs e2)
                        {
                            try
                            {
                                ValueEnum last_venum = so.ValueType;
                                ValueEnum venum = so.ValueType;
                                if (InfoBox.EditBox(Application.ProductName, $"Set new value: ", ref value, ref venum) == DialogResult.OK)
                                {
                                    object parse = 0;
                                    switch (venum)
                                    {
                                        case ValueEnum.INT32:
                                            {
                                                parse = Convert.ToInt32(value);
                                                break;
                                            }
                                        case ValueEnum.FLOAT:
                                            {
                                                parse = Convert.ToSingle(value);
                                                break;
                                            }
                                        case ValueEnum.STRING:
                                            {
                                                parse = value;
                                                break;
                                            }
                                        case ValueEnum.POS1:
                                            {
                                                Half3 half = new Half3(float.Parse(value.Split(';')[0].Substring(3)), float.Parse(value.Split(';')[1].Substring(3)), float.Parse(value.Split(';')[2].Substring(3)));
                                                parse = $"X: {half.X} Y: {half.Y} Z: {half.Z}";
                                                break;
                                            }
                                        case ValueEnum.POS2:
                                            {
                                                Half4 half = new Half4(float.Parse(value.Split(';')[0].Substring(3)), float.Parse(value.Split(';')[1].Substring(3)), float.Parse(value.Split(';')[2].Substring(3)), float.Parse(value.Split(';')[3].Substring(3)));
                                                parse = $"X: {half.X} Y: {half.Y} Z: {half.Z}; W: {half.W}";
                                                break;
                                            }
                                        case ValueEnum.HEX:
                                            {
                                                parse = value;
                                                break;
                                            }
                                    }
                                    if (!(value.Equals(last_value) && venum.Equals(last_venum)))
                                    {
                                        if (UpdateTextIndexTable(obj.Children, item.Index, 4, parse.ToString()) && (last_venum == venum ? true : UpdateTextIndexTable(obj.Children, item.Index, 3, venum.ToString())))
                                        {
                                            bool put_all = false;
                                            if (last_venum != venum)
                                            {
                                                int bit = 0;
                                                switch (venum)
                                                {
                                                    case ValueEnum.INT32:
                                                    case ValueEnum.FLOAT:
                                                    case ValueEnum.HEX:
                                                        {
                                                            bit = 4;
                                                            break;
                                                        }
                                                    case ValueEnum.STRING:
                                                        {
                                                            bit = 8 + (parse.ToString().Length * 2);
                                                            break;
                                                        }
                                                    case ValueEnum.POS1:
                                                        {
                                                            bit = 12;
                                                            break;
                                                        }
                                                    case ValueEnum.POS2:
                                                        {
                                                            bit = 16;
                                                            break;
                                                        }
                                                }
                                                ulong diference = obj.Size - (obj.Size = (ulong)(so.Name.Length + 1 + 4 + (so.ItemType == 9 ? 8 : 0) + (so.ItemsNations.Count * bit)));
                                                if (diference < 0)
                                                    obj.Offset += diference;
                                                else if (diference > 0)
                                                    obj.Offset -= diference;
                                                Logger.gI().Info($"Type was changed, all values were updated.", MessageBoxIcon.Information);
                                                put_all = true;
                                            }
                                            if (venum == ValueEnum.STRING && !put_all)
                                            {
                                                ulong diference = (ulong)((last_value.Length * 2) - (parse.ToString().Length * 2));
                                                if (diference < 0)
                                                {
                                                    obj.Offset += diference;
                                                    obj.Size += diference;
                                                }
                                                else if (diference > 0)
                                                {
                                                    obj.Offset -= diference;
                                                    obj.Size -= diference;
                                                }
                                            }
                                            if (put_all)
                                            {
                                                for (int i = 0; i < so.ItemsNations.Count; i++)
                                                    so.ItemsNations[i] = parse;
                                            }
                                            else
                                                so.ItemsNations[so.ItemType == 9 ? comboBox1.SelectedIndex : 0] = parse;
                                            so.ValueType = venum;
                                            Logger.gI().Info($"{so.Name} saved successfully.", MessageBoxIcon.Information);
                                        }
                                        else
                                        {
                                            UpdateTextIndexTable(obj.Children, item.Index, 4, last_value);
                                            UpdateTextIndexTable(obj.Children, item.Index, 3, last_venum.ToString());
                                            Logger.gI().Info($"Save item error.", MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Logger.gI().Info($"Edit value error.", MessageBoxIcon.Error);
                            }
                            finally
                            {
                                m.Dispose();
                                menu.Dispose();
                            }
                        };
                        m.MenuItems.Add(menu);
                    }
                    m.Show(viewnodes, new System.Drawing.Point(e.X, e.Y));
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
        }
        private void tnodes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode node = e.Node;
                if (!node.SelectedImageKey.Equals("-1") && !node.ImageKey.Equals("0") && node != null && om.objectKeys.TryGetValue(ulong.Parse(node.ImageKey), out Objects obj) && ((long)obj.ID != last_node))
                {
                    Objects.StateObject so = obj.State;
                    ListView listv = so.ListView;
                    if (listv != null)
                    {
                        var items = listv.Items;
                        if (items.Count > 0 && so.Nations == 0)
                        {
                            viewnodes.Items.Clear();
                            foreach (ListViewItem lv in items)
                            {
                                viewnodes.Items.Add(lv.Clone() as ListViewItem);
                                if (updateBar(viewnodes.Items.Count, items.Count))
                                    continue;
                            }
                            viewnodes.Refresh();
                            last_node = (long)obj.ID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
            finally
            {
                updateBar(-1, 0);
            }
        }
        public bool UpdateTextIndexTable(ulong idx, int index, int index2, string newText)
        {
            try
            {
                viewnodes.Items[index].SubItems[index2].Text = newText;
                if (idx != 0 && om.objectKeys.TryGetValue(idx, out Objects obj))
                    obj.State.ListView.Items[index].SubItems[index2].Text = newText;
                return true;
            }
            catch { }
            return false;
        }
        private void tnodes_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                TreeNode item = tnodes.SelectedNode;
                if (e.Button == MouseButtons.Right && item.Bounds.Contains(e.Location) && item != null && !item.ImageKey.Equals("0") && !item.SelectedImageKey.Equals("-1"))
                {
                    ContextMenu m = new ContextMenu();
                    MenuItem menu = new MenuItem("Info");
                    {
                        menu.Click += delegate (object sender2, EventArgs e2)
                        {
                            try
                            {
                                Logger.gI().Info(item.SelectedImageKey, MessageBoxIcon.Information);
                            }
                            catch
                            {
                                Logger.gI().Info($"Edit value error.", MessageBoxIcon.Error);
                            }
                            finally
                            {
                                m.Dispose();
                                menu.Dispose();
                            }
                        };
                        m.MenuItems.Add(menu);
                    }
                    m.Show(tnodes, new System.Drawing.Point(e.X, e.Y));
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
        }
        public string readInfos(Objects obj)
        {
            string info = "NON";
            try
            {
                if (obj != null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"[INFO]");
                    builder.AppendLine($"Name: {obj.State.Name}");
                    builder.AppendLine($"Type: {obj.Type}");
                    builder.AppendLine($"ID: {obj.ID}");
                    builder.AppendLine($"Offset: {obj.Offset}");
                    builder.AppendLine($"Size: {obj.Size}");
                    builder.AppendLine($"Initial: {obj.Initial}");
                    builder.AppendLine($"Children: {obj.Children}");
                    if (obj.Initial == 0)
                    {
                        builder.AppendLine($"ItemType: {obj.State.ItemType}");
                        builder.AppendLine($"ValueType: {obj.State.ValueType}");
                        builder.AppendLine($"Nations: {obj.State.Nations}");
                    }
                    builder.AppendLine($"Items: {obj.State.Items.Count}");
                    builder.AppendLine($"Tables: {obj.State.List1.Count}");
                    builder.AppendLine($"Keys: {obj.State.List2.Count}");
                    info = builder.ToString();
                    builder = null;
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info(ex.Message, MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
            return info;
        }
        private void shiftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> bits = new List<string>();
            try
            {
                for (int i = 1; i < 9; i++)
                    bits.Add(i.ToString());
                string bit = "1";
                if (InfoBox.InputBox(bits, Application.ProductName, $"Set shift to encrypt:", ref bit) == DialogResult.OK)
                {
                    for (var j = 0; j < fileBuffer.Length; j += 2048)
                    {
                        Utils.Shift(fileBuffer, j, fileBuffer.Length, Convert.ToInt32(bit));
                    }

                    textBox1.Clear();
                    textBox1.Text = Settings.gI().Encoding.GetString(fileBuffer);
                    textBox1.Refresh();
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info("Shift error.", MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
            finally
            {
                bits.Clear();
                bits = null;
            }
        }
        private void unshiftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> bits = new List<string>();
            try
            {
                for (int i = 1; i < 9; i++)
                    bits.Add(i.ToString());
                string bit = "1";
                if (InfoBox.InputBox(bits, Application.ProductName, $"Set shift to decrypt:", ref bit) == DialogResult.OK)
                {
                    for (var j = 0; j < fileBuffer.Length; j += 2048)
                    {
                        Utils.Unshift(fileBuffer, j, fileBuffer.Length, Convert.ToInt32(bit));
                    }

                    textBox1.Clear();
                    textBox1.Text = Settings.gI().Encoding.GetString(fileBuffer);
                    textBox1.Refresh();
                }
            }
            catch (Exception ex)
            {
                Logger.gI().Info("Unshift error.", MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
            finally
            {
                bits.Clear();
                bits = null;
            }
        }
        private void Search(object sender, EventArgs e)
        {
            try
            {
                string value = "";
                if (InfoBox.PutText(Application.ProductName, "Write text to find:", ref value) == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        setState(1);
                        updateBar(-1, 0);
                        toolsToolStripMenuItem.Enabled = false;
                        comboBox1.Enabled = false;
                        tnodes.Enabled = false;
                        viewnodes.Enabled = false;
                        int count = 0, all = om.objectKeys.Count;
                        foreach (Objects obj in om.objectKeys.Values)
                        {
                            if (obj.State.Name.ToLower().Equals(value.Trim().ToLower()))
                            {
                                updateBar(all, all);
                                MessageBox.Show("exists.");
                                break;
                            }
                            if (updateBar(count++, all))
                                continue;
                        }
                        if (count == all)
                            Logger.gI().Info($"Could not find '{value.Trim()}'", MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                setState(2);
                Logger.gI().Info("Error to find.", MessageBoxIcon.Error);
                Logger.gI().Log(ex.ToString());
            }
            finally
            {
                setState(1);
                updateBar(-1, 0);
                toolsToolStripMenuItem.Enabled = true;
                comboBox1.Enabled = true;
                tnodes.Enabled = true;
                viewnodes.Enabled = true;
            }
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            lock (this)
            {
                if (saveStrip.Enabled)
                {
                    menuStrip1.Enabled = false;
                    try
                    {
                        saveFileDialog1.Filter = FilesExtData.getTypes((int)fext);
                        saveFileDialog1.FileName = info.Name;
                        int permission = 0;
                        if ((e == null && sender == null))
                        {
                            saveFileDialog1.FileName = info.FullName;
                            saveFileDialog1.FilterIndex = h.CryptDefault ? 2 : 1;
                            permission = 1;
                        }
                        else if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                            permission = 2;
                        if (permission != 0)
                        {
                            updateBar(-1, 0);
                            byte[] buffer = new byte[0];
                            if (fext == FilesExt.SIF || fext == FilesExt.TEXT)
                            {
                                buffer = Utils.ToByteArray(textBox1.Text);
                                if (saveFileDialog1.FilterIndex == 2)
                                {
                                    for (var j = 0; j < buffer.Length; j += 2048)
                                    {
                                        Utils.Shift(buffer, j, buffer.Length, 7);
                                    }
                                }
                            }
                            else if (fext == FilesExt.ENCRYPT || fext == FilesExt.DECRYPT)
                            {
                                buffer = sender as byte[];
                            }
                            else if (fext == FilesExt.I3I)
                            {
                                updateBar(2, 4);
                                using (MemoryStream stream = new MemoryStream())
                                using (Write writer = new Write(new BinaryWriter(stream)))
                                {
                                    if (sender is DDS && h.dds != null)
                                    {
                                        DDS dds = sender as DDS;
                                        writer.WriteString(h.Item, 4);
                                        writer.WriteUShort(h.Unknown);
                                        writer.WriteUShort((ushort)dds.Height);
                                        writer.WriteUShort((ushort)dds.Width);
                                        writer.WriteUInt(dds.Surface);
                                        writer.WriteBytes(new byte[10]);
                                        writer.WriteUShort((ushort)dds.MipMapCount);
                                        writer.WriteUShort(0);
                                        writer.WriteBytes(new byte[32]);
                                        writer.WriteBytes(dds.Data);
                                        Utils.RemoveEvents(sender);
                                    }
                                    else if (sender is TGA && h.tga != null)
                                    {
                                        TGA tga = sender as TGA;
                                        writer.WriteString(h.Item, 4);
                                        writer.WriteUShort(h.Unknown);
                                        writer.WriteUShort((ushort)tga.Header.Height);
                                        writer.WriteUShort((ushort)tga.Header.Width);
                                        writer.WriteUInt(tga.Surface);
                                        writer.WriteBytes(new byte[10]);
                                        writer.WriteUShort(0); //MipMapCount
                                        writer.WriteUShort((ushort)tga.PathImage.Length);
                                        writer.WriteBytes(new byte[32]);
                                        writer.WriteString(tga.PathImage, tga.PathImage.Length);
                                        writer.WriteBytes(tga.Data);
                                        Utils.RemoveEvents(sender);
                                    }
                                    updateBar(3, 4);
                                    buffer = new byte[stream.Length];
                                    Array.Copy(stream.GetBuffer(), 0, buffer, 0, stream.Length);
                                    if (saveFileDialog1.FilterIndex == 2)
                                    {
                                        for (var j = 0; j < buffer.Length; j += 2048)
                                        {
                                            Utils.Shift(buffer, j, buffer.Length, 3);
                                        }
                                    }
                                    updateBar(4, 4);
                                }
                            }
                            else if (fext == FilesExt.I3VTEX)
                            {
                                updateBar(2, 4);
                                using (MemoryStream stream = new MemoryStream())
                                using (Write writer = new Write(new BinaryWriter(stream)))
                                {
                                    if (sender is TGA && h.tga != null)
                                    {
                                        TGA tga = sender as TGA;
                                        writer.WriteString(h.Item, 4);
                                        writer.WriteUInt(h.Unknown);
                                        writer.WriteUShort((ushort)tga.Header.Height);
                                        writer.WriteUShort(0);
                                        writer.WriteUShort((ushort)tga.Header.Width);
                                        writer.WriteBytes(new byte[6]);
                                        writer.WriteUInt((ushort)tga.PathImage.Length);
                                        writer.WriteUShort(0); //MipMapCount
                                        writer.WriteBytes(new byte[102]);
                                        writer.WriteString(tga.PathImage, tga.PathImage.Length);
                                        writer.WriteBytes(new byte[1880]);
                                        writer.WriteBytes(tga.Data);
                                        Utils.RemoveEvents(sender);
                                    }
                                    updateBar(3, 4);
                                    buffer = new byte[stream.Length];
                                    Array.Copy(stream.GetBuffer(), 0, buffer, 0, stream.Length);
                                    if (saveFileDialog1.FilterIndex == 2)
                                    {
                                        for (var j = 0; j < buffer.Length; j += 2048)
                                        {
                                            Utils.Shift(buffer, j, buffer.Length, 3);
                                        }
                                    }
                                    updateBar(4, 4);
                                }
                            }
                            else if (fext == FilesExt.PEF)
                            {
                                using (MemoryStream stream = new MemoryStream())
                                using (Write writer = new Write(new BinaryWriter(stream)))
                                {
                                    writer.WriteString(h.Item, 8);
                                    writer.WriteUShort(h.VersionMajor);
                                    writer.WriteUShort(h.VersionMinor);
                                    writer.WriteInt(h.StringTableCount);
                                    writer.WriteULong(h.StringTableOffset);
                                    writer.WriteULong(h.StringTableSizes);
                                    writer.WriteInt(h.ObjectInfoCount);
                                    writer.WriteULong(h.ObjectInfoOffset);
                                    writer.WriteULong(h.ObjectInfoSize);
                                    writer.WriteBytes(new byte[132]);
                                    writer.WriteBytes(tm.buffer);
                                    foreach (Objects obj in om.objectKeys.Values)
                                    {
                                        writer.WriteInt(obj.Type);
                                        writer.WriteULong(obj.ID);
                                        writer.WriteULong(obj.Offset);
                                        writer.WriteULong(obj.Size);
                                        if (updateBar((int)obj.ID, h.ObjectInfoCount))
                                            continue;
                                    }
                                    updateBar(-1, 0);
                                    foreach (Objects obj in om.objectKeys.Values)
                                    {
                                        Objects.StateObject item = obj.State;
                                        writer.WriteByte((byte)item.Name.Length);
                                        writer.WriteString(item.Name, item.Name.Length);
                                        if (obj.Initial > 0)
                                        {
                                            writer.WriteString("TRN3", 4);
                                            writer.WriteShort(0);
                                            writer.WriteInt(obj.Initial - 1);
                                            writer.WriteShort(0);
                                            writer.WriteInt(item.List1.Count);
                                            writer.WriteBytes(new byte[60]);
                                            foreach (ulong keys in item.List1)
                                                writer.WriteInt((int)keys);
                                            writer.WriteString("RGK1", 4);
                                            writer.WriteInt(item.List2.Count);
                                            writer.WriteLong(0);
                                            foreach (ulong keys in item.List2)
                                                writer.WriteULong(keys);
                                        }
                                        else
                                        {
                                            writer.WriteInt(item.ItemType);
                                            if (item.ItemType == 9)
                                            {
                                                writer.WriteInt((int)item.ValueType);
                                                writer.WriteInt(item.Nations);
                                            }
                                            foreach (object value in item.ItemsNations)
                                            {
                                                switch (obj.State.ValueType)
                                                {
                                                    case ValueEnum.INT32:
                                                        {
                                                            writer.WriteInt(Convert.ToInt32(value));
                                                            break;
                                                        }
                                                    case ValueEnum.FLOAT:
                                                        {
                                                            writer.WriteFloat(Convert.ToSingle(value));
                                                            break;
                                                        }
                                                    case ValueEnum.STRING:
                                                        {
                                                            writer.WriteString("RGS3", 4);
                                                            writer.WriteInt(value.ToString().Length);
                                                            writer.WriteUString(value.ToString());
                                                            break;
                                                        }
                                                    case ValueEnum.POS1:
                                                        {
                                                            Half3 half = new Half3(float.Parse(value.ToString().Split(';')[0].Substring(3)), float.Parse(value.ToString().Split(';')[1].Substring(3)), float.Parse(value.ToString().Split(';')[2].Substring(3)));
                                                            writer.WriteFloat(half.X);
                                                            writer.WriteFloat(half.Y);
                                                            writer.WriteFloat(half.Z);
                                                            break;
                                                        }
                                                    case ValueEnum.POS2:
                                                        {
                                                            Half4 half = new Half4(float.Parse(value.ToString().Split(';')[0].Substring(3)), float.Parse(value.ToString().Split(';')[1].Substring(3)), float.Parse(value.ToString().Split(';')[2].Substring(3)), float.Parse(value.ToString().Split(';')[3].Substring(3)));
                                                            writer.WriteFloat(half.X);
                                                            writer.WriteFloat(half.Y);
                                                            writer.WriteFloat(half.Z);
                                                            writer.WriteFloat(half.W);
                                                            break;
                                                        }
                                                    case ValueEnum.HEX:
                                                        {
                                                            for (int i = 0; i < 8; i += 2)
                                                                writer.WriteByte(byte.Parse(value.ToString().Substring(i, 2), NumberStyles.HexNumber));
                                                            break;
                                                        }
                                                }
                                            }
                                        }
                                        if (updateBar((int)obj.ID, h.ObjectInfoCount))
                                            continue;
                                    }
                                    buffer = new byte[stream.Length];
                                    Array.Copy(stream.GetBuffer(), 0, buffer, 0, stream.Length);
                                    if (saveFileDialog1.FilterIndex == 2)
                                    {
                                        for (var j = 0; j < buffer.Length; j += 2048)
                                        {
                                            Utils.Shift(buffer, j, buffer.Length, 3);
                                        }
                                    }
                                }
                            }
                            if (buffer != null && buffer.Length > 0)
                            {
                                using (FileStream stream = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                                {
                                    stream.Write(buffer, 0, buffer.Length);
                                    stream.Flush();
                                    stream.Close();
                                    Utils.RemoveEvents(stream);
                                    buffer = null;
                                }
                                Logger.gI().Info($"File saved successfully.", MessageBoxIcon.Information);
                                if (fext == FilesExt.I3I) closeToolStripMenuItem_Click(null, null);
                            }
                            else throw new Exception();
                        }
                    }
                    catch (Exception ex)
                    {
                        setState(2);
                        Logger.gI().Info($"Build file error or aborted.", MessageBoxIcon.Error);
                        Logger.gI().Log(ex.ToString());
                    }
                    finally
                    {
                        setState(1);
                        menuStrip1.Enabled = true;
                        updateBar(-1, 0);
                    }
                }
            }
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (saveStrip.Enabled)
                toolStripMenuItem3_Click(null, null);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Enabled && last_nation != comboBox1.SelectedIndex)
            {
                last_nation = comboBox1.SelectedIndex;
                toolsToolStripMenuItem.Enabled = false;
                comboBox1.Enabled = false;
                tnodes.Enabled = false;
                viewnodes.Enabled = false;
                try
                {
                    setState(1);
                    updateBar(-1, 0);
                    tnodes.CollapseAll();
                    tnodes.Refresh();
                    viewnodes.Items.Clear();
                    viewnodes.Refresh();
                    foreach (Objects obj in om.objectKeys.Values)
                    {
                        Objects.StateObject so = obj.State;
                        so.ListView.Clear();
                        so.ListView.Dispose();
                        Utils.RemoveEvents(so.ListView);
                        //GC.ReRegisterForFinalize(so.ListView);
                        so.ListView = null;
                        so.ListView = new ListView();
                        foreach (ulong idx in so.List2)
                        {
                            if (om.objectKeys.TryGetValue(idx, out Objects value))
                                so.ListView.Items.Add(new ListViewItem(new string[] { $"{value.ID}", $"{value.State.Name}", $"{value.State.ItemType}", $"{value.State.ValueType}", $"{value.State.ItemsNations[value.State.ItemType == 9 ? comboBox1.SelectedIndex : 0]}" }));
                        }
                        if (updateBar((int)obj.ID, om.objectKeys.Count))
                            continue;
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Application.DoEvents();
                    Refresh();
                    last_node = -1;
                    toolsToolStripMenuItem.Enabled = true;
                    comboBox1.Enabled = true;
                    tnodes.Enabled = true;
                    viewnodes.Enabled = true;
                    updateBar(-1, 0);
                    setState(1);
                }
                catch (Exception ex)
                {
                    setState(2);
                    Logger.gI().Info($"Fatal error. Cache has broken!." + Environment.NewLine + ex.ToString(), MessageBoxIcon.Error);
                    openToolStripMenuItem.Enabled = false;
                    closeToolStripMenuItem.Enabled = true;
                    toolsToolStripMenuItem.Enabled = false;
                    comboBox1.Enabled = false;
                    tnodes.Enabled = false;
                    tnodes.Visible = true;
                    viewnodes.Enabled = false;
                    viewnodes.Visible = true;
                    textBox1.Enabled = false;
                    textBox1.Visible = false;
                    AllowDrop = false;
                }
            }
        }
    }
}