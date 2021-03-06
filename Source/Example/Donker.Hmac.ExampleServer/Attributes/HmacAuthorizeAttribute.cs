﻿using System.Net;
using System.Web;
using System.Web.Mvc;
using Donker.Hmac.Configuration;
using Donker.Hmac.Signing;
using Donker.Hmac.Validation;

namespace Donker.Hmac.ExampleServer.Attributes
{
    public class HmacAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly IHmacConfigurationManager _configurationManager;
        private readonly IHmacKeyRepository _keyRepository;

        public HmacAuthorizeAttribute(IHmacConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _keyRepository = new SingleUserHmacKeyRepository("Neo", "FollowTheWhiteRabbit");
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;

            IHmacConfiguration configuration = _configurationManager.Get("Example");
            IHmacSigner signer = new HmacSigner(configuration, _keyRepository);
            IHmacValidator validator = new HmacValidator(configuration, signer);

            HmacValidationResult result = validator.ValidateHttpRequest(filterContext.HttpContext.Request);

            if (result.ResultCode == HmacValidationResultCode.Ok)
                return;

            HttpResponseBase response = filterContext.HttpContext.Response;
            validator.AddWwwAuthenticateHeader(response, configuration.AuthorizationScheme);
            response.Headers.Add("X-Auth-ErrorCode", result.ResultCode.ToString());
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.Write(result.ErrorMessage);
            response.End();
        }
    }
}