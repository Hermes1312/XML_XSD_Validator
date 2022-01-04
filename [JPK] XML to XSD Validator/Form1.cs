using _JPK__XML_to_XSD_Validator.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace _JPK__XML_to_XSD_Validator
{
    public partial class Form1 : Form
    {
        // Zmienne
        public string currentXml, nameSpace;
        private string SchemaFilePath = null, XmlFilePath;
        private static Form1 instance;
        private bool expanded = false;

        // Funkcje startowe
        public Form1()
        {
            instance = this;
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ReadConfig("config.txt");
            
            pictureBox1.BackColor = Color.Transparent;
            fastColoredTextBox1.Language = FastColoredTextBoxNS.Language.XML;
        }
        private void ReadConfig(string fileName)
        {
            string _schemaFilePath = null;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    _schemaFilePath = sr.ReadLine();

                    if (File.Exists(_schemaFilePath))
                    {
                        SchemaFilePath = _schemaFilePath;
                        instance.XsdStatusIcon.Image = Resources.okStatus;
                    }
                    else
                    {
                        instance.XsdStatusIcon.Image = Resources.errorStatus;
                    }
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadNS()
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

        private void defaultSchema(object sender, EventArgs e)
        {
            if (SchemaFilePath != null)
            {
                File.WriteAllText("config.txt", SchemaFilePath);
                MessageBox.Show("Plik " + SchemaFilePath + "\nbędzie teraz używany jako domyślny schemat!");
            }
            else
            {
                MessageBox.Show("Ścieżka pliku nie może być pusta.", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Funkcje XML'owe
        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    instance.richTextBox1.Text = Environment.NewLine + "Błąd: " + e.Message;
                    break;

                case XmlSeverityType.Warning:
                    instance.richTextBox1.Text = Environment.NewLine + "Ostrzeżenie: " + e.Message;
                    break;
            }
        }
        private void populateTreeview(string path)
        {
            try
            {
                treeView1.Nodes.Clear();
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(path);
                
                TreeNode tNode = new TreeNode();
                tNode.Nodes.Add(new TreeNode(xDoc.DocumentElement.Name));
                tNode = tNode.Nodes[0];
                addTreeNode(xDoc.DocumentElement, tNode);
                treeView1.Nodes.Insert(0, tNode);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void addTreeNode(XmlNode xmlNode, TreeNode treeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList xNodeList;
            if (xmlNode.HasChildNodes)
            {
                xNodeList = xmlNode.ChildNodes;
                for (int x = 0; x <= xNodeList.Count - 1; x++)
                {
                    xNode = xmlNode.ChildNodes[x];
                    treeNode.Nodes.Add(new TreeNode(xNode.Name));
                    tNode = treeNode.Nodes[x];
                    addTreeNode(xNode, tNode);
                }
            }
            else
                treeNode.Text = xmlNode.OuterXml.Trim();
        }
        private void ValidXML()
        {
            if (SchemaFilePath != null & XmlFilePath != null)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.Schemas.Add(nameSpace, SchemaFilePath);
                    settings.ValidationType = ValidationType.Schema;
                    XmlReader reader = XmlReader.Create("temp.xml", settings);
                    XmlDocument document = new XmlDocument();
                    document.Load(reader);
                    ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);
                    // the following call to Validate succeeds.
                    document.Validate(eventHandler);
                    Cursor.Current = Cursors.Default;
                    richTextBox1.Text = "Brak błędów!";
                }

                catch (Exception ex)
                {
                    richTextBox1.Text = ex.Message;
                }
            }

            else if (SchemaFilePath == null)
            {
                MessageBox.Show($"Ścieżka pliku XSD nie może być pusta!", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            else if (XmlFilePath == null)
            {
                MessageBox.Show($"Ścieżka pliku XML nie może być pusta!", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void UpdateXML(XmlNode newNode, string xPath)
        {
            ReadNS();
            XDocument xdoc = XDocument.Load("temp.xml");
            var reader = xdoc.CreateReader();
            var namespacemanager = new XmlNamespaceManager(reader.NameTable);
            namespacemanager.AddNamespace("tns", nameSpace);
            XDocument xdoc1 = XDocument.Parse(File.ReadAllText("temp.xml"));
            XElement old = xdoc1.XPathSelectElement(xPath, namespacemanager);
            XDocument xdoc2 = XDocument.Parse(newNode.OuterXml);
            XElement _new = xdoc2.Root;
            MessageBox.Show(xdoc1.ToString());
            old.ReplaceWith(_new);
            File.WriteAllText("temp.xml", xdoc1.ToString(), Encoding.UTF8);
        }

        private void RTB()
        {
            fastColoredTextBox1.Text = File.ReadAllText("temp.xml", Encoding.UTF8);
        }

        public string GetXPath(XmlNode xmlNode)
        {
            string pathName = xmlNode.Name;
            XmlNode node = xmlNode;
            while (true)
            {
                if (node.ParentNode.Name != "#document")
                {
                    pathName = $"{node.ParentNode.Name}/{pathName}";
                }
                else
                {
                    return pathName;

                }
                node = node.ParentNode;
            }
        }

        // Przyciski
        private void zapiszToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Plik XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.Copy("temp.xml", sfd.FileName, true);
            }
        }
        private void sprawdz_Click(object sender, EventArgs e)
        {
            ValidXML();
        }
        private void XML_OFD(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Plik XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*";

            if (!String.IsNullOrEmpty(SchemaFilePath)){
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    XmlFilePath = ofd.FileName;
                    fastColoredTextBox1.Text = File.ReadAllText(XmlFilePath, Encoding.UTF8);
                    File.WriteAllText("temp.xml", File.ReadAllText(XmlFilePath, Encoding.UTF8), Encoding.UTF8);
                    ReadNS();
                    ReloadTreeView();
                    XmlStatusIcon.Image = Resources.okStatus;
                    Cursor.Current = Cursors.Default;
                }
            }
            else
            {
                MessageBox.Show("Plik schematu nie zotał wybrany!", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void rozwin_Click(object sender, EventArgs e)
        {
            if (!expanded)
            {
                treeView1.ExpandAll();
                button1.Text = "Zwiń";
                expanded = true;
            }
            else
            {
                treeView1.CollapseAll();
                button1.Text = "Rozwiń";
                expanded = false;
            }
        }
        private void reload_Click(object sender, EventArgs e)
        {
            ReloadTreeView();
            button3.Text = "Załaduj ponownie";
        }
        private void ReloadTreeView()
        {

                Cursor.Current = Cursors.WaitCursor;
                populateTreeview("temp.xml");
                Cursor.Current = Cursors.Default;
        }
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(fastColoredTextBox1.Text);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                    nsmgr.AddNamespace("tns", nameSpace);
                    string path = treeView1.SelectedNode.FullPath.Replace(@"\", "/");
                    XmlNode node = document.SelectSingleNode(path, nsmgr);
                    Form2 form2 = new Form2(node, document);
                    form2.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void zawijanieWierszówToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.WordWrap = !zawijanieWierszówToolStripMenuItem.Checked;
            zawijanieWierszówToolStripMenuItem.Checked = !zawijanieWierszówToolStripMenuItem.Checked;
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void fastColoredTextBox1_Load(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RTB();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            fastColoredTextBox1.Text = File.ReadAllText("temp.xml", Encoding.UTF8);
        }
        private void XSD_OFD(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Plik XSD (*.xsd)|*.xsd|Wszystkie pliki (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SchemaFilePath = ofd.FileName;
                XsdStatusIcon.Image = Resources.okStatus;
            }
        }
    }

}


