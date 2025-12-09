namespace AssetRegistry.DTOs.Division
{
    public class DivisionUpdateDTO
    {
        public int Id { get; set; }
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
    }
}
