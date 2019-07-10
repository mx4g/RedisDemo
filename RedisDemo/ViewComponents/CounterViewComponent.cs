﻿using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisDemo.ViewComponents
{
    public class CounterViewComponent : ViewComponent
    {
        private readonly IDatabase _db;

        public CounterViewComponent(IConnectionMultiplexer redis)
        {            
            _db = redis.GetDatabase();
        }

        public async Task<IViewComponentResult> InvokeAsync() {
            var controlller = RouteData.Values["controller"] as string;
            var action = RouteData.Values["action"] as string;

            if (!string.IsNullOrWhiteSpace(controlller) && !string.IsNullOrWhiteSpace(action)) {

                var pageId = $"{controlller}-{action}";

                await _db.StringIncrementAsync(pageId);

                var count = await _db.StringGetAsync(pageId);

                return View("Default", pageId + "：" + count);
            }

            throw new Exception("Cannot Get PageId");
        }
    }
}
