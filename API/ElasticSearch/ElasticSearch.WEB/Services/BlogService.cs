using ElasticSearch.WEB.Models;
using ElasticSearch.WEB.Models.Repositories;
using ElasticSearch.WEB.ViewModels;
using System.Security.AccessControl;

namespace ElasticSearch.WEB.Services
{
    public class BlogService
    {
        private readonly BlogRepository _blogRepository;

        public BlogService(BlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }

        public async Task<bool> SaveAsync(BlogCreateViewModel model)
        {
            Blog newBlog = new Blog
            {
                UserId = Guid.NewGuid(), //normalde cookieden filan kullanıcı id alıyoruz.
                Title = model.Title,
                Content = model.Content,
                Tags = model.Tags.Split(',')
            };

            var isCreatedBlog = await _blogRepository.SaveAsync(newBlog);

            return isCreatedBlog != null;

        }
        public async Task<List<BlogViewModel>> SearchAsync(string searchText)
        {
            var blogList = await _blogRepository.SearchAsync(searchText);
            var model = blogList.Select(b=> new BlogViewModel()
            {
                Id = b.Id,
                Title = b.Title,
                Content= b.Content,
                Created = b.Created.ToShortDateString(),
                Tags = string.Join(",", b.Tags),
                UserId = b.UserId.ToString()
            }).ToList();
            return model;
        }
    }
}
