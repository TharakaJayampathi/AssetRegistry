namespace AssetRegistry.DTOs.Location
{
    public class LocationListDTO
    {
        public int Id { get; set; }
        public string LocationId { get; set; }
        public string LocationAddress { get; set; }
        public string CompanyName { get; set; }
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public bool IsActive { get; set; }
    }
}
