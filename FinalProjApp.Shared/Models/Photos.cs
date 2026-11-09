namespace FinalProjApp.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// True for the public Showcase album guests see; false for the private
        /// Families album, where parents only see photos of their own kids.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>The kids in this photo. Only filled in for staff; parents never get it.</summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<int> ChildIds { get; set; } = new();
    }

    /// <summary>Links a photo to a kid in it. A photo can have several kids.</summary>
    public class PhotoChild
    {
        public int PhotoId { get; set; }
        public int ChildId { get; set; }
    }
}
