using System.ComponentModel.DataAnnotations;

namespace AurHER.DTOs.Admin
{
    public class UpdateCollectionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Collection name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
    }
}