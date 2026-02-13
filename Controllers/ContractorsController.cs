using Microsoft.AspNetCore.Mvc;
using JonyBalls3.Services;

namespace JonyBalls3.Controllers
{
    public class ContractorsController : Controller
    {
        private readonly ContractorService _contractorService;

        public ContractorsController(ContractorService contractorService)
        {
            _contractorService = contractorService;
        }

        public IActionResult Index(string? specialization = null, decimal? maxPrice = null)
        {
            var contractors = _contractorService.SearchContractors(specialization ?? "", maxPrice);
            ViewBag.Specialization = specialization ?? "";
            ViewBag.MaxPrice = maxPrice;
            return View(contractors);
        }

        public IActionResult Details(int id)
        {
            var contractor = _contractorService.GetContractorById(id);
            if (contractor == null)
            {
                return NotFound();
            }
            return View(contractor);
        }
    }
}
