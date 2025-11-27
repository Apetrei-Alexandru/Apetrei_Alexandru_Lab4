using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Apetrei_Alexandru_Lab4.TimePredictionModel;

namespace Apetrei_Alexandru_Lab4.Controllers
{
    public class TimePredictionController : Controller
    {
        public IActionResult Time(ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();

            var modelPath = "TimePredictionModel.mlnet";
            ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);

            // Create prediction engine using loaded model
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            // Predict trip time (in seconds)
            ModelOutput result = predEngine.Predict(input);

            // Send result to view
            ViewBag.Time = result.Score;

            return View(input);
        }
    }
}
