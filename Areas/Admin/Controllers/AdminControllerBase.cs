using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QA.Services;

namespace QA.Areas.Admin.Controllers
{
    // Mọi controller trong khu vực quản trị đều yêu cầu quyền Admin
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public abstract class AdminControllerBase : Controller
    {
        protected void SetMessage(ServiceResult result)
        {
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }

        protected void Success(string message) => TempData["Success"] = message;

        protected void Error(string message) => TempData["Error"] = message;
    }
}
