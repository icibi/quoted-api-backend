using Amazon.DynamoDBv2;
using backend.Model;

namespace backend.Repository
{
    public interface IQuoteRepo
    {
        Task<List<AllQuotes>> GetAllQuotesAsync();
        Task<bool> AddQuoteAsync(QuoteDTO quote);
        Task<QuoteDTO> GetQuoteById(string Qid);
        Task<bool> PatchQuoteAsync(string Qid, Dictionary<string, string> updatedQuote);
        Task<bool> UpdateQuoteAsync(string Qid, AllQuotes quoteUpdated);
        Task<bool> UpdateQuoteInUserFavAsync(QuoteDTO quoteUpdated);
        Task<bool> DeleteQuoteByIdAsync(string Qid);

    }

}
