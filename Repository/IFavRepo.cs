using backend.Model;

namespace backend.Repository
{
    public interface IFavRepo
    {
        Task<List<FavouriteDTO>> GetUsersFavByUserId(string Uid, string category);
        Task<bool> AddQuoteToFavAsync(string Uid, QuoteDTO quote);
        Task<FavouriteDTO> GetFavQuoteById(string Uid, string Fid);
        Task<bool> DeleteQuoteById(string Uid, string Fid);
        Task<bool> UpdateQuoteTagsById(string Uid, string Fid, string tags);

    }
}
