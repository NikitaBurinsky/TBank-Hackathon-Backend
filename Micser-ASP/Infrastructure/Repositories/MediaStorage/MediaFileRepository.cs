using tbank_back_web.Infrastructure.DbContext;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Zoobee.Domain;
using Zoobee.Domain.DataEntities.Media;
using Zoobee.Infrastructure.Repositoties;

namespace Zoobee.Infrastructure.Repositoties.MediaStorage
{
    public class MediaFileRepository 
	{
		public MediaFileRepository(ApplicationDbContext odbContext) 
		{
		dbContext = odbContext;
		}
		ApplicationDbContext dbContext;		

		public IQueryable<MediaFileEntity> MediaFiles => dbContext.MediaFiles;

		public async Task<OperationResult<ulong>> AddMediaFile(MediaFileEntity Entity)
		{
			var res = dbContext.MediaFiles.Add(Entity);
			if (res == null)
				return OperationResult<ulong>.Error("Error.MediaFiles.WriteDbError", HttpStatusCode.InternalServerError);
			return OperationResult<ulong>.Success(res.Entity.Id);
		}

		public async Task<OperationResult> DeleteMediaFile(MediaFileEntity Entity)
		{
			if (dbContext.MediaFiles.Any(e => e.FileLocalURL == Entity.FileLocalURL))
			{
				dbContext.MediaFiles.Remove(Entity);
				return OperationResult.Success();
			}
			else
				return OperationResult.Error("Error.MediaFile.MediaFileNotFound", HttpStatusCode.NotFound);
		}

		public async Task<MediaFileEntity> GetMediaFileAsync(ulong Id)
			=> dbContext.MediaFiles.FirstOrDefault(MediaFiles => MediaFiles.Id == Id);
		public async Task<MediaFileEntity> GetMediaFileAsync(string filename)
			=> dbContext.MediaFiles.FirstOrDefault(e => e.FileName == filename);

		public async Task<OperationResult> UpdateMediaFile(MediaFileEntity Entity, Action<MediaFileEntity> action)
		{
			var res = dbContext.Update(Entity);
			if (res == null)
				return OperationResult.Error("Error.MediaFile.UpdateError", HttpStatusCode.InternalServerError);
			action(res.Entity);
			return OperationResult.Success();
		}

		public async Task<int> SaveChangesAsync()
		{
			return await dbContext.SaveChangesAsync();
		}
	}
}
