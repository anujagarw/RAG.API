
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace RAG.API.Services
{
    /// <summary>
    /// Provides services for searching relevant content chunks
    /// from an Azure AI Search index.
    /// </summary>
    public class AzureSearchService
    {
        private readonly SearchClient _searchClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSearchService"/> class.
        /// Configures the Azure AI Search client using settings from application configuration.
        /// </summary>
        /// <param name="config">
        /// Application configuration containing Azure Search endpoint, index name, and API key.
        /// </param>
        public AzureSearchService(IConfiguration config)
        {
            var endpoint = new Uri(config["AzureSearch:Endpoint"]);
            var indexName = config["AzureSearch:IndexName"];
            var apiKey = config["AzureSearch:ApiKey"];

            _searchClient = new SearchClient(
                endpoint,
                indexName,
                new AzureKeyCredential(apiKey));
        }

        /// <summary>
        /// Searches the Azure AI Search index for content relevant to the specified query
        /// and returns the matching content chunks.
        /// </summary>
        /// <param name="query">The search query used to retrieve relevant content chunks.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a list of matching content chunks.
        /// </returns>
        public async Task<List<string>> SearchRelevantChunksAsync(string query)
        {
            var options = new SearchOptions()
            {
                IncludeTotalCount = true
            };

            // Add the content field to Select to return only relevant data.
            options.Select.Add("content");

            // Execute the search and retrieve deserialized documents.
            var response = await _searchClient
                .SearchAsync<SearchDocument>(query, options)
                .ConfigureAwait(false);

            var results = response.Value.GetResults();

            return results
                .Select(r => r.Document.TryGetValue("content", out var content)
                    ? content?.ToString()
                    : null)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList()!;
        }
    }
}
