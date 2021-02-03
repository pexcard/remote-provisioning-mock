using System.Collections.Generic;
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
    [Produces("application/json")]
    public class SettingsController : ControllerBase
    {
        private readonly ResponseSettingsStorage _responseSettingsStorage;

        public SettingsController(ResponseSettingsStorage responseSettingsStorage)
        {
            _responseSettingsStorage = responseSettingsStorage;
        }

        /// <summary>
        /// Get Response settings
        /// </summary>
        /// <param name="businessId">acct id</param>
        /// <param name="cardholderId">acct id</param>
        /// <param name="ct">cancellation token</param>
        /// <returns></returns>
        [HttpGet, Route("business/{businessId:min(1)}/cardholder/{cardholderId:min(1)}/settings")]
        public async Task<ActionResult<ResponseSettingsModel>> GetSettings(int businessId, int cardholderId, CancellationToken ct)
        {
            var entity = await _responseSettingsStorage
                .Get(businessId, cardholderId, ct);

            if (entity == null)
                return NotFound();

            return new ResponseSettingsModel
            {
                SharedSecret = entity.SharedSecret,
                StatusCode = entity.StatusCode,
                TimeoutMls = entity.DelayMls,
                Contacts = entity.ContactsJson.DeserializeTo<List<ContactItem>>()
            };
        }

        /// <summary>
        /// Insert/Update Response settings
        /// </summary>
        /// <param name="businessId">acct id</param>
        /// <param name="cardholderId">acct id</param>
        /// <param name="model">settings</param>
        /// <param name="ct">cancellation token</param>
        /// <returns></returns>
        [HttpPut, Route("business/{businessId:min(1)}/cardholder/{cardholderId:min(1)}/settings")]
        public async Task InsertOrUpdateSettings(int businessId, int cardholderId, ResponseSettingsModel model, CancellationToken ct)
        {
            var entity = new ResponseSettingsEntity
            {
                SharedSecret = model.SharedSecret,
                StatusCode = model.StatusCode,
                DelayMls = model.TimeoutMls,
                ContactsJson = model.Contacts.ToJson()
            };

            await _responseSettingsStorage
                .InsertOrMerge(businessId, cardholderId, entity, ct);
        }
    }
}