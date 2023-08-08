using ElasticSearch.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ECommerceController : ControllerBase
    {
        private readonly ECommerceRepository _repo;

        public ECommerceController(ECommerceRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> TermQuery(string customerFirstName)
        {
            var response = await _repo.TermQuery(customerFirstName);
            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> TermsQuery(List<string> customerFirstNameList)
        {
            var response = await _repo.TermsQuery(customerFirstNameList);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> PrefixQuery(string customerFullName)
        {
            var response = await _repo.PrefixQuery(customerFullName);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> RangeQuery(double fromPrice, double toPrice)
        {

            // from price 200 verince 199.98 de geliyor.
            var response = await _repo.RangeQuery(fromPrice, toPrice);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> MatchAllQuery()
        {
            var response = await _repo.MatchAllQuery();
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> PaginationQuery(int page = 1, int pageSize = 5)
        {
            var response = await _repo.PaginationQuery(page, pageSize);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> WildCardQuery(string customerFullName)
        {
            var response = await _repo.WildCardQuery(customerFullName);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> FuzzyQuery(string customerName)
        {
            var response = await _repo.FuzzyQuery(customerName);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> MatchFullTextQuery(string categoryName)
        {
            var response = await _repo.MatchFullTextQuery(categoryName);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> MatchBooleanPrefixQuery(string customerFullName)
        {
            var response = await _repo.MatchBooleanPrefixQuery(customerFullName);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> MatchPhraseQuery(string customerFullName)
        {
            var response = await _repo.MatchPhraseQuery(customerFullName);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> CompoundQueryExample(string cityName, double taxfulTotalPrice, string categoryName, string manufacturer)
        {
            var response = await _repo.CompoundQueryExample(cityName,taxfulTotalPrice,categoryName,manufacturer);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> CompoundQueryExample1(string cutomerFullName)
        {
            var response = await _repo.CompoundQueryExample1(cutomerFullName);
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> MultiMatchFullTextQuery(string name)
        {
            var response = await _repo.MultiMatchFullTextQuery(name);
            return Ok(response);
        }
    }
}
