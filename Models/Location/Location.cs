using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetRegistry.Models.Location
{
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public bool IsActive { get; set; }
    }
}
