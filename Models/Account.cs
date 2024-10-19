using System.ComponentModel.DataAnnotations;
using static PFMSApi.Models.BankTypes;

namespace PFMSApi.Models
{
    public class Account
    {
        [Key]
        public int Number { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BankBalance { get; set; }
        public BankType BankType { get; set; }
        public decimal MonthSpending { get; set; }
        public string ChangePercentage { get; set; } = string.Empty;
    }
}
