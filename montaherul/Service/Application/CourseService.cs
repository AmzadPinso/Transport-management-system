using montaherul.Models;
using montaherul.Service.Interface;
using montaherul.UnitOfWork.Application;
using montaherul.UnitOfWork.Interface;
using Newtonsoft.Json;

namespace montaherul.Service.Application
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _uow;

        public CourseService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<CourseModel>> GetAllAsync()
        {
            return await _uow.Course.GetAllAsync();
        }

        public async Task<CourseModel?> GetByIdAsync(int id)
        {
            return await _uow.Course.GetByIdAsync(id);
        }

        public async Task<CourseModel> CreateAsync(CourseModel course)
        {
            await _uow.Course.AddAsync(course);
            await _uow.SaveChangesAsync();
            return course;
        }

        public async Task<bool> UpdateAsync(int id, CourseModel course)
        {
            var existing = await _uow.Course.GetByIdAsync(id);
            if (existing == null) return false;

            existing.CourseName = course.CourseName;
            existing.TeacherId = course.TeacherId;

            _uow.Course.Update(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _uow.Course.GetByIdAsync(id);
            if (existing == null) return false;

            _uow.Course.Delete(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<List<CourseVM>> GetCourseList(int page = 1, decimal size = 5, string searchquery = "")
        {

            List<FilterItem> filterItems = new List<FilterItem>();
            var searchQueryModel = JsonConvert.DeserializeObject<SearchQueryModel>(searchquery);

            if (searchQueryModel != null && searchQueryModel.filter != null)
            {
                filterItems = searchQueryModel.filter;

            }

            size = searchQueryModel.size;
            page = searchQueryModel.page;
            string filterQuery = "";
            if (filterItems.Count > 0)
            {
                foreach (var item in filterItems)
                {
                    if (!string.IsNullOrEmpty(item.field) && !string.IsNullOrEmpty(item.value))
                    {
                        if (item.field == "teacherName")
                        {
                            item.field = "t.name";
                        }
                        if (!string.IsNullOrEmpty(filterQuery))
                        {
                            filterQuery += " And ";
                        }

                        filterQuery += $"{item.field} LIKE '%{item.value}%'";
                    }
                }
            }

            var companyList = await _uow.Course.GetAllCourseAsync(filterQuery, page, (int)size);

            return companyList;
        }
    }
}