﻿using gView.Server.AppCode;
using gView.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace gView.Server.Controllers
{
    public class TokenController : BaseController
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (Globals.AllowFormsLogin == false)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new TokenLoginModel());
        }

        [HttpPost]
        public IActionResult Login(TokenLoginModel model)
        {
            try
            {
                if (Globals.AllowFormsLogin == false)
                {
                    return RedirectToAction("Index", "Home");
                }

                var loginManager = new LoginManager(Globals.LoginManagerRootPath);
                var authToken = loginManager.GetAuthToken(model.Username, model.Password);

                if (authToken == null)
                {
                    throw new Exception("Unknown user or password");
                }

                base.SetAuthCookie(authToken);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            base.RemoveAuthCookie();

            return RedirectToAction("Index", "Home");
        }
    }
}