using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearch.WEB.ViewModels;
using Humanizer;
using System.Collections;

namespace ElasticSearch.WEB.Models.Repositories
{
	public class ECommerceRepository
	{
		private readonly ElasticsearchClient _client;

		private const string IndexName = "kibana_sample_data_ecommerce";
		public ECommerceRepository(ElasticsearchClient client)
		{
			_client = client;
		}

		public async Task<(List<ECommerce>list,long count)> SearchAsync(ECommerceSearchViewModel searchViewModel, int page, int pageSize)
		{
			List<Action<QueryDescriptor<ECommerce>>> queryList = new();

			if (searchViewModel is null)
			{
				queryList.Add(q => q.MatchAll());
				return await QueryCalculater(page, pageSize, queryList);
			}

			if (!string.IsNullOrEmpty(searchViewModel.Category))
			{
				/* böyle yazmak yerine aşağıda kısa yoldan yazıyoruz delege ile.
				Action<QueryDescriptor<ECommerce>> action = (q) => q.Match(m => m.Field(f => f.Category).Query(searchViewModel.Category));
				*/
				queryList.Add((q) => q.Match(m => m.Field(f => f.Category).Query(searchViewModel.Category)));
			}
			if (!string.IsNullOrEmpty(searchViewModel.CustomerFullName))
			{
				/* böyle yazmak yerine aşağıda kısa yoldan yazıyoruz delege ile.
				Action<QueryDescriptor<ECommerce>> action = (q) => q.Match(m => m.Field(f => f.Category).Query(searchViewModel.Category));
				*/
				queryList.Add((q) => q.Match(m => m.Field(f => f.CustomerFullName).Query(searchViewModel.CustomerFullName)));
			}
			if (searchViewModel.OrderDateStart.HasValue)
			{
				/* böyle yazmak yerine aşağıda kısa yoldan yazıyoruz delege ile.
				Action<QueryDescriptor<ECommerce>> action = (q) => q.Match(m => m.Field(f => f.Category).Query(searchViewModel.Category));
				*/
				queryList.Add((q) => q.Range(r => r.DateRange(dr => dr.Field(f => f.OrderDate).Gte(searchViewModel.OrderDateStart.Value))));
			}
			if (searchViewModel.OrderDateEnd.HasValue)
			{
				/* böyle yazmak yerine aşağıda kısa yoldan yazıyoruz delege ile.
				Action<QueryDescriptor<ECommerce>> action = (q) => q.Match(m => m.Field(f => f.Category).Query(searchViewModel.Category));
				*/
				queryList.Add((q) => q.Range(r => r.DateRange(dr => dr.Field(f => f.OrderDate).Lte(searchViewModel.OrderDateEnd.Value))));
			}
			if (!string.IsNullOrEmpty(searchViewModel.Gender))
			{
				/* böyle yazmak yerine aşağıda kısa yoldan yazıyoruz delege ile.
				Action<QueryDescriptor<ECommerce>> action = (q) => q.Match(m => m.Field(f => f.Category).Query(searchViewModel.Category));
				*/
				queryList.Add((q) => q.Term(t => t.Field(f => f.Gender).Value(searchViewModel.Gender).CaseInsensitive()));
			}

			if (!queryList.Any())
			{
				queryList.Add(q => q.MatchAll());
			}
			return await QueryCalculater(page, pageSize, queryList);
		}

		private async Task<(List<ECommerce> list, long count)> QueryCalculater(int page, int pageSize, List<Action<QueryDescriptor<ECommerce>>> queryList)
		{
			var pageFrom = (page - 1) * pageSize;

			var result = await _client.SearchAsync<ECommerce>(s => s.Index(IndexName)
																	.Size(pageSize).From(pageFrom)
																		.Query(q => q
																			.Bool(b => b
																				.Must(queryList.ToArray()))));

			foreach (var hit in result.Hits)
				hit.Source.Id = hit.Id;

			return (list: result.Documents.ToList(), result.Total);
		}
	}
}
