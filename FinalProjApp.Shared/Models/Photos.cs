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
        /// Families album only signed-in parents and staff see.
        /// </summary>
        public bool IsPublic { get; set; }
    }
}
