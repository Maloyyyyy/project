using Microsoft.AspNetCore.Mvc;
using JonyBalls3.Services;

namespace JonyBalls3.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly CalculatorService _calculatorService;

        public CalculatorController(CalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate(CalculatorService.CalculationRequest request)
        {
            if (ModelState.IsValid)
            {
                var result = _calculatorService.Calculate(request);
                ViewBag.Result = result;
                return View("Result", result);
            }
            return View("Index");
        }

        public IActionResult Result()
        {
            return View();
        }
    }
}
