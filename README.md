# Dynamics 365 for Finance and Operations Security Converter

This project is designed to convert the XML output a user gets from exporting security from the user interface in D365FO to the XML needed to either: 
- Create security elements in code via the AOT
- Generate security XML to import via D365FO user interface into another D365FO enviornment

You have the ability to select which security layers you would like to export and to be able to rename the Name, Label, and Description properties of the security layers.

## Input file
PathToFile/SecurityDatabaseCustomizations.xml

## Output Folder
UI Output:
Will create an XML file title SecurityDatabaseCustomizations.xml

Code Output:
Will create a folder with the following structure:
- D365FOCustomizedSecurity
  - SecurityRoles
  - SecurityDuties
  - SecurityPrivileges

## License
<a href="http://opensource.org/licenses/MIT">MIT-licensed</a>.
