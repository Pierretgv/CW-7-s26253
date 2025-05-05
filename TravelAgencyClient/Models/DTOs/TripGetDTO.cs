﻿namespace TravelAgencyAPI.Models.DTOs

public class TripGetDTO
{
    public int IdTrip { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<string> Countries { get; set; } = new();
    public int? RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}