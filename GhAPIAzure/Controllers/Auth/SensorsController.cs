﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using DbStructure.User;
using GhAPIAzure.Models;
using System.Security.Claims;
using EntityFramework.Extensions;
using EFExtensions;
using DbStructure;
using Swashbuckle.Swagger.Annotations;
using System.Web.Http.ModelBinding;

namespace GhAPIAzure.Controllers
{

    [Authorize]
    public class SensorsController : BaseController
    {
        private DataContext db = new DataContext();

        // GET: api/Sensors
        /// <summary>
        /// Gets All sensors that belong to this user
        /// </summary>
        /// <remarks> Gets all the sensors that belong to the user that's currently logged in. 
        /// Ownership is determined through connection to a device, that in installed in a 
        /// location that belongs to the user. </remarks>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Sensor>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public async Task<HttpResponseMessage> GetSensors()
        {
            List<Sensor> devices = await db.Sensors.Where(sen => sen.Device.Location.PersonId == _UserID).ToListAsync();
            return Request.CreateResponse(HttpStatusCode.OK, devices); 
        }

        // POST: api/Sensors
        /// <summary>
        /// Accepts a list of Sensors you want to create or edit
        /// </summary>
        /// <remarks> This accepts a list of Sensors. It will create one if it does not exist, or edit one if it already exists.</remarks>
        /// <param name="sensRecivedList">The list of Sensors</param>
        /// <returns>Ok if all good, otherwise you will get an ErrorResponce</returns>

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Sensor>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ModelStateDictionary))]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Happens when you try to edit someone else's stuff",Type = typeof(ErrorResponse<Sensor>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Happens when you refer to items that do not exist", Type = typeof(ErrorResponse<Sensor>))]
        public async Task<HttpResponseMessage> PostRelays(List<Sensor> sensRecivedList)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            var duplicate = sensRecivedList.FirstOrDefault(p => sensRecivedList.Any(q => (p != q && p.ID == q.ID))); 
            if (null != duplicate)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Sensor>(
                        "You submitted a list with duplicates", duplicate)); 
            }
            
            //Check for illegal locations
            List<Guid> DeviceIDs = sensRecivedList.Select(sens => sens.DeviceID).ToList();
            List<Device> devicesDb = await db.Devices.Where(dev => DeviceIDs.Contains(dev.ID))
                .Include(dev => dev.Location).ToListAsync();

            foreach (Sensor relRecieved in sensRecivedList)
            {
                Device device = devicesDb.FirstOrDefault(dev => dev.ID == relRecieved.DeviceID);
                if (device == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, new ErrorResponse<Sensor>(
                        "the device you referenced does not exist. Create it first", relRecieved));
                else if (device.Location.PersonId != _UserID)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Sensor>(
                        "You can't attach a Sensor to someone else's device", relRecieved));
                }
            }
             
            List<Guid> sensorsIDs = sensRecivedList.Select(sens => sens.ID).ToList();
            List<Sensor> sensorsDbList = await db.Sensors.Where(sens => sensorsIDs.Contains(sens.ID)).ToListAsync(); 
            for(int i =0; i < sensRecivedList.Count; i++)
            {
                var MatchDB = sensorsDbList.FirstOrDefault(sens => sens.ID == sensRecivedList[i].ID); 

                if(MatchDB != null)
                { 
                    if(MatchDB.DeviceID != sensRecivedList[i].DeviceID)
                        return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Sensor>(
                        "You can't move the sensor to a different device", sensRecivedList[i]));
                    else if (MatchDB.SensorTypeID!= sensRecivedList[i].SensorTypeID)
                        return Request.CreateResponse(HttpStatusCode.Forbidden, new ErrorResponse<Sensor>(
                        "You can't change the sensorType", sensRecivedList[i]));
                    else
                        db.Entry(MatchDB).CurrentValues.SetValues(sensRecivedList[i]); 
                }
                else
                {
                    db.Entry(sensRecivedList[i]).State = EntityState.Added; 
                }         
            }

            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK, sensRecivedList);
        }
    }
}