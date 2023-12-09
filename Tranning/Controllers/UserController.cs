using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tranning.DataDBContext;
using Tranning.Models;


namespace Tranning.Controllers
{
    public class UserController : Controller
    {
        private readonly TranningDBContext _dbContext;

        public UserController(TranningDBContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public IActionResult Index(string SearchString)
        {
            UserModel userModel = new UserModel();
            userModel.UserDetailLists = new List<UserDetail>();

            var data = from m in _dbContext.Users select m;

            data = data.Where(m => m.deleted_at == null);
            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.username.Contains(SearchString) || m.full_name.Contains(SearchString));
            }

            // Assign the result of ToList back to the data variable
            data.ToList();

            foreach (var item in data)
            {
                userModel.UserDetailLists.Add(new UserDetail
                {
                    id = item.id,
                    role_id = item.role_id,
                    extra_code = item.extra_code,
                    username = item.username,
                    email = item.email,
                    phone = item.phone,
                    gender = item.gender,
                    full_name = item.full_name,
                    avatar = item.avatar,
                    status = item.status,
                    created_at = item.created_at,
                    updated_at = item.updated_at
                });
            }

            ViewData["CurrentFilter"] = SearchString;
            return View(userModel);
        }


        [HttpGet]
        public IActionResult Add()
        {
            UserDetail user = new UserDetail();
            var roleList = _dbContext.Roles
                .Where(m => m.deleted_at == null)
                .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = roleList;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(UserDetail user, IFormFile Photo)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadFile(Photo);
                    var userData = new User()
                    {
                        username = user.username,
                        extra_code = user.extra_code,
                        role_id = user.role_id,
                        password = user.password,
                        email = user.email,
                        phone = user.phone,
                        full_name = user.full_name,
                        gender = user.gender,
                        status = user.status,
                        avatar = uniqueFileName,
                        created_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    };

                    _dbContext.Users.Add(userData);
                    _dbContext.SaveChanges(true);
                    TempData["saveStatus"] = true;
                }
                catch (Exception ex)
                {
                    TempData["saveStatus"] = false;
                }
                    return RedirectToAction(nameof(UserController.Index), "User");
                
            }

            var userList = _dbContext.Users
              .Where(m => m.deleted_at == null)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.username }).ToList();
            ViewBag.Stores = userList;
            Console.WriteLine(ModelState.IsValid);
            return View(user);
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
            UserDetail user = new UserDetail();
            var data = _dbContext.Users.Where(m => m.id == id).FirstOrDefault();
            if (data != null)
            {
                user.id = data.id;
                user.username = data.username;
                user.role_id = data.role_id;
                user.password = data.password;
                user.email = data.email;
                user.phone = data.phone;
                user.full_name = data.full_name;
                user.extra_code = data.extra_code;
                user.avatar = data.avatar;
                user.status = data.status;
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult Update(UserDetail user, IFormFile file)
        {
            try
            {

                var data = _dbContext.Users.Where(m => m.id == user.id).FirstOrDefault();
                string uniqueIconAvatar = "";
                if (user.Photo != null)
                {
                    uniqueIconAvatar = uniqueIconAvatar = UploadFile(user.Photo);
                }

                if (data != null)
                {
                    // gan lai du lieu trong db bang du lieu tu form model gui len
                    data.username = user.username;
                    data.extra_code = user.extra_code;
                    data.role_id = user.role_id;
                    data.password = user.password;
                    data.email = user.email;
                    data.phone = user.phone;
                    data.status = user.status;
                    data.full_name = user.full_name;

                    if (!string.IsNullOrEmpty(uniqueIconAvatar))
                    {
                        data.avatar = uniqueIconAvatar;
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

                return RedirectToAction(nameof(UserController.Index), "User");
            

        }
        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            try
            {
                var data = _dbContext.Users.Where(m => m.id == id).FirstOrDefault();
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
            return RedirectToAction(nameof(UserController.Index), "User");
        }
        [HttpGet]
        public IActionResult Trainer(string SearchString)
        {
            UserModel userModel = new UserModel();
            userModel.UserDetailLists = new List<UserDetail>();

            var data = from m in _dbContext.Users select m;

            data = data.Where(m => m.deleted_at == null);
            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.username.Contains(SearchString) || m.full_name.Contains(SearchString));
            }

            // Assign the result of ToList back to the data variable
            data.ToList();

            foreach (var item in data)
            {
                userModel.UserDetailLists.Add(new UserDetail
                {
                    id = item.id,
                    role_id = item.role_id,
                    extra_code = item.extra_code,
                    username = item.username,
                    email = item.email,
                    phone = item.phone,
                    gender = item.gender,
                    full_name = item.full_name,
                    avatar = item.avatar,
                    status = item.status,
                    created_at = item.created_at,
                    updated_at = item.updated_at
                });
            }

            ViewData["CurrentFilter"] = SearchString;
            return View(userModel);
        }
        [HttpGet]
        public IActionResult TrainerIndex(string SearchString)
        {
            UserModel userModel = new UserModel();
            userModel.UserDetailLists = new List<UserDetail>();

            var data = from m in _dbContext.Users select m;

            data = data.Where(m => m.deleted_at == null);
            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.username.Contains(SearchString) || m.full_name.Contains(SearchString));
            }

            // Assign the result of ToList back to the data variable
            data.ToList();

            foreach (var item in data)
            {
                userModel.UserDetailLists.Add(new UserDetail
                {
                    id = item.id,
                    role_id = item.role_id,
                    extra_code = item.extra_code,
                    username = item.username,
                    email = item.email,
                    phone = item.phone,
                    gender = item.gender,
                    full_name = item.full_name,
                    avatar = item.avatar,
                    status = item.status,
                    created_at = item.created_at,
                    updated_at = item.updated_at
                });
            }

            ViewData["CurrentFilter"] = SearchString;
            return View(userModel);
        }
        [HttpGet]
        public IActionResult TrainerAdd()
        {
            UserDetail user = new UserDetail();
            var roleList = _dbContext.Roles
                .Where(m => m.deleted_at == null)
                .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = roleList;
            return View(user);
        }

    }
}
