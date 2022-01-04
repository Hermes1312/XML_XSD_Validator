using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace _JPK__XML_to_XSD_Validator
{
    public partial class Form2 : Form
    {
        private DataSet dataSet = new DataSet();
        private XmlNode node;
        private XmlDocument xDoc;
        public string nameSpace;
        public Form2(XmlNode _node, XmlDocument _xDoc)
        {
            InitializeComponent();
            ReadConfig();
            xDoc = _xDoc;
            node = _node;
            this.Text = node.Name;
            CreateGridView(node.OuterXml);
        }

        private void ReadConfig()
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load("temp.xml");
                nameSpace = xDoc.DocumentElement.GetAttribute("xmlns:tns");
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateGridView(string outerXml)
        {
            try
            {
                dataSet.ReadXml(XmlReader.Create(new StringReader(outerXml)));
                //dataGridView1.DataSource = dataSet.Tables[0];
                object[] rows = new object[node.ChildNodes.Count];

                for (int i=0; i < node.ChildNodes.Count; i++)
                {
                    dataGridView1.Columns.Add("", node.ChildNodes[i].Name);
                    rows[i] = node.ChildNodes[i].InnerText;
                }

                dataGridView1.Rows.Add(rows);


            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                this.Hide();
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        public void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            var msg = MessageBox.Show("Czy chcesz zapisać zmiany?", "", MessageBoxButtons.YesNoCancel);
            if (msg == DialogResult.Yes)
            {
                for(int i=0; i < node.ChildNodes.Count; i++)
                    node.ChildNodes[i].InnerText = dataGridView1.Rows[0].Cells[i].Value.ToString();

                XmlNode newNode = node;
                var nsmgr = new XmlNamespaceManager(xDoc.NameTable);
                nsmgr.AddNamespace("tns", nameSpace);
                Form1 form1 = new Form1();
                string xpath = form1.GetXPath(node);
                Cursor.Current = Cursors.WaitCursor;
                form1.UpdateXML(newNode, xpath);
                Cursor.Current = Cursors.Default;
            }
            else if (msg == DialogResult.No)
            {

            }
            else if (msg == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}
