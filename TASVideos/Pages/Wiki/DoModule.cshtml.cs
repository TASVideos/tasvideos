using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
    [AllowAnonymous]
    public class DoModuleModel : BasePageModel
    {
        public DoModuleModel(UserTasks userTasks)
                : base(userTasks)
        {
        }

        [FromQuery]
        public string Name { get; set; }

        [FromQuery]
        public string Params { get; set; }
    }
}
