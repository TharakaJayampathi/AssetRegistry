using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetRegistry.Models.Claims
{
    public class ClaimType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Resource { get; set; }
        public string Claim { get; set; }
    }
}
