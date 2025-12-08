using System.Threading.Tasks;
using UniversityTuitionApi.Models;

namespace UniversityTuitionApi.Services
{
    public interface ITuitionService
    {
        Task<TuitionRecord?> GetTuitionAsync(string studentNo, string term);

        Task<TuitionRecord> AddOrUpdateTuitionAsync(
            string studentNo,
            string term,
            decimal tuitionTotal);

        Task<(bool Success, string Message, decimal? RemainingBalance)> ApplyPaymentAsync(
            string studentNo,
            string term,
            decimal amount);
    }
}
