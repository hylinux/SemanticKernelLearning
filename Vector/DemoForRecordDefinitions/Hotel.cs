internal class Hotel
{

    public ulong HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

    public string[]? Tags { get; set; }
    


}