using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RemoteProvisioningServiceMock.Extensions;
using RemoteProvisioningServiceMock.Models;
using RemoteProvisioningServiceMock.Storage;
using RemoteProvisioningServiceMock.Storage.Entity;

namespace RemoteProvisioningServiceMock.Controllers
{
    [ApiController]
    [Route("token")]
    [Produces("application/json")]
    public class TokenController : ControllerBase
    {
        private readonly ResponseSettingsStorage _responseSettingsStorage;
        private readonly TokenProvisioningStorage _tokenProvisioningStorage;
        private readonly OtpCodeStorage _otpCodeStorage;
        private readonly CallbackStorage _callbackStorage;

        public TokenController(
            ResponseSettingsStorage responseSettingsStorage,
            TokenProvisioningStorage tokenProvisioningStorage,
            OtpCodeStorage otpCodeStorage,
            CallbackStorage callbackStorage)
        {
            _responseSettingsStorage = responseSettingsStorage;
            _tokenProvisioningStorage = tokenProvisioningStorage;
            _otpCodeStorage = otpCodeStorage;
            _callbackStorage = callbackStorage;
        }

        /// <summary>
        /// Get decision for step-up flow 
        /// </summary>
        /// <param name="model">token provisioning</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>decision response</returns>
        [HttpPost, Route("provisioning")]
        public async Task<ActionResult<DecisionResponse>> TokenProvisioning(DecisionRequest model, CancellationToken ct)
        {
            var response = new DecisionResponse
            {
                AcctId = model.AcctId,
                BusinessAcctId = model.BusinessAcctId,
                Last4CardNumber = model.Last4CardNumber
            };

            var settings = await _responseSettingsStorage
                .Get(model.BusinessAcctId, model.AcctId, ct);

            if (settings == null)
            {
                await SaveTokenProvisioning(model, 404, 0, null);

                return NotFound();
            }

            if (!SecretsAreEqual(settings.SharedSecret))
            {
                await SaveTokenProvisioning(model, 403, 0, null);

                return StatusCode(403, "SharedSecret in Authorization header doesn't match the one configured");
            }

            await Task
                .Delay(settings.DelayMls, ct);

            var customResult = GetCustomResult(settings.StatusCode);

            if (customResult != null)
            {
                await SaveTokenProvisioning(model, settings.StatusCode, settings.DelayMls, null);

                return customResult;
            }

            response.Contacts = settings.ContactsJson.DeserializeTo<List<ContactItem>>();

            await SaveTokenProvisioning(model, settings.StatusCode, settings.DelayMls, response);

            return response;
        }

        /// <summary>
        /// Send OTP pass code for step up flow
        /// </summary>
        /// <param name="model">OTP code</param>
        /// <param name="ct">cancellation token</param>
        /// <returns></returns>
        [HttpPost, Route("otp-code")]
        public async Task<ActionResult> SendOtpCode(SendOtpRequest model, CancellationToken ct)
        {
            var settings = await _responseSettingsStorage
                .Get(model.BusinessAcctId, model.AcctId, ct);

            if (settings == null)
            {
                await SaveOtpCode(model, 404, 0);

                return NotFound();
            }

            if (!SecretsAreEqual(settings.SharedSecret))
            {
                await SaveOtpCode(model, 403, 0);

                return StatusCode(403, "SharedSecret 'Authorization' header doesn't match the one configured");
            }

            await SaveOtpCode(model, settings.StatusCode, settings.DelayMls);

            await Task
                .Delay(settings.DelayMls, ct);

            // send otp code here

            var customResult = GetCustomResult(settings.StatusCode);

            return customResult ?? NoContent();
        }

        /// <summary>
        /// Receive callbacks about token creation
        /// </summary>
        /// <param name="model">callback</param>
        /// <param name="ct">cancellation token</param>
        /// <returns></returns>
        [HttpPost, Route("callback")]
        public async Task<ActionResult> Callback(CallbackRequest model, CancellationToken ct)
        {
            var cardholderId = model?.Data?.AcctId ?? 0;

            var entity = new CallbackEntity
            {
                SharedSecret = GetSharedSecretFromHeaders(),
                RequestJson = model.ToJson()
            };

            await _callbackStorage
                .Insert(cardholderId, entity, ct);

            return NoContent();
        }

        #region private members

        private ActionResult GetCustomResult(int code)
        {
            switch (code)
            {
                case 400:
                    return BadRequest("Service is configured to return 'BadRequest'");
                case 401:
                    return Unauthorized("Service is configured to return 'Unauthorized'");
                case 403:
                    return StatusCode(403, "Service is configured to return 'Forbidden'");
                case 404:
                    return NotFound("Service is configured to return 'NotFound'");
                case 500:
                    throw new Exception("Service is configured to throw 'Internal Service Error'");
                default:
                    return null;
            }
        }

        private string GetSharedSecretFromHeaders()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var stringValues) || !stringValues.Any()) 
                return "";

            try
            {
                var sharedSecret = stringValues.ToString()
                    .Substring("Basic ".Length)
                    .Trim();

                return Encoding.UTF8.GetString(Convert.FromBase64String(sharedSecret));
            }
            catch
            {
                return "";
            }
        }

        private bool SecretsAreEqual(string expectedSharedSecret)
        {
            var headersSecret = GetSharedSecretFromHeaders();

            // if not configured - treat as okay
            if (string.IsNullOrEmpty(expectedSharedSecret) && string.IsNullOrEmpty(headersSecret))
                return true;

            return string.Equals(expectedSharedSecret, headersSecret, StringComparison.OrdinalIgnoreCase);
        }

        private async Task SaveTokenProvisioning(DecisionRequest model, int statusCode, int delay, DecisionResponse response)
        {
            var entity = new TokenProvisioningEntity
            {
                SharedSecret = GetSharedSecretFromHeaders(),
                StatusCode = statusCode,
                DelayMls = delay,
                RequestJson = model.ToJson(),
                ResponseJson = response.ToJson()
            };

            await _tokenProvisioningStorage
                .Insert(model.AcctId, entity, CancellationToken.None);
        }

        private async Task SaveOtpCode(SendOtpRequest model, int statusCode, int delay)
        {
            var entity = new OtpCodeEntity
            {
                SharedSecret = GetSharedSecretFromHeaders(),
                StatusCode = statusCode,
                DelayMls = delay,
                RequestJson = model.ToJson()
            };

            await _otpCodeStorage
                .Insert(model.AcctId, entity, CancellationToken.None);
        }

        #endregion
    }
}