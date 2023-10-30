using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace webapp.Entities
{
  public class Site
  {
    [SimpleField(IsKey = true, IsFilterable = true)]
    public string id { get; set; } = String.Empty;

    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string siteId { get; set; } = String.Empty;

    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string address { get; set; } = String.Empty;

    public double? score { get; set; }

  }
}