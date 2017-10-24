using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ForgeSample
{
    public partial class _default : System.Web.UI.Page
    {
        //private static BucketsApi bucketsApi = new BucketsApi();
        //private static ObjectsApi objectsApi = new ObjectsApi();
        //private static DerivativesApi derivativesApi = new DerivativesApi();
        //private static TwoLeggedApi oauth2TwoLegged;
        //private static dynamic twoLeggedCredentials;


            protected void Page_Load(object sender, EventArgs e)
        {

        }

    
        protected async void uploadAndTranslate(object sender, EventArgs e)
        {
            // create a randomg bucket name (fixed prefix + randomg guid)
            string bucketKey = "forgeapp" + Guid.NewGuid().ToString("N").ToLower();

            // upload the file (to your server)
            string fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), bucketKey, FileUpload1.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fileSavePath));
            FileUpload1.SaveAs(fileSavePath);

            // get a write enabled token
            TwoLeggedApi oauthApi = new TwoLeggedApi();
            dynamic bearer = await oauthApi.AuthenticateAsync(
                WebConfigurationManager.AppSettings["FORGE_CLIENT_ID"],
                WebConfigurationManager.AppSettings["FORGE_CLIENT_SECRET"],
                "client_credentials",
                new Scope[] { Scope.BucketCreate, Scope.DataCreate, Scope.DataWrite, Scope.DataRead });

            // create the Forge bucket
            PostBucketsPayload postBucket = new PostBucketsPayload(bucketKey, null, PostBucketsPayload.PolicyKeyEnum.Transient /* erase after 24h*/ );
            BucketsApi bucketsApi = new BucketsApi();
            bucketsApi.Configuration.AccessToken = bearer.access_token;
            dynamic newBucket = await bucketsApi.CreateBucketAsync(postBucket);

            // upload file (a.k.a. Objects)
            ObjectsApi objectsApi = new ObjectsApi();
            oauthApi.Configuration.AccessToken = bearer.access_token;
            dynamic newObject;
            using (StreamReader fileStream = new StreamReader(fileSavePath))
            {
                newObject = await objectsApi.UploadObjectAsync(bucketKey, FileUpload1.FileName,
                    (int)fileStream.BaseStream.Length, fileStream.BaseStream,
                    "application/octet-stream");
            }

            // translate file
            string objectIdBase64 = ToBase64(newObject.objectId);
            List<JobPayloadItem> postTranslationOutput = new List<JobPayloadItem>()
    {
        new JobPayloadItem(
        JobPayloadItem.TypeEnum.Svf /* Viewer*/,
        new List<JobPayloadItem.ViewsEnum>()
        {
           JobPayloadItem.ViewsEnum._3d,
           JobPayloadItem.ViewsEnum._2d
        })
    };
            JobPayload postTranslation = new JobPayload(
                new JobPayloadInput(objectIdBase64),
                new JobPayloadOutput(postTranslationOutput));
            DerivativesApi derivativeApi = new DerivativesApi();
            derivativeApi.Configuration.AccessToken = bearer.access_token;
            dynamic translation = await derivativeApi.TranslateAsync(postTranslation);

            // check if is ready
            int progress = 0;
            do
            {
                System.Threading.Thread.Sleep(1000); // wait 1 second
                try
                {
                    dynamic manifest = await derivativeApi.GetManifestAsync(objectIdBase64);
                    progress = (string.IsNullOrWhiteSpace(Regex.Match(manifest.progress, @"\d+").Value) ? 100 : Int32.Parse(Regex.Match(manifest.progress, @"\d+").Value));
                }
                catch (Exception ex)
                {

                }
            } while (progress < 100);

            // ready!!!!

            // register a client-side script to show this model
            Page.ClientScript.RegisterStartupScript(this.GetType(), "ShowModel", string.Format("<script>showModel('{0}');</script>", objectIdBase64));

            // clean up
            Directory.Delete(Path.GetDirectoryName(fileSavePath), true);

        }

        /// <summary>
        /// Convert a string into Base64 (source http://stackoverflow.com/a/11743162)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ToBase64(string input)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}