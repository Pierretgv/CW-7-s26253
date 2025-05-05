namespace TravelAgencyAPI.Models

public class Country
{
    public int IdCountry { get; set; }
    public string Name { get; set; }
    public List<Trip> Trips { get; set; }
}