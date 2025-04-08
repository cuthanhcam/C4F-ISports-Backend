namespace api.Dtos.Booking
{
    public class BookingServiceDto
    {
        public int FieldServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}