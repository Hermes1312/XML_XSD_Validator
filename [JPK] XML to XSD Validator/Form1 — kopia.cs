using _JPK__XML_to_XSD_Validator.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

namespace _JPK__XML_to_XSD_Validator
{
    public partial class Form1 : Form
    {
        // Zmienne
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
        private void defaultSchema(object sender, EventArgs e)
        {
            if(SchemaFilePath != null)
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
        private int i;
        private void populateTreeview()
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(new StringReader(fastColoredTextBox1.Text));
                treeView1.Nodes.Clear();
                
                for (i=0; i < 4; i++)
                {
                    treeView1.Nodes.Add(new TreeNode(xDoc.DocumentElement.ChildNodes[i].Name));
                    TreeNode tNode = new TreeNode();
                    tNode = (TreeNode)treeView1.Nodes[i];
                    //addTreeNode(xDoc.DocumentElement.ChildNodes[i], tNode);
                    XmlNode xn = xDoc.DocumentElement.ChildNodes[i];
                    Thread thread = new Thread(() => ControlExtensions.UIThread(treeView1, new Action(() => addTreeNode(xn, tNode))));
                    thread.Start();
                    
                }
            }

            catch (Exception ex) //General exception
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void addTreeNode(XmlNode xmlNode, TreeNode treeNode)
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


        // Funkcje przycisków
        private void zapiszToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Plik XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                fastColoredTextBox1.SaveToFile(sfd.FileName, Encoding.UTF8);
            }
        }
        private void sprawdz_Click(object sender, EventArgs e)
        {
            if (SchemaFilePath != null & XmlFilePath != null)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.Schemas.Add("http://crd.gov.pl/wzor/2020/03/06/9196/", SchemaFilePath);
                    settings.ValidationType = ValidationType.Schema;
                    XmlReader reader = XmlReader.Create(new StringReader(fastColoredTextBox1.Text), settings);
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

            else if(SchemaFilePath == null)
            {
                MessageBox.Show($"Ścieżka pliku XSD nie może być pusta!", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            else if (XmlFilePath == null)
            {
                MessageBox.Show($"Ścieżka pliku XML nie może być pusta!", "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void XML_OFD(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Plik XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                XmlFilePath = ofd.FileName;
                fastColoredTextBox1.OpenFile(XmlFilePath, Encoding.UTF8);
                /*XmlDocument xDoc = new XmlDocument();
                xDoc.Load(new StringReader(fastColoredTextBox1.Text));
                MessageBox.Show(xDoc.DocumentElement.ChildNodes[0].Name);*/
                    //populateTreeview();
                    XmlStatusIcon.Image = Resources.okStatus;
                Cursor.Current = Cursors.Default;
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
            populateTreeview();
        }

        private void zawijanieWierszówToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.WordWrap = !zawijanieWierszówToolStripMenuItem.Checked;
            zawijanieWierszówToolStripMenuItem.Checked = !zawijanieWierszówToolStripMenuItem.Checked;
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

    public static class ControlExtensions
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void UIThread(this Control @this, Action code)
        {
            if (@this.InvokeRequired)
            {
                @this.BeginInvoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }
}
