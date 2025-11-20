using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Zoobee.Domain;
using Zoobee.Domain.DataEntities.Media;

namespace Zoobee.Infrastructure.Repositoties.MediaStorage
{
    public class FileStorageRepository 
	{
		public uint MaxFileSize { get; }
		public FileStorageRepository()
		{
			MaxFileSize = 15000000;//TODO appsettings.json
		}

		public async Task<OperationResult> DeleteFileAsync(string fullPath)
		{
			File.Delete(fullPath);
			return File.Exists(fullPath) ?
				OperationResult.Error("Error.FileStorage.DeleteError", HttpStatusCode.InternalServerError) :
				OperationResult.Success();
		}

		public async Task<Stream> GetFileAsync(string fullPath)
		{
			Stream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
			return fileStream;
		}


		public async Task<OperationResult<MediaFileEntity>> WriteFileAsync(Stream fileStream, string fileName, MediaType mediaType, string localpath, string storagePath)
		{
			var fullpath = storagePath + Path.Combine(localpath, fileName);
			var fileSize = fileStream.Length;
			if (File.Exists(fullpath))
				return OperationResult<MediaFileEntity>.Error("Error.FileStorage.SimilarNameExists", HttpStatusCode.BadRequest);

			FileStream stream = new FileStream(fullpath, FileMode.CreateNew);
			if (stream != null && stream.CanWrite)
			{
				await fileStream.CopyToAsync(stream);
				await stream.FlushAsync();
				stream.Close();
				return OperationResult<MediaFileEntity>.Success(new MediaFileEntity
				{
					Size = fileSize,
					FileLocalURL = Path.Combine(localpath, fileName),
					Type = mediaType,
					FileName = fileName,
				});
			}
			return OperationResult<MediaFileEntity>.Error("Error.FileStorage.OpenStreamFailure", HttpStatusCode.InternalServerError);
		}

		public bool IsFileExists(string path)
			=> File.Exists(path);
	}
}
