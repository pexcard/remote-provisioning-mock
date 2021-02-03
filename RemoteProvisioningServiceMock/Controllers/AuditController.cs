using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RemoteProvisioningServiceMock.Extensions;
using RemoteProvisioningServiceMock.Models;
using RemoteProvisioningServiceMock.Storage;

namespace RemoteProvisioningServiceMock.Controllers
{
    [ApiController]
    [Route("audit")]
    [Produces("application/json")]
    public class AuditController : ControllerBase
    {
        private readonly TokenProvisioningStorage _tokenProvisioningStorage;
        private readonly OtpCodeStorage _otpCodeStorage;
        private readonly CallbackStorage _callbackStorage;

        public AuditController(
            TokenProvisioningStorage tokenProvisioningStorage,
            OtpCodeStorage otpCodeStorage,
            CallbackStorage callbackStorage)
        {
            _tokenProvisioningStorage = tokenProvisioningStorage;
            _otpCodeStorage = otpCodeStorage;
            _callbackStorage = callbackStorage;
        }

        /// <summary>
        /// Get requests to endpoint: POST /token/provisioning 
        /// </summary>
        /// <param name="cardholderId">acct id</param>
        /// <param name="from">request date-time</param>
        /// <param name="search">query</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>decision response</returns>
        [HttpGet, Route("provisioning")]
        public async Task<ActionResult<IList<DecisionRequestAudit>>> GetTokenProvisioning(
            [Required] int cardholderId, [Required] DateTime from, string search, CancellationToken ct)
        {
            var items = await _tokenProvisioningStorage
                .GetTokenProvisioning(cardholderId, from, search, ct);

            var models = items
                .Select(x => new DecisionRequestAudit
                {
                    SharedSecret = x.SharedSecret,
                    StatusCode = x.StatusCode,
                    DelayMls = x.DelayMls,
                    Request = x.RequestJson.DeserializeTo<DecisionRequest>(),
                    Response = x.ResponseJson.DeserializeTo<DecisionResponse>()
                }).ToList();

            return Ok(models);
        }

        /// <summary>
        /// Get requests to endpoint: POST /token/otp-code 
        /// </summary>
        /// <param name="cardholderId">acct id</param>
        /// <param name="from">request date-time</param>
        /// <param name="search">query</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>otp-code response</returns>
        [HttpGet, Route("otp-code")]
        public async Task<ActionResult<IList<SendOtpRequestAudit>>> GetOtpCode(
            [Required] int cardholderId, [Required] DateTime from, string search, CancellationToken ct)
        {
            var items = await _otpCodeStorage
                .GetOtpCode(cardholderId, from, search, ct);

            var models = items
                .Select(x => new SendOtpRequestAudit
                {
                    SharedSecret = x.SharedSecret,
                    StatusCode = x.StatusCode,
                    DelayMls = x.DelayMls,
                    Request = x.RequestJson.DeserializeTo<SendOtpRequest>()
                }).ToList();

            return Ok(models);
        }

        /// <summary>
        /// Get requests to endpoint: POST /token/callback 
        /// </summary>
        /// <param name="cardholderId">acct id</param>
        /// <param name="from">request date-time</param>
        /// <param name="search">query</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>callback response</returns>
        [HttpGet, Route("callback")]
        public async Task<ActionResult<IList<CallbackRequestAudit>>> GetCallback(
            [Required] int cardholderId, [Required] DateTime from, string search, CancellationToken ct)
        {
            var items = await _callbackStorage
                .GetCallback(cardholderId, from, search, ct);

            var models = items
                .Select(x => new CallbackRequestAudit
                {
                    SharedSecret = x.SharedSecret,
                    Request = x.RequestJson.DeserializeTo<CallbackRequest>()
                }).ToList();

            return Ok(models);
        }
    }
}