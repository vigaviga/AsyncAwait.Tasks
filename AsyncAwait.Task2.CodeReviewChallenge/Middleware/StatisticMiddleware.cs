using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwait.Task2.CodeReviewChallenge.Headers;
using CloudServices.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AsyncAwait.Task2.CodeReviewChallenge.Middleware;

public class StatisticMiddleware
{
    private readonly RequestDelegate _next;

    private readonly IStatisticService _statisticService;

    public StatisticMiddleware(RequestDelegate next, IStatisticService statisticService)
    {
        _next = next;
        _statisticService = statisticService ?? throw new ArgumentNullException(nameof(statisticService));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path;

        await _statisticService.RegisterVisitAsync(path);
        
        await UpdateHeaders(context, path);

        await _next(context);
    }

    // When I write the above code like this UI gets returned before the headers are updated. Why? Should not the result be the same?
    //public async Task InvokeAsync(HttpContext context)
    //{
    //    string path = context.Request.Path;
    //    Task staticRegTask = _statisticService.RegisterVisitAsync(path);
    //    Task updateHeaders = staticRegTask.ContinueWith((staticRegTaskResult) => UpdateHeaders(context, path));
    //    await updateHeaders;
    //    await _next(context);
    //}

    private async Task UpdateHeaders(HttpContext context, string path)
    {
        var totalPageVisits = await _statisticService.GetVisitsCountAsync(path);
        context.Response.Headers.Add(CustomHttpHeaders.TotalPageVisits, totalPageVisits.ToString());
    }
}
