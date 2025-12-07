using Apetrei_Alexandru_Lab4.Data;
using Apetrei_Alexandru_Lab4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Apetrei_Alexandru_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor pentru injectarea DbContext
        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Price Prediction - afișează formularul
        [HttpGet]
        public IActionResult Price()
        {
            return View();
        }

        // POST: Price Prediction - procesează formularul și salvează predicția
        [HttpPost]
        public async Task<IActionResult> Price(PricePredictionModel.ModelInput input)
        {
            // Încarcă modelul ML.NET
            MLContext mlContext = new MLContext();
            var modelPath = "PricePredictionModel.mlnet";
            ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);

            // Creează engine pentru predicție
            var predEngine = mlContext.Model.CreatePredictionEngine<PricePredictionModel.ModelInput, PricePredictionModel.ModelOutput>(mlModel);
            PricePredictionModel.ModelOutput result = predEngine.Predict(input);

            // Trimite rezultatul către view
            ViewBag.Price = result.Score;

            // Creează obiectul pentru istoric
            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripTimeInSecs = input.Trip_time_in_secs,
                TripDistance = input.Trip_distance,
                PaymentType = input.Payment_type,
                PredictedPrice = result.Score,
                CreatedAt = DateTime.Now
            };

            // Salvează în baza de date
            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();

            return View(input);
        }

        // GET: Istoricul predicțiilor
        [HttpGet]
        public async Task<IActionResult> History(
    string? paymentType,
    float? minPrice,
    float? maxPrice,
    DateTime? startDate,
    DateTime? endDate,
    string? sortOrder)
        {
            var query = _context.PredictionHistories.AsQueryable();

            // Filtrare paymentType
            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType == paymentType);
            }

            // Filtrare preț
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }

            // Filtrare interval date
            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= endDate.Value);
            }

            // Sortare
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // default
            };

            // Păstrarea valorilor în ViewBag
            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentSortOrder = sortOrder;

            var result = await query.ToListAsync();
            return View(result);
        }


        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // 1. Numărul total de predicții
            var totalPredictions = await _context.PredictionHistories.CountAsync();
            // 2. Preț mediu per tip de plată + număr de predicții per tip
            var paymentTypeStats = await _context.PredictionHistories
            .GroupBy(p => p.PaymentType)
            .Select(g => new PaymentTypeStat
            {
                PaymentType = g.Key,
                AveragePrice = g.Average(x => x.PredictedPrice),
                Count = g.Count()
            })
            .ToListAsync();
            // 3. Distribuția prețurilor pe intervale (buckets)
            // Definim intervalele: 0-10, 10-20, 20-30, 30-50, >50 (exemplu)
            var allPredictions = await _context.PredictionHistories
            .Select(p => p.PredictedPrice)
            .ToListAsync();
            var buckets = new List<PriceBucketStat>
            {
                new PriceBucketStat { Label = "0 - 10" },
                new PriceBucketStat { Label = "10 - 20" },
                new PriceBucketStat { Label = "20 - 30" },
                new PriceBucketStat { Label = "30 - 50" },
                new PriceBucketStat { Label = "> 50" }
            };
            foreach (var price in allPredictions)
            {
                if (price < 10)
                    buckets[0].Count++;
                else if (price < 20)
                    buckets[1].Count++;
                else if (price < 30)
                    buckets[2].Count++;
                else if (price < 50)
                    buckets[3].Count++;
                else
                    buckets[4].Count++;
            }
            // 4. Construim ViewModel-ul
            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets
            };
            return View(vm);

        }
    }
}
