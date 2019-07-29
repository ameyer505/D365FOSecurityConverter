using D365FOSecurityConverter.Models;
using Equin.ApplicationFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace D365FOSecurityConverter
{
    public partial class Main : Form
    {

        public Main()
        {
            InitializeComponent();
        }

        private void btnInputFileBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = inputFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                tb_inputFile.Text = inputFileDialog.FileName;
            }
        }

        private void btnOutputFolderBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = outputFolderDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                tb_outputFolder.Text = outputFolderDialog.SelectedPath;
            }
        }

        private void ExportSecurityXMLFiles(string inputFilePath, string outputFolderPath)
        {
            Equin.ApplicationFramework.BindingListView<SecurityLayer> ds = dgvSecurityLayers.DataSource as Equin.ApplicationFramework.BindingListView<SecurityLayer>;
            List<SecurityLayer> securityLayerList = ds.NewItemsList as List<SecurityLayer>;
            string rootFolderPath = outputFolderPath + @"\D365FOCustomizedSecurity";
            string roleFolderPath = rootFolderPath + @"\AxSecurityRole";
            string dutyFolderPath = rootFolderPath + @"\AxSecurityDuty";
            string privFolderPath = rootFolderPath + @"\AxSecurityPrivilege";

            Directory.CreateDirectory(rootFolderPath);
            Directory.CreateDirectory(roleFolderPath);
            Directory.CreateDirectory(dutyFolderPath);
            Directory.CreateDirectory(privFolderPath);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(inputFilePath);

            string xml = xDoc.OuterXml;
            foreach(var securityLayer in securityLayerList)
            {
                xml = ReplaceSecurityLayerParameters(xml, securityLayer);
            }

            XmlDocument renamedXDoc = new XmlDocument();
            TextReader tr = new StringReader(xml);
            renamedXDoc.Load(tr);

            XmlNodeList roles = renamedXDoc.GetElementsByTagName("AxSecurityRole");
            foreach (XmlNode role in roles)
            {
                string roleName = role["Name"]?.InnerText;
                string fileName = roleFolderPath + @"\" + roleName + @".xml";
                File.WriteAllText(fileName, role.OuterXml);
            }

            XmlNodeList duties = renamedXDoc.GetElementsByTagName("AxSecurityDuty");
            foreach (XmlNode duty in duties)
            {
                string dutyName = duty["Name"]?.InnerText;
                string fileName = dutyFolderPath + @"\" + dutyName + @".xml";
                File.WriteAllText(fileName, duty.OuterXml);
            }

            XmlNodeList privileges = renamedXDoc.GetElementsByTagName("AxSecurityPrivilege");
            foreach (XmlNode privilege in privileges)
            {
                string privilegeName = privilege["Name"]?.InnerText;
                string fileName = privFolderPath + @"\" + privilegeName + @".xml";
                File.WriteAllText(fileName, privilege.OuterXml);
            }
        }

        private string ReplaceSecurityLayerParameters(string xml, SecurityLayer securityLayer)
        {
            if(securityLayer.OldName != securityLayer.Name)
            {
                xml = xml.Replace("<Name>"+securityLayer.OldName+"</Name>", "<Name>" + securityLayer.Name + "</Name>");
            }
            if(securityLayer.OldLabel != securityLayer.Label)
            {
                xml = xml.Replace("<Label>" + securityLayer.OldLabel + "</Label>", "<Label>" + securityLayer.Label + "</Label>");
            }
            if(securityLayer.OldDescription != securityLayer.Description)
            {
                xml = xml.Replace("<Description>" + securityLayer.OldDescription + "</Description>", "<Description>" + securityLayer.Description + "</Description>");
            }
            return xml;
        }

        private List<SecurityLayer> ParseInputXML(string inputFilePath)
        {
            List<SecurityLayer> securityLayerList = new List<SecurityLayer>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(inputFilePath);
            XmlNodeList roles = xDoc.GetElementsByTagName("AxSecurityRole");
            foreach (XmlNode role in roles)
            {
                string roleName = role["Name"]?.InnerText;
                if (roleName != null)
                {
                    SecurityLayer sl = new SecurityLayer
                    {
                        OldName = roleName,
                        Name = roleName,
                        OldLabel = role["Label"]?.InnerText ?? "",
                        Label = role["Label"]?.InnerText ?? "",
                        OldDescription = role["Description"]?.InnerText ?? "",
                        Description = role["Description"]?.InnerText ?? "",
                        Type = "Role"
                    };
                    securityLayerList.Add(sl);
                }
            }

            XmlNodeList duties = xDoc.GetElementsByTagName("AxSecurityDuty");
            foreach (XmlNode duty in duties)
            {
                string dutyName = duty["Name"]?.InnerText;
                if(dutyName != null)
                {
                    SecurityLayer sl = new SecurityLayer
                    {
                        OldName = dutyName,
                        Name = dutyName,
                        OldLabel = duty["Label"]?.InnerText ?? "",
                        Label = duty["Label"]?.InnerText ?? "",
                        OldDescription = duty["Description"]?.InnerText ?? "",
                        Description = duty["Description"]?.InnerText ?? "",
                        Type = "Duty"
                    };
                    securityLayerList.Add(sl);
                }
            }

            XmlNodeList privileges = xDoc.GetElementsByTagName("AxSecurityPrivilege");
            foreach (XmlNode privilege in privileges)
            {
                string privilegeName = privilege["Name"]?.InnerText;
                if(privilegeName != null)
                {
                    SecurityLayer sl = new SecurityLayer
                    {
                        OldName = privilegeName,
                        Name = privilegeName,
                        OldLabel = privilege["Label"]?.InnerText ?? "",
                        Label = privilege["Label"]?.InnerText ?? "",
                        OldDescription = privilege["Description"]?.InnerText ?? "",
                        Description = privilege["Description"]?.InnerText ?? "",
                        Type = "Privilege"
                    };
                    securityLayerList.Add(sl);
                }
            }
            return securityLayerList;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            string inputFilePath = tb_inputFile.Text;
            string outputFolderPath = tb_outputFolder.Text;

            if (!File.Exists(inputFilePath))
            {
                MessageBox.Show("Input file does not exist", "Error Processing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!Directory.Exists(outputFolderPath))
            {
                MessageBox.Show("Output folder path does not exist", "Error Processing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    ExportSecurityXMLFiles(inputFilePath, outputFolderPath);
                    MessageBox.Show("Processing of security has completed successfully!", "Security File Processed Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Processing Security File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            string inputFilePath = tb_inputFile.Text;

            if (!File.Exists(inputFilePath))
            {
                MessageBox.Show("Input file does not exist", "Error Processing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    BindingListView<SecurityLayer> blv = new BindingListView<SecurityLayer>(ParseInputXML(inputFilePath));
                    dgvSecurityLayers.DataSource = blv;
                    dgvSecurityLayers.Columns["OldName"].Visible = false;
                    dgvSecurityLayers.Columns["OldLabel"].Visible = false;
                    dgvSecurityLayers.Columns["OldDescription"].Visible = false;
                    dgvSecurityLayers.Columns["Type"].ReadOnly = true;
                    dgvSecurityLayers.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    if(tb_outputFolder.Text != "")
                        btn_Export.Enabled = true;

                    dgvSecurityLayers.Columns["Name"].SortMode = DataGridViewColumnSortMode.Automatic;
                    dgvSecurityLayers.Columns["Label"].SortMode = DataGridViewColumnSortMode.Automatic;
                    dgvSecurityLayers.Columns["Type"].SortMode = DataGridViewColumnSortMode.Automatic;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Processing Security File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tbInputFile_TextChanged(object sender, EventArgs e)
        {
            btn_Export.Enabled = false;
            if (tb_inputFile.Text == "")
                btn_Process.Enabled = false;
            else
                btn_Process.Enabled = true;

        }

        private void tbOutputFolder_TextChanged(object sender, EventArgs e)
        {
            if (tb_outputFolder.Text == "" || tb_inputFile.Text == "")
                btn_Export.Enabled = false;
            if (tb_outputFolder.Text != "" && dgvSecurityLayers.Rows.Count > 0)
                btn_Export.Enabled = true;
        }
    }
}
