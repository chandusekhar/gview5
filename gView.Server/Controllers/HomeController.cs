﻿using gView.Server.AppCode;
using gView.Server.Models;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace gView.Server.Controllers
{
    public class HomeController : BaseController
    {
        private readonly MapServiceManager _mapServiceMananger;
        private readonly LoginManager _loginManager;

        public HomeController(
            MapServiceManager mapServiceMananger, 
            LoginManager loginManager,
            EncryptionCertificateService encryptionCertificateService)
            : base(mapServiceMananger, loginManager, encryptionCertificateService)
        {
            _mapServiceMananger = mapServiceMananger;
            _loginManager = loginManager;
        }

        public IActionResult Index()
        {
            if (_mapServiceMananger.Options.IsValid == false)
            {
                return RedirectToAction("ConfigInvalid");
            }

            var user = Globals.ExternalAuthService != null ?
                Globals.ExternalAuthService.Perform(this.Request) :
                null;

            if (!String.IsNullOrWhiteSpace(user))
            {
                var authToken = _loginManager.CreateUserAuthTokenWithoutPasswordCheck(user);
                if (authToken != null)
                {
                    base.SetAuthCookie(authToken);
                }

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ConfigInvalid()
        {
            return View();
        }
    }
}
