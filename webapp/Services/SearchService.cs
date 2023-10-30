using Azure.Search.Documents;
using webapp.Entities;

namespace webapp.Services
{
  public class SearchService : ISearchService
  {
    private readonly SearchClient searchClient;
    public SearchService(SearchClient searchClient)
    {
      this.searchClient = searchClient;
    }

    public IList<Site> SearchByAddress(string address)
    {
      address = FuzzifySearchString(address);
      Console.WriteLine($"Querying for address {address}");
      IList<Site> sites = new List<Site>();
      SearchOptions options = new SearchOptions();
      options.SearchFields.Add("address");
      options.Size = 5;
      var results = this.searchClient.Search<Site>(address, options);
      var data = results.Value.GetResults();
      foreach (var d in data)
      {
        var s = d.Document;
        s.score = d.Score;
        sites.Add(s);

      }
      return sites;
    }

    public IList<Site> SearchByPhone(string phone)
    {
      throw new NotImplementedException();
    }

    public IList<Site> SearchBySiteId(string siteId)
    {
      Console.WriteLine($"Querying for address {siteId}");
      IList<Site> sites = new List<Site>();
      SearchOptions options = new SearchOptions();
      options.SearchFields.Add("siteId");
      options.Size = 5;
      var results = this.searchClient.Search<Site>(siteId, options);
      var data = results.Value.GetResults();
      foreach (var d in data)
      {
        sites.Add(d.Document);
      }
      return sites;
    }

    private string FuzzifySearchString(string search) =>
      search.Trim().Replace(" ", "~ ") + "~";
  }
}