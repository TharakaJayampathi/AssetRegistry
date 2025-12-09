namespace AssetRegistry.DTOs.Company
{
    public class CompanyUpdateDTO
    {
        public int Id { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
