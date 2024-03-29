﻿using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter.ExtensionsandConfig.Cookies;
using Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter.Layers.Bussines.Abstract;
using Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter.Layers.Entities.Concrete;
using Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter.Models.VMs.UserVM;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserManager userManager;
        private readonly IMapper mapper;
        private readonly INotyfService notyf;

        public AccountController(IUserManager userManager, IMapper mapper, INotyfService notyf)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.notyf = notyf;
        }
        public IActionResult Index()
        {
            UserLoginVM loginvm = new UserLoginVM();
            return View(loginvm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // asp-action tag helperi ve metodu post olan form tarafindan olusturulmus bir asptokeni cookie ile karsilastiriyor eger. Eger kendi urettigi token ile esit ise erisime izin veriyor.

        public async Task<IActionResult> Login(UserLoginVM loginVM)
        {
            if (ModelState.IsValid) // Tarayicinin JS kullanimi kapatilma ihtimaline karsin koruma. Kapatilirsa validjquery kodlari calismayacagindan oturu.
            {
                try
                {

                    User user = mapper.Map<User>(loginVM);
                    User user1 = await userManager.ChackUser(user);

                    CookieManager cookieManager = new CookieManager();
                    await cookieManager.CookieCreate(HttpContext, user1, loginVM);

                    if (user1.Rol == "Admin") //HttpContext nesnesi, post metodu tamamlanıp yonlenecegi acitionun controlorune ulasıncaya kadar olusmaz, ulasir ulasmaz olusur. HttpContext ile birlikte cookie de olusmus olur.  Bundan dolayi henuz post metodu icerisinde (User.IsInRole("Admin")) && (User.Identity.IsAuthenticated) gibi HttpContext nesnesi kontrollerini yapamıyorum.
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                    else if (user1.Rol == "Üye")
                    {
                        return RedirectToAction("Index", "Shopping");
                    }

                }
                catch (Exception ex)
                {
                    notyf.Error("Hata:" + ex.Message);
                }
            }
            else
            {
                notyf.Error("Hata: Tarayıcınızın JavaScript kullanım iznini kontrol edin, kapalı ise izin verin");
            }

            return RedirectToAction("Index", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");

        }



    }
}
