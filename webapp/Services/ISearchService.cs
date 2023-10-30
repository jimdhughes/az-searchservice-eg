using webapp.Entities;

namespace webapp.Services
{
  public interface ISearchService
  {
    public IList<Site> SearchByAddress(string address);

    public IList<Site> SearchByPhone(string phone);

    public IList<Site> SearchBySiteId(string siteId);
  }
}