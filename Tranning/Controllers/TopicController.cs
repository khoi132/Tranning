using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tranning.DataDBContext;
using Tranning.Models;
using Tranning.DBContext;

namespace Tranning.Controllers
{
    public class TopicController : Controller
    {
        private readonly TranningDBContext _dbContext;

        public TopicController(TranningDBContext context)
        {
            _dbContext = context;
        }
        [HttpGet]
        public IActionResult Index(string searchString)
        {
            TopicModel topicModel = new TopicModel();
            topicModel.TopicDetailLists = new List<TopicDetail>();
                
            var data = from topic in _dbContext.Topics
                       join course in _dbContext.Courses on topic.course_id equals course.id
                       select new { Course = course, Topic = topic };

            // Apply additional conditions
            data = data.Where(m => m.Topic.deleted_at == null);

            if (!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(m => m.Topic.name.Contains(searchString) || m.Topic.description.Contains(searchString));
            }

            // Execute the query and materialize the results
            var dataList = data.ToList();

            foreach (var item in dataList)
            {
                topicModel.TopicDetailLists.Add(new TopicDetail
                {
                    id = item.Topic.id,
                    course_id = item.Topic.course_id,
                    namecourse = item.Course.name, // Replace CourseName with the actual property name
                    name = item.Topic.name,
                    description = item.Topic.description,
                    attack_file = item.Topic.attack_file,
                    status = item.Topic.status,
                    created_at = item.Topic.created_at,
                    updated_at = item.Topic.updated_at
                });
            }

            ViewData["CurrentFilter"] = searchString;
            return View(topicModel);
        }


        [HttpGet]
        public IActionResult Add()
        {
            TopicDetail topic = new TopicDetail();
            var topicList = _dbContext.Courses
                .Where(m => m.deleted_at == null)
                .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = topicList;
            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TopicDetail topic, IFormFile Photo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadFile(Photo);
                    var topicData = new Topic()
                    {
                        name = topic.name,
                        description = topic.description,
                        course_id = topic.course_id,
                        status = topic.status,
                        attack_file = uniqueFileName,
                        created_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    };

                    _dbContext.Topics.Add(topicData);
                    _dbContext.SaveChanges(true);
                    TempData["saveStatus"] = true;
                }
                catch (Exception ex)
                {
                    TempData["saveStatus"] = false;
                }
                return RedirectToAction(nameof(TopicController.Index), "Topic");
            }

            var topicList = _dbContext.Topics
              .Where(m => m.deleted_at == null)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = topicList;
            Console.WriteLine(ModelState.IsValid);
            return View(topic);
        }

        private string UploadFile(IFormFile file)
        {
            string uniqueFileName;
            try
            {
                string pathUploadServer = "wwwroot\\uploads\\images";

                string fileName = file.FileName;
                fileName = Path.GetFileName(fileName);
                string uniqueStr = Guid.NewGuid().ToString(); // random tao ra cac ky tu khong trung lap
                // tao ra ten fil ko trung nhau
                fileName = uniqueStr + "-" + fileName;
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), pathUploadServer, fileName);
                var stream = new FileStream(uploadPath, FileMode.Create);
                file.CopyToAsync(stream);
                // lay lai ten anh de luu database sau nay
                uniqueFileName = fileName;
            }
            catch (Exception ex)
            {
                uniqueFileName = ex.Message.ToString();
            }
            return uniqueFileName;
        }
        [HttpGet]
        public IActionResult Update(int id = 0)
        {
            TopicDetail topic = new TopicDetail();
            var data = _dbContext.Topics.Where(m => m.id == id).FirstOrDefault();
            if (data != null)
            {
                topic.id = data.id;
                topic.name = data.name;
                topic.attack_file = data.attack_file;
                topic.description = data.description;
                topic.status = data.status;


            }

            return View(topic);
        }

        [HttpPost]
        public IActionResult Update(TopicDetail topic, IFormFile file)
        {
            try
            {

                var data = _dbContext.Topics.Where(m => m.id == topic.id).FirstOrDefault();
                string uniqueIconAvatar = "";
                if (topic.Photo != null)
                {
                    uniqueIconAvatar = uniqueIconAvatar = UploadFile(topic.Photo);
                }

                if (data != null)
                {
                    // gan lai du lieu trong db bang du lieu tu form model gui len
                    data.name = topic.name;
                    data.description = topic.description;
                    data.status = topic.status;

                    if (!string.IsNullOrEmpty(uniqueIconAvatar))
                    {
                        data.attack_file = uniqueIconAvatar;
                    }
                    _dbContext.SaveChanges(true);
                    TempData["UpdateStatus"] = true;
                }
                else
                {
                    TempData["UpdateStatus"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["UpdateStatus"] = false;
            }
            return RedirectToAction(nameof(TopicController.Index), "Topic");
        }
        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            try
            {
                var data = _dbContext.Topics.Where(m => m.id == id).FirstOrDefault();
                if (data != null)
                {
                    data.deleted_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    _dbContext.SaveChanges(true);
                    TempData["DeleteStatus"] = true;
                }
                else
                {
                    TempData["DeleteStatus"] = false;
                }
            }
            catch
            {
                TempData["DeleteStatus"] = false;
            }
            return RedirectToAction(nameof(TopicController.Index), "Topic");
        }
    }
}
