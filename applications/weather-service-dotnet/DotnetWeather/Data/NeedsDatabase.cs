using System.Data.Entity.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;

namespace DotnetWeather.Data;

public class NeedsDatabase : ActionFilterAttribute
{
    private readonly DotnetWeatherContext dbContext;

    private static bool lastTrySuccessful = false;
    
    public NeedsDatabase(DotnetWeatherContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (!dbContext.Database.CanConnect())
        {
            filterContext.Result = new RedirectToActionResult("DatabaseError", "Home", null);
            lastTrySuccessful = false;
            return;
        }

        if (!lastTrySuccessful)
        {
            DbSeed.Seed(dbContext);
        }

        lastTrySuccessful = true;
    }
}