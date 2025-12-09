namespace AssetRegistry.DTOs.Location
{
    public class LocationUpdateDTO
    {
        public int Id { get; set; }
        public string LocatonId { get; set; }
        public string LocationAddress { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public bool IsActive { get; set; }
    }
}
