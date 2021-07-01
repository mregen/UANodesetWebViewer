
namespace UANodesetWebViewer.Models
{
    public class UACLUploadModel
    {
        public UACLUploadModel()
        {
            Name = string.Empty;
            NodeSetXml = string.Empty;
            NodeSetJson = string.Empty;
            Cost = string.Empty;
            Owner = string.Empty;
            VersionInfo = string.Empty;
            Remarks = string.Empty;
        }

        public string Name { get; set; }

        public string NodeSetXml { get; set; }

        public string NodeSetJson { get; set; }

        public string Cost { get; set; }

        public string Owner { get; set; }

        public string VersionInfo { get; set; }

        public string Remarks { get; set; }
    }
}
