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

        private void ExportSecurityToUI(string inputFilePath, string outputFilePath)
        {
            List<SecurityLayer> securityLayerList = ConvertGridToObjects();

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

            string CustomizationsforTypeListXML = "_x003C_CustomizationsForTypeList_x003E_k__BackingField";
            string CustomizedObjectListXML = "_x003C_CustomizedObjectList_x003E_k__BackingField";
            string DisabledObjectSetXML = "_x003C_DisabledObjectSet_x003E_k__BackingField";
            string NewObjectListXML = "_x003C_NewObjectList_x003E_k__BackingField";
            string securityManagementNamespace = "http://schemas.datacontract.org/2004/07/Microsoft.Dynamics.AX.Security.Management";
            string schemaNamespace = "http://www.w3.org/2001/XMLSchema-instance";  

            XmlDocument resultXml = new XmlDocument();
            XmlElement rootElement = resultXml.CreateElement("SecurityCustomizationData");
            rootElement.SetAttribute("xmlns", securityManagementNamespace);
            rootElement.SetAttribute("xmlns:i", schemaNamespace);

            XmlNode customizationsForTypeList = rootElement.AppendChild(resultXml.CreateElement(CustomizationsforTypeListXML));
            

            XmlNodeList customizationList = renamedXDoc.GetElementsByTagName("BaseRepositoryCustomizations");
            foreach(XmlNode customization in customizationList)
            {
                //Roles
                if (customization.Attributes["i:type"].Value.Contains("Role"))
                {
                    XmlElement baseRepositoryCustomizations_Role = resultXml.CreateElement("BaseRepositoryCustomizations");
                    baseRepositoryCustomizations_Role.SetAttribute("i:type", "RepositoryCustomizationsOfAxSecurityRoleNcCATIYq");
                    XmlNode baseRepositoryCustomizationsRoleNode = customizationsForTypeList.AppendChild(baseRepositoryCustomizations_Role);
                    XmlNode customizedObjectListRole = baseRepositoryCustomizationsRoleNode.AppendChild(resultXml.CreateElement(CustomizedObjectListXML));
                    XmlNodeList customizedRoles = customization.SelectNodes(@"/"+CustomizedObjectListXML+"/AxSecurityRole");

                    foreach (XmlNode customizedRole in customizedRoles)
                    {
                        string roleName = customizedRole["Name"]?.InnerText;
                        if (selectedRoles.Contains(roleName))
                            customizedObjectListRole.AppendChild(customizedRole);
                    }

                    XmlNode disabledObjectSetRole = baseRepositoryCustomizationsRoleNode.AppendChild(resultXml.CreateElement(DisabledObjectSetXML));

                    XmlNode newObjectListRole = baseRepositoryCustomizationsRoleNode.AppendChild(resultXml.CreateElement(NewObjectListXML));
                    XmlNodeList createdRoles = customization.SelectNodes(@"/"+NewObjectListXML+"/AxSecurityRole");
                    foreach(XmlNode createdRole in createdRoles)
                    {
                        string roleName = createdRole["Name"]?.InnerText;
                        if (selectedRoles.Contains(roleName))
                        {
                            newObjectListRole.AppendChild(createdRole);
                        }
                    }
                }
                //Duties
                else if (customization.Attributes["i:type"].Value.Contains("Duty"))
                {
                    XmlElement baseRepositoryCustomizations_Duty = resultXml.CreateElement("BaseRepositoryCustomizations");
                    baseRepositoryCustomizations_Duty.SetAttribute("i:type", "RepositoryCustomizationsOfAxSecurityDutyNcCATIYq");
                    XmlNode baseRepositoryCustomizationsDutyNode = customizationsForTypeList.AppendChild(baseRepositoryCustomizations_Duty);
                    XmlNode customizedObjectListDuty = baseRepositoryCustomizationsDutyNode.AppendChild(resultXml.CreateElement(CustomizedObjectListXML));
                    XmlNodeList customizedDuties = customization.SelectNodes(@"/" + CustomizedObjectListXML + "/AxSecurityDuty");

                    foreach(XmlNode customizedDuty in customizedDuties)
                    {
                        string dutyName = customizedDuty["Name"]?.InnerText;
                        if (selectedDuties.Contains(dutyName))
                            customizedObjectListDuty.AppendChild(customizedDuty);
                    }

                    XmlNode disabledObjectSetDuty = baseRepositoryCustomizationsDutyNode.AppendChild(resultXml.CreateElement(DisabledObjectSetXML));

                    XmlNode newObjectListDuty = baseRepositoryCustomizationsDutyNode.AppendChild(resultXml.CreateElement(NewObjectListXML));
                    XmlNodeList createdDuties = customization.SelectNodes(@"/" + NewObjectListXML + "/AxSecurityDuty");
                    foreach (XmlNode createdDuty in createdDuties)
                    {
                        string dutyName = createdDuty["Name"]?.InnerText;
                        if (selectedDuties.Contains(dutyName))
                        {
                            newObjectListDuty.AppendChild(createdDuty);
                        }
                    }
                }
                //Privileges
                else if (customization.Attributes["i:type"].Value.Contains("Privilege"))
                {
                    XmlNamespaceManager nsm = new XmlNamespaceManager(resultXml.NameTable);
                    nsm.AddNamespace("i", securityManagementNamespace);
                    
                    XmlElement baseRepositoryCustomizations_Privilege = resultXml.CreateElement("BaseRepositoryCustomizations");
                    baseRepositoryCustomizations_Privilege.SetAttribute("i:type", "RepositoryCustomizationsOfAxSecurityPrivilegeNcCATIYq");
                    XmlNode baseRepositoryCustomizationsPrivilegeNode = customizationsForTypeList.AppendChild(baseRepositoryCustomizations_Privilege);
                    XmlNode customizedObjectListPrivilege = baseRepositoryCustomizationsPrivilegeNode.AppendChild(resultXml.CreateElement(CustomizedObjectListXML));

                    XmlNodeList customizedPrivileges = customization.FirstChild.SelectNodes("AxSecurityPrivilege");
                    foreach (XmlNode customizedPrivilege in customizedPrivileges)
                    {
                        string privilegeName = customizedPrivilege["Name"]?.InnerText;
                        if (selectedDuties.Contains(privilegeName))
                            customizedObjectListPrivilege.AppendChild(customizedPrivilege);
                    }

                    XmlNode disabledObjectSetPriv = baseRepositoryCustomizationsPrivilegeNode.AppendChild(resultXml.CreateElement(DisabledObjectSetXML));

                    XmlNode newObjectListDuty = baseRepositoryCustomizationsPrivilegeNode.AppendChild(resultXml.CreateElement(NewObjectListXML));
                    XmlNodeList createdPrivileges = customization.SelectNodes(@"/" + NewObjectListXML + "/AxSecurityPrivilege");
                    foreach (XmlNode createdPrivilege in createdPrivileges)
                    {
                        string privilegeName = createdPrivilege["Name"]?.InnerText;
                        if (selectedPrivs.Contains(privilegeName))
                        {
                            newObjectListDuty.AppendChild(createdPrivilege);
                        }
                    }
                }
            }

            resultXml.AppendChild(rootElement);
            resultXml.Save(outputFilePath + @"\SecurityDatabaseCustomizations.xml");
        }

        private List<SecurityLayer> ConvertGridToObjects()
        {
            List<SecurityLayer> securityLayers = new List<SecurityLayer>();
            int count = dgvSecurityLayers.RowCount;
            for (int index = 0; index < count; index++)
            {
                DataGridViewRow row = dgvSecurityLayers.Rows[index];
                SecurityLayer sl = new SecurityLayer()
                {
                    Selected = (bool)row.Cells["Selected"].Value,
                    OldName = (string)row.Cells["OldName"].Value,
                    Name = (string)row.Cells["Name"].Value,
                    OldLabel = (string)row.Cells["OldLabel"].Value,
                    Label = (string)row.Cells["Label"].Value,
                    OldDescription = (string)row.Cells["OldDescription"].Value,
                    Description = (string)row.Cells["Description"].Value,
                    Type = (string)row.Cells["Type"].Value
                };
                securityLayers.Add(sl);
            }
            return securityLayers;
        }

        private void ExportSecurityToCode(string inputFilePath, string outputFolderPath)
        {
            List<SecurityLayer> securityLayerList = ConvertGridToObjects();
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
                ExportSecurityToUI(tb_inputFile.Text, tb_outputFolder.Text);
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
