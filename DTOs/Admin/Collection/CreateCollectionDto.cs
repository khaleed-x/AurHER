using System.ComponentModel.DataAnnotations;

namespace AurHER.DTOs.Admin
{
    public class CreateCollectionDto
    {
        [Required(ErrorMessage = "Collection name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
    }
}