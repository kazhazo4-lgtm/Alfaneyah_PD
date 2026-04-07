namespace ProjectsDashboards.Models
{
    public class DashboardViewModel
    {
        public List<Project>? AllProjects { get; set; }
        public Project? SelectedProject { get; set; }

        // Summary statistics
        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int InProgressProjects { get; set; }
        public int NotStartedProjects { get; set; }

        // Financial statistics
        public decimal TotalContractValue { get; set; }
        public decimal TotalVOs { get; set; }
        public decimal TotalClaims { get; set; }
        public decimal TotalRevisedValue { get; set; }

        // Chart data
        public List<ChartData>? ProjectStatusData { get; set; }
        public List<ChartData>? TopProjectsByValue { get; set; }
        public List<MonthlyProgressData>? MonthlyProgress { get; set; }
    }

    public class ChartData
    {
        public string? Label { get; set; }
        public decimal Value { get; set; }
        public string? Color { get; set; }
    }

    public class MonthlyProgressData
    {
        public string? Month { get; set; }
        public string? MonthKey { get; set; }
        public decimal Claims { get; set; }
        public decimal VOs { get; set; }
    }
}
