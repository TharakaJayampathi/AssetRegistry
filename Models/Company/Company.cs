using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetRegistry.Models.Company
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
