namespace D365FOSecurityConverter.Models
{
    public class ParentToChildAssociation
    {
        public string ParentSystemName { get; set; }
        public LayerType ParentType { get; set; }
        public string ChildSystemName { get; set; }
        public LayerType ChildType { get; set; }
    }
}
