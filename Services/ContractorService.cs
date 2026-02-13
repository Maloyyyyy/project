using JonyBalls3.Models;
using System.Linq;

namespace JonyBalls3.Services
{
    public class ContractorService
    {
        private readonly SimpleContractor[] _contractors;
        
        public ContractorService()
        {
            _contractors = SimpleContractor.GetSampleData();
        }
        
        public SimpleContractor[] GetAllContractors()
        {
            return _contractors.OrderByDescending(c => c.Rating).ToArray();
        }
        
        public SimpleContractor? GetContractorById(int id)
        {
            return _contractors.FirstOrDefault(c => c.Id == id);
        }
        
        public SimpleContractor[] SearchContractors(string specialization, decimal? maxPrice)
        {
            var query = _contractors.AsEnumerable();
            
            if (!string.IsNullOrEmpty(specialization))
            {
                query = query.Where(c => c.Specialization.Contains(specialization));
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.PricePerHour <= maxPrice.Value);
            }
            
            return query.ToArray();
        }
    }
}
