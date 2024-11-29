using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using backend.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using backend.Services;
using System.Net;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        //private readonly IConfiguration _config;
        private readonly QuoteService _quoteService;


        public QuotesController(QuoteService quoteService)
        {
            //_config = config;
            _quoteService = quoteService;
        }

        //get all quotes
        [HttpGet("all")]
        public async Task<IActionResult> GetAllQuotes()
        {
            var quotes = await _quoteService.GetAllQuotesAsync();

            if (quotes == null)
            {
                return NotFound();
            }
            return Ok(quotes);
        }


        //get quote by id
        [HttpGet("{Qid}")]
        public async Task<IActionResult> GetQuoteById([FromRoute] string Qid)
        {
            var quote = await _quoteService.GetQuoteById(Qid);

            if (quote != null)
            {
                return Ok(quote);
            }
            return NotFound("Quote not found.");
        }

        //update part(s) of a quote
        [HttpPatch("{Qid}/patch")]
        public async Task<IActionResult> PatchQuote(string Qid, [FromBody] Dictionary<string, string> updatedQuote)
        {
            var result = await _quoteService.PatchQuoteAsync(Qid, updatedQuote);

            if (result)
            {
                return Ok("Quote updated successfully.");
            }
            
            return StatusCode(500, "Failed to update quote.");
        }

        //update an entire quote
        [HttpPut("{Qid}/update")]
        public async Task<IActionResult> UpdateQuote(string Qid, [FromBody] AllQuotes updatedQuote)
        {
            if(Qid != updatedQuote.Qid)
            {
                return BadRequest("ID mismatch");
            }

            var result = await _quoteService.UpdateQuoteAsync(Qid, updatedQuote);

            if (result)
            {
                return Ok("Quote updated successfully.");
            }

            return StatusCode(500, "Failed to update quote.");

        }

        //add a quote 
        [HttpPost("add")]
        public async Task<IActionResult> AddQuote([FromQuery]string Uid, [FromBody] AddQuoteDTO newQuote)
        {
            if(Uid == null || newQuote == null)
            {
                return BadRequest("Fields missing");
            } 

            var result = await _quoteService.AddQuoteAsync(Uid, newQuote);

            if(result)
            {
                return Ok("Quote added successfully.");
            }
            return StatusCode(500, "Failed to add quote.");
        }

        //delete a quote
        [HttpDelete("delete/{Qid}")]
        public async Task<IActionResult> DeleteQuote([FromRoute] string Qid)
        {
           Boolean r  = await _quoteService.DeleteQuoteByIdAsync(Qid);

            if (r)
            {
                return Ok(new { Message = "Quote has been deleted." });
            }

            return StatusCode(500, new { Message = "Quote doesn't exist." });
        }
    }
}
