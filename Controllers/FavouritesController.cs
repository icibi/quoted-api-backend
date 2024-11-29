using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouritesController : Controller
    {
        private readonly FavouriteService _favouriteService;
        private readonly QuoteService _quoteService;

        public FavouritesController(FavouriteService favouriteService, QuoteService quoteService)
        {
            _favouriteService = favouriteService;
            _quoteService = quoteService;
        }


        //get all favourites by user id
        [HttpGet]
        public async Task<IActionResult> GetUsersFav([FromQuery] string Uid, string? tag = null)
        {
            var favs = await _favouriteService.GetUsersFavByUserId(Uid, tag);

            if (favs == null)
            {
                return NotFound(new { message = $"User with user ID: {Uid} not found." });
            }

            if (favs.Count == 0)
            {
                return Ok(new { message = $"User with userId {Uid} has no favorite quotes.", favourites = new List<FavouriteDTO>() });
            }

            return Ok(favs);
        }

        //get a quote in user's favourites list by fid
        [HttpGet("quote/{Fid}")]
        public async Task<IActionResult> GetUsersFavQuoteById([FromQuery]string Uid, [FromRoute] string Fid)
        {
            if (Fid == null || Uid == null)
            {
                return BadRequest("Quote Id is required.");
            }

            FavouriteDTO quote = await _favouriteService.GetFavQuoteById(Uid, Fid);

            if (quote != null)
            {
                return Ok(new { message = "Quote found.", Quote = quote });
            }

            return BadRequest(new { message = "Failed to get quote." });
        }

        //add quote to user's favourites
        [HttpPost("add-new")]
        public async Task<IActionResult> AddQuoteToFav([FromQuery] string Uid, [FromBody] QuoteIdDTO qidDTO)
        {
            if (qidDTO.Qid == null)
            {
                return BadRequest("Quote Id is required.");
            }

            var quote = await _quoteService.GetQuoteById(qidDTO.Qid);

            if (quote == null)
            {
                return NotFound("Quote not found.");
            }

            var added = await _favouriteService.AddQuoteToFavAsync(Uid, quote);

            if (added)
            {
                return Ok(new { message = "Quote added to favorites." });
            }

            return BadRequest("Failed to add quote to favorites.");
        }

        //delete a quote from the user's favourites list
        [HttpDelete("delete/{Fid}")]
        public async Task<IActionResult> DeleteQuoteFromFav([FromQuery] string Uid, [FromRoute] string Fid)
        {
            if(string.IsNullOrEmpty(Uid) || string.IsNullOrEmpty(Fid))
            {
                return BadRequest("Request Invalid.");
            }

            var result = await _favouriteService.DeleteQuoteById(Uid, Fid);

            if(!result)
            {
                return NotFound("Quote not found in favourites list.");
            }

            return Ok(new {message = "Quote removed from favourites list."});
        }

        //update tags
        [HttpPatch("patch-tags/{Fid}")]
        public async Task<IActionResult> PatchQuoteTagsFromFav([FromQuery] string Uid, [FromRoute] string Fid, string tags)
        {
            if (string.IsNullOrEmpty(Uid) || string.IsNullOrEmpty(Fid))
            {
                return BadRequest("Invalid");
            }

            var r = await _favouriteService.UpdateQuoteTagsById(Uid, Fid, tags);

            if (!r)
            {
                return NotFound("Tags not found.");
            }

            return Ok(new { message = "Tags updated ." });

        }
    }
    
}
