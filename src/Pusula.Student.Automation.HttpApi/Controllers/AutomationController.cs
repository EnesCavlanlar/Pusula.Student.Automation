using Pusula.Student.Automation.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Pusula.Student.Automation.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AutomationController : AbpControllerBase
{
    protected AutomationController()
    {
        LocalizationResource = typeof(AutomationResource);
    }
}
