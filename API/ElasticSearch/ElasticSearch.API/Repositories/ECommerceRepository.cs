using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.TermVectors;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearch.API.Models.ECommerceModel;
using System.Collections.Immutable;

namespace ElasticSearch.API.Repositories
{
    public class ECommerceRepository
    {
        private readonly ElasticsearchClient _client;

        public ECommerceRepository(ElasticsearchClient client)
        {
            _client = client;
        }

        private const string indexName = "kibana_sample_data_ecommerce";




        public async Task<ImmutableList<ECommerce>> TermQuery(string customerFirstName)
        {
            // 1.yol
            var result1 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName).Query(q => q.Term(t => t.Field("customer_first_name.keyword").Value(customerFirstName))));

            //2.yol
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName).Query(q => q.Term(t => t.CustomerFirstName.Suffix("keyword"), customerFirstName)));

            //3.yol

            var termQuery = new TermQuery("customer_first_name.keyword") { Value = customerFirstName, CaseInsensitive = true };

            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName).Query(termQuery));


            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }

        public async Task<ImmutableList<ECommerce>> TermsQuery(List<string> customerFirstNameList)
        {
            //1.yol
            List<FieldValue> terms = new List<FieldValue>();
            customerFirstNameList.ForEach(x =>
            {
                terms.Add(x);
            });

            //var termsQuery = new TermsQuery() 
            //{ 
            //    Field = "customer_first_name.keyword",
            //    Terms = new TermsQueryField(terms.AsReadOnly())
            //};

            //var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName).Query(termsQuery));
            //2.yol



            // burada datalar 10 ar tane geliyor.size ekleyip değiştirebilir
            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Query(q => q
                                                                    .Terms(t => t
                                                                    .Field(f => f.CustomerFirstName.Suffix("keyword"))
                                                                    .Terms(new TermsQueryField(terms.AsReadOnly())))));


            //burada size ekleyip 10000 tane data getiriyor.
            var resultSize = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                        .Size(10000)
                                                                        .Query(q => q
                                                                        .Terms(t => t
                                                                        .Field(f => f.CustomerFirstName.Suffix("keyword"))
                                                                        .Terms(new TermsQueryField(terms.AsReadOnly())))));
            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();
        }
        public async Task<ImmutableList<ECommerce>> PrefixQuery(string customerFullName)
        {
            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Query(q => q
                                                                    .Prefix(p => p
                                                                    .Field(f => f.CustomerFullName.Suffix("keyword"))
                                                                    .Value(customerFullName))));

            return result.Documents.ToImmutableList();
        }
        public async Task<ImmutableList<ECommerce>> RangeQuery(double fromPrice, double toPrice)
        {
            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Query(q => q
                                                                    .Range(r => r
                                                                    .NumberRange(nr => nr
                                                                    .Field(f => f.TaxfulTotalPrice)
                                                                    .Gte(fromPrice)
                                                                    .Lte(toPrice)))));

            return result.Documents.ToImmutableList();
        }
        public async Task<ImmutableList<ECommerce>> MatchAllQuery()
        {
            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Size(100)  // burada tüm datayı çektiğim için bir sürü veri gelecek o yüzden size yazdım
                                                                    .Query(q => q.MatchAll()));

            return result.Documents.ToImmutableList();
        }
        public async Task<ImmutableList<ECommerce>> PaginationQuery(int page, int pageSize)
        {
            // page 1 pagesize 10 => 1-10
            // page 2 pagesize 10 => 11-20
            // page 3 pagesize 10 => 21-30
            var fromPage = (page - 1) * pageSize;
            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Size(pageSize).From(fromPage)  // burada tüm datayı çektiğim için bir sürü veri gelecek o yüzden size yazdım
                                                                    .Query(q => q.MatchAll()));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> WildCardQuery(string customerFullName)
        {
            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Query(q => q
                                                                    .Wildcard(w => w
                                                                    .Field(f => f.CustomerFullName.Suffix("keyword"))
                                                                    .Wildcard(customerFullName))));

            foreach (var hit in result.Hits)
                hit.Source.Id = hit.Id;

            return result.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> FuzzyQuery(string customerName)
        {

            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Query(q => q
                                                                    .Fuzzy(w => w
                                                                    .Field(f => f.CustomerFirstName.Suffix("keyword")).Value(customerName)
                                                                    .Fuzziness(new Fuzziness(1)))));


            //burada sort da kullandık
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                        .Query(q => q
                                                        .Fuzzy(w => w
                                                        .Field(f => f.CustomerFirstName.Suffix("keyword")).Value(customerName)
                                                        .Fuzziness(new Fuzziness(2))))
                                                        .Sort(sort => sort
                                                        .Field(f => f.TaxfulTotalPrice, new FieldSort() { Order = SortOrder.Desc })));

            foreach (var hit in result2.Hits)
                hit.Source.Id = hit.Id;

            return result2.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> MatchFullTextQuery(string categoryName)
        {
            //term level olursa tam adları yazmak lazım.query yazdığımız için skora göre aradığımızı getirecek.o yüzden sona query yazdık.
            var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                                    .Size(100)
                                                                    .Query(q => q
                                                                    .Match(m => m
                                                                    .Field(f => f.Category)
                                                                    .Query(categoryName))));


            // burada and  operatör koyarak yaptık.iki kelime varsa eğer iki kelime aynı satırda olacak.or olarak yapsaydık iki kelimeden birini içeren farklı satırlar da gelecekti.
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                        .Size(100)
                                                        .Query(q => q
                                                        .Match(m => m
                                                        .Field(f => f.Category)
                                                        .Query(categoryName).Operator(Operator.And))));


            foreach (var hit in result2.Hits)
                hit.Source.Id = hit.Id;

            return result2.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> MatchBooleanPrefixQuery(string customerFullName)
        {
            // burada and  operatör koyarak yaptık.iki kelime varsa eğer iki kelime aynı satırda olacak.or olarak yapsaydık iki kelimeden birini içeren farklı satırlar da gelecekti.
            //operatör kullanmazsak default or olur.
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                        .Size(100)
                                                        .Query(q => q
                                                        .MatchBoolPrefix(m => m
                                                        .Field(f => f.CustomerFullName)
                                                        .Query(customerFullName))));


            foreach (var hit in result2.Hits)
                hit.Source.Id = hit.Id;

            return result2.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> MatchPhraseQuery(string customerFullName)
        {
            //aradığımız ismi öbek halinde getirir.
            // Sultan Al Meyer aradığımızı varsayalım bunu olduğu gibi getirir böyle kayıt var. Ama Sultan Meyer ararsak getirmez. Aradığımız kelimeler arasındaki sıra önemli araya kelime girmemesi lazım.
            // Sultan Al olarak ararsak yine Sultan Al Meyer gelir veya Al Meyer olarak ararsak yine Sultan Al Meyer gelir.
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                        .Size(100)
                                                        .Query(q => q
                                                        .MatchPhrase(m => m
                                                        .Field(f => f.CustomerFullName)
                                                        .Query(customerFullName))));


            foreach (var hit in result2.Hits)
                hit.Source.Id = hit.Id;

            return result2.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> CompoundQueryExample(string cityName, double taxfulTotalPrice, string categoryName, string manufacturer)
        {
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                        .Size(1000)
                                                        .Query(q => q
                                                        .Bool(b => b
                                                        .Must(m => m.Term(t => t.Field("geoip.city_name").Value(cityName)))
                                                        .MustNot(mn => mn.Range(r => r.NumberRange(nr => nr.Field(f => f.TaxfulTotalPrice).Lte(taxfulTotalPrice))))
                                                        .Should(sh => sh.Term(t => t.Field(f => f.Category.Suffix("keyword")).Value(categoryName)))
                                                        .Filter(f => f.Term(t => t.Field("manufacturer.keyword").Value(manufacturer))))));


            foreach (var hit in result2.Hits)
                hit.Source.Id = hit.Id;

            return result2.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> CompoundQueryExample1(string customerFullName)
        {

            // Burada Recip Brock aradık. Recip veya Brock olanlar geldi fakat Rec yazınca birşey gelmedi.Altta diğer yöntemde bunu ele alacağız.
            //var result = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
            //                                            .Size(1000)
            //                                            .Query(q => q
            //                                            .Bool(b => b
            //                                            .Must(m => m
            //                                            .Match(m => m
            //                                            .Field(f => f.CustomerFullName)
            //                                            .Query(customerFullName))))));


            // Burada yukarıdaki Rec yazınca gelmesi ayarlandı.Fakat aşağıda daha iyi yöntem var.
            //var result1 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
            //                                .Size(1000)
            //                                .Query(q => q
            //                                .Bool(b => b
            //                                .Should(m => m
            //                                .Match(m => m
            //                                .Field(f => f.CustomerFullName)
            //                                .Query(customerFullName)).Prefix(p=>p.Field(f=>f.CustomerFullName.Suffix("keyword")).Value(customerFullName))))));


            // burada en etkili yöntem bu fakat diğer queryleri nasıl kullanacağız diye yazdık.
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                            .Size(1000)
                                            .Query(q => q
                                            .MatchPhrasePrefix(m => m
                                            .Field(f => f.CustomerFullName)
                                            .Query(customerFullName))));

            foreach (var hit in result2.Hits)
                hit.Source.Id = hit.Id;

            return result2.Documents.ToImmutableList();

        }
        public async Task<ImmutableList<ECommerce>> MultiMatchFullTextQuery(string name)
        {
            var result2 = await _client.SearchAsync<ECommerce>(i => i.Index(indexName)
                                                        .Size(1000)
                                                        .Query(q => q
                                                        .MultiMatch(mm => mm
                                                        .Fields(new Field("customer_first_name")
                                                        .And(new Field("customer_last_name"))
                                                        .And(new Field("customer_full_name")))
                                                        .Query(name))));


            foreach (var hit in result2.Hits)
                hit.Source.Id = hit.Id;

            return result2.Documents.ToImmutableList();

        }
    }
}
