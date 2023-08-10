using ElasticSearch.WEB.Models.Repositories;
using ElasticSearch.WEB.ViewModels;

namespace ElasticSearch.WEB.Services
{
	public class ECommerceService
	{
		private readonly ECommerceRepository _repository;

		public ECommerceService(ECommerceRepository repository)
		{
			_repository = repository;
		}
		public async Task<(List<ECommerceViewModel> list, long totalCount,long pageLinkCount)> SearchAsync(ECommerceSearchViewModel searchViewModel, int page, int pageSize)
		{
			var (eCommerceList,totalCount) = await _repository.SearchAsync(searchViewModel, page, pageSize);

			var pageLinkCountcalculate = totalCount % pageSize;

			long pageLinkCount = 0;
			
			if (pageLinkCountcalculate > 0)
			{
				pageLinkCount = totalCount / pageSize;
			}
			else
			{
				pageLinkCount = (totalCount / pageSize) + 1;
			}

			var eCommerceListViewModel = eCommerceList.Select(x => new ECommerceViewModel()
			{
				Category = string.Join(",", x.Category),
				CustomerFullName = x.CustomerFullName,
				CustomerFirstName = x.CustomerFirstName,
				CustomerLastName = x.CustomerLastName,
				OrderDate = x.OrderDate.ToShortDateString(),
				Gender = x.Gender.ToLower(),
				Id = x.Id,
				OrderId = x.OrderId,
				TaxfulTotalPrice = x.TaxfulTotalPrice
			}).ToList();

			return (list: eCommerceListViewModel, totalCount, pageLinkCount);
		}
	}
}
