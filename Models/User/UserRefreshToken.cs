using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetRegistry.Models.User
{
    public class UserRefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
        public long ExpireOn { get; set; }
        public ApplicationUser User { get; set; }
    }
}
