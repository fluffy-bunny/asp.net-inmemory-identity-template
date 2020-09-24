using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.AspNetCore.Mvc
{
    public static class CookieExtensions
    {
        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="value">value to store in cookie object</param>  
        /// <param name="expireTime">expiration time</param>  
        public static void SetJsonCookie<T>(this HttpResponse response,string key, T value, int? expireTime, IDataProtector dataProtector = null)
        {
            CookieOptions option = new CookieOptions()
            {
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Secure = true
            };

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            var json = JsonSerializer.Serialize(value);
            if(dataProtector != null)
            {
                json = dataProtector.Protect(json);
            }
     //       var encryptedValue = Convert.ToBase64String(MachineKey.Protect(cookieText, "ProtectCookie"));

            response.Cookies.Append(key, json, option);
        }
        public static void SetJsonCookie<T>(this ControllerBase controllerBase, 
                                            string key, 
                                            T value, 
                                            int? expireTime, 
                                            IDataProtector dataProtector = null)
        {
            controllerBase.Response.SetJsonCookie<T>(key,value,expireTime, dataProtector);
        }
        public static void SetJsonCookie<T>(this PageModel pageModel,
                                            string key,
                                            T value,
                                            int? expireTime,
                                            IDataProtector dataProtector = null)
        {
            pageModel.Response.SetJsonCookie<T>(key, value, expireTime, dataProtector);
        }
        
        public static T GetJsonCookie<T>(
            this HttpRequest request, 
            string key,
            IDataProtector dataProtector = null) where T : class
        {
            //read cookie from Request object  
            string cookieValueFromReq = request.Cookies[key];
            if (string.IsNullOrWhiteSpace(cookieValueFromReq))
            {
                return null;
            }
            if(dataProtector != null)
            {
                cookieValueFromReq = dataProtector.Unprotect(cookieValueFromReq);
            }
            return JsonSerializer.Deserialize<T>(cookieValueFromReq);

        }
        public static T GetJsonCookie<T>(
            this ControllerBase controllerBase, 
            string key,
            IDataProtector dataProtector = null) where T : class
        {
            //read cookie from Request object  
            return controllerBase.Request.GetJsonCookie<T>(key, dataProtector);
        }
        public static T GetJsonCookie<T>(
            this PageModel pageModel, 
            string key,
            IDataProtector dataProtector = null) where T : class
        {
            //read cookie from Request object  
            return pageModel.Request.GetJsonCookie<T>(key, dataProtector);
        }
    }
}