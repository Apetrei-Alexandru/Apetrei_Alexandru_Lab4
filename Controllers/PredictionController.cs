using Apetrei_Alexandru_Lab4.Models;
using Apetrei_Alexandru_Lab4.Data;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(history);
        }
    }
}
