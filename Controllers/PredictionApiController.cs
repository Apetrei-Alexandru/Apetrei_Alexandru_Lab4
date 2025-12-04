using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apetrei_Alexandru_Lab4.Data;   // <--- important!
using Apetrei_Alexandru_Lab4.Models;

namespace Apetrei_Alexandru_Lab4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionApiController(AppDbContext context)  // <--- folosim AppDbContext
        {
            _context = context;
        }

        // GET: api/predictionapi
        [HttpGet]
        public async Task<IActionResult> GetPredictions()
        {
            // Tabela ta este PredictionHistories
            var predictions = await _context.PredictionHistories.ToListAsync();
            return Ok(predictions);
        }

        // DELETE: api/predictionapi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrediction(int id)
        {
            var prediction = await _context.PredictionHistories.FindAsync(id);

            if (prediction == null)
            {
                return NotFound(new { Message = "Predictia nu a fost găsită." });
            }

            _context.PredictionHistories.Remove(prediction);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Predictia a fost ștearsă cu succes." });
        }
    }
}
