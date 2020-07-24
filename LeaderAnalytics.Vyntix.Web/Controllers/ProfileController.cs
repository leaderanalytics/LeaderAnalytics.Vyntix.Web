﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaderAnalytics.Vyntix.Web.Controllers
{
    public class ProfileController : Controller
    {
        [Authorize]
        public IActionResult List()
        {
            return View();
        }
    }
}
