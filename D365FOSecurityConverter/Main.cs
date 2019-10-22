using D365FOSecurityConverter.Models;
using Equin.ApplicationFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

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

        private void ExportToUI(string inputFilePath, string outputFilePath)
        {
            List<SecurityLayer> securityLayerList = (List<SecurityLayer>)dgvSecurityLayers.DataSource;

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(inputFilePath);

            string xml = xDoc.OuterXml;
            foreach (var securityLayer in securityLayerList.Where(sl => sl.Selected == true))
            {
                xml = ReplaceSecurityLayerParameters(xml, securityLayer);
            }

            XmlDocument renamedXDoc = new XmlDocument();
            TextReader tr = new StringReader(xml);
            renamedXDoc.Load(tr);

            IEnumerable<string> selectedRoles = securityLayerList.Where(sl => sl.Selected == true && sl.Type == "Role").Select(x => x.Name);
            IEnumerable<string> selectedDuties = securityLayerList.Where(sl => sl.Selected == true && sl.Type == "Duty").Select(x => x.Name);
            IEnumerable<string> selectedPrivs = securityLayerList.Where(sl => sl.Selected == true && sl.Type == "Privilege").Select(x => x.Name);

            XmlDocument resultXml = new XmlDocument();
            resultXml.DocumentElement.SetAttribute("xmlns", "http://schemas.datacontract.org/2004/07/Microsoft.Dynamics.AX.Security.Management");
            resultXml.DocumentElement.SetAttribute("xmlns:i", "http://www.w3.org/2001/XMLSchema-instance");

            XmlNodeList customizationList = renamedXDoc.GetElementsByTagName("BaseRepositoryCustomizations");
            foreach(XmlNode customization in customizationList)
            {
                if (customization.Attributes["type"].Value.Contains("Role"))
                {
                    //customization.ChildNodes.
                }
            }
            XmlNodeList CustomizedObjectList = renamedXDoc.GetElementsByTagName("_x003C_CustomizedObjectList_x003E_k__BackingField");
           // XmlNodeList a = CustomizedObjectList.
            XmlNodeList DisabledObjectList = renamedXDoc.GetElementsByTagName("_x003C_DisabledObjectSet_x003E_k__BackingField");
            XmlNodeList NewObjectList = renamedXDoc.GetElementsByTagName("_x003C_NewObjectList_x003E_k__BackingField");

            XmlNodeList roles = renamedXDoc.GetElementsByTagName("AxSecurityRole");
            foreach (XmlNode role in roles)
            {
                string roleName = role["Name"]?.InnerText;
                if (selectedRoles.Contains(roleName))
                {
                }
            }

            XmlNodeList duties = renamedXDoc.GetElementsByTagName("AxSecurityDuty");
            foreach(XmlNode duty in duties)
            {
                string dutyName = duty["Name"]?.InnerText;
                if (selectedDuties.Contains(dutyName))
                {

                }
            }

            XmlNodeList privileges = renamedXDoc.GetElementsByTagName("AxSecurityPrivilege");
            foreach(XmlNode priv in privileges)
            {
                string privName = priv["Name"]?.InnerText;
                if (selectedPrivs.Contains(privName))
                {

                }
            }
            
        }

        private void ExportSecurityToCode(string inputFilePath, string outputFolderPath)
        {
            List<SecurityLayer> securityLayerList = (List<SecurityLayer>)dgvSecurityLayers.DataSource;
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

            IEnumerable<string> selectedRoles = securityLayerList.Where(sl => sl.Selected == true && sl.Type == "Role").Select(x => x.Name);
            IEnumerable<string> selectedDuties = securityLayerList.Where(sl => sl.Selected == true && sl.Type == "Duty").Select(x => x.Name);
            IEnumerable<string> selectedPrivs = securityLayerList.Where(sl => sl.Selected == true && sl.Type == "Privilege").Select(x => x.Name);

            XmlNodeList roles = renamedXDoc.GetElementsByTagName("AxSecurityRole");
            foreach (XmlNode role in roles)
            {
                string roleName = role["Name"]?.InnerText;
                if (selectedRoles.Contains(roleName))
                {
                    string fileName = roleFolderPath + @"\" + roleName + @".xml";
                    File.WriteAllText(fileName, role.OuterXml);
                }
            }

            XmlNodeList duties = renamedXDoc.GetElementsByTagName("AxSecurityDuty");
            foreach (XmlNode duty in duties)
            {
                string dutyName = duty["Name"]?.InnerText;
                if (selectedDuties.Contains(dutyName))
                {
                    string fileName = dutyFolderPath + @"\" + dutyName + @".xml";
                    File.WriteAllText(fileName, duty.OuterXml);
                }
            }

            XmlNodeList privileges = renamedXDoc.GetElementsByTagName("AxSecurityPrivilege");
            foreach (XmlNode privilege in privileges)
            {
                string privilegeName = privilege["Name"]?.InnerText;
                if (selectedPrivs.Contains(privilegeName))
                {
                    string fileName = privFolderPath + @"\" + privilegeName + @".xml";
                    File.WriteAllText(fileName, privilege.OuterXml);
                }
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
                        Selected = false,
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
                        Selected = false,
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
                        Selected = false,
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
                    {
                        btn_ExportToCode.Enabled = true;
                        btn_ExportToUI.Enabled = true;
                    }
                        

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
            btn_ExportToCode.Enabled = false;
            if (tb_inputFile.Text == "")
                btn_Process.Enabled = false;
            else
                btn_Process.Enabled = true;

        }

        private void tbOutputFolder_TextChanged(object sender, EventArgs e)
        {
            if (tb_outputFolder.Text == "" || tb_inputFile.Text == "")
                btn_ExportToCode.Enabled = false;
            if (tb_outputFolder.Text != "" && dgvSecurityLayers.Rows.Count > 0)
                btn_ExportToCode.Enabled = true;
        }

        private void btnExportToCode_Click(object sender, EventArgs e)
        {
            FilePathCheck();
            
            try
            {
                ExportSecurityToCode(tb_inputFile.Text, tb_outputFolder.Text);
                MessageBox.Show("Processing of security has completed successfully!", "Security File Processed Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Processing Security File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportToUI_Click(object sender, EventArgs e)
        {
            FilePathCheck();
            try
            {

                MessageBox.Show("Processing of security has completed successfully!", "Security File Processed Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Processing Security File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool FilePathCheck()
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
            return true;
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {

        }

        private void btnUncheckAll_Click(object sender, EventArgs e)
        {

        }

        private void dgvSecurityLayers_OnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == dgvSecurityLayers.Columns["Selected"].Index && e.RowIndex != -1)
            {
                DataGridViewRow row = dgvSecurityLayers.Rows[e.RowIndex];
                bool selected = (bool)row.Cells["Selected"].Value;
                string name = (string)row.Cells["Name"].Value;
                string type = (string)row.Cells["Type"].Value;

                //TODO : NEED TO FIND CASCADING SECURITY ELEMENTS
            }
        }

        private void dgvSecurityLayers_OnCellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == dgvSecurityLayers.Columns["Selected"].Index && e.RowIndex != -1)
            {
                dgvSecurityLayers.EndEdit();
            }
        }
    }
}
