using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Net;
using Zoobee.Domain.DataEntities.Media;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zoobee.Domain;
using Zoobee.Infrastructure.Repositoties.MediaStorage;
/// <summary>
/// TODO Описание
/// </summary>
public class AcceptableMediaTypesConfig : Dictionary<string, MediaType> { }
public class MediaFilesSizesBytesConfig : Dictionary<MediaType, uint> { }
public class MediaStorageService 
//TODO Ограничение на размер файлов
{
	public string _StorageFolderPath { get; }
	public FileStorageRepository _fileStorageRepository { get; set; }
	public MediaFileRepository _mediaFileRepository { get; set; }
	public Dictionary<string, MediaType> _acceptableMediaTypes { get; set; }
	public Dictionary<MediaType, uint> _maxMediaSizeBytes { get; set; }
	public MediaStorageService(IHostEnvironment environment,
		IConfiguration configuration,
		FileStorageRepository fileStorage,
		MediaFileRepository mediaFiles,
		IOptions<AcceptableMediaTypesConfig> amtConfig,
		IOptions<MediaFilesSizesBytesConfig> mfsbConfig)
	{
		_fileStorageRepository = fileStorage;
		_mediaFileRepository = mediaFiles;
		_StorageFolderPath = environment.ContentRootPath + configuration["FileStorage:RootSubFolder"] + "/";
		_acceptableMediaTypes = amtConfig.Value;
		_maxMediaSizeBytes = mfsbConfig.Value;
	}
	public async Task<OperationResult<MediaFileEntity>> SaveFileEasy()
	{
		var mediaFile = new MediaFileEntity
		{
			Id = (ulong)new Random().Next(0, 99999999),
			FileLocalURL = _StorageFolderPath,
			FileName = Guid.NewGuid().ToString(),
			OwnerUserId = null,
			Size = 5000000,
			Type = MediaType.Image
		};
		return OperationResult<MediaFileEntity>.Success(mediaFile);
	}
	public async Task<OperationResult<MediaFileEntity>> SaveUploadedFileAsync(Stream file, string filename)
	{
		if (true)
		{
			return await SaveFileEasy();
		}
		string fileExtension = Path.GetExtension(filename);
		string newFileName = Guid.NewGuid().ToString() + fileExtension;
		using var memoryStream = new MemoryStream();
		await file.CopyToAsync(memoryStream);
		memoryStream.Position = 0;

		MediaType mediaType = MediaType.Image;
		var writeFileResult = await _fileStorageRepository.WriteFileAsync(memoryStream, newFileName, mediaType, mediaType.ToString(), "");
		if (writeFileResult.Succeeded)
		{
			var mediaFileRes = await _mediaFileRepository.AddMediaFile(writeFileResult.Returns);
			await _mediaFileRepository.SaveChangesAsync();
			if (mediaFileRes.Succeeded)
			{
				var Entity = await _mediaFileRepository.GetMediaFileAsync(mediaFileRes.Returns);
				return Entity != null ?
						OperationResult<MediaFileEntity>.Success(Entity) :
						OperationResult<MediaFileEntity>.Error("Error.MediaStorage.WriteDbError", HttpStatusCode.InternalServerError);
			}
			return OperationResult<MediaFileEntity>.Error(mediaFileRes.Message, mediaFileRes.ErrCode);
		}
		return writeFileResult;
	}

	public bool IsOkFileSize(long filesize, MediaType mediaType)
	{
		return _maxMediaSizeBytes[mediaType] <= filesize;
	}
	public MediaType GetMediaType(string extension)
	{
		var acceptFound = _acceptableMediaTypes.TryGetValue(extension, out var mediaType);
		return acceptFound ? mediaType : MediaType.NONE;
	}
	public async Task<OperationResult> DeleteMediaAsync(string filename)
	{
		var mediaFile = await _mediaFileRepository.GetMediaFileAsync(filename);
		if (mediaFile == null)
			return OperationResult.Error("Error.MediaStorage.MediaFileNotFound", HttpStatusCode.NotFound);
		if (_fileStorageRepository.IsFileExists(filename))
		{
			var res = await _fileStorageRepository.DeleteFileAsync(Path.Combine(_StorageFolderPath, mediaFile.FileLocalURL));
			await _mediaFileRepository.SaveChangesAsync();
			return res;
		}
		return OperationResult.Error("Error.MediaStorage.FileNotFound", HttpStatusCode.InternalServerError);
	}

	public bool IsMediaExists(string MediaUri)
		=> File.Exists(_StorageFolderPath + MediaUri);
}