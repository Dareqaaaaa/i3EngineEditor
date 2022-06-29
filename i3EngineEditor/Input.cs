/* TeamExploit \o/
 * 
 * File created by PISTOLA
 * 
 * 11 jul 2019
 */

using System;
using System.Windows.Forms;

namespace i3EngineEditor
{
    public partial class Input : Form
    {
        public int Value;
        public Input(ListView list, int lastLength, int newLength)
        {
            InitializeComponent();
            foreach (ListViewItem item in list.Items)
                objectView.Items.Add(item.Clone() as ListViewItem);
            label3.Text = $"Last Length: {lastLength}";
            label4.Text = $"Recv Length: {newLength}";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int Length = int.Parse(textBox1.Text);
                if (Length != 0)
                {
                    Value = Length;
                    DialogResult = DialogResult.OK;
                }
                else
                    MessageBox.Show("Value can not be equal to 0.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}