using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using static Google.Apis.Drive.v3.DriveService;

namespace DriveUse
{
    public class Drive
    {
        private  string accessToken;
        private string refreshToken;
        private string applicationName;
        private string username;
        private string clientId;
        private string clientSecret;

        public Drive(string accessToken, string refreshToken, string applicationName, string username, string clientId, string clientSecret)
        {
            this.accessToken = accessToken;
            this.refreshToken = refreshToken;
            this.applicationName = applicationName;
            this.username = username;
            this.clientId = clientId;
            this.clientSecret = clientSecret;

        }



        private static DriveService GetService(string accessToken, string refreshToken, string webapplicationName, string emailusername, string clientId, string clientSecret)
        {
            var tokenResponse = new TokenResponse
            {
                //AccessToken = "ya29.a0ARrdaM9M2O7xMDXJFZniLN8P5Yf_zn6me9RlpZvmrbRqoSugAAZrwDdG28aS7IuIN07EAhvPn7dN48oy-x08Z1gl3Nwl3dgeDuHyEWONANB3s7dh8GvQHJP1Yj85yeS4v9ugrIkSBvqBHHxYT6ej9FoHixT9",
                //RefreshToken = "1//047D1zkUcnqvcCgYIARAAGAQSNwF-L9IrTf3Z6TxNmeB5Ke80Pq5kdnWr-noVZEsiLjwtB3Yb2z-Us1KfWsmj_DwajlzM5YqdCo8",
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };


            //var applicationName = "codemart";// Use the name of the project in Google Cloud
            //var username = "sahancodemart@gmail.com";// Use your email
            var applicationName = webapplicationName;
            var username = emailusername;

            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    //ClientId = "207000393786-6dcp1k7h1sb3qa3spkvg7fm2i6j0q7oh.apps.googleusercontent.com",
                    //ClientSecret = "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"
                    ClientId=clientId,
                    ClientSecret=clientSecret
                },
                Scopes = new[] { Scope.Drive },
                DataStore = new FileDataStore(applicationName)
            });


            var credential = new UserCredential(apiCodeFlow, username, tokenResponse);


            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });



            return service;
        }


        public string CreateFolder(string parent, string folderName)
        {
            var service = GetService(this.accessToken,this.refreshToken,this.applicationName,this.username,this.clientId,this.clientSecret);
            var driveFolder = new Google.Apis.Drive.v3.Data.File();
            driveFolder.Name = folderName;
            driveFolder.MimeType = "application/vnd.google-apps.folder";
            driveFolder.Parents = new string[] { parent };
            var command = service.Files.Create(driveFolder);

            var file = command.Execute();
            return file.Id;
        }

        public string UploadFile(Stream file, string fileName, string fileMime, string folder, string fileDescription)
        {
            DriveService service = GetService(this.accessToken, this.refreshToken, this.applicationName, this.username, this.clientId, this.clientSecret);


            var driveFile = new Google.Apis.Drive.v3.Data.File();
            driveFile.Name = fileName;
            driveFile.Description = fileDescription;
            driveFile.MimeType = fileMime;
            driveFile.Parents = new string[] { folder };


            var request = service.Files.Create(driveFile, file, fileMime);
            request.Fields = "id";

            var response = request.Upload();
            if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw response.Exception;

            return request.ResponseBody.Id;
        }


        public IEnumerable<Google.Apis.Drive.v3.Data.File> GetFiles(string folder)
        {
            var service = GetService(this.accessToken, this.refreshToken, this.applicationName, this.username, this.clientId, this.clientSecret);

            var fileList = service.Files.List();
            fileList.Q = $"mimeType!='application/vnd.google-apps.folder'  and  '{folder}' in parents  ";
            fileList.Fields = "nextPageToken, files(id, name, size, mimeType)";

            var result = new List<Google.Apis.Drive.v3.Data.File>();
            string pageToken = null;
            do
            {
                fileList.PageToken = pageToken;
                var filesResult = fileList.Execute();
                var files = filesResult.Files;
                pageToken = filesResult.NextPageToken;
                result.AddRange(files);
            } while (pageToken != null);


            return result;
        }


        public IEnumerable<Google.Apis.Drive.v3.Data.File> GetFolders(string folder)
        {
            var service = GetService(this.accessToken, this.refreshToken, this.applicationName, this.username, this.clientId, this.clientSecret);

            var fileList = service.Files.List();
            fileList.Q = $"mimeType!='application/vnd.google-apps.document' or  mimeType != 'text/x-sql'   and '{folder}' in parents ";
            fileList.Fields = "nextPageToken, files(id, name, size, mimeType)";

            var result = new List<Google.Apis.Drive.v3.Data.File>();
            string pageToken = null;
            do
            {
                fileList.PageToken = pageToken;
                var filesResult = fileList.Execute();
                var files = filesResult.Files;
                pageToken = filesResult.NextPageToken;
                result.AddRange(files);
            } while (pageToken != null);


            return result;
        }


        public void DeleteFile(string fileId)
        {
            var service = GetService(this.accessToken, this.refreshToken, this.applicationName, this.username, this.clientId, this.clientSecret);
            var command = service.Files.Delete(fileId);
            var result = command.Execute();
        }


        public void downLoadFile(string fileId, string filepath)
        {
            
                var service = GetService(this.accessToken, this.refreshToken, this.applicationName, this.username, this.clientId, this.clientSecret);
                Stream outputStream = File.OpenWrite(filepath);
                service.Files.Get(fileId).Download(outputStream);
             
        }


    }



}
