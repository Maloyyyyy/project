namespace JonyBalls3.Services
{
    public class CalculatorService
    {
        public class CalculationRequest
        {
            public decimal Area { get; set; }
            public string? RepairType { get; set; }
            public bool IncludeMaterials { get; set; }
            public bool IncludeWorks { get; set; }
        }
        
        public class CalculationResult
        {
            public decimal TotalCost { get; set; }
            public string Details { get; set; } = "";
            public string[] Recommendations { get; set; } = System.Array.Empty<string>();
        }
        
        public CalculationResult Calculate(CalculationRequest request)
        {
            decimal basePrice = 0;
            string details = "";
            
            switch (request.RepairType)
            {
                case "Косметический":
                    basePrice = 5000;
                    break;
                case "Капитальный":
                    basePrice = 10000;
                    break;
                case "Дизайнерский":
                    basePrice = 15000;
                    break;
                default:
                    basePrice = 5000;
                    break;
            }
            
            decimal workCost = basePrice * request.Area;
            decimal materialCost = request.IncludeMaterials ? workCost * 0.7m : 0;
            decimal total = workCost + materialCost;
            
            if (request.IncludeWorks)
            {
                total += total * 0.3m;
            }
            
            details = $"Площадь: {request.Area}м²\n" +
                     $"Тип ремонта: {request.RepairType ?? "Не указан"}\n" +
                     $"Стоимость работ: {workCost:N0} руб.\n" +
                     $"Стоимость материалов: {materialCost:N0} руб.\n" +
                     $"Итого: {total:N0} руб.";
            
            return new CalculationResult
            {
                TotalCost = total,
                Details = details,
                Recommendations = new[] 
                {
                    "Добавьте 10-15% на непредвиденные расходы",
                    "Получите несколько предложений от подрядчиков",
                    "Учитывайте сезонные скидки на материалы"
                }
            };
        }
    }
}
