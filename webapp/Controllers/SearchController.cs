using Microsoft.AspNetCore.Mvc;
using webapp.Entities;
using webapp.Services;

namespace webapp.Controllers
{

  [ApiController]
  [Route("[controller]")]
  public class SearchController : ControllerBase
  {
    private readonly ISearchService searchService;
    public SearchController(ISearchService searchService)
    {
      this.searchService = searchService;
    }

    [HttpGet("byAddress/{address}")]
    public IList<Site> GetSitesByAddress(string address)
    {
      return searchService.SearchByAddress(address);
    }

    [HttpGet("bySiteId/{siteId}")]
    public IList<Site> GetSitesBySiteId(string siteId)
    {
      return searchService.SearchBySiteId(siteId);
    }
  }
}