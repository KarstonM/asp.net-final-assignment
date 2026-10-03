namespace asp.net_final_assignment.ViewModels
{
    public class ReportsVM
    {
        public string MostRentedModel { get; set; }
        public double MostRentedModelPercentage { get; set; }

        public int AverageRentalDays { get; set; }

        public string PeakRentalMonth { get; set; }
        public double PeakRentalMonthPercentage { get; set; }

        public string MostActiveRenter { get; set; }
        public int ActiveReservationsCount { get; set; }

        public double ActiveCustomerRate { get; set; }
        public double InactiveCustomerRate { get; set; }

        // Filters
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
