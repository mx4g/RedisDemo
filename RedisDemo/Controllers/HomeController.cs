using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RedisDemo.Models;
using StackExchange.Redis;

namespace RedisDemo.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 注入连接 因为是注册了单例AddSingleton，所以可以直接取
        /// </summary>
        //private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        //分布式缓存
        private IDistributedCache _distributedCache;

        public HomeController(IConnectionMultiplexer redis, IDistributedCache distributedCache)
        {
            //_redis = redis;
            _db = redis.GetDatabase();
            _distributedCache = distributedCache;
        }

        public IActionResult Index()
        {
            _db.StringSet("fullName", "Michael Jackson");
            var name = _db.StringGet("fullName").ToString();
            return View("index", name);
        }

        public IActionResult Privacy()
        {
            //累加集合，可以做一个用户累计看了多少个商品
            List<string> products = new List<string>();
            products.Add("篮球");
            products.Add("羽毛球");
            products.Add("足球");
            products.Add("兵乓球");
            products.Add("排球");

            foreach (var str in products)
            {
                //保存集合
                _db.ListLeftPush("浏览记录", str);
            }

            //截取前面3个
            _db.ListTrim("浏览记录", 0, 2);

            //取出集合
            var list = _db.ListRange("浏览记录");




            return View(list);
        }


        public IActionResult Distributed()
        {
            //获取分布式缓存的数据
            var value = _distributedCache.Get("我是分布式缓存Key");


            if (value == null)
            {
                var obj = new Dictionary<string, string>
                {
                    ["FirstName"] = "Nick",
                    ["LastName"] = "Carter",
                };

                //保存的数据
                var newStr = JsonConvert.SerializeObject(obj);
                byte[] encoded = Encoding.UTF8.GetBytes(newStr);

                //设置时间
                var t = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30));

                _distributedCache.Set("我是分布式缓存Key", encoded, t);

                var byteStr = _distributedCache.Get("我是分布式缓存Key");
                var str2 = Encoding.UTF8.GetString(byteStr);
                var data2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(str2);
                return View("Distributed", data2);
            }


            var str = Encoding.UTF8.GetString(value);
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);

            return View("Distributed", data);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
