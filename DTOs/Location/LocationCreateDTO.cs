namespace AssetRegistry.DTOs.Location
{
    public class LocationCreateDTO
    {
        public string LocatonId { get; set; }
        public string LocationAddress { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
    }
}
