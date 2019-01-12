using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
    [AllowAnonymous]
    public class DoIfModuleModel : BasePageModel
    {
        public DoIfModuleModel(UserTasks userTasks)
                : base(userTasks)
        {
        }

        [FromQuery]
        public string Condition { get; set; }
    }
}
