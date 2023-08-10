using ElasticSearch.WEB.Services;
using ElasticSearch.WEB.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.WEB.Controllers
{
	public class ECommerceController : Controller
	{
		private readonly ECommerceService _service;

		public ECommerceController(ECommerceService service)
		{
			_service = service;
		}

		public async Task<IActionResult> Search([FromQuery] SearchPageViewModel searchPageViewModel)
		{
			var (eCommerceList,totalCount,pageLinkCount) = await _service.SearchAsync(searchPageViewModel.SearchViewModel, searchPageViewModel.Page, searchPageViewModel.PageSize);

			searchPageViewModel.List = eCommerceList;
			searchPageViewModel.TotalCount = totalCount;
			searchPageViewModel.PageLinkCount = pageLinkCount;

			return View(searchPageViewModel);
		}
	}
}
