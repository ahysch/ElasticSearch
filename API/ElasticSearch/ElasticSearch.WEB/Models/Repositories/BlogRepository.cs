using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System.Runtime.CompilerServices;

namespace ElasticSearch.WEB.Models.Repositories
{
    public class BlogRepository
    {
        private readonly ElasticsearchClient _client;
        private const string indexName = "blog";

        public BlogRepository(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<Blog?> SaveAsync(Blog newBlog)
        {
            newBlog.Created = DateTime.Now;

            var response = await _client.IndexAsync(newBlog, x => x.Index(indexName).Id(Guid.NewGuid().ToString()));

            if (!response.IsValidResponse)
            {
                return null;
            }

            newBlog.Id = response.Id;

            return newBlog;
        }
        public async Task<List<Blog>> SearchAsync(string searchText)
        {
            /*
             Herhangi bir arama yapmadan veri gelmesi için bu metot aşağıdaki şekilde geliştirildi.Yani içerik gelecek o içerik içerisinden arayacağız.

            //should içerisine 3 tane değer koyalım (term1,term2,term3) Should dedikten sonra kriterimizi belirleyip . ile diğer kritere geçersek and olur ama , diyerek tekrar => ile gidersek or olur.

            var result = await _client.SearchAsync<Blog>(i => i.Index(indexName)
                                            .Size(1000)
                                            .Query(q => q
                                            .Bool(b => b
                                            .Should(s => s
                                                .Match(m => m
                                                    .Field(f => f.Content)
                                                        .Query(searchText)), // buradaki virgülden bahsediyorum.
                                                    s => s.MatchBoolPrefix(p => p //buradaki => 'dan bahsediyorum  
                                                    .Field(f => f.Title)
                                                        .Query(searchText))))));
            */

            List<Action<QueryDescriptor<Blog>>> queryList = new();

            Action<QueryDescriptor<Blog>> matchall = (query) => query.MatchAll();

            Action<QueryDescriptor<Blog>> matchContent = (query) => query.MatchPhrase(m => m.Field(f => f.Content).Query(searchText)); // eğer aradığımız 3 kelimelik öbeğin tamamı olsun istiyorsak matchphrase kullancaz.

            Action<QueryDescriptor<Blog>> titleMatchBoolPrefix = (query) => query.MatchBoolPrefix(p => p.Field(f => f.Title).Query(searchText));

            Action<QueryDescriptor<Blog>> tagTerm = (query) => query.Term(t => t.Field(f => f.Tags).Value(searchText));

            if (string.IsNullOrEmpty(searchText))
            {
                queryList.Add(matchall);
            }
            else
            {
                queryList.Add(matchContent);
                queryList.Add(titleMatchBoolPrefix);
                queryList.Add(tagTerm);
            }

            var result = await _client.SearchAsync<Blog>(i => i.Index(indexName)
                                            .Size(1000)
                                            .Query(q => q
                                            .Bool(b => b
                                            .Should(queryList.ToArray()))));

			foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToList();
        }
    }
}
